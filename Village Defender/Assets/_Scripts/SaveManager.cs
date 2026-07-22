using System;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("Références")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private ResourceFarmManager resourceFarmManager;

    [Header("Sauvegarde automatique")]
    [SerializeField] private float autosaveInterval = 30f;

    [Header("Progression hors ligne")]
    [SerializeField] private float maximumOfflineHours = 24f;
    [SerializeField, Range(0f, 1f)] private float offlineProductionMultiplier = 0.5f;

    private string savePath;
    private float autosaveTimer;
    private bool sessionStarted;
    private bool isLoading;
    private bool offlineProgressAlreadyApplied;

    private const int CurrentSaveVersion = 1;
    private const string SaveFileName = "savegame.json";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        EnsureSavePath();
        Debug.Log("SaveManager : chemin de sauvegarde = " + savePath);
    }

    private void OnValidate()
    {
        autosaveInterval = Mathf.Max(1f, autosaveInterval);
        maximumOfflineHours = Mathf.Max(0f, maximumOfflineHours);
    }

    private void Update()
    {
        if (!sessionStarted)
            return;

        autosaveTimer += Time.unscaledDeltaTime;
        if (autosaveTimer < autosaveInterval)
            return;

        autosaveTimer = 0f;
        SaveGame();
    }

    public bool HasValidSave()
    {
        SaveData data;
        return TryReadSave(out data);
    }

    public void BeginNewGame()
    {
        sessionStarted = true;
        offlineProgressAlreadyApplied = false;
        autosaveTimer = 0f;
        SaveGame();
    }

    public bool LoadGameAndContinue()
    {
        if (isLoading)
        {
            Debug.LogWarning("SaveManager : chargement déjà en cours.");
            return false;
        }

        if (!TryReadSave(out SaveData data))
            return false;

        if (gameManager == null || resourceFarmManager == null)
        {
            Debug.LogError("SaveManager : chargement impossible, références GameManager ou ResourceFarmManager manquantes.");
            return false;
        }

        isLoading = true;
        offlineProgressAlreadyApplied = false;

        try
        {
            gameManager.PrepareForLoadedGame();

            gameManager.ApplyLoadedProgress(
                data.gold,
                data.wood,
                data.stone,
                data.iron,
                data.currentWaveIndex);

            resourceFarmManager.RestoreFarm(ResourceType.Wood, data.woodFarmPurchased);
            resourceFarmManager.RestoreFarm(ResourceType.Stone, data.stoneFarmPurchased);
            resourceFarmManager.RestoreFarm(ResourceType.Iron, data.ironFarmPurchased);

            ApplyOfflineProgress(data);

            gameManager.RefreshLoadedGameUI();

            sessionStarted = true;
            isLoading = false;
            autosaveTimer = 0f;
            SaveGame();

            gameManager.SetPhase(GameManager.GamePhase.Village);
            return true;
        }
        catch (Exception exception)
        {
            isLoading = false;
            sessionStarted = false;
            Debug.LogError("SaveManager : échec du chargement de la sauvegarde. " + exception.Message);
            return false;
        }
    }

    public void SaveGame()
    {
        if (!sessionStarted || isLoading)
            return;

        if (gameManager == null || resourceFarmManager == null)
        {
            Debug.LogWarning("SaveManager : sauvegarde ignorée, références GameManager ou ResourceFarmManager manquantes.");
            return;
        }

        SaveData data = new SaveData
        {
            saveVersion = CurrentSaveVersion,
            gold = gameManager.gold,
            wood = gameManager.wood,
            stone = gameManager.stone,
            iron = gameManager.iron,
            currentWaveIndex = gameManager.currentWaveIndex,
            woodFarmPurchased = resourceFarmManager.IsFarmPurchased(ResourceType.Wood),
            stoneFarmPurchased = resourceFarmManager.IsFarmPurchased(ResourceType.Stone),
            ironFarmPurchased = resourceFarmManager.IsFarmPurchased(ResourceType.Iron),
            lastSaveUtcTicks = DateTime.UtcNow.Ticks
        };

        try
        {
            EnsureSavePath();
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("SaveManager : impossible d'écrire la sauvegarde. " + exception.Message);
        }
    }

    public void DeleteSave()
    {
        try
        {
            EnsureSavePath();

            if (File.Exists(savePath))
                File.Delete(savePath);

            sessionStarted = false;
            offlineProgressAlreadyApplied = false;
            autosaveTimer = 0f;

            Debug.Log("SaveManager : sauvegarde supprimée.");
        }
        catch (Exception exception)
        {
            Debug.LogWarning("SaveManager : impossible de supprimer la sauvegarde. " + exception.Message);
        }
    }

    [ContextMenu("Delete Save File")]
    private void DeleteSaveFromInspector()
    {
        DeleteSave();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && sessionStarted)
            SaveGame();
    }

    private void OnApplicationQuit()
    {
        if (sessionStarted)
            SaveGame();
    }

    private bool TryReadSave(out SaveData data)
    {
        data = null;

        try
        {
            EnsureSavePath();

            if (!File.Exists(savePath))
                return false;

            string json = File.ReadAllText(savePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogWarning("SaveManager : sauvegarde illisible, fichier vide.");
                return false;
            }

            data = JsonUtility.FromJson<SaveData>(json);
            if (data == null)
            {
                Debug.LogWarning("SaveManager : sauvegarde illisible, JSON invalide.");
                return false;
            }

            if (data.saveVersion != CurrentSaveVersion)
            {
                Debug.LogWarning("SaveManager : version de sauvegarde inconnue : " + data.saveVersion + ".");
                data = null;
                return false;
            }

            if (!IsValidUtcTicks(data.lastSaveUtcTicks))
            {
                Debug.LogWarning("SaveManager : sauvegarde illisible, timestamp invalide.");
                data = null;
                return false;
            }

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("SaveManager : sauvegarde illisible. " + exception.Message);
            data = null;
            return false;
        }
    }

    private bool IsValidUtcTicks(long ticks)
    {
        if (ticks <= 0)
            return false;

        return ticks >= DateTime.MinValue.Ticks && ticks <= DateTime.MaxValue.Ticks;
    }

    private void EnsureSavePath()
    {
        if (string.IsNullOrEmpty(savePath))
            savePath = Path.Combine(Application.persistentDataPath, SaveFileName);
    }

    private void ApplyOfflineProgress(SaveData data)
    {
        if (offlineProgressAlreadyApplied || data == null || resourceFarmManager == null || gameManager == null)
            return;

        offlineProgressAlreadyApplied = true;

        DateTime nowUtc = DateTime.UtcNow;
        DateTime lastSaveUtc = new DateTime(data.lastSaveUtcTicks, DateTimeKind.Utc);
        TimeSpan elapsed = nowUtc - lastSaveUtc;

        if (elapsed < TimeSpan.Zero)
            elapsed = TimeSpan.Zero;

        double cappedSeconds = Math.Min(elapsed.TotalSeconds, maximumOfflineHours * 3600d);
        int woodGain = CalculateOfflineGain(ResourceType.Wood, data.woodFarmPurchased, cappedSeconds);
        int stoneGain = CalculateOfflineGain(ResourceType.Stone, data.stoneFarmPurchased, cappedSeconds);
        int ironGain = CalculateOfflineGain(ResourceType.Iron, data.ironFarmPurchased, cappedSeconds);

        gameManager.AddResource(ResourceType.Wood, woodGain);
        gameManager.AddResource(ResourceType.Stone, stoneGain);
        gameManager.AddResource(ResourceType.Iron, ironGain);

        data.lastSaveUtcTicks = nowUtc.Ticks;

        Debug.Log(
            "Progression hors ligne :\n" +
            "Bois +" + woodGain + "\n" +
            "Pierre +" + stoneGain + "\n" +
            "Fer +" + ironGain + "\n" +
            "Temps pris en compte : " + Mathf.FloorToInt((float)cappedSeconds) + " secondes");
    }

    private int CalculateOfflineGain(ResourceType resourceType, bool farmPurchased, double cappedSeconds)
    {
        if (!farmPurchased || cappedSeconds <= 0d)
            return 0;

        float productionPerSecond = resourceFarmManager.GetProductionPerSecond(resourceType);
        if (productionPerSecond <= 0f)
            return 0;

        double gain = cappedSeconds * productionPerSecond * offlineProductionMultiplier;
        return Mathf.Max(0, (int)Math.Floor(gain));
    }
}
