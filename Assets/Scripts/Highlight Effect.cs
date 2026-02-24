using UnityEngine;

public class HighlightEffect : MonoBehaviour
{
    public float rotationSpeed = 60f;   
    public float bounceHeight = 1f;    
    public float bounceSpeed = 2f;      

    private Vector3 startLocalPos;
    private Quaternion uprightRotation;

    void Start()
    {
        
        startLocalPos = transform.localPosition;
        uprightRotation = Quaternion.Euler(90, 0, 0);
    }

    void Update()
    {
        
        transform.rotation = Quaternion.Euler(90, transform.rotation.eulerAngles.y, 0);

      
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime, Space.World);

       
        float newY = startLocalPos.y + Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
        Vector3 pos = startLocalPos;
        pos.y = newY;
        transform.localPosition = pos;
    }
}
