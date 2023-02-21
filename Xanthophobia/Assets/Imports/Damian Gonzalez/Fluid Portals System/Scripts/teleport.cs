//using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class teleport : MonoBehaviour {

    [HideInInspector] public Transform reciever;
    [HideInInspector] public portalSetup setup;

    [HideInInspector] public portalCamMovement cameraScript;

    [HideInInspector] public teleport otherScript;

    bool teleportPlayerOnExit = false;

    List<Transform> clones = new List<Transform>();
    List<Transform> originals = new List<Transform>();
    GameObject cloneParent;

  
    
    void Teleport(Transform objectToTeleport, bool forceTeleportAndNoEvent = false, bool entering = true) {

        if ((objectToTeleport.position - transform.position).magnitude > 5) return;

        //which side of the portal is the object?
        if (!forceTeleportAndNoEvent) {
            float dotProduct = Vector3.Dot(transform.up, objectToTeleport.position - transform.position);
            if ((entering && dotProduct < 0) || (!entering && dotProduct > 0)){
                //wrong side. Don't teleport.
                teleportPlayerOnExit = false;
                return;
            }
        }
        //good side, continue


        // Teleport the object
        if (otherScript != null) {
            otherScript.teleportPlayerOnExit = true;
        }

        Vector3 oldPosition = objectToTeleport.position;
        Vector3 rbOffset = Vector3.zero;
        Rigidbody rb = objectToTeleport.GetComponent<Rigidbody>();
        if (rb != null) rbOffset = rb.position - transform.position;


        //position
        objectToTeleport.position = reciever.parent.TransformPoint(
            transform.parent.InverseTransformPoint(objectToTeleport.position)
        );

        //rotation
        Quaternion rot = reciever.parent.rotation * Quaternion.Inverse(transform.parent.rotation);
        Quaternion rot2 = rot * Quaternion.Inverse(objectToTeleport.rotation);
        objectToTeleport.Rotate(
            (objectToTeleport.CompareTag("Player") ? rot2 : rot).eulerAngles.x,
            rot.eulerAngles.y,
            (objectToTeleport.CompareTag("Player") ? rot2 : rot).eulerAngles.z
        );

        //velocity (if object has rigidbody)
        if (rb != null) {
            rb.velocity = reciever.parent.TransformDirection(
                transform.parent.InverseTransformDirection(rb.velocity)
            );
            rb.position = transform.position + rbOffset; //not entirely necessary
        }




        if (objectToTeleport.CompareTag("Player")) {
            //player has crossed. If using clones, may be necessary to swap clones and originals (see documentation)
            if (setup.clones.useClones && setup.clones.whichOneHasRb == portalSetup.WhichOneHasRb.playerSide) {
                int howManyOnTheOtherSide = otherScript.originals.Count;
                SwapSidesOfClonesOnThisSide(originals.Count);
                otherScript.SwapSidesOfClonesOnThisSide(howManyOnTheOtherSide);
            }

            //refresh camera position before rendering, in order to avoid flickering
            otherScript.cameraScript.LateUpdate();
            cameraScript.LateUpdate();
            
            
            /*
             * If you need to do something to your player when it's teleporting, this is when.
             * See online documentation about controller (pipasjourney.com/damianGonzalez/portals/#controller)
             * and how to adapt Unity's Rigidbody First Person Controller to skewed portals
            */
        }


        //finally, fire event
        if (!forceTeleportAndNoEvent)
        portalEvents.teleport?.Invoke(setup.groupId, transform.parent, reciever.parent, objectToTeleport, oldPosition, objectToTeleport.position);
    }

    public void SwapSidesOfClonesOnThisSide(int howMany) {
        int i;
        for (i = howMany - 1; i >= 0; i--) {
            Teleport(originals[i], true); //the other side will create a clone in this side
            DestroyClone(originals[i]);
        }

    }

    void OnTriggerEnter (Collider other)
	{
        if (setup.clones.useClones && other.gameObject.name.Contains("(portal clone)")) return;
        if (!thisObjectCanCross(other.transform)) return;
        if (other.CompareTag("Player")) {

            switch (setup.advanced.whenToTeleportPlayer) {
                case portalSetup.WhenToTeleport.auto:

                    //facing the portal, or crossing totally sideways (looking at the ark)?
                    if (Vector3.Dot(transform.up, -setup.playerCamera.forward) > 0
                        ||
                        Mathf.Abs(Vector3.Dot(transform.right, -setup.playerCamera.forward)) > .9f
                    ) {
                        //yes, teleport on enter (now)
                        Teleport(other.transform, false, true);
                        teleportPlayerOnExit = false;
                    } else {
                        //no, player is looking back. Teleport on exit (later)
                        teleportPlayerOnExit = true;
                    }
                    break;

                case portalSetup.WhenToTeleport.onEnter:
                    Teleport(other.transform, false, true);
                    teleportPlayerOnExit = false;
                    break;

                case portalSetup.WhenToTeleport.onExit:
                    teleportPlayerOnExit = true;
                    break;
            }

        } else {
            //not player, and it can cross

            //default values (in case user changes them to "auto" in play mode)
            if (setup.advanced.whenToTeleportOthers == portalSetup.WhenToTeleport.auto) {
                setup.advanced.whenToTeleportOthers = (setup.clones.useClones)
                    ? portalSetup.WhenToTeleport.onExit
                    : portalSetup.WhenToTeleport.onEnter;
            }

            if (setup.clones.useClones &&
                setup.advanced.whenToTeleportOthers == portalSetup.WhenToTeleport.onExit &&
                !originals.Contains(other.transform)
            ) CreateClone(other.transform, transform); //teleport on exit, not yet


            if (setup.advanced.whenToTeleportOthers == portalSetup.WhenToTeleport.onEnter) {
                Teleport(other.transform, false, true);//and the cloning is up to the other portal
                
            }
        }
    }

    void OnTriggerExit(Collider other) {
        //this original has a clone?
        if (setup.clones.useClones) {
            if (originals.Contains(other.transform)) DestroyClone(other.transform);
            if (thisObjectCanCross(other.transform)) {
                Teleport(other.transform, false, false);
            }
            return;
        }

        if (!thisObjectCanCross(other.transform)) return;
        if (other.CompareTag("Player")) {
            if (teleportPlayerOnExit || setup.advanced.whenToTeleportPlayer == portalSetup.WhenToTeleport.onExit) {
                Teleport(other.transform, false, false);
                teleportPlayerOnExit = false;
            }
        } else {
            //not a player, and can cross
            Teleport(other.transform, false, false); //else, when?

        }

    }
    
    void OnTriggerStay(Collider other) {
        //other side has a clone? update it
        if (setup.clones.useClones && originals.Contains(other.transform)) UpdateClone(other.transform);

    }


    void CreateClone(Transform original, Transform originPortal) {
        if (cloneParent == null) {
            cloneParent = GameObject.Find("Portal Clones") ?? new GameObject("Portal Clones");
        }

        if (setup.clones.whichOneHasRb == portalSetup.WhichOneHasRb.destinationPortal && originPortal == transform) {
            Teleport(original, true);   //put it on the other side
            otherScript.CreateClone(original, transform); //and then ask the other portal to create here a clone
        } else { 
            //originPortal or playerSide
            Transform clone = Instantiate(original.gameObject, cloneParent.transform).transform;
            clone.name = "(portal clone) " + original.name;

            //disable rigidbody
            if (original.GetComponent<Rigidbody>() != null) {
                clone.GetComponent<Rigidbody>().isKinematic = true;
                clone.GetComponent<Collider>().enabled = false;
            }

            Teleport(clone, true);

            clones.Add(clone);
            originals.Add(original);
        }

    }

    void UpdateClone(Transform original) {
        //get the corresponding clone
        int i;
        for (i = 0; i < originals.Count; i++) {
            if (originals[i] == original) break;
        }
        Transform clone = clones[i];


        //and update it (similar calculations to the actual teleporting)
        
        //--position
        clone.position = reciever.parent.TransformPoint(
            transform.parent.InverseTransformPoint(original.position)
        );
        
        //--rotation
        clone.rotation =
             (Quaternion.Inverse(transform.parent.rotation)
             * reciever.parent.rotation)
             * original.rotation
        ;

    }


    void DestroyClone(Transform original) {
        //get the corresponding clone
        int i;
        for (i = 0; i < originals.Count; i++) {
            if (originals[i] == original) break;
        }
        Transform clone = clones[i];

        clones.Remove(clone);
        originals.Remove(original);

        Destroy(clone.gameObject);
    }

    bool thisObjectCanCross(Transform obj) {
        //player always can cross
        if (obj.CompareTag("Player")) return true;

        //main filter
        if (!setup.filters.otherObjectsCanCross) return false;

        //filter: only these objects can cross
        if (setup.filters.tagsCanCross.Count > 0 && !setup.filters.tagsCanCross.Contains(obj.tag)) return false;

        //filter: these objects cannot cross
        if (setup.filters.tagsCannotCross.Contains(obj.tag)) return false;

        return true;
    }
}
