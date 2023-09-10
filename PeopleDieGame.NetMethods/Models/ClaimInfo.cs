using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PeopleDieGame.NetMethods.Models
{
    public struct ClaimInfo
    {
        public Vector3 Position;
        public float SquareRadius;

        public ClaimInfo(Vector3 position, float squareRadius)
        {
            Position = position;
            SquareRadius = squareRadius;
        }
    }
}
