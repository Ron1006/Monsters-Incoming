using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BannerController : MonoBehaviour
{
    void Start()
    {
        
    }

    void OnDestroy()
    {
        // 页面退出时可选隐藏 Banner
        if (AdManager.Instance != null)
        {
            AdManager.Instance.HideBanner();
        }
    }

    void OnEnable()
    {
        if (AdManager.Instance != null)
        {
            AdManager.Instance.LoadBannerAd(); // 强制重新加载 → 新展示
            Debug.Log("banner广告重新加载,并展示");
        }
    }
}
