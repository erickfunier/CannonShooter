using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playBtnSounds : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip clip;
    public float volume = 0.5f;

    public void playOneShot() {
        if (PlayerPrefs.HasKey("playSounds")) {
            if (PlayerPrefs.GetInt("playSounds") == 1) {
                audioSource.PlayOneShot(clip, volume);
            }
            
        }
    }
}
