using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAttack : MonoBehaviour
{
    public int damage = 25;
    private bool canAttack = true; // 控制是否可以攻击
    public float attackCooldown = 2.0f; // 攻击冷却时间
    private MonsterController monsterController; // 引用 MonsterController
    public float attackDelay = 0.5f; // 攻击伤害延迟时间
    public float knockbackForce = 50f; // 击退力的大小
    private MonsterAudio monsterAudio;  // 引用DefenderAudio脚本

    // Toggle for the initial knockback skill, its only for boss
    public bool enableInitialKnockback = false; // 可以通过 Unity Inspector 启用/禁用初始击退技能
    public float initialKnockbackForce = 150f; // 初始击退的力度

    // 范围攻击设置
    public bool enableAreaAttack = false; //是否范围攻击
    public float attackRadius = 3.0f; // 攻击范围半径


    void Start()
    {
        // 从子对象中查找 MonsterController 组件
        monsterController = GetComponentInChildren<MonsterController>();

        // 获取DefenderAudio脚本组件
        monsterAudio = GetComponent<MonsterAudio>();

        // 如果没有找到 MonsterController，输出调试信息
        if (monsterController == null)
        {
            Debug.LogError("MonsterController not found on child objects!");
        }

        // Apply initial knockback to all "Player" objects if enabled
        if (enableInitialKnockback)
        {
            ApplyInitialKnockback();
        }
    }

    // 初始击退效果：推动所有标记为 "Player" 的对象
    void ApplyInitialKnockback()
    {
        Debug.Log("Applying initial knockback and damage to all defenders");
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                // 造成伤害并触发击退后禁用动作
                MeleeHealth meleeHealth = player.GetComponent<MeleeHealth>();
                if (meleeHealth != null)
                {
                    meleeHealth.TakeDamageBoss(1); // 根据你的需求设定伤害值
                }
                // 造成伤害并触发击退后禁用动作
                DefenderHealth defenderHealth = player.GetComponent<DefenderHealth>();
                if (defenderHealth != null)
                {
                    defenderHealth.TakeDamageBoss(1); // 根据你的需求设定伤害值
                }

                // 计算击退方向（从怪物位置到玩家的方向）
                Vector2 knockbackDirection = (player.transform.position - transform.position).normalized;

                // 应用击退力
                playerRb.AddForce(knockbackDirection * initialKnockbackForce, ForceMode2D.Impulse);

                

               
            }
        }
    }

    // onCollisionStay2D 可以持续检测， OnCollisionEnter2D只能检测一次
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (canAttack)
        {
            if(enableAreaAttack)
            {
                //启用范围攻击
                AreaAttack();
            }
            

            // 检测是否碰撞到了 Tower
            if (collision.gameObject.GetComponent<TowerHealth>())
            {
                // 调用 MonsterController 的 TriggerAttack() 方法，触发攻击动画
                monsterController.TriggerAttack();

                StartCoroutine(DelayedDamageAndKnockback(collision.gameObject, "Tower"));
                StartCoroutine(AttackCooldown()); // 启动冷却
            }
            // 检测是否碰撞到了 Defender
            else if (collision.gameObject.GetComponent<DefenderHealth>())
            {
                // 调用 MonsterController 的 TriggerAttack() 方法，触发攻击动画
                monsterController.TriggerAttack();

                StartCoroutine(DelayedDamageAndKnockback(collision.gameObject, "Defender"));
                StartCoroutine(AttackCooldown()); // 启动冷却
            }
            else if (collision.gameObject.GetComponent<MeleeHealth>())
            {
                // 调用 MonsterController 的 TriggerAttack() 方法，触发攻击动画
                monsterController.TriggerAttack();

                StartCoroutine(DelayedDamageAndKnockback(collision.gameObject, "Melee"));
                StartCoroutine(AttackCooldown()); // 启动冷却
            }
        }
    }

    // 新增：范围攻击方法
    void AreaAttack()
    {
        Debug.Log("Performing area attack");

        

        // 获取攻击范围内的所有碰撞对象
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, attackRadius);
        foreach(Collider2D collider in hitColliders)
        {
            GameObject target = collider.gameObject;

            //检查目标是否是Defender 或者 Melee
            DefenderHealth defenderHealth = target.GetComponent<DefenderHealth>();
            MeleeHealth meleeHealth = target.GetComponent<MeleeHealth>();
            TowerHealth towerHealth = target.GetComponent<TowerHealth>();
            Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();

            if(defenderHealth != null && targetRb != null)
            {
                // 触发攻击动画（只触发一次，而不是对每个目标触发）
                monsterController.TriggerAttack();
                StartCoroutine(DelayedDamageAndKnockbackArea(target, "Defender"));
            }
            else if (meleeHealth != null && targetRb != null)
            {
                Debug.Log($"AreaAttack found Melee: {target.name}");
                // 触发攻击动画（只触发一次，而不是对每个目标触发）
                monsterController.TriggerAttack();
                StartCoroutine(DelayedDamageAndKnockbackArea(target, "Melee"));
            }
            else if (towerHealth != null)
            {
                // 触发攻击动画（只触发一次，而不是对每个目标触发）
                Debug.Log($"AreaAttack found Tower: {target.name}");
                monsterController.TriggerAttack();
                StartCoroutine(DelayedDamageAndKnockbackArea(target, "Tower"));
            }
            else
            {
                Debug.Log($"AreaAttack ignored object: {target.name}");
            }
        }
        // 冷却时间
        StartCoroutine(AttackCooldown());
    }

    // 协程处理攻击冷却
    IEnumerator AttackCooldown()
    {
        canAttack = false; // 暂时禁止攻击
        yield return new WaitForSeconds(attackCooldown); // 等待 2 秒
        canAttack = true; // 恢复攻击
    }

    // 延迟伤害和击退的协程，确保动画完成后触发
    IEnumerator DelayedDamageAndKnockback(GameObject target, string targetType)
    {
        // 等待攻击延迟时间，确保动画播放到指定位置
        yield return new WaitForSeconds(attackDelay);

        if (monsterAudio != null)
        {
            monsterAudio.PlayAttackSound();
        }
        

        // 根据类型对目标造成伤害
        if (targetType == "Tower")
        {
            TowerHealth towerHealth = target.GetComponent<TowerHealth>();
            if (towerHealth != null)
            {
                towerHealth.health -= damage;
            }
        }
        else if (targetType == "Defender" && targetType != null)
        {
            if (target == null)
            {
                Debug.LogWarning("[MonsterAttack] Target was destroyed before damage could be applied.");
                yield break; // **终止协程**
            }

            DefenderHealth defenderHealth = target.GetComponent<DefenderHealth>();
            Rigidbody2D defenderRb = target.GetComponent<Rigidbody2D>();
            if (defenderHealth != null && defenderRb != null)
            {
                // 计算击退方向（从攻击者位置到敌人的方向）
                Vector2 knockbackDirection = (target.transform.position - transform.position).normalized;

                // 应用击退力
                defenderRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

                // 造成伤害
                defenderHealth.TakeDamage(damage);
            }
        }
        else if (targetType == "Melee")
        {
            MeleeHealth meleeHealth = target.GetComponent<MeleeHealth>();
            Rigidbody2D meleeRb = target.GetComponent<Rigidbody2D>();
            if (meleeHealth != null && meleeRb != null)
            {
                // 计算击退方向（从攻击者位置到敌人的方向）
                Vector2 knockbackDirection = (target.transform.position - transform.position).normalized;

                // 应用击退力
                meleeRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

                // 造成伤害
                meleeHealth.TakeDamage(damage);
            }
        }
    }

    IEnumerator DelayedDamageAndKnockbackArea(GameObject target, string targetType)
    {
        // 等待攻击延迟时间，确保动画播放到指定位置
        yield return new WaitForSeconds(attackDelay);

        if (monsterAudio != null)
        {
            monsterAudio.PlayAttackSound();
        }

        Debug.Log($"DelayedDamageAndKnockbackArea called for targetType: {targetType}");

        // 根据目标类型处理逻辑
        if (targetType == "Defender")
        {
            DefenderHealth defenderHealth = target.GetComponent<DefenderHealth>();
            Rigidbody2D defenderRb = target.GetComponent<Rigidbody2D>();

            if (defenderHealth != null && defenderRb != null)
            {
                // 计算击退方向
                Vector2 knockbackDirection = (target.transform.position - transform.position).normalized;

                // 应用击退力
                defenderRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

                // 造成伤害
                defenderHealth.TakeDamage(damage);
            }
        }
        else if (targetType == "Melee")
        {
            MeleeHealth meleeHealth = target.GetComponent<MeleeHealth>();
            Rigidbody2D meleeRb = target.GetComponent<Rigidbody2D>();

            if (meleeHealth != null && meleeRb != null)
            {
                // 计算击退方向
                Vector2 knockbackDirection = (target.transform.position - transform.position).normalized;

                // 应用击退力
                meleeRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

                // 造成伤害
                meleeHealth.TakeDamage(damage);
            }
        }
        else if (targetType == "Tower")
        {
            TowerHealth towerHealth = target.GetComponent<TowerHealth>();

            if (towerHealth != null)
            {
                Debug.Log($"TowerHealth found. Current health: {towerHealth.health}");

                // 造成伤害
                towerHealth.health -= damage;

                Debug.Log($"TowerHealth updated. New health: {towerHealth.health}");
            }
            else
            {
                Debug.LogWarning("TowerHealth component not found on target!");
            }
        }
    }
}
