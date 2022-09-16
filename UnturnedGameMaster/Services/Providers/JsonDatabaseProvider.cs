using Newtonsoft.Json;
using System.IO;
using UnityEngine;

namespace UnturnedGameMaster.Services.Providers
{
    public class JsonDatabaseProvider<T> : IDatabaseProvider<T>
    {
        private string path;
        private T collection;

        public JsonDatabaseProvider(string path)
        {
            this.path = path;
            Read();
        }

        public JsonDatabaseProvider(string path, T collection)
        {
            this.collection = collection;
            this.path = path;
            Write();
        }


        private bool Read()
        {
            if (!File.Exists(path))
                return false;

            Debug.Log($"Loading database from {Path.GetFullPath(path)}");

            collection = JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
            return (collection != null);
        }

        private bool Write()
        {
            Debug.Log($"Saving database to {Path.GetFullPath(path)}");
            string data = JsonConvert.SerializeObject(collection);
            File.WriteAllText(path, data);
            return true;
        }

        public T GetData()
        {
            return collection;
        }

        public bool CommitData()
        {
            return Write();
        }
    }
}
