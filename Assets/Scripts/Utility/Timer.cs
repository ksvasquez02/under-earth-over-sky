
using System;
using UnityEngine;

[Serializable]
public class Timer
{
    public float totalTime = 0;
    public float timeRemaining = 0;
    public bool isRunning = false;
    public bool IsCounting { get { return timeRemaining > 0; } }

    public event Action OnComplete;

    public Timer(float totalTime, bool startNow = false)
    {
        this.totalTime = totalTime;
        this.isRunning = startNow;
    }
    public Timer(float totalTime, Action onComplete, bool startNow = false)
    {
        this.totalTime = totalTime;
        this.OnComplete += onComplete;
        this.isRunning = startNow;
    }

    public void Update()
    {
        if (!isRunning) return;

        if (IsCounting)
        {
            timeRemaining -= Time.deltaTime;
        }
        else
        {
            float overTime = timeRemaining;
            do
            {
                Do();
                overTime += totalTime;
            } while (totalTime > 0 && overTime < 0);
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
        OnComplete?.Invoke();
    }
}