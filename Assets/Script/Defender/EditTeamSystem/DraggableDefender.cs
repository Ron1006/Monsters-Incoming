using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class DraggableDefender : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform originalParent; // 记录原始父级容器
    private Vector3 originalPosition; // 记录原始位置
    private CanvasGroup canvasGroup;

    private Vector2 initialTouchPosition; // 初始触摸点
    //private bool isDragging = false; // 是否正在拖拽
    //private bool isScrolling = false; // 是否正在滑动

    public Action OnDefenderRemoved; // 定义回调事件

    public string defenderName; // 防御者的名字
    private void Awake()
    {

        //Debug.Log($"DraggableDefender {gameObject.name}, DefenderName: {defenderName}");

        // 确保对象有 CanvasGroup
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    void Start()
    {
        this.enabled = false; // 启动时禁用自己
    }

    public void OnBeginDrag(PointerEventData eventData)
    {

        // 记录原始父级和位置
        originalParent = transform.parent;
        originalPosition = transform.localPosition;

        // 禁用defender本身的射线检测，避免遮挡下方的object接收不到事件
        canvasGroup.blocksRaycasts = false;

        // 禁用场景中所有 Defender 的射线检测
        DisableAllDefenderRaycasts();

        // 通知父面板 Defender 已被移除, panel更新为空闲状态，允许接收其他defender, 同时可以更新界面，来清除defender name
        var dropTarget = originalParent.GetComponent<DropTargetPanel>();
        if (dropTarget != null && OnDefenderRemoved != null)
        {

            OnDefenderRemoved.Invoke();
        }

        Debug.Log($"Begin drag: {gameObject.name}, DefenderName: {defenderName}");
    }

    private void DisableAllDefenderRaycasts()
    {
        // 查找所有带有 CanvasGroup 的 Defender
        var defenders = FindObjectsOfType<DraggableDefender>();
        foreach (var defender in defenders)
        {
            var canvasGroup = defender.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
                Debug.Log($"Disabled raycasts for {defender.name}");
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 如果不是滑动，则执行拖拽逻辑
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            Vector2 localMousePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform.parent as RectTransform,  // 父级 RectTransform
                eventData.position,                     // 鼠标屏幕坐标
                eventData.pressEventCamera,             // 摄像机
                out localMousePosition);                // 输出局部坐标
            rectTransform.localPosition = localMousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        

        // 检查拖拽目标是否有效
        GameObject target = eventData.pointerEnter;

        // 恢复场景中所有 Defender 的射线检测
        EnableAllDefenderRaycasts();

        if (target == null)
        {
            // 恢复射线检测
            canvasGroup.blocksRaycasts = true;
            Debug.Log("Dropped outside target. Returning to original position.");
            transform.SetParent(originalParent);
            transform.localPosition = originalPosition;
            return;
        }

        // 递归检查目标是否为有效的 DropTarget
        DropTargetEditTeam editTeamPanel = target.GetComponentInParent<DropTargetEditTeam>();
        DropTargetPanel teamPanel = target.GetComponentInParent<DropTargetPanel>();

        if (editTeamPanel != null && editTeamPanel.containerTransform == originalParent)
        {
            // 如果拖动到原来的 `EditTeam` 容器内，但没有进入有效目标，返回原位置
            Debug.Log($"{gameObject.name} dropped back to EditTeam without a valid target. Returning to original position.");
            ResetToOriginalPosition();
            return;
        }

        if (editTeamPanel != null )
        {
            // 恢复射线检测
            canvasGroup.blocksRaycasts = true;
            // 如果拖到 Edit Team Container
            transform.SetParent(editTeamPanel.containerTransform);
            transform.localPosition = Vector3.zero;
            Debug.Log(defenderName + "Dropped into Edit Team Container.");
            return;
        }

       

        if (teamPanel != null)
        {
            

            // 恢复射线检测
            canvasGroup.blocksRaycasts = true;

            // 更新位置
            transform.SetParent(teamPanel.transform);
            transform.localPosition = Vector3.zero;
            Debug.Log(defenderName + "Dropped into Team Panel.");
            return;
        }

        // 如果未拖拽到任何有效区域，返回原位置
        Debug.Log(defenderName + "Dropped outside target. Returning to original position.");
        transform.SetParent(originalParent);
        transform.localPosition = originalPosition;
    }

    private void EnableAllDefenderRaycasts()
    {
        // 查找所有带有 CanvasGroup 的 Defender
        var defenders = FindObjectsOfType<DraggableDefender>();
        foreach (var defender in defenders)
        {
            var canvasGroup = defender.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
                Debug.Log($"Enabled raycasts for {defender.name}");
            }
        }
    }


    // 提供一个方法来获取名字
    public string GetDefenderName()
    {
        return defenderName;
    }

    private void ResetToOriginalPosition()
    {
        if (originalParent != null)
        {
            // 设置为原始父级
            transform.SetParent(originalParent);
            // 恢复到原始位置
            transform.localPosition = originalPosition;

            Debug.Log($"{gameObject.name} returned to original parent and position.");
        }
        else
        {
            Debug.LogError("Original parent is null. Cannot return to original position.");
        }
    }

}
