using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BGMManager : MonoBehaviour
{
    [SerializeField] private AudioClip bgmClip;
    [SerializeField] [Range(0f,1f)] private float desiredVolume;
    [SerializeField] [Range(0,10f)] private float fadeInDuration;
    
    private AudioSource audioSource;
    
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(FadeInAudio());
    }

    IEnumerator FadeInAudio()
    {
        audioSource.volume = 0;
        audioSource.Play();
        float countUp = 0;
        while (countUp<fadeInDuration) {
            countUp += Time.deltaTime;
            audioSource.volume = desiredVolume * Mathf.Clamp(countUp / fadeInDuration,0,1);
            yield return null;
        }
    }
}
