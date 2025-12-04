using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public List<GameObject> EnemySpawnerWave;
    public List<GameObject> EnemySpawnerRegular;
    public List<GameObject> Cave;

    void Start()
    {
        // 从存储中获取玩家选择的难度级别, 默认难度1
        int selectedDifficultyLevel = PlayerPrefs.GetInt("SelectedDifficultyLevel", 1);

        // 遍历所有难度级别的生成器
        for (int i = 0; i < EnemySpawnerWave.Count; i++)
        {
            if (i == selectedDifficultyLevel - 1)
            {
                // 激活对应难度级别的生成器
                EnemySpawnerWave[i].SetActive(true);
                EnemySpawnerRegular[i].SetActive(true);
                Cave[i].SetActive(true);
            }
            else
            {
                // 禁用其他难度级别的生成器
                EnemySpawnerWave[i].SetActive(false);
                EnemySpawnerRegular[i].SetActive(false);
                Cave[i].SetActive(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
