using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static EquipmentManager;

public class PlayerEquipmentManager : MonoBehaviour
{
    public static PlayerEquipmentManager Instance { get; private set; }

    private string saveFilePath;
    public List<Equipment> ownedEquipments = new List<Equipment>(); // 玩家已抽取的装备
    //changed start
    public Dictionary<int, Equipment> equippedItems = new Dictionary<int, Equipment>(); // 已装备的装备 
    private Dictionary<int, string> equippedByDefender = new Dictionary<int, string>(); // **装备归属 Defender**
    //changed end

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            saveFilePath = Path.Combine(Application.persistentDataPath, "player_equipment.json");
            LoadPlayerEquipments(); // **加载玩家装备数据**

            
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // **添加装备到玩家存储**
    public void AddEquipment(Equipment newEquipment)
    {
        // ✅ 确保 UI 只在数据加载后刷新
        EquipmentScrollView.Instance.RefreshAllEquipmentUI();
        ownedEquipments.Add(newEquipment);
        SavePlayerEquipments(); // **保存数据**
        Debug.Log($"玩家获得装备并保存: {newEquipment.equipmentName}");
    }

    // **获取某个类型的装备**
    public List<Equipment> GetEquipmentsByType(EquipmentType type)
    {
        //Debug.Log($"[DEBUG] 查找类型 {type} 的装备...");

        List<Equipment> filtered = new List<Equipment>();

        foreach (var eq in ownedEquipments)
        {
            // ✅ 这里添加类型检查，确保只获取指定类型的装备
            if (eq.type != type)
            {
                continue;  // 跳过不符合类型的装备
            }

            string equippedBy = equippedByDefender.ContainsKey(eq.uid) ? equippedByDefender[eq.uid] : "";
            Debug.Log($"[DEBUG] 过滤装备: {eq.equipmentName}, 类型: {eq.type}, 归属 Defender: {equippedBy}");

            bool isEquippedByCurrentDefender = equippedBy == EquipmentButtonManager.Instance.currentDefender.defenderName;
            bool isUnEquipped = string.IsNullOrEmpty(equippedBy);

            if (isEquippedByCurrentDefender || isUnEquipped)
            {
                filtered.Add(eq);
            }
        }

        //Debug.Log($"[DEBUG] 在 ownedEquipments 中查找 {type}，找到 {filtered.Count} 个装备");
        return filtered;
    }

    // **检查某个装备是否已装备**
    public bool IsItemEquipped(Equipment equipment)
    {
        if (equipment == null)
        {
            Debug.LogWarning("[WARNING] 传入的装备为 NULL！");
            return false;
        }

        return equippedItems.ContainsKey(equipment.uid);
        //return equippedItems.TryGetValue(equipment.type, out Equipment equipped) && equipped == equipment;
    }

    // **获取当前装备的物品**
    public Equipment GetEquippedItem(int uid)
    {
        return equippedItems.ContainsKey(uid) ? equippedItems[uid] : null;
    }

    
    // **装备物品，确保装备归属于特定 Defender**
    public void EquipItemForDefender(Defender defender, Equipment equipment)
    {
        //if (equippedItems.ContainsKey(equipment.type))
        //{
        //    // **先卸下当前 Defender 这个装备类型的旧装备**
        //    UnequipItemFromDefender(defender, equipment.type);
        //}

        // ✅ **确保只装备一个同类型装备**
        // Equipment itemToEquip = ownedEquipments.FirstOrDefault(eq =>
        //     eq.equipmentName == equipment.equipmentName &&
        //     !eq.isEquipped);
        //
        //
        // if (itemToEquip == null)
        // {
        //     Debug.LogError($"[EQUIPMENT] 没有找到可装备的 {equipment.equipmentName}！");
        //     return;
        // }
        Equipment itemToEquip = equipment;

        // ✅ **装备该物品**
        equippedItems[equipment.uid] = itemToEquip;
        equippedByDefender[equipment.uid] = defender.defenderName;
        itemToEquip.isEquipped = true; // **标记为已装备**
        defender.EquipItem(itemToEquip);

        SavePlayerEquipments();

        DefenderDataManager.Instance.SaveDefenderEquipment(defender.defenderName, defender.equippedItems); // ✅ 额外存储 Defender 装备信息

        // ✅ **更新 UI**
        EquipmentButtonManager.Instance.UpdateEquipmentButtonIcon(itemToEquip.type, itemToEquip.equipmentSprite);
        EquipmentScrollView.Instance.RefreshAllEquipmentUI();
    }

    // **卸下装备，确保从特定 Defender 卸下**
    public void UnequipItemFromDefender(Defender defender, int uid)
    {
        if (equippedItems.ContainsKey(uid))
        {
            Equipment removedEquipment = equippedItems[uid];
            removedEquipment.isEquipped = false; // ✅ 取消装备状态
            equippedItems.Remove(uid);

            // ✅ **仅当装备属于当前 Defender 时才清除绑定**
            if (equippedByDefender.ContainsKey(uid) && equippedByDefender[uid] == defender.defenderName)
            {
                equippedByDefender.Remove(uid);
            }

            // **更新 Defender 的装备列表**
            if (defender.equippedItems.ContainsKey(removedEquipment.type))
            {
                defender.equippedItems.Remove(removedEquipment.type);
            }

            DefenderDataManager.Instance.SaveDefenderEquipment(defender.defenderName, defender.equippedItems); // ✅ 确保 Defender 数据更新

            SavePlayerEquipments();

            // ✅ **更新 UI**
            EquipmentButtonManager.Instance.ResetEquipmentButtonIcon(removedEquipment.type);
            EquipmentScrollView.Instance.RefreshAllEquipmentUI();
        }
    }

    // **获取当前装备所属的 Defender**
    public string GetEquippedDefenderName(Equipment equipment)
    {
        if (equippedByDefender.ContainsKey(equipment.uid))
        {
            return equippedByDefender[equipment.uid]; // 返回装备归属的 Defender 名称
        }
        return null; // **未被装备**
    }


    // 出售装备
    public void SellEquipment(Equipment equipment)
    {
        // **获取当前装备属于哪个 Defender**
        string defenderName = GetEquippedDefenderName(equipment);
        if (!string.IsNullOrEmpty(defenderName))
        {
            Defender defender = FindDefenderByName(defenderName);
            if (defender != null)
            {
                // **如果装备被装备，先卸下**
                UnequipItemFromDefender(defender, equipment.uid);
            }
        }

        ownedEquipments.Remove(equipment);
        SavePlayerEquipments();

        // **增加金币**
        InventoryManager.Instance.AddItem(equipment.value, "Coin");
        Debug.Log($"已出售 {equipment.equipmentName}，获得 {equipment.value} 金币");

        // **刷新装备 UI**
        EquipmentScrollView.Instance.RefreshAllEquipmentUI();
    }

    // **保存玩家装备数据**
    private void SavePlayerEquipments()
    {
        List<EquipmentData> dataToSave = new List<EquipmentData>();
        foreach (var equipment in ownedEquipments)
        {
            equippedByDefender.TryGetValue(equipment.uid, out string defenderName);

            // ✅ **如果装备未被装备，清空 equippedByDefender**
            if (!equippedItems.ContainsValue(equipment))
            {
                defenderName = "";
            }

            dataToSave.Add(new EquipmentData()
            {
                uid = equipment.uid,
                equipmentName = equipment.equipmentName,
                type = equipment.type.ToString(),
                flatAttackBonus = equipment.flatAttackBonus,
                percentAttackBonus = equipment.percentAttackBonus,
                flatHealthBonus = equipment.flatHealthBonus,
                percentHealthBonus = equipment.percentHealthBonus,
                value = equipment.value,
                spriteName = equipment.equipmentSprite.name,
                level = equipment.level,
                isEquipped = equippedItems.ContainsValue(equipment),
                equippedByDefender = defenderName
            });
        }

        string json = JsonUtility.ToJson(new EquipmentDataList { equipments = dataToSave }, true);
        File.WriteAllText(saveFilePath, json);
        //Debug.Log($"玩家装备数据已保存到: {saveFilePath}");
    }

