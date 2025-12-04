using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;

public class LotterySystem : MonoBehaviour
{
    public GameObject[] defenderPrefabs; // 存储所有可能的 defender prefabs
    public Transform defenderContainer; // 父物体，用于容纳生成的 defender
    public Transform cardContainer; // 用于放置抽到的卡片图片

    public Transform editTeamContainer; // Edit Team Canvas 中的容器
    public Transform[] dropTargetPanel; // Edit Team Panel

    public GameObject cardPopup; // 显示抽取结果
    public GameObject fortuneWheel; // 大转盘
    public GameObject CongratsPopup; // 获得所有defender之后的弹窗

    private List<GameObject> defenderInstance = new List<GameObject>(); // 存储生成的 defender 实例，可以动态排列

    public Button clearButton; // 清空按钮
    public Button closeButton; // 关闭抽中英雄后的弹窗
    public Button clearPlayerPrefsButton; // 清空 PlayerPrefs 的按钮
    public GameObject prizeDefener; // 关闭抽中英雄后的背景动画

    public InventoryManager inventoryManager; // 用于获取金币数量并减少金币
    public TurntableController turntableController; // 用于获取倒计时

    // cardpopup起始动画
    private Vector2 offscreenPosition = new Vector2(-1500, 0); // 左侧起始位置
    private Vector2 onscreenPosition = new Vector2(0, 0); // 目标位置（屏幕中央）
    public float slideDuration = 0.3f; // 滑动动画时长

    private void Start()
    {
        InitializeDefaultDefendersOnce();
        LoadDefenderOrderFromFile(); // 从文件加载数据
        closeButton.onClick.AddListener(CloseCardPopup);
        clearPlayerPrefsButton.onClick.AddListener(ClearPlayerPrefs);
        cardPopup.SetActive(false); //激活抽取结果界面

        // **检测是否有新 Defender 需要解锁** 用于通关奖励
        CheckForNewDefender();
    }

    private void InitializeDefaultDefendersOnce()
    {
        if (PlayerPrefs.HasKey("DefaultDefendersInitialized")) return;

        string[] defaultDefenders = { "Bonkster", "BowBoi" };
        List<string> defenderNamesToSave = new List<string>();

        foreach (string defenderName in defaultDefenders)
        {
            GameObject prefab = GetDefenderPrefabByName(defenderName);
            if (prefab != null)
            {
                defenderNamesToSave.Add(defenderName);
                DefenderDataManager.Instance.SaveDefenderLevel(defenderName, 1);
                Debug.Log($"[首次进入] 默认 Defender '{defenderName}' 已添加，等级设为1");
            }
            else
            {
                Debug.LogWarning($"[首次进入] 未找到默认 Defender prefab: {defenderName}");
            }
        }

        // 写入 defendersList.json 文件
        string json = JsonUtility.ToJson(new SerializableList<string>(defenderNamesToSave));
        File.WriteAllText(GetSaveFilePath(), json);
        Debug.Log("[首次进入] 默认 defender 数据已写入 defendersList.json");

        DefenderDataManager.Instance.SaveAllData();

        PlayerPrefs.SetInt("DefaultDefendersInitialized", 1);
        PlayerPrefs.Save();
        AssignDefaultDefendersToPanels();
    }

    private void AssignDefaultDefendersToPanels()
    {
        // 面板名称与默认 defender 名字一一对应
        string[] defaultDefenders = { "Bonkster", "BowBoi" };
        string[] targetPanels = { "Panel1", "Panel2" };

        for (int i = 0; i < defaultDefenders.Length; i++)
        {
            string defenderName = defaultDefenders[i];
            string panelName = targetPanels[i];

            // 保存到 PlayerPrefs
            PlayerPrefs.SetString(panelName, defenderName);
            Debug.Log($"[初始化队伍] 设置 {defenderName} 到 {panelName}");
        }

        PlayerPrefs.Save();
    }



    // **检测是否有新 Defender 需要解锁**
    private void CheckForNewDefender()
    {
        List<string> pendingDefenders = UnlockDefenderManager.Instance.GetPendingDefenders(); // 获得带解锁Defender

        if (pendingDefenders.Count > 0) // 确保有待解锁的 Defender
        {
            //Debug.Log($"检测到 {pendingDefenders.Count} 个待解锁的 Defender");

            foreach (string defenderName in pendingDefenders)
            {
                //Debug.Log($"正在解锁: {defenderName}");

                GameObject newDefenderPrefab = GetDefenderPrefabByName(defenderName);

                if (newDefenderPrefab != null)
                {
                    AddDefenderToContainers(newDefenderPrefab);
                    Debug.Log($"new Defender: {defenderName} has been added to Containers");

                    DefenderDataManager.Instance.SaveDefenderLevel(defenderName, 1);
                    Debug.Log($"[DATABASE] new Defender '{defenderName}' 已存入 JSON，等级: 1");
                    DefenderDataManager.Instance.SaveAllData();

                    // **移除待解锁 Defender 记录，防止重复解锁**
                    UnlockDefenderManager.Instance.RemovePendingDefender(defenderName);
                }

                else
                {
                    Debug.LogWarning($"未找到 Defender 预制体: {defenderName}");
                }
            }


        }
    }

