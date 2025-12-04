using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuButtonManager : MonoBehaviour
{
    public Button attackButton;
    public Button backButton;

    public Button forwardButton; // 前进按钮
    public Button backwardButton; // 后退按钮

    public List<Button> levelButtons; // 所有level按钮 (按顺序添加)

    //public MoveManToLevel moveManToLevel; // 引用MoveManToLevel脚本
    public MoveManWithButtons moveManWithButtons;

    // Start is called before the first frame update
    void Start()
    {
        attackButton.onClick.AddListener(OnAttackButtonClicked);
        backButton.onClick.AddListener(OnBackButtonClicked);

        forwardButton.onClick.AddListener(OnFowardButtonClicked);
        backwardButton.onClick.AddListener(OnBackwardButtonClicked);

        // 为每个点位按钮绑定点击事件
        for (int i = 0; i < levelButtons.Count; i++)
        {
            int index = i; // 防止闭包问题
            levelButtons[index].onClick.AddListener(() => OnLevelButtonClicked(index));
        }

    }

    // 点位按钮点击事件
    private void OnLevelButtonClicked(int index)
    {
        Transform targetLevel = moveManWithButtons.levelPositions[index];
        moveManWithButtons.MoveToLevelByClick(targetLevel);
    }


    private void OnAttackButtonClicked()
    {
        // 获取当前选中的Level名称
        string currentLevel = moveManWithButtons.GetCurrentLevelName();


        // 根据当前Level名称加载相应的场景
        switch (currentLevel)
        {
            case "Level1":
                SceneManager.LoadScene("ForestScene");
                break;
            case "Level2":
                SceneManager.LoadScene("ForestScene2");
                break;
            case "Level3":
                SceneManager.LoadScene("ForestScene3");
                break;
            case "Level4":
                SceneManager.LoadScene("ForestScene4");
                break;
            case "Level5":
                SceneManager.LoadScene("ForestScene5");
                break;
            case "Level6":
                SceneManager.LoadScene("ForestScene6");
                break;
            case "Level7":
                SceneManager.LoadScene("ForestScene7");
                break;
            case "Level8":
                SceneManager.LoadScene("ForestScene8");
                break;
            case "Level9":
                SceneManager.LoadScene("ForestScene9");
                break;
            case "Level10":
                SceneManager.LoadScene("ForestScene10");
                break;
            default:
                Debug.LogWarning("No level selected or invalid level.");
                break;
        }
    }

    private void OnBackButtonClicked()
    {
        SceneManager.LoadScene("MenuScene");
    }

    private void OnFowardButtonClicked()
    {
        moveManWithButtons.MoveForward();
    }

    private void OnBackwardButtonClicked()
    {
        moveManWithButtons.MoveBackward();
    }
}
