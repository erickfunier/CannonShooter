using System;
using UnityEngine;
using UnityEngine.UI;

public class startMainMenu : MonoBehaviour {
    public GameObject scrollViewContent = null;
    public GameObject txtTitle = null;
    public GameObject txtTitleSettings = null;
    public GameObject txtTitleLevels = null;
    public GameObject btnBackLevels = null;
    public GameObject btnBackSettings = null;
    public GameObject btnStartMenu = null;
    public GameObject btnSettingsMenu = null;
    public GameObject scrollLevel = null;
    public Toggle toggleHelpPath = null;
    public Toggle toggleHurryUp = null;
    public Toggle togglePlayMusic = null;
    public Toggle togglePlaySounds = null;
    public AudioSource audioSourceBG;
    public AudioSource audioSource;
    public AudioClip bgMainMenu;
    public float volume = 0.5f;

    public static Vector2 GetAspectRatio(int x, int y) {
        float f = (float)x / (float)y;
        int i = 0;
        while (true) {
            i++;
            if (Math.Round(f * i, 2) == Mathf.RoundToInt(f * i))
                break;
        }
        return new Vector2((float)System.Math.Round(f * i, 2), i);
    }

    // Start is called before the first frame update
    void Start() {

        int savedLevel = 0;
        if (PlayerPrefs.HasKey("savedLevel")) {
            savedLevel = PlayerPrefs.GetInt("savedLevel");
        } else {
            PlayerPrefs.SetInt("savedLevel", 0);
        }
        savedLevel = 99;
        for (int i = 0; i < savedLevel + 1; i++) {
            scrollViewContent.transform.GetChild(i).GetComponent<Image>().color = Color.white;
            scrollViewContent.transform.GetChild(i).GetComponent<Button>().interactable = true;
        }

        // Disable screen dimming
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        //audioSource.mute = true;
        int tempPlaySounds = 0;
        if (PlayerPrefs.HasKey("playSounds")) {
            switch (PlayerPrefs.GetInt("playSounds")) {
                case 0:
                    togglePlaySounds.isOn = false;
                    break;
                case 1:
                    togglePlaySounds.isOn = true;
                    tempPlaySounds = 1;
                    break;
            }
        } else {
            //PlayerPrefs.SetInt("playSounds", 1);
            togglePlaySounds.isOn = true;
            tempPlaySounds = 1;
        }

        PlayerPrefs.SetInt("playSounds", 0);

        if (PlayerPrefs.HasKey("helpPath")) {
            switch (PlayerPrefs.GetInt("helpPath")) {
                case 0:
                    toggleHelpPath.isOn = false;
                    break;
                case 1:
                    toggleHelpPath.isOn = true;
                    break;
            }
        } else {
            PlayerPrefs.SetInt("helpPath", 1);
            toggleHelpPath.isOn = true;
        }

        if (PlayerPrefs.HasKey("hurryUp")) {
            switch (PlayerPrefs.GetInt("hurryUp")) {
                case 0:
                    toggleHurryUp.isOn = false;
                    break;
                case 1:
                    toggleHurryUp.isOn = true;
                    break;
            }
        } else {
            PlayerPrefs.SetInt("hurryUp", 1);
            toggleHurryUp.isOn = true;
        }

        if (PlayerPrefs.HasKey("playMusic")) {
            switch (PlayerPrefs.GetInt("playMusic")) {
                case 0:
                    togglePlayMusic.isOn = false;
                    break;
                case 1:
                    togglePlayMusic.isOn = true;
                    audioSourceBG.Play();
                    break;
            }
        } else {
            PlayerPrefs.SetInt("playMusic", 1);
            togglePlayMusic.isOn = true;
            audioSourceBG.Play();
        }

        PlayerPrefs.SetInt("playSounds", tempPlaySounds);

        Vector2 aspect = GetAspectRatio(Screen.width, Screen.height);
        //Debug.Log("Aspect before: " + aspect);

        if (aspect.x < 9) {
            aspect.y = 9/aspect.x * aspect.y;
            aspect.x = 9;
        } else if (aspect.x > 9) {
            aspect.y /= (aspect.x / 9);
            aspect.x = 9;
        }

        //Debug.Log("Aspect after: " + aspect);

        if (aspect.y <= 15) {
            txtTitle.transform.localPosition = new Vector2(0, 507);
            txtTitleSettings.transform.localPosition = new Vector2(0, 230);
            txtTitleLevels.transform.localPosition = new Vector2(0, 254.66f);

            btnStartMenu.transform.localPosition = new Vector2(0, 64);
            btnSettingsMenu.transform.localPosition = new Vector2(0, -155);
            btnBackLevels.transform.localPosition = new Vector2(-476, 270);
            btnBackSettings.transform.localPosition = new Vector2(0, -780);

            scrollLevel.GetComponent<RectTransform>().localPosition = new Vector2(0, -348.75f);
            scrollLevel.GetComponent<RectTransform>().sizeDelta = new Vector2(1080, 1102.5f);
            scrollViewContent.GetComponent<RectTransform>().offsetMin = new Vector2(0, -1920);
        } else if (aspect.y <= 16) {
            txtTitle.transform.localPosition = new Vector2(0, 537);
            txtTitleSettings.transform.localPosition = new Vector2(0, 250);
            txtTitleLevels.transform.localPosition = new Vector2(0, 254.66f);

            btnStartMenu.transform.localPosition = new Vector2(0, 34);
            btnSettingsMenu.transform.localPosition = new Vector2(0, -185);
            btnBackLevels.transform.localPosition = new Vector2(-476, 272.87f);
            btnBackSettings.transform.localPosition = new Vector2(0, -841);

            scrollLevel.GetComponent<RectTransform>().localPosition = new Vector2(0, -385.88f);
            scrollLevel.GetComponent<RectTransform>().sizeDelta = new Vector2(1080, 1148.245f);
            scrollViewContent.GetComponent<RectTransform>().offsetMin = new Vector2(0, -1893.266f);

        } else if (aspect.y <= 18) {
            txtTitle.transform.localPosition = new Vector2(0, 587);
            txtTitleSettings.transform.localPosition = new Vector2(0, 274);
            txtTitleLevels.transform.localPosition = new Vector2(0, 316);

            btnStartMenu.transform.localPosition = new Vector2(0, 134);
            btnSettingsMenu.transform.localPosition = new Vector2(0, -85);
            btnBackLevels.transform.localPosition = new Vector2(-476, 320);
            btnBackSettings.transform.localPosition = new Vector2(0, -780);

            scrollLevel.GetComponent<RectTransform>().localPosition = new Vector2(0, -426.02f);
            scrollLevel.GetComponent<RectTransform>().sizeDelta = new Vector2(1080, 1308);
            scrollViewContent.GetComponent<RectTransform>().offsetMin = new Vector2(0, -1736);
        } else if (aspect.y <= 18.5f) {
            txtTitle.transform.localPosition = new Vector2(0, 610);
            txtTitleSettings.transform.localPosition = new Vector2(0, 274);
            txtTitleLevels.transform.localPosition = new Vector2(0, 330);

            btnStartMenu.transform.localPosition = new Vector2(0, 107);
            btnSettingsMenu.transform.localPosition = new Vector2(0, -112);
            btnBackLevels.transform.localPosition = new Vector2(-476, 330);
            btnBackSettings.transform.localPosition = new Vector2(0, -780);

            scrollLevel.GetComponent<RectTransform>().localPosition = new Vector2(0, -428.19f);
            scrollLevel.GetComponent<RectTransform>().sizeDelta = new Vector2(1080, 1363.611f);
            scrollViewContent.GetComponent<RectTransform>().offsetMin = new Vector2(0, -1670);
        } else if (aspect.y <= 19) {
            txtTitle.transform.localPosition = new Vector2(0, 619);
            txtTitleSettings.transform.localPosition = new Vector2(0, 292);
            txtTitleLevels.transform.localPosition = new Vector2(0, 342);

            btnStartMenu.transform.localPosition = new Vector2(0, 116);
            btnSettingsMenu.transform.localPosition = new Vector2(0, -104);
            btnBackLevels.transform.localPosition = new Vector2(-476, 345);
            btnBackSettings.transform.localPosition = new Vector2(0, -750);

            scrollLevel.GetComponent<RectTransform>().localPosition = new Vector2(0, -437.81f);
            scrollLevel.GetComponent<RectTransform>().sizeDelta = new Vector2(1080, 1404.389f);
            scrollViewContent.GetComponent<RectTransform>().offsetMin = new Vector2(0, -1638);
        } else if (aspect.y <= 19.5f) {
            txtTitle.transform.localPosition = new Vector2(0, 630);
            txtTitleSettings.transform.localPosition = new Vector2(0, 292);
            txtTitleLevels.transform.localPosition = new Vector2(0, 342);

            btnStartMenu.transform.localPosition = new Vector2(0, 145);
            btnSettingsMenu.transform.localPosition = new Vector2(0, -74);
            btnBackLevels.transform.localPosition = new Vector2(-476, 345);
            btnBackSettings.transform.localPosition = new Vector2(0, -750);

            scrollLevel.GetComponent<RectTransform>().localPosition = new Vector2(0, -451.31f);
            scrollLevel.GetComponent<RectTransform>().sizeDelta = new Vector2(1080, 1437.38f);
            scrollViewContent.GetComponent<RectTransform>().offsetMin = new Vector2(0, -1590);
        } else if (aspect.y <= 20f) {
            txtTitle.transform.localPosition = new Vector2(0, 637);
            txtTitleSettings.transform.localPosition = new Vector2(0, 314);
            txtTitleLevels.transform.localPosition = new Vector2(0, 360);

            btnStartMenu.transform.localPosition = new Vector2(0, 134);
            btnSettingsMenu.transform.localPosition = new Vector2(0, -85);
            btnBackLevels.transform.localPosition = new Vector2(-476, 360);
            btnBackSettings.transform.localPosition = new Vector2(0, -740);

            scrollLevel.GetComponent<RectTransform>().localPosition = new Vector2(0, -467.97f);
            scrollLevel.GetComponent<RectTransform>().sizeDelta = new Vector2(1080, 1464);
            scrollViewContent.GetComponent<RectTransform>().offsetMin = new Vector2(0, -1560);
        } else if (aspect.y <= 20.5f) {
            txtTitle.transform.localPosition = new Vector2(0, 653);
            txtTitleSettings.transform.localPosition = new Vector2(0, 314);
            txtTitleLevels.transform.localPosition = new Vector2(0, 360);

            btnStartMenu.transform.localPosition = new Vector2(0, 150);
            btnSettingsMenu.transform.localPosition = new Vector2(0, -69);
            btnBackLevels.transform.localPosition = new Vector2(-476, 364);
            btnBackSettings.transform.localPosition = new Vector2(0, -750);

            scrollLevel.GetComponent<RectTransform>().localPosition = new Vector2(0, -473);
            scrollLevel.GetComponent<RectTransform>().sizeDelta = new Vector2(1080, 1514);
            scrollViewContent.GetComponent<RectTransform>().offsetMin = new Vector2(0, -1530);

        } else if (aspect.y <= 21f) {
            txtTitle.transform.localPosition = new Vector2(0, 670);
            txtTitleSettings.transform.localPosition = new Vector2(0, 336);
            txtTitleLevels.transform.localPosition = new Vector2(0, 375);

            btnStartMenu.transform.localPosition = new Vector2(0, 167);
            btnSettingsMenu.transform.localPosition = new Vector2(0, -52);
            btnBackLevels.transform.localPosition = new Vector2(-476, 378);
            btnBackSettings.transform.localPosition = new Vector2(0, -750);

            scrollLevel.GetComponent<RectTransform>().localPosition = new Vector2(0, -480.21f);
            scrollLevel.GetComponent<RectTransform>().sizeDelta = new Vector2(1080, 1560);
            scrollViewContent.GetComponent<RectTransform>().offsetMin = new Vector2(0, -1480);
        }
    }
}
