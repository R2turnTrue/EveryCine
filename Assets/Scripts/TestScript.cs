using System.Collections;
using System.Collections.Generic;
using EveryCine;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    public ECCinema cinema;
    public GameObject o1;
    public GameObject o2;
    public ECClip clip;
    
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
}
