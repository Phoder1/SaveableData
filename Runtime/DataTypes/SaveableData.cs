using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Phoder1.SaveableData
{
    public interface ISaveable
    {
        void Save(Action<bool> callback = null);
        void Load();
        Task<bool> SaveAsync(string json, Action<bool> callback = null);
        event Action OnSaveStarted;
        event Action OnSaveFinished;
    }
    public class SaveableData<T> : DirtyData, ISaveable where T : DirtyData, ISaveable, new()
    {
        #region Singleton
        protected SaveableData()
        {
            this.Subscribe();
        }
        private static T _instance = null;
        public static T Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                Load();

                return _instance;
            }
        }
        #endregion
        #region Naming Conventions
        //Naming conventions are used to consistently determin the directory in which you save the data by it's type
        public static string DirectoryPath => DataHandler.DirectoryPath;
        public static string FilePath => DirectoryPath + FileName + ".txt";
        public static string FileName => typeof(T).ToString().Replace("+", "_");
        public static bool FileExists() => File.Exists(FilePath);
        #endregion
        #region Save
        public void Save(Action<bool> callback = null) => _ = SaveAsync(DataHandler.GetJson(Instance), callback);
        public async Task<bool> SaveAsync(string json, Action<bool> callback = null)
        {
            var task = new Task<bool>(() => TrySave(json));
            task.Start();
            var success = await task;

            callback?.Invoke(success);
            return success;
        }
        private bool TrySave(string json)
        {
            Type type = typeof(T);
            if (!type.IsSerializable)
                throw new InvalidOperationException("A serializable Type is required");

            BeforeSave();

            if (!Instance.IsDirty)
                return true;

            if (Directory.Exists(DirectoryPath))
                return Save();
            else if (CreateDirectory())
                return Save();

            return false;

            bool Save()
            {
                lock (DataHandler.WriteReadLock)
                {
                    try
                    {
                        File.WriteAllText(FilePath, json);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                        return false;
                    }
                    Instance.Clean();
#if UNITY_EDITOR
                    Debug.Log("Saved " + Instance.GetType().ToString());
#endif
                    AfterSave();
                    return true;
                }
            }
            bool CreateDirectory()
            {
                lock (DataHandler.WriteReadLock)
                {
                    try
                    {
                        Directory.CreateDirectory(DirectoryPath);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                        return false;
                    }
                    return Directory.Exists(DirectoryPath);
                }
            }
        }
        #endregion
        #region Load
        void ISaveable.Load() => Load();
        public static void Load()
        {
            if (!TryLoad())
                _instance = new T();

            bool TryLoad()
            {
                if (!FileExists())
                    return false;

                string json = "";
                lock (DataHandler.WriteReadLock)
                    try
                    {
                        json = File.ReadAllText(FilePath);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(e);
                    }

                if (string.IsNullOrEmpty(json))
                    return false;

                if (_instance == null)
                    _instance = JsonUtility.FromJson<T>(json);
                else
                    JsonUtility.FromJsonOverwrite(json, _instance);

                if (_instance == null)
                    return false;

                _instance.Subscribe();
                OnLoad?.Invoke();
                return true;
            }
        }
        #endregion
        #region ISaveable
        public event Action OnSaveStarted;
        public event Action OnSaveFinished;
        public static event Action OnLoad;

        public virtual void BeforeSave()
        {
            OnSaveStarted?.Invoke();
        }
        public void AfterSave()
        {
            OnSaveFinished?.Invoke();
        }
        #endregion
    }
}
