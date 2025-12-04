using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class EquipmentDrawButton : MonoBehaviour
{
    public Button drawButton; // �� UI ��ť
    public Button drawFiveButton;
    public GameObject drawGearPanel; // 抽一次的界面
    public GameObject drawGearPanel5; // 5连抽父对象
    public List<GameObject> drawPanels; // 5连抽object

    public Image drawButtonImage;
    public Sprite drawAvailableSprite; // Gem 足够时的图片
    public Sprite drawForbiddenSprite; // Gem 不足时的图片
    public Image drawFiveButtonImage;
    public Sprite draw5AvailableSprite; // Gem 足够时的图片
    public Sprite draw5ForbiddenSprite; // Gem 不足时的图片

    public int costDraw = 100;
    public int costDraw5 = 450;


    private void Start()
    {
        drawButton.onClick.AddListener(DrawRandomEquipment);
        drawFiveButton.onClick.AddListener(DrawFiveEquipments);
        drawGearPanel.SetActive(false);
        drawGearPanel5.SetActive(false);

        UpdateDrawButton(); // 初始时检查Button 状态
    }

   public void UpdateDrawButton()
    {
        bool canAfford = InventoryManager.Instance.CanAfford(costDraw, "Gem");

        // 切换按钮图片
        drawButtonImage.sprite = canAfford ? drawAvailableSprite : drawForbiddenSprite;

        // 如果 Gem 不足，禁用按钮
        drawButton.interactable = canAfford;

        bool canAfford5 = InventoryManager.Instance.CanAfford(costDraw5, "Gem");
        drawFiveButtonImage.sprite = canAfford5 ? draw5AvailableSprite : draw5ForbiddenSprite;
        drawFiveButton.interactable = canAfford5;
    }

    private void OnEnable()
    {
        UpdateDrawButton();
    }

    public void DrawRandomEquipment()
    {
        if (!InventoryManager.Instance.CanAfford(costDraw, "Gem"))
        {
            Debug.LogWarning("Gems 不足，无法抽取装备！");
            return;
        }
        // **扣除 Gems**
        InventoryManager.Instance.AddItem(-costDraw, "Gem");
        // **继续执行装备抽取逻辑**
        Debug.Log("成功抽取装备！");

        if (EquipmentManager.Instance.allEquipments.Count == 0)
        {
            Debug.LogWarning("装备数据库为空，无法抽取装备");
            return;
        }

        // **随机选取装备**
        Equipment randomEquipment = new Equipment(EquipmentManager.Instance.allEquipments[Random.Range(0, EquipmentManager.Instance.allEquipments.Count)]);
        // Equipment randomEquipment = EquipmentManager.Instance.allEquipments[0];
        //changed start
        randomEquipment.uid = EquipmentManager.Instance.SpawnEquipmentUid();
        //changed end
        // **存入玩家装备管理器**
        PlayerEquipmentManager.Instance.AddEquipment(randomEquipment);

        // **显示装备界面**
        drawGearPanel5.SetActive(false);
        drawGearPanel.SetActive(false); // 先禁用，重置动画
        drawGearPanel.SetActive(true);
        UpdateEquipmentUI(randomEquipment);
        UpdateDrawButton();
    }

    public void UpdateEquipmentUI(Equipment randomEquipment)
    {
        if(drawGearPanel == null)
        {
            Debug.LogError("drawGearPanel 为空，无法更新装备 UI");
            return;
        }

        // **自动查找子对象**
        Image equipmentImage = drawGearPanel.transform.Find("equipmentPIC")?.GetComponent<Image>();
        TMP_Text healthText = drawGearPanel.transform.Find("Health")?.GetComponent<TMP_Text>();
        TMP_Text damageText = drawGearPanel.transform.Find("Damage")?.GetComponent<TMP_Text>();
        TMP_Text coinText = drawGearPanel.transform.Find("Coin")?.GetComponent<TMP_Text>();

        // **检查是否找到所有 UI 组件**
        if (equipmentImage == null || healthText == null || damageText == null || coinText == null)
        {
            Debug.LogError("未能找到部分 UI 组件，请检查层级命名是否正确！");
            return;
        }

        // **加载装备图标** 
        equipmentImage.sprite = randomEquipment.equipmentSprite;

        // **更新装备属性显示**
        if (healthText != null)
        {
            healthText.text = randomEquipment.percentHealthBonus > 0
                ? $"Health +{randomEquipment.percentHealthBonus * 100}%"
                : randomEquipment.flatHealthBonus > 0
                    ? $"Health +{randomEquipment.flatHealthBonus}"
                    : "Health + 0";
        }

        if (damageText != null)
        {
            damageText.text = randomEquipment.percentAttackBonus > 0
                ? $"Damage +{randomEquipment.percentAttackBonus * 100}%"
                : randomEquipment.flatAttackBonus > 0
                    ? $"Damage +{randomEquipment.flatAttackBonus}"
                    : "Damage + 0";
        }
        coinText.text = $"Coin: {randomEquipment.value}";
    }

    public void DrawFiveEquipments()
    {
        if (!InventoryManager.Instance.CanAfford(costDraw5, "Gem"))
        {
            Debug.LogWarning("Gems 不足，无法抽取装备！");
            return;
        }
        // **扣除 Gems**
        InventoryManager.Instance.AddItem(-costDraw5, "Gem");
        // **继续执行装备抽取逻辑**
        Debug.Log("成功抽取装备！");

        if (EquipmentManager.Instance.allEquipments.Count == 0)
        {
            Debug.LogWarning("装备数据库为空，无法抽取装备");
            return;
        }
        drawGearPanel.SetActive(false);

        drawGearPanel5.SetActive(false);
        drawGearPanel5.SetActive(true); // 显示5连抽界面

        // 随机抽取 5 件装备
        List<Equipment> drawnEquipments = new List<Equipment>();
        for (int i = 0; i < 5;  i++)
        {
            Equipment randomEquipment = new Equipment(EquipmentManager.Instance.allEquipments[Random.Range(0, EquipmentManager.Instance.allEquipments.Count)]);
            randomEquipment.uid = EquipmentManager.Instance.SpawnEquipmentUid();
            drawnEquipments.Add(randomEquipment);
            PlayerEquipmentManager.Instance.AddEquipment(randomEquipment);
        }

        // 更新 UI
        for(int i = 0; i < drawPanels.Count; i++)
        {
            if(i < drawnEquipments.Count)
            {
                drawPanels[i].SetActive(true);
                UpdateEquipmentMutipleUI(drawPanels[i], drawnEquipments[i]);   
            }
            else
            {
                drawPanels[i].SetActive(false); // 隐藏未使用的面板
            }
        }
        UpdateDrawButton();
    }

    private void UpdateEquipmentMutipleUI(GameObject panel, Equipment equipment)
    {
        Image equipmentImage = panel.transform.Find("equipmentPIC")?.GetComponent<Image>();
        TMP_Text healthText = panel.transform.Find("Health")?.GetComponent<TMP_Text>();
        TMP_Text damageText = panel.transform.Find("Damage")?.GetComponent<TMP_Text>();
        TMP_Text coinText = panel.transform.Find("Coin")?.GetComponent<TMP_Text>();

        if (equipmentImage == null || healthText == null || damageText == null || coinText == null)
        {
            Debug.LogError("未能找到 UI 组件，请检查层级命名！");
            return;
        }

        // 直接赋值装备的 Sprite
        equipmentImage.sprite = equipment.equipmentSprite;

        // 更新属性
        if (healthText != null)
        {
            healthText.text = equipment.percentHealthBonus > 0
                ? $"Health +{equipment.percentHealthBonus * 100}%"
                : equipment.flatHealthBonus > 0
                    ? $"Health +{equipment.flatHealthBonus}"
                    : "Health + 0";
        }

        if (damageText != null)
        {
            damageText.text = equipment.percentAttackBonus > 0
                ? $"Damage +{equipment.percentAttackBonus * 100}%"
                : equipment.flatAttackBonus > 0
                    ? $"Damage +{equipment.flatAttackBonus}"
                    : "Damage + 0";
        }
        coinText.text = $"Coin: {equipment.value}";
    }
}
