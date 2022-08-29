using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnturnedGameMaster.Models
{
    public class PlayerData
    {
        public ulong Id { get; private set; }
        public string Bio { get; private set; }
        public int? TeamId { get; set; }

        public PlayerData(ulong id, string bio = "", int? teamId = null)
        {
            Id = id;
            Bio = bio ?? throw new ArgumentNullException(nameof(bio));
            TeamId = teamId;
        }

        public void SetBio(string bio)
        {
            if (bio == null)
                throw new ArgumentNullException(nameof(bio));

            Bio = bio;
        }
    }
}
