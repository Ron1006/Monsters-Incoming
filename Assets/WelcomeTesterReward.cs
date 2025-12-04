using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WelcomeTesterReward : MonoBehaviour
{
    public GameObject welcomeCanvas;
    public InventoryManager inventoryManager;
    public int gemReward = 500;
    public int coinReward = 3000;
    public Button closeWelcomeCanvasButton;

    private const string hasReceivedKey = "TesterWelcomeGiven";

    void Start()
    {
        if (closeWelcomeCanvasButton != null)
        {
            closeWelcomeCanvasButton.onClick.AddListener(CloseCanvas);
        }

        if (!PlayerPrefs.HasKey(hasReceivedKey))
        {
            welcomeCanvas.SetActive(true);
            inventoryManager.AddItem(1000, "Gem");
            inventoryManager.AddItem(5000, "Coin");
            PlayerPrefs.SetInt(hasReceivedKey, 1); //Mark as rewarded
            PlayerPrefs.Save();
        }
        else
        {
            welcomeCanvas.SetActive(false);
        }
    }

    void CloseCanvas()
    {
        welcomeCanvas.SetActive(false);
    }
}
