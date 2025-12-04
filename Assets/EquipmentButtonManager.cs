using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentButtonManager : MonoBehaviour
{
    public static EquipmentButtonManager Instance { get; private set; } // **添加单例**

    public Button ringLeftButton;
    public Button necklaceButton;
    public Button bootsButton;
    public Button armorButton;
    public Button helmetButton;
    public Button weaponButton;

    public Image ringLeftIcon;
    public Image necklaceIcon;
    public Image bootsIcon;
    public Image armorIcon;
    public Image helmetIcon;
    public Image weaponIcon;

    //public GameObject noEquipmentText;
    public GameObject tapIconText;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip openEquipmentSound;

    // **手动拖入默认灰色装备图标**
    [Header("defalut equipment icon")]
    public Sprite defaultRingIcon;
    public Sprite defaultBootsIcon;
    public Sprite defaultArmorIcon;
    public Sprite defaultHelmetIcon;
    public Sprite defaultWeaponIcon;
    public Sprite defaultNecklaceIcon;

    private Dictionary<EquipmentType, Sprite> defaultIcons = new Dictionary<EquipmentType, Sprite>(); // **存储默认图标**

    public EquipmentScrollView equipmentScrollView; // 引用装备滚动视图
    public Defender currentDefender; //储存当前defender

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;  // **赋值单例**
            DontDestroyOnLoad(gameObject);  // **确保不会在场景切换时销毁**
        }
        else
        {
            Destroy(gameObject);  // 确保只有一个实例
        }
    }

    private void Start()
    {
        //Debug.Log("EquipmentButtonManager 初始化成功！");

        tapIconText.SetActive(true);
        // **设置默认装备图标**
        defaultIcons[EquipmentType.Ring] = defaultRingIcon;
        defaultIcons[EquipmentType.Boots] = defaultBootsIcon;
        defaultIcons[EquipmentType.Armor] = defaultArmorIcon;
        defaultIcons[EquipmentType.Helmet] = defaultHelmetIcon;
        defaultIcons[EquipmentType.Weapon] = defaultWeaponIcon;
        defaultIcons[EquipmentType.Necklace] = defaultNecklaceIcon;

        ringLeftButton.onClick.AddListener(() => OpenEquipmentList(EquipmentType.Ring));
        necklaceButton.onClick.AddListener(() => OpenEquipmentList(EquipmentType.Necklace));
        bootsButton.onClick.AddListener(() => OpenEquipmentList(EquipmentType.Boots));
        armorButton.onClick.AddListener(() => OpenEquipmentList(EquipmentType.Armor));
        helmetButton.onClick.AddListener(() => OpenEquipmentList(EquipmentType.Helmet));
        weaponButton.onClick.AddListener(() => OpenEquipmentList(EquipmentType.Weapon));
    }

    // **设置当前 Defender 并加载装备**
    public void SetCurrentDefender(Defender defender)
    {
        if (defender == null)
        {
            Debug.LogError("[EQUIPMENT] 传入的 Defender 为空！");
            return;
        }

        Debug.Log($"[EQUIPMENT] 设置当前 Defender: {defender.defenderName}");

        // **确保装备数据正确加载**
        DefenderDataManager.Instance.RestoreDefenderEquipment(defender);

        currentDefender = defender;

        // **更新装备 UI**
        LoadEquippedItems();

    }

    // **打开装备列表**
    private void OpenEquipmentList(EquipmentType type)
    {
        if (openEquipmentSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(openEquipmentSound);
        }

        if (currentDefender == null)
        {
            //noEquipmentText.SetActive(true);
            Debug.LogWarning("[ERROR] 当前 Defender 为空，无法打开装备列表！");
            return;
        }

        Debug.Log($"打开 {type} 装备列表，当前 Defender: {currentDefender.defenderName}");
        equipmentScrollView.PopulateList(type, currentDefender); // 传入当前 Defender
        tapIconText.SetActive(false);
    }

    // **更新装备按钮图标**
    public void UpdateEquipmentButtonIcon(EquipmentType type, Sprite newIcon)
    {
        if (newIcon == null)
        {
            Debug.LogWarning($"[WARNING] 试图更新 {type} 的图标，但 newIcon 为空！");
            ResetEquipmentButtonIcon(type);
            return;
        }
        //Debug.Log($"[EQUIPMENT] 更新 {type} 图标: {newIcon.name}");

        switch (type)
        {
            case EquipmentType.Weapon:
                weaponIcon.sprite = newIcon;
                break;
            case EquipmentType.Armor:
                armorIcon.sprite = newIcon;
                break;
            case EquipmentType.Boots:
                bootsIcon.sprite = newIcon;
                break;
            case EquipmentType.Helmet:
                helmetIcon.sprite = newIcon;
                break;
            case EquipmentType.Ring:
                ringLeftIcon.sprite = newIcon;
                break;
            case EquipmentType.Necklace:
                necklaceIcon.sprite = newIcon;
                break;
            default:
                Debug.LogWarning($"[WARNING] 未知装备类型 {type}，无法更新图标！");
                break;

        }
    }

    public void ResetEquipmentButtonIcon(EquipmentType type)
    {
        if (!defaultIcons.ContainsKey(type))
        {
            Debug.LogWarning($"[WARNING] {type} 没有找到默认图标！");
            return;
        }

        //Debug.Log($"[EQUIPMENT] 恢复 {type} 为默认图标");

        UpdateEquipmentButtonIcon(type, defaultIcons[type]);
    }




    // **加载当前 Defender 的已装备装备**
    private void LoadEquippedItems()
    {
        if (currentDefender == null)
        {
            Debug.LogWarning("[ERROR] 当前 Defender 为空，无法加载装备！");
            return;
        }

        //Debug.Log($"[EQUIPMENT] 正在加载 {currentDefender.defenderName} 的装备...");

        // **获取 Defender 的已装备装备**
        Dictionary<EquipmentType, Equipment> equippedItems = currentDefender.equippedItems;

        if (equippedItems == null || equippedItems.Count == 0)
        {
            Debug.LogWarning($"[EQUIPMENT] {currentDefender.defenderName} 没有装备！");
            ResetAllEquipmentIcons();
            return;
        }

        foreach (var kvp in equippedItems)
        {
            EquipmentType type = kvp.Key;
            Equipment equipment = kvp.Value;

            //Debug.Log($"[EQUIPMENT] {currentDefender.defenderName} 装备 {type}: {equipment.equipmentName}");

            // **检查是否有 Sprite**
            if (equipment == null || equipment.equipmentSprite == null)
            {
                Debug.LogError($"[ERROR] {equipment.equipmentName} 没有 Sprite，可能是资源路径错误！");
                ResetEquipmentButtonIcon(type);
                continue;
            }

            // **更新装备 UI**
            UpdateEquipmentButtonIcon(type, equipment.equipmentSprite);
        }

        // **检查未装备的部位，并恢复默认图标**
        ResetMissingEquipmentIcons(equippedItems);
    }

    // **重置所有装备槽的图标**
    private void ResetAllEquipmentIcons()
    {
        foreach (var type in defaultIcons.Keys)
        {
            ResetEquipmentButtonIcon(type);
        }
    }

    // **仅重置未装备的装备槽图标**
    private void ResetMissingEquipmentIcons(Dictionary<EquipmentType, Equipment> equippedItems)
    {
        foreach (var type in defaultIcons.Keys)
        {
            if (!equippedItems.ContainsKey(type))
            {
                ResetEquipmentButtonIcon(type);
            }
        }
    }
}
