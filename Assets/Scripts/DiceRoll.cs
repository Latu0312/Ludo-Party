using System.Collections;
using UnityEngine;

public class DiceRoll : MonoBehaviour
{
    Rigidbody rigid;

  
    [SerializeField] private float maxRandomTorqueForce, startRollingJumpForce;
    private float forceX, forceY, forceZ;
    
    public int diceFaceNum;
    private Vector3 startPosition;
    SoundManager soundManager;
    private bool hasPlayedSound = false;

    Vector3 currentVelocity;
    public float smoothTime = 0.3f;
    public float maxSpeed = 10f;

    public void Awake()
    {
        Intialize();
        startPosition = transform.position;
    }

   
    private void Update()
    {
        if(soundManager == null)
        {
            soundManager = GameObject.FindFirstObjectByType<SoundManager>();
        }
        if (rigid.linearVelocity.magnitude < 0.1f && rigid.angularVelocity.magnitude < 0.1f)
        {
            MoveDiceToCenter();
        }
    }

    public void RollDice()
    {
        soundManager.PlayRollDice();
        hasPlayedSound = false;
        forceX = Random.Range(-maxRandomTorqueForce, maxRandomTorqueForce);
        forceY = Random.Range(-maxRandomTorqueForce, maxRandomTorqueForce);
        forceZ = Random.Range(-maxRandomTorqueForce, maxRandomTorqueForce);

        Vector3 randomDirection = new Vector3(Random.Range(-0.1f, 0.1f), 0f, Random.Range(-0.1f, 0.1f));

        rigid.AddForce((Vector3.up + randomDirection) * startRollingJumpForce);
        rigid.AddTorque(forceX, forceY, forceZ);
    }

    private void Intialize()
    {
        rigid = GetComponent<Rigidbody>();
        if(rigid != null)
        {
            Debug.Log("Rigidbody found on " + gameObject.name);
        }
        transform.rotation = Random.rotation;
    }

    public void ResetDicePosition()
    {
        
        rigid.linearVelocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;

        
        transform.position = startPosition;
    }
    private void OnCollisionEnter(Collision collision)
    {
        
        if (!hasPlayedSound && collision.gameObject.CompareTag("Table"))
        {
            if (soundManager != null)
                soundManager.PlayRollSound();

            hasPlayedSound = true;
        }
    }

    private void MoveDiceToCenter()
    {
        transform.position = Vector3.SmoothDamp(transform.position, startPosition, ref currentVelocity, smoothTime, maxSpeed);

        if (Vector3.Distance(transform.position, startPosition) < 0.01f)
        {
            ResetDicePosition();
        }
    }
}
