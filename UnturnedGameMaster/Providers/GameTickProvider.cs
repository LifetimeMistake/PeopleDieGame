using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
