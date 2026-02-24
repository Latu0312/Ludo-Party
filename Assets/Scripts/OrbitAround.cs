using System.Collections;
using UnityEngine;

public class OrbitAround : MonoBehaviour
{
     public Transform target;
    public float orbitSpeed = 180f;
    public float distance = 2.5f;
    public float selfRotateSpeed = 180f;

    private Vector3 offset;

    private void Start()
    {
        if (target != null)
        {
            offset = Quaternion.identity * (Vector3.right * distance + new Vector3(0, 1.7f, 0));
            transform.position = target.position + offset;
        }
    }

    private float currentAngle = 0f;

    private void Update()
    {
        if (target == null) return;

       
        currentAngle += orbitSpeed * Time.deltaTime;

       
        Quaternion rotation = Quaternion.Euler(0, currentAngle, 0);
        Vector3 rotatedOffset = rotation * offset;

        
        transform.position = target.position + rotatedOffset;

        
        transform.Rotate(Vector3.up, selfRotateSpeed * Time.deltaTime);
    }

}
