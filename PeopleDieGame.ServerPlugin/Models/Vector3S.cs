using UnityEngine;

namespace PeopleDieGame.ServerPlugin.Models
{
    public struct Vector3S
    {
        public float x;
        public float y;
        public float z;

        public Vector3S(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static implicit operator Vector3(Vector3S vector3S) => new Vector3(vector3S.x, vector3S.y, vector3S.z);
        public static implicit operator Vector3S(Vector3 vector) => new Vector3S(vector.x, vector.y, vector.z);

        public override string ToString()
        {
            return $"X: {x} Y: {y} Z: {z}";
        }
    }
}
