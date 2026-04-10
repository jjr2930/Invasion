using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


public class TimeCheckingScope : IDisposable
{
    public string format;
    public DateTime startTime;
    public TimeCheckingScope(string format)
    {
        this.format = format;
        startTime = DateTime.Now;
    }

    public void Dispose()
    {
        TimeSpan elapsedTime = DateTime.Now - startTime;
        double totalSeconds = elapsedTime.TotalSeconds;
        double ticksPerSecond = (double)TimeSpan.TicksPerSecond;
        double elapsedSec = totalSeconds / ticksPerSecond;
        Debug.Log(string.Format(format, elapsedSec));
    }
}

