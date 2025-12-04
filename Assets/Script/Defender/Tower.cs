using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : Defender
{
    private InventoryManager inventoryManager; // 引用coin组件来管理coin用来升级
    //public GameObject[] weapons; // An array to hold references to Weapon_lv1, Weapon_lv2, etc.

    // 从数据初始化 Defender
    public override void Initialize(DefenderBaseData baseData, int savedLevel)
    {
        base.Initialize(baseData, savedLevel);  // 调用基类的初始化方法
        ActivateWeaponBasedOnLevel();           // 根据等级激活武器
    }


    // 自定义升级逻辑
    public override bool LevelUp()
    {
        inventoryManager = FindObjectOfType<InventoryManager>(); // 获取金币管理器

        if (level < maxLevel)
        {
            if (inventoryManager != null && inventoryManager.GetCurrencyAmount("Coin") >= UpgradeCost)
            {
                inventoryManager.AddItem(-UpgradeCost, "Coin"); // 扣除升级所需的金币
                level++;                               // 等级提升
                RecalculateState();                    // 重新计算属性
                ActivateWeaponBasedOnLevel();          // 根据新等级激活武器
                DefenderDataManager.Instance.SaveDefenderLevel(defenderName, level); // 保存等级数据

                Debug.Log($"{gameObject.name} 升级到 {level} 级！攻击力：{attackPower} 血量：{health}");
                return true;
            }
            else
            {
                Debug.LogWarning("Not enough coins.");
                return false;
            }
        }
        else
        {
            Debug.Log(gameObject.name + " 已达到最大等级！");
            return false;
        }
    }


    private void ActivateWeaponBasedOnLevel()
    {
        int weaponIndex = Mathf.Clamp((level - 1) / 4, 0, weapons.Length - 1);

        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].SetActive(i == weaponIndex);
        }
    }

}
