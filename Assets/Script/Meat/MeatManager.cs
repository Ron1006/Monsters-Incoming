using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;



public class MeatManager : MonoBehaviour
{
    public static MeatManager Instance { get; private set; }
    private bool _isPrimary = false;   // ← 只在真正的单例上为 true

    [Header("Meat Settings")]
    public int baseMeatQuantity = 1;     // 基础肉数量
    public float baseLoadingSpeed = 0.5f; // 基础加载速度
    public int level = 1;                // 当前等级
    public int maxLevel = 20;            // 最大等级
    public float loadingSpeedRate = 0.12f; // 每级增加10%的速度

    public int meatQuantity;            // 当前肉的数量
    public float loadingSpeed;          // 当前加载速度
    private float timer = 0f;            // 计时器

    //public Image meatPIC;
    public Sprite[] meatImages; // 不同等级的图片

    public InventoryManager inventoryManager; // 管理金币的组件

    [Header("Upgrade Settings")]
    public int initialUpgradeCost = 100;
    public float upgradeRate = 1.5f;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("重复的 MeatManager 被创建，销毁！");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _isPrimary = true;             // 只有第一个实例是主实例
        DontDestroyOnLoad(gameObject);

        // 其余初始化…
        LoadMeatData();
    }

    // Start is called before the first frame update
    void Start()
    {
        inventoryManager = FindObjectOfType<InventoryManager>();
        //LoadMeatData();        // 加载等级数据
    }


    // 重新计算肉数量和加载速度
    private void RecalculateStats()
    {
        meatQuantity = baseMeatQuantity + (level - 1);           // 每升一级增加 1 肉量
        loadingSpeed = baseLoadingSpeed + (level - 1) * loadingSpeedRate;   // 每升一级增加 loadingSpeedRate 加载速度
    }

    // 保存数据到 Json
    public void SaveMeatData()
    {
        MeatData data = new MeatData
        {
            level = this.level,
            meatQuantity = this.meatQuantity
        };

        string json = JsonUtility.ToJson(data);
        Debug.Log($"正在保存meat数据到 {GetSavePath()} 内容: {json}");
        System.IO.File.WriteAllText(GetSavePath(), json);
    }

    // 从 Json 加载数据
    private void LoadMeatData()
    {
        string path = GetSavePath();
        if (System.IO.File.Exists(path))
        {
            string json = System.IO.File.ReadAllText(path);
            MeatData data = JsonUtility.FromJson<MeatData>(json);

            this.level = data.level;
            //this.meatQuantity = data.meatQuantity;
        }
        else
        {
            // default
            this.level = 1;
            //this.meatQuantity = baseMeatQuantity;
        }

        // 读完 level 之后再算一次 meatQuantity 和 loadingSpeed
        RecalculateStats();
    }

    private string GetSavePath()
    {
        return Application.persistentDataPath + "/meat_data.json";
    }

    // 更新meat图片
    public Sprite GetCurrentMeatSprite()
    {
        if (level <= meatImages.Length)
            return meatImages[level - 1];
        else
            return meatImages[meatImages.Length - 1];
    }

    public int GetUpgradeCost()
    {
        return Mathf.CeilToInt(initialUpgradeCost * Mathf.Pow(upgradeRate, level - 1));
    }

    // 升级函数（返回 true 表示成功）
    public bool LevelUpMeat()
    {
        if (level >= maxLevel)
        {
            Debug.Log("已达最大等级！");
            return false;
        }

        int cost = GetUpgradeCost();

        if (inventoryManager.GetCurrencyAmount("Coin") >= cost)
        {
            inventoryManager.AddItem(-cost, "Coin");
            level++;
            RecalculateStats();
            SaveMeatData();
            return true;
        }
        else
        {
            Debug.Log("金币不足");
            return false;
        }
    }

    private void OnApplicationQuit()
    {
        if (_isPrimary)
            SaveMeatData();
    }

    private void OnDestroy()
    {
        // 只有主实例才写文件，重复实例直接退出
        if (_isPrimary)
            SaveMeatData();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        inventoryManager = FindObjectOfType<InventoryManager>();
        SaveMeatData();  // 切换场景时自动保存一次
    }

    void OnApplicationPause(bool pause)
    {
        if (pause)      // 进入后台、被来电/锁屏时
        {
            SaveMeatData();
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)  // 失去焦点（切到别的 App）
        {
            SaveMeatData();
        }
    }
}

[System.Serializable]
public class MeatData
{
    public int level;
    public int meatQuantity;
}