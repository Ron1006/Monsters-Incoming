using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class DefenderBaseData
{
    public string name;
    public int baseAttackPower;
    public float baseHealth;
    public int maxLevel;
    public float attackPowerPerLevel;
    public float healthPerLevel;
}

[CreateAssetMenu(fileName = "DefenderDatabase", menuName = "Defender/Database")]
public class DefenderDatabase : ScriptableObject
{
    public List<DefenderBaseData> defenders;
}
