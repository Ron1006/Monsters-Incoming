using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO;

[System.Serializable]
public class LevelEntry
{
    public string key;
    public bool value;

    public LevelEntry(string key, bool value)
    {
        this.key = key;
        this.value = value;
    }
}

[System.Serializable]
public class LevelProgressData
{
    public List<LevelEntry> levelUnlockStatusList = new List<LevelEntry>();

    public Dictionary<string, bool> ToDictionary()
    {
        Dictionary<string, bool> dict = new Dictionary<string, bool>();
        foreach (var entry in levelUnlockStatusList)
        {
            dict[entry.key] = entry.value;
        }
        return dict;
    }

    public void FromDictionary(Dictionary<string, bool> dict)
    {
        levelUnlockStatusList.Clear();
        foreach (var pair in dict)
        {
            levelUnlockStatusList.Add(new LevelEntry(pair.Key, pair.Value));
        }
    }
}


public class LevelManager : MonoBehaviour
{
    public Button attackButton;
    public Button backButton;
    public Button closeButton;
    public Canvas difficultyCanva;
    public TMP_Text title;

    public List<Button> levelButtons; // 所有level按钮 (按顺序添加)
    public List<Button> difficultyButtons; // 所有difficulty按钮

    public Sprite difficultySelectedSprite; // 选中状态图片
    public Sprite difficultyUnselectedSprite; // 未选中状态图片
    public Sprite difficultyLockedSprite; // 锁定状态图片

    public Sprite levelUnlockedSprite; // 可点击状态图片
    public Sprite levelLockedSprite;   // 不可点击状态图片
    public AudioSource lockedSound;    // 锁定状态播放的音效

    private LevelProgressData levelProgress;
    private string jsonFilePath;

    public GameObject information1;
    public GameObject information2;
    public GameObject information3;
    public GameObject information4;


    private Dictionary<string, (string title, string sceneName)> levelData = new Dictionary<string, (string title, string sceneName)>()
    {
        { "Level1", ("The Forest of Porko", "ForestScene") },
        { "Level2", ("Ancient ruins", "ForestScene2") },
        { "Level3", ("The Wheel of Doom", "ForestScene3") },
        { "Level4", ("The Forest of Slime", "ForestScene4") },
        { "Level5", ("Whispering Shadows", "ForestScene5") },
        { "Level6", ("The Hive of Horrors", "ForestScene6") },
        { "Level7", ("Realm of the Roaring Beasts", "ForestScene7") },
        { "Level8", ("The Orcish Stronghold", "ForestScene8") },
        { "Level9", ("Cave of Screeching Terror", "ForestScene9") },
        { "Level10", ("The King", "ForestScene10") }
        // 根据需要继续添加关卡的标题和场景名字
    };

    // 用于匹配 关卡名 → Level 编号
    private Dictionary<string, string> sceneToLevelMap = new Dictionary<string, string>()
{
    { "ForestScene", "Level1" },
    { "ForestScene2", "Level2" },
    { "ForestScene3", "Level3" },
    { "ForestScene4", "Level4" },
    { "ForestScene5", "Level5" },
    { "ForestScene6", "Level6" },
    { "ForestScene7", "Level7" },
    { "ForestScene8", "Level8" },
    { "ForestScene9", "Level9" },
    { "ForestScene10", "Level10" }
};

    private string selectedSceneName; // 用于存储当前选中的场景名称
    private Button selectedDifficultyButton; // 当前选中的难度按钮
    private int selectedDifficultyLevel = 1; // 默认难度级别为 1

    private void Awake()
    {
        Time.timeScale = 1f; // 确保恢复正常时间流动
        jsonFilePath = Path.Combine(Application.persistentDataPath, "level_progress.json");
        LoadLevelProgress();
    }


    private void Start()
    {
        CheckAndUnlockNextLevel();
        attackButton.onClick.AddListener(OnAttackButtonClicked);
        backButton.onClick.AddListener(OnBackButtonClicked);
        closeButton.onClick.AddListener(OnCloseButtonClicked);

        MeatManager.Instance.SaveMeatData(); // 保存食物数据，防止丢失

        // 为每个点位按钮绑定点击事件
        for (int i = 0; i < levelButtons.Count; i++)
        {
            int index = i;
            levelButtons[index].onClick.AddListener(() => OnLevelButtonClicked(levelButtons[index].gameObject.name));
        }

        // 为每个难度按钮绑定事件
        foreach (Button difficultyButton in difficultyButtons)
        {
            difficultyButton.onClick.AddListener(() => OnDifficultyButtonClicked(difficultyButton) );
        }

        // 设置第一个难度按钮为默认选中状态
        if (difficultyButtons.Count > 0)
        {
            selectedDifficultyButton = difficultyButtons[0];
            SetButtonState(selectedDifficultyButton, true);
        }

        UpdateLevelUI(); 
        difficultyCanva.gameObject.SetActive(false);
    }

