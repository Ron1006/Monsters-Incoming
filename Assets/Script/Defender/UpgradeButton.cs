using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeButton : MonoBehaviour
{
    [Header("References")]
    public Defender defender;                   // 直接引用 Defender
    public Button upgradeButton;                     // 升级按钮
    public TextMeshProUGUI buttonText;               // 按钮文字
    public InventoryManager inventoryManager;   // 引用金币管理器

    [Header("Audio")]
    public AudioManager audioManager;                // 用于播放升级按钮音效

    private void Start()
    {
        // 自动查找场景中的 InventoryManager
        if (inventoryManager == null)
        {
            inventoryManager = FindObjectOfType<InventoryManager>();
        }

        if ( audioManager == null)
        {
            audioManager = FindObjectOfType<AudioManager>();
        }

        if (defender == null || upgradeButton == null || buttonText == null)
        {
            Debug.LogError("UpgradeButton: 缺少必要的引用组件！");
            return;
        }

        // 绑定按钮点击事件
        upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
        UpdateButtonState(); // 初始状态
    }

    private void Update()
    {
        UpdateButtonState(); // 每帧更新按钮状态，确保实时检测金币和等级变化
    }

    // 点击升级按钮的逻辑
    // 点击升级按钮时的逻辑
    private void OnUpgradeButtonClicked()
    {
        if (defender != null)
        {
            bool upgraded = defender.LevelUp();  // 直接调用 Defender 的升级逻辑

            if (upgraded)
            {
                UpdateButtonState();            // 更新按钮状态

                if (audioManager != null)
                {
                    audioManager.PlayUpgradeButtonSound();  // 播放升级按钮音效
                }
            }
        }
    }

    // 动态更新按钮状态
    private void UpdateButtonState()
    {
        if (defender == null || inventoryManager == null)
        {
            upgradeButton.interactable = false;
            buttonText.color = Color.gray;
            return;
        }

        bool canUpgrade = defender.level < defender.maxLevel &&
                          inventoryManager.GetCurrencyAmount("Coin") >= defender.UpgradeCost;

        upgradeButton.interactable = canUpgrade;
        buttonText.color = canUpgrade ? Color.white : Color.red;
    }
}
