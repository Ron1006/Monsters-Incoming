using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CurrencyDataManager : MonoBehaviour
{
    public static CurrencyDataManager Instance { get; private set; }
    public string jsonFileName = "currencies.json";
    public string FilePath => Path.Combine(Application.persistentDataPath, jsonFileName);
    private List<CurrencyData> cachedData;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        //Debug.Log("JSON File Path: " + FilePath);

        if (!File.Exists(FilePath))
        {
            Debug.LogWarning("JSON file not found. Creating a sample file.");
            CreateSampleFile();
        }
    }

    private void CreateSampleFile()
    {
        var sampleData = new CurrencyDataCollection
        {
            currencies = new List<CurrencyData>
            {
                new CurrencyData { name = "Coin", quantity = 10, type = "Coin" },
                new CurrencyData { name = "Gem", quantity = 10, type = "Gem" }
            }
        };
        SaveCurrencyData(sampleData.currencies);
    }

    public void SaveCurrencyData(List<CurrencyData> currencyList)
    {
        if (currencyList == null || currencyList.Count == 0)
        {
            Debug.LogWarning("Coin data list is empty. Nothing to save.");
            return;
        }

        try
        {
            var dataCollection = new CurrencyDataCollection { currencies = currencyList };
            string json = JsonUtility.ToJson(dataCollection, true);
            File.WriteAllText(FilePath, json);
            //Debug.Log("Saved JSON content: " + json);
            cachedData = new List<CurrencyData>(currencyList); // ¸üÐÂ»º´æ
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save coin data: " + e.Message);
        }
    }

    public List<CurrencyData> LoadCurrencyData()
    {
        if (cachedData != null) return cachedData;

        if (File.Exists(FilePath))
        {
            try
            {
                string json = File.ReadAllText(FilePath);
                //Debug.Log("Loaded JSON raw content: " + json);

                if (string.IsNullOrWhiteSpace(json))
                {
                    Debug.LogError("JSON file is empty or invalid.");
                    return null;
                }

                var dataCollection = JsonUtility.FromJson<CurrencyDataCollection>(json);
                if (dataCollection?.currencies == null)
                {
                    Debug.LogError("Failed to parse JSON data.");
                    return null;
                }

                cachedData = new List<CurrencyData>(dataCollection.currencies);
                return cachedData;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load currency data: " + e.Message);
                return null;
            }
        }
        else
        {
            Debug.LogError("JSON file not found.");
            return null;
        }
    }
}
