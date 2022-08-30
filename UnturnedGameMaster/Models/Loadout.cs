using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace UnturnedGameMaster
{
    public class Loadout
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public Dictionary<int, int> Items { get; private set; }

        public Loadout(int id, string name, string description, Dictionary<int, int> items)
        {
            Id = id;
            SetName(name);
            SetDescription(description);
            Items = items ?? throw new ArgumentNullException(nameof(items));
        }

        public Loadout(int id, string name, string description = "")
        {
            Id = id;
            SetName(name);
            SetDescription(description);
            Items = new Dictionary<int, int>();
        }

        public bool AddItem(int itemId, int amount = 1)
        {
            if (Items.ContainsKey(itemId))
                return false;

            Items.Add(itemId, amount);
            return true;
        }

        public void AddItemOrSetAmount(int itemId, int amount = 1)
        {
            if (Items.ContainsKey(itemId))
                Items[itemId] = amount;
            else
                Items.Add(itemId, amount);
        }

        public void AddItemOrAddAmount(int itemId, int amount = 1)
        {
            if (Items.ContainsKey(itemId))
                Items[itemId] += amount;
            else
                Items.Add(itemId, amount);
        }

        public bool RemoveItem(int itemId)
        {
            return Items.Remove(itemId);
        }

        public int GetItemAmount(int itemId)
        {
            if (!Items.ContainsKey(itemId))
                return 0;

            return Items[itemId];
        }

        public bool SetItemAmount(int itemId, int amount)
        {
            if (!Items.ContainsKey(itemId))
                return false;

            Items[itemId] = amount;
            return true;
        }

        public Dictionary<int, int> GetItems()
        {
            return Items;
        }

        public void SetName(string name)
        {
            if(name == null)
                throw new ArgumentNullException(nameof(name));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name));

            Name = name;
        }

        public void SetDescription(string description)
        {
            if (description == null)
                throw new ArgumentNullException(nameof(description));

            Description = description;
        }
    }
}
