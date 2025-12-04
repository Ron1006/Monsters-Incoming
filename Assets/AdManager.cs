using UnityEngine;
using GoogleMobileAds.Api;
using GoogleMobileAds;
using System;
using System.Collections;
using System.Collections.Generic;

static class ThreadUtil
{
    public static void RunOnMainThread(Action job) =>
        GoogleMobileAds.Common.MobileAdsEventExecutor.ExecuteInUpdate(job);
}

public class AdManager : MonoBehaviour
{
    public static AdManager Instance;

    //private string _adUnitId = "ca-app-pub-3940256099942544/5224354917"; // 测试激励广告 ID
    //private string _adUnitId = "ca-app-pub-1267483619973265/7267951623"; // 你的激励广告 ID
    [SerializeField] string _adUnitId = "ca-app-pub-1267483619973265/7267951623";

    RewardedAd _rewardedAd;
    bool _isLoading;      // 正在请求中？
    bool _hasInit;        // SDK 是否初始化完

    [SerializeField] string _bannerAdUnitId = "ca-app-pub-1267483619973265/3866395686";
    private BannerView _bannerView;

    //ca-app-pub-1267483619973265/3866395686 正式banner广告ID
    //ca-app-pub-3940256099942544/9214589741 测试banner 广告 ID



    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        // 先初始化 EventExecutor
        GoogleMobileAds.Common.MobileAdsEventExecutor.Initialize();

        MobileAds.Initialize(_ =>
        {
            ThreadUtil.RunOnMainThread(() =>
            {
                _hasInit = true;
                //Debug.Log($"[Ad] Using AdUnitID: '{_adUnitId}'");
                LoadRewardedAd();
                //LoadBannerAd();
            });


        });


    }

    //加载激励广告（RewardedAd）。这是你游戏中“广告加载”逻辑的核心部分
    public void LoadRewardedAd(Action onLoaded = null)
    {
        if (!_hasInit || _isLoading) return;
        _isLoading = true;

        Debug.Log($"[Ad] LoadRewardedAd() t={DateTime.Now:HH:mm:ss.fff}");

        RewardedAd.Load(_adUnitId, new AdRequest(), (ad, error) =>
        {
            ThreadUtil.RunOnMainThread(() =>
            {
                _isLoading = false;

                if (error != null || ad == null)
                {
                    Debug.LogError($"[Ad] Load FAILED code={error?.GetCode()} " +
                                   $"msg={error?.GetMessage()}");
                    return;
                }

                Debug.Log("[Ad] RewardedAd loaded OK");
                _rewardedAd = ad;
                RegisterCallbacks(_rewardedAd);
                onLoaded?.Invoke();
            });
        });

        //Debug.Log($"[Ad] Using AdUnitID: '{_adUnitId}'");
    }

    //展示激励广告并在观看完成后给予奖励
    public void ShowRewardedAd(Action onRewardEarned, Action onNotReady = null)
    {
        Debug.Log($"[Ad] Show? ready={_rewardedAd != null}, " +
              $"canShow={_rewardedAd?.CanShowAd()}, loading={_isLoading}");

        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            _rewardedAd.Show(reward =>
            {
                ThreadUtil.RunOnMainThread(() => onRewardEarned?.Invoke());
            });
        }
        else
        {
            Debug.LogWarning("Ad not ready");
            onNotReady?.Invoke();
        }
    }

    /* ---------- 回调注册 ---------- */
    void RegisterCallbacks(RewardedAd ad)
    {
        ad.OnAdFullScreenContentClosed += () =>
            ThreadUtil.RunOnMainThread(() =>
            {
                Debug.Log("Ad closed → preload next");
                _rewardedAd.Destroy();
                _rewardedAd = null;
                LoadRewardedAd();
            });

        ad.OnAdFullScreenContentFailed += err =>
            ThreadUtil.RunOnMainThread(() =>
            {
                Debug.LogError($"Show failed: {err}");
                _rewardedAd = null;
                LoadRewardedAd();
            });

        // 如需统计点击/曝光/付费，可在此继续添加其它事件
    }

    // 判断广告是否准备好
    public bool IsRewardedAdReady() =>
        _rewardedAd != null && _rewardedAd.CanShowAd();


    // banner AD
    public void LoadBannerAd()
    {
        if (!_hasInit) return; // ✅ 防止未初始化就加载

        if (_bannerView != null) { _bannerView.Destroy(); _bannerView = null; }

        // 适配尺寸banner代码
        //AdSize adaptiveSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
        //_bannerView = new BannerView(_bannerAdUnitId, adaptiveSize, AdPosition.Bottom);

        // 固定尺寸banner代码
        AdSize size = AdSize.Banner; // 或 AdSize.MediumRectangle, AdSize.IABBanner 等
        _bannerView = new BannerView(_bannerAdUnitId, size, AdPosition.BottomRight);

        // 设置监听器
        _bannerView.OnBannerAdLoaded += () => Debug.Log("[Ad] Banner Loaded");
        _bannerView.OnBannerAdLoadFailed += (error) => Debug.LogError("[Ad] Banner Load Failed: " + error.GetMessage());
        _bannerView.OnAdFullScreenContentOpened += () => Debug.Log("[Ad] Banner Opened");
        _bannerView.OnAdFullScreenContentClosed += () => Debug.Log("[Ad] Banner Closed");
        _bannerView.OnAdPaid += (value) => Debug.Log("[Ad] Banner Paid Event");

        // 加载广告
        AdRequest request = new AdRequest();
        _bannerView.LoadAd(request);
    }

    public void ShowBanner()
    {
        _bannerView?.Show();
    }

    public void HideBanner()
    {
        _bannerView?.Hide();
    }

    public void DestroyBanner()
    {
        _bannerView?.Destroy();
        _bannerView = null;
    }
}
