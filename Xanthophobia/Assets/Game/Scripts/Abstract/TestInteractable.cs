using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FIMSpace.GroundFitter;

public class TestInteractable : Interactable
{
    public override void OnFocus(GameObject i){
        Debug.Log(i.name + " is looking at " + this.name);
    }

    public override void OnHoldInteract(GameObject i)
    {
        Debug.Log(i.name + " is holding interact on " + this.name);
    }

    public override void OnInteract(GameObject i)
    {
        Debug.Log(i.name + " interacted with " + this.name);
    }

    public override void OnLoseFocus(GameObject i)
    {
        Debug.Log(i.name + " stopped looking at " + this.name);
    }

    public override void OnReleaseInteract(GameObject i)
    {
        Debug.Log(i.name + " released interact on " + this.name);
    }
}
