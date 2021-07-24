using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class togFunctions : MonoBehaviour
{
    public Toggle toggleObject = null;
    public AudioSource audioSource = null;
    public AudioSource audioSourceBG = null;

    // Start is called before the first frame update
    public void updatePlayerPrefs(int option)
    {
        switch (option) {
            case 0: //  Toggle Help Path
                if (toggleObject.isOn) {
                    PlayerPrefs.SetInt("helpPath", 1);
                } else {
                    PlayerPrefs.SetInt("helpPath", 0);
                }
                break;
            case 1: // Toggle Hurry Up
                if (toggleObject.isOn) {
                    PlayerPrefs.SetInt("hurryUp", 1);
                } else {
                    PlayerPrefs.SetInt("hurryUp", 0);
                }
                break;
            case 2: // Toggle Play Music
                if (toggleObject.isOn) {
                    PlayerPrefs.SetInt("playMusic", 1);
                    audioSourceBG.Play();
                } else {
                    PlayerPrefs.SetInt("playMusic", 0);
                    audioSourceBG.Stop();
                }
                break;
            case 3: // Toggle Play Sounds
                if (toggleObject.isOn) {
                    PlayerPrefs.SetInt("playSounds", 1);
                } else {
                    PlayerPrefs.SetInt("playSounds", 0);
                }
                break;
        }
        //Debug.Log("Toggle");
        PlayerPrefs.Save();
    }
}
