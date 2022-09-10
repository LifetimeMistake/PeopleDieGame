using System;
using UnityEngine;

namespace UnturnedGameMaster
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
