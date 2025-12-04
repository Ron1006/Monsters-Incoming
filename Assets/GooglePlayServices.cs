using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using TMPro;
using GooglePlayGames.BasicApi.SavedGame;
using System.Text;
using System.IO;




public class GooglePlayServices : MonoBehaviour
{

    public TextMeshProUGUI loginDetails;
    private string fileName = "player_data.json";
    private string localFilePath;

    void Awake()
    {

        localFilePath = Application.persistentDataPath + "/" + fileName;
        InitializeGPGS();
    }

    // 自动初始化 GPGS
    public void InitializeGPGS()
    {

        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();

        if (!PlayGamesPlatform.Instance.IsAuthenticated())
        {
            PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
        }
        else
        {
            Debug.Log("[GPGS] 已经登录，自动加载存档");
            LoadGame();
        }
    }


    internal void ProcessAuthentication(SignInStatus status)
    {
        Debug.Log($"[GPGS] Sign-in status = {status}");
        if (status == SignInStatus.Success)
        {
            Debug.Log("[GPGS] 登录成功 ✅");
            // Continue with Play Games Services

            string name = PlayGamesPlatform.Instance.GetUserDisplayName();
            string id = PlayGamesPlatform.Instance.GetUserId();
            string ImgUrl = PlayGamesPlatform.Instance.GetUserImageUrl();

            loginDetails.text = "Success \n" + name + id;

            LoadGame();
        }
        else
        {
            loginDetails.text = "Sign in Failed";
            Debug.LogWarning("[GPGS] 登录失败 ❌，请检查网络或配置");
            // Disable your integration with Play Games Services or show a login button
            // to ask users to sign-in. Clicking it should call
            //PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication); // 自动登录
            //PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication); //手动登录
        }
    }

    // 自动加载存档
    private void LoadGame()
    {
        PlayGamesPlatform.Instance.SavedGame.OpenWithAutomaticConflictResolution(
            fileName,
            DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLongestPlaytime,
            (status, meta) =>
            {
                if (status == SavedGameRequestStatus.Success)
                {
                    Debug.Log($"[GPGS] 成功打开存档: {fileName}");
                    ReadSaveData(meta);
                }
                else
                {
                    Debug.LogError($"[GPGS] 无法打开存档 {fileName}.错误：{status}");
                    // 如果没有存档，加载本地 JSON
                    LoadLocalJson();
                }
            });
    }

    // 读取存档数据
    private void ReadSaveData(ISavedGameMetadata meta)
    {
        PlayGamesPlatform.Instance.SavedGame.ReadBinaryData(meta, (status, data) =>
        {
            if (status == SavedGameRequestStatus.Success)
            {
                string jsonData = Encoding.UTF8.GetString(data);
                Debug.Log($"[GPGS] 读取云端存档成功: {jsonData}");

                // 将云端存档保存到本地
                File.WriteAllText(localFilePath, jsonData);
            }
            else
            {
                Debug.LogError("[GPGS] 读取云端存档失败，尝试加载本地存档");
                LoadLocalJson();
            }
        });
    }

    // 自动保存存档
    public void SaveGame()
    {
        Debug.Log("[GPGS] 正在保存存档…");

        string jsonData = LoadLocalJson();
        if (jsonData == null) jsonData = "{\"player\":\"test\",\"score\":0}";

        PlayGamesPlatform.Instance.SavedGame.OpenWithAutomaticConflictResolution(
            fileName,
            DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLongestPlaytime,
            (status, meta) =>
            {
                if (status == SavedGameRequestStatus.Success)
                {
                    Debug.Log($"[GPGS] 成功打开存档: {fileName}");
                    SaveJsonToDrive(meta, jsonData);
                }
                else
                {
                    Debug.LogError($"[GPGS] 无法打开存档 {fileName}.错误：{status}");
                }
            });
    }

    // 保存 JSON 文件到 Google Drive
    private void SaveJsonToDrive(ISavedGameMetadata meta, string jsonData)
    {
        byte[] data = Encoding.UTF8.GetBytes(jsonData);
        SavedGameMetadataUpdate update = new SavedGameMetadataUpdate.Builder().Build();

        PlayGamesPlatform.Instance.SavedGame.CommitUpdate(meta, update, data, (status, updatedMeta) =>
        {
            if (status == SavedGameRequestStatus.Success)
            {
                Debug.Log("[GPGS] JSON 文件已成功上传到 Google Drive");
            }
            else
            {
                Debug.LogError("[GPGS] 上传 JSON 文件失败：" + status);
            }
        });
    }

    // 读取本地 JSON 文件
    private string LoadLocalJson()
    {
        if (File.Exists(localFilePath))
        {
            string jsonData = File.ReadAllText(localFilePath);
            Debug.Log($"[GPGS] 读取本地 JSON 成功: {jsonData}");
            return jsonData;
        }
        else
        {
            Debug.LogWarning("[GPGS] 本地 JSON 文件不存在，创建新存档");
            return "{\"player\":\"test\",\"score\":0}";
        }
    }

    // 退出游戏时自动保存
    void OnApplicationQuit()
    {
        SaveGame();
    }
}
