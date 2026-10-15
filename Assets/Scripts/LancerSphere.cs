using UnityEngine;
using UnityEngine.InputSystem;

// Appuyer sur E lance une sphère de capture devant le personnage, en cloche.
// Au lancement du jeu, 3 sphères sont posées au sol devant le personnage (décor).
public class LancerSphere : MonoBehaviour
{
    [Tooltip("Matériau de base de la sphère (une texture rouge et blanche lui est ajoutée).")]
    public Material materiau;

    [Tooltip("Diamètre de la sphère.")]
    public float taille = 0.35f;

    [Tooltip("Vitesse vers l'avant.")]
    public float vitesseAvant = 12f;

    [Tooltip("Vitesse vers le haut au départ (donne la courbe du lancer).")]
    public float vitesseHaut = 4f;

    [Tooltip("Temps minimum entre deux lancers, en secondes.")]
    public float delaiEntreLancers = 0.5f;

    float dernierLancer = -10f;
    DeplacementBonhomme deplacement;
    Material materiauCoque;
    Material materiauNoir;
    Material materiauBlanc;

    void Awake()
    {
        deplacement = GetComponent<DeplacementBonhomme>();
    }

    void Update()
    {
        var clavier = Keyboard.current;
        if (clavier == null || !clavier.eKey.wasPressedThisFrame) return;
        if (Time.time - dernierLancer < delaiEntreLancers) return;
        dernierLancer = Time.time;
        Lancer();
    }

    void Start()
    {
        PoserSpheresAuSol();
    }

    // Au lancement du jeu : 3 sphères alignées sur le sol, face au personnage.
    void PoserSpheresAuSol()
    {
        Vector3 devant = deplacement != null ? deplacement.Orientation * Vector3.forward : transform.forward;
        Vector3 droite = Vector3.Cross(Vector3.up, devant).normalized;
        Vector3 centre = transform.position + devant * 2.5f;
        for (int i = -1; i <= 1; i++)
        {
            Vector3 p = centre + droite * (i * 0.8f);
            float sol = p.y - 0.5f;
            if (Physics.Raycast(p + Vector3.up * 5f, Vector3.down, out RaycastHit impact, 20f, ~0, QueryTriggerInteraction.Ignore))
                sol = impact.point.y;
            GameObject sphere = CreerSphere(new Vector3(p.x, sol + taille / 2f, p.z), Quaternion.LookRotation(-devant));
            sphere.name = "Sphere au sol";
        }
    }

    void Lancer()
    {
        Vector3 devant = deplacement != null ? deplacement.Orientation * Vector3.forward : transform.forward;
        Vector3 depart = transform.position + Vector3.up * 0.6f + devant * 0.7f;

        GameObject sphere = CreerSphere(depart, Quaternion.LookRotation(devant));
        sphere.name = "Sphere lancee";

        // La sphère ne doit pas toucher le personnage qui la lance.
        Collider colSphere = sphere.GetComponent<Collider>();
        foreach (Collider c in GetComponentsInChildren<Collider>())
            Physics.IgnoreCollision(colSphere, c);

        Rigidbody corps = sphere.AddComponent<Rigidbody>();
        corps.mass = 0.2f;
        corps.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        corps.interpolation = RigidbodyInterpolation.Interpolate;
        corps.linearVelocity = devant * vitesseAvant + Vector3.up * vitesseHaut;

        sphere.AddComponent<SphereLancee>();
    }

    // Une sphère de capture : coque rouge et blanche, bande noire, bouton blanc devant (+Z).
    GameObject CreerSphere(Vector3 position, Quaternion rotation)
    {
        PreparerMateriaux();
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.SetPositionAndRotation(position, rotation);
        sphere.transform.localScale = Vector3.one * taille;
        sphere.GetComponent<Renderer>().sharedMaterial = materiauCoque;
        AjouterBouton(sphere.transform, 0.36f, 0.43f, materiauNoir);
        AjouterBouton(sphere.transform, 0.24f, 0.47f, materiauBlanc);
        return sphere;
    }

    static void AjouterBouton(Transform parent, float diametre, float avance, Material mat)
    {
        GameObject bouton = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        DestroyImmediate(bouton.GetComponent<Collider>());
        bouton.name = "Bouton";
        bouton.transform.SetParent(parent, false);
        bouton.transform.localPosition = new Vector3(0f, 0f, avance);
        bouton.transform.localScale = new Vector3(diametre, diametre, diametre * 0.4f);
        bouton.GetComponent<Renderer>().sharedMaterial = mat;
    }

    void PreparerMateriaux()
    {
        if (materiauCoque != null) return;
        Material baseMat = materiau != null ? materiau : new Material(Shader.Find("Universal Render Pipeline/Lit"));

        // Texture de la coque : haut rouge, bande noire au milieu, bas blanc.
        var tex = new Texture2D(4, 64, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < 64; y++)
        {
            float v = (y + 0.5f) / 64f;
            Color c = v > 0.535f ? new Color(0.85f, 0.1f, 0.1f) : v > 0.465f ? new Color(0.08f, 0.08f, 0.08f) : new Color(0.95f, 0.95f, 0.95f);
            for (int x = 0; x < 4; x++) tex.SetPixel(x, y, c);
        }
        tex.Apply();

        materiauCoque = new Material(baseMat);
        materiauCoque.SetTexture("_BaseMap", tex);
        materiauCoque.SetColor("_BaseColor", Color.white);
        materiauNoir = new Material(baseMat);
        materiauNoir.SetColor("_BaseColor", new Color(0.08f, 0.08f, 0.08f));
        materiauBlanc = new Material(baseMat);
        materiauBlanc.SetColor("_BaseColor", Color.white);
    }
}
