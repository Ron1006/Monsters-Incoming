using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // 引入 TextMeshPro 命名空间

[System.Serializable]  // 使 Wave 类可在 Inspector 中编辑
public class Wave
{
    public GameObject[] enemies; // 本波次要生成的敌人
    public float spawnInterval; // 每个敌人生成间隔
    public float waveDuration; // 本波持续时间
    
}


public class EnemySpawner : MonoBehaviour
{
    public float healthMultiplier = 1f; // 血量倍数
    public float damageMultiplier = 1f; // 攻击力倍数

    public Wave[] waves; //存储所有波次
    private int currentWaveIndex = 0; // 当前波次索引 
    private bool isSpawningWave = false; // 标记当前是否生成波次

    public float startDelay = 2f; // 游戏开始后的延迟时间

    public TextMeshProUGUI waveText; // 显示当前波次
    public GameObject waveBackground;

    

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(StartWaves());
    }

    // 控制波次开始的协程
    IEnumerator StartWaves()
    {
        yield return new WaitForSeconds(startDelay); // 等待游戏开始延迟
        StartCoroutine(SpawnWave());
    }

    // 生成波次的协程
    IEnumerator SpawnWave()
    {
        isSpawningWave = true;
        Wave currentWave = waves[currentWaveIndex]; //获取当前波次

        UpdateWaveText();

        // 遍历当前波次的所有敌人类型
        for (int i = 0; i < currentWave.enemies.Length; i++)
        {
            // 保持 x 轴位置，随机 y 轴坐标在 0.5 到 -0.1 之间
            float randomY = Random.Range(-0.7f, -0.1f);
            Vector3 spawnPosition = new Vector3(transform.position.x, randomY, randomY);

            //生成敌人
            GameObject newEnemy = Instantiate(currentWave.enemies[i], spawnPosition, Quaternion.identity);

            // 修改enemy的血量
            EnemyHealth enemyHealth = newEnemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
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

            // 等待敌人生成间隔
            yield return new WaitForSeconds(currentWave.spawnInterval);
        }

        // 等待波次的持续时间
        yield return new WaitForSeconds(currentWave.waveDuration);

        // 切换到下一波
        currentWaveIndex++;
        if (currentWaveIndex < waves.Length)
        {
            StartCoroutine(SpawnWave());
        }
        else
        {
            Debug.Log("All waves completed");
            isSpawningWave = false;
        }
    }

    //直接生成指定波次, 用于生成boss
    public void SpawnSpecificWave(int waveIndex)
    {
        if (waveIndex < waves.Length)
        {
            StopAllCoroutines(); // 停止之前的协程
            currentWaveIndex = waveIndex;
            StartCoroutine(SpawnWave()); // 生成指定波次
        }
    }

    // 更新波次文本
    void UpdateWaveText()
    {
        waveText.text = "Wave " + (currentWaveIndex + 1); // 在 UI 中显示当前波次 (从1开始)
        waveText.gameObject.SetActive(true);
        waveBackground.gameObject.SetActive(true);

        // 启动协程，10秒后wave文本隐藏
        StartCoroutine(HideWaveTextAfterDelay(10f));
    }

    IEnumerator HideWaveTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        waveText.gameObject.SetActive(false);
        waveBackground.gameObject.SetActive(false);
    }
}
