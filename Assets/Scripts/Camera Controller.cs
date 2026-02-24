using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
   
    public CinemachineCamera cineCam; 
    
    public float horizontalSensitivity = 0.2f; 
   
    public float verticalSensitivity = 0.2f;
  
    public float zoomSpeed = 5f;
    public float minZoom = -20f;
    public float maxZoom = -5f;

    private CinemachineOrbitalFollow orbital;
    private bool isDragging = false;
    private Vector2 lastPos;

    void Start()
    {
        
        orbital = cineCam.GetComponent<CinemachineOrbitalFollow>();
        if (orbital == null)
        {
            Debug.LogError("Camera không có CinemachineOrbitalFollow!");
        }
    }

    void Update()
    {
       
        if (Input.GetMouseButtonDown(1))
        {
            isDragging = true;
            lastPos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            isDragging = false;
        }

        if (isDragging && orbital != null)
        {
            Vector2 delta = (Vector2)Input.mousePosition - lastPos;
            lastPos = Input.mousePosition;

            orbital.HorizontalAxis.Value += delta.x * horizontalSensitivity;
            orbital.VerticalAxis.Value -= delta.y * verticalSensitivity;
        }

       


#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(1))
        {
            isDragging = true;
            lastPos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            isDragging = false;
        }

        if (isDragging && orbital != null)
        {
            Vector2 delta = (Vector2)Input.mousePosition - lastPos;
            lastPos = Input.mousePosition;

            orbital.HorizontalAxis.Value += delta.x * horizontalSensitivity;
            orbital.VerticalAxis.Value -= delta.y * verticalSensitivity;
        }
#else
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                isDragging = true;
                lastPos = touch.position;
            }
            else if (touch.phase == TouchPhase.Moved && isDragging && orbital != null)
            {
                Vector2 delta = touch.position - lastPos;
                lastPos = touch.position;

                orbital.HorizontalAxis.Value += delta.x * horizontalSensitivity;
                orbital.VerticalAxis.Value -= delta.y * verticalSensitivity;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                isDragging = false;
            }
        }
#endif
    }
}
