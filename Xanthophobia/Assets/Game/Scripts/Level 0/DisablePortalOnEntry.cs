using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisablePortalOnEntry : MonoBehaviour
{
    GameObject[] lvlPortals;

    void Start(){
        lvlPortals = GameObject.FindGameObjectsWithTag("0.01 portals");
        for (int i = 0; i < lvlPortals.Length; i++)
        {
            lvlPortals[i].SetActive(false);
        }
    }

    void OnTriggerExit(Collider other)
    {
        StartCoroutine(WaitBitch());
        for (int i = 0; i < lvlPortals.Length; i++)
        {
            lvlPortals[i].SetActive(true);
        }
    }
    public IEnumerator WaitBitch(){
        yield return new WaitForSeconds(0.001f);
        Destroy(this.transform.parent.gameObject.transform.parent.gameObject);
    }
}
