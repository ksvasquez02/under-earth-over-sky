
using System;
using UnityEngine;

[Serializable]
public class Timer
{
    public float totalTime = 0;
    public float timeRemaining = 0;
    public bool isRunning = false;
    public bool isCounting { get { return timeRemaining > 0; } }

    public event Action onComplete;

    public Timer(float totalTime, bool startNow = false)
    {
        this.totalTime = totalTime;
        this.isRunning = startNow;
    }
    public Timer(float totalTime, Action onComplete, bool startNow = false)
    {
        this.totalTime = totalTime;
        this.onComplete += onComplete;
        this.isRunning = startNow;
    }

    public void Update()
    {
        if (!isRunning) return;

        if (isCounting)
        {
            timeRemaining -= Time.deltaTime;
        }
        else
        {
            Do();
        }
    }
    public void Reset()
    {
        timeRemaining = totalTime;
        isRunning = true;
    }
    public void Reset(float newTime)
    {
        totalTime = newTime;
        Reset();
    }
    public void Pause()
    {
        isRunning = false;
    }
    public void Play()
    {
        isRunning = true;
    }
    public void Do()
    {
        isRunning = false;
        onComplete?.Invoke();
        Debug.Log($"Timer complete at {Time.time}");
    }
}