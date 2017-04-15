using System;
using UnityEngine;
using System.Collections;

public class CoroutineHelper : MonoBehaviour {
    public static IEnumerator ContinuouslyDo( Action action )
    {
        while ( true )
        {
            action();
            yield return new WaitForFixedUpdate();
        }
    }

    public static IEnumerator DoEvery( float step, Action action )
    {
        while ( true )
        {
            action();
            yield return new WaitForSeconds(step);
        }
    }

    public static IEnumerator WaitAndDo( float delay, Action action )
    {
        yield return new WaitForSeconds( delay );
        action();
    }

}
