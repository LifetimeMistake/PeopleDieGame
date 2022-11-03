using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Services.Managers;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleDieGame.ServerPlugin.Models
{
    public class Altar
    {
        public Vector3S? Position { get; set; }
        public double Radius { get; set; }
        public byte BoundId { get; set; }
        public bool ItemsSubmitted { get; set; }

        public Altar()
        {
            Position = null;
            Radius = 0;
            BoundId = 0;
            ItemsSubmitted = false;
        }

        public void SetPosition(Vector3S position)
        {
            byte boundId;
            if (!LevelNavigation.tryGetBounds(position, out boundId))
                throw new ArgumentException("Point is outside of navigation grid bounds.");

            BoundId = boundId;
            Position = position;
        }

        public void SetRadius(double radius)
        {
            if (radius < 0)
                throw new ArgumentOutOfRangeException(nameof(radius));

            Radius = radius;
        }
    }
}
