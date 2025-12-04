using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttack : MonoBehaviour
{
    public int damage = 15;
    private bool canAttack = true; // 控制是否可以攻击
    public float attackCooldown = 2.0f; // 攻击冷却时间
    private Animator weaponAnimator; // 引用 Animator 组件
    public string attackTriggerName = "Attack";
    public float attackDelay = 0.5f; // 攻击伤害延迟时间
    public float knockbackForce = 50f; // 击退力的大小
    private DefenderAudio defenderAudio;  // 引用 DefenderAudio 脚本
    public Defender defender; // 引用 Defender 类

    void Start()
    {
        // 查找父对象或子对象中的 Defender 组件
        defender = GetComponentInParent<Defender>();
        if (defender != null)
        {
            damage = defender.attackPower;
        }
        else
        {
            Debug.LogError("Defender reference is null!");
        }

        // 延迟 0.1 秒调用 SetActiveWeaponAnimator 以确保 Defender 已完全初始化
        Invoke("SetActiveWeaponAnimator", 0.1f);

        // 获取 DefenderAudio 脚本组件
        defenderAudio = GetComponent<DefenderAudio>();


    }

    void Update()
    {
        if (defender != null)
        {
            damage = defender.attackPower; // 实时同步攻击力
        }
        //SetActiveWeaponAnimator();
    }

    // 查找激活的武器的animator
    void SetActiveWeaponAnimator()
    {
        // 重置 Animator
        weaponAnimator = null;

        // 遍历所有武器子对象
        for (int i = 1; i <= 5; i++)
        {
            Transform weaponTransform = transform.Find($"Weapon_lv{i}");
            if (weaponTransform != null && weaponTransform.gameObject.activeSelf)
            {
                weaponAnimator = weaponTransform.GetComponent<Animator>();
                if (weaponAnimator != null)
                {
                    Debug.Log($"Found active weapon: Weapon_lv{i} with Animator component.");
                    break;
                }
            }
        }

        // 如果没有找到激活的武器或 Animator 组件，输出错误信息
        if (weaponAnimator == null)
        {
            Debug.LogError("No active weapon with Animator component found!");
        }
    }

    // 使用 OnCollisionStay2D 来检测持续碰撞
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (canAttack)
        {
            // 检查是否碰撞到 Cave
            if (collision.gameObject.GetComponent<CaveHealth>())
            {
                TriggerAttack(collision.gameObject, "Cave");
            }
            // 检查是否碰撞到 Monster
            else if (collision.gameObject.GetComponent<EnemyHealth>())
            {
                TriggerAttack(collision.gameObject, "Enemy");
            }
        }
    }

    // 触发攻击动画并处理冷却
    private void TriggerAttack(GameObject target, string targetType)
    {
        if(weaponAnimator != null)
        {
            // 触发攻击动画
            weaponAnimator.SetTrigger(attackTriggerName);
        }
        

        // 开始伤害和击退的延迟处理
        StartCoroutine(DelayedDamageAndKnockback(target, targetType));

        // 启动协程重置 Trigger
        StartCoroutine(ResetTriggerAfterAnimation());

        // 开始冷却协程
        StartCoroutine(AttackCooldown());
    }

    // 协程处理攻击冷却
    IEnumerator AttackCooldown()
    {
        canAttack = false; // 暂时禁止攻击
        yield return new WaitForSeconds(attackCooldown); // 等待冷却时间
        canAttack = true; // 恢复攻击
    }

    // 延迟伤害和击退的协程，确保动画完成后触发
    IEnumerator DelayedDamageAndKnockback(GameObject target, string targetType)
    {
        yield return new WaitForSeconds(attackDelay); // 等待攻击动画完成

        if (defenderAudio != null)
        {
            defenderAudio.PlayAttackSound();
        }

        // 处理 Cave 或 Enemy 的伤害和击退效果
        if (targetType == "Cave")
        {
            CaveHealth caveHealth = target.GetComponent<CaveHealth>();
            if (caveHealth != null)
            {
                caveHealth.health -= damage;  // 扣除血量
            }
        }
        else if (targetType == "Enemy")
        {
            // 处理敌人伤害和击退效果
            EnemyHealth enemyHealth = target.GetComponent<EnemyHealth>();
            Rigidbody2D enemyRb = target.GetComponent<Rigidbody2D>();

            if (enemyHealth != null && enemyRb != null)
            {
                // 计算击退方向（从攻击者位置到敌人的方向）
                Vector2 knockbackDirection = (target.transform.position - transform.position).normalized;

                // 应用击退力
                enemyRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

                // 造成伤害
                enemyHealth.TakeDamage(damage);
            }
        }
    }

    // 协程在动画完成后重置 Trigger
    IEnumerator ResetTriggerAfterAnimation()
    {
        yield return new WaitForSeconds(attackDelay); // 等待动画播放时间或攻击延迟
        if(weaponAnimator != null)
        {
            weaponAnimator.ResetTrigger(attackTriggerName); // 重置 Trigger
        }
        
    }
}
