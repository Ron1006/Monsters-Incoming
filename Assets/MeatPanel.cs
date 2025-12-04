using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MeatPanel : MonoBehaviour
{
    public Image loadingPanel;           // 加载进度面板
    public TMP_Text meatText;            // 显示肉数量的文本
    public Image meatPIC;                // 不同等级的图片
    public int meatQuantityGame;         // 游戏实时的食物数量


    private RectTransform loadingRect;   // 控制加载进度条
    private float maxWidth = 178f;       // 进度条的最大宽度

    private float timer = 0f;            // 计时器


    void Start()
    {
        meatQuantityGame = MeatManager.Instance.meatQuantity;

        // 自动查找组件
        loadingPanel = transform.Find("Loading").GetComponent<Image>();
        meatText = transform.Find("MeatText").GetComponent<TMP_Text>();
        meatPIC = transform.Find("MeatPIC").GetComponent<Image>();

        UpdateMeatSprite();    // 更新对应的图片

        loadingRect = loadingPanel.GetComponent<RectTransform>();
        loadingRect.anchorMin = new Vector2(0, 0.5f);
        loadingRect.anchorMax = new Vector2(0, 0.5f);
        loadingRect.pivot = new Vector2(0, 0.5f);

        UpdateMeatText();
    }

    // Update is called once per frame
    void Update()
    {
        if (MeatManager.Instance == null) return;

        UpdateMeatText();

        timer += Time.deltaTime * MeatManager.Instance.loadingSpeed;

        float newWidth = timer * maxWidth;
        loadingRect.sizeDelta = new Vector2(newWidth, loadingRect.sizeDelta.y);

        if (loadingRect.sizeDelta.x >= maxWidth)
        {
            meatQuantityGame++;
            timer = 0f;
            loadingRect.sizeDelta = new Vector2(0, loadingRect.sizeDelta.y);
        }
    }

    //更新显示肉的数量
    void UpdateMeatText()
    {


        if (meatText != null && MeatManager.Instance != null)
            meatText.text = meatQuantityGame.ToString();
    }

    // 更新meat图片
    void UpdateMeatSprite()
    {
        int level = MeatManager.Instance.level;

        meatPIC.sprite = MeatManager.Instance.GetCurrentMeatSprite();
    }
}
