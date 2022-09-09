using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnturnedGameMaster.Managers
{
    public class TimerManager : IDisposableManager
    {
        public delegate void TimerAction();
        private GameObject tickGameObject;
        private GameTickProvider gameTickProvider;
        private ulong tickCounter;
        private Dictionary<TimerAction, ulong> timers;

        public void Init()
        {
            timers = new Dictionary<TimerAction, ulong>();
            tickCounter = 0;
            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
            InitTickProvider();
        }

        public void Dispose()
        {
            SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
            DisposeTickProvider();
        }

        private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
        {
            // unity...
            InitTickProvider();
        }

        private void InitTickProvider()
        {
            DisposeTickProvider();
            tickGameObject = new GameObject();
            gameTickProvider = tickGameObject.AddComponent<GameTickProvider>();
            gameTickProvider.OnFixedUpdate += GameTickProvider_OnFixedUpdate;
        }

        private void DisposeTickProvider()
        {
            if (gameTickProvider != null)
            {
                gameTickProvider.OnFixedUpdate -= GameTickProvider_OnFixedUpdate;
                gameTickProvider = null;
            }

            if (tickGameObject != null)
            {
                GameObject.Destroy(tickGameObject);
                tickGameObject = null;
            }
        }

        public bool Register(TimerAction timerAction, ulong interval)
        {
            if (timers.ContainsKey(timerAction))
                return false;

            timers.Add(timerAction, interval);
            return true;
        }

        public bool UpdateInterval(TimerAction timerAction, ulong interval)
        {
            if (!timers.ContainsKey(timerAction))
                return false;

            timers[timerAction] = interval;
            return true;
        }

        public bool Unregister(TimerAction timerAction)
        {
            return timers.Remove(timerAction);
        }

        private void GameTickProvider_OnFixedUpdate(object sender, EventArgs e)
        {
            foreach(KeyValuePair<TimerAction, ulong> kvp in timers)
            {
                if (tickCounter % kvp.Value == 0)
                {
                    kvp.Key.Invoke();
                }
            }

            tickCounter++;
        }
    }
}
