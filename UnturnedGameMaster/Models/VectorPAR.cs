using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
