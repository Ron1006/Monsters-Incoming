using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentButton : MonoBehaviour
{
    public Canvas equipmentCanva;
    public Image equipmentDefenderPic; // 需要替换的目标图片
    private string defenderName; // 存储当前 Defender 的名称
    private Defender assignedDefender; // 绑定的 Defender

    void Start()
    {
        // 查找名为 "EquipmentCanva" 的 GameObject 并获取 Canvas 组件
        GameObject upgradeCanva = GameObject.Find("UpgradeCanva");
        if (upgradeCanva != null)
        {
            Transform equipmentTransform = upgradeCanva.transform.Find("EquipmentCanva");
            if (equipmentTransform != null)
            {
                equipmentCanva = equipmentTransform.GetComponent<Canvas>();

                // **查找 EquipmentCanva 下的 DefenderPic**
                Transform defenderPicTransform = equipmentTransform.Find("DefenderBackground/DefenderPic");
                if (defenderPicTransform != null)
                {
                    equipmentDefenderPic = defenderPicTransform.GetComponent<Image>();
                    //Debug.Log("DefenderPic found: " + equipmentDefenderPic.gameObject.name);
                }
                else
                {
                    Debug.LogError("DefenderPic not found in EquipmentCanva!");
                }

            }
            else
            {
                Debug.LogError("EquipmentCanva not found under UpgradeCanva!");
            }
        }
        else
        {
            Debug.LogError("EquipmentCanva GameObject not found! Make sure it exists in the scene.");
        }

        


        if (equipmentCanva != null)
        {
            equipmentCanva.gameObject.SetActive(false); // 确保初始状态关闭
        }

        // **获取绑定的 Defender**
        assignedDefender = GetComponentInParent<Defender>(); // 直接获取 Defender 组件
        if (assignedDefender != null)
        {
            defenderName = assignedDefender.defenderName;
        }
    }

    public void OnClick()
    {
        if (equipmentCanva == null)
        {
            Debug.LogError("EquipmentCanva is NULL! Cannot open the equipment UI.");
            return;
        }

        if (assignedDefender == null)
        {
            Debug.LogError("assignedDefender is NULL! Cannot set current defender.");
            return;
        }

        if (EquipmentScrollView.Instance == null)
        {
            Debug.LogError("EquipmentScrollView.Instance is NULL!");
            return;
        }

        if (EquipmentButtonManager.Instance == null)
        {
            Debug.LogError("EquipmentButtonManager.Instance is NULL!");
            return;
        }
        if (equipmentCanva != null && assignedDefender != null)
        {
            //Debug.Log($"[EQUIPMENT] 按钮点击，当前 Defender: {assignedDefender.defenderName}");

            // **清空旧的装备列表**
            EquipmentScrollView.Instance.ClearEquipmentList();

            // **设置当前 Defender**
            EquipmentButtonManager.Instance.SetCurrentDefender(assignedDefender);

            equipmentCanva.gameObject.SetActive(true);

            

            // **更新 Defender 图片**
            UpdateDefenderImage();

        }
        else
        {
            Debug.LogError("EquipmentCanva 或 assignedDefender 为空，无法打开装备界面！");
        }


    }
    private void UpdateDefenderImage()
    {
        Transform defenderPicTransform = transform.parent.Find("DefenderPic");
        if (defenderPicTransform != null)
        {
            Image defenderPicImage = defenderPicTransform.GetComponent<Image>();
            if (defenderPicImage != null && equipmentDefenderPic != null)
            {
                equipmentDefenderPic.sprite = defenderPicImage.sprite;
                //Debug.Log("DefenderPic updated: " + defenderPicImage.sprite.name);
            }
        }
        else
        {
            Debug.LogError("DefenderPic not found in the same parent!");
        }
    }

}
