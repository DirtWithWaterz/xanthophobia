using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class VoiceRecognition : MonoBehaviour {

    EnemyAI LinkedAI;
    private KeywordRecognizer keyWordRecognizer;
    private Dictionary<string, Action> actions = new Dictionary<string, Action>();

    GameObject candidateTarget;


    void Start(){
        actions.Add("come in command, Can you hear me? Over.", TestAction);
        actions.Add("can you hear me?", CyhmAnswer);

        keyWordRecognizer = new KeywordRecognizer(actions.Keys.ToArray());
        keyWordRecognizer.OnPhraseRecognized += RecognizedSpeech;
        keyWordRecognizer.Start();
        LinkedAI = GetComponent<EnemyAI>();
        candidateTarget = GameObject.FindGameObjectWithTag("Player");
    }

    private void CyhmAnswer()
    {
        Debug.Log("Yes I can :)");
    }

    private void TestAction()
    {
        Debug.Log("This is command, we can hear you loud and clear! Over.");
        
    }

    private void RecognizedSpeech(PhraseRecognizedEventArgs speech)
    {
        if (Vector3.Distance(LinkedAI.EyeLocation, candidateTarget.transform.position) <= LinkedAI.HearingRange){
            Debug.Log(speech.text);
            actions[speech.text].Invoke();
        }
        else{
            Debug.Log("Too far away for me to hear you!!");
        }
    }
}
