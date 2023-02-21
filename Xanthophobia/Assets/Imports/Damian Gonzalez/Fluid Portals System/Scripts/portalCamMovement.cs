//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;

/* 
 * this script controls what THIS side portal displays,
 * moving the camera on the OTHER side
 */


public class portalCamMovement : MonoBehaviour
{
    //this 3 variables are assigned automatically by the Setup script

    [HideInInspector] public Transform playerCamera;
    [HideInInspector] public Transform thisPortal;
    [HideInInspector] public Transform otherPortal;

    public string cameraId; //useful if some debugging is needed. This is automatically assigned by portalSetup
    public Vector3 offset; //only used for a special case in the "long tunnel effect".

    private Camera thisCamera;

    [Range(-10f,10f)] public float clippingOffset = -3f;


    private void Start() {
        if (playerCamera == null) playerCamera = Camera.main.transform;
        thisCamera = transform.GetComponent<Camera>();
    }

    public void LateUpdate()
    {
        float dist = (transform.position - thisPortal.position).magnitude;
        thisCamera.nearClipPlane = Mathf.Clamp( dist + clippingOffset, 0.01f, float.MaxValue);


        //rotation
        transform.rotation = 
             (thisPortal.rotation
             * Quaternion.Inverse(otherPortal.rotation))
             * playerCamera.rotation
        ;
        

        //position
        Vector3 distanceFromPlayerToPortal = playerCamera.position - (otherPortal.position);
        Vector3 whereTheOtherCamShouldBe = thisPortal.position + (distanceFromPlayerToPortal) + offset;
        transform.position = RotatePointAroundPivot(
            whereTheOtherCamShouldBe,
            thisPortal.position,
            (thisPortal.rotation * Quaternion.Inverse(otherPortal.rotation)).eulerAngles
        );

    }

    public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles) {
        Vector3 dir = point - pivot; // get point direction relative to pivot
        dir = Quaternion.Euler(angles) * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point; // return it
    }

    public void hhh() {
        float dist = (transform.position - thisPortal.position).magnitude;
        thisCamera.nearClipPlane = Mathf.Clamp(dist + clippingOffset, 0.01f, float.MaxValue);


        //rotation
        transform.rotation =
             (thisPortal.rotation
             * Quaternion.Inverse(otherPortal.rotation))
             * playerCamera.rotation
        ;


        //position
        Vector3 distanceFromPlayerToPortal = playerCamera.position - (otherPortal.position);
        Vector3 whereTheOtherCamShouldBe = thisPortal.position + (distanceFromPlayerToPortal) + offset;
        transform.position = RotatePointAroundPivot(
            whereTheOtherCamShouldBe,
            thisPortal.position,
            (thisPortal.rotation * Quaternion.Inverse(otherPortal.rotation)).eulerAngles
        );
    }
}
