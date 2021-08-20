using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Phoder1.SaveableData
{
    public static class DataHandler
    {
        public static readonly object WriteReadLock = new object();
        #region Naming convention
        public static string PersistentPath = Application.persistentDataPath;
        public static string DirectoryPath => PersistentPath + "/Saves/";
        public static string GetJson(object saveObj) => JsonUtility.ToJson(saveObj, true);
        public static bool DirectoryExists(string path) => Directory.Exists(path);
        #endregion
        #region Observer
        private static List<ISaveable> saveables = new List<ISaveable>();
        public static void Subscribe(this ISaveable saveable)
        {
            if (saveables.Contains(saveable))
                return;

            saveables.Add(saveable);
        }
        public static bool Unsubscribe(this ISaveable saveable) => saveables.Remove(saveable);
        #endregion
        public static void ClearAllSavedData()
        {
            lock (WriteReadLock)
            {
                try
                {
                    var files = Directory.GetFiles(DirectoryPath);

                    foreach (var file in files)
                        File.Delete(file);

                    ReloadAll();
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
        }
        public static void SaveAll(Action<bool> callback = null)
        => _ = SaveAllAsync(saveables.ConvertAll((x) => (x, GetJson(x))), callback);
        public static async Task<bool> SaveAllAsync(List<(ISaveable data, string json)> saveables, Action<bool> callback = null)
        {
            bool success = true;

            foreach ((ISaveable data, string json) in saveables)
            {
                success &= await data.SaveAsync(json);

                if (!success)
                {
#if UNITY_EDITOR
                    Debug.LogError($"Error saving {data.GetType()}");
#endif
                    break;
                }
            }

            callback?.Invoke(success);
            return success;
        }
        public static void ReloadAll()
            => saveables.ForEach((x) => x.Load());
    }
}