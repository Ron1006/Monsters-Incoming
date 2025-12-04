using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentScrollView : MonoBehaviour
{
    public static EquipmentScrollView Instance { get; private set; }

    public Transform contentParent; // **滚动视图的 Content 容器**a
    public GameObject itemPrefab;  // **装备 UI 预制体**
    private Defender selectedDefender; // **当前选中的 Defender**
    private EquipmentType currentFilterType; // **当前筛选的装备类型**

    public GameObject noEquipmentText;
    public GameObject tapIconText;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }

    // **调用此方法，传入要查看的 Defender**
    public void PopulateList(EquipmentType filterType, Defender defender)
    {
        if (defender == null)
        {
            Debug.LogError("[ERROR] Defender 为空，无法加载装备！");
            return;
        }

        selectedDefender = defender;
        currentFilterType = filterType;

        //Debug.Log($"加载 {defender.defenderName} 的 {filterType} 装备列表");

        // **清空旧装备 UI**
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 关闭“无装备”提示（先默认隐藏）
        noEquipmentText.SetActive(false);

        // **获取玩家所有该类型装备**
        List<Equipment> playerEquipments = PlayerEquipmentManager.Instance.GetEquipmentsByType(filterType);

        // **排序逻辑**
        playerEquipments.Sort((a, b) =>
        {
            bool aEquipped = PlayerEquipmentManager.Instance.IsItemEquipped(a);
            bool bEquipped = PlayerEquipmentManager.Instance.IsItemEquipped(b);

            if (aEquipped && !bEquipped) return -1; // **已装备的放前面**
            if (!aEquipped && bEquipped) return 1;  // **未装备的放后面**

            return b.level.CompareTo(a.level); // **按 Level 降序排序**
        });

        Debug.Log($"[DEBUG] 获取 {filterType} 类型的装备，总数: {playerEquipments.Count}");

        //foreach (var eq in playerEquipments)
        //{
            //Debug.Log($"[DEBUG] 装备: {eq.equipmentName}, 类型: {eq.type}, 已装备: {eq.isEquipped}, 归属 Defender: {eq.equippedByDefender}");
        //}

        if (playerEquipments == null || playerEquipments.Count == 0)
        {
            noEquipmentText.SetActive(true);
            Debug.LogWarning($"没有找到 {filterType} 类型的装备！");
            return;
        }

        // **遍历玩家拥有的装备**
        foreach (Equipment equipment in playerEquipments)
        {
            // **获取当前装备的所属 Defender**
            string equippedBy = PlayerEquipmentManager.Instance.GetEquippedDefenderName(equipment);

            // **条件1：未被任何 Defender 装备**
            bool isUnEquipped = string.IsNullOrEmpty(equippedBy);

            // **条件2：当前 Defender 已装备该装备**
            bool isEquippedByCurrentDefender = equippedBy == defender.defenderName;

            // **只显示未被装备或属于当前 Defender 的装备**
            if (isUnEquipped || isEquippedByCurrentDefender)
            {
                GameObject newItem = Instantiate(itemPrefab, contentParent);
                EquipmentItemUI itemUI = newItem.GetComponent<EquipmentItemUI>();
                itemUI.Setup(equipment, defender);
            }
        }
    }

    // **刷新所有装备 UI，当装备发生变化时调用**
    public void RefreshAllEquipmentUI()
    {
        if (selectedDefender == null)
        {
            Debug.LogWarning("[EQUIPMENT] 没有选中的 Defender，无法刷新装备 UI！");
            return;
        }

        //Debug.Log("[EQUIPMENT] 正在刷新装备 UI...");
        PopulateList(currentFilterType, selectedDefender);
    }

    public void ClearEquipmentList()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        //Debug.Log("[EQUIPMENT] 清空装备列表");
    }

    // 窗口关闭隐藏no equipment文字
    void OnDisable()
    {
        noEquipmentText.SetActive(false);
        tapIconText.SetActive(true);
    }
}
