using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class loadLevel : MonoBehaviour {

    public int age = 1;
    public int cannonId = 0;

    public void loadSceneLevel() {
        string number = GetComponentInChildren<Text>().text;
        int savedLevel = PlayerPrefs.GetInt("savedLevel");
        
        if (int.Parse(number)-1 <= savedLevel) {
            PlayerPrefs.SetInt("cannonId", cannonId);
            PlayerPrefs.SetInt("level", int.Parse(number)-1);
            PlayerPrefs.Save();
            if (age == 1) {        
                SceneManager.LoadScene("MiddleAge");
            } else if (age == 2) {
                SceneManager.LoadScene("ww2");
            } else if (age == 3) {
                SceneManager.LoadScene("Future");
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
        if (level < 39) {
            PlayerPrefs.SetInt("cannonId", 0);
            SceneManager.LoadScene("MiddleAge");
        } else if (level >= 39 && level < 69) {
            PlayerPrefs.SetInt("cannonId", 1);
            SceneManager.LoadScene("ww2");
        } else if (level >= 69 && level < 100) {
            PlayerPrefs.SetInt("cannonId", 2);
            SceneManager.LoadScene("Future");            
        } else {
            SceneManager.LoadScene("EndGame");
        }
    }
}
