using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnerRegular : MonoBehaviour
{
    public float healthMultiplier = 1f; // 血量倍数
    public float damageMultiplier = 1f; // 攻击力倍数

    public GameObject[] enemyPrefab;
    public float spawnTime = 2;
    private float timer;
    private int currentEnemy;

    

    // Start is called before the first frame update
    void Start()
    {
        timer = spawnTime;
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        // 随机生成 y 轴坐标在 0.5 到 -0.1 之间
        float randomY = Random.Range(-0.7f, -0.1f);

        // 生成时保持 x 轴 z = y，更新 y 轴
        Vector3 spawnPosition = new Vector3(transform.position.x, randomY, randomY);


        GameObject newEnemy = Instantiate(enemyPrefab[currentEnemy], spawnPosition, Quaternion.identity);

        // 修改enemy的血量
        EnemyHealth enemyHealth = newEnemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null )
        {
            enemyHealth.maxHealth = (int)(enemyHealth.maxHealth * healthMultiplier);
            enemyHealth.health = enemyHealth.maxHealth;
            enemyHealth.quantity = (int)(enemyHealth.quantity * healthMultiplier);
        }

        // 2修改敌人的近战攻击力
        MonsterAttack enemyAttack = newEnemy.GetComponent<MonsterAttack>();
        if (enemyAttack != null)
        {
            enemyAttack.damage = (int)(enemyAttack.damage * damageMultiplier);
        }

        // 3️远程敌人特殊处理
        MonsterAttackRange enemyRangeAttack = newEnemy.GetComponent<MonsterAttackRange>();
        if (enemyRangeAttack != null)
        {
            enemyRangeAttack.damage = (int)(enemyRangeAttack.damage * damageMultiplier);
        }

        currentEnemy++;
        if (currentEnemy >= enemyPrefab.Length)
        {
            currentEnemy = 0;
        }
        timer = spawnTime;
    }
}
