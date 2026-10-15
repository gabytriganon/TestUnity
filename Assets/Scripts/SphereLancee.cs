using System.Collections;
using UnityEngine;

// Sphère lancée par le héros. Si elle touche une créature : la créature entre dans
// la sphère, la sphère tombe au sol, oscille de droite à gauche, puis la capture
// et son image va dans un emplacement libre du menu.
public class SphereLancee : MonoBehaviour
{
    [Tooltip("Temps avant que la sphère disparaisse si elle ne touche aucune créature.")]
    public float dureeDeVie = 4f;

    [Tooltip("Angle de chaque oscillation, en degrés.")]
    public float angleOscillation = 25f;

    [Tooltip("Nombre d'oscillations avant la capture.")]
    public int nombreOscillations = 3;

    Rigidbody corps;
    bool enCapture;
    float inclinaison;

    void Start()
    {
        corps = GetComponent<Rigidbody>();
        Invoke(nameof(Disparaitre), dureeDeVie);
    }

    void Disparaitre()
    {
        if (!enCapture) Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (enCapture) return;
        Creature creature = collision.collider.GetComponentInParent<Creature>();
        if (creature == null || creature.EnCapture) return;

        // Plus de place dans le menu : la sphère rebondit simplement.
        MenuCreatures menu = FindAnyObjectByType<MenuCreatures>();
        if (menu != null && !menu.APlaceLibre) return;

        StartCoroutine(Capturer(creature, menu));
    }

    IEnumerator Capturer(Creature creature, MenuCreatures menu)
    {
        enCapture = true;
        creature.EnCapture = true;
        corps.linearVelocity = Vector3.zero;
        corps.angularVelocity = Vector3.zero;
        corps.isKinematic = true;

        // 1. La créature rétrécit et entre dans la sphère.
        Transform t = creature.transform;
        Vector3 positionDepart = t.position;
        Vector3 echelleDepart = t.localScale;
        for (float f = 0f; f < 1f; f += Time.deltaTime / 0.5f)
        {
            t.localScale = Vector3.Lerp(echelleDepart, Vector3.zero, f);
            t.position = Vector3.Lerp(positionDepart, transform.position, f);
            yield return null;
        }
        creature.gameObject.SetActive(false);

        // 2. La sphère retombe au sol, sans rouler.
        corps.isKinematic = false;
        corps.freezeRotation = true;
        float debut = Time.time;
        yield return new WaitForSeconds(0.25f);
        while (Time.time - debut < 2.5f && corps.linearVelocity.sqrMagnitude > 0.02f)
            yield return null;
        corps.isKinematic = true;

        // 3. Le bouton se tourne vers la caméra, puis la sphère oscille de droite à gauche.
        Vector3 versCamera = Camera.main != null ? Camera.main.transform.position - transform.position : -transform.forward;
        versCamera.y = 0f;
        if (versCamera.sqrMagnitude < 0.001f) versCamera = Vector3.back;
        Quaternion repos = Quaternion.LookRotation(versCamera.normalized, Vector3.up);
        transform.rotation = repos;
        yield return new WaitForSeconds(0.4f);

        for (int i = 0; i < nombreOscillations; i++)
        {
            yield return Pencher(repos, angleOscillation, 0.15f);
            yield return Pencher(repos, -angleOscillation, 0.3f);
            yield return Pencher(repos, 0f, 0.15f);
            yield return new WaitForSeconds(0.5f);
        }

        // 4. Capturée : l'image va dans le premier emplacement libre du menu.
        if (menu != null) menu.Ajouter(creature.portrait);
        Vector3 echelle = transform.localScale;
        for (float f = 0f; f < 1f; f += Time.deltaTime / 0.2f)
        {
            transform.localScale = echelle * (1f + 0.25f * Mathf.Sin(f * Mathf.PI));
            yield return null;
        }
        transform.localScale = echelle;
        yield return new WaitForSeconds(0.8f);

        Destroy(creature.gameObject);
        Destroy(gameObject);
    }

    IEnumerator Pencher(Quaternion repos, float cible, float duree)
    {
        float depart = inclinaison;
        for (float f = 0f; f < 1f; f += Time.deltaTime / duree)
        {
            inclinaison = Mathf.Lerp(depart, cible, Mathf.SmoothStep(0f, 1f, f));
            transform.rotation = repos * Quaternion.AngleAxis(inclinaison, Vector3.forward);
            yield return null;
        }
        inclinaison = cible;
        transform.rotation = repos * Quaternion.AngleAxis(inclinaison, Vector3.forward);
    }
}
