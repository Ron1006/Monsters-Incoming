using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TowerHealth : MonoBehaviour
{
    [Header("Tower Health Settings")]
    public float health;
    public float maxHealth = 50;
    public Slider healthSlider;


    [Header("Tower Visuals")]
    public Sprite[] towerSprites;
    public SpriteRenderer towerSr;

    [Header("UI Components")]
    public Canvas gameOverCanvas;
    public Canvas spawnCanvas;

    [Header("Scripts to Disable on Game Over")]
    public List<MonoBehaviour> scriptsToDisable = new List<MonoBehaviour>(); // 设置当game over时需要禁用的脚本

    //public int level = 1;

    private Tower tower;
    private float lastHealth;
    private bool isGameOver = false;

    public Defender defender; // 引用Defender类

    // Start is called before the first frame update
    void Start()
    {
        tower = GetComponent<Tower>(); // 直接获取挂载在同一对象上的 Tower 组件

        defender = GetComponent<Defender>();

        // 初始化 Defender 引用
        InitializeDefenderReference();

        // 查找 HealthSlider
        healthSlider = GameObject.Find("HealthSlider")?.GetComponent<Slider>();

        // 查找所有 MonoBehaviour 组件, (true)包括禁用对象
        MonoBehaviour[] allScripts = FindObjectsOfType<MonoBehaviour>(true);

        // 定义要匹配的前缀
        string[] prefixes = { "EnemySpawnerWave", "EnemySpawnerRegular" };

        // 筛选名称以指定前缀开头的对象
        foreach (var script in allScripts)
        {
            foreach (var prefix in prefixes)
            {
                if (script.gameObject.name.StartsWith(prefix))
                {
                    scriptsToDisable.Add(script);
                    break; // 找到匹配项后跳出内层循环，避免重复添加
                }
            }
        }
        scriptsToDisable.Add(GameObject.Find("HealthCanvas")?.GetComponent<CanvasScaler>());
        

        LoadTowerData();
        SyncHealthWithTower(); // 同步初始数据
        InitializeUI();
        //UpdateTowerSprite();
        
        lastHealth = health; // 记录初始生命值

        // 获取 Fill 和 EaseFill 组件
        //fillImage = healthSlider.transform.Find("Fill Area/Fill").GetComponent<Image>();
        //easeFillImage = healthSlider.transform.Find("Fill Area/EaseFill").GetComponent<Image>();

    }

    // Update is called once per frame
    void Update()
    {
        // 避免重复调用 value = health
        if (health != lastHealth)
        {
            healthSlider.value = health;
            lastHealth = health;
        }
 
        // 避免重复调用方法
        if (!isGameOver && health <= 0)
        {
            TriggerGameOver();
            isGameOver = true;
        }
        if (tower != null)
        {
            SyncHealthWithTower(); // 实时同步 Tower 数据
        }
    }

    private void InitializeUI()
    {
        healthSlider.maxValue = maxHealth;
        healthSlider.value = health;
        gameOverCanvas.enabled = false;

        if (spawnCanvas == null)
        {
            spawnCanvas = GameObject.Find("SpawnCanvas")?.GetComponent<Canvas>();
            if (spawnCanvas == null)
                Debug.LogWarning("SpawnCanvas not found in the scene!");
        }
    }




    private void LoadTowerData()
    {
        maxHealth = defender.health; ; // 从 Tower 获取最大生命值
        health = maxHealth;
        Debug.Log($"Tower loaded with maxHealth {maxHealth}, health {health}");
    }

    public void TakeDamage(float damage)
    {
        //Debug.Log($"[TOWER] Took {damage} damage. Before: {health}");
        health -= damage;

        if (healthSlider != null)
        {
            healthSlider.value = health;
        }

        //Debug.Log($"[TOWER] After damage, health = {health}");

        if (health <= 0 && !isGameOver)
        {
            TriggerGameOver();
            isGameOver = true;
        }
    }


    public void TriggerGameOver()
    {
        health = 0;

        DestroyAllDefenders();

        gameOverCanvas.gameObject.SetActive(true);
        gameOverCanvas.enabled = true;
        
        //gameOverCanvas.gameObject.SetActive(true);

        // 禁用指定的脚本
        foreach (var script in scriptsToDisable)
        {
            script.enabled = false;
        }

        spawnCanvas.enabled = false;
        spawnCanvas.gameObject.SetActive(false);


    }

    private void DestroyAllDefenders()
    {
        // 找到所有标记为 "Player" 的对象
        GameObject[] defenders = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject defender in defenders)
        {
            Destroy(defender);  // 销毁每个防御者
        }
    }

    // 同步 Tower 的生命值
    private void SyncHealthWithTower()
    {
        if (tower != null)
        {
            maxHealth = defender.health;        // 同步 Tower 的最大生命值
            health = defender.health; // 防止当前生命值超出最大值
            healthSlider.maxValue = maxHealth;
        }
    }

    private void InitializeDefenderReference()
    {
        defender = GetComponentInParent<Defender>();
        if (defender == null)
        {
            Debug.LogError($"{gameObject.name}: Defender component is missing on parent object.");
        }
    }
}
