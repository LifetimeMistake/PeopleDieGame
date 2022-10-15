using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Services.Managers;
using Rocket.Unturned.Chat;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleDieGame.ServerPlugin.Services.Providers
{
    public class ObjectiveEventMessageProvider : IDisposableService
    {
        [InjectDependency]
        private ObjectiveManager objectiveManager { get; set; }

        public void Init()
        {
            objectiveManager.ObjectiveItemUpdated += ObjectiveManager_ObjectiveItemUpdated;
        }

        public void Dispose()
        {
            objectiveManager.ObjectiveItemUpdated -= ObjectiveManager_ObjectiveItemUpdated;
        }

        private void ObjectiveManager_ObjectiveItemUpdated(object sender, Models.EventArgs.ObjectiveItemEventArgs e)
        {
            ItemAsset itemAsset = Assets.find(EAssetType.ITEM, e.ObjectiveItem.ItemId) as ItemAsset;
            switch (e.ObjectiveItem.State)
            {
                case Enums.ObjectiveState.AwaitingDrop:
                    UnturnedChat.Say($"Bóg zstąpił z niebios i odebrał ludziom jeden z artefaktów :(");
                    break;
                case Enums.ObjectiveState.Roaming:
                    UnturnedChat.Say($"Artefakt \"{itemAsset.FriendlyName}\" został wygrzebany, sprawdź mapę!");
                    break;
                case Enums.ObjectiveState.Stored:
                    UnturnedChat.Say($"Artefakt \"{itemAsset.FriendlyName}\" został ukryty przez jednego z graczy.");
                    break;
                case Enums.ObjectiveState.Secured:
                    UnturnedChat.Say($"Artefakt \"{itemAsset.FriendlyName}\" został włożony do altaru!");
                    break;
            }
        }
    }
}
