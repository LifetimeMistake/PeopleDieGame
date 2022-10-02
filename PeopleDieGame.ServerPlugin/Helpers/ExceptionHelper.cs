using Rocket.API;
using System;
using UnityEngine;

namespace PeopleDieGame.ServerPlugin.Helpers
{
    public static class ExceptionHelper
    {
        public static void Handle(Exception ex, bool quiet = false)
        {
            Debug.LogError($"An exception has occurred:\n{ex}\nTraceback:\n{Environment.StackTrace}");

            if (!quiet)
            {
                ChatHelper.Say($"Wystąpił problem podczas wykonywania kodu: {ex.Message}, zobacz logi serwera w celu poznania szczegółów.");
            }
        }

        public static void Handle(Exception ex, string message)
        {
            Debug.LogError($"An exception has occurred:\n{message}\n{ex}\n{Environment.StackTrace}");
            ChatHelper.Say(message);
        }

        public static void Handle(Exception ex, IRocketPlayer caller, string message)
        {
            Debug.LogError($"An exception has occurred:\n{message}\n{ex}\n{Environment.StackTrace}");
            ChatHelper.Say(caller, message);
        }
    }
}
