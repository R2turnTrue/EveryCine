using System;
using System.Collections;
using System.Collections.Generic;
using EveryCine;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    public static TestScript Instance;
    
    public ECCinema cinema;
    public GameObject o1;
    public GameObject o2;
    public ECClip clip;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Invoke("StartAnimation", 2.0f);
    }

    void StartAnimation()
    {
        cinema.SetupAnimation(clip)
            .SetGameObject("Var1", o1)
            .SetGameObject("Var2", o2)
            .Play();
    }

    public void ResumeAfter(ECCinemable cinemable, float seconds)
    {
        StartCoroutine(coResumeAfter(cinemable, seconds));
    }

    IEnumerator coResumeAfter(ECCinemable cine, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        cine.Resume();
    }
}
