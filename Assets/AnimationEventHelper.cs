using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventHelper : MonoBehaviour
{
    public void PlaySoundFromParent()
    {
        DefenderAudio parentAudio = GetComponentInParent<DefenderAudio>();
        if (parentAudio != null)
        {
            parentAudio.PlaySpellSound();
        }
    }
}
