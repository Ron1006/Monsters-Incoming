using UnityEngine;

public class DefenderActivationLogger : MonoBehaviour
{
    private bool wasActive = true;

    void Update()
    {
        if (!wasActive && gameObject.activeSelf)
        {
            Debug.LogWarning($"[检测] 对象 {gameObject.name} 被重新激活了！");
            Debug.Break(); // 游戏暂停，方便你定位谁做的
        }

        wasActive = gameObject.activeSelf;
    }
}
