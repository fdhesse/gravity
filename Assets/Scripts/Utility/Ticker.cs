using UnityEngine;
using System.Collections;

public class Ticker
{
    private float rate;
    private float time;
    public Ticker(float rate, bool startReady)
    {
        this.rate = rate;
        time = startReady ? 0 : Time.time;
    }

    public bool isReady()
    {
        if ((Time.time > (time + rate)) || time == 0)
        {
            time = Time.time;
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool isOver()
    {
        return isReady();
    }

}

