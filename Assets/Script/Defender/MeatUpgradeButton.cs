using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MeatUpgradeButton : MonoBehaviour
{
    public DisplayMeat displayMeat;
    public Button upgradeButton;
    public TextMeshProUGUI buttonText; // 按钮文字

    // Start is called before the first frame update
    void Start()
    {
        upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
        UpdateButtonState(); // 初始状态
    }

    void OnUpgradeButtonClicked()
    {
        if (MeatManager.Instance.LevelUpMeat()) // 调用升级逻辑
        {
            displayMeat.UpdateUI(); // 升级成功后更新 UI
            displayMeat.PlayAnimation(); // 播放动画
        }

        AudioManager.instance.PlayUpgradeButtonSound();
    }
    private void Update()
    {
        UpdateButtonState(); // 初始状态
    }

    void UpdateButtonState()
    {
        bool canUpgrade = MeatManager.Instance != null &&
                          MeatManager.Instance.level < MeatManager.Instance.maxLevel &&
                          displayMeat.inventoryManager.GetCurrencyAmount("Coin") >= MeatManager.Instance.GetUpgradeCost();

        upgradeButton.interactable = canUpgrade;

        if (canUpgrade)
        {
            buttonText.color = Color.white; // 可用时恢复颜色
        }
        else
        {
            buttonText.color = Color.red; // 不可用时显示红色
        }
    }
}
