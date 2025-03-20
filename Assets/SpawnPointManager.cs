using UnityEngine;

public static class SpawnPointManager
{
    private static string _targetSpawnPointName; // Nom du spawn point cible

    public static void SetTargetSpawnPoint(string spawnPointName)
    { // Stocker le nom du spawn point cible
        _targetSpawnPointName = spawnPointName;
    }

    public static Transform GetTargetSpawnPoint()
    {
        // Vérifier si le nom du spawn point cible est défini
        if (string.IsNullOrEmpty(_targetSpawnPointName))
        {
            Debug.LogWarning("Aucun spawn point cible n'a été défini.");
            return null;
        }

        // Trouver le spawn point cible dans la scène
        GameObject spawnPoint = GameObject.Find(_targetSpawnPointName);
        if (spawnPoint == null)
        {
            Debug.LogWarning($"Spawn point '{_targetSpawnPointName}' introuvable dans la scène.");
            return null;
        }

        // Retourner la position du spawn point
        return spawnPoint.transform;
    }
}