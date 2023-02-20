using TMPro;
using UnityEngine;
public class FPS : MonoBehaviour
{
    private TextMeshProUGUI fpsText;

    private int runningTotalFps;

    public float secondsPerUpdate;
    private float currentTime = 0f;
    private int framesChecked = 0;

    private void Start()
    {
        fpsText = GetComponent<TextMeshProUGUI>();
        int currentFPS = (int)(1 / Time.unscaledDeltaTime);
        fpsText.SetText($"{currentFPS + 1}");

    }

    private void Update()
    {

        int currentFPS = (int)(1 / Time.unscaledDeltaTime);
        runningTotalFps += currentFPS;
        framesChecked++;
        currentTime += Time.deltaTime;

        if (!(currentTime >= secondsPerUpdate)) return;
        int averageFps = (int)(runningTotalFps / framesChecked);
        fpsText.SetText($"{averageFps + 1}");
        runningTotalFps = 0;
        currentTime = 0;
        framesChecked = 0;
    }
}
