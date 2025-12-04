using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeedSwitch : MonoBehaviour
{
    public Button speedButton; // 绑定 UI 按钮
    public Sprite normalSpeedSprite; // 1x 速度的图片
    public Sprite fastSpeedSprite; // 1.5x 速度的图片
    private Image buttonImage;

    void Start()
    {
        buttonImage = speedButton.GetComponent<Image>(); // 获取按钮的 Image 组件
        speedButton.onClick.AddListener(OnSpeedSwitchClicked); // 绑定点击事件
    }

    void OnSpeedSwitchClicked()
    {
        if (Time.timeScale == 1)
        {
            Time.timeScale = 1.5f;
            buttonImage.sprite = fastSpeedSprite; // 切换到 1.5x 速度的图片
        }
        else
        {
            Time.timeScale = 1;
            buttonImage.sprite = normalSpeedSprite; // 切换回 1x 速度的图片
        }
    }
}
