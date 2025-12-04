using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreventScreenDimming : MonoBehaviour
{

    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        
    }

    void OnApplicationQuit()
    {
        Screen.sleepTimeout = SleepTimeout.SystemSetting;
    }

    //void LoadAllDefendersData()
    //{

    //    foreach (var defender in defenders) // 假设 allDefenders 是包含所有防御者的列表或数组
    //    {
    //        string defenderKey = defender.name;

    //        defender.level = PlayerPrefs.GetInt(defenderKey + "Level", defender.level);
    //        defender.attackPower = PlayerPrefs.GetInt(defenderKey + "AttackPower", defender.attackPower);
    //        defender.health = PlayerPrefs.GetInt(defenderKey + "Health", defender.health);
    //        defender.upgradeCost = PlayerPrefs.GetInt(defenderKey + "UpgradeCost", defender.upgradeCost);
    //    }

        
    //}
}
