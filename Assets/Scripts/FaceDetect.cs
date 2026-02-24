using System.Collections;
using UnityEngine;

public class FaceDetect : MonoBehaviour
{
    DiceRoll dice;
    public GameObject explosion;
    public SoundManager soundManager;

    public void Awake()
    {
        dice = FindFirstObjectByType<DiceRoll>();
        soundManager = GameObject.FindFirstObjectByType<SoundManager>();
    }

    private void OnTriggerStay(Collider other)
    {
        if(dice != null)
        {
            if(dice.GetComponent<Rigidbody>().linearVelocity.magnitude < 0.05f && dice.GetComponent<Rigidbody>().angularVelocity.magnitude < 0.05f)
            {
                dice.diceFaceNum = int.Parse(other.name);
            }
        }
    }

    private void OnCollisionEnter(Collision col)
    {
        if (col.collider.CompareTag("Dice"))
        {
           
            Vector3 contactPoint = col.contacts[0].point;
            StartCoroutine(Explosion(contactPoint));
        }
    }

    IEnumerator Explosion(Vector3 position)
    {
        GameObject exp = Instantiate(explosion, position, Quaternion.identity);
        soundManager.PlayExplosionFaceDetect();
        yield return new WaitForSeconds(0.5f);
        Destroy(exp);
    }
}
