//using System.Collections;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;

//public class DisplayTower : MonoBehaviour
//{

//    [Header("Tower Stats")]
//    public TMP_Text towerName;
//    public TMP_Text upgradeCostTMP;
//    public TMP_Text attackPowerTMP;
//    public TMP_Text healthTMP;
//    public TMP_Text towerLevel;
//    public Image towerPIC;
//    public Sprite[] towerImages;

//    [Header("Base Stats")]
//    public int baseAttackPower = 10;
//    public float baseHealth = 50f;
//    public int attackPowerPerLevel = 20;
//    public float healthPerLevel = 50f;

//    [Header("Current Stats")]
//    public int attackPower;
//    public float health;
//    public int level = 1;
//    public int maxLevel = 20;

//    [Header("Upgrade Settings")]
//    public int initialUpgradeCost = 100;
//    public float upgradeRate = 1.5f;

//    public InventoryManager inventoryManager; // 用于获取金币数量并减少金币
//    public Animator upgradeAnimator;
//    public GameObject animationObject;

//    private DefenderDataManager dataManager;
//    private TowerHealth towerHealth;


//    // 动态计算的升级费用,根据等级和费率倒推
//    public int UpgradeCost
//    {
//        get
//        {
//            return Mathf.CeilToInt(initialUpgradeCost * Mathf.Pow(upgradeRate, level - 1));
//        }
//    }


//    private void Awake()
//    {
//        //attackPowerPerLevel = 20;
//        //healthPerLevel = 50f;
//    }

//    // Start is called before the first frame update
//    void Start()
//    {
//        dataManager = DefenderDataManager.Instance;
//        if (dataManager == null)
//        {
//            Debug.LogError("DefenderDataManager instance not found!");
//            return;
//        }

//        LoadTowerData();  // 在对象启用时加载数据
//        RecalculateStats();  // 计算初始状态
//        // 初始化时更新 UI
//        UpdateUI();
//    }

//    //public void RecalculateAndSave()
//    //{
//    //    RecalculateStats();  // 重新计算生命值
//    //    SaveTowerData();     // 保存到 JSON 文件
//    //    UpdateUI();          // 更新 UI
//    //}

//    // 计算攻击力和血量
//    private void RecalculateStats()
//    {
//        attackPower = baseAttackPower + (level - 1) * attackPowerPerLevel;
//        health = baseHealth + (level - 1) * healthPerLevel;

//        towerHealth = GetComponent<TowerHealth>();
//        if (towerHealth != null)
//        {
//            towerHealth.maxHealth = health;
//            towerHealth.health = health;

//            if (towerHealth.healthSlider != null)
//            {
//                towerHealth.healthSlider.value = health;
//                towerHealth.healthSlider.maxValue = health;
//            }
//        }
//        SaveTowerData();

//        //Debug.Log($"Recalculated Stats: Level = {level}, Attack Power = {attackPower}, Health = {health}");
//    }

//    private void UpdateUI()
//    {

//        // 更新 UI 文本框内容
//        towerName.text = "Tower";
//        upgradeCostTMP.text = FormatUpgradeCost(UpgradeCost);
//        attackPowerTMP.text = attackPower.ToString();
//        healthTMP.text = health.ToString();
//        towerLevel.text = level.ToString();

//        // 设置图片，towerImages 按等级依次存放不同的图片
//        if (level <= towerImages.Length)
//        {
//            towerPIC.sprite = towerImages[level - 1];
//        }
//        else
//        {
//            towerPIC.sprite = towerImages[towerImages.Length - 1]; // 如果等级超过图片数量，使用最后一张图片
//        }
//    }

//    string FormatUpgradeCost(int cost)
//    {
//        if (cost >= 1000000000)
//            return (cost / 1000000000f).ToString("0.#") + "B";  // 十亿及以上
//        else if (cost >= 1000000)
//            return (cost / 1000000f).ToString("0.#") + "M";  // 百万及以上
//        else if (cost >= 1000)
//            return (cost / 1000f).ToString("0.#") + "K";  // 千及以上
//        else
//            return cost.ToString();  // 小于 1000，直接显示
//    }




//    public bool LevelUp()
//    {
//        inventoryManager = FindObjectOfType<InventoryManager>();
//        PlayAnimation();

//        if (level < maxLevel)
//        {
//            if (inventoryManager != null && inventoryManager.GetCurrentCoins() >= UpgradeCost)
//            {
//                inventoryManager.AddItem(-UpgradeCost);  // Deduct coins
//                level++;  // Increase level

//                RecalculateStats();  // Apply updated stats
//                SaveTowerData();     // Save after calculation
//                UpdateUI();          // Reflect on UI

//                Debug.Log($"{gameObject.name} 升级到 {level} 级！攻击力：{attackPower}，血量：{health}");
//                return true;
//            }
//            else
//            {
//                Debug.LogWarning("Not enough coin.");
//                return false;
//            }
//        }
//        else
//        {
//            Debug.Log($"{gameObject.name} 已达到最大等级！");
//            return false;
//        }
//    }

//    private void SaveTowerData()
//    {
//        if (dataManager == null)
//        {
//            Debug.LogError("DefenderDataManager instance not found. Cannot save data.");
//            return;
//        }

//        List<DefenderData> defenderDataList = dataManager.LoadDefenderData() ?? new List<DefenderData>();
//        DefenderData towerData = defenderDataList.Find(d => d.name == "Tower");

//        if (towerData == null)
//        {
//            towerData = new DefenderData
//            {
//                name = "Tower"
//            };
//            defenderDataList.Add(towerData);
//        }

//        towerData.level = level;
//        towerData.attackPower = attackPower; // 保存计算后的攻击力
//        towerData.health = health;           // 保存计算后的血量
//        towerData.baseAttackPower = baseAttackPower;
//        towerData.baseHealth = baseHealth;

//        dataManager.SaveDefenderData(defenderDataList);
//        //Debug.Log("Tower data saved to JSON.");
//    }

//    void LoadTowerData()
//    {
//        if (dataManager == null)
//        {
//            Debug.LogError("DefenderDataManager instance not found. Cannot load data.");
//            return;
//        }

//        List<DefenderData> defenderDataList = dataManager.LoadDefenderData();
//        if (defenderDataList != null)
//        {
//            DefenderData towerData = defenderDataList.Find(d => d.name == "Tower");
//            if (towerData != null)
//            {
//                level = towerData.level;
//                baseAttackPower = towerData.baseAttackPower;
//                baseHealth = towerData.baseHealth;
//                //Debug.Log($"Loaded Tower data: Level = {level}, Attack Power = {towerData.attackPower}, Health = {towerData.health}");

//            }
//            else
//            {
//                Debug.LogWarning("Tower data not found in JSON. Using default values.");
//            }
//        }
//        else
//        {
//            Debug.LogError("Failed to load defender data from JSON.");
//        }
//    }


//    void PlayAnimation()
//    {
//        if (upgradeAnimator != null)
//        {

//            animationObject.SetActive(true);
//            upgradeAnimator.SetTrigger("UpgradeArrowTrigger");


//            // 启动协程，等待动画播放完成后禁用子对象
//            StartCoroutine(DisableAnimationObjectAfterDelay(0.5f));
//        }
//    }

//    IEnumerator DisableAnimationObjectAfterDelay(float delay)
//    {
//        yield return new WaitForSeconds(delay);
//        // 遍历禁用所有动画对象

//        if (animationObject != null)
//        {
//            animationObject.SetActive(false);  // 禁用子对象
//            upgradeAnimator.ResetTrigger("UpgradeArrowTrigger");
//        }

//    }
//}
