using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LotterySystemBAK : MonoBehaviour
{
    public GameObject[] defenderPrefabs; // 存储所有可能的 defender prefabs, 只负责储存，不能排列什么的
    public Transform defenderContainer; // 父物体，用于容纳生成的 defender
    public Transform cardContainer; // 用于放置抽到的卡片图片
    public int drawQuantitySingle = 1; // 最大抽取1个
    public int drawQuantityMultiple = 5; // 抽取多个
    public int drawOnceCost = 100;
    public int drawMultipleCost = 450;

    public Transform editTeamContainer; // Edit Team Canvas 中的容器
    public Transform[] dropTargetPanel;

    public GameObject cardPopup; // 显示抽取结果


    private List<GameObject> defenderInstance = new List<GameObject>(); // 存储生成的 defender 实例，可以动态排列

    public Button clearButton; // 清空按钮
    public Button closeButton; // 关闭抽奖弹窗
    public Button clearPlayerPrefsButton; // 清空 PlayerPrefs 的按钮

    public InventoryManager inventoryManager; // 用于获取金币数量并减少金币

    // cardpopup起始动画
    private Vector2 offscreenPosition = new Vector2(-1500, 0); // 左侧起始位置
    private Vector2 onscreenPosition = new Vector2(0, 0); // 目标位置（屏幕中央）
    public float slideDuration = 0.3f; // 滑动动画时长



    private void Start()
    {

        LoadDefenderOrder();

        // 将清空按钮的点击事件绑定到 ClearDefenderList 方法
        closeButton.onClick.AddListener(CloseCardPopup);
        clearPlayerPrefsButton.onClick.AddListener(ClearPlayerPrefs);

        cardPopup.SetActive(false); //激活抽取结果界面
    }


    // 抽奖并添加新的 defender
    public void DrawDefender(int quantity, int cost)
    {
        // 获取 InventoryManager（假设它在场景中的某个对象上）
        inventoryManager = FindObjectOfType<InventoryManager>();

        if (inventoryManager != null)
        {
            if (inventoryManager.GetCurrencyAmount("Coin") >= cost)
            {
                // 扣除coin
                inventoryManager.AddItem(-cost, "Coin"); //减少金币数量

                cardPopup.SetActive(true);
                StartCoroutine(SlideInCardPopup(quantity)); // 启动滑动协程

                // 抽取指定数量的defender
                for (int i = 0; i < quantity; i++)
                {
                    // 随机选择一个 defender prefab, 4是跳过前4个defender
                    int randomIndex = Random.Range(4, defenderPrefabs.Length);
                    GameObject selectedPrefab = defenderPrefabs[randomIndex];

                    // 检查 defenderInstance 中是否已有相同的名字（考虑默认对象和克隆对象）
                    bool isDuplicate = defenderInstance.Exists(defender => defender.name == selectedPrefab.name || defender.name == selectedPrefab.name + "(Clone)");

                    if (isDuplicate)
                    {
                        // Duplicate found, convert to 50 coins
                        inventoryManager.AddItem(50, "Coin"); // 增加50金币
                        Debug.Log("Duplicate defender found, converted to 50 coins.");

                        GameObject newCard = Instantiate(selectedPrefab, cardContainer);

                        //DisableRaycast(newCard); // 禁用 Raycast

                        // 仅显示图片，隐藏其他组件
                        foreach (Transform child in newCard.transform)
                        {
                            if (child.name != "DefenderPic" && child.name != "Square") //如果名字不是DefenderPic, 则隐藏
                            {
                                child.gameObject.SetActive(false);
                            }
                        }

                        // Start a coroutine to change the DefenderPic to a coin image after 1 second
                        StartCoroutine(ChangeToCoinImage(newCard));
                    }
                    else
                    {
                        // No duplicate, add defender to container
                        GameObject newDefender = Instantiate(selectedPrefab, defenderContainer);
                        GameObject newCard = Instantiate(selectedPrefab, cardContainer);

                        DisableRaycast(newCard); // 禁用 Raycast
                        DisableRaycast(newDefender);

                        if (!HasDefenderWithName(editTeamContainer, selectedPrefab.name) && !HasDefenderInPanels(dropTargetPanel, selectedPrefab.name))
                        {
                            // 同步到edit Team容器
                            GameObject editTeamDefender = Instantiate(selectedPrefab, editTeamContainer);

                            // 启用 Raycast，仅限于 editTeam 中的实例
                            EnableRaycast(editTeamDefender);

                            // 禁用升级按钮
                            Button upgradeButton = editTeamDefender.GetComponentInChildren<Button>();

                            if (upgradeButton != null)
                            {
                                upgradeButton.gameObject.SetActive(false);
                            }

                            // 仅显示图片，隐藏其他组件
                            foreach (Transform child in newCard.transform)
                            {
                                if (child.name != "DefenderPic" && child.name != "Square") //如果名字不是DefenderPic, 则隐藏
                                {
                                    child.gameObject.SetActive(false);
                                }
                            }

                            newDefender.SetActive(true); // 强制激活
                                                         // 设置 cardContainer 中的卡片不可见，稍后再显示
                            newCard.SetActive(false);
                            defenderInstance.Add(newDefender);
                            // 设置 defender 在 UI 中的显示位置
                            ArrangeDefenders();
                            ArrangeEditTeamDefenders();
                        }
                        else
                        {
                            Debug.Log($"Defender {selectedPrefab.name} already exists in editTeamContainer or dropTargetPanel. Skipping.");
                        }
                    }
                }



                // 抽奖完成后保存 defender 数量
                SaveDefenderOrder();
            }
            else
            {
                Debug.LogWarning("Not enough coin.");
            }
        }

    }

    // 动态排列 defender 位置
    private void ArrangeDefenders()
    {
        float spacing = 1800f; // 控制每个 defender 之间的间距

        for (int i = 0; i < defenderInstance.Count; i++)
        {
            GameObject defender = defenderInstance[i];
            RectTransform rectTransform = defender.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(i * spacing, 0);
        }
    }

    // 把defender 排列到edit team 容器里
    private void ArrangeEditTeamDefenders()
    {
        float spacing = 200f; // 控制每个 defender 之间的间距
        int defenderIndex = 0;

        foreach (Transform child in editTeamContainer)
        {
            // 检查是否是防御者对象（根据是否有特定组件）
            if (child.GetComponent<Defender>() != null)
            {

                RectTransform rectTransform = child.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    // 只调整防御者的位置，不影响其他 UI 元素
                    float originalX = rectTransform.anchoredPosition.x; // 保留原始 X 坐标
                    float originalY = rectTransform.anchoredPosition.y; // 保留原始 Y 坐标

                    rectTransform.anchoredPosition = new Vector2(originalX, defenderIndex * -spacing + originalY);
                }
                defenderIndex++;
            }
        }
    }

    public void ClearDefenderList()
    {
        // 删除列表中的所有防御者对象
        foreach (GameObject defender in defenderInstance)
        {
            if (defender != null)
            {
                Destroy(defender);
            }
        }

        // 清空列表
        defenderInstance.Clear();
        //Debug.Log("Defender list has been cleared.");



    }


    public void CloseCardPopup()
    {
        cardPopup.SetActive(false);

        // 清空card container
        foreach (Transform child in cardContainer)
        {
            Destroy(child.gameObject);
        }
        Debug.Log("Card container has been cleared.");
    }

    // 滑动协程， 从左侧划入到目标位置
    private IEnumerator SlideInCardPopup(int quantity)
    {
        RectTransform rectTransform = cardPopup.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = offscreenPosition; // 设置初始位置

        float elapsedTime = 0;
        while (elapsedTime < slideDuration)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(offscreenPosition, onscreenPosition, elapsedTime / slideDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        // 确保最终位置准确
        rectTransform.anchoredPosition = onscreenPosition;

        // 滑动完成后显示卡片内容
        StartCoroutine(ShowCardContent(quantity));
    }

    private IEnumerator ShowCardContent(int quantity)
    {
        // 在 cardContainer 中显示卡片内容，或执行其他逻辑
        for (int i = 0; i < quantity; i++)
        {
            if (i < cardContainer.childCount)
            {
                Transform card = cardContainer.GetChild(i);
                card.gameObject.SetActive(true);
                // 开始放大缩小的动画效果
                StartCoroutine(ScaleCardEffect(card));
                yield return new WaitForSeconds(0.3f); // 每张卡片显示时延迟0.5秒
            }
        }
    }

    // 卡片动画
    private IEnumerator ScaleCardEffect(Transform card)
    {
        float duration = 0.2f; // 缩放动画持续时间
        float elapsedTime = 0;
        Vector3 initialScale = Vector3.one * 1.5f; // 初始放大的比例
        Vector3 targetScale = Vector3.one; // 最终正常比例

        card.localScale = initialScale; // 设置初始缩放

        while (elapsedTime < duration)
        {
            card.localScale = Vector3.Lerp(initialScale, targetScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 确保最终缩放精确
        card.localScale = targetScale;
    }

    public void SaveDefenderOrder()
    {
        HashSet<string> defenderNames = new HashSet<string>();

        foreach (GameObject defender in defenderInstance)
        {
            string originalName = defender.name.Replace("(Clone)", "");
            defenderNames.Add(originalName);
        }

        string json = JsonUtility.ToJson(new SerializableList<string>(new List<string>(defenderNames)));
        PlayerPrefs.SetString("DefenderNames", json);
        PlayerPrefs.Save();
    }

    private GameObject GetDefenderPrefabByName(string name)
    {
        foreach (GameObject prefab in defenderPrefabs)
        {
            if (prefab.name == name)
            {
                return prefab;
            }
        }
        return null;
    }
    public void LoadDefenderOrder()
    {
        // 清空现有的 defenderInstance 列表，避免重复实例化
        ClearDefenderList();

        // 获取存储的 DefenderNames JSON
        string json = PlayerPrefs.GetString("DefenderNames", "{}");
        SerializableList<string> defenderNames = JsonUtility.FromJson<SerializableList<string>>(json);

        foreach (string defenderName in defenderNames.list)
        {
            GameObject defenderPrefab = GetDefenderPrefabByName(defenderName);
            if (defenderPrefab != null)
            {
                // 添加到主容器
                GameObject newDefender = Instantiate(defenderPrefab, defenderContainer);
                newDefender.SetActive(true);
                defenderInstance.Add(newDefender);

                // 添加到 Edit Team 容器
                GameObject editTeamDefender = Instantiate(defenderPrefab, editTeamContainer);
                editTeamDefender.SetActive(true);

                // 禁用升级按钮和相关子对象
                DisableUnnecessaryComponents(editTeamDefender);
            }
            else
            {
                Debug.LogWarning($"Defender prefab not found for: {defenderName}");
            }
        }

        ArrangeDefenders();
        ArrangeEditTeamDefenders();
    }





    // 清空 PlayerPrefs 的方法
    public void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll(); // 清空所有 PlayerPrefs 数据
        PlayerPrefs.Save();
        Debug.Log("PlayerPrefs has been cleared.");

        // 清空现有的 defender，并重新加载默认 defender
        ClearDefenderList();
    }

    private void OnDisable()
    {
        SaveDefenderOrder();
    }

    [System.Serializable]
    public class SerializableList<T>
    {
        public List<T> list;
        public SerializableList(List<T> list)
        {
            this.list = list;
        }
    }

    // Coroutine to change the DefenderPic image to a coin image after a delay
    private IEnumerator ChangeToCoinImage(GameObject card)
    {
        // wait for 1 second
        yield return new WaitForSeconds(2f);

        // 检查 card 是否已经被销毁
        if (card == null)
        {
            Debug.LogWarning("Card object has been destroyed. Exiting coroutine.");
            yield break; // 提前退出协程
        }

        // Find the DefenderPic child and change its image to a coin image
        Transform defenderPic = card.transform.Find("DefenderPic");
        if (defenderPic != null)
        {
            Image defenderPicImage = defenderPic.GetComponent<Image>();
            if (defenderPicImage != null)
            {
                Sprite coinSprite = Resources.Load<Sprite>("coinSprite");
                if (coinSprite != null)
                {
                    Debug.Log("Coin image loaded");  // Confirm coin sprite is loaded
                    defenderPicImage.sprite = coinSprite;

                    // Create "+50" text
                    GameObject bonusText = new GameObject("BonusText");
                    bonusText.transform.SetParent(defenderPic, false);

                    // Set postion just below the coin image
                    RectTransform rectTransform = bonusText.AddComponent<RectTransform>();
                    rectTransform.anchoredPosition = new Vector2(60, -160); // Adjust Y value to position below

                    // Add Text Component
                    TextMeshProUGUI text = bonusText.AddComponent<TextMeshProUGUI>();
                    text.text = "+50";
                    text.fontSize = 54;
                    text.color = Color.white;
                    text.fontStyle = FontStyles.Bold;

                    // Assign Roboto-Bold font
                    text.font = Resources.Load<TMP_FontAsset>("Fonts/Roboto-Bold SDF");

                    // Add outline effect
                    text.outlineWidth = 0.3f; // Adjust the outline width as needed
                    text.outlineColor = Color.black;


                }
            }
        }
    }

    private void DisableUnnecessaryComponents(GameObject defender)
    {
        // 禁用升级按钮
        Button upgradeButton = defender.GetComponentInChildren<Button>();
        if (upgradeButton != null)
        {
            upgradeButton.interactable = false;
            upgradeButton.gameObject.SetActive(false);
        }

        // 禁用特定子对象
        string[] childNamesToDisable = new string[] {
        "Square", "AttackPower", "Health", "Level", "LevelTitle", "IconSword", "IconHeart", "Name", "IconLV"
    };

        foreach (string childName in childNamesToDisable)
        {
            Transform child = defender.transform.Find(childName);
            if (child != null)
            {
                child.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning($"Child object {childName} not found in {defender.name}");
            }
        }
    }

    // 检查edit team容器中是否有同名的 defender
    private bool HasDefenderWithName(Transform container, string defenderName)
    {
        foreach (Transform child in container)
        {
            if (child.name == defenderName || child.name == defenderName + "(Clone)")
            {
                return true; // 找到同名 defender
            }
        }
        return false;  // 未找到同名 defender
    }

    // 检查Drop target panel容器中是否有同名的 defender
    private bool HasDefenderInPanels(Transform[] panels, string defenderName)
    {
        foreach (Transform panel in panels)
        {
            foreach (Transform child in panel)
            {
                if (child.name == defenderName || child.name == defenderName + "(Clone)")
                {
                    return true; // 找到同名 defender
                }
            }
        }
        return false; // 所有 panel 中都未找到同名 defender
    }


    private void DisableRaycast(GameObject defender)
    {
        CanvasGroup canvasGroup = defender.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = defender.AddComponent<CanvasGroup>();
        }
        canvasGroup.blocksRaycasts = false; // 禁用 Raycast
    }

    // 启用 Raycast
    private void EnableRaycast(GameObject defender)
    {
        CanvasGroup canvasGroup = defender.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true; // 启用 Raycast
        }
    }
}

