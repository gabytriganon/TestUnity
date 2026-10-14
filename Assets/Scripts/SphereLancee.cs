using UnityEngine;

// Sphère lancée par le héros : fait disparaître la créature qu'elle touche,
// et disparaît d'elle-même au bout de quelques secondes.
public class SphereLancee : MonoBehaviour
{
    public float dureeDeVie = 3f;

    void Start()
    {
        Destroy(gameObject, dureeDeVie);
    }

    void OnCollisionEnter(Collision collision)
    {
        Creature creature = collision.collider.GetComponentInParent<Creature>();
        if (creature == null) return;

        creature.Capturer();
        Destroy(gameObject);
    }
}
