using System;
using UnityEngine;

public class Ticker : MonoBehaviour
{
    public static float tickTime = 0.2f;

    private float _tickerTimer;

    public delegate void TickAction();

    public static TickAction OnTickAction;

    private void Update()
    {
        _tickerTimer += Time.deltaTime;

        if (_tickerTimer >= tickTime)
        {
            _tickerTimer = 0;
            TickEvent();
        }
    }

    private void TickEvent()
    {
        OnTickAction?.Invoke();
    }
}
