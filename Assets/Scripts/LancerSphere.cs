using UnityEngine;
using UnityEngine.InputSystem;

// Appuyer sur E lance une petite sphère devant le personnage, en cloche.
public class LancerSphere : MonoBehaviour
{
    [Tooltip("Le modèle visible du personnage : la sphère part dans la direction où il regarde.")]
    public Transform modele;

    [Tooltip("Matériau de la sphère lancée.")]
    public Material materiau;

    [Tooltip("Diamètre de la sphère.")]
    public float taille = 0.25f;

    [Tooltip("Vitesse vers l'avant.")]
    public float vitesseAvant = 12f;

    [Tooltip("Vitesse vers le haut au départ (donne la courbe du lancer).")]
    public float vitesseHaut = 3f;

    [Tooltip("Temps minimum entre deux lancers, en secondes.")]
    public float delaiEntreLancers = 0.4f;

    float dernierLancer = -10f;

    void Update()
    {
        var clavier = Keyboard.current;
        if (clavier == null || !clavier.eKey.wasPressedThisFrame) return;
        if (Time.time - dernierLancer < delaiEntreLancers) return;
        dernierLancer = Time.time;
        Lancer();
    }

    void Lancer()
    {
        Transform repere = modele != null ? modele : transform;
        Vector3 devant = repere.forward;
        Vector3 depart = transform.position + Vector3.up * 0.3f + devant * 0.5f;

        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "Sphere lancee";
        sphere.transform.position = depart;
        sphere.transform.localScale = Vector3.one * taille;
        if (materiau != null)
            sphere.GetComponent<Renderer>().sharedMaterial = materiau;

        // La sphère ne doit pas toucher le personnage qui la lance.
        Collider colSphere = sphere.GetComponent<Collider>();
        foreach (Collider c in GetComponentsInChildren<Collider>())
            Physics.IgnoreCollision(colSphere, c);

        Rigidbody corps = sphere.AddComponent<Rigidbody>();
        corps.mass = 0.2f;
        corps.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        corps.linearVelocity = devant * vitesseAvant + Vector3.up * vitesseHaut;

        sphere.AddComponent<SphereLancee>();
    }
}
