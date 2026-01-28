using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 3f;
    public float zoomSpeed = 10f;
    public float minZoom = 2f;
    public float maxZoom = 20f;
    
    [Header("Input Settings")]
    public KeyCode rotateKey = KeyCode.Mouse1; // DESNI KLIK
    public KeyCode panKey = KeyCode.Mouse2;    // SREDNJI KLIK
    
    private Vector3 lastMousePosition;
    private bool isRotating = false;
    private bool isPanning = false;
    private float currentZoom;
    
    void Start()
    {
        currentZoom = transform.position.y;
    }
    
    void Update()
    {
        HandleRotation();
        HandlePan();
        HandleZoom();
    }
    
    void HandleRotation()
    {
        // START   Right mouse click rotation 
        if (Input.GetMouseButtonDown(1))
        {
            isRotating = true;
            lastMousePosition = Input.mousePosition;
            Cursor.visible = false;
        }
        
        // END   Right mouse click rotation 
        if (Input.GetMouseButtonUp(1))
        {
            isRotating = false;
            Cursor.visible = true; // Vrati kursor
        }
        
        if (isRotating)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            
            // Y axis rotation
            transform.RotateAround(transform.position, Vector3.up, delta.x * rotationSpeed);
            
            // X axis rotation
            transform.RotateAround(transform.position, transform.right, -delta.y * rotationSpeed);
            
            // Lock Z rotation
            Vector3 euler = transform.eulerAngles;
            euler.z = 0; 
            transform.eulerAngles = euler;
            
            lastMousePosition = Input.mousePosition;
        }
    }
    
    void HandlePan()
    {
        // START   Middle mouse click rotation 
        if (Input.GetMouseButtonDown(2))
        {
            isPanning = true;
            lastMousePosition = Input.mousePosition;
        }
        
        // END   Middle mouse click rotation 
        if (Input.GetMouseButtonUp(2))
        {
            isPanning = false;
        }
        
        if (isPanning)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            
            Vector3 right = transform.right;
            Vector3 forward = transform.forward;
            
            // Lock Y axis
            forward.y = 0;
            forward.Normalize();
            
            // Moving camera 
            Vector3 move = (-right * delta.x + -forward * delta.y) * moveSpeed * Time.deltaTime;
            transform.position += move;
            
            // Lock Y axis (keep the height)
            Vector3 pos = transform.position;
            pos.y = currentZoom;
            transform.position = pos;
            
            lastMousePosition = Input.mousePosition;
        }
    }
    
    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        
        if (Mathf.Abs(scroll) > 0.01f)
        {
            // Zoom in/out
            currentZoom -= scroll * zoomSpeed;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
            
            // Move camera on zoom
            Vector3 zoomMove = transform.forward * scroll * zoomSpeed;
            transform.position += zoomMove;
            
            currentZoom = transform.position.y;
        }
    }
    
    
    void OnEnable()
    {
        // Controlls
        Debug.Log("Kontrole:");
        Debug.Log("\t- Desni klik + miš = Rotacija kamere");
        Debug.Log("\t- Srednji klik + miš = Pomicanje kamere");
        Debug.Log("\t- Scroll wheel = Zoom in/out");
    }
}