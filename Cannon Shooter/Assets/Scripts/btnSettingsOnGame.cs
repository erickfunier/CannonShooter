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
            Color color = cylinder.GetComponent<MeshRenderer>().material.color;
            color.a = 0;
            transpMaterial.color = color;
            cylinder.GetComponent<MeshRenderer>().material = transpMaterial;
            if (ring.GetComponent<MeshRenderer>() == null) {
                ring.transform.GetChild(0).GetComponentInChildren<MeshRenderer>().material = transpMaterial;
                ring.transform.GetChild(1).GetComponentInChildren<MeshRenderer>().material = transpMaterial;
                ring.transform.GetChild(2).GetComponentInChildren<MeshRenderer>().material = transpMaterial;
            } else {
                ring.GetComponent<MeshRenderer>().material = transpMaterial;
            }            
            m_menuOnGame.SetActive(true);


        }        
    }
}
