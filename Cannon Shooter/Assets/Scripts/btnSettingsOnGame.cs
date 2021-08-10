using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class btnSettingsOnGame : MonoBehaviour
{
    public GameObject m_menuOnGame = null;
    public GameObject cylinder = null;
    public GameObject ring = null;
    public Material transpMaterial = null;

    public void TaskOnClick() {

        if (m_menuOnGame != null) {
            Color color;
            if (!cylinder.GetComponent<MeshRenderer>()) {
                color = cylinder.transform.GetChild(0).GetComponent<MeshRenderer>().material.color;
            } else {
                color = cylinder.GetComponent<MeshRenderer>().material.color;
            }
            color.a = 0;
            transpMaterial.color = color;
            if (!cylinder.GetComponent<MeshRenderer>()) {
                cylinder.transform.GetChild(0).GetComponent<MeshRenderer>().material = transpMaterial;
                cylinder.transform.GetChild(2).GetComponent<MeshRenderer>().material = transpMaterial;
            } else {
                cylinder.GetComponent<MeshRenderer>().material = transpMaterial;
            }            
            if (ring != null && !ring.GetComponent<MeshRenderer>()) {
                ring.transform.GetChild(0).GetComponentInChildren<MeshRenderer>().material = transpMaterial;
                ring.transform.GetChild(1).GetComponentInChildren<MeshRenderer>().material = transpMaterial;
                ring.transform.GetChild(2).GetComponentInChildren<MeshRenderer>().material = transpMaterial;
            } else if (ring != null) {
                ring.GetComponent<MeshRenderer>().material = transpMaterial;
            }            
            m_menuOnGame.SetActive(true);


        }        
    }
}
