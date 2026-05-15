using System.Numerics;

namespace BasyaFramework.Utils
{
    public static partial class Utils
    {
        public static Vector2 Bezier(float t, Vector2 a, Vector2 b, Vector2 c)
        {
            // float u = 1 - t;
            // float uu = u * u;
            // float tt = t * t;
            // float ut2 = 2 * u * t;
            //
            // Vector2 point = uu * a + ut2 * b + tt * c;
            // return point;
            var ab = Vector2.Lerp(a, b, t);
            var bc = Vector2.Lerp(b, c, t);
            return Vector2.Lerp(ab, bc, t);
        }
        
        public static Vector3 Bezier(float t, Vector3 a, Vector3 b, Vector3 c)
        {
            var ab = Vector3.Lerp(a, b, t);
            var bc = Vector3.Lerp(b, c, t);
            return Vector3.Lerp(ab, bc, t);
        }
    }
}