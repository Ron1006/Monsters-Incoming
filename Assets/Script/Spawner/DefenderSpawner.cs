using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DefenderSpawner : MonoBehaviour
{
    public GameObject[] defenders; // 自动从 Resources 加载的 Defender 预制体
    public Button[] defenderButtons; // 存储按钮，用于获取Image和Text组件
    public Transform spawnPoint;

    private float[] cooldownTimers; //每个按钮的冷却时间
    private Image[] cooldownMasks; // 每个按钮的冷却这招

    private bool autoSpawnEnabled = false; // 用于自动生成defender
    public Button autoSpawnButton;
    public Sprite autoOnSprite;
    public Sprite autoOffSprite;
    private float autoSpawnTimer = 0f;
    private float autoSpawnInterval = 2f; // 每隔0.5秒尝试自动部署

    public MeatPanel meatPanel;

    private Dictionary<string, Defender> defenderData = new Dictionary<string, Defender>();

    void Start()
    {
        meatPanel = FindObjectOfType<MeatPanel>();

        // **自动加载 Resources/Defenders 目录下的所有 Defender 预制体**
        defenders = Resources.LoadAll<GameObject>("Defenders");

        // 初始化防御者数据
        foreach (var defender in defenders)
        {
            Defender defenderComponent = defender.GetComponent<Defender>();

            if (defenderComponent != null)
            {
                int savedLevel = DefenderDataManager.Instance.GetDefenderLevel(defenderComponent.defenderName);
                defenderComponent.level = savedLevel;
                defenderComponent.RecalculateState();

                defenderData[defenderComponent.defenderName] = defenderComponent;

                //Debug.Log($"[DEBUG] 加载 {defenderComponent.defenderName}，等级: {savedLevel}，攻击力: {defenderComponent.attackPower}，血量: {defenderComponent.health}");
            }
        }

        // 从 PlayerPrefs 中加载并排序 defenders
        defenders = LoadAndSortDefenders();

        // 初始化冷却时间数组, cooldownTimers数组的长度等于defenders的长度
        cooldownTimers = new float[defenders.Length];
        cooldownMasks = new Image[defenders.Length];

        // 初始化按钮状态
        SetupDefenderButtons();
        // 初始化冷却遮罩
        InitializeCooldownMasks();
        // 初始化按钮颜色和文本
        UpdateButtonStatus();
    }

    // Update is called once per frame
    void Update()
    {
        // 每帧更新冷却时间并刷新按钮状态
        UpdateCooldownTimers();

        // 每帧都更新按钮颜色和文本，以确保肉的数量变化时按钮会更新
        UpdateButtonStatus();

        // 自动生成defender
        if (autoSpawnEnabled)
        {
            for (int i = 0; i < defenders.Length; i++)
            {
                if (defenders[i] == null || cooldownTimers[i] > 0) continue;

                DefenderAttribute attr = defenders[i].GetComponent<DefenderAttribute>();
                if (attr == null) continue;

                if (meatPanel.meatQuantityGame >= attr.cost)
                {
                    autoSpawnTimer += Time.deltaTime;

                    if (autoSpawnTimer >= autoSpawnInterval)
                    {
                        autoSpawnTimer = 0f;
                        SpawnDefender(i);
                        break;
                    }

                }
            }
        }
    }

    // 加载并排序defenders
    GameObject[] LoadAndSortDefenders()
    {
        GameObject[] sortedDefenders = new GameObject[7]; // 用于存储排序后的防御者
        List<GameObject> remainingDefenders = new List<GameObject>(defenders); // 存储所有未排序的防御者

        for (int i = 1; i <= 7; i++) // 遍历 Panel1 到 Panel7
        {
            string panelKey = "Panel" + i;
            string defenderName = PlayerPrefs.GetString(panelKey, null);

            if (!string.IsNullOrEmpty(defenderName))
            {
                // 在所有防御者中查找与 defenderName 匹配的防御者（忽略大小写）
                GameObject matchedDefender = remainingDefenders.Find(defender =>
               defender.GetComponent<Defender>().defenderName == defenderName);


                if (matchedDefender != null)
                {
                    sortedDefenders[i - 1] = matchedDefender; // 按顺序填充
                    remainingDefenders.Remove(matchedDefender); // 从剩余列表中移除
                }
                else
                {
                    Debug.LogWarning($"No defender found with name {defenderName} for {panelKey}");
                }
            }

            else if (i <= 2)
            {
                sortedDefenders[i - 1] = remainingDefenders[i - 1];
            }

            else
            {
                //Debug.Log($"No defender assigned to {panelKey}");
                sortedDefenders[i - 1] = null; // 确保空面板留空
            }
        }
        return sortedDefenders;

    }


    // 设置按钮图像和文字，自动从Defender的Prefab获取
    void SetupDefenderButtons()
    {
        for (int i = 0; i < defenders.Length; i++)
        {
            if (defenders[i] == null || defenderButtons[i] == null)
                continue;

            Defender defenderComponent = defenders[i].GetComponent<Defender>();
            DefenderAttribute defenderAttribute = defenders[i].GetComponent<DefenderAttribute>();

            if (defenderAttribute != null && defenderAttribute.defenderLevelImages.Length > 0)
            {
                int levelIndex = Mathf.Clamp((defenderComponent.level - 1) / 5, 0, defenderAttribute.defenderLevelImages.Length - 1);
                defenderButtons[i].GetComponent<Image>().sprite = defenderAttribute.defenderLevelImages[levelIndex];
                defenderButtons[i].GetComponentInChildren<TMP_Text>().text = defenderAttribute.cost.ToString();
            }
        }
    }

    void InitializeCooldownMasks()
    {
        for (int i = 0; i < defenderButtons.Length; i++)
        {
            if (defenderButtons[i] == null || defenders[i] == null)
            {
                Debug.LogWarning($"Button or defender at index {i} is null. Skipping cooldown mask initialization.");
                continue;
            }

            Transform maskTransform = defenderButtons[i].transform.Find("CooldownMask");
            if (maskTransform != null)
            {
                cooldownMasks[i] = maskTransform.GetComponent<Image>();
                if (cooldownMasks[i] != null)
                {
                    // 设置初始冷却时间
                    float initialCooldown = defenders[i].GetComponent<Defender>().spawnCoolDown / 2;

                    // 设置冷却计时器，表示现在正在冷却
                    cooldownTimers[i] = initialCooldown;

                    // 设置遮罩为满值，表示正在冷却
                    cooldownMasks[i].fillAmount = 0.5f;
                }
                else
                {
                    Debug.LogWarning($"CooldownMask Image component missing on button: {defenderButtons[i].name}");
                }
            }
            else
            {
                Debug.LogWarning($"CooldownMask not found on button: {defenderButtons[i].name}");
            }
        }
    }

    // 生成指定的Defender，传入Defender的索引
    public void SpawnDefender(int index)
    {
        // 检查冷却时间是否已完成
        if (cooldownTimers[index] > 0)
        {
            Debug.Log("Defender is on cooldown");
            return;
        }

        // 获取Defender的属性
        DefenderAttribute defenderAttribute = defenders[index].GetComponent<DefenderAttribute>();

        if (meatPanel.meatQuantityGame >= defenderAttribute.cost)
        {
            // 消耗meat
            meatPanel.meatQuantityGame -= defenderAttribute.cost;

            // 随机生成 y 轴坐标在 0.5 到 -0.1 之间
            float randomY = Random.Range(-0.9f, -0.3f);

            // 生成时保持 x 轴 z=y，更新 y 轴
            Vector3 spawnPosition = new Vector3(spawnPoint.position.x, randomY, randomY);

            // 实例化防御者
            GameObject defenderInstance = Instantiate(defenders[index], spawnPosition, Quaternion.identity);

            // 从 PlayerPrefs 中加载数据到生成的实例
            Defender defenderComponent = defenderInstance.GetComponent<Defender>();
            if (defenderComponent != null)
            {
                if (defenderComponent != null && defenderData.ContainsKey(defenderComponent.defenderName))
                {
                    var savedData = defenderData[defenderComponent.defenderName];
                    defenderComponent.level = savedData.level;
                    defenderComponent.attackPower = savedData.attackPower;
                    defenderComponent.health = savedData.health;
                }
                else
                {
                    defenderComponent.RecalculateState();
                }


                cooldownTimers[index] = defenderComponent.spawnCoolDown;
                if (cooldownMasks[index] != null)
                {
                    cooldownMasks[index].fillAmount = 1;
                }
            }
        }
        else
        {
            Debug.Log("Not enough meat to spawn defender!");
        }
    }


    // 更新所有按钮的状态
    void UpdateButtonStatus()
    {
        for (int i = 0; i < defenders.Length; i++)
        {
            if (defenders[i] == null || defenderButtons[i] == null)
            {
                //Debug.LogWarning($"Defender or button at index {i} is null. Skipping update.");
                continue;
            }

            // 获取Defender的属性
            DefenderAttribute defenderAttribute = defenders[i].GetComponent<DefenderAttribute>();

            // 获取按钮的Image组件
            Image buttonImage = defenderButtons[i].GetComponent<Image>();

            // 检查当前肉的数量是否足够，以及按钮是否处于冷却状态
            bool canAfford = meatPanel.meatQuantityGame >= defenderAttribute.cost;
            bool isOnCooldown = cooldownTimers[i] > 0;

            // 根据条件设置按钮的颜色和交互性
            if (canAfford && !isOnCooldown)
            {
                buttonImage.color = Color.white; // 可用状态，按钮为白色
                defenderButtons[i].interactable = true; // 启用按钮交互
            }
            else
            {
                buttonImage.color = Color.gray; // 不可用状态，按钮为灰色
                defenderButtons[i].interactable = false; // 禁用按钮交互
            }
            // 更新冷却遮罩填充量
            if (cooldownMasks[i] != null)
            {
                float cooldownDuration = defenders[i].GetComponent<Defender>().spawnCoolDown;
                if (cooldownDuration > 0)
                {
                    cooldownMasks[i].fillAmount = cooldownTimers[i] / cooldownDuration;
                }
                else
                {
                    cooldownMasks[i].fillAmount = 0;
                }
            }
        }
    }

    // 更新冷却计时器
    void UpdateCooldownTimers()
    {
        for (int i = 0; i < cooldownTimers.Length; i++)
        {
            if (cooldownTimers[i] > 0)
            {
                cooldownTimers[i] -= Time.deltaTime;
                //减少冷却计时器后，确保其不小于零
                if (cooldownTimers[i] < 0)
                {
                    cooldownTimers[i] = 0;
                }
            }
        }
    }

    // Helper method to determine the level-based image index
    int GetDefenderLevelIndex(GameObject defender)
    {
        Defender defenderComponent = defender.GetComponent<Defender>();
        if (defenderComponent != null)
        {
            return (defenderComponent.level - 1) / 4;
        }
        return 0; // Default to index 0 if no level is found
    }

    public void UpdateDefenderData(Defender defender)
    {
        if (defenderData.ContainsKey(defender.defenderName))
        {
            defenderData[defender.defenderName] = defender;
        }
        else
        {
            defenderData.Add(defender.defenderName, defender);
        }
    }

    public void ToggleAutoSpawn()
    {
        autoSpawnEnabled = !autoSpawnEnabled;


        // 切换按钮图像
        if (autoSpawnButton != null)
        {
            Image btnImage = autoSpawnButton.GetComponent<Image>();
            if (btnImage != null)
            {
                btnImage.sprite = autoSpawnEnabled ? autoOnSprite : autoOffSprite;
            }
        }

        Debug.Log("自动部署状态：" + (autoSpawnEnabled ? "开启" : "关闭"));
    }
}
