using UnityEngine;

public static class DebugUtils
{
    public static void DrawPoint( Vector3 point )
    {
        var up = point + Vector3.up;
        var down = point - Vector3.up;
        var left = point - Vector3.right;
        var right = point + Vector3.right;
        var forward = point + Vector3.forward;
        var back = point - Vector3.forward;

        Debug.DrawLine( up, down, Color.green, 1f );
        Debug.DrawLine( left, right, Color.red, 1f );
        Debug.DrawLine( forward, back, Color.blue, 1f );
    }

    public static void DrawPoint( Vector3 point, Color color )
    {
        var up = point + Vector3.up;
        var down = point - Vector3.up;
        var left = point - Vector3.right;
        var right = point + Vector3.right;
        var forward = point + Vector3.forward;
        var back = point - Vector3.forward;

        Debug.DrawLine( up, down, color, 1f );
        Debug.DrawLine( left, right, color, 1f );
        Debug.DrawLine( forward, back, color, 1f );
    }
}