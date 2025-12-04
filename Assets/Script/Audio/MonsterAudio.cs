using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class MonsterAudio : MonoBehaviour
{
    // 定义音效变量
    public AudioClip spawnSound;
    public AudioClip attackSound;
    public AudioClip deathSound;
    public AudioClip weaponCharging;
    public AudioSource backgroundAudioSource;  // 用于背景音乐或长时间播放的音效
    
    public AudioSource sfxAudioSource;  // 用于短促音效的播放

    // Start is called before the first frame update
    void Start()
    {
        // 播放出场音效，不影响其他音效的播放
        PlaySound(spawnSound);
    }

    // 播放音效方法（使用 PlayOneShot 避免音效重叠问题）
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && sfxAudioSource != null)
        {
            sfxAudioSource.PlayOneShot(clip);  // PlayOneShot 不会打断当前播放的音效
        }
    }

    // 播放背景音乐方法（持续播放，不会被其他音效打断）
    public void PlayBackgroundMusic(AudioClip clip)
    {
        if (clip != null && backgroundAudioSource != null && !backgroundAudioSource.isPlaying)
        {
            backgroundAudioSource.clip = clip;
            backgroundAudioSource.loop = true;  // 设置为循环播放
            backgroundAudioSource.Play();
        }
    }

    // 调用此方法播放攻击音效
    public void PlayAttackSound()
    {
        PlaySound(attackSound);
    }

    // 调用此方法播放死亡音效
    public void PlayDeathSound()
    {
        PlaySound(deathSound);
    }

    public void PlayWeaponCharging()
    {
        PlaySound(weaponCharging);
    }
}

