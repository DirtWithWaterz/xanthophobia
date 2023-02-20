using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class Camcorder : MonoBehaviour
{
    [SerializeField] Image Batt01;
    [SerializeField] Slider Batt02;
    [SerializeField] Image Batt03;
    [SerializeField] VideoPlayer videoPlayer;
    [SerializeField] Text dateTime;
    string month;

    bool BattBlinkOn = false;

    void Awake()
    {
#if !UNITY_EDITOR
        videoPlayer.enabled = true;
        videoPlayer.Play();
        videoPlayer.targetCameraAlpha = 1;
#endif
    }

    void Start()
    {
        if (DateTime.Now.Month == 1)
        {
            month = "JAN";
        }
        else if (DateTime.Now.Month == 2)
        {
            month = "FEB";
        }
        else if (DateTime.Now.Month == 3)
        {
            month = "MAR";
        }
        else if (DateTime.Now.Month == 4)
        {
            month = "APR";
        }
        else if (DateTime.Now.Month == 5)
        {
            month = "MAY";
        }
        else if (DateTime.Now.Month == 6)
        {
            month = "JUN";
        }
        else if (DateTime.Now.Month == 7)
        {
            month = "JUL";
        }
        else if (DateTime.Now.Month == 8)
        {
            month = "AUG";
        }
        else if (DateTime.Now.Month == 9)
        {
            month = "SEP";
        }
        else if (DateTime.Now.Month == 10)
        {
            month = "OCT";
        }
        else if (DateTime.Now.Month == 11)
        {
            month = "NOV";
        }
        else
        {
            month = "DEC";
        }
    }

    void Update()
    {
        if (!videoPlayer.isPlaying)
        {
            StartCoroutine(WaitaSec());
        }
        else
        {
            videoPlayer.enabled = true;
            videoPlayer.targetCameraAlpha = 1;
        }
        if (Batt02.value == 0)
        {
            if (!BattBlinkOn)
                StartCoroutine(BatteryBlinkOn());
            if (BattBlinkOn)
                StartCoroutine(BatteryBlinkOff());

        }
        else { Batt01.enabled = true; }

        dateTime.text = DateTime.Now.ToString("hh:mm tt", CultureInfo.InstalledUICulture) + "\n" + DateTime.Now.Date.ToString("dd") + " " + month + ". " + DateTime.Now.Year;
    }

    private IEnumerator WaitaSec()
    {
        yield return new WaitForSeconds(2);
        if (!videoPlayer.isPlaying)
        {
            videoPlayer.enabled = false;
            videoPlayer.targetCameraAlpha = 0;
        }
        else { videoPlayer.targetCameraAlpha = 1; }
    }

    private IEnumerator BatteryBlinkOff()
    {
        yield return new WaitForSeconds(0.3f);
        Batt01.enabled = false;
        Batt03.enabled = false;
        BattBlinkOn = false;
    }

    private IEnumerator BatteryBlinkOn()
    {
        yield return new WaitForSeconds(0.3f);
        Batt01.enabled = true;
        Batt03.enabled = false;
        BattBlinkOn = true;
    }
}
