using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisplayMeat : MonoBehaviour
{
    public TMP_Text meatName;
    public TMP_Text upgradeCostTMP;
    public TMP_Text meatQuantityTMP;
    public TMP_Text loadingSpeedTMP;
    public TMP_Text meatLevel;
    public Image meatPIC;


    public InventoryManager inventoryManager; // 管理金币的组件
    //public MeatManager meatManager;

    [Header("Upgrade Settings")]
    public int initialUpgradeCost = 100;  // 初始升级费用
    public float upgradeRate = 1.5f;      // 升级费率

    public Animator upgradeAnimator;
    public GameObject animationObject;


    // 启动时加载数据
    void Start()
    {
        UpdateUI();
    }


    // 更新 UI
    public void UpdateUI()
    {
        meatName.text = "Food";
        upgradeCostTMP.text = FormatUpgradeCost(MeatManager.Instance.GetUpgradeCost());
        meatQuantityTMP.text = MeatManager.Instance.meatQuantity.ToString();
        loadingSpeedTMP.text = MeatManager.Instance.loadingSpeed.ToString("0.00");
        meatLevel.text = MeatManager.Instance.level.ToString();

        meatPIC.sprite = MeatManager.Instance.GetCurrentMeatSprite();
    }




    // 格式化升级费用显示
    private string FormatUpgradeCost(int cost)
    {
        if (cost >= 1_000_000_000) return (cost / 1_000_000_000f).ToString("0.#") + "B"; // 十亿
        if (cost >= 1_000_000) return (cost / 1_000_000f).ToString("0.#") + "M"; // 百万
        if (cost >= 1_000) return (cost / 1_000f).ToString("0.#") + "K"; // 千
        return cost.ToString();
    }

    // Start is called before the first frame update




    // 升级逻辑
    public void TryUpgradeMeat()
    {
        //if (meatManager == null) meatManager = FindObjectOfType<MeatManager>();

        bool success = MeatManager.Instance.LevelUpMeat();
        if (success)
        {
            UpdateUI();
            PlayAnimation();
        }
    }




    public void PlayAnimation()
    {
        if (upgradeAnimator != null)
        {
            animationObject.SetActive(true);
            upgradeAnimator.SetTrigger("UpgradeArrowTrigger");

            // 启动协程，等待动画播放完成后禁用子对象
            StartCoroutine(DisableAnimationObjectAfterDelay(0.5f));
        }
    }

    IEnumerator DisableAnimationObjectAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (animationObject != null)
        {
            animationObject.SetActive(false);
            upgradeAnimator.ResetTrigger("UpgradeArrowTrigger");
        }
    }

}