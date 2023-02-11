using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FIMSpace.GroundFitter;

public abstract class Interactable : MonoBehaviour
{
    GameObject objectToFind;

    public virtual void Awake()
    {
        objectToFind = GameObject.FindGameObjectWithTag("MainCamera");
        gameObject.layer = 9;
    }
    public abstract void OnHoldInteract(GameObject i);
    public abstract void OnInteract(GameObject i);
    public abstract void OnReleaseInteract(GameObject i);
    public abstract void OnFocus(GameObject i);
    public abstract void OnLoseFocus(GameObject i);
}
