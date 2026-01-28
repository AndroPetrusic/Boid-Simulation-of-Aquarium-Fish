using UnityEngine;
using System.Collections;

public class FishFood : MonoBehaviour
{
    [Header("Food Settings")]
    public float lifetime = 10f;
    public float sinkSpeed = 0.3f;
    public float eatRadius = 1e-3f;
    
    public FoodManager foodManager;
    private float lifeTimer = 0.4f;
    private float disappearDelay = 0.2f;
    private bool isBeingEaten = false;
    
    void Start()
    {
        if (foodManager == null)
            foodManager = FindObjectOfType<FoodManager>();
        
        SetupFoodCollider();
        
        gameObject.tag = "Food";
    }
    
    void SetupFoodCollider()
    {
        SphereCollider collider = GetComponent<SphereCollider>();
        if (collider == null)
            collider = gameObject.AddComponent<SphereCollider>();
        
        collider.isTrigger = true;
        collider.radius = eatRadius;
    }
    
    void Update()
    {
        if (!isBeingEaten)
        {
            transform.position += Vector3.down * sinkSpeed * Time.deltaTime;
            lifeTimer += Time.deltaTime;
            
            if (lifeTimer >= lifetime || transform.position.y < 0.2f)
                DestroyFood();
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Fish") && !isBeingEaten)
        {
            StartCoroutine(EatAfterDelay());
        }
    }
    
    public void Eat()
    {
        if (!isBeingEaten)
        {
            StartCoroutine(EatAfterDelay());
        }
    }
    
    IEnumerator EatAfterDelay()
    {
        isBeingEaten = true;
        yield return new WaitForSeconds(disappearDelay);
        DestroyFood();
    }
    
    void DestroyFood()
    {
        if (foodManager != null)
            foodManager.RemoveFood(gameObject);
        
        Destroy(gameObject);
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, eatRadius);
    }
}