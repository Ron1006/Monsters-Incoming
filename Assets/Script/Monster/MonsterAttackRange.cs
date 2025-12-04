using System.Collections;
using UnityEngine;

public class MonsterAttackRange : MonoBehaviour
{
    [Header("Attack Settings")]
    public int damage = 10;
    public float bulletSpawnDelay = 0.5f; // 延迟生成炮弹时间
    public GameObject bulletPrefab; // 远程炮弹的 Prefab
    public Transform firePoint; // 炮弹生成的位置
    public float bulletSpeed = 10f; // 炮弹速度

    [Header("Components")]
    private bool canAttack = true; // 控制是否可以攻击
    private MonsterController monsterController; // 引用 MonsterController，用于触发攻击动画
    private MonsterAudio monsterAudio; // 引用音效组件
    private MonsterRangeStateMachine rangeStateMachine;



    void Start()
    {
        // 获取 MonsterRangeStateMachine
        rangeStateMachine = GetComponent<MonsterRangeStateMachine>();
        if (rangeStateMachine == null)
        {
            Debug.LogError("MonsterRangeStateMachine not found!");
        }

        // 获取 MonsterController
        monsterController = GetComponentInChildren<MonsterController>();
        if (monsterController == null)
        {
            Debug.LogError("MonsterController not found on child objects!");
        }

        // 获取音效组件
        monsterAudio = GetComponent<MonsterAudio>();
    }

    void Update()
    {
    }

    public void TriggerRangedAttack()
    {
        if (canAttack)
        {
            // 触发攻击动画
            if (monsterController != null)
            {
                monsterController.TriggerAttack();
            }

            // 播放攻击音效
            if (monsterAudio != null)
            {
                monsterAudio.PlayWeaponCharging();
            }

            // 启动延迟生成炮弹协程
            StartCoroutine(SpawnBulletWithDelay());

            // 启动攻击冷却协程
            StartCoroutine(AttackCooldown());
        }
    }

    private IEnumerator SpawnBulletWithDelay()
    {
        // 等待炮弹生成延迟时间，确保动画播放完成
        yield return new WaitForSeconds(bulletSpawnDelay);

        if (bulletPrefab != null && firePoint != null)
        {
            // 实例化炮弹
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            // **获取 BulletMonster 组件，并设置 damage**
            BulletMonster bulletScript = bullet.GetComponent<BulletMonster>();
            if (bulletScript != null)
            {
                bulletScript.damage = this.damage; // 继承 MonsterAttackRange 的 damage
            }

            // 设置炮弹的速度和方向
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                bulletRb.velocity = -firePoint.right * bulletSpeed;
            }

            // 播放攻击音效
            if (monsterAudio != null)
            {
                monsterAudio.PlayAttackSound();
            }

        }
        else
        {
            Debug.LogWarning("BulletPrefab or FirePoint is not set!");
        }
    }

    private IEnumerator AttackCooldown()
    {
        canAttack = false; // 暂时禁止攻击
        yield return new WaitForSeconds(rangeStateMachine.coolDown); // 等待冷却时间
        canAttack = true; // 恢复攻击
    }
}
