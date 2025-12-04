using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DefenderHealth : MonoBehaviour
{
    public float health;
    public float maxHealth;
    public float delayTime = .15f;
    public Slider sliderPrefab; // 拖入你的 Slider 预制件
    private Image fillImage;     // 实时扣血的血条
    private Image easeFillImage; // 延迟扣血的血条
    public float easeSpeed = 10.1f; // 控制EaseFill延迟跟随的速度
    public Slider sliderInstance;
    public Vector3 offset = new Vector3(0, 1, 0); // 调整 slider 在defender头部的偏移量
    public GameObject damageTextPrefab; // 显示扣血数字

    public Camera mainCamera; // 用于将世界坐标转换为屏幕坐标
    public Canvas healthCanvas; // 拖入你的 Canvas，确保 slider 渲染在屏幕上
    public float sliderScale = 1.0f; // 用于调整血条缩放的参数
    private Rigidbody2D rb; // 引用 Rigidbody2D
    public ArcherMovement archerMovement;
    private Collider2D col; // 引用 Collider2D
    private ArcherMan archerMan;
    public ArcherState archerState;
    private DefenderAudio defenderAudio; // 引用DefenderAudio脚本

    private Animator defenderMovement; // 引用 Animator 组件
    public string attackTriggerName = "Dead";

    public Defender defender; // 引用Defender类

    // Start is called before the first frame update
    void Start()
    {

        // 初始化 Defender 引用
        InitializeDefenderReference();

        // 同步 Health 和 MaxHealth
        InitializeHealthValues();

        // 初始化 Animator 组件
        Invoke(nameof(InitializeAnimator), 0.2f); // **延迟 0.2 秒执行**

        // 初始化其他组件
        rb = GetComponent<Rigidbody2D>();
        defenderAudio = GetComponent<DefenderAudio>();
        col = GetComponent<Collider2D>();
        archerMan = GetComponent<ArcherMan>();

        // 自动查找或分配主摄像机
        InitializeMainCamera();

        // 初始化 Slider 并设置初始值
        InitializeSlider();
        UpdateHealthUI(); 

        //Debug.Log($"{gameObject.name} successfully initialized.");
    }

    // Update is called once per frame
    void Update()
    {
        float heightOffset = col.bounds.size.y * 0.8f; // 让血条始终在怪物头上方 0.2f 处
        if (sliderInstance != null) 
        {
            sliderInstance.transform.position = transform.position + new Vector3(0, heightOffset, 0);
        }
        
    }

    // 处理defender受到的伤害
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

            // **这里应该检查是否死亡**
            if (health <= 0)
            {
                defenderAudio.PlayDeathSound();
                Dead();  // **如果生命值小于等于 0，调用 Die()**
            }
        }
    }

    // 生成伤害数字
    // 生成伤害或治疗数字
    public void ShowDamage(int value, bool isHealing = false)
    {
        if (damageTextPrefab != null)
        {
            float heightOffset = col.bounds.size.y * 1.5f;
            Vector3 worldPos = transform.position + new Vector3(0, heightOffset + 10.8f, 0);

            // **实例化伤害数字**
            GameObject damageTextInstance = Instantiate(damageTextPrefab, sliderInstance.transform);

            // **设置 UI 位置**
            damageTextInstance.transform.position = worldPos;

            // **获取文本组件**
            DamageText damageTextComponent = damageTextInstance.GetComponent<DamageText>();

            if (damageTextComponent != null)
            {
                if (isHealing)
                {
                    damageTextComponent.SetHealing(value); // **调用治疗显示**
                }
                else
                {
                    damageTextComponent.SetDamage(value); // **调用伤害显示**
                }
            }
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
            currentFill = Mathf.MoveTowards(currentFill, targetFillAmount, easeSpeed * Time.deltaTime*30);

            easeFillImage.fillAmount = currentFill;



            yield return null;
        }

        easeFillImage.fillAmount = targetFillAmount; // 确保最终与 Fill 相同
    }

    public void TakeDamageBoss(int damage)
    {
        health -= damage;

        sliderInstance.value = health; // 更新该defender的 slider
        StartCoroutine(knockbackDelayBoss());
    }

    // Coroutine methods that can be paused and resumed
    IEnumerator knockbackDelay()
    {
        archerMovement.enabled = false;
        archerMan.enabled = true;
        yield return new WaitForSeconds(delayTime);

        if (health <= 0)
        {
            // 取消冻结 Y 轴的位移约束
            rb.constraints = RigidbodyConstraints2D.None;

            if (defenderAudio != null)
            {
                defenderAudio.PlayDeathSound();
            }

            Dead();
            
            Destroy(sliderInstance.gameObject); // 销毁该defender的 slider
        }
        else
        {
            // 在这里我们不直接启用移动，而是交由状态机决定
            if (archerState == ArcherState.Shooting)
            {
                // 如果还在射击状态，保持攻击
                
                archerMan.enabled = true;
                archerMovement.enabled = false;

            }
            else if (archerState == ArcherState.Walking)
            {
                // 如果在移动状态，恢复移动
                archerMovement.enabled = true;
                archerMan.enabled = false;
            }
        }
    }

    IEnumerator knockbackDelayBoss()
    {
        archerMovement.enabled = false;
        archerMan.enabled = true;
        yield return new WaitForSeconds(delayTime + 1f);

        if (health <= 0)
        {
            // 取消冻结 Y 轴的位移约束
            rb.constraints = RigidbodyConstraints2D.None;

            if (defenderAudio != null)
            {
                defenderAudio.PlayDeathSound();
            }

            Dead();

            Destroy(sliderInstance.gameObject); // 销毁该defender的 slider
        }
        else
        {
            // 在这里我们不直接启用移动，而是交由状态机决定
            if (archerState == ArcherState.Shooting)
            {
                // 如果还在射击状态，保持攻击

                archerMan.enabled = true;
                archerMovement.enabled = false;

            }
            else if (archerState == ArcherState.Walking)
            {
                // 如果在移动状态，恢复移动
                archerMovement.enabled = true;
                archerMan.enabled = false;
            }
        }
    }

    // 死亡时让defender飞出去
    void Dead()
    {
        // 触发攻击动画
        if (defenderMovement != null)
        {
            defenderMovement.SetTrigger("Dead");  // 确保触发器名称与 Animator 中的匹配
            Debug.Log($"{defenderMovement.gameObject.name} Death animation triggered.");
        }
        else
        {
            Debug.LogWarning("No Animator found. Death animation not triggered.");
        }

        // 遍历所有子对象，查找名称包含 "Weapon" 的子对象
        Transform weaponTransform = null;
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Weapon")) // 检查子对象名称是否以 "Weapon" 开头
            {
                weaponTransform = child;
                Destroy(weaponTransform.gameObject, 0.5f); // 立即销毁子对象
                //break; // 找到第一个匹配的对象后退出循环
            }
        }

        if (sliderInstance != null)
        {
            Destroy(sliderInstance.gameObject);
        }

        Destroy(gameObject, 0.5f); // 2秒后销毁游戏对象
    }

    private void InitializeSlider()
    {
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
    }



    // 确保 UI 显示正确
    void UpdateHealthUI()
    {
        if (sliderInstance != null)
        {
            sliderInstance.maxValue = maxHealth;
            sliderInstance.value = health;
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

    private void InitializeHealthValues()
    {
        if (defender != null)
        {
            maxHealth = defender.health;
            health = maxHealth;
            //Debug.Log($"{gameObject.name} initialized with MaxHealth: {maxHealth}, Health: {health}");
        }
        else
        {
            Debug.LogError($"{gameObject.name}: Defender reference is null, health values cannot be initialized.");
        }
    }

    private void InitializeAnimator()
    {
        Transform defenderTransform = transform.Find("Defender");
        if (defenderTransform != null)
        {
            defenderMovement = defenderTransform.GetComponent<Animator>();
        }

        // 如果在 Defender 上未找到 Animator，则递归查找整个层级中第一个 Animator
        if (defenderMovement == null)
        {
            defenderMovement = FindAnimatorInChildren(transform);
        }

        if (defenderMovement == null)
        {
            Debug.LogWarning($"{gameObject.name}: No Animator found in Defender or any child objects.");
        }
    }

    private Transform FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child;
            }
            Transform found = FindChildByName(child, name);
            if (found != null) return found;
        }
        return null;
    }

    private Animator FindAnimatorInChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            // 仅查找当前激活的对象
            if (!child.gameObject.activeInHierarchy)
            {
                continue; // 跳过未激活的 GameObject
            }

            Animator animator = child.GetComponent<Animator>();
            if (animator != null)
            {
                Debug.Log($"Animator found on {child.name}");
                return animator;
            }

            // 递归查找子对象
            Animator foundAnimator = FindAnimatorInChildren(child);
            if (foundAnimator != null)
            {
                return foundAnimator;
            }
        }
        return null;
    }

    private void InitializeMainCamera()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found!");
        }
    }


}
