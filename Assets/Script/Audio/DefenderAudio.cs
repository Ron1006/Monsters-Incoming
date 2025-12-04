using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenderAudio : MonoBehaviour
{
    // 定义音效变量
    public AudioClip spawnSound;
    public AudioClip attackSound;
    public AudioClip deathSound;
    public AudioClip spellSound;
    public AudioClip healSound;

    private AudioSource audioSource;  // 引用AudioSource组件

    // Start is called before the first frame update
    void Start()
    {
        // 获取AudioSource组件
        audioSource = GetComponent<AudioSource>();

        // 播放出场音效
        PlaySound(spawnSound);
    }

    // 播放音效方法
    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // 调用此方法播放攻击音效
    public void PlayAttackSound()
    {
        PlaySound(attackSound);
    }

    public void PlayHealSound()
    {
        if (healSound == null)
        {
            Debug.LogWarning("[AUDIO] Heal sound is missing!");
            return;
        }

        Debug.Log("[AUDIO] Playing heal sound...");
        PlaySound(healSound);
    }

    // 调用此方法播放死亡音效
    public void PlayDeathSound()
    {
        PlaySound(deathSound);
    }

    // 调用技能音效
    public void PlaySpellSound()
    {
        PlaySound(spellSound);
    }
}
