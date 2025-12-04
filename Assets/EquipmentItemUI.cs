using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentItemUI : MonoBehaviour
{
    public Image equipmentIcon; //显示装备图标
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI valueText;
    public Button equipButton;
    public Button sellButton;
    public Button removeButton;

    private Equipment equipment; // 当前装备信息
    private Defender assignedDefender; // 关联的 Defender


    public void Setup(Equipment newEquipment, Defender defender)
    {
        equipment = newEquipment;
        assignedDefender = defender; // 记录当前装备属于哪个 Defender

        // **获取装备是否已装备，并检查归属 Defender**
        string equippedBy = PlayerEquipmentManager.Instance.GetEquippedDefenderName(equipment);
        bool isEquipped = equippedBy == defender.defenderName; // 只在当前 Defender 这里才算已装备

        // 更新UI
        equipmentIcon.sprite = equipment.equipmentSprite;

        if (equipment.percentHealthBonus > 0)
        {
            healthText.text = $"Health +{equipment.percentHealthBonus * 100}%";
        }
        else if (equipment.flatHealthBonus > 0)
        {
            healthText.text = $"Health +{equipment.flatHealthBonus}";
        }
        else
        {
            healthText.text = "Health + 0"; 
        }

        if (equipment.percentAttackBonus > 0)
        {
            damageText.text = $"Damage +{equipment.percentAttackBonus * 100}%";
        }
        else if (equipment.flatAttackBonus > 0)
        {
            damageText.text = $"Damage +{equipment.flatAttackBonus}";
        }
        else
        {
            damageText.text = "Damage + 0"; // 如果没有 Damage 加成，隐藏
        }

        valueText.text = $"Coin: {equipment.value}";

        // **装备 UI 逻辑**
        removeButton.gameObject.SetActive(isEquipped);
        equipButton.gameObject.SetActive(!isEquipped);
        sellButton.gameObject.SetActive(!isEquipped);

        // 绑定按钮点击事件
        equipButton.onClick.RemoveAllListeners();
        equipButton.onClick.AddListener(() => EquipItem());

        removeButton.onClick.RemoveAllListeners();
        removeButton.onClick.AddListener(() => RemoveItem());

        sellButton.onClick.RemoveAllListeners();
        sellButton.onClick.AddListener(() => SellItem());
    }

    private void EquipItem()
    {
        if (assignedDefender == null)
        {
            Debug.LogError("No Defender assigned to this equipment!");
            return;
        }

        // **获取真正的 Defender**
        Defender actualDefender = assignedDefender;
        if (assignedDefender is DisplayDefender displayDefender)
        {
            actualDefender = displayDefender.defender; // **获取真实 Defender**
            //Debug.Log($"获取到对象defender");
        }

        if (actualDefender == null)
        {
            Debug.LogError($"[ERROR] {assignedDefender.name} 没有正确引用 defender 对象！");
            return;
        }

        if (actualDefender.equippedItems.ContainsKey(equipment.type))
        {
            Equipment previousEquipment = actualDefender.equippedItems[equipment.type];
            if (previousEquipment != null)
            {
                Debug.Log($"替换 {previousEquipment.equipmentName} 为 {equipment.equipmentName}");
                PlayerEquipmentManager.Instance.UnequipItemFromDefender(actualDefender, previousEquipment.uid);
            }
        }

        PlayerEquipmentManager.Instance.EquipItemForDefender(actualDefender, equipment);
        //actualDefender.EquipItem(equipment); // 确保 Defender 也装备
        actualDefender.RecalculateState(); // 重新计算属性

        SoundManager.Instance.PlaySound(SoundManager.Instance.equipSound);

        Debug.Log($"已装备 {equipment.equipmentName} 到 {assignedDefender.defenderName}");

        // 更新 UI
        removeButton.gameObject.SetActive(true);
        equipButton.gameObject.SetActive(false);

        // **更新所有 UI**
        //EquipmentScrollView.Instance.RefreshAllEquipmentUI();
    }

    private void RemoveItem()
    {
        if (assignedDefender == null)
        {
            Debug.LogError("No Defender assigned to this equipment!");
            return;
        }

        // **获取真正的 Defender**
        Defender actualDefender = assignedDefender;
        if (assignedDefender is DisplayDefender displayDefender)
        {
            actualDefender = displayDefender.defender; // **获取真实 Defender**
            Debug.Log($"获取到对象defender");
        }

        if (actualDefender == null)
        {
            Debug.LogError($"[ERROR] {assignedDefender.name} 没有正确引用 defender 对象！");
            return;
        }

        // **使用新的方法卸载装备，并清除归属 Defender**
        PlayerEquipmentManager.Instance.UnequipItemFromDefender(actualDefender, equipment.uid);
        //actualDefender.UnequipItem(equipment.type); //  Defender 也卸下
        actualDefender.RecalculateState(); //  重新计算属性

        SoundManager.Instance.PlaySound(SoundManager.Instance.dropEquipSound);

        Debug.Log($"已卸下 {equipment.equipmentName} 从 {actualDefender.defenderName}");

        EquipmentButtonManager.Instance.ResetEquipmentButtonIcon(equipment.type);
        

        // **更新所有 UI**
        //EquipmentScrollView.Instance.RefreshAllEquipmentUI();

        // 更新 UI
        removeButton.gameObject.SetActive(false);
        equipButton.gameObject.SetActive(true);
    }

    private void SellItem()
    {
        if (assignedDefender == null)
        {
            Debug.LogError("No Defender assigned to this equipment!");
            return;
        }

        // **如果装备正在使用，先卸下**
        if (PlayerEquipmentManager.Instance.IsItemEquipped(equipment))
        {
            RemoveItem();
        }

        PlayerEquipmentManager.Instance.SellEquipment(equipment);

        SoundManager.Instance.PlaySound(SoundManager.Instance.sellEquipSound);

        Debug.Log($"已出售 {equipment.equipmentName} 获得 {equipment.value} 金币");

        // **更新所有 UI**
        //EquipmentScrollView.Instance.RefreshAllEquipmentUI();

        // 销毁该 UI 物体
        Destroy(gameObject);
    }
}
