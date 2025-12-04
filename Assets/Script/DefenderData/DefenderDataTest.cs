using System.Collections.Generic;
using UnityEngine;

public class DefenderDataTest : MonoBehaviour
{
    private DefenderDataManager dataManager;

    private void Start()
    {
        dataManager = FindObjectOfType<DefenderDataManager>();

        // 创建测试数据
        List<DefenderData> testDefenders = new List<DefenderData>
        {
            new DefenderData
            {
                name = "Harden",
                baseAttackPower = 3,
                baseHealth = 25,
                level = 1,
                maxLevel = 20,
                attackPower = 3,
                health = 25,

            },
            new DefenderData
            {
                name = "SwordMan",
                baseAttackPower = 12,
                baseHealth = 15,
                level = 1,
                maxLevel = 20,
                attackPower = 12,
                health = 15,

            },
            new DefenderData
            {
                name = "StickMan",
                baseAttackPower = 10,
                baseHealth = 16,
                level = 1,
                maxLevel = 20,
                attackPower = 10,
                health = 16,

            },
            new DefenderData
            {
                name = "ArcherMan",
                baseAttackPower = 5,
                baseHealth = 5,
                level = 1,
                maxLevel = 20,
                attackPower = 5,
                health = 5,

            },
            new DefenderData
            {
                name = "Brienne",
                baseAttackPower = 25,
                baseHealth = 90,
                level = 1,
                maxLevel = 20,
                attackPower = 25,
                health = 90,

            },
            new DefenderData
            {
                name = "MageGandodo",
                baseAttackPower = 20,
                baseHealth = 8,
                level = 1,
                maxLevel = 20,
                attackPower = 20,
                health = 9,

            },
            new DefenderData
            {
                name = "Rider",
                baseAttackPower = 18,
                baseHealth = 30,
                level = 1,
                maxLevel = 20,
                attackPower = 18,
                health = 30,

            },
            new DefenderData
            {
                name = "SpartaMan",
                baseAttackPower = 15,
                baseHealth = 22,
                level = 1,
                maxLevel = 20,
                attackPower = 15,
                health = 22,

            },
            new DefenderData
                {
                    name = "Jackie",
                    baseAttackPower = 1,
                    baseHealth = 18,
                    level = 1,
                    maxLevel = 20,
                    attackPower = 15,
                    health = 22,

                },
            new DefenderData
            {
                name = "Tower",
                baseAttackPower = 10,
                baseHealth = 50,
                level = 1,
                maxLevel = 20,
                attackPower = 10,
                health = 50,

            },
            new DefenderData
            {
                name = "Meat",
                baseAttackPower = 0,
                baseHealth = 0,
                level = 1,
                maxLevel = 20,
                attackPower = 0,
                health = 0.5f,

            }
        };

        // 保存到 JSON 文件
        //dataManager.SaveDefenderData(testDefenders);
    }
}
