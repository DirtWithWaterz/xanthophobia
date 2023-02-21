using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BatteryInteraction : Interactable
{
    public TMP_Text pickUpText;
    private GameObject pickupTextHolder;
    [SerializeField] private FlashlightAdvanced flashlight;
    private GameObject flashlightAdvancedHolder;

    public AudioSource pickUpSound;

    void Start()
    {
        flashlightAdvancedHolder = GameObject.FindGameObjectWithTag("Flashlight");
        pickupTextHolder = GameObject.FindGameObjectWithTag("PickUpText");
        pickUpText = pickupTextHolder.GetComponent<TMP_Text>();
        flashlight = flashlightAdvancedHolder.GetComponent<FlashlightAdvanced>();
        pickUpText.enabled = false;
    }

    public override void OnFocus(GameObject i)
    {
        if(!pickUpSound.isPlaying)
        {
        if(i.tag == "Player"){
            if(!pickUpText.enabled){
                pickUpText.enabled = true;
                if(pickUpText.text == "")
                    pickUpText.text = "PICK UP\n" + "[" + "E" + "]";
            }
        }
        }
    }

    public override void OnHoldInteract(GameObject i)
    {
        return;
    }

    public override void OnInteract(GameObject i)
    {
        if(i.tag == "Player"){
            flashlight.batteries += 1;
            pickUpSound.Play();
            HearingManager.Instance.OnSoundEmitted(gameObject, transform.position, EHeardSoundCategory.EPickUp_Small, 0.06f);
            pickUpText.text = "";
            pickUpText.enabled = false;
            StartCoroutine(WaitToDestroy());
        }
    } private IEnumerator WaitToDestroy(){
        if(pickUpSound.isPlaying){
            yield return new WaitForSeconds(0.2f);
            Destroy(this.gameObject);
        } else{
            Destroy(this.gameObject);
        }
    }

    public override void OnLoseFocus(GameObject i)
    {
        if(i.tag == "Player"){
            pickUpText.text = "";
            pickUpText.enabled = false;
        }
    }

    public override void OnReleaseInteract(GameObject i)
    {
        return;
    }
}
