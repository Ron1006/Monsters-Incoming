using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance {  get; private set; }
    //changed start
    private int _equipmentUid = 1;
    public void InitEquipmentUid (int uid)
    {
        _equipmentUid = uid;
    }
    public int SpawnEquipmentUid()
    {
        return _equipmentUid++;
    }
    //changed end

    public List<Equipment> allEquipments = new List<Equipment> (); // 存储所有装备

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 确保场景切换时不销毁
            LoadEquipmentDatabase(); // 加载默认装备数据
            //Debug.Log($"装备数据库加载完成，共 {allEquipments.Count} 件装备"); // 检查是否正确加载
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // **从 JSON 读取装备数据**
    private void LoadEquipmentDatabase()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("equipment_data");

        if (jsonFile == null)
        {
            Debug.LogError("找不到装备 JSON 文件！");
            return;
        }

        Debug.Log("成功找到装备 JSON 文件，内容如下：\n" + jsonFile.text);

        try
        {
            EquipmentDataList dataList = JsonUtility.FromJson<EquipmentDataList>(jsonFile.text);
            if (dataList == null || dataList.equipments == null)
            {
                Debug.LogError("JSON 解析失败！请检查 JSON 结构");
                return;
            }

            foreach (var data in dataList.equipments)
            {
                EquipmentType type = (EquipmentType)System.Enum.Parse(typeof(EquipmentType), data.type);
                Sprite sprite = LoadSprite(data.spriteName);
                Equipment newEquipment = new Equipment(
                    data.uid,
                    data.equipmentName, type,
                    data.flatAttackBonus, data.percentAttackBonus,
                    data.flatHealthBonus, data.percentHealthBonus,
                    data.value, sprite, 
                    data.level
                );
                allEquipments.Add(newEquipment);
            }

            //Debug.Log($"从 JSON 加载装备，共 {allEquipments.Count} 件装备。");
        }
        catch (System.Exception e)
        {
            Debug.LogError("JSON 解析失败: " + e.Message);
        }
    }

    // **加载装备的 Sprite 图片**
    private Sprite LoadSprite(string spriteName)
    {
        return Resources.Load<Sprite>("EquipmentSprites/" + spriteName);
    }

    // **获取所有未被装备的装备**
    public List<Equipment> GetAvailableEquipments()
    {
        return allEquipments.FindAll(e => !e.isEquipped);
    }

    // **JSON 结构**
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
    }

    [System.Serializable]
    public class EquipmentDataList
    {
        public List<EquipmentData> equipments;
    }
}
