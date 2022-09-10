using UnityEngine;

namespace UnturnedGameMaster.Models
{
    public struct VectorPAR
    {
        public Vector3 Position { get; set; }
        public byte Rotation { get; set; }

        public VectorPAR(Vector3 position, byte rotation)
        {
            Position = position;
            Rotation = rotation;
        }
    }
}