    // **添加 解锁的Defender 到 UI 容器**
    private void AddDefenderToContainers(GameObject newDefenderPrefab)
    {
        // **添加到 defenderContainer**
        GameObject newDefender = Instantiate(newDefenderPrefab, defenderContainer);
        newDefender.SetActive(true);
        defenderInstance.Add(newDefender);

        // **添加到 Edit Team**
        GameObject editTeamDefender = Instantiate(newDefenderPrefab, editTeamContainer);
        editTeamDefender.SetActive(true);

        SaveDefenderOrderToFile();
    }

    private void Update()
    {
        // 每帧更新倒计时显示
        turntableController.UpdateCooldownText();
    }

    // 抽奖并添加新的 defender
    public void DrawDefender()
    {
        cardPopup.SetActive(true);
        StartCoroutine(SlideInCardPopup()); // 启动滑动协程

        // 筛选出尚未拥有的 defender （跳过前 4 个默认 defender）
        List<GameObject> availableDefenders = new List<GameObject>();
        for (int i = 1; i < defenderPrefabs.Length; i++) // 从索引 4 开始遍历
        {
            GameObject defenderPrefab = defenderPrefabs[i];
            bool alreadyOwned = defenderInstance.Exists(defender =>
                defender.name == defenderPrefab.name || defender.name == defenderPrefab.name + "(Clone)");

            if (!alreadyOwned)
            {
                availableDefenders.Add(defenderPrefab); // 只添加未拥有的 defender
            }
        }

        // 检查是否已收集所有 defender
        if (availableDefenders.Count == 0)
        {
            Debug.Log("Congratulations! You have collected all defenders!");
            CongratsPopup.SetActive(true);
            inventoryManager.AddItem(1000, "Coin");
            return;
        }

        // 从未拥有的 defender 中随机抽取
        int randomIndex = Random.Range(0, availableDefenders.Count); // 跳过前4个defender
        GameObject selectedPrefab = availableDefenders[randomIndex];

        // 检查是否是重复 defender
        bool isDuplicate = defenderInstance.Exists(defender => defender.name == selectedPrefab.name || defender.name == selectedPrefab.name + "(Clone)");

        // 添加到主容器
        GameObject newDefender = Instantiate(selectedPrefab, defenderContainer);
        GameObject newCard = Instantiate(selectedPrefab, cardContainer);

        DisableRaycast(newCard);
        DisableRaycast(newDefender);

        if (!HasDefenderWithName(editTeamContainer, selectedPrefab.name) && !HasDefenderInPanels(dropTargetPanel, selectedPrefab.name))
        {
            GameObject editTeamDefender = Instantiate(selectedPrefab, editTeamContainer);
            EnableRaycast(editTeamDefender);

            Button upgradeButton = editTeamDefender.GetComponentInChildren<Button>();
            if (upgradeButton != null)
            {
                upgradeButton.gameObject.SetActive(false);
            }

            foreach (Transform child in newCard.transform)
            {
                if (child.name != "DefenderPic" && child.name != "Square")
                {
                    child.gameObject.SetActive(false);
                }
            }

            newDefender.SetActive(true);
            newCard.SetActive(true);
            defenderInstance.Add(newDefender);
            ArrangeDefenders();
            ArrangeEditTeamDefenders();
        }
        else
        {
            Debug.Log($"Defender {selectedPrefab.name} already exists in editTeamContainer or dropTargetPanel. Skipping.");
        }

        // **存入 JSON 数据库**
        Defender newDefenderComponent = newDefender.GetComponent<Defender>();
        if (newDefenderComponent != null)
        {
            int defaultLevel = 1; // 设定默认等级
            DefenderDataManager.Instance.SaveDefenderLevel(newDefenderComponent.defenderName, defaultLevel);
            Debug.Log($"[DATABASE] new Defender '{newDefenderComponent.defenderName}' 已存入 JSON，等级: {defaultLevel}");
        }

        // **确保数据存入 JSON**
        DefenderDataManager.Instance.SaveAllData();

        SaveDefenderOrder();
    }

