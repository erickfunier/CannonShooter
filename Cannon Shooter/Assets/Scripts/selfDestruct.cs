using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class selfDestruct : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(delayCoroutine());
    }

    IEnumerator delayCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        Destroy(gameObject.transform.GetChild(8).gameObject);
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}
