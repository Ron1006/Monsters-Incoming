using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.UI;


public class TurntableController : MonoBehaviour
{
    public RectTransform turntable; // 转盘的 RectTransform
    public Button spinButton; // Spin 按钮
    public TMPro.TextMeshProUGUI spinsLeftText; // 显示剩余次数
    //public TMPro.TextMeshProUGUI spinsLeftTextButton; // 显示剩余次数
    public string cooldownText; 
    public int maxSpins = 120; // 最大旋转次数
    public int spinsLeft; // 当前剩余旋转次数

    public float spinDuration = 4f; // 基础旋转时间
    public float randomDurationRange = 1f; // 旋转时间随机范围
    public float maxSpeed = 800f; // 最大旋转速度
    public float randomSpeedRange = 200f; // 旋转速度随机范围

    public InventoryManager inventoryManager;

    public List<int> coinRewards = new List<int> { 0, 500, 100, 1000, 500, 100, 100, 100 }; // 奖品对应的金币数量


    private bool isSpinning = false;

    public LotterySystem lotterySystem;

    public Button closeButton; // 关闭抽奖弹窗
    public GameObject fortuneWheel;
    public GameObject prizeMoney; // 抽中金币的动画
    public GameObject prizeGem; // 抽中Gem的动画
    public GameObject prizeDefender; // 抽中defender的动画
    public TMPro.TextMeshProUGUI prizeMoneyAmount; // 抽中金币的数量
    public TMPro.TextMeshProUGUI prizeGemAmount; // 抽中金币的数量

    private void Start()
    {
        //ResetSpins(); // 测试用

        //最终代码
        if (!PlayerPrefs.HasKey("LastResetDate"))
        {
            Debug.Log("首次启动游戏，初始化抽奖次数");
            ResetSpins();
        }
        else
        {
            Debug.Log("检查是否需要重置抽奖次数");
            CheckReset();

            // 如果有保存的次数，加载它
            spinsLeft = PlayerPrefs.GetInt("SpinsLeft", maxSpins);
            // 确保状态正确
            if (spinsLeft <= 0)
            {
                spinButton.interactable = false;
            }
            else
            {
                spinButton.interactable = true;
            }

            UpdateSpinsLeftText();
        }

        spinButton.onClick.AddListener(StartSpin);
        closeButton.onClick.AddListener(CloseFortuneWheel);
    }

    

    private void Update()
    {
        // 每帧更新倒计时显示
        UpdateCooldownText();
    }

    private void CheckReset()
    {
        // 从 PlayerPrefs 获取保存的日期
        string lastResetDate = PlayerPrefs.GetString("LastResetDate", DateTime.MinValue.ToString("yyyy-MM-dd"));
        DateTime lastReset;

        // 支持的日期格式数组
        string[] formats = { "yyyy-MM-dd", "dd/MM/yyyy" };

        // 尝试解析日期
        bool parseSuccess = DateTime.TryParseExact(lastResetDate, formats, null, System.Globalization.DateTimeStyles.None, out lastReset);

        if (!parseSuccess)
        {
            Debug.LogError($"日期解析失败，重置为默认日期：{lastResetDate}");
            lastReset = DateTime.MinValue;
        }

        DateTime today = DateTime.Today;

        // 检查是否需要重置
        if (lastReset.Date < today)
        {
            ResetSpins();
        }
    }

    private void ResetSpins()
    {
        spinsLeft = maxSpins;
        PlayerPrefs.SetInt("SpinsLeft", spinsLeft); // 保存重置后的次数

        // 保存日期
        PlayerPrefs.SetString("LastResetDate", DateTime.Today.ToString("yyyy-MM-dd"));
        PlayerPrefs.Save();

        UpdateSpinsLeftText();
        cooldownText = "";
        spinButton.interactable = true;
    }


    public void UpdateCooldownText()
    {
        // 计算距离下一次重置的剩余时间
        DateTime now = DateTime.Now;
        DateTime nextReset = DateTime.Today.AddDays(1); // 次日0点

        TimeSpan timeLeft = nextReset - now;

        // 检查是否需要跨天重置
        if (PlayerPrefs.HasKey("LastResetDate"))
        {
            string lastResetDate = PlayerPrefs.GetString("LastResetDate", DateTime.MinValue.ToString("yyyy-MM-dd"));
            DateTime lastReset;

            // 尝试解析日期
            if (DateTime.TryParseExact(lastResetDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out lastReset))
            {
                if (lastReset.Date < DateTime.Today)
                {
                    ResetSpins(); // 跨天直接重置次数
                    return;
                }
            }
        }

        // 如果次数用完，则进入倒计时逻辑
        if (spinsLeft <= 0)
        {
            int hours = timeLeft.Hours;
            int minutes = timeLeft.Minutes;
            int seconds = timeLeft.Seconds;

            // 如果次数为0，显示倒计时
            cooldownText = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
            spinsLeftText.text = cooldownText;
            //spinsLeftTextButton.text = cooldownText;
            spinButton.interactable = false;
        }
        else
        {
            spinButton.interactable = true; // 启用按钮
            UpdateSpinsLeftText();
        }
    }


