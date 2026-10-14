using UnityEngine;
using UnityEngine.InputSystem;

// Déplace le bonhomme avec Z (avancer), Q (gauche), S (reculer) et D (droite),
// et le fait sauter avec Espace.
// Les touches sont lues par leur position physique sur le clavier :
// sur un clavier AZERTY, ce sont bien Z Q S D (et W A S D sur un QWERTY).
[RequireComponent(typeof(Rigidbody))]
public class DeplacementBonhomme : MonoBehaviour
{
    [Header("Déplacement")]
    [Tooltip("Vitesse de déplacement en unités par seconde.")]
    public float vitesse = 5f;

    [Tooltip("Caméra utilisée pour savoir où est « devant ». Par défaut : la caméra principale.")]
    public Transform cameraReference;

    [Header("Saut")]
    [Tooltip("Hauteur maximale du saut, en unités.")]
    public float hauteurSaut = 1.5f;

    [Tooltip("Temps pour atteindre le sommet du saut, en secondes.")]
    public float tempsJusquauSommet = 0.35f;

    [Tooltip("La chute est plus rapide que la montée (saut plus nerveux).")]
    public float multiplicateurChute = 1.6f;

    [Tooltip("Relâcher Espace pendant la montée coupe le saut : petit saut sur un appui bref.")]
    public float multiplicateurSautCourt = 0.5f;

    [Tooltip("Délai pendant lequel on peut encore sauter juste après avoir quitté le sol.")]
    public float tempsGrace = 0.1f;

    [Tooltip("Un appui sur Espace juste avant d'atterrir est gardé en mémoire pendant ce délai.")]
    public float tempsMemoireSaut = 0.1f;

    Rigidbody corps;
    SphereCollider sphere;
    Vector3 direction;
    float dernierSol = -1f;
    float dernierAppuiSaut = -1f;
    bool espaceMaintenu;

    // Gravité et vitesse de départ calculées à partir de la hauteur et du temps voulus.
    float Gravite => 2f * hauteurSaut / (tempsJusquauSommet * tempsJusquauSommet);
    float VitesseSaut => Gravite * tempsJusquauSommet;

    void Awake()
    {
        corps = GetComponent<Rigidbody>();
        sphere = GetComponent<SphereCollider>();
        corps.freezeRotation = true;
        corps.useGravity = false; // on applique notre propre gravité, réglée pour le saut
        if (cameraReference == null && Camera.main != null)
            cameraReference = Camera.main.transform;
    }

    void Update()
    {
        var clavier = Keyboard.current;
        if (clavier == null)
        {
            direction = Vector3.zero;
            espaceMaintenu = false;
            return;
        }

        float avant = 0f;
        float cote = 0f;
        if (clavier.wKey.isPressed) avant += 1f; // Z sur AZERTY
        if (clavier.sKey.isPressed) avant -= 1f; // S
        if (clavier.aKey.isPressed) cote -= 1f;  // Q sur AZERTY
        if (clavier.dKey.isPressed) cote += 1f;  // D

        // « Devant » et « à droite » sont ceux de la caméra, ramenés à l'horizontale.
        Vector3 devant = Vector3.forward;
        Vector3 droite = Vector3.right;
        if (cameraReference != null)
        {
            devant = Vector3.ProjectOnPlane(cameraReference.forward, Vector3.up).normalized;
            droite = Vector3.ProjectOnPlane(cameraReference.right, Vector3.up).normalized;
        }

        direction = Vector3.ClampMagnitude(devant * avant + droite * cote, 1f);

        if (clavier.spaceKey.wasPressedThisFrame)
            dernierAppuiSaut = Time.time;
        espaceMaintenu = clavier.spaceKey.isPressed;
    }

    void FixedUpdate()
    {
        if (EstAuSol())
            dernierSol = Time.time;

        Vector3 v = direction * vitesse;
        v.y = corps.linearVelocity.y;

        // Saut : appui récent sur Espace et sol touché il y a très peu de temps.
        bool sautDemande = Time.time - dernierAppuiSaut <= tempsMemoireSaut;
        bool peutSauter = Time.time - dernierSol <= tempsGrace;
        if (sautDemande && peutSauter && v.y <= 0.01f)
        {
            v.y = VitesseSaut;
            dernierAppuiSaut = -1f;
            dernierSol = -1f;
        }

        // Gravité : plus forte en chute, et saut coupé si on relâche Espace en montant.
        float g = Gravite;
        if (v.y < 0f)
            g *= multiplicateurChute;
        else if (v.y > 0f && !espaceMaintenu)
            g /= multiplicateurSautCourt;
        v.y -= g * Time.fixedDeltaTime;

        corps.linearVelocity = v;
    }

    bool EstAuSol()
    {
        float rayon = sphere != null ? sphere.radius * transform.lossyScale.y : 0.5f;
        Vector3 centre = transform.position + (sphere != null ? sphere.center : Vector3.zero);
        return Physics.SphereCast(centre, rayon * 0.9f, Vector3.down, out _, rayon * 0.1f + 0.05f,
            ~0, QueryTriggerInteraction.Ignore);
    }
}
