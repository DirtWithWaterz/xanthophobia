//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;

//this script is not part of the portals system, it's only for the demo scene,
//only to ilustrate how other object can (or can't) cross portals

public class shootBalls : MonoBehaviour
{
    public GameObject prefabBall;
    public float shortThrowForce = 100f;
    public float longThrowForce = 250f;
    Transform projectilesContainer; //generated in Start()

    private void Start()
    {
        projectilesContainer = new GameObject("projectiles").transform;
    }
    void Update()
    {
        


        //restart scene (only on the demo scene)
        //if (Input.GetKeyDown(KeyCode.R)) UnityEngine.SceneManagement.SceneManager.LoadScene(0);

    }

    void ThrowProjectile(GameObject prefab) {
        //new projectile slightly in front of player
        GameObject projectile = Instantiate(
            prefab,
            transform.position + (transform.forward * 1f),
            transform.rotation,
            projectilesContainer
        );

        //add force
        projectile.GetComponent<Rigidbody>().AddForce(
            transform.forward * (Input.GetKey(KeyCode.LeftShift) ? longThrowForce : shortThrowForce),
            ForceMode.Impulse
        );

        //random color to the ball
        projectile.GetComponent<MeshRenderer>().material.color = Random.ColorHSV();
    }
}
