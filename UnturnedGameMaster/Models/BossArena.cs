using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnturnedGameMaster.Models
{
    public class BossArena
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public bool Conquered { get; set; }
        public Boss BossModel { get; private set; }

        public BossArena(int id, string name, bool conquered, Boss bossModel)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Conquered = conquered;
            BossModel = bossModel ?? throw new ArgumentNullException(nameof(bossModel));
        }

        public void SetName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            Name = name;
        }

        public void SetBoss(Boss model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            BossModel = model;
        }
    }
}
