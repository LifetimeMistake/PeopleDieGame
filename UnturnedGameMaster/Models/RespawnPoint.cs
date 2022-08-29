using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnturnedGameMaster.Models
{
    public struct RespawnPoint
    {
        public Vector3 Position;
        public byte Rotation;

        public RespawnPoint(Vector3 position, byte rotation)
        {
            Position = position;
            Rotation = rotation;
        }
    }
}
