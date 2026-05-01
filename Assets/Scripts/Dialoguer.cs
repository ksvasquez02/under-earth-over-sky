using UnityEngine;
using System;
using System.Collections.Generic;

public class Dialoguer : MonoBehaviour
{
    public bool isActive = true;
    public string title;
    public List<string> entries = new List<string>();

    public float fadeTime = 5f;
    private float timer = 0f;

    public event Action OnTimerOff;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                timer = 0f;
                OnTimerOff.Invoke();
                Debug.Log("times up!");
            }
        }
    }

    public void StartFadeTimer()
    {
        timer = fadeTime;
    }
}
