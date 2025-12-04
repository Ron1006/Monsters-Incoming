using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeleeHealth : MonoBehaviour
{
    public float health;
    public float maxHealth;
    public float delayTime = .15f;
    public Slider sliderPrefab; // 拖入你的 Slider 预制件
    public Slider sliderInstance;
    public Vector3 offset = new Vector3(0, 1, 0); // 调整 slider 在defender头部的偏移量
    public Camera mainCamera; // 用于将世界坐标转换为屏幕坐标
    public Canvas canvas; // 拖入你的 Canvas，确保 slider 渲染在屏幕上
    private Rigidbody2D rb; // 引用 Rigidbody2D
    public MeleeMovement meleeMovement;
    private Collider2D col; // 引用 Collider2D
    private MeleeAttack meleeAttack;
    private DefenderAudio defenderAudio;  // 引用DefenderAudio脚本

    private Animator defenderMovement; // 引用 Animator 组件
    public string attackTriggerName = "Dead";

    public Defender defender; // 引用Defender类



    // Start is called before the first frame update
    void Start()
    {
        // 在父对象或子对象中查找 Defender 组件
        defender = GetComponentInParent<Defender>();
        // 确保 health 和 maxHealth 与 Defender 类同步
        if (defender != null)
        {
            maxHealth = defender.health;
            health = defender.health;
            //Debug.Log(defender + " maxHealth: " + maxHealth + " current: " + health);
        }
        else
        {
            Debug.LogError("Defender reference is null!");
        }

        // 查找子对象中名为 "Defender" 的对象并获取 Animator 组件
        defenderMovement = transform.Find("Defender").GetComponent<Animator>();


        // 获取DefenderAudio脚本组件
        defenderAudio = GetComponent<DefenderAudio>();

        // 获取 Rigidbody2D 组件
        rb = GetComponent<Rigidbody2D>();

        // 获取 Collider2D 组件
        col = GetComponent<Collider2D>();

        meleeAttack = GetComponent<MeleeAttack>();

        // 自动查找主摄像机
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // 查找场景中的 Canvas，假设场景中只有一个 Canvas
        if (canvas == null)
        {
            GameObject healthCanvasObj = GameObject.Find("HealthCanvas");
            canvas = healthCanvasObj.GetComponent<Canvas>();
        }

        // 实例化 Slider，并将其作为 Canvas 的子对象
        sliderInstance = Instantiate(sliderPrefab, canvas.transform);
        sliderInstance.maxValue = maxHealth;
        sliderInstance.value = health; // 在实例化之后设置 sliderInstance 的初始值

        // 调用一个刷新 UI 的方法，确保 Slider 在初始时显示正确
        UpdateHealthUI();

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
    }

    // Update is called once per frame
    void Update()
    {
        // 更新 slider 位置，使其位于defender头部
        if (sliderInstance != null && mainCamera != null)
        {
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(transform.position + offset);
            sliderInstance.transform.position = screenPosition;
        }
    }
    void LateUpdate()
    {
        if (sliderInstance != null)
        {
            sliderInstance.value = health;
        }
    }

    // 处理defender受到的伤害
    public void TakeDamage(int damage)
    {
        health -= damage;
        sliderInstance.value = health; // 更新该defender的 slider
        StartCoroutine(knockbackDelay());
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
        meleeMovement.enabled = false;
        meleeAttack.enabled = false;
        yield return new WaitForSeconds(delayTime);

        if (health <= 0)
        {
            if (defenderAudio != null)
            {
                defenderAudio.PlayDeathSound();
            }

            Dead();
            if (sliderInstance.gameObject != null)
            {
                Destroy(sliderInstance.gameObject); // 销毁该defender的 slider
            }
            
        }
        else
        {
            meleeMovement.enabled = true;
            meleeAttack.enabled = true;
        }
    }

    IEnumerator knockbackDelayBoss()
    {
        meleeMovement.enabled = false;
        meleeAttack.enabled = false;
        yield return new WaitForSeconds(delayTime + 1f);

        if (health <= 0)
        {
            if (defenderAudio != null)
            {
                defenderAudio.PlayDeathSound();
            }

            Dead();

            Destroy(sliderInstance.gameObject); // 销毁该defender的 slider
        }
        else
        {
            meleeMovement.enabled = true;
            meleeAttack.enabled = true;
        }
    }

    // 死亡时让defender飞出去
    void Dead()
    {
        // 触发死亡动画
        defenderMovement.SetTrigger(attackTriggerName);

        // 遍历所有带有 "Weapon_lv" 前缀的子对象
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Weapon_lv") && child.gameObject.activeSelf)
            {
                Debug.Log("Active weapon object found and will be destroyed immediately: " + child.name);
                Destroy(child.gameObject); // 销毁当前激活的武器
                break; // 找到后退出循环
            }
        }

        Destroy(gameObject, 0.5f); // 0.5秒后销毁游戏对象
    }

    //在升级后更新健康值
    public void UpdateHealthAfterUpgrade()
    {
        if(defender != null)
        {
            maxHealth = defender.health; // 升级后更新最大健康值
            health = maxHealth; // 更新当前健康值
            sliderInstance.maxValue = maxHealth; // 更新 Slider 最大值
            sliderInstance.value = health; // 更新 Slider 当前值
        }

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
}
