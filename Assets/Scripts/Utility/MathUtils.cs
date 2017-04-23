using UnityEngine;

public static class MathUtils {
    public static Vector3 CubicBezierCurve( Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t )
    {
        var r = 1f - t;
        var f0 = r * r * r;
        var f1 = r * r * t * 3;
        var f2 = r * t * t * 3;
        var f3 = t * t * t;
        return f0 * p0 + f1 * p1 + f2 * p2 + f3 * p3;
    }
}
