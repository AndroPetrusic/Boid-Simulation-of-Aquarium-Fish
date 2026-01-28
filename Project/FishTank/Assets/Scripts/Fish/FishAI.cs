using UnityEngine;
using System.Collections.Generic;

public class FishBoidAI : MonoBehaviour
{
    [Header("Boid Settings")]
    public float separationWeight = 1.5f;
    public float seperationMargin = 3.2f;
    public float alignmentWeight = 1.0f;
    public float cohesionWeight = 3f;
    public float boundsWeight = 3.0f;
    
    [Header("Movement")]
    public float maxSpeed = 0.5f;
    public float rotationSpeed = 3f;
    
    [Header("Water Boundaries")]
    public GameObject waterBounds;
    public float boundaryMargin = 0.2f;
    public float boundaryForce = 3f;
    
    [Header("Detection")]
    public float neighborRadius = 0.5f;
    public LayerMask fishLayer;
    
    [Header("Fish Orientation Fix")]
    public Vector3 fishForwardAxis = Vector3.left;
    
    [Header("Food Attraction")]
    public float foodDetectionRange = 0.5f;
    public float foodAttractionWeight = 2f;
    
    [Header("Eating")]
    public float eatCooldown = 0.5f;
    
    private Vector3 velocity;
    private Collider waterCollider;
    private FoodManager foodManager;
    private float lastEatTime = 0f;
    
    void Start()
    {
        if (waterBounds != null) {waterCollider = waterBounds.GetComponent<Collider>();}
        
        foodManager = FindObjectOfType<FoodManager>();
        velocity = GetFishForward() * maxSpeed;
        
        // Collider for fish
        SetupFishCollider();
    }
    
    void SetupFishCollider()
    {
        SphereCollider collider = GetComponent<SphereCollider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<SphereCollider>();
        }
        
        collider.radius = 15e-4f;
        collider.isTrigger = false;
    }
    
    Vector3 GetFishForward(){return transform.TransformDirection(fishForwardAxis).normalized;}
    
    void Update()
    {
        List<Transform> neighbors = GetNeighbors();
        
        // Force keeping neighbours from overlaping
        Vector3 separation = CalculateSeparation(neighbors) * separationWeight;

        // Rotation towards average neighbour
        Vector3 alignment = CalculateAlignment(neighbors) * alignmentWeight;

        // Force towards average neighbour
        Vector3 cohesion = CalculateCohesion(neighbors) * cohesionWeight;

        // Force keeping it in aquarium
        Vector3 bounds = CalculateBoundsForce() * boundsWeight;

        // Force towards nearest food
        Vector3 foodAttraction = CalculateFoodAttraction() * foodAttractionWeight;
        
        // Sum of all vectors
        Vector3 acceleration = separation + alignment + cohesion + bounds + foodAttraction;
        velocity += acceleration * Time.deltaTime;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        
        ForceStayInBounds();
        RotateFishToVelocity();
        
        transform.Translate(fishForwardAxis * velocity.magnitude * Time.deltaTime, Space.Self);
    }
    
    
    void OnTriggerEnter(Collider other)
    {
        // Food detection
        if (other.CompareTag("Food"))
        {
            if (Time.time > lastEatTime + eatCooldown)
            {
                lastEatTime = Time.time;
                
                FishFood food = other.GetComponent<FishFood>();
                if (food != null) {food.Eat();}
            }
        }
    }
    
    Vector3 CalculateFoodAttraction()
    {
        if (foodManager == null) return Vector3.zero;
        
        float distance;
        GameObject closestFood = foodManager.GetClosestFood(transform.position, out distance);
        
        if (closestFood != null)
        {
            Vector3 toFood = (closestFood.transform.position - transform.position).normalized;
            return toFood;
        }
        
        return Vector3.zero;
    }
    
    void RotateFishToVelocity()
    {
        if (velocity.magnitude > 0.1f)
        {
            Vector3 desiredForward = velocity.normalized;
            Quaternion targetRotation = Quaternion.FromToRotation(GetFishForward(), desiredForward) * transform.rotation;
            
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
    void ForceStayInBounds()
    {
        if (waterCollider == null) return;
        
        Vector3 pos = transform.position;
        Bounds bounds = waterCollider.bounds;
        Bounds safeBounds = new Bounds(bounds.center, bounds.size - Vector3.one * boundaryMargin * 2);
        
        if (!safeBounds.Contains(pos))
        {
            Vector3 closestPoint = safeBounds.ClosestPoint(pos);
            Vector3 pushDirection = (closestPoint - pos).normalized;
            float distance = Vector3.Distance(pos, closestPoint);
            
            velocity += pushDirection * boundaryForce * (1f + distance) * Time.deltaTime;
            
            if (distance < 0.2f)
                transform.position = closestPoint;
        }
    }
    
    Vector3 CalculateBoundsForce()
    {
        if (waterCollider == null) return Vector3.zero;
        
        Vector3 boundsForce = Vector3.zero;
        Vector3 pos = transform.position;
        Bounds bounds = waterCollider.bounds;
        Bounds innerBounds = new Bounds(bounds.center, bounds.size - Vector3.one * boundaryMargin * 2);
        
        if (pos.x < innerBounds.min.x) boundsForce.x = (innerBounds.min.x - pos.x) * 3;
        else if (pos.x > innerBounds.max.x) boundsForce.x = (innerBounds.max.x - pos.x) * 3;
        
        if (pos.y < innerBounds.min.y) boundsForce.y = (innerBounds.min.y - pos.y) * 3;
        else if (pos.y > innerBounds.max.y) boundsForce.y = (innerBounds.max.y - pos.y) * 3;
        
        if (pos.z < innerBounds.min.z) boundsForce.z = (innerBounds.min.z - pos.z) * 3;
        else if (pos.z > innerBounds.max.z) boundsForce.z = (innerBounds.max.z - pos.z) * 3;
        
        return boundsForce;
    }
    
    List<Transform> GetNeighbors()
    {
        List<Transform> neighbors = new List<Transform>();
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, neighborRadius, fishLayer);
        
        foreach (Collider col in hitColliders)
            if (col.gameObject != gameObject && col.CompareTag("Fish"))
                neighbors.Add(col.transform);
        
        return neighbors;
    }
    
    Vector3 CalculateSeparation(List<Transform> neighbors)
    {
        if (neighbors.Count == 0) return Vector3.zero;
        
        Vector3 separation = Vector3.zero;
        foreach (Transform neighbor in neighbors)
        {
            Vector3 toNeighbor = transform.position - neighbor.position;
            float distance = toNeighbor.magnitude;
            if (distance < seperationMargin)
                separation += toNeighbor.normalized / Mathf.Max(distance, 0.1f);
        }
        return separation.normalized;
    }
    
    Vector3 CalculateAlignment(List<Transform> neighbors)
    {
        if (neighbors.Count == 0) return velocity.normalized;
        
        Vector3 avgDirection = Vector3.zero;
        foreach (Transform neighbor in neighbors)
        {
            FishBoidAI neighborAI = neighbor.GetComponent<FishBoidAI>();
            if (neighborAI != null)
                avgDirection += neighborAI.velocity;
        }
        
        avgDirection /= neighbors.Count;
        return (avgDirection.normalized - velocity.normalized) * 0.5f;
    }
    
    Vector3 CalculateCohesion(List<Transform> neighbors)
    {
        if (neighbors.Count == 0) return Vector3.zero;
        
        Vector3 center = Vector3.zero;
        foreach (Transform neighbor in neighbors)
            center += neighbor.position;
        
        center /= neighbors.Count;
        return (center - transform.position).normalized * 0.3f;
    }
}