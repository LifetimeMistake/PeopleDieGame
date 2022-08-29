using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnturnedGameMaster.Managers
{
    public class TimerManager : IDisposableManager
    {
        private GameObject tickGameObject;
        private GameTickProvider gameTickProvider;
        public event EventHandler OnFixedUpdate
        { 
            add
            {
                gameTickProvider.OnFixedUpdate += value;
            }
            remove
            {
                gameTickProvider.OnFixedUpdate -= value;
            }
        }

        public void Init()
        {
            tickGameObject = new GameObject();
            gameTickProvider = tickGameObject.AddComponent<GameTickProvider>();
            Debug.Log("Now pumping OnFixedUpdate events!");
        }

        public void Dispose()
        {
            GameObject.Destroy(tickGameObject);
            Debug.Log("Stopped pumping OnFixedUpdate events!");
        }
    }
}
