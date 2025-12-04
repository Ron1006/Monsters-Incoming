using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MapSceneCoinDisplay : MonoBehaviour
{
    public TMP_Text lootQuantities; // coin总数
    public TMP_Text gemQuantities; // gem 总数

    void Start()
    {
        // 加载 Coin 和 Gem 的数量
        Dictionary<string, int> currencyData = LoadCurrencyFromJson();

        // 更新 UI
        UpdateUI(currencyData["Coin"], currencyData["Gem"]);
    }

    // 从 JSON 文件加载金币数量
    private Dictionary<string, int> LoadCurrencyFromJson()
    {
        Dictionary<string, int> currencyValues = new Dictionary<string, int>
        {
            { "Coin", 0 },  // 默认值
            { "Gem", 0 }
        };

        // 调用 CurrencyDataManager 加载数据
        List<CurrencyData> currencyList = CurrencyDataManager.Instance.LoadCurrencyData();

        if (currencyList != null)
        {
            foreach (var currency in currencyList)
            {
                if (currencyValues.ContainsKey(currency.name))
                {
                    currencyValues[currency.name] = currency.quantity;
                }
            }
        }
        else
        {
            Debug.LogWarning("No currency data found in JSON. Returning defaults.");
        }

        return currencyValues;
    }

    // 更新 UI 方法，分别显示金币和宝石数量
    void UpdateUI(int coins, int gems)
    {
        lootQuantities.text = FormatNumber(coins);
        gemQuantities.text = FormatNumber(gems);
    }

    // 数字格式化方法，例如 1k, 1.5k, 1m
    private string FormatNumber(int num)
    {
        if (num >= 1000000)
            return (num / 1000000f).ToString("0.#") + "m";
        else if (num >= 1000)
            return (num / 1000f).ToString("0.#") + "k";
        else
            return num.ToString();
    }
}
