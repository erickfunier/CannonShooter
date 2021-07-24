using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class loadLevel : MonoBehaviour {

    public int age = 1;

    public void loadSceneLevel() {
        string number = GetComponentInChildren<Text>().text;
        int savedLevel = PlayerPrefs.GetInt("savedLevel");
        //Debug.Log("Text: " + number);
        //Debug.Log("Saved: " + savedLevel);
        if (int.Parse(number)-1 <= savedLevel) {
            PlayerPrefs.SetInt("level", int.Parse(number)-1);
            PlayerPrefs.Save();
            if (age == 1) {
                SceneManager.LoadScene("MiddleAge");
            } else if (age == 2) {
                SceneManager.LoadScene("ww2");
            } else {
                SceneManager.LoadScene("MiddleAge");
            }
        } else {
            //Debug.Log("Unavailable");
        }
    }

    public void loadMainMenu() {
        SceneManager.LoadScene("MainMenuScene");
    }

    public void loadNextLevel() {
        int level = -1;
        if (PlayerPrefs.HasKey("level")) {
            if (level == -1) {
                level = PlayerPrefs.GetInt("level");
            }
        }
        if (age == 1) {
            SceneManager.LoadScene("MiddleAge");
        } else if (age == 2 || level >= 40) {
            SceneManager.LoadScene("ww2");
        } else {
            SceneManager.LoadScene("MiddleAge");
        }
        
    }
}
