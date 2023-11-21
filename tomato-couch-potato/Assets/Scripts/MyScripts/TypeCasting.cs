using UnityEngine;

namespace trrne.Box
{
    public static class TypeCasting
    {
        public static Vector2 ToVec2(this Vector3 vector) => (Vector2)vector;

        public static Vector3 ToVec3(this Vector2 vector) => (Vector3)vector;
        public static Vector3 ToVec3(this Quaternion q) => q.eulerAngles;

        public static Quaternion ToQ(this Vector3 vector) => Quaternion.Euler(vector);
        public static Quaternion ToQ(this Vector2 vector) => Quaternion.Euler(vector);

        // from bool
        public static string ToString(this bool boo) => boo ? "true" : "false";
        public static int ToInt(this bool boo) => boo ? 1 : 0;
        public static float ToSingle(this bool boo) => boo ? 1f : 0f;

        // from string
        public static int ToInt(this string str) => int.Parse(str);
        public static float ToSingle(this string str) => float.Parse(str);
    }
}