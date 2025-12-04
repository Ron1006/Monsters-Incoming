using UnityEngine;

public class RectTransformExample : MonoBehaviour
{
    public RectTransform targetRectTransform; // 将需要修改的对象拖入此字段

    void Start()
    {
        if (targetRectTransform != null)
        {
            // 设置 Anchor
            targetRectTransform.anchorMin = new Vector2(0, 0.5f); // 左中
            targetRectTransform.anchorMax = new Vector2(0, 0.5f);

            // 设置 Pivot
            targetRectTransform.pivot = new Vector2(0, 0.5f);

            // 设置位置 (Position)
            targetRectTransform.anchoredPosition = new Vector2(10, 0); // 距离左锚点10单位，Y轴居中

            // 设置大小 (SizeDelta)
            targetRectTransform.sizeDelta = new Vector2(200, 50); // 宽200，高50
        }
        else
        {
            Debug.LogWarning("Target RectTransform is not assigned!");
        }
    }
}
