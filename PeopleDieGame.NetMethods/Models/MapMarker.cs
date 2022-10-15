using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PeopleDieGame.NetMethods.Models
{
    public class MapMarker
    {
        public Guid Id { get; internal set; }
        public Vector3 Position { get; internal set; }
        public string Label { get; internal set; }
        public Color Color { get; internal set; }

        public MapMarker(Guid id, Vector3 position, string name = null, Color color = default)
        {
            Id = id;
            Position = position;
            Label = name;
            Color = color;
        }
    }
}
