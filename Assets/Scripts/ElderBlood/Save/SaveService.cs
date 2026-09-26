using System;
using System.IO;
using UnityEngine;

namespace ElderBlood.Save
{
    // Cross-cutting save backend: meta-progression as JSON in persistentDataPath.
    // Registered as a VContainer singleton; loads on construction.
    public sealed class SaveService
    {
        private const string FileName = "save.json";

        private readonly string _filePath;

        private SaveData _data;

        public PlayerProgressData Progress => _data.player;
        public VillageStateData Village => _data.village;
        public SettingsData Settings => _data.settings;

        public SaveService()
        {
            _filePath = Path.Combine(Application.persistentDataPath, FileName);
            Load();
        }

        public void Load()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _data = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
            }
            else
            {
                _data = new SaveData();
            }

            _data.player ??= new PlayerProgressData();
            _data.village ??= new VillageStateData();
            _data.settings ??= new SettingsData();
        }

        public void Save()
        {
            var json = JsonUtility.ToJson(_data, true);
            File.WriteAllText(_filePath, json);
        }

        [Serializable]
        private class SaveData
        {
            public PlayerProgressData player = new();
            public VillageStateData village = new();
            public SettingsData settings = new();
        }
    }
}
