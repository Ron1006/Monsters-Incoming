using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance; // 单例模式，全局

    public AudioSource audioSource;  // 音频源
    public AudioClip upgradeButtonSound;  // 升级按钮音效
    public AudioClip clickButtonSound;  // 点击按钮音效

    void Awake()
    {
        // 确保单例模式的正确运行
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("AudioSource or AudioClip is missing.");
        }
    }

    public void PlayUpgradeButtonSound()
    {
        PlaySound(upgradeButtonSound);
    }

    public void PlayClickButtonSound()
    {
        PlaySound(clickButtonSound);
    }
}

