using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    public TextMeshProUGUI damageText;

    // **显示伤害**
    public void SetDamage(int damage)
    {
        damageText.text = "-" + damage.ToString(); // 显示伤害数字
        //damageText.color = Color.red; // **红色**
        Destroy(gameObject, 1f); // 1 秒后销毁
    }

    // **显示治疗**
    public void SetHealing(int healAmount)
    {
        damageText.text = "+" + healAmount.ToString(); // 显示治疗数字
        damageText.color = Color.green; // **绿色**
        Destroy(gameObject, 1f); // 1 秒后销毁
    }
}
