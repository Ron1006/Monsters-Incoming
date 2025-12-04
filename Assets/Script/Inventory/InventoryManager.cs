using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; } //单例模式

    private int coinInventory;
    private int gemInventory;

    public TMP_Text lootQuantites; // 显示当前金币总数的文本
    public TMP_Text gemQuantites; // 显示当前gem总数的文本
    public TMP_Text gemQuantitesForDraw; // 显示当前gem总数的文本

    public TMP_Text rewardQuantites; // 过关获得的奖励金币
    public TMP_Text rewardGemsQuantites; // 过关获得的gem

    private int currentCoin = 0;
    public TMP_Text currentLoot; // 显示关卡结束后金币获取数量的文本

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple InventoryManager instances found! Destroying extra instance.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadFromJson();

    }

    // **添加金币或宝石**
    public void AddItem(int quantity, string type)
    {
        if (type == "Coin")
        {
            coinInventory += quantity;
            lootQuantites.text = FormatNumber(coinInventory);

            currentCoin += quantity;
            if(currentLoot != null)
            {
                currentLoot.text = currentCoin.ToString();
            }
        }
        else if(type == "Gem")
        {
            gemInventory += quantity;
            gemQuantites.text = FormatNumber(gemInventory);
            gemQuantitesForDraw.text = FormatNumber(gemInventory);
        }
        else
        {
            Debug.LogError("Invalid currency type: " + type);
            return;
        }

        SaveToJson();
    }

    // **一次性增加多个奖励（同时获取 Coin 和 Gem）**
    public void AddRewards(Dictionary<string, int> rewards)
    {
        foreach (var reward in rewards)
        {
            if(reward.Key == "Coin")
            {
                coinInventory += reward.Value;
                rewardQuantites.text = FormatNumber(reward.Value);
            }
            else if (reward.Key == "Gem")
            {
                gemInventory += reward.Value;
                rewardGemsQuantites.text = FormatNumber(reward.Value);
            }
            else
            {
                Debug.LogError("Invalid currency type: " + reward.Key);
                continue;
            }
        }
        SaveToJson();
    }

    public void DoubleLoot()
    {
        int doubleLoot = currentCoin * 2;
        coinInventory += doubleLoot - currentCoin; // 只加上额外部分
        currentCoin = doubleLoot;  // 确保 currentCoin 也更新

        // 更新 UI 显示（如果有相关 UI）
        if (currentLoot != null)
        {
            currentLoot.text = currentCoin.ToString();
        }

        // 保存到 JSON，确保数据不会丢失
        SaveToJson();
    }

    // 保存到 JSON 文件的方法
    private void SaveToJson()
    {
        // 创建一个新的 CoinData 列表
        List<CurrencyData> currencyList = new List<CurrencyData>
        {
            new CurrencyData { name = "Coin", quantity = coinInventory },
            new CurrencyData { name = "Gem", quantity = gemInventory }
        };

        // 调用 CoinDataManager 保存数据
        CurrencyDataManager.Instance.SaveCurrencyData(currencyList);
    }


    // 获取当前金币数量
    public int GetCurrencyAmount(string type)
    {
        if(type == "Coin") return coinInventory;
        if(type == "Gem") return gemInventory;

        Debug.LogError("Invalid currency type: " + type);
        return 0;
    }

    // 更新 UI 的方法
    void UpdateUI(int coins)
    {
        lootQuantites.text = FormatNumber(coins); // 使用 FormatNumber 来格式化显示
    }

    // 数字格式化方法，例如 1k, 1.5k, 1m
    private string FormatNumber(int num)
    {
        if (num >= 1000000)
            return (num / 1000000f).ToString("0.0") + "m";
        else if (num >= 1000)
            return (num / 1000f).ToString("0.0") + "k";
        else
            return num.ToString();
    }

    private void LoadFromJson()
    {
        List<CurrencyData> currencyDataList = CurrencyDataManager.Instance.LoadCurrencyData();
        
        if(currencyDataList != null)
        {
            foreach (var currency in currencyDataList)
            {
                if (currency.name == "Coin")
                {
                    coinInventory = currency.quantity;
                    lootQuantites.text = FormatNumber(coinInventory);
                }
                else if (currency.name == "Gem")
                {
                    gemInventory = currency.quantity;
                    gemQuantites.text = FormatNumber(gemInventory);
                }
            }
        }
        else
        {
            coinInventory = 0;
            gemInventory = 1000; // 默认初始1000宝石

            // **更新 UI**
            gemQuantites.text = FormatNumber(gemInventory);
            gemQuantitesForDraw.text = FormatNumber(gemInventory);
        }
    }

    // 检查 Gem和coin 是否足够
    public bool CanAfford(int amount, string type)
    {
        if (type == "Gem")
        {
            return gemInventory >= amount;
        }
        else if (type == "Coin")
        {
            return coinInventory >= amount;
        }

        Debug.LogError("Invalid currency type: " + type);
        return false;
    }
}
