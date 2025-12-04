using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasScalerWorldSpace : MonoBehaviour
{
    void Start()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas.renderMode == RenderMode.WorldSpace)
        {
            float scaleFactor = Screen.height / 1080f; // 以 1080p 作为基准
            transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);
        }
    }
}
