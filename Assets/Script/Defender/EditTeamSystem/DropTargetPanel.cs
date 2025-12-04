using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using TMPro;


public class DropTargetPanel : MonoBehaviour, IDropHandler
{
    public TextMeshProUGUI panelText; // 用于显示拖入的 Defender 名字
    public DraggableDefender currentDefender; // 当前再panel里的defender
    public Transform containerTransform; // Defender Scroll View 的 Container

    private void Awake()
    {

        PrintPanelMapping();
        // 如果没有手动分配 panelText，自动尝试查找
        if (panelText == null)
        {
            panelText = GetComponentInChildren<TextMeshProUGUI>();
        }

        if (panelText == null)
        {
            Debug.LogError("PanelText is missing! Ensure a TextMeshProUGUI component is attached as a child.");
        }

        if (containerTransform == null)
        {
            Debug.LogError("ContainerTransform is missing! Assign the correct container in the Inspector.");
        }


        // 恢复保存的防御者
        RestoreDefender();

    }

    private void Start()
    {
        StartCoroutine(DelayedFindDefender());
    }

    private IEnumerator DelayedFindDefender()
    {
        yield return new WaitForSeconds(0.5f); // 延迟 0.5 秒执行

        // **遍历当前 Panel（this.transform）下的所有子对象**
        foreach (Transform child in transform)
        {
            DraggableDefender defender = child.GetComponent<DraggableDefender>();
            if (defender != null)
            {
                currentDefender = defender;
                //Debug.Log($"[INFO] currentDefender set to {currentDefender.defenderName} in {gameObject.name}");
                break; //  只获取第一个找到的 Defender
            }
            else
            {
                //Debug.Log($"没有找到defender");
            }
        }
    }


    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop triggered for Panel.");

        if (eventData.pointerDrag == null)
        {
            Debug.Log("No object is being dragged.");
            return;
        }

        // 获取被拖拽的对象
        GameObject draggedObject = eventData.pointerDrag;

        // 检查拖拽对象是否有 DraggableDefender 组件
        DraggableDefender draggableDefender = draggedObject.GetComponent<DraggableDefender>();
        if (draggableDefender == null)
        {
            Debug.Log("Dragged object does not have a DraggableDefender component.");
            return;
        }

        //替换现有 Defender
        if (currentDefender != null)
        {
            Debug.Log($"Replacing existing Defender: {currentDefender.defenderName}");

            // 清空当前面板的文本和 PlayerPrefs 数据
            ClearPanelText();

            ReturnDefenderToContainer(currentDefender);
        }

        // 更新当前 Defender 并绑定事件
        currentDefender = draggableDefender;
        currentDefender.OnDefenderRemoved += ClearPanelText;

        // 获取防御者的名字并显示在 Panel 的文本中
        string defenderName = draggableDefender.GetDefenderName();
        if (panelText != null)
        {
            panelText.text = defenderName;
        }

        // 将拖动对象设置为目标面板的子对象
        draggedObject.transform.SetParent(transform);

        // 设置为顶层（调整层级）
        draggedObject.transform.SetAsLastSibling();

