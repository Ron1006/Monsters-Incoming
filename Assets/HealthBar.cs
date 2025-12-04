using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider healthSlider;
    public Slider easeHealthSlider;
    public int maxHealth = 100;
    public int health;
    private int easeHealth;
    private float lerpSpeed = 0.02f;

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        easeHealth = health;
    }

    // Update is called once per frame
    void Update()
    {
        if(healthSlider.value != health)
        {
            healthSlider.value = health;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            takeDamage(10);
        }

        if(healthSlider.value != easeHealthSlider.value)
        {
            easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, health, lerpSpeed);
        }
    }

    void takeDamage (int damage)
    {
        health -= damage;
        easeHealth -= damage;
        Debug.Log($"get damage = {damage}");
    }
}
