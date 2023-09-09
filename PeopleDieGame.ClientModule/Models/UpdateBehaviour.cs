using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PeopleDieGame.NetMethods.Models
{
    public class UpdateBehaviour : MonoBehaviour
    {
        private bool active;
        public event EventHandler OnFixedUpdate;

        public void Start()
        {
            active = true;
        }

        public void Stop()
        {
            active = false;
        }

        public void FixedUpdate()
        {
            if (active)
            {
                OnFixedUpdate?.Invoke(this, System.EventArgs.Empty);
            }
        }
    }
}
