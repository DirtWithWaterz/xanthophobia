//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;

public class tunnelSetup : MonoBehaviour
{
    public Transform playerCamera = null;   //optional. Default value is main camera

    //this 4 variables are already assigned
    public Transform longTunnelSouthPortal;     
    public Transform longTunnelNorthPortal;
    public Transform shortTunnelSouthPortal;
    public Transform shortTunnelNorthPortal;

    [HideInInspector] public bool setupComplete = false ;
    public string playerTag = "Player";

    public bool playerBeginsOutsideLongTunnel = true;

    //references to the actual scripts of the cameras
    portalCamMovement shortS, shortN, longS, longN;

    private void Start() {
        if (playerCamera == null) playerCamera = Camera.main.transform;
    }
    void Setup() {
        //this would be the Start method, but I need the portal's Start() method to run first
        shortS = shortTunnelSouthPortal.Find("cameraContainer").Find("camera").GetComponent<portalCamMovement>();
        shortN = shortTunnelNorthPortal.Find("cameraContainer").Find("camera").GetComponent<portalCamMovement>();
        longS  =  longTunnelSouthPortal.Find("cameraContainer").Find("camera").GetComponent<portalCamMovement>();
        longN  =  longTunnelNorthPortal.Find("cameraContainer").Find("camera").GetComponent<portalCamMovement>();

        RecalculateOffset(!playerBeginsOutsideLongTunnel);
        setupComplete = true;

        //subscription to public event
        portalEvents.teleport += somethingTeleported;
    }

    void Update()
    {
        //check if both portals are already set
        if (!setupComplete) {
            if (
                transform.GetChild(0).transform.GetComponent<portalSetup>().setupComplete
                &&
                transform.GetChild(1).transform.GetComponent<portalSetup>().setupComplete
            ) Setup();
        }
    }


    void somethingTeleported(string groupId, Transform portalFrom, Transform portalTo, Transform objectTeleported, Vector3 positionFrom, Vector3 positionTo) {
        if (objectTeleported.CompareTag("Player")) {
            if (portalFrom == longTunnelNorthPortal || portalFrom == longTunnelSouthPortal) RecalculateOffset(false);
            if (portalFrom == shortTunnelNorthPortal || portalFrom == shortTunnelSouthPortal) RecalculateOffset(true);
        }
    }

    void RecalculateOffset(bool isInsideLongTunnel) {

        if (isInsideLongTunnel) {
            //player is inside the long tunnel, so
            //cameras of the short tunnel must work normally
            shortS.offset = Vector3.zero;
            shortN.offset = Vector3.zero;

        } else {
            /*
               player is outside the long tunnel, (maybe in the area of the short tunnel, looking through it)
               so cameras of the short tunnel must work different. 

               this is because... when player is looking THROUGH the short tunnel (but seeing the long one), there's this
               explicit problem: the other side of the tunnel is not "playing" in the back panel of the long tunnel,
               because player is not in the long tunnel, so I have to fake as he was inside, so the short-tunnel cameras
               take the back of the short-side and not elsewhere

               Normally, the camera in the other side mimics the player's movement but in the OTHER side.
               But, in this particular case, player and the "other" camera are in the SAME side: they're both in the
               short-tunnel side, but acting as if player were inside the long tunnel (in the other side)

               This is the only time when I have to adjust the normal placement of cameras
             */

            Vector3 longTunnelLongitude = longTunnelSouthPortal.position - longTunnelNorthPortal.position;
            Vector3 distShortSouth = playerCamera.position - shortTunnelSouthPortal.position;
            Vector3 oldPosition = shortTunnelNorthPortal.position + playerCamera.position - longTunnelNorthPortal.position;
            Vector3 newPosition = shortTunnelNorthPortal.position + longTunnelLongitude + distShortSouth;
            shortN.offset = newPosition - oldPosition;

            Vector3 distShortNorth = playerCamera.position - shortTunnelNorthPortal.position;
            oldPosition = shortTunnelSouthPortal.position + playerCamera.position - longTunnelSouthPortal.position;
            newPosition = shortTunnelSouthPortal.position - longTunnelLongitude + distShortNorth;
            shortS.offset = newPosition - oldPosition;

        }

        shortS.LateUpdate();
        shortN.LateUpdate();
        longS.LateUpdate();
        longN.LateUpdate();
    }
}
