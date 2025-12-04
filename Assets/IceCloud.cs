using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceCloud : MonoBehaviour
{
    public GameObject iceShardPrefab; // 小冰块
    public float duration = 10f;
    public float spawnInterval = 1f; // 生成冰块的间隔
    public float spawnRadius = 2f; // 冰块降落的半径范围
    private float timer;

    public int damage = 5; // 从 Archer 传进来的攻击力



    // Start is called before the first frame update
    void Start()
    {
        timer = duration;
        StartCoroutine(SpawnIceShards());
    }

    IEnumerator SpawnIceShards()
    {
        while (timer > 0)
        {
            SpawnIceShard();
            yield return new WaitForSeconds(spawnInterval);
            timer -= spawnInterval;
        }

        Destroy(gameObject); // 乌云持续结束后销毁
    }

    void SpawnIceShard()
    {
        Vector2 randomOffset = Random.insideUnitCircle * spawnRadius; 
        // 在 二维单位圆（半径为1）内 随机生成一个点，然后乘上你设定的 spawnRadius，来控制生成范围。
        Vector3 spawnPosition = transform.position + new Vector3(randomOffset.x, 0, 0);
        // 计算小冰块实际的生成位置。 transform.position 是乌云当前的位置（世界坐标）, randomOffset.x 是随机的水平偏移量, 我们只取 x 偏移量，y 保持乌云高度，

        GameObject shard = Instantiate(iceShardPrefab, spawnPosition, Quaternion.identity);
        // 可以给 shard 添加一个初速度往下坠落，或者由它自己处理

        // 将攻击力传递给 IceShard
        IceShard shardScript = shard.GetComponent<IceShard>();
        if (shardScript != null)
        {
            shardScript.SetDamage(damage);

            // 设置方向为右下 30 度角
            float angleInDegrees = 60f;
            float radians = angleInDegrees * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(radians), -Mathf.Sin(radians), 0f);

            shardScript.SetDirection(dir);
        }
    }
}