    // 动态排列 defender 位置
    private void ArrangeDefenders()
    {
        float spacing = 1800f; // 控制每个 defender 之间的间距

        for (int i = 0; i < defenderInstance.Count; i++)
        {
            GameObject defender = defenderInstance[i];
            RectTransform rectTransform = defender.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(i * spacing, 0);
        }
    }

    // 排列 Edit Team 容器中的 defender
    private void ArrangeEditTeamDefenders()
    {
        float spacing = 200f; // 控制每个 defender 之间的间距
        int defenderIndex = 0;

        foreach (Transform child in editTeamContainer)
        {
            if (child.GetComponent<Defender>() != null)
            {
                RectTransform rectTransform = child.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    float originalX = rectTransform.anchoredPosition.x;
                    float originalY = rectTransform.anchoredPosition.y;
                    rectTransform.anchoredPosition = new Vector2(originalX, defenderIndex * -spacing + originalY);
                }
                defenderIndex++;
            }
        }
    }

    public void ClearDefenderList()
    {
        foreach (GameObject defender in defenderInstance)
        {
            if (defender != null)
            {
                Destroy(defender);
            }
        }

        defenderInstance.Clear();
    }

    public void CloseCardPopup()
    {
        cardPopup.SetActive(false);
        prizeDefener.SetActive(false);


        foreach (Transform child in cardContainer)
        {
            Destroy(child.gameObject);
        }
        Debug.Log("Card container has been cleared.");
    }


    // 滑动协程， 从左侧划入到目标位置
    private IEnumerator SlideInCardPopup()
    {
        RectTransform rectTransform = cardPopup.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = onscreenPosition; // 设置初始位置

        float elapsedTime = 0;
        while (elapsedTime < slideDuration)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(onscreenPosition, onscreenPosition, elapsedTime / slideDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = onscreenPosition;
    }

    public void SaveDefenderOrder()
    {
        HashSet<string> defenderNames = new HashSet<string>();

        foreach (GameObject defender in defenderInstance)
        {
            string originalName = defender.name.Replace("(Clone)", "");
            defenderNames.Add(originalName);
        }

        string json = JsonUtility.ToJson(new SerializableList<string>(new List<string>(defenderNames)));
        PlayerPrefs.SetString("DefenderNames", json);
        SaveDefenderOrderToFile(); // 使用文件存储数据
    }

    private GameObject GetDefenderPrefabByName(string name)
    {
        foreach (GameObject prefab in defenderPrefabs)
        {
            if (prefab.name == name)
            {
                return prefab;
            }
        }

        // **如果 defenderPrefabs[] 里没有，尝试从 Resources 目录加载**
        GameObject loadedPrefab = Resources.Load<GameObject>($"DefenderUpgrade/{name}");
        if (loadedPrefab != null)
        {
            //Debug.Log($"从 Resources 目录加载 Defender 预制体: {name}");
            return loadedPrefab;
        }

        Debug.LogError($"无法找到 Defender 预制体: {name}（请确保它在 Resources/DefenderUpgrade 文件夹中）");
        return null;


    }

    public void LoadDefenderOrder()
    {
        ClearDefenderList();

        LoadDefenderOrderFromFile(); // 使用文件加载数据

        string json = PlayerPrefs.GetString("DefenderNames", "{}");
        SerializableList<string> defenderNames = JsonUtility.FromJson<SerializableList<string>>(json);

        foreach (string defenderName in defenderNames.list)
        {
            GameObject defenderPrefab = GetDefenderPrefabByName(defenderName);
            if (defenderPrefab != null)
            {
                GameObject newDefender = Instantiate(defenderPrefab, defenderContainer);
                newDefender.SetActive(true);
                defenderInstance.Add(newDefender);

                GameObject editTeamDefender = Instantiate(defenderPrefab, editTeamContainer);
                editTeamDefender.SetActive(true);

                //DisableUnnecessaryComponents(editTeamDefender);
            }
            else
            {
                Debug.LogWarning($"Defender prefab not found for: {defenderName}");
            }
        }

        ArrangeDefenders();
        ArrangeEditTeamDefenders();
    }

    private Transform FindInChildren(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;

            Transform found = FindInChildren(child, childName);
            if (found != null)
                return found;
        }
        return null;
    }

    private bool HasDefenderWithName(Transform container, string defenderName)
    {
        foreach (Transform child in container)
        {
            if (child.name == defenderName || child.name == defenderName + "(Clone)")
            {
                return true;
            }
        }
        return false;
    }

    private bool HasDefenderInPanels(Transform[] panels, string defenderName)
    {
        foreach (Transform panel in panels)
        {
            foreach (Transform child in panel)
            {
                if (child.name == defenderName || child.name == defenderName + "(Clone)")
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void DisableRaycast(GameObject defender)
    {
        CanvasGroup canvasGroup = defender.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = defender.AddComponent<CanvasGroup>();
        }
        canvasGroup.blocksRaycasts = false;
    }

    private void EnableRaycast(GameObject defender)
    {
        CanvasGroup canvasGroup = defender.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }
    }

    [System.Serializable]
    public class SerializableList<T>
    {
        public List<T> list;
        public SerializableList(List<T> list)
        {
            this.list = list;
        }
    }

    // 清空 PlayerPrefs 的方法
    public void ClearPlayerPrefs()
    {
        Debug.Log("Clearing PlayerPrefs...");
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("PlayerPrefs cleared successfully.");

        // 清空本地缓存的 savedLevels 数据
        DefenderDataManager.Instance.ClearSavedLevels();

        // 删除本地文件
        string filePath = GetSaveFilePath();
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log($"Defender data file deleted: {filePath}");
        }

        // 清空现有的 defender，并重新加载默认 defender
        ClearDefenderList();
        // 重置所有 Defender 等级
        ResetDefendersLevel();



        // 重置所有 Defender 等级
        foreach (var defender in defenderInstance)
        {
            Defender defenderComponent = defender.GetComponent<Defender>();
            if (defenderComponent != null)
            {
                defenderComponent.level = 1;
                defenderComponent.RecalculateState();
            }
        }

        // 重新加载场景，确保数据刷新
        //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }



    // 将所有Defender等级设为1
    private void ResetDefendersLevel()
    {
        // 从 Resources/Defenders 文件夹加载所有 Defender，包括 Tower 和 Meat
        Defender[] defenders = Resources.LoadAll<Defender>("Defenders");

        foreach (var defender in defenders)
        {
            defender.level = 1;  // 重置等级
            defender.RecalculateState();
            //Debug.Log($"{defender.defenderName} level reset to 1.");
        }
    }

    // defender列表储存到本地
    private string GetSaveFilePath()
    {
        string path = Path.Combine(Application.persistentDataPath, "defendersList.json");
        //Debug.Log($"Save file path: {path}");
        return path;
    }

    private void SaveDefenderOrderToFile()
    {
        List<string> defenderNames = new List<string>();

        foreach (GameObject defender in defenderInstance)
        {
            string originalName = defender.name.Replace("(Clone)", "");
            defenderNames.Add(originalName);
        }

        string json = JsonUtility.ToJson(new SerializableList<string>(defenderNames));
        File.WriteAllText(GetSaveFilePath(), json);
        Debug.Log($"Defender data saved to {GetSaveFilePath()}");
    }

    private void LoadDefenderOrderFromFile()
    {
        string filePath = GetSaveFilePath();

        // 清空现有 defender 和 edit team 实例
        ClearDefenderList();

        foreach (Transform child in editTeamContainer)
        {
            Destroy(child.gameObject);
        }

        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"Defender data file not found: {filePath}. 跳过加载。");
            return;
        }

        string json = File.ReadAllText(filePath);
        SerializableList<string> defenderNames = JsonUtility.FromJson<SerializableList<string>>(json);

        foreach (string defenderName in defenderNames.list)
        {
            GameObject defenderPrefab = GetDefenderPrefabByName(defenderName);
            if (defenderPrefab != null)
            {
                GameObject newDefender = Instantiate(defenderPrefab, defenderContainer);
                newDefender.SetActive(true);
                defenderInstance.Add(newDefender);

                GameObject editTeamDefender = Instantiate(defenderPrefab, editTeamContainer);
                editTeamDefender.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"Defender prefab not found for: {defenderName}");
            }
        }

        ArrangeDefenders();
        ArrangeEditTeamDefenders();
    }


    private void InitializeDefaultDefenders()
    {
        foreach (GameObject prefab in defenderPrefabs)
        {
            GameObject newDefender = Instantiate(prefab, defenderContainer);
            newDefender.SetActive(true);
            defenderInstance.Add(newDefender);

            GameObject editTeamDefender = Instantiate(prefab, editTeamContainer);
            editTeamDefender.SetActive(true);

            //DisableUnnecessaryComponents(editTeamDefender);
        }

        ArrangeDefenders();
        ArrangeEditTeamDefenders();
    }


}