    private void CheckAndUnlockNextLevel()
    {
        if (PlayerPrefs.GetInt("LastCompletedLevel", 0) == 1) // 如果上次通关了
        {
            string completedLevel = PlayerPrefs.GetString("CompletedLevelName", "");

            if (!string.IsNullOrEmpty(completedLevel))
            {
                Debug.Log($"检测到已通关的关卡: {completedLevel}");

                // **解锁下一关**
                UnlockNextLevel(completedLevel);

                // **清除通关记录，防止重复解锁**
                PlayerPrefs.DeleteKey("LastCompletedLevel");
                PlayerPrefs.DeleteKey("CompletedLevelName");
                PlayerPrefs.Save();
            }
            else
            {
                Debug.LogWarning("未找到 `CompletedLevelName`，跳过解锁");
            }
        }
    }

    private void LoadLevelProgress()
    {
        if (File.Exists(jsonFilePath))
        {
            string json = File.ReadAllText(jsonFilePath);
            levelProgress = JsonUtility.FromJson<LevelProgressData>(json);
            Debug.Log("成功加载 Level 进度: " + json);
        }
        else
        {
            levelProgress = new LevelProgressData();
            Debug.Log("未找到 Level 进度文件，创建新的进度数据！");
        }

        // **确保 `levelUnlockStatus` 不为空**
        Dictionary<string, bool> levelUnlockDict = levelProgress.ToDictionary();

        // **默认解锁第一关**
        if (!levelUnlockDict.ContainsKey("Level1"))
        {
            levelUnlockDict["Level1"] = true;
            Debug.Log("默认解锁 Level1");
        }

        levelProgress.FromDictionary(levelUnlockDict);
        SaveLevelProgress(); // **存储更新的 JSON**
    }

    private void SaveLevelProgress()
    {
        if (levelProgress == null)
        {
            levelProgress = new LevelProgressData();
        }

        string json = JsonUtility.ToJson(levelProgress, true);
        File.WriteAllText(jsonFilePath, json);
        Debug.Log("已保存 Level 进度: " + json);
    }

    public void UpdateLevelUI()
    {
        // **获取 Dictionary**
        Dictionary<string, bool> levelUnlockDict = levelProgress.ToDictionary();

        for (int i = 0; i < levelButtons.Count; i++)
        {
            string levelKey = $"Level{i + 1}";
            bool isUnlocked = levelUnlockDict.ContainsKey(levelKey) && levelUnlockDict[levelKey];

            Button levelButton = levelButtons[i];
            levelButton.interactable = isUnlocked;
            levelButton.GetComponent<Image>().sprite = isUnlocked ? levelUnlockedSprite : levelLockedSprite;

            //Debug.Log($"关卡 {levelKey} 状态: {(isUnlocked ? "已解锁" : "未解锁")}");

            if (!isUnlocked)
            {
                levelButton.onClick.RemoveAllListeners();
                levelButton.onClick.AddListener(() => PlayLockedSound());
            }
        }
    }

    private void PlayLockedSound()
    {
        if (lockedSound != null)
        {
            lockedSound.Play();
        }
    }

