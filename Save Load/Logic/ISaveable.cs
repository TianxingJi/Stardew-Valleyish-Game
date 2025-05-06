namespace TFarm.Save
{
    public interface ISaveable
    {
        string GUID { get; }
        void RegisterSaveable()
        {
            SaveLoadManager.Instance.RegisterSaveable(this);
        }

        GameSaveData GenerateSaveData(); // Just method declaration

        void RestoreData(GameSaveData saveData);
    }

}