using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class blinkText : MonoBehaviour
{
    Text text;
    // Start is called before the first frame update

    IEnumerator Blink() {
        while (true) {
            text.text = "Hurry Up!!!";
            yield return new WaitForSeconds(1f);
            text.text = "";
            yield return new WaitForSeconds(1f);
        }

    }

    void StartBlinking() {
        StartCoroutine("Blink");
    }
    void StopBlinking() {
        StopCoroutine("Blink");
    }

    private void OnEnable() {
        //Debug.Log("Start");
        text = GetComponent<Text>();
        StartBlinking();
    }
}
