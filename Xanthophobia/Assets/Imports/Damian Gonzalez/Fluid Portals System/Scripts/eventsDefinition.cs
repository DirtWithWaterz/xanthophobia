using System;
using UnityEngine;


public static class portalEvents 
{

    //                   groupId, portal
    public static Action<string,  Transform> setupComplete;




    //                  groupId, portalFrom, portalTo,  objTeleported, positionFrom, positionTo
    public static Action<string, Transform,  Transform, Transform,     Vector3,      Vector3>   teleport;




    //                   groupId, portal,    oldSize, newSize
    public static Action<string,  Transform, Vector2, Vector2> gameResized;


    //see documentation, or script portalEventsListenerExample.cs, for instructions on how to listen these events.
}
