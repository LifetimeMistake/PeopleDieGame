﻿using System;

namespace PeopleDieGame.ServerPlugin.Models.EventArgs
{
    public class ObjectiveItemEventArgs : System.EventArgs
    {
        public ObjectiveItem ObjectiveItem;

        public ObjectiveItemEventArgs(ObjectiveItem objectiveItem)
        {
            ObjectiveItem = objectiveItem ?? throw new ArgumentNullException(nameof(objectiveItem));
        }
    }
}
