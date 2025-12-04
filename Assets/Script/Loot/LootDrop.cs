using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LootDrop : MonoBehaviour
{
    public int quantity;
    public InventoryManager inventoryManager;
    
    // Start is called before the first frame update
    void Start()
    {
        inventoryManager = GameObject.Find("InventoryManager").GetComponent<InventoryManager>();
        inventoryManager.AddItem(quantity, "Coin");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
