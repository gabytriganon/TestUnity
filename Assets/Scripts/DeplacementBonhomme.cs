using UnityEngine;
using UnityEngine.InputSystem;

// Déplace le bonhomme avec Z (avancer), Q (gauche), S (reculer) et D (droite),
// et le fait sauter avec Espace. Il monte seul les petites marches (relief des
// montagnes) et s'enfonce à moitié dans l'eau (zones avec un ZoneEau).
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

    [Tooltip("Le modèle visible du personnage, tourné vers la direction de déplacement.")]
    public Transform modele;

    [Tooltip("Vitesse à laquelle le personnage se tourne vers sa direction.")]
    public float vitesseRotation = 12f;

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

    [Header("Marches")]
    [Tooltip("Hauteur maximale d'une marche franchie en marchant. Au-delà, il faut sauter.")]
    public float hauteurMarche = 0.65f;

    [Header("Eau")]
    [Tooltip("Part de la hauteur du personnage sous l'eau (0.5 = à moitié immergé).")]
    [Range(0f, 1f)] public float immersion = 0.5f;

    [Tooltip("Vitesse dans l'eau, en proportion de la vitesse normale.")]
    public float facteurVitesseEau = 0.6f;

    [Tooltip("Vitesse à laquelle le personnage s'enfonce ou ressort de l'eau, en unités par seconde.")]
    public float vitesseImmersion = 2f;

    Rigidbody corps;
    Collider forme;
    Vector3 direction;
    float dernierSol = -1f;
    float dernierAppuiSaut = -1f;
    bool espaceMaintenu;
    SphereCollider sphere;
    Vector3 centreSphere;
    float enfoncement;
    readonly Collider[] tampon = new Collider[8];

    // Gravité et vitesse de départ calculées à partir de la hauteur et du temps voulus.
    float Gravite => 2f * hauteurSaut / (tempsJusquauSommet * tempsJusquauSommet);
    float VitesseSaut => Gravite * tempsJusquauSommet;

    void Awake()
    {
        corps = GetComponent<Rigidbody>();
        forme = GetComponent<Collider>();
        sphere = forme as SphereCollider;
        if (sphere != null)
            centreSphere = sphere.center;
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

        // Le personnage regarde là où il va.
        if (modele != null && direction.sqrMagnitude > 0.01f)
        {
            Quaternion cible = Quaternion.LookRotation(direction, Vector3.up);
            modele.rotation = Quaternion.Slerp(modele.rotation, cible, vitesseRotation * Time.deltaTime);
        }

        if (clavier.spaceKey.wasPressedThisFrame)
            dernierAppuiSaut = Time.time;
        espaceMaintenu = clavier.spaceKey.isPressed;
    }

    void FixedUpdate()
    {
        bool auSol = EstAuSol();
        if (auSol)
            dernierSol = Time.time;

        bool dansEau = EstDansEau();
        MettreAJourImmersion(dansEau);

        if (auSol && direction.sqrMagnitude > 0.01f)
            MonterMarche();

        Vector3 v = direction * (dansEau ? vitesse * facteurVitesseEau : vitesse);
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

    // Le centre du personnage est-il au-dessus d'une zone d'eau ?
    bool EstDansEau()
    {
        if (forme == null) return false;
        int n = Physics.OverlapSphereNonAlloc(forme.bounds.center, 0.05f, tampon, ~0,
            QueryTriggerInteraction.Collide);
        for (int i = 0; i < n; i++)
            if (tampon[i].GetComponent<ZoneEau>() != null)
                return true;
        return false;
    }

    // Dans l'eau, on remonte le collider par rapport au modèle visible :
    // le collider reste posé sur le fond et la sphère affichée s'enfonce sous la surface.
    void MettreAJourImmersion(bool dansEau)
    {
        if (sphere == null) return;
        float diametre = sphere.radius * 2f * transform.lossyScale.y;
        float cible = dansEau ? diametre * immersion : 0f;
        enfoncement = Mathf.MoveTowards(enfoncement, cible, vitesseImmersion * Time.fixedDeltaTime);
        sphere.center = centreSphere + Vector3.up * (enfoncement / transform.lossyScale.y);
    }

    // Monte automatiquement une marche basse devant le personnage (comme le « step offset »
    // des contrôleurs de personnage classiques).
    void MonterMarche()
    {
        Bounds b = forme.bounds;
        float rayon = Mathf.Min(b.extents.x, b.extents.z);
        float bas = b.min.y;
        Vector3 dir = direction.normalized;

        // Un obstacle juste devant, au ras du sol ?
        Vector3 origineBas = new Vector3(b.center.x, bas + 0.05f, b.center.z);
        if (!Physics.Raycast(origineBas, dir, out RaycastHit mur, rayon + 0.15f, ~0, QueryTriggerInteraction.Ignore))
            return;
        if (mur.normal.y > 0.7f) return; // pente douce : rien à faire

        // On cherche le dessus de la marche, juste derrière son bord.
        Vector3 origineHaut = mur.point + dir * 0.05f;
        origineHaut.y = bas + hauteurMarche + 0.05f;
        if (!Physics.Raycast(origineHaut, Vector3.down, out RaycastHit dessus, hauteurMarche + 0.05f, ~0,
                QueryTriggerInteraction.Ignore))
            return;
        float hauteur = dessus.point.y - bas;
        if (hauteur <= 0.01f || hauteur > hauteurMarche || dessus.normal.y < 0.7f) return;

        // Assez de place au-dessus de la marche pour le personnage ?
        Vector3 centreApres = b.center + Vector3.up * (hauteur + 0.02f) + dir * 0.1f;
        int n = Physics.OverlapSphereNonAlloc(centreApres, rayon * 0.95f, tampon, ~0,
            QueryTriggerInteraction.Ignore);
        for (int i = 0; i < n; i++)
            if (tampon[i] != forme)
                return;

        corps.position += Vector3.up * (hauteur + 0.02f);
        Vector3 v = corps.linearVelocity;
        if (v.y < 0f) corps.linearVelocity = new Vector3(v.x, 0f, v.z);
    }

    bool EstAuSol()
    {
        if (forme == null) return false;

        // Petite sphère lancée vers le bas depuis le bas du collider.
        Bounds b = forme.bounds;
        float rayon = Mathf.Min(b.extents.x, b.extents.z);
        Vector3 centre = b.center + Vector3.down * (b.extents.y - rayon);
        return Physics.SphereCast(centre, rayon * 0.9f, Vector3.down, out _, rayon * 0.1f + 0.05f,
            ~0, QueryTriggerInteraction.Ignore);
    }
}
