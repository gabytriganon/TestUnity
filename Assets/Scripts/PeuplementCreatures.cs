using System.Collections.Generic;
using UnityEngine;

// Au lancement du jeu, place les créatures sauvages dans leurs biomes.
// Les images sont lues dans Assets/Resources/Creatures/<Nom>.png : pour ajouter une
// créature qui manque (par exemple Apireine), il suffit d'y déposer son image.
public class PeuplementCreatures : MonoBehaviour
{
    [Tooltip("Matériau de base des images (découpe selon la transparence).")]
    public Material materiauSprite;

    [Tooltip("Matériau de l'ombre posée au sol.")]
    public Material materiauOmbre;

    [Header("Eau (à moitié immergés)")]
    public int nombreWailmer = 8;
    public int nombreWailord = 3;

    [Header("Herbe")]
    public int nombreAmbidextre = 6;
    public int nombreHordesApitrini = 5;
    public int nombreApireine = 2;

    [Header("Route")]
    public int nombrePichu = 5;
    public int nombrePikachu = 4;
    public int nombreRoucool = 6;

    [Header("Montagne")]
    public int nombreRacaillou = 9;
    public int nombreGravalanche = 5;
    public int nombreGrolem = 3;
    [Range(0, 2)] public int nombreOnix = 2;

    readonly Dictionary<string, Material> materiaux = new Dictionary<string, Material>();
    readonly HashSet<string> manquantes = new HashSet<string>();
    Transform groupe;

    void Start()
    {
        groupe = new GameObject("Creatures sauvages").transform;

        // Eau : les Wailord, très grands, vont en eau profonde.
        for (int i = 0; i < nombreWailord; i++)
            if (PointEauProfonde(out Vector3 p)) Creer("Wailord", p, Biome.Eau, 5f, 0f, 4f, 0.8f);
        for (int i = 0; i < nombreWailmer; i++)
            if (PointAuHasard(Biome.Eau, out Vector3 p)) Creer("Wailmer", p, Biome.Eau, 1.8f, 0f, 3f, 1f);

        // Herbe : Ambidextre près des arbres, Apitrini en hordes de 2 à 5 près des buissons.
        List<Transform> arbres = Enfants("Arbre");
        List<Transform> buissons = Enfants("Buisson");
        for (int i = 0; i < nombreAmbidextre; i++)
            if (PointPres(arbres, 1.2f, 2.5f, out Vector3 p)) Creer("Ambidextre", p, Biome.Herbe, 1.3f, 0f, 2f, 1.5f);
        for (int h = 0; h < nombreHordesApitrini; h++)
        {
            if (!PointPres(buissons, 1f, 1.8f, out Vector3 centre)) continue;
            int taille = Random.Range(2, 6);
            for (int i = 0; i < taille; i++)
                if (PointAutour(centre, 1.2f, Biome.Herbe, out Vector3 p)) Creer("Apitrini", p, Biome.Herbe, 0.6f, 0.9f, 1.5f, 1.2f);
            if (h < nombreApireine && PointAutour(centre, 1.2f, Biome.Herbe, out Vector3 reine))
                Creer("Apireine", reine, Biome.Herbe, 1.3f, 0.5f, 1.5f, 0.8f);
        }

        // Route.
        for (int i = 0; i < nombrePichu; i++)
            if (PointAuHasard(Biome.Route, out Vector3 p)) Creer("Pichu", p, Biome.Route, 0.6f, 0f, 2.5f, 1.4f);
        for (int i = 0; i < nombrePikachu; i++)
            if (PointAuHasard(Biome.Route, out Vector3 p)) Creer("Pikachu", p, Biome.Route, 0.8f, 0f, 2.5f, 1.6f);
        for (int i = 0; i < nombreRoucool; i++)
            if (PointAuHasard(Biome.Route, out Vector3 p)) Creer("Roucool", p, Biome.Route, 0.8f, 0f, 2.5f, 1.2f);

        // Montagne.
        for (int i = 0; i < nombreRacaillou; i++)
            if (PointAuHasard(Biome.Montagne, out Vector3 p)) Creer("Racaillou", p, Biome.Montagne, 0.7f, 0.3f, 2f, 0.8f);
        for (int i = 0; i < nombreGravalanche; i++)
            if (PointAuHasard(Biome.Montagne, out Vector3 p)) Creer("Gravalanche", p, Biome.Montagne, 1.3f, 0f, 2f, 0.6f);
        for (int i = 0; i < nombreGrolem; i++)
            if (PointAuHasard(Biome.Montagne, out Vector3 p)) Creer("Grolem", p, Biome.Montagne, 1.8f, 0f, 2f, 0.5f);
        for (int i = 0; i < Mathf.Min(nombreOnix, 2); i++)
            if (PointAuHasard(Biome.Montagne, out Vector3 p)) Creer("Onix", p, Biome.Montagne, 5f, 0f, 3f, 1f);

        // Mew : un seul, n'importe où sur la terre ferme, en lévitation.
        if (PointAuHasard(Biome.Partout, out Vector3 m)) Creer("Mew", m, Biome.Partout, 0.8f, 1.2f, 5f, 2f);

        foreach (string nom in manquantes)
            Debug.LogWarning($"Image manquante pour {nom} : ajoute Assets/Resources/Creatures/{nom}.png pour la faire apparaître.");
    }

    // --- Choix des emplacements ---

    bool PointAuHasard(Biome biome, out Vector3 point)
    {
        Bounds b = Biomes.Limites;
        for (int essai = 0; essai < 600; essai++)
        {
            var p = new Vector3(Random.Range(b.min.x, b.max.x), 0f, Random.Range(b.min.z, b.max.z));
            if (Biomes.Convient(biome, Biomes.Classer(p, out float y)))
            {
                point = new Vector3(p.x, y, p.z);
                return true;
            }
        }
        point = Vector3.zero;
        return false;
    }

    bool PointEauProfonde(out Vector3 point)
    {
        var zones = Biomes.ZonesEauProfonde;
        for (int essai = 0; essai < 200 && zones.Count > 0; essai++)
        {
            Bounds b = zones[Random.Range(0, zones.Count)];
            var p = new Vector3(Random.Range(b.min.x + 2f, b.max.x - 2f), 0f, Random.Range(b.min.z + 2f, b.max.z - 2f));
            if (Biomes.Classer(p, out float y) == Biome.Eau)
            {
                point = new Vector3(p.x, y, p.z);
                return true;
            }
        }
        return PointAuHasard(Biome.Eau, out point);
    }

    bool PointPres(List<Transform> reperes, float distMin, float distMax, out Vector3 point)
    {
        for (int essai = 0; essai < 60 && reperes.Count > 0; essai++)
        {
            Vector3 r = reperes[Random.Range(0, reperes.Count)].position;
            Vector2 d = Random.insideUnitCircle.normalized * Random.Range(distMin, distMax);
            var p = new Vector3(r.x + d.x, 0f, r.z + d.y);
            if (Biomes.Classer(p, out float y) == Biome.Herbe)
            {
                point = new Vector3(p.x, y, p.z);
                return true;
            }
        }
        point = Vector3.zero;
        return false;
    }

    bool PointAutour(Vector3 centre, float rayon, Biome biome, out Vector3 point)
    {
        for (int essai = 0; essai < 20; essai++)
        {
            Vector2 d = Random.insideUnitCircle * rayon;
            var p = new Vector3(centre.x + d.x, 0f, centre.z + d.y);
            if (Biomes.Convient(biome, Biomes.Classer(p, out float y)))
            {
                point = new Vector3(p.x, y, p.z);
                return true;
            }
        }
        point = Vector3.zero;
        return false;
    }

    static List<Transform> Enfants(string prefixe)
    {
        var liste = new List<Transform>();
        GameObject vegetation = GameObject.Find("Vegetation");
        if (vegetation == null) return liste;
        foreach (Transform t in vegetation.GetComponentsInChildren<Transform>())
            if (t.name.StartsWith(prefixe + " ")) liste.Add(t);
        return liste;
    }

    // --- Création d'une créature ---

    // hauteur : taille de l'image ; vol : hauteur au-dessus du sol (0 = posée ; ignoré dans l'eau).
    void Creer(string nom, Vector3 position, Biome biome, float hauteur, float vol, float rayon, float vitesse)
    {
        Texture2D image = Resources.Load<Texture2D>("Creatures/" + nom);
        if (image == null)
        {
            manquantes.Add(nom);
            return;
        }

        float largeur = hauteur * image.width / image.height;
        bool dansEau = biome == Biome.Eau;
        // Dans l'eau, l'image est centrée sur la surface : la moitié est sous l'eau.
        float yImage = dansEau ? 0f : hauteur / 2f + vol;

        var racine = new GameObject(nom);
        racine.transform.SetParent(groupe, false);
        racine.transform.position = position;

        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        DestroyImmediate(quad.GetComponent<Collider>());
        quad.name = "Sprite";
        quad.transform.SetParent(racine.transform, false);
        quad.transform.localPosition = new Vector3(0f, yImage, 0f);
        quad.transform.localScale = new Vector3(largeur, hauteur, 1f);
        var rendu = quad.GetComponent<Renderer>();
        rendu.sharedMaterial = Materiau(nom, image);
        rendu.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        quad.AddComponent<PanneauFaceCamera>();

        if (!dansEau && materiauOmbre != null)
        {
            GameObject ombre = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            DestroyImmediate(ombre.GetComponent<Collider>());
            ombre.name = "Ombre";
            ombre.transform.SetParent(racine.transform, false);
            ombre.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            ombre.transform.localScale = new Vector3(largeur * 0.6f, 0.005f, largeur * 0.6f);
            var r = ombre.GetComponent<Renderer>();
            r.sharedMaterial = materiauOmbre;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        // Boîte de collision autour de l'image.
        var boite = racine.AddComponent<BoxCollider>();
        boite.center = new Vector3(0f, yImage, 0f);
        boite.size = new Vector3(largeur * 0.8f, hauteur * 0.9f, Mathf.Max(0.4f, largeur * 0.5f));

        var corps = racine.AddComponent<Rigidbody>();
        corps.isKinematic = true;
        corps.interpolation = RigidbodyInterpolation.Interpolate;

        racine.AddComponent<Creature>().portrait = image;

        var errance = racine.AddComponent<Errance>();
        errance.biome = biome;
        errance.rayon = rayon;
        errance.vitesse = vitesse;
        errance.image = quad.transform;
    }

    Material Materiau(string nom, Texture2D image)
    {
        if (materiaux.TryGetValue(nom, out Material m)) return m;
        m = materiauSprite != null ? new Material(materiauSprite) : new Material(Shader.Find("Universal Render Pipeline/Lit"));
        m.SetTexture("_BaseMap", image);
        m.SetTexture("_MainTex", image);
        materiaux[nom] = m;
        return m;
    }
}
