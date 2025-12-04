using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DefenderDataManager : MonoBehaviour
{
    public static DefenderDataManager Instance { get; private set; } // 单例实例，确保全局唯一

    private Dictionary<string, DefenderSaveData> defenderData = new Dictionary<string, DefenderSaveData>();
    private string saveFilePath;




    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 确保只有一个实例
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // 使对象在场景切换时不被销毁

        saveFilePath = Path.Combine(Application.persistentDataPath, "defenderData.json");

        LoadData();


    }

    // **获取 Defender 等级**
    public int GetDefenderLevel(string defenderName)
    {
        if (defenderData.ContainsKey(defenderName))
        {
            return defenderData[defenderName].level;
        }
        else
        {
            //Debug.LogWarning($"[WARNING] Defender {defenderName} not found. Returning default level 1.");
            return 1;
        }
    }

    // ** 保存指定 Defender 的等级**
    public void SaveDefenderLevel(string defenderName, int level)
    {
        if (string.IsNullOrEmpty(defenderName))
        {
            Debug.LogError("[ERROR] defenderName is empty. 拒绝保存！");
            return;
        }

        if (!defenderData.ContainsKey(defenderName))
        {
            defenderData[defenderName] = new DefenderSaveData();
        }
        defenderData[defenderName].level = level;
        defenderData[defenderName].defenderName = defenderName; // 一定要手动设置这个值
        Debug.Log($"[SAVE] 保存 Defender: name = '{defenderName}', level = {level}");
        SaveToJson();
    }

    // **保存 Defender 的装备**
    public void SaveDefenderEquipment(string defenderName, Dictionary<EquipmentType, Equipment> equippedItems)
    {
        if (!defenderData.ContainsKey(defenderName))
        {
            defenderData[defenderName] = new DefenderSaveData();
        }

        // ✅ **确保当 Defender 没有装备时，存储空列表**
        if (equippedItems == null || equippedItems.Count == 0)
        {
            Debug.Log($"[INFO] Defender {defenderName} 没有装备，清空数据.");
            defenderData[defenderName].equippedItems = new List<EquipmentData>(); // 存空列表，防止 NullException
            SaveToJson();
            return;
        }

        List<EquipmentData> equipmentDataList = new List<EquipmentData>();
        foreach (var kvp in equippedItems)
        {
            Equipment eq = kvp.Value;
            if (eq == null) continue; // **防止 Null 进入数据**

            equipmentDataList.Add(new EquipmentData
            {
                uid = eq.uid,
                equipmentName = eq.equipmentName,
                type = eq.type.ToString(),
                flatAttackBonus = eq.flatAttackBonus,
                percentAttackBonus = eq.percentAttackBonus,
                flatHealthBonus = eq.flatHealthBonus,
                percentHealthBonus = eq.percentHealthBonus,
                value = eq.value,
                spriteName = eq.equipmentSprite.name
            });
        }

        defenderData[defenderName].equippedItems = equipmentDataList;
        SaveToJson();
    }

    // **加载 Defender 数据**
    public void LoadData()
    {
        //Debug.Log("尝试加载 Defender 数据，路径：" + saveFilePath);

        if (!File.Exists(saveFilePath))
        {
            Debug.LogError("Defender JSON 数据文件不存在，使用默认数据！");
            return;
        }
        try
        {
            string json = File.ReadAllText(saveFilePath);
            Debug.Log($"[DEBUG] 读取的 JSON 数据: {json}");
            DefenderSaveDataList dataList = JsonUtility.FromJson<DefenderSaveDataList>(json);

            defenderData.Clear();
            foreach (var data in dataList.defenders)
            {
                //Debug.Log($"[DEBUG] 加载 Defender: {data.defenderName}, 级别: {data.level}, 已装备数量: {data.equippedItems.Count}");
                defenderData[data.defenderName] = data;
            }

            Debug.Log("所有 Defender 数据已成功从 JSON 加载！");
        }
        catch (Exception e)
        {
            Debug.LogError("读取 Defender JSON 失败：" + e.Message);
        }
    }

    // **保存数据到 JSON**
    private void SaveToJson()
    {
        List<DefenderSaveData> defenderList = new List<DefenderSaveData>(defenderData.Values);
        string json = JsonUtility.ToJson(new DefenderSaveDataList { defenders = defenderList }, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("所有 Defender 数据已成功保存到 JSON。");
    }

    // **恢复装备状态**
    public void RestoreDefenderEquipment(Defender defender)
    {
        if (defenderData.TryGetValue(defender.defenderName, out DefenderSaveData savedData))
        {
            //Debug.Log($"[DEBUG] 正在恢复 {defender.defenderName} 的装备数据...");
            //Debug.Log($"[DEBUG] {defender.defenderName} 的已装备列表数量: {savedData.equippedItems.Count}");

            if (savedData.equippedItems == null || savedData.equippedItems.Count == 0)
            {
                defender.equippedItems.Clear(); // ✅  确保清空旧装备
                return;
            }

            Dictionary<EquipmentType, Equipment> restoredEquipment = new Dictionary<EquipmentType, Equipment>();

            foreach (var equipmentData in savedData.equippedItems)
            {
                //Debug.Log($"[DEBUG] 正在恢复装备: {equipmentData.equipmentName}, 类型: {equipmentData.type}");

                EquipmentType type = (EquipmentType)System.Enum.Parse(typeof(EquipmentType), equipmentData.type);
                Sprite sprite = Resources.Load<Sprite>("EquipmentSprites/" + equipmentData.spriteName);

                Equipment newEquipment = new Equipment(
                    equipmentData.uid,
                equipmentData.equipmentName, type,
                equipmentData.flatAttackBonus, equipmentData.percentAttackBonus,
                equipmentData.flatHealthBonus, equipmentData.percentHealthBonus,
                equipmentData.value, sprite, 1
            );

                restoredEquipment[type] = newEquipment;
                //Debug.Log($"[EQUIPMENT] {defender.defenderName} 恢复装备 {type}: {equipmentData.equipmentName}");
            }

            defender.equippedItems = restoredEquipment;
            //Debug.Log($"[EQUIPMENT] {defender.defenderName} 恢复装备数量: {defender.equippedItems.Count}");

            defender.RecalculateState();
        }
        else
        {
            //Debug.LogWarning($"[WARNING] 没有找到 {defender.defenderName} 的装备数据！");
        }
    }

    // **初始化 Defender**
    public void InitializeDefenders(List<Defender> defenders)
    {
        foreach (var defender in defenders)
        {
            //Debug.Log($"[DEBUG] 初始化 Defender: {defender.defenderName}");

            int savedLevel = GetDefenderLevel(defender.defenderName);

            // **更新 Defender 的等级**
            defender.level = savedLevel;

            //Debug.Log($"[DEBUG] {defender.defenderName} 级别: {savedLevel}");

            // **恢复装备**
            RestoreDefenderEquipment(defender);

            //Debug.Log($"[DEBUG] {defender.defenderName} 的最终装备数量: {defender.equippedItems.Count}");

            // **重新计算属性**
            defender.RecalculateState();
        }
    }

    // **清空数据**
    public void ClearSavedLevels()
    {
        defenderData.Clear();
        File.Delete(saveFilePath);
        Debug.Log("所有 Defender 记录已清空。");
    }

    public void SaveAllData()
    {
        SaveToJson();
        //Debug.Log("所有 Defender 数据已保存到 JSON。");
    }

    public void ClearPlayerPrefs()
{
    PlayerPrefs.DeleteAll();
    PlayerPrefs.Save();
    Debug.Log("PlayerPrefs 已清理，所有旧 Defender 数据已删除。");
}



    // ** 结构化 JSON 数据**
    [System.Serializable]
    private class DefenderSaveDataList
    {
        public List<DefenderSaveData> defenders;
    }

    [System.Serializable]
    public class DefenderSaveData
    {
        public string defenderName;
        public int level;
        public List<EquipmentData> equippedItems = new List<EquipmentData>();
    }

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
    }
}