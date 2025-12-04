using System.Collections;
using UnityEngine;
using TMPro;

public class TypewriterEffect : MonoBehaviour
{
    public TMP_Text dialogueText; // 绑定到 TextMeshPro 组件
    public float typingSpeed = 0.05f; // 文字出现的时间间隔（调大一点，确保看得出效果）

    private string fullText; // 存储完整文本
    private Coroutine typingCoroutine; // 防止多个协程冲突
    private bool isTyping = false; // 追踪打字状态

    void Start()
    {
        if (dialogueText == null)
        {
            Debug.LogError("TypewriterEffect: 没有绑定 TextMeshPro 组件！");
        }
    }

    public void StartTypewriter(string text)
    {
        if (dialogueText == null)
        {
            Debug.LogError("TypewriterEffect: 没有绑定 TextMeshPro 组件！");
            return;
        }

        fullText = text;
        dialogueText.text = ""; // 清空文字
        Debug.Log($"启动打字机效果: {fullText}");

        if (typingCoroutine != null) StopCoroutine(typingCoroutine); // 防止协程冲突
        typingCoroutine = StartCoroutine(TypeText());
    }

    private IEnumerator TypeText()
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in fullText.ToCharArray())
        {
            if (!isTyping) // 如果被跳过，立即显示完整文本
            {
                dialogueText.text = fullText;
                break;
            }

            dialogueText.text += letter; // 逐字显示
            Debug.Log($"当前文本: {dialogueText.text}");
            yield return new WaitForSeconds(typingSpeed); // 等待一段时间
        }

        isTyping = false;
        Debug.Log("打字机动画结束！");
    }

    // 提供一个跳过打字动画的方法
    public void SkipTypewriter()
    {
        if (isTyping)
        {
            isTyping = false;
            dialogueText.text = fullText; // 立即显示完整文本
        }
    }
}
