using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Volume : MonoBehaviour
{
    public AudioMixer mixer;

    public void SetBGMVolume(float sliderVal)
    {
        mixer.SetFloat("BGMVol", Mathf.Log10(sliderVal) * 20);
    }
    public void SetSFXVolume(float sliderVal)
    {
        mixer.SetFloat("SFXVol", Mathf.Log10(sliderVal) * 20);
    }
}
