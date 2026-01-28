using UnityEngine;
using System.Collections.Generic;

public class FishSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject fishPrefab;
    public int numberOfFish = 5;
    public Transform spawnArea;
    
    [Header("Fish Collider")]
    public float fishColliderRadius = 0.05f;
    
    private List<GameObject> spawnedFish = new List<GameObject>();
    
    void Start()
    {
        SpawnAllFish();
    }
    
    void SpawnAllFish()
    {
        if (fishPrefab == null) return;
        
        foreach (GameObject fish in spawnedFish)
            if (fish != null) Destroy(fish);
        
        spawnedFish.Clear();
        
        for (int i = 0; i < numberOfFish; i++)
            SpawnSingleFish(i);
    }
    
    void SpawnSingleFish(int index)
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();
        GameObject newFish = Instantiate(fishPrefab, spawnPosition, Random.rotation);
        newFish.name = "Fish_" + index;
        newFish.tag = "Fish";
        
        AddFishCollider(newFish);
        AddFishAI(newFish);
        
        spawnedFish.Add(newFish);
    }
    
    void AddFishCollider(GameObject fish)
    {
        // Ukloni postojeće collidere
        Collider[] existingColliders = fish.GetComponents<Collider>();
        foreach (Collider col in existingColliders)
            Destroy(col);
        
        // Dodaj NOVI sphere collider
        SphereCollider collider = fish.AddComponent<SphereCollider>();
        collider.radius = fishColliderRadius;
        collider.isTrigger = false; // BITNO: TRUE za detekciju hrane!
    }
    
    void AddFishAI(GameObject fish)
    {
        FishBoidAI ai = fish.AddComponent<FishBoidAI>();
        if (spawnArea != null)
            ai.waterBounds = spawnArea.gameObject;
    }
    
    Vector3 GetRandomSpawnPosition()
    {
        if (spawnArea != null)
        {
            Collider waterCollider = spawnArea.GetComponent<Collider>();
            if (waterCollider != null)
            {
                Bounds bounds = waterCollider.bounds;
                return new Vector3(
                    Random.Range(bounds.min.x + 0.5f, bounds.max.x - 0.5f),
                    Random.Range(bounds.min.y + 0.5f, bounds.max.y - 0.5f),
                    Random.Range(bounds.min.z + 0.5f, bounds.max.z - 0.5f)
                );
            }
        }
        
        return new Vector3(
            Random.Range(-4f, 4f),
            Random.Range(1f, 3f),
            Random.Range(-4f, 4f)
        );
    }
}