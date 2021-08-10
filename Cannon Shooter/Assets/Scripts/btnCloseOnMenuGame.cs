using System.Collections;
using UnityEngine;

public class btnCloseOnMenuGame : MonoBehaviour {
    public GameObject m_menuOnGame = null;
    public GameObject cylinder = null;
    public GameObject ring = null;
    public Material defaultCylinderMaterial = null;
    public Material defaultRingMaterial = null;
    public void TaskOnClick() {
        if (m_menuOnGame != null) {
            StartCoroutine(delayCoroutine());
        }
    }

    IEnumerator delayCoroutine() {
        yield return new WaitForSeconds(0.6f);
        Color color;
        if (!cylinder.GetComponent<MeshRenderer>()) {
            color = cylinder.transform.GetChild(0).GetComponent<MeshRenderer>().material.color;
        } else {
            color = cylinder.GetComponent<MeshRenderer>().material.color;
        }
        color.a = 1;
        defaultCylinderMaterial.color = color;
        if (!cylinder.GetComponent<MeshRenderer>()) {
            cylinder.transform.GetChild(0).GetComponent<MeshRenderer>().material = defaultCylinderMaterial;
            cylinder.transform.GetChild(2).GetComponent<MeshRenderer>().material = defaultCylinderMaterial;
        } else {
            cylinder.GetComponent<MeshRenderer>().material = defaultCylinderMaterial;
        }
        
        if (ring != null && ring.GetComponent<MeshRenderer>() == null) {
            ring.transform.GetChild(0).GetComponentInChildren<MeshRenderer>().material = defaultRingMaterial;
            ring.transform.GetChild(1).GetComponentInChildren<MeshRenderer>().material = defaultRingMaterial;
            ring.transform.GetChild(2).GetComponentInChildren<MeshRenderer>().material = defaultRingMaterial;
        } else if (ring != null) {
            ring.GetComponent<MeshRenderer>().material = defaultRingMaterial;
        }
        m_menuOnGame.SetActive(false);
    }
}
