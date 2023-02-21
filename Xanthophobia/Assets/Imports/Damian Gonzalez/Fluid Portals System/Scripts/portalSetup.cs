//using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[DefaultExecutionOrder(100)]

public class portalSetup : MonoBehaviour {

    public Transform playerCamera;  //this is optional. Default value is main camera.
    public Shader refShader;        //this is already assigned. Should be "screenRender.shader"
    public string groupId = "";     //not necessary, but useful for debugging

    [HideInInspector] public bool setupComplete = false;


    private int screenHeight;
    private int screenWidth;

    private Camera cameraA;
    private Camera cameraB;

    private Transform portalA;
    private Transform portalB;

    private float timeStartResize = -1;


    public enum WhichOneHasRb { originPortal, destinationPortal, playerSide }
    public enum WhenToTeleport { onEnter, onExit, auto }


    [Serializable] public class Filters {
        [Header("------ Main filter. If unchecked, only the player will pass ------------------------------------------------")]
        public bool otherObjectsCanCross = true;

        [Header("------ Positive filter. Only objects with these tags can pass. ------------------------")]
        [Space(height: 20f)]
        public List<string> tagsCanCross = new List<string>();

        [Header("------ Gandalf filter. Objects with these tags shall not pass. ------------------------")]
        [Space(height: 20f)]
        public List<string> tagsCannotCross = new List<string>();
    }
    public Filters filters;

    [Serializable] public class Clones {
        [Header("Use of clones")]
        public bool useClones = true;
        public WhichOneHasRb whichOneHasRb = WhichOneHasRb.playerSide;

    }
    public Clones clones;

    [Serializable] public class Advanced {
        [Range(-5f, 5f)] public float clippingOffset = -3f;

        
        public WhenToTeleport whenToTeleportPlayer = WhenToTeleport.auto;
        public WhenToTeleport whenToTeleportOthers = WhenToTeleport.auto;

    }
    public Advanced advanced;

    private void Update() {
        //1 second after resize is done, it updates the render textures
        if (timeStartResize == -1 && (screenHeight != Screen.height || screenWidth != Screen.width)) timeStartResize = Time.time;

        if (timeStartResize > 0 && Time.time > timeStartResize + 1f) Resize(true);

        
    }

    void Start () {

        //if not provided, use default values
        if (playerCamera == null) playerCamera = Camera.main.transform;
        if (groupId == "") groupId = transform.name;

        //reference to each portal of this set
        portalA = transform.GetChild(0);
        portalB = transform.GetChild(1);

        //I generate containers for cameras (for pivot points)
        GameObject containerCamA = new GameObject("cameraContainer");
        GameObject containerCamB = new GameObject("cameraContainer");

        //and put them inside each portal
        containerCamA.transform.SetParent(portalA, true);
        containerCamB.transform.SetParent(portalB, true);

        //I generate the empty objects for the cameras
        GameObject objCamA = new GameObject("camera");
        GameObject objCamB = new GameObject("camera");

        //and put them inside the containers. 
        objCamA.transform.SetParent(containerCamA.transform, true);
        objCamB.transform.SetParent(containerCamB.transform, true);
        // (now we have portal>container>empty-obj-camera)

        //add camera components to the cameras
        cameraA = objCamA.AddComponent<Camera>();
        cameraB = objCamB.AddComponent<Camera>();

        //and its scripts
        portalCamMovement scriptCamA = cameraA.gameObject.AddComponent<portalCamMovement>();
        portalCamMovement scriptCamB = cameraB.gameObject.AddComponent<portalCamMovement>();

        //I give this new cameras same setup than main camera
        Camera cameraComp = playerCamera.GetComponent<Camera>();
        cameraA.CopyFrom(cameraComp);
        cameraB.CopyFrom(cameraComp);


        //I setup both camera's scripts
        scriptCamA.playerCamera = playerCamera;
        scriptCamA.thisPortal = portalA;
        scriptCamA.otherPortal = portalB;
        scriptCamA.clippingOffset = advanced.clippingOffset;
        scriptCamA.cameraId = groupId + ".a";

        scriptCamB.playerCamera = playerCamera;
        scriptCamB.thisPortal = portalB;
        scriptCamB.otherPortal = portalA;
        scriptCamB.clippingOffset = advanced.clippingOffset;
        scriptCamB.cameraId = groupId + ".b";

        //and setup both portal's script
        teleport scriptPortalA = portalA.Find("plane").GetComponent<teleport>();
        teleport scriptPortalB = portalB.Find("plane").GetComponent<teleport>();
        scriptPortalA.setup = this;
        scriptPortalA.cameraScript = scriptCamA;
        scriptPortalA.otherScript = scriptPortalB;

        scriptPortalB.setup = this;
        scriptPortalB.cameraScript = scriptCamB;
        scriptPortalB.otherScript = scriptPortalA;

        //I create materials with the shader
        //and asign those materials to the planes (here is where they cross)
        Resize(false);

        setupComplete = true;
        portalEvents.setupComplete?.Invoke(groupId, transform);

    }

    void Resize(bool fireEvent) {

        timeStartResize = -1;

        //I create materials with the shader
        Material matA = new Material(refShader);
        cameraA.targetTexture?.Release();
        cameraA.targetTexture = new RenderTexture(Screen.width, Screen.height, 0);
        matA.mainTexture = cameraA.targetTexture;
        matA.SetTexture("_MainTex", cameraA.targetTexture);

        Material matB = new Material(refShader);
        cameraB.targetTexture?.Release();
        cameraB.targetTexture = new RenderTexture(Screen.width, Screen.height, 0);
        matB.mainTexture = cameraB.targetTexture;

        //and asign those materials to the planes (here is where they cross)
        portalA.Find("plane").GetComponent<MeshRenderer>().material = matB;
        portalB.Find("plane").GetComponent<MeshRenderer>().material = matA;

        //also to the "emergency" plane (see online documentation)
        if (portalA.Find("plane2") != null) portalA.Find("plane2").GetComponent<MeshRenderer>().material = matB;
        if (portalB.Find("plane2") != null) portalB.Find("plane2").GetComponent<MeshRenderer>().material = matA;

        //fire event
        if (fireEvent) {
            portalEvents.gameResized?.Invoke(
                groupId,
                transform,
                new Vector2(screenHeight, screenWidth),
                new Vector2(Screen.height, Screen.width)
            );
        }

        screenHeight = Screen.height;
        screenWidth = Screen.width;

    }

}