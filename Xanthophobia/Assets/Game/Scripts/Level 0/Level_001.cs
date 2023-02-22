using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level_001 : MonoBehaviour
{

    GameObject doorWay;
    GameObject wall_divider_med;
    GameObject sublevel_001;
    GameObject target;
    GameObject player;
    Rigidbody rb;
    public bool Entered = false;
    public bool Entering = false;

    void Awake()
    {
        doorWay = GameObject.FindGameObjectWithTag("0.01 entrance");
        wall_divider_med = GameObject.FindGameObjectWithTag("0.01 entrance wr");
        sublevel_001 = GameObject.FindGameObjectWithTag("sublevel 0.01");
        player = GameObject.FindGameObjectWithTag("Player");
        rb = player.GetComponent<Rigidbody>();
    }

    // Start is called before the first frame update
    void Start()
    {
        doorWay.SetActive(true);
        wall_divider_med.SetActive(false);
        sublevel_001.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(Entering){
            sublevel_001.SetActive(true);
        }
        if(Entered){
            doorWay.SetActive(false);
            wall_divider_med.SetActive(true);
        }
    }

}
