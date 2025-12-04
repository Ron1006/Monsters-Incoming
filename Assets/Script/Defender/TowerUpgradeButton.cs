using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerUpgradeButton : MonoBehaviour
{
    //public DisplayTower displayTower;
    public Button upgradeButton;
    public TextMeshProUGUI buttonText; // 按钮文字

    // Start is called before the first frame update
    //void Start()
    //{
    //    upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
    //    UpdateButtonState(); // 初始状态
    //}

    //void OnUpgradeButtonClicked()
    //{
    //    //displayTower.LevelUp();
    //    UpdateButtonState(); // 点击后更新按钮状态
    //    // 播放全局按钮点击声音
    //    AudioManager.instance.PlayUpgradeButtonSound();
    //}

    //private void Update()
    //{
    //    UpdateButtonState(); // 初始状态
    //}

    //void UpdateButtonState()
    //{
    //    bool canUpgrade = displayTower != null &&
    //                      displayTower.level < displayTower.maxLevel &&
    //                      displayTower.inventoryManager.GetCurrentCoins() >= displayTower.UpgradeCost;

    //    upgradeButton.interactable = canUpgrade;

    //    if (canUpgrade)
    //    {
    //        buttonText.color = Color.white; // 可用时恢复颜色
    //    }
    //    else
    //    {
    //        buttonText.color = Color.red; // 不可用时显示红色
    //    }
    //}
}