        // 更新拖拽对象的属性
        CanvasGroup canvasGroup = draggedObject.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true; // 恢复射线检测
            Debug.Log("draggedObject: " + draggedObject + canvasGroup.blocksRaycasts);
        }

        // 居中对象
        RectTransform draggedRect = draggedObject.GetComponent<RectTransform>();
        if (draggedRect != null)
        {
            draggedRect.anchoredPosition = Vector2.zero;
        }
        else
        {
            draggedObject.transform.localPosition = Vector3.zero; // 如果没有 RectTransform 则使用 LocalPosition
        }
        Debug.Log($"Object {defenderName} successfully dropped onto {gameObject.name}.");


        SaveDefender(defenderName);
    }

    private void SaveDefender(string defenderName)
    {
        PlayerPrefs.SetString(gameObject.name, defenderName);
        PlayerPrefs.Save();
        Debug.Log($"Saved {gameObject.name}: {defenderName} to PlayerPrefs.");
    }

    private void ReturnDefenderToContainer(DraggableDefender defender)
    {
        if (containerTransform == null)
        {
            Debug.LogError("Container Transform is not set! Please assign it in the Inspector.");
            return;
        }


        // 移除事件绑定
        defender.OnDefenderRemoved -= ClearPanelText;

        // 禁用射线检测，避免遮挡
        CanvasGroup canvasGroup = defender.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
        }

        // 将 Defender 移回 Container 并重置位置
        defender.transform.SetParent(containerTransform);
        defender.transform.localPosition = Vector3.zero;

        Debug.Log($"Defender {defender.defenderName} returned to Container.");

        // 恢复射线检测
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }
    }

    private void ClearPanelText()
    {
        if (panelText != null)
        {
            panelText.text = string.Empty;
            //Debug.Log("Panel text cleared.");
        }

        // 删除当前面板对应的 PlayerPrefs 数据
        PlayerPrefs.DeleteKey(gameObject.name);
        PlayerPrefs.Save();
        //Debug.Log($"Cleared PlayerPrefs for {gameObject.name}.");
    }

    private void RestoreDefender()
    {
        // 从 PlayerPrefs 加载对应的防御者名字
        string savedDefenderName = PlayerPrefs.GetString(gameObject.name, null);

        if (!string.IsNullOrEmpty(savedDefenderName))
        {
            //Debug.Log($"Restoring {gameObject.name} with Defender: {savedDefenderName}");

            // 加载 "DefenderUpgrade" 文件夹下的所有预制件
            GameObject[] prefabs = Resources.LoadAll<GameObject>("DefenderUpgrade");

            // 遍历找到匹配名字的 Prefab
            foreach (var prefab in prefabs)
            {
                if (prefab.name == savedDefenderName)
                {
                    // 实例化防御者并设置到面板
                    GameObject defenderInstance = Instantiate(prefab, transform);
                    currentDefender = defenderInstance.GetComponent<DraggableDefender>();

                    if (currentDefender != null)
                    {
                        currentDefender.OnDefenderRemoved += ClearPanelText;
                    }

                    // 禁用所有子对象
                    foreach (Transform child in defenderInstance.transform)
                    {
                        child.gameObject.SetActive(false);
                    }

                    // 启用 DefenderPic 对象
                    Transform defenderPic = defenderInstance.transform.Find("DefenderPic");
                    if (defenderPic != null)
                    {
                        defenderPic.gameObject.SetActive(true);
                        //Debug.Log($"Enabled DefenderPic for {savedDefenderName}.");
                    }
                    else
                    {
                        Debug.LogWarning($"DefenderPic not found for {savedDefenderName}.");
                    }

                    // 更新面板文本
                    if (panelText != null)
                    {
                        panelText.text = savedDefenderName;
                    }

                    // 设置布局
                    RectTransform rectTransform = defenderInstance.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        rectTransform.anchoredPosition = Vector2.zero;
                        rectTransform.localScale = Vector3.one;
                    }

                    //Debug.Log($"Defender {savedDefenderName} successfully restored to {gameObject.name}.");
                    return; // 找到并恢复后，结束方法
                }
            }

            Debug.LogWarning($"Prefab for defender {savedDefenderName} not found in DefenderUpgrade folder. Cannot restore.");
        }
        else
        {
            //Debug.Log($"No defender saved for {gameObject.name}.");
        }
    }






    public void PrintPanelMapping()
    {
        // 获取当前面板名称
        string panelName = gameObject.name;

        // 从 PlayerPrefs 获取对应的 Defender 名字
        string defenderName = PlayerPrefs.GetString(panelName, "None");

        // 打印结果
        //Debug.Log($"Panel: {panelName}, Defender: {defenderName}");
    }

    public void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll(); // 清空所有 PlayerPrefs 数据
        PlayerPrefs.Save(); // 保存更改
        Debug.Log("All PlayerPrefs have been cleared.");
    }


}
