using UnityEngine;

// Fait se promener une créature dans une petite zone autour de son point de départ,
// sans sortir de son biome. Les créatures volantes flottent au-dessus du sol.
[RequireComponent(typeof(Rigidbody))]
public class Errance : MonoBehaviour
{
    public Biome biome = Biome.Herbe;

    [Tooltip("Rayon de la zone de promenade autour du point de départ.")]
    public float rayon = 3f;

    public float vitesse = 1.2f;

    [Tooltip("Image de la créature, retournée selon le sens de la marche.")]
    public Transform image;

    Vector3 origine;
    Vector3 cible;
    float pause;
    Rigidbody corps;
    Creature creature;

    void Start()
    {
        corps = GetComponent<Rigidbody>();
        creature = GetComponent<Creature>();
        origine = transform.position;
        cible = origine;
        pause = Random.Range(0f, 2f);
    }

    void FixedUpdate()
    {
        if (creature != null && creature.EnCapture) return;

        if (pause > 0f)
        {
            pause -= Time.fixedDeltaTime;
            if (pause <= 0f) NouvelleCible();
            return;
        }

        Vector3 pos = corps.position;
        Vector3 versCible = cible - pos;
        versCible.y = 0f;
        if (versCible.magnitude < 0.1f)
        {
            pause = Random.Range(0.5f, 3f);
            return;
        }

        Vector3 suivant = pos + versCible.normalized * Mathf.Min(vitesse * Time.fixedDeltaTime, versCible.magnitude);
        if (!Hauteur(suivant, out float y))
        {
            // Le chemin sort du biome : on s'arrête et on choisit une autre direction.
            cible = pos;
            pause = 0.3f;
            return;
        }
        suivant.y = y;
        corps.MovePosition(suivant);
        Retourner(versCible);
    }

    bool Hauteur(Vector3 p, out float y)
    {
        Biome trouve = Biomes.Classer(p, out y);
        return Biomes.Convient(biome, trouve);
    }

    void NouvelleCible()
    {
        for (int essai = 0; essai < 8; essai++)
        {
            Vector2 c = Random.insideUnitCircle * rayon;
            Vector3 p = origine + new Vector3(c.x, 0f, c.y);
            if (Hauteur(p, out _))
            {
                cible = p;
                return;
            }
        }
        cible = origine;
    }

    // L'image regarde vers la gauche : on la retourne quand la créature va vers la droite de l'écran.
    void Retourner(Vector3 direction)
    {
        if (image == null || Camera.main == null) return;
        float versDroite = Vector3.Dot(direction, Camera.main.transform.right);
        if (Mathf.Abs(versDroite) < 0.01f) return;
        Vector3 s = image.localScale;
        s.x = Mathf.Abs(s.x) * (versDroite > 0f ? -1f : 1f);
        image.localScale = s;
    }
}
