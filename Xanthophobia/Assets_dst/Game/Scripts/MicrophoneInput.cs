using UnityEngine;
using UnityEngine.UI; //for accessing Sliders and Dropdown
using System.Collections.Generic; // So we can use List<>

[RequireComponent(typeof(AudioSource))]
public class MicrophoneInput : MonoBehaviour {
	public float minThreshold = 0;
	public float frequency = 0.0f;
	public int audioSampleRate = 44100;
	public string microphone;
	public FFTWindow fftWindow;

	private List<string> options = new List<string>();
	private int samples = 8192; 
	private AudioSource audioSource;

    public float updateStep = 0.1f;
    public int sampleDataLength = 1024;
 
    private float currentUpdateTime = 0f;
 
    private float clipLoudness;
    private float[] clipSampleData;

    void Awake(){
        clipSampleData = new float[sampleDataLength];
    }

	void Start() {
		

		//get components you'll need
		audioSource = GetComponent<AudioSource> ();

		// get all available microphones
		foreach (string device in Microphone.devices) {
			if (microphone == null) {
				//set default mic to first mic found.
				microphone = device;
			}
			options.Add(device);
		}
		//initialize input with default mic
		UpdateMicrophone ();
	}

    void Update(){
        currentUpdateTime += Time.deltaTime;
        if (currentUpdateTime >= updateStep) {
            currentUpdateTime = 0f;
            audioSource.clip.GetData(clipSampleData, audioSource.timeSamples); //I read 1024 samples, which is about 80 ms on a 44khz stereo clip, beginning at the current sample position of the clip.
            clipLoudness = 0f;
            foreach (var sample in clipSampleData) {
                clipLoudness += Mathf.Abs(sample);
            }
            clipLoudness /= sampleDataLength; //clipLoudness is what you are looking for
            clipLoudness = clipLoudness > 0.2f ? 2 : clipLoudness > 0.05f ? 1 : 0;
        }
        HearingManager.Instance.OnSoundEmitted(gameObject, transform.position, EHeardSoundCategory.ETalking, clipLoudness);
    }

	void UpdateMicrophone(){
		audioSource.Stop(); 
		//Start recording to audioclip from the mic
		audioSource.clip = Microphone.Start(microphone, true, 10, audioSampleRate);
		audioSource.loop = true; 
		// Mute the sound with an Audio Mixer group becuase we don't want the player to hear it
		Debug.Log(Microphone.IsRecording(microphone).ToString());

		if (Microphone.IsRecording (microphone)) { //check that the mic is recording, otherwise you'll get stuck in an infinite loop waiting for it to start
			while (!(Microphone.GetPosition (microphone) > 0)) {
			} // Wait until the recording has started. 
		
			Debug.Log ("recording started with " + microphone);

			// Start playing the audio source
			audioSource.Play ();
		} else {
			//microphone doesn't work for some reason

			Debug.Log (microphone + " doesn't work!");
		}
	}
	
	public float GetAveragedVolume()
	{ 
		float[] data = new float[256];
		float a = 0;
		audioSource.GetOutputData(data,0);
		foreach(float s in data)
		{
			a += Mathf.Abs(s);
		}
		return a/256;
	}
	
	public float GetFundamentalFrequency()
	{
		float fundamentalFrequency = 0.0f;
		float[] data = new float[samples];
		audioSource.GetSpectrumData(data,0,fftWindow);
		float s = 0.0f;
		int i = 0;
		for (int j = 1; j < samples; j++)
		{
			if(data[j] > minThreshold) // volumn must meet minimum threshold
			{
				if ( s < data[j] )
				{
					s = data[j];
					i = j;
				}
			}
		}
		fundamentalFrequency = i * audioSampleRate / samples;
		frequency = fundamentalFrequency;
		return fundamentalFrequency;
	}
}