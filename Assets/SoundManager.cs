using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("UI Sounds")]
    public AudioClip equipSound;
    public AudioClip dropEquipSound;
    public AudioClip sellEquipSound;
    public AudioClip clickSound;
    public AudioClip openPanelSound;
    public AudioClip drawSound;
    public AudioClip successSound;

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
        }
        else Destroy(gameObject);
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null && gameObject.activeInHierarchy)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
