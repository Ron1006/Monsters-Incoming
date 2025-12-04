using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverButton : MonoBehaviour
{
    public Button closeButton;
    public Button collectX2Button;
    public Button giveUpButton;
    public Button giveUpConfirm;
    public Button closeButtonGiveUpButton;
    public Button closeNewDefenderCanvas;
    public Canvas giveUpCanvas;
    public Canvas newDefenderCanvas;

    public InventoryManager inventoryManager;

    bool rewardPending;


    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1f; // 确保游戏启动时是正常运行的

        inventoryManager = FindObjectOfType<InventoryManager>();

        closeButton.onClick.AddListener(OnCloseClicked);
        collectX2Button.onClick.AddListener(OnCollectClicked);
        giveUpButton.onClick.AddListener(OnGiveUpClicked);
        giveUpConfirm.onClick.AddListener(OnGiveUpConfirmClicked);
        closeButtonGiveUpButton.onClick.AddListener(OnCloseGiveUpClicked);

        //showData.onClick.AddListener(OnShowDataClicked);
        if (newDefenderCanvas != null)
        {
            closeNewDefenderCanvas.onClick.AddListener(OnCloseNewDefenderCanvasClicked);
        }
        else
        {
            Debug.Log("newDefenderCanvas is not assigned. Skipping closeNewDefenderCanvas setup.");
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (rewardPending)
        {
            rewardPending = false;     // 只执行一次
            ApplyDoubleLoot();         // 这里安全地更新 UI / 存档
        }
    }


    void OnCloseNewDefenderCanvasClicked()
    {
        newDefenderCanvas.gameObject.SetActive(false);
    }

    // 打开give up弹窗并暂停游戏
    void OnGiveUpClicked()
    {
        giveUpCanvas.gameObject.SetActive(true);
        if (Time.timeScale != 0)  // 只有游戏运行时才执行
        {
            giveUpCanvas.gameObject.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    // 确认放弃游戏，并关闭give up弹窗
    void OnGiveUpConfirmClicked()
    {
        giveUpCanvas.gameObject.SetActive(false);
        TowerHealth towerHealth = FindObjectOfType<TowerHealth>();

        if (towerHealth != null)
        {
            towerHealth.TriggerGameOver();
        }
    }

    // 关闭give up 弹窗，并继续游戏
    void OnCloseGiveUpClicked()
    {
        giveUpCanvas.gameObject.SetActive(false);
        Time.timeScale = 1f;

    }

    // game over, 点击关闭回到map
    void OnCloseClicked()
    {
        SceneManager.LoadScene("MapScene");
    }


    // 观看广告，双倍金币
    void OnCollectClicked()
    {

        collectX2Button.interactable = false;

        MeatManager.Instance.SaveMeatData(); // 防止食物数据被重置

        AdManager.Instance.ShowRewardedAd(
            onRewardEarned: () => rewardPending = true,
            onNotReady: () =>
            {
                // 广告没准备好，回退逻辑 or 提示玩家稍后再试
                rewardPending = true;   // 也可以直接发奖励，方便测试
            });
    }

    /// <summary>‼️ 注意：这里可能在子线程，只做标记。</summary>
    void OnRewardEarned() => rewardPending = true;

    /// <summary>真正的加倍逻辑，始终在主线程执行。</summary>
    void ApplyDoubleLoot()
    {
        inventoryManager.DoubleLoot();
        //collectX2Button.interactable = true;   // 允许再点
        StartCoroutine(LoadSceneWithDelay());
    }


    IEnumerator LoadSceneWithDelay()
    {
        yield return new WaitForSeconds(1f); // 等待0.5秒
        SceneManager.LoadScene("MapScene");
    }


}
