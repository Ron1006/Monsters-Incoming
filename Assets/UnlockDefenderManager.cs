using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UnlockDefenderManager : MonoBehaviour
{
    private static UnlockDefenderManager instance;
    public static UnlockDefenderManager Instance => instance;

    private string saveFilePath;

    private SaveData saveData = new SaveData();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            saveFilePath = Path.Combine(Application.persistentDataPath, "defenderUnlockData.json");
            LoadData();
        }
    }

    // **检查 Defender 是否已解锁**
    public bool IsDefenderUnlocked(string defenderName)
    {
        return saveData.unlockedDefenders.Contains(defenderName);
    }

    // **解锁 Defender**
    public void UnlockDefender(string defenderName)
    {
        if (!saveData.unlockedDefenders.Contains(defenderName))
        {
            saveData.unlockedDefenders.Add(defenderName);
            SaveDataToFile();
        }
    }

    // **添加待解锁 Defender**
    public void AddPendingDefender(string defenderName)
    {
        if (!saveData.pendingDefenders.Contains(defenderName))
        {
            saveData.pendingDefenders.Add(defenderName);
        }
        SaveDataToFile();
    }

    // **获取待解锁 Defender**
    public List<string> GetPendingDefenders()
    {
        return new List<string>(saveData.pendingDefenders);
    }

    // **清除待解锁 Defender**
    public void RemovePendingDefender(string defenderName)
    {
        if (saveData.pendingDefenders.Contains(defenderName))
        {
            saveData.pendingDefenders.Remove(defenderName);
            SaveDataToFile();
        }
    }

    // **清除所有待解锁 Defender**
    public void ClearPendingDefenders()
    {
        saveData.pendingDefenders.Clear();
        SaveDataToFile();
    }

    private void SaveDataToFile()
    {
        string json = JsonUtility.ToJson(saveData);
        File.WriteAllText(saveFilePath, json);
    }

    private void LoadData()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            saveData = JsonUtility.FromJson<SaveData>(json);
        }
    }

}

// **存储结构**
[System.Serializable]
public class SaveData
{
    public List<string> unlockedDefenders = new List<string>();
    public List<string> pendingDefenders = new List<string>();
    //public string pendingDefender = "";  // 新增字段，存储待解锁 Defender
}
