using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // 添加这行来引入UI命名空间

public class EnemyHealth : MonoBehaviour
{
    public int health;
    public int maxHealth = 50;
    
    public float delayTime = .15f;
    public MonsterMovement monsterMovement;
    public Slider sliderPrefab; // 拖入你的 Slider 预制件
    private Image fillImage;     // 实时扣血的血条
    private Image easeFillImage; // 延迟扣血的血条
    private Slider sliderInstance;
    public float sliderScale = 1.0f; // 用于调整血条缩放的参数
    public float easeSpeed = 0.01f; // 控制EaseFill延迟跟随的速度
    public GameObject damageTextPrefab; // 显示扣血数字
    //public Transform damageTextPosition; // 伤害数字位置

    public Vector3 deathEffectOffset = new Vector3(0, 0, 0); // 死亡动画位置偏移
    public Vector3 offset = new Vector3(0, 1, 0); // 调整 slider 在怪物头部的偏移量
    public Camera mainCamera; // 用于将世界坐标转换为屏幕坐标
    public Canvas healthCanvas; // 拖入你的 Canvas，确保 slider 渲染在屏幕上
    private MonsterController monsterController; // 引用 MonsterController
    private Rigidbody2D rb; // 引用 Rigidbody2D
    private Collider2D col; // 引用 Collider2D
    public GameObject coin;
    public InventoryManager inventoryManager;
    public int quantity; // monster的金币数量
    private MonsterAudio monsterAudio;  // 引用DefenderAudio脚本

    private GameObject deathEffectPrefab; // 自动加载的死亡动画 Prefab



    

    // Start is called before the first frame update
    void Start()
    {

        health = maxHealth;

        // 获取 Rigidbody2D 组件
        rb = GetComponent<Rigidbody2D>();

        // 获取 Collider2D 组件
        col = GetComponent<Collider2D>();

        if (col != null)
        {
            // 打印宽度
            float width = col.bounds.size.x;
            //Debug.Log($"Monster '{gameObject.name}' has a Collider2D with width: {width}");
        }
        else
        {
            Debug.LogWarning($"No Collider2D found on Monster '{gameObject.name}'");
        }

        // 自动加载死亡动画 Prefab
        deathEffectPrefab = Resources.Load<GameObject>("DeathEffect");

        monsterController = GetComponent<MonsterController>();

        monsterAudio = GetComponent<MonsterAudio>();

        

        // 自动查找主摄像机
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // 查找场景中的 Canvas，假设场景中只有一个 Canvas
        if (healthCanvas == null)
        {
            GameObject healthCanvasObj = GameObject.Find("HealthCanvasUnit");
            healthCanvas = healthCanvasObj.GetComponent<Canvas>();
        }

        // **实例化血条并附加到怪物**
        sliderInstance = Instantiate(sliderPrefab, healthCanvas.transform);

        // **在怪物头顶创建血条**
        sliderInstance.transform.localPosition = offset; // `offset` 代表头顶偏移量
        sliderInstance.transform.localScale = new Vector3(sliderScale, sliderScale, 1f);

        // **设置血条初始值**
        sliderInstance.maxValue = maxHealth;
        sliderInstance.value = health; // 在实例化之后设置 sliderInstance 的初始值

        // 获取 Fill 和 EaseFill 组件
        fillImage = sliderInstance.transform.Find("Fill Area/Fill").GetComponent<Image>();
        easeFillImage = sliderInstance.transform.Find("Fill Area/EaseFill").GetComponent<Image>();


        // 初始化时隐藏血条
        sliderInstance.gameObject.SetActive(false);

        // 获取主摄像头
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // 检查 mainCamera 是否为空
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found!");
        }

        // 从子对象中查找 MonsterController 组件
        monsterController = GetComponentInChildren<MonsterController>();

        // 如果没有找到 MonsterController，输出调试信息
        if (monsterController == null)
        {
            Debug.LogError("MonsterController not found on child objects!");
        }

        // 自动获取inventoryManager
        if (inventoryManager == null) // 自动获取
        {
            inventoryManager = InventoryManager.Instance;
            if (inventoryManager == null)
            {
                Debug.LogError("InventoryManager instance is not set!");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

        float heightOffset = col.bounds.size.y * 0.8f; // 让血条始终在怪物头上方 0.2f 处
        sliderInstance.transform.position = transform.position + new Vector3(0, heightOffset, 0);
    }

    // 处理怪物受到的伤害
    public void TakeDamage(int damage)
    {
        // 显示血条并更新
        if (sliderInstance != null)
        {
            health -= damage;
            sliderInstance.value = health;
            // 计算血量百分比
            float healthPercent = (float)health / maxHealth;
            fillImage.fillAmount = healthPercent;
            sliderInstance.gameObject.SetActive(true);

            // **生成伤害数字**
            ShowDamage(damage);

            // 让 EaseFill 延迟跟随 Fill
            StopCoroutine("EaseHealthBar");
            StartCoroutine(EaseHealthBar(healthPercent));
        }

        // 如果血量小于等于 0，则触发死亡逻辑
        if (health <= 0)
        {
            monsterAudio.PlayDeathSound();
            Die();
        }
    }

    // 生成伤害数字
    void ShowDamage(int damage)
    {
        if (damageTextPrefab != null )
        {
            // **与血条相同的高度，
            float heightOffset = col.bounds.size.y * 1.5f;
            Vector3 worldPos = transform.position + new Vector3(0, heightOffset + 10.8f, 0);

            // **实例化伤害数字**
            GameObject damageTextInstance = Instantiate(damageTextPrefab, sliderInstance.transform); // **让它跟随血条，而不是怪物**

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
            currentFill = Mathf.MoveTowards(currentFill, targetFillAmount, easeSpeed * Time.deltaTime * 30);

            easeFillImage.fillAmount = currentFill;



            yield return null;
        }

        easeFillImage.fillAmount = targetFillAmount; // 确保最终与 Fill 相同
    }

    private void Die()
    {
        
        // 禁用怪物的移动和其他功能
        monsterMovement.enabled = false;
        col.enabled = false;
        // Drop loot (instantiate the coin prefab) destroy只能删除instance实例，不能删除prefab或者asset
        Vector3 coinSpawnPosition = transform.position + new Vector3(0, 0, 0); // 向上偏移1单位
        GameObject coinInstance = Instantiate(coin, coinSpawnPosition, Quaternion.identity);
        inventoryManager.AddItem(quantity, "Coin");

        Destroy(coinInstance, 2f);

        if (monsterAudio != null)
        {
            monsterAudio.PlayDeathSound();
        }

        // 播放死亡动画
        if (deathEffectPrefab != null)
        {
            GameObject deathEffect = Instantiate(deathEffectPrefab, transform.position + deathEffectOffset, Quaternion.identity);
            //Animator animator = deathEffect.GetComponent<Animator>();


            // 调整死亡动画大小
            AdjustDeathEffectSize(deathEffect);
            
            col.enabled = false;
            Destroy(deathEffect, 2f); // 2 秒后销毁死亡动画实例
        }

        // 销毁血条
        if (sliderInstance != null)
        {
            Destroy(sliderInstance.gameObject);
        }

        // 销毁怪物本身
        Destroy(gameObject);
    }


    private void AdjustDeathEffectSize(GameObject deathEffect)
    {
        // 检查死亡特效的 Collider2D
        Collider2D effectCollider = deathEffect.GetComponent<Collider2D>();
        if (effectCollider != null)
        {
            //Debug.Log($"333 DeathEffect '{deathEffect.name}' Collider2D bounds: {effectCollider.bounds.size}");
        }
        else
        {
            Debug.LogWarning($"No Collider2D found on DeathEffect '{deathEffect.name}'.");
        }

        float monsterWidth = col.bounds.size.x;
        float effectWidth = GetObjectWidth(deathEffect);
        float effectHeight = GetObjectHeight(deathEffect);

        //Debug.Log(monsterWidth);
        //Debug.Log(GetObjectWidth(deathEffect));
        //Debug.Log(GetObjectHeight(deathEffect));
        if (effectWidth > 0 && effectHeight > 0 && monsterWidth > 0)
        {
            float scaleRatio = (monsterWidth * 4.5f) / effectWidth;
            float heightRatio = effectHeight * scaleRatio; // 按比例缩放高度
            deathEffect.transform.localScale = new Vector3(scaleRatio, scaleRatio, 1f);
            //Debug.Log($"Adjusted death effect size: ScaleRatio = {scaleRatio}, HeightRatio = {heightRatio}");
        }
        else
        {
            Debug.LogWarning("Failed to determine dimensions. Using default scale.");
            deathEffect.transform.localScale = Vector3.one; // 默认比例
        }
        
    }

    private float GetObjectWidth(GameObject obj)
    {
        Collider2D collider = GetCollider(obj);
        if (collider != null)
        {
            return collider.bounds.size.x;
        }
        Debug.LogWarning($"No Collider2D found on {obj.name}");
        return 0f;
    }

    private float GetObjectHeight(GameObject obj)
    {
        Collider2D collider = GetCollider(obj);
        if (collider != null)
        {
            return collider.bounds.size.y;
        }
        Debug.LogWarning($"No Collider2D found on {obj.name}");
        return 0f;
    }

    private Collider2D GetCollider(GameObject obj)
    {
        Collider2D collider = obj.GetComponent<Collider2D>();
        if (collider == null)
        {
            collider = obj.GetComponentInChildren<Collider2D>();
        }
        return collider;
    }




}
