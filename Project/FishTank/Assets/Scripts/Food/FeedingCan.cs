using UnityEngine;

public class FeedingCan : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float followSpeed = 20f;
    [SerializeField] private float heightAboveFood = 0.15f;
    
    [Header("Animation Reference")]
    [SerializeField] private GameObject animatedPart; // DRAG THE CHILD OBJECT WITH ANIMATOR HERE!
    
    private Camera mainCamera;
    private Animator animator;
    private Vector3 targetPosition;
    private RaycastHit hit;
    
    void Start()
    {
        mainCamera = Camera.main;

        transform.rotation = Quaternion.Euler(0, 0, 180);
    }
    
    void Update()
    {
        FollowMouse();
        
        if (Input.GetMouseButtonDown(0))
        {
            StartCoroutine(PlayFeedAnimation());
        }
    }
    
    void FollowMouse()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out hit))
        {
            targetPosition = hit.point + Vector3.up * (0.1f + heightAboveFood);
        }
        else
        {
            Vector3 spawnPosition = GetWorldPositionFromMouse(4f);
            targetPosition = spawnPosition + Vector3.up * heightAboveFood;
        }
        
        transform.position = Vector3.Lerp(
            transform.position, 
            targetPosition, 
            followSpeed * Time.deltaTime
        );
    }
    
    System.Collections.IEnumerator PlayFeedAnimation()
    {
        transform.rotation = Quaternion.identity;
        transform.rotation = Quaternion.Euler(0, 0, 180);

        float tiltAngle = -30f;
        float duration = 0.3f;
        
        Quaternion startRot = transform.rotation;
        
        Quaternion tiltRot = startRot * Quaternion.Euler(tiltAngle, 0, 0);
        
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            transform.rotation = Quaternion.Slerp(startRot, tiltRot, t);
            yield return null;
        }
        
        yield return new WaitForSeconds(0.1f);
        
        timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            transform.rotation = Quaternion.Slerp(tiltRot, startRot, t);
            yield return null;
        }
        
        transform.rotation = startRot;
    }
    
    Vector3 GetWorldPositionFromMouse(float height)
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, new Vector3(0, height, 0));
        
        float distance;
        if (plane.Raycast(ray, out distance))
        {
            return ray.GetPoint(distance);
        }
        
        return new Vector3(0, height, 0);
    }
}