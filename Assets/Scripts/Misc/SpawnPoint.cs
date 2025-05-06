using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    private static List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

    private void OnEnable()
    {
        if (!spawnPoints.Contains(this))
        {
            spawnPoints.Add(this);
            //Debug.Log($"SpawnPoint added at {transform.position}. Total spawn points: {spawnPoints.Count}");
        }
    }

    private void OnDisable()
    {
        if (spawnPoints.Remove(this))
        {
            //Debug.Log($"SpawnPoint removed at {transform.position}. Total spawn points: {spawnPoints.Count}");
        }
    }

    public static Vector3 GetRandomSpawnPos()
    {
        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning("No spawn points available!");
            return Vector3.zero;
        }

        int index = Random.Range(0, spawnPoints.Count);
        Vector3 position = spawnPoints[index].transform.position;
        Debug.Log($"Selected spawn point {index} at position {position}");
        return position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 1f);
    }
}