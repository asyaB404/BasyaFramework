using System.Numerics;

namespace BasyaFramework.Utils
{
    public static partial class Utils
    {
        public static Vector2 Bezier(float t, Vector2 a, Vector2 b, Vector2 c)
        {
            var ab = Vector2.Lerp(a, b, t);
            var bc = Vector2.Lerp(b, c, t);
            return Vector2.Lerp(ab, bc, t);
        }
    }
}