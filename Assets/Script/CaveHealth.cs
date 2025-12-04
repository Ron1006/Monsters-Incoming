using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CaveHealth : MonoBehaviour
{
    public int health;
    public int maxHealth = 50;
    public int reward = 50;
    public int rewardGem = 10;

    public Sprite[] caveSprites;
    public SpriteRenderer caveSr;
    public Canvas gameOverCanvas;
    public Canvas newDefenderCanvas; // 过关解锁新defender canvas
    private Canvas spawnCanvas;
    private Collider2D col; // 引用 Collider2D

    public Slider slider;
    public float easeSpeed = 0.01f; // 控制EaseFill延迟跟随的速度
    public GameObject damageTextPrefab; // 显示扣血数字
    //public Transform damageTextPosition; // 伤害数字位置
    private Image fillImage;     // 实时扣血的血条
    private Image easeFillImage; // 延迟扣血的血条

    private List<GameObject> enemySpawners = new List<GameObject>();
    //private EnemySpawner enemySpawner; // 引用 EnemySpawner 脚本
    //private bool isFifthWaveSpawned = false; // 增加一个标志位，记录是否已经生成过第五波怪物

    public Animator shockWaveAnimator; // 引用 ShockWave 上的 Animator
    public SpriteRenderer shockWaveRenderer;

    public string unlockDefenderName; // 用于解锁对应的过关奖励

    private bool isGameOver = false;

    public InventoryManager inventoryManager;


    // Start is called before the first frame update
    void Start()
    {

        health = maxHealth;
        //automatically scale slider itself
        slider.maxValue = maxHealth;
        slider.value = maxHealth;
        gameOverCanvas.gameObject.SetActive(false);

        // 获取 Collider2D 组件
        col = GetComponent<Collider2D>();



        // 获取 EnemySpawner 引用
        // enemySpawner = FindObjectOfType<EnemySpawner>();

        // 初始时让 ShockWave 不可见
        shockWaveRenderer.enabled = false;

        // 查找名为 "SpawnCanvas" 的对象并获取其 Canvas 组件
        GameObject spawnCanvasObject = GameObject.Find("SpawnCanvas");

        if (spawnCanvasObject != null)
        {
            spawnCanvas = spawnCanvasObject.GetComponent<Canvas>();
        }
        else
        {
            Debug.LogWarning("SpawnCanvas not found in the scene!");
        }

        // 缓存所有名字包含 "EnemySpawner" 的对象
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("EnemySpawner"))
            {
                enemySpawners.Add(obj);
            }
        }

        // 获取 Fill 和 EaseFill 组件
        fillImage = slider.transform.Find("Fill Area/Fill").GetComponent<Image>();
        easeFillImage = slider.transform.Find("Fill Area/EaseFill").GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {


    }

    // 解锁过关奖励，新defender
    public void TriggerVictory()
    {
        Debug.Log($"关卡胜利！解锁新 Defender: {unlockDefenderName}");

        // **检查是否已经解锁**
        bool alreadyUnlocked = UnlockDefenderManager.Instance.IsDefenderUnlocked(unlockDefenderName);

        if (!alreadyUnlocked)
        {
            // **设置待解锁 Defender**
            UnlockDefenderManager.Instance.AddPendingDefender(unlockDefenderName);
            Debug.Log($"记录 {unlockDefenderName} 为待解锁 Defender，等待主界面处理解锁！");

            // **正式解锁 Defender**
            UnlockDefenderManager.Instance.UnlockDefender(unlockDefenderName);

            // **显示 New Defender Canvas**
            if (newDefenderCanvas != null)
            {
                newDefenderCanvas.gameObject.SetActive(true);
                // **查找名为 `unlockDefenderName` 的子对象，并激活**
                Transform newDefender = newDefenderCanvas.transform.Find(unlockDefenderName);
                if (newDefender != null)
                {
                    newDefender.gameObject.SetActive(true);
                    Debug.Log($"成功激活 {unlockDefenderName} 显示在 New Defender Canvas！");
                }
                else
                {
                    Debug.LogWarning($"未找到 {unlockDefenderName}，请检查 New Defender Canvas 内的对象命名！");
                }
            }
        }

        else
        {
            Debug.Log($"{unlockDefenderName} 已经解锁过，不重复解锁！");
        }

        // **存储当前难度已完成**
        int currentDifficulty = PlayerPrefs.GetInt("SelectedDifficultyLevel", 1);
        string currentLevel = SceneManager.GetActiveScene().name; // 获取当前关卡名
        string difficultyKey = $"{currentLevel}_Difficulty_{currentDifficulty}_Completed";

        PlayerPrefs.SetInt(difficultyKey, 1); // 储存这个难度已完成
        PlayerPrefs.Save();

        Debug.Log($"已完成难度 {currentDifficulty}，存储键：{difficultyKey}");

        // **解锁下一个难度**
        string nextDifficultyKey = $"{currentLevel}_Difficulty_{currentDifficulty + 1}_Unlocked";
        PlayerPrefs.SetInt(nextDifficultyKey, 1);
        Debug.Log($"解锁下一个难度 {currentDifficulty + 1}，存储键：{nextDifficultyKey}");

        PlayerPrefs.Save();

        Debug.Log($"当前关卡名: {currentLevel}");

        // **存储通关状态到 PlayerPrefs**
        PlayerPrefs.SetInt("LastCompletedLevel", 1); // 1 代表已完成
        PlayerPrefs.SetString("CompletedLevelName", currentLevel);
        PlayerPrefs.Save();

        Dictionary<string, int> rewardData = new Dictionary<string, int>
        {
            { "Coin", reward }, // 如果 reward 是整数，比如 100
            { "Gem", rewardGem }
        };
        inventoryManager.AddRewards(rewardData);

        // 禁用所有名字包含 "EnemySpawner" 的对象
        DisableAllEnemySpawners();

        //Destroy(slider.gameObject);
        caveSr.sprite = caveSprites[0];
        gameOverCanvas.gameObject.SetActive(true);
        gameOverCanvas.enabled = true;

        spawnCanvas.enabled = false;
        spawnCanvas.gameObject.SetActive(false);
        DestroyAllDefenders();
        //foreach (var script in scriptsToDisable)
        //{
        //    script.enabled = false;
        //}



    }

    public void TakeDamage(int damage)
    {
        if (isGameOver) return; // 如果已经Game Over, 不执行下去

        health -= damage;
        slider.value = health;

        // 计算血量百分比
        float healthPercent = (float)health / maxHealth;
        fillImage.fillAmount = healthPercent;

        // **生成伤害数字**
        ShowDamage(damage);

        // 让 EaseFill 延迟跟随 Fill
        StopCoroutine("EaseHealthBar");
        StartCoroutine(EaseHealthBar(healthPercent));

        if (health <= 0)
        {
            isGameOver = true;
            TriggerVictory();
        }
    }

    // 生成伤害数字
    void ShowDamage(int damage)
    {
        if (damageTextPrefab != null)
        {
            // **与血条相同的高度，并再向上偏移 0.2**
            float heightOffset = col.bounds.size.y * 1.5f;
            Vector3 worldPos = transform.position + new Vector3(0, heightOffset + 10.8f, 0);

            // **实例化伤害数字**
            GameObject damageTextInstance = Instantiate(damageTextPrefab, slider.transform); // **让它跟随血条，而不是怪物**

            // **设置 UI 位置**
            damageTextInstance.transform.position = worldPos;

            // **设置伤害数值**
            damageTextInstance.GetComponent<DamageText>().SetDamage(damage);
        }
    }

    IEnumerator EaseHealthBar(float targetFillAmount)
    {
        yield return new WaitForSeconds(0.2f); // 延迟 0.2 秒后开始动画

        float currentFill = easeFillImage.fillAmount;

        // **让 EaseFill 目标值比 Fill 少一点，防止“超出”**
        targetFillAmount -= 0.03f; // 这里减少 1%，你可以调整这个值

        while (currentFill > targetFillAmount)
        {
            // **使用 MoveTowards 确保平稳过渡**
            currentFill = Mathf.MoveTowards(currentFill, targetFillAmount, easeSpeed * Time.deltaTime * 10);

            easeFillImage.fillAmount = currentFill;



            yield return null;
        }

        easeFillImage.fillAmount = targetFillAmount; // 确保最终与 Fill 相同
    }



    void DestroyAllDefenders()
    {
        // 找到所有标记为 "Player" 的对象
        GameObject[] defenders = GameObject.FindGameObjectsWithTag("Player");

        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
        foreach (GameObject defender in defenders)
        {
            Destroy(defender);  // 销毁每个防御者
        }

        foreach (GameObject monster in monsters)
        {
            Destroy(monster);  // 销毁每个防御者
        }
    }

    void DisableAllEnemySpawners()
    {
        foreach (GameObject spawner in enemySpawners)
        {
            if (spawner.activeSelf)
            {
                //Debug.Log($"Disabling EnemySpawner: {spawner.name}");
                spawner.SetActive(false);
            }
        }
    }

}
