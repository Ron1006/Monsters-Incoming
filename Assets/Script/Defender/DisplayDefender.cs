using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisplayDefender : Defender  // 不继承 Defender
{
    [Header("Defender Info")]
    public Defender defender; // 引用要展示的 Defender

    [Header("UI Elements")]
    public TMP_Text defenderNameText;
    public TMP_Text upgradeCostTMP;
    public TMP_Text attackPowerTMP;
    public TMP_Text healthTMP;
    public TMP_Text defenderLevel;
    public Image defenderPIC;
    public Image equipped;
    public Image unEquipped;

    [Header("Upgrade Animation")]
    public Animator upgradeAnimator;
    public GameObject animationObject;

    private void Start()
    {
        if (defender == null)
        {
            Debug.LogWarning("Defender reference is not assigned.");
            return;
        }
        UpdateUI();  // 只在开始时调用


    }


    // 手动刷新 UI 的方法
    public void RefreshUI()
    {
        UpdateUI();
    }

    // 实时更新 UI（可根据需要删除）
    private void Update()
    {
        UpdateUI();
    }

    // 更新 UI 逻辑
    private void UpdateUI()
    {
        if (defender == null) return;

        if (defenderNameText != null) defenderNameText.text = defender.defenderName;
        if (upgradeCostTMP != null) upgradeCostTMP.text = FormatUpgradeCost(defender.UpgradeCost);
        if (defender.level == defender.maxLevel)
        {
            upgradeCostTMP.text = "Max";
        }
        if (attackPowerTMP != null) attackPowerTMP.text = Mathf.RoundToInt(defender.attackPower).ToString();
        if (healthTMP != null) healthTMP.text = Mathf.RoundToInt(defender.health).ToString();
        if (defenderLevel != null) defenderLevel.text = defender.level.ToString();

        DefenderAttribute defenderAttribute = defender.GetComponent<DefenderAttribute>();
        if (defenderPIC != null && defenderAttribute != null && defenderAttribute.defenderLevelImages.Length > 0)
        {
            int index = Mathf.Clamp((defender.level - 1) / 5, 0, defenderAttribute.defenderLevelImages.Length - 1);
            defenderPIC.sprite = defenderAttribute.defenderLevelImages[index];
        }

        bool hasEquipment = defender.equippedItems.Count > 0; // 判断是否有装备

        if(equipped != null) equipped.gameObject.SetActive(hasEquipment);
        if(unEquipped != null) unEquipped.gameObject.SetActive(!hasEquipment);
    }

    // 格式化升级费用显示
    private string FormatUpgradeCost(int cost)
    {
        if (cost >= 1_000_000_000) return (cost / 1_000_000_000f).ToString("0.#") + "B"; // 十亿
        if (cost >= 1_000_000) return (cost / 1_000_000f).ToString("0.#") + "M"; // 百万
        if (cost >= 1_000) return (cost / 1_000f).ToString("0.#") + "K"; // 千
        return cost.ToString();
    }

    // 播放升级动画
    public void PlayUpgradeAnimation()
    {
        if (upgradeAnimator == null || animationObject == null)
        {
            Debug.LogWarning("Upgrade animation components are missing!");
            return;
        }

        animationObject.SetActive(true);
        upgradeAnimator.SetTrigger("UpgradeArrowTrigger");
        StartCoroutine(DisableAnimationAfterDelay(0.5f));
    }

    // 延迟关闭动画
    private IEnumerator DisableAnimationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (animationObject != null)
        {
            animationObject.SetActive(false);
            upgradeAnimator.ResetTrigger("UpgradeArrowTrigger");
        }
    }
}