    public void UpdateSpinsLeftText()
    {
        spinsLeftText.text = $"SPINS LEFT: {spinsLeft}/{maxSpins}";
        //spinsLeftTextButton.text = $"{spinsLeft} / {maxSpins}";
    }

    public void StartSpin()
    {
        if (!isSpinning && spinsLeft > 0)
        {
            // 禁用关闭按钮
            closeButton.interactable = false;

            // 重置转盘角度
            turntable.eulerAngles = new Vector3(0f, 0f, 0f);

            // 减少剩余次数
            spinsLeft--;
            PlayerPrefs.SetInt("SpinsLeft", spinsLeft); // 每次抽奖后更新保存次数
            PlayerPrefs.Save();

            UpdateSpinsLeftText();

            // 开始旋转逻辑
            StartCoroutine(SpinCoroutine());
        }
    }

    private IEnumerator SpinCoroutine()
    {
        isSpinning = true;

        // 随机化旋转时间和速度
        float spinTime = spinDuration + UnityEngine.Random.Range(-randomDurationRange, randomDurationRange);
        float currentSpeed = maxSpeed + UnityEngine.Random.Range(-randomSpeedRange, randomSpeedRange);

        float elapsedTime = 0f; // 转动的时间

        while (elapsedTime < spinTime)
        {
            float deceleration = currentSpeed * (elapsedTime / spinTime);
            float speed = currentSpeed - deceleration;

            turntable.Rotate(0f, 0f, speed * Time.deltaTime);
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // 确定转盘停下的角度
        float finalAngle = turntable.eulerAngles.z % 360; // 当前转盘的停止角度

        // 假设第一个奖品的中间角度为偏移角度
        float offsetAngle = 360f / coinRewards.Count / 2f; // 每个奖品占用的角度一半
        float adjustedAngle = (finalAngle + offsetAngle) % 360; // 调整角度，加上偏移量
        Debug.Log($"最终角度: {finalAngle}, 调整后角度: {adjustedAngle}");

        // 根据调整后的角度计算奖品索引
        int prizeIndex = Mathf.FloorToInt(adjustedAngle / (360f / coinRewards.Count));
        Debug.Log($"中奖奖品索引: {prizeIndex}");

        // 执行奖品逻辑
        HandlePrize(prizeIndex);

        isSpinning = false;

        // 启用关闭按钮
        closeButton.interactable = true;
    }


    private void HandlePrize(int prizeIndex)
    {
        //lotterySystem.DrawDefender();
        if (prizeIndex == 0)
        {
            prizeDefender.SetActive(true);
            lotterySystem.DrawDefender();
        }
        else if (prizeIndex == 2 || prizeIndex == 7)
        {
            prizeGem.SetActive(true);

            StartCoroutine(HidePrizeGemAfterDelay(2f)); // 启用协程，延迟2秒后隐藏
            // 增加对应的金币数量
            int gem = coinRewards[prizeIndex];

            prizeGemAmount.text = gem.ToString();

            inventoryManager.AddItem(gem, "Gem"); // 增加金币
            Debug.Log($"获得 {gem} 宝石！");
        }
        else
        {


            prizeMoney.SetActive(true);

            StartCoroutine(HidePrizeMoneyAfterDelay(2f)); // 启用协程，延迟2秒后隐藏
            // 增加对应的金币数量
            int coins = coinRewards[prizeIndex];

            prizeMoneyAmount.text = coins.ToString();

            inventoryManager.AddItem(coins, "Coin"); // 增加金币
            Debug.Log($"获得 {coins} 金币！");
        }
    }

    private IEnumerator HidePrizeMoneyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); //等待指定秒数
        prizeMoney.SetActive(false);
    }

    private IEnumerator HidePrizeGemAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); //等待指定秒数
        prizeGem.SetActive(false);
    }

    private void CloseFortuneWheel()
    {
        fortuneWheel.SetActive(false);
        
    }


}
