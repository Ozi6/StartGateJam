using UnityEngine;
using System;
using System.Collections;

public class Timer : MonoBehaviour
{
    public enum TimerMode { Countdown, Stopwatch }

    public TimerMode Mode { get; private set; } = TimerMode.Countdown;
    public float Duration { get; private set; }
    public float CurrentTime { get; private set; }
    public float RemainingTime => Duration - CurrentTime;
    public float Progress => Duration > 0 ? CurrentTime / Duration : 0f;
    public bool IsRunning { get; private set; }
    public bool IsPaused { get; private set; }

    public event Action OnTimerStart;
    public event Action OnTimerComplete;
    public event Action OnTimerPause;
    public event Action OnTimerResume;
    public event Action OnTimerStop;
    public event Action<float> OnTimerTick;

    private Coroutine timerCoroutine;

    public void StartCountdown(float duration, Action onComplete = null)
    {
        if (onComplete != null)
            OnTimerComplete += onComplete;

        Mode = TimerMode.Countdown;
        Duration = duration;
        CurrentTime = 0f;

        StartTimer();
    }

    public void StartStopwatch(Action<float> onTick = null)
    {
        if (onTick != null)
            OnTimerTick += onTick;

        Mode = TimerMode.Stopwatch;
        Duration = 0f;
        CurrentTime = 0f;

        StartTimer();
    }

    public void Pause()
    {
        if (IsRunning && !IsPaused)
        {
            IsPaused = true;
            OnTimerPause?.Invoke();
        }
    }

    public void Resume()
    {
        if (IsRunning && IsPaused)
        {
            IsPaused = false;
            OnTimerResume?.Invoke();
        }
    }

    public void Stop()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }

        IsRunning = false;
        IsPaused = false;
        OnTimerStop?.Invoke();
    }

    public void Reset()
    {
        Stop();
        CurrentTime = 0f;
    }

    public void Restart()
    {
        Stop();
        CurrentTime = 0f;
        StartTimer();
    }

    public void AddTime(float seconds)
    {
        if (Mode == TimerMode.Countdown)
        {
            Duration += seconds;
            Duration = Mathf.Max(0, Duration);
        }
    }

    private void StartTimer()
    {
        Stop();
        IsRunning = true;
        IsPaused = false;
        OnTimerStart?.Invoke();
        timerCoroutine = StartCoroutine(TimerCoroutine());
    }

    private IEnumerator TimerCoroutine()
    {
        while (IsRunning)
        {
            if (!IsPaused)
            {
                CurrentTime += Time.deltaTime;
                OnTimerTick?.Invoke(CurrentTime);
                if (Mode == TimerMode.Countdown && CurrentTime >= Duration)
                {
                    CurrentTime = Duration;
                    IsRunning = false;
                    OnTimerComplete?.Invoke();
                    yield break;
                }
            }
            yield return null;
        }
    }

    public static Timer Create(string name = "Timer")
    {
        GameObject obj = new GameObject(name);
        return obj.AddComponent<Timer>();
    }

    public static Timer CreateCountdown(float duration, Action onComplete = null, string name = "CountdownTimer")
    {
        Timer timer = Create(name);
        timer.OnTimerComplete += () => {
            if (timer != null && timer.gameObject != null)
                Destroy(timer.gameObject);
        };
        timer.StartCountdown(duration, onComplete);
        return timer;
    }

    private void OnDestroy()
    {
        Stop();
    }
}