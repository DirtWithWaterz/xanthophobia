using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchTrigger : MonoBehaviour
{
    GameObject Sublevel_001_parent;
    Level_001 level_001;

    void Start(){
        Sublevel_001_parent = GameObject.FindGameObjectWithTag("0.01 parent");
        level_001 = Sublevel_001_parent.GetComponent<Level_001>();
    }

    void OnTriggerEnter(Collider other)
    {
        level_001.Entering = true;
    }

    void OnTriggerExit(Collider other)
    {
        level_001.Entered = true;
    }
}