    private void OnLevelButtonClicked(string levelName)
    {
        // **获取 Dictionary**
        Dictionary<string, bool> levelUnlockDict = levelProgress.ToDictionary();

        if (!levelUnlockDict.ContainsKey(levelName) || !levelUnlockDict[levelName])
        {
            PlayLockedSound();
            return;
        }

        Debug.Log($"Level button clicked: {levelName}");

        // 根据关卡名字设置标题和场景名称
        if (levelData.ContainsKey(levelName))
        {
            title.text = levelData[levelName].title;
            selectedSceneName = levelData[levelName].sceneName;
            //Debug.Log($"Title set to: {title.text}, Scene set to: {selectedSceneName}");
        }
        else
        {
            title.text = "Unknown Level"; // 如果找不到名字映射，使用默认值
            selectedSceneName = null;    // 没有匹配的场景名称
        }

        // **检查解锁状态**
        for (int i = 0; i < difficultyButtons.Count; i++)
        {
            string difficultyKey = $"{selectedSceneName}_Difficulty_{i + 1}_Unlocked";
            bool isUnlocked = PlayerPrefs.GetInt(difficultyKey, i == 0 ? 1 : 0) == 1; // 第一难度默认解锁

            difficultyButtons[i].interactable = isUnlocked;
            difficultyButtons[i].GetComponent<Image>().sprite = isUnlocked ? difficultyUnselectedSprite : difficultyLockedSprite;
            difficultyButtons[i].GetComponent<Image>().color = Color.white; // 确保所有按钮都不透明
        }

        // **默认选中 Rookie（难度 1）**
        if (difficultyButtons.Count > 0)
        {
            OnDifficultyButtonClicked(difficultyButtons[0]);
        }

        // 打开选择难度的 canvas
        difficultyCanva.gameObject.SetActive(true);
    }

    private void OnDifficultyButtonClicked(Button clickedButton)
    {
        // 如果已有选中的按钮，切换回未选中状态
        if (selectedDifficultyButton != null)
        {
            SetButtonState(selectedDifficultyButton, false);
        }

        // 设置当前点击的按钮为选中状态
        selectedDifficultyButton = clickedButton;
        SetButtonState(selectedDifficultyButton, true);

        // 获取所选难度索引
        selectedDifficultyLevel = difficultyButtons.IndexOf(clickedButton) + 1;

        // **更新 Information UI**
        UpdateInformationUI(selectedDifficultyLevel);
    }

    private void UpdateInformationUI(int difficultyLevel)
    {
        // 先隐藏所有 information UI
        information1.SetActive(false);
        information2.SetActive(false);
        information3.SetActive(false);
        information4.SetActive(false);

        // 根据选择的难度激活对应的 information
        switch (difficultyLevel)
        {
            case 1:
                information1.SetActive(true);
                break;
            case 2:
                information2.SetActive(true);
                break;
            case 3:
                information3.SetActive(true);
                break;
            case 4:
                information4.SetActive(true);
                break;
            default:
                Debug.LogWarning("unknow difficulty");
                break;
        }
    }

    private void SetButtonState(Button button, bool isSelected)
    {
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            bool isUnlocked = button.interactable; // 只有可点击的才修改状态
            buttonImage.sprite = isUnlocked ? (isSelected ? difficultySelectedSprite : difficultyUnselectedSprite) : difficultyLockedSprite;
            buttonImage.color = Color.white; // 保证所有按钮不透明
        }

        // 设置按钮的缩放比例
        button.transform.localScale = isSelected ? Vector3.one : new Vector3(0.8f, 0.8f, 1f); // 三元运算，如果isSelected = true, 返回Vector3.one, 如果ifSelected = false 返回new Vector3
    }

    public void UnlockNextLevel(string currentSceneName)
    {
        if (!sceneToLevelMap.TryGetValue(currentSceneName, out string currentLevelKey))
        {
            Debug.LogError($"UnlockNextLevel: 无法找到匹配的 Level，当前场景名: {currentSceneName}");
            return;
        }

        int currentIndex = int.Parse(currentLevelKey.Replace("Level", "").Trim());
        int nextIndex = currentIndex + 1;
        string nextLevelKey = $"Level{nextIndex}";

        Dictionary<string, bool> levelUnlockDict = levelProgress.ToDictionary();

        if (!levelUnlockDict.ContainsKey(nextLevelKey))
        {
            levelUnlockDict[nextLevelKey] = true;
            levelProgress.FromDictionary(levelUnlockDict);
            SaveLevelProgress();
            Debug.Log($"解锁 {nextLevelKey}");
        }
    }


    private void OnAttackButtonClicked()
    {
        if (!string.IsNullOrEmpty(selectedSceneName))
        {
            //使用PlayerPrefs 储存所选难度级别
            PlayerPrefs.SetInt("SelectedDifficultyLevel", selectedDifficultyLevel);
            PlayerPrefs.Save();

            // 加载选择的场景

            SceneManager.LoadScene(selectedSceneName);


        }
        else
        {
            Debug.LogWarning("No scene selected!");
        }
    }

    private void OnBackButtonClicked()
    {
        SceneManager.LoadScene("MenuScene");
    }

    private void OnCloseButtonClicked()
    {
        difficultyCanva.gameObject.SetActive(false);
    }
}
