using System.Collections;
using UnityEngine;

public class GlassEffect : MonoBehaviour
{
    public GameObject explosion;
    public SoundManager soundManager;

    private void Awake()
    {
        soundManager = GameObject.FindFirstObjectByType<SoundManager>();
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
        soundManager.PlayExplosionWallDetect();
        yield return new WaitForSeconds(0.5f);
        Destroy(exp);
    }
}
