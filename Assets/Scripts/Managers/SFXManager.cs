using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public AudioSource audioSource;

    [Header("Clips")]
    public AudioClip buttonSFX;
    public AudioClip bombSFX;
    public AudioClip setGameSFX;
    public AudioClip winSFX;
    public AudioClip getScoreSFX;
    public AudioClip voteSFX;
    public void OnClickBtn()
    {
        audioSource.clip = buttonSFX;
        audioSource.Play();
    }

    public void WinSFX()
    {
        audioSource.clip = winSFX;
        audioSource.Play();
    }
    public void SetGameSFX()
    {
        audioSource.clip = setGameSFX;
        audioSource.Play();
    }
    public void BombSFX()
    {
        audioSource.clip = bombSFX;
        audioSource.Play();
    }
    public void GetScoreSFX()
    {
        audioSource.clip = getScoreSFX;
        audioSource.Play();
    }
    public void VoteSFX()
    {
        audioSource.clip = voteSFX;
        audioSource.Play();
    }
}