    // **加载玩家装备数据**
    private void LoadPlayerEquipments()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.Log("没有找到玩家装备数据文件，创建新数据！");
            return;
        }

        string json = File.ReadAllText(saveFilePath);
        EquipmentDataList dataList = JsonUtility.FromJson<EquipmentDataList>(json);

        ownedEquipments.Clear();
        equippedItems.Clear();  // **确保清空已装备列表，避免旧数据影响**
        equippedByDefender.Clear();

        //Debug.Log($"[DEBUG] 读取装备数据 JSON, 共有 {dataList.equipments.Count} 件装备");

        int equipmentUid = 0;

        foreach (var data in dataList.equipments)
        {
            // **检查 JSON 数据是否正确**
            //Debug.Log($"[DEBUG] 解析 JSON 装备: {data.equipmentName}, 类型: {data.type}, 归属: {data.equippedByDefender}");

            // **确保类型转换不会出错**
            if (!System.Enum.TryParse(data.type, true, out EquipmentType type))
            {
                Debug.LogError($"[ERROR] 无法解析装备类型: {data.type}，跳过装备 {data.equipmentName}");
                continue;
            }


            Sprite sprite = Resources.Load<Sprite>("EquipmentSprites/" + data.spriteName);

            Equipment newEquipment = new Equipment(
                data.uid,
            data.equipmentName, type,
            data.flatAttackBonus, data.percentAttackBonus,
            data.flatHealthBonus, data.percentHealthBonus,
            data.value, sprite, data.level
            );

            equipmentUid = Mathf.Max(equipmentUid, data.uid);

            newEquipment.isEquipped = data.isEquipped;
            newEquipment.equippedByDefender = data.equippedByDefender;
            // ✅ **无论是否被 Defender 装备，都要加入 ownedEquipments**
            ownedEquipments.Add(newEquipment);

            // ✅ **修正：如果 equippedByDefender 为空，不存入**
            if (!string.IsNullOrEmpty(data.equippedByDefender))
            {
                equippedByDefender[newEquipment.uid] = data.equippedByDefender;

                // ✅ **确保 equippedItems 也更新**
                if (newEquipment.isEquipped)
                {
                    equippedItems[newEquipment.uid] = newEquipment;
                }
            }
        }
        EquipmentManager.Instance.InitEquipmentUid(equipmentUid + 1);


        Debug.Log($"[DEBUG] 加载完成，ownedEquipments 里有 {ownedEquipments.Count} 件装备");
    }

    // **用于查找 Defender**
    private Defender FindDefenderByName(string defenderName)
    {
        return FindObjectsOfType<Defender>().FirstOrDefault(d => d.defenderName == defenderName);
    }

    // // **检查当前是否装备了某个类型的装备**
    // public bool HasEquipment(EquipmentType type)
    // {
    //     return equippedItems.ContainsKey(type);
    // }





    // **JSON 数据结构**
    [System.Serializable]
    public class EquipmentData
    {
        public int uid;
        public string equipmentName;
        public string type;
        public int flatAttackBonus;
        public float percentAttackBonus;
        public int flatHealthBonus;
        public float percentHealthBonus;
        public int value;
        public string spriteName;
        public int level;
        public bool isEquipped; // **新增字段，标记是否已装备**
        public string equippedByDefender; // **新增字段，记录当前装备的 Defender**
    }

    [System.Serializable]
    public class EquipmentDataList
    {
        public List<EquipmentData> equipments;
    }
}
