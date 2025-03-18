using UnityEngine;

public static class SpawnPointManager
{
    private static string _targetSpawnPointName;

    public static void SetTargetSpawnPoint(string spawnPointName)
    {
        _targetSpawnPointName = spawnPointName;
    }

    public static Transform GetTargetSpawnPoint()
    {
        if (string.IsNullOrEmpty(_targetSpawnPointName))
        {
            Debug.LogWarning("Aucun spawn point cible n'a été défini.");
            return null;
        }

        GameObject spawnPoint = GameObject.Find(_targetSpawnPointName);
        if (spawnPoint == null)
        {
            Debug.LogWarning($"Spawn point '{_targetSpawnPointName}' introuvable dans la scène.");
            return null;
        }

        return spawnPoint.transform;
    }
}