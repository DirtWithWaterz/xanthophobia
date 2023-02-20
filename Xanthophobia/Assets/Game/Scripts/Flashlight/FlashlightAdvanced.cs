using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FlashlightAdvanced : MonoBehaviour
{
#pragma warning disable 0108
    [SerializeField] Light light;
#pragma warning restore 0108
    public TMP_Text text;

    public TMP_Text batteryText;
    [SerializeField] Slider batterySlider;

    public float lifetime = 100;

    public float batteries = 0;

    public AudioSource flashON;
    public AudioSource flashOFF;

    private bool on;
    private bool off;




    void Start()
    {
        light = GetComponent<Light>();

        off = true;
        light.enabled = false;

    }



    void Update()
    {
        text.text = lifetime.ToString("0") + "%";
        batteryText.text = batteries.ToString();

        if (Input.GetKeyDown(KeyCode.F) && off)
        {
            flashON.Play();
            light.enabled = true;
            on = true;
            off = false;
        }

        else if (Input.GetKeyDown(KeyCode.F) && on)
        {
            flashOFF.Play();
            light.enabled = false;
            on = false;
            off = true;
        }

        if (on)
        {
            lifetime -= 0.06f * Time.deltaTime;
        }
        else if (off)
        {
            lifetime -= 0.03f * Time.deltaTime;
        }
        batterySlider.value = lifetime / 25;

        if (lifetime <= 0)
        {
            light.enabled = false;
            on = false;
            off = true;
            lifetime = 0;
        }

        if (lifetime >= 100)
        {
            lifetime = 100;
        }

        if (Input.GetKeyDown(KeyCode.B) && batteries >= 1)
        {
            batteries -= 1;
            lifetime += 100;
        }

        if (Input.GetKeyDown(KeyCode.B) && batteries == 0)
        {
            return;
        }

        if (batteries <= 0)
        {
            batteries = 0;
        }



    }

}
