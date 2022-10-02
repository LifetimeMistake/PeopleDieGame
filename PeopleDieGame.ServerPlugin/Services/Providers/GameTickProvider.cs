using System;
using UnityEngine;

namespace PeopleDieGame.ServerPlugin
{
    public class GameTickProvider : MonoBehaviour
    {
        public event EventHandler OnFixedUpdate;

        void FixedUpdate()
        {
            OnFixedUpdate?.Invoke(this, EventArgs.Empty);
        }
    }
}
