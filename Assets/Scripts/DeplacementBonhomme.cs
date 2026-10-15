using UnityEngine;
using UnityEngine.InputSystem;

// Déplace le bonhomme : Z avance, S recule, Q et D le font tourner à gauche et à droite
// (la caméra tourne avec lui pour rester derrière). Espace le fait sauter : tout obstacle,
// même bas, doit être franchi en sautant. Il s'enfonce à moitié dans l'eau (zones avec un ZoneEau).
// En eau profonde (zones avec un ZoneEauProfonde), il flotte ; A plonge et fait remonter.
// Les touches sont lues par leur position physique sur le clavier :
// sur un clavier AZERTY, ce sont bien Z Q S D (et W A S D sur un QWERTY).
[RequireComponent(typeof(Rigidbody))]
public class DeplacementBonhomme : MonoBehaviour
{
    [Header("Déplacement")]
    [Tooltip("Vitesse de déplacement en unités par seconde.")]
    public float vitesse = 5f;

    [Tooltip("Vitesse à laquelle Q et D font tourner le personnage, en degrés par seconde.")]
    public float vitesseVirage = 150f;

    [Tooltip("Le modèle visible du personnage, tourné vers là où il regarde (facultatif).")]
    public Transform modele;

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

    [Header("Eau")]
    [Tooltip("Part de la hauteur du personnage sous l'eau (0.5 = à moitié immergé).")]
    [Range(0f, 1f)] public float immersion = 0.5f;

    [Tooltip("Vitesse dans l'eau, en proportion de la vitesse normale.")]
    public float facteurVitesseEau = 0.6f;

    [Tooltip("Vitesse à laquelle le personnage s'enfonce ou ressort de l'eau, en unités par seconde.")]
    public float vitesseImmersion = 2f;

    [Header("Eau profonde")]
    [Tooltip("Hauteur de la surface de l'eau.")]
    public float niveauEau = 0f;

    [Tooltip("Vitesse de descente et de remontée sous l'eau, en unités par seconde.")]
    public float vitesseNage = 3f;

    Rigidbody corps;
    Collider forme;
    Vector3 direction;
    float cap;
    float dernierSol = -1f;
    float dernierAppuiSaut = -1f;
    bool espaceMaintenu;
    bool plongeeDemandee;
    bool plonge;
    float hauteurSautSol; // hauteur du saut depuis le sol touché (plus haute sur une PlateformeSaut)
    SphereCollider sphere;
    Vector3 centreSphere;
    float enfoncement;
    readonly Collider[] tampon = new Collider[8];

    // Direction dans laquelle regarde le personnage (la caméra se place derrière).
    public Quaternion Orientation => Quaternion.Euler(0f, cap, 0f);

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
        cap = transform.eulerAngles.y;
        hauteurSautSol = hauteurSaut;
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
        float virage = 0f;
        if (clavier.wKey.isPressed) avant += 1f;  // Z sur AZERTY
        if (clavier.sKey.isPressed) avant -= 1f;  // S
        if (clavier.aKey.isPressed) virage -= 1f; // Q sur AZERTY
        if (clavier.dKey.isPressed) virage += 1f; // D

        // Q et D font tourner le personnage ; Z et S le font avancer ou reculer droit devant lui.
        cap += virage * vitesseVirage * Time.deltaTime;
        direction = Orientation * Vector3.forward * avant;

        if (modele != null)
            modele.rotation = Orientation;

        if (clavier.spaceKey.wasPressedThisFrame)
            dernierAppuiSaut = Time.time;
        espaceMaintenu = clavier.spaceKey.isPressed;

        // Touche A (position du Q sur un clavier QWERTY) : plonger ou remonter.
        if (clavier.qKey.wasPressedThisFrame)
            plongeeDemandee = true;
    }

    void FixedUpdate()
    {
        bool auSol = EstAuSol();
        if (auSol)
            dernierSol = Time.time;

        if (EstDans<ZoneEauProfonde>())
        {
            Nager();
            return;
        }
        plonge = false;
        plongeeDemandee = false;

        bool dansEau = EstDans<ZoneEau>();
        MettreAJourImmersion(dansEau);

        Vector3 v = direction * (dansEau ? vitesse * facteurVitesseEau : vitesse);
        v.y = corps.linearVelocity.y;

        // Saut : appui récent sur Espace et sol touché il y a très peu de temps.
        bool sautDemande = Time.time - dernierAppuiSaut <= tempsMemoireSaut;
        bool peutSauter = Time.time - dernierSol <= tempsGrace;
        if (sautDemande && peutSauter && v.y <= 0.01f)
        {
            v.y = Mathf.Sqrt(2f * Gravite * hauteurSautSol);
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

    // En eau profonde : on flotte à moitié immergé, ou on nage sous l'eau après avoir plongé.
    void Nager()
    {
        if (plongeeDemandee)
        {
            plonge = !plonge;
            plongeeDemandee = false;
        }

        Vector3 v = direction * vitesse * facteurVitesseEau;
        float y = corps.position.y;

        if (plonge)
        {
            // Sous l'eau : on descend doucement vers le fond, Espace fait remonter.
            MettreAJourImmersion(false);
            float cible = espaceMaintenu ? vitesseNage : -vitesseNage;
            v.y = Mathf.MoveTowards(corps.linearVelocity.y, cible, 8f * Time.fixedDeltaTime);
            if (y > niveauEau - 0.5f && v.y > 0f) v.y = 0f; // on ne ressort pas sans appuyer sur A
        }
        else
        {
            // En surface : la moitié de la sphère sous l'eau, comme dans l'eau peu profonde.
            MettreAJourImmersion(true);
            bool sautDemande = Time.time - dernierAppuiSaut <= tempsMemoireSaut;
            if (sautDemande && Mathf.Abs(y - niveauEau) < 0.2f)
            {
                v.y = VitesseSaut; // pour sortir de l'eau ou monter sur un rocher
                dernierAppuiSaut = -1f;
            }
            else if (y > niveauEau + 0.05f)
                v.y = corps.linearVelocity.y - Gravite * multiplicateurChute * Time.fixedDeltaTime;
            else
                v.y = Mathf.Clamp((niveauEau - y) * 5f, -vitesseNage * 2f, vitesseNage * 2f);
        }

        corps.linearVelocity = v;
    }

    // Le centre du personnage est-il dans une zone portant le composant T ?
    bool EstDans<T>() where T : Component
    {
        if (forme == null) return false;
        int n = Physics.OverlapSphereNonAlloc(forme.bounds.center, 0.05f, tampon, ~0,
            QueryTriggerInteraction.Collide);
        for (int i = 0; i < n; i++)
            if (tampon[i].GetComponent<T>() != null)
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

    bool EstAuSol()
    {
        if (forme == null) return false;

        // Petite sphère lancée vers le bas depuis le bas du collider.
        Bounds b = forme.bounds;
        float rayon = Mathf.Min(b.extents.x, b.extents.z);
        Vector3 centre = b.center + Vector3.down * (b.extents.y - rayon);
        if (!Physics.SphereCast(centre, rayon * 0.9f, Vector3.down, out RaycastHit sol, rayon * 0.1f + 0.05f,
                ~0, QueryTriggerInteraction.Ignore))
            return false;

        PlateformeSaut plateforme = sol.collider.GetComponentInParent<PlateformeSaut>();
        hauteurSautSol = plateforme != null ? plateforme.hauteurSaut : hauteurSaut;
        return true;
    }
}
