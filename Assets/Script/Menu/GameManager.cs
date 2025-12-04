using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public List<Defender> defenders;

    private void Start()
    {
        Time.timeScale = 1f; // 确保恢复正常时间流动
        DefenderDataManager.Instance.InitializeDefenders(defenders);


    }

    private void OnApplicationQuit()
    {
        if (DefenderDataManager.Instance != null)
        {
            DefenderDataManager.Instance.SaveAllData();
        }
        else
        {
            Debug.LogWarning("DefenderDataManager.Instance 在退出时为 null，数据未保存！");
        }
    }

    private void OnDestroy()
    {
        if (DefenderDataManager.Instance != null)
        {
            DefenderDataManager.Instance.SaveAllData();
        }
        else
        {
            Debug.LogWarning("DefenderDataManager.Instance 在销毁时为 null，数据未保存！");
        }
    }
}
