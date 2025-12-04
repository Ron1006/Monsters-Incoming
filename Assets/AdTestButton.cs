using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdTestButton : MonoBehaviour
{
    public Button adButton;
    public float timeoutSeconds = 999f; // 广告加载最大等待时间

    //private Coroutine loadCoroutine;

    void Start()
    {
        adButton.onClick.AddListener(() => { StartCoroutine(LoadAndShowRewardedAd()); });
    }

    IEnumerator LoadAndShowRewardedAd()
    {
        Debug.Log("Start loading rewarded ad...");

        bool adLoaded = false;

        AdManager.Instance.LoadRewardedAd(() =>
        {
            Debug.Log("Ad callback");
            adLoaded = true;
        });

        float elapsed = 0f;
        while (!adLoaded && elapsed < timeoutSeconds)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (adLoaded && AdManager.Instance.IsRewardedAdReady())
        {
            Debug.Log("Ad loaded, now showing.");
            AdManager.Instance.ShowRewardedAd(OnRewardEarned);
        }
        else
        {
            Debug.LogWarning("Ad failed to load within timeout.");
        }

    }

    void OnRewardEarned()
    {
        Debug.Log("Rewarded ad watched.");
    }
}
