[System.Serializable]
public class SaveData
{
    public int saveVersion = 1;

    public int gold;
    public int wood;
    public int stone;
    public int iron;

    public int currentWaveIndex;

    public bool woodFarmPurchased;
    public bool stoneFarmPurchased;
    public bool ironFarmPurchased;

    public long lastSaveUtcTicks;
}
