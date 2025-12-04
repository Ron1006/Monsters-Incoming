using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadData : MonoBehaviour
{
    //private bool allDataLoaded = false; // 标记数据是否已加载

    //// Start is called before the first frame update
    //void Awake()
    //{
    //    LoadAllDefendersData(); // 在战斗场景初始化时加载数据
    //}

    //// Update is called once per frame
    //void Update()
    //{
    //    // 仅当数据未加载时才加载数据
    //    if (!allDataLoaded)
    //    {
    //        LoadAllDefendersData();
    //    }
    //}

    //void LoadAllDefendersData()
    //{
    //    // 获取当前场景中所有的 Defender 对象
    //    Defender[] allDefenders = FindObjectsOfType<Defender>();

    //    Debug.Log("Number of defenders found: " + allDefenders.Length); // 检查找到的 Defender 数量

    //    foreach (var defender in allDefenders)
    //    {
    //        string defenderKey = defender.name;

    //        // 从 PlayerPrefs 中加载数据
    //        defender.level = PlayerPrefs.GetInt(defenderKey + "Level", defender.level);
    //        defender.attackPower = PlayerPrefs.GetInt(defenderKey + "AttackPower", defender.attackPower);
    //        defender.health = PlayerPrefs.GetInt(defenderKey + "Health", defender.health);
    //        defender.upgradeCost = PlayerPrefs.GetInt(defenderKey + "UpgradeCost", defender.upgradeCost);

    //        // 输出加载的每个防御者的详细信息
    //        Debug.Log("Loaded data for defender: " + defenderKey +
    //                  ", Level: " + defender.level +
    //                  ", AttackPower: " + defender.attackPower +
    //                  ", Health: " + defender.health +
    //                  ", UpgradeCost: " + defender.upgradeCost);
    //    }

    //    // 标记所有数据已加载
    //    allDataLoaded = true;
    //    Debug.Log("All defender data loaded successfully.");
    //}

}
