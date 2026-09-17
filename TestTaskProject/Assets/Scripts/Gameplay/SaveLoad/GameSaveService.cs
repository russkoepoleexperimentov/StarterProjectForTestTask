using UnityEngine;

namespace Gameplay.SaveLoad
{
    public class GameSaveService
    {
        private const string SAVE_KEY = "PlayerCarSave";

        // выставляется главным меню: означает, что после загрузки сцены
        // нужно применить сохранение, а не начинать заново
        public bool ContinueRequested { get; set; }

        public bool HasSave => PlayerPrefs.HasKey(SAVE_KEY);

        public void Save(CarSaveData data)
        {
            PlayerPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        public bool TryLoad(out CarSaveData data)
        {
            data = default;

            if (!HasSave) return false;

            var raw = PlayerPrefs.GetString(SAVE_KEY);

            if (string.IsNullOrEmpty(raw)) return false;

            try
            {
                data = JsonUtility.FromJson<CarSaveData>(raw);
                return true;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"Failed to read save: {exception.Message}");
                return false;
            }
        }

        public void DeleteSave()
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            PlayerPrefs.Save();
        }
    }
}
