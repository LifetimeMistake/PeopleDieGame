
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PeopleDieGame.ServerPlugin.Services.Managers
{
    public class TimerManager : IDisposableService
    {
        public delegate void TimerAction();
        private GameObject tickGameObject;
        private GameTickProvider gameTickProvider;
        private ulong tickCounter = 0;
        private Dictionary<TimerAction, ulong> timers = new Dictionary<TimerAction, ulong>();
        private Dictionary<TimerAction, ulong> timersPendingAddition = new Dictionary<TimerAction, ulong>();
        private List<TimerAction> timersPendingDeletion = new List<TimerAction>();
        private bool isEnumerating;

        public void Init()
        {
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
            if (timers.ContainsKey(timerAction) || timersPendingAddition.ContainsKey(timerAction))
                return false;

            if (isEnumerating)
                timersPendingAddition.Add(timerAction, interval);
            else
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
            if (isEnumerating)
            {
                if (timersPendingDeletion.Contains(timerAction))
                    return false;

                timersPendingDeletion.Add(timerAction);
                return true;
            }
            else
                return timers.Remove(timerAction);
        }

        private void GameTickProvider_OnFixedUpdate(object sender, EventArgs e)
        {
            if (timersPendingAddition.Count != 0)
            {
                foreach (KeyValuePair<TimerAction, ulong> kvp in timersPendingAddition)
                    timers.Add(kvp.Key, kvp.Value);

                timersPendingAddition.Clear();
            }

            if (timersPendingDeletion.Count != 0)
            {
                foreach (TimerAction timerAction in timersPendingDeletion)
                    timers.Remove(timerAction);

                timersPendingDeletion.Clear();
            }

            isEnumerating = true;
            foreach (KeyValuePair<TimerAction, ulong> kvp in timers)
            {
                if (tickCounter % kvp.Value == 0)
                {
                    kvp.Key.Invoke();
                }
            }
            isEnumerating = false;

            tickCounter++;
        }
    }
}
