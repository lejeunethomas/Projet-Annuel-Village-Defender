using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class Era2ContentGenerator
{
    private const string MenuPath = "Tools/Village Defender/Generate Era 2 Content";
    private const string Era2SoldierAssetName = "Soldat2";
    private const string Wave6AssetName = "Wave6";
    private const string Wave7AssetName = "Wave7";

    [MenuItem(MenuPath)]
    public static void GenerateEra2Content()
    {
        EnemyData baseSoldier = FindUniqueEnemyData("Soldat");
        GameObject soldier2Prefab = FindUniquePrefab("Soldat2");
        WaveData wave5 = FindUniqueWaveData("Wave5");

        if (baseSoldier == null || soldier2Prefab == null || wave5 == null)
            return;

        string enemyDataDirectory = GetAssetDirectory(AssetDatabase.GetAssetPath(baseSoldier));
        string waveDataDirectory = GetAssetDirectory(AssetDatabase.GetAssetPath(wave5));

        EnemyData soldier2 = LoadOrCreateScriptableObject<EnemyData>(enemyDataDirectory + "/" + Era2SoldierAssetName + ".asset");
        WaveData wave6 = LoadOrCreateScriptableObject<WaveData>(waveDataDirectory + "/" + Wave6AssetName + ".asset");
        WaveData wave7 = LoadOrCreateScriptableObject<WaveData>(waveDataDirectory + "/" + Wave7AssetName + ".asset");

        if (soldier2 == null || wave6 == null || wave7 == null)
            return;

        UpdateSoldier2(baseSoldier, soldier2Prefab, soldier2);
        UpdateWaveFromWave5(wave5, soldier2, wave6, 1.10f, 1.05f, 6, 1.0f);
        UpdateWaveFromWave5(wave5, soldier2, wave7, 1.20f, 1.10f, 10, 1.2f);

        EditorUtility.SetDirty(soldier2);
        EditorUtility.SetDirty(wave6);
        EditorUtility.SetDirty(wave7);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.objects = new Object[] { soldier2, wave6, wave7 };
        EditorGUIUtility.PingObject(soldier2);
        EditorGUIUtility.PingObject(wave6);
        EditorGUIUtility.PingObject(wave7);

        Debug.Log("Era 2 Content Generator : asset généré/mis à jour : " + AssetDatabase.GetAssetPath(soldier2));
        Debug.Log("Era 2 Content Generator : asset généré/mis à jour : " + AssetDatabase.GetAssetPath(wave6));
        Debug.Log("Era 2 Content Generator : asset généré/mis à jour : " + AssetDatabase.GetAssetPath(wave7));
    }

    private static void UpdateSoldier2(EnemyData source, GameObject prefab, EnemyData target)
    {
        target.name = Era2SoldierAssetName;
        target.enemyName = "Soldat Ère 2";
        target.Type = source.Type;
        target.enemyPrefab = prefab;
        target.priority = source.priority;
        target.maxHealth = Mathf.CeilToInt(source.maxHealth * 1.30f);
        target.moveSpeed = source.moveSpeed;
        target.goldReward = Mathf.CeilToInt(source.goldReward * 1.25f);
        target.attackDamage = Mathf.CeilToInt(source.attackDamage * 1.20f);
        target.attackRate = source.attackRate;
        target.attackRange = source.attackRange;
    }

    private static void UpdateWaveFromWave5(
        WaveData wave5,
        EnemyData soldier2,
        WaveData targetWave,
        float countMultiplier,
        float rateMultiplier,
        int soldier2Count,
        float soldier2Rate)
    {
        targetWave.groups = new List<WaveData.EnemyGroup>();

        if (wave5.groups != null)
        {
            string wave5Path = AssetDatabase.GetAssetPath(wave5);
            foreach (WaveData.EnemyGroup sourceGroup in wave5.groups)
            {
                if (sourceGroup == null)
                    continue;

                float sourceRate = sourceGroup.rate;
                float scaledRate = 0f;
                if (sourceRate > 0f)
                {
                    scaledRate = sourceRate * rateMultiplier;
                }
                else
                {
                    Debug.LogWarning("Era 2 Content Generator : rate nulle ou négative protégée dans " + wave5Path + ".");
                }

                targetWave.groups.Add(new WaveData.EnemyGroup
                {
                    enemyType = sourceGroup.enemyType,
                    count = Mathf.CeilToInt(Mathf.Max(0, sourceGroup.count) * countMultiplier),
                    rate = scaledRate
                });
            }
        }

        targetWave.groups.Add(new WaveData.EnemyGroup
        {
            enemyType = soldier2,
            count = Mathf.Max(0, soldier2Count),
            rate = Mathf.Max(0.0001f, soldier2Rate)
        });

        targetWave.boss = false;
    }

    private static EnemyData FindUniqueEnemyData(string exactAssetName)
    {
        return FindUniqueAsset<EnemyData>(
            exactAssetName,
            "t:EnemyData " + exactAssetName,
            path =>
            {
                EnemyData data = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
                return data != null && data.name == exactAssetName && data.enemyName == exactAssetName;
            });
    }

    private static WaveData FindUniqueWaveData(string exactAssetName)
    {
        return FindUniqueAsset<WaveData>(
            exactAssetName,
            "t:WaveData " + exactAssetName,
            path =>
            {
                WaveData data = AssetDatabase.LoadAssetAtPath<WaveData>(path);
                return data != null && data.name == exactAssetName;
            });
    }

    private static GameObject FindUniquePrefab(string exactPrefabName)
    {
        return FindUniqueAsset<GameObject>(
            exactPrefabName,
            "t:Prefab " + exactPrefabName,
            path =>
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                return prefab != null && prefab.name == exactPrefabName;
            });
    }

    private static T FindUniqueAsset<T>(string displayName, string searchFilter, System.Func<string, bool> pathPredicate)
        where T : Object
    {
        string[] guids = AssetDatabase.FindAssets(searchFilter);
        List<string> matchingPaths = guids
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(pathPredicate)
            .Distinct()
            .OrderBy(path => path)
            .ToList();

        if (matchingPaths.Count == 0)
        {
            Debug.LogError("Era 2 Content Generator : aucun asset unique trouvé pour '" + displayName + "'. Filtre : " + searchFilter + ".");
            return null;
        }

        if (matchingPaths.Count > 1)
        {
            Debug.LogError("Era 2 Content Generator : plusieurs assets correspondent à '" + displayName + "', génération annulée :\n" + string.Join("\n", matchingPaths));
            return null;
        }

        return AssetDatabase.LoadAssetAtPath<T>(matchingPaths[0]);
    }

    private static T LoadOrCreateScriptableObject<T>(string path)
        where T : ScriptableObject
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset != null)
            return asset;

        Object existingAsset = AssetDatabase.LoadMainAssetAtPath(path);
        if (existingAsset != null)
        {
            Debug.LogError("Era 2 Content Generator : impossible de créer " + path + ", un asset d'un autre type existe déjà à ce chemin.");
            return null;
        }

        asset = ScriptableObject.CreateInstance<T>();
        asset.name = Path.GetFileNameWithoutExtension(path);
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private static string GetAssetDirectory(string assetPath)
    {
        return Path.GetDirectoryName(assetPath)?.Replace("\\", "/");
    }
}
