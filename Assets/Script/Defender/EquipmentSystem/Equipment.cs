using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// **装备类型枚举**
public enum EquipmentType
{
    Helmet,
    Ring,
    Weapon,
    Armor,
    Boots,
    Necklace
}

// **装备类**
[System.Serializable]
public class Equipment
{
    //changed start
    public int uid; // 装备Unique ID,作为装备的唯一标志
    //changed end
    public string equipmentName; // 装备名称
    public EquipmentType type; // 装备类型（头盔/戒指等）
    public int flatAttackBonus;    // 固定攻击力加成
    public float percentAttackBonus; // **百分比攻击力加成（0.02 = 2%）**
    public int flatHealthBonus;    // 固定生命值加成
    public float percentHealthBonus; // **百分比生命值加成**
    public int value; // 装备的金币价值
    public bool isEquipped; // 是否已被使用
    public Defender assignedDefender; // 被装备的 Defender（如果有）
    public Sprite equipmentSprite; // **装备的图片**
    public int level; // 装备等级，用来排序
    public string equippedByDefender = "";  // 记录当前装备的归属 Defender

    public Equipment(int uid, string name, EquipmentType type, int flatAttack, float percentAttack, int flatHealth, float percentHealth, int value, Sprite sprite, int level)
    {
        this.uid = uid;
        this.equipmentName = name;
        this.type = type;
        this.flatAttackBonus = flatAttack;
        this.percentAttackBonus = percentAttack;
        this.flatHealthBonus = flatHealth;
        this.percentHealthBonus = percentHealth;
        this.value = value;
        this.isEquipped = false;
        this.assignedDefender = null;
        this.equipmentSprite = sprite;
        this.level = level;
        this.equippedByDefender = "";  // 初始化为空
    }

    public Equipment(Equipment other)
    {
        this.uid = other.uid;
        this.equipmentName = other.equipmentName;
        this.type = other.type;
        this.flatAttackBonus = other.flatAttackBonus;
        this.percentAttackBonus = other.percentAttackBonus;
        this.flatHealthBonus = other.flatHealthBonus;
        this.percentHealthBonus = other.percentHealthBonus;
        this.value = other.value;
        this.isEquipped = other.isEquipped;
        this.assignedDefender = other.assignedDefender;
        this.equipmentSprite = other.equipmentSprite;
        this.level = other.level;
        this.equippedByDefender = other.equippedByDefender;
    }
}
