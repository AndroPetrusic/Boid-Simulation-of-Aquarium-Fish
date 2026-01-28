using UnityEngine;
using System.Collections.Generic;

public class FoodManager : MonoBehaviour
{
    [Header("Food Settings")]
    public GameObject foodPrefab;
    public int maxFoodPieces = 20;
    
    [Header("Spawn Settings")]
    public float spawnHeight = 4f;
    
    private List<GameObject> activeFood = new List<GameObject>();
    private Camera mainCamera;
    
    void Start()
    {
        mainCamera = Camera.main;
    }
    
    void Update()
    {
        // Spawn food
        if (Input.GetMouseButtonDown(0))
        {
            SpawnFoodAtMousePosition();
        }
        
        // Cleaning up null food references 
        for (int i = activeFood.Count - 1; i >= 0; i--)
            if (activeFood[i] == null)
                activeFood.RemoveAt(i);
    }
    
    void SpawnFoodAtMousePosition()
    {
        if (foodPrefab == null || mainCamera == null) return;
        
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            Vector3 spawnPosition = hit.point + Vector3.up * 0.1f;
            SpawnFood(spawnPosition);
        }
        else
        {
            Vector3 spawnPosition = GetWorldPositionFromMouse(spawnHeight);
            SpawnFood(spawnPosition);
        }
    }
    
    void SpawnFoodAtRandomPosition()
    {
        if (foodPrefab == null) return;
        
        Vector3 spawnPosition = new Vector3(
            Random.Range(-3f, 3f),
            spawnHeight,
            Random.Range(-3f, 3f)
        );
        
        SpawnFood(spawnPosition);
    }
    
    void SpawnFood(Vector3 position)
    {
        // Max food limit checking
        if (activeFood.Count >= maxFoodPieces && activeFood.Count > 0)
        {
            GameObject oldestFood = activeFood[0];
            if (oldestFood != null)
                Destroy(oldestFood);
            activeFood.RemoveAt(0);
        }
        
        GameObject food = Instantiate(foodPrefab, position, Quaternion.identity);
        food.transform.parent = transform;
        food.tag = "Food";

        Rigidbody rb = food.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = food.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        
        SphereCollider collider = food.GetComponent<SphereCollider>();
        if (collider == null)
            collider = food.AddComponent<SphereCollider>();
        
        collider.isTrigger = true;
        collider.radius = 2e-4f;
        
        FishFood foodScript = food.AddComponent<FishFood>();
        foodScript.foodManager = this;
        
        activeFood.Add(food);
    }
    
    Vector3 GetWorldPositionFromMouse(float height)
    {
        if (mainCamera == null) return Vector3.zero;
        
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, new Vector3(0, height, 0));
        
        float distance;
        if (plane.Raycast(ray, out distance))
        {
            return ray.GetPoint(distance);
        }
        
        return new Vector3(0, spawnHeight, 0);
    }
    
    public GameObject GetClosestFood(Vector3 position, out float distance)
    {
        GameObject closest = null;
        distance = Mathf.Infinity;
        
        foreach (GameObject food in activeFood)
        {
            if (food == null) continue;
            
            float dist = Vector3.Distance(position, food.transform.position);
            if (dist < distance && dist < 5f)
            {
                distance = dist;
                closest = food;
            }
        }
        
        return closest;
    }
    
    public void RemoveFood(GameObject food)
    {
        if (activeFood.Contains(food))
            activeFood.Remove(food);
    }
}