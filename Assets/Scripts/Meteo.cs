using System.Collections.Generic;
using UnityEngine;

// Météo qui change de temps en temps : calme, vent (arbres et buissons qui se balancent,
// feuilles qui volent), pluie, ou canicule (lumière chaude et écrasante, poussière dans l'air).
public class Meteo : MonoBehaviour
{
    public enum Temps { Calme, Vent, Pluie, Canicule }

    [Tooltip("Matériau de base des particules (feuilles, gouttes, poussière).")]
    public Material materiauParticules;

    [Tooltip("Durée d'un épisode de vent, de pluie ou de canicule (min, max), en secondes.")]
    public Vector2 dureeEpisode = new Vector2(25f, 45f);

    [Tooltip("Durée d'un moment calme entre deux épisodes (min, max), en secondes.")]
    public Vector2 dureeCalme = new Vector2(30f, 60f);

    [Tooltip("Temps pour passer d'une météo à l'autre, en secondes.")]
    public float transition = 4f;

    public Temps TempsActuel { get; private set; } = Temps.Calme;

    // Intensité de chaque effet, de 0 à 1 (monte et descend doucement).
    float vent, pluie, canicule;
    float finEpisode;
    Vector3 directionVent = Vector3.right;

    Light soleil;
    float intensiteSoleil;
    Color couleurSoleil;

    ParticleSystem feuilles, gouttes, poussiere;

    class Balancier { public Transform t; public Quaternion repos; public float phase; public float souplesse; }
    readonly List<Balancier> vegetaux = new List<Balancier>();

    void Start()
    {
        foreach (Light l in FindObjectsByType<Light>(FindObjectsSortMode.None))
            if (l.type == LightType.Directional) { soleil = l; break; }
        if (soleil != null)
        {
            intensiteSoleil = soleil.intensity;
            couleurSoleil = soleil.color;
        }

        GameObject vegetation = GameObject.Find("Vegetation");
        if (vegetation != null)
        {
            foreach (Transform t in vegetation.GetComponentsInChildren<Transform>())
            {
                bool arbre = t.name.StartsWith("Arbre ");
                bool buisson = t.name.StartsWith("Buisson ");
                if (!arbre && !buisson) continue;
                vegetaux.Add(new Balancier
                {
                    t = t,
                    repos = t.localRotation,
                    phase = Random.Range(0f, 10f),
                    souplesse = arbre ? 1f : 1.6f,
                });
            }
        }

        feuilles = CreerParticules("Feuilles", new Color(0.35f, 0.65f, 0.25f), 0.12f, 0.22f, false);
        gouttes = CreerParticules("Pluie", new Color(0.75f, 0.85f, 1f, 0.8f), 0.03f, 0.05f, true);
        poussiere = CreerParticules("Poussiere", new Color(1f, 0.85f, 0.55f, 0.7f), 0.04f, 0.08f, false);

        finEpisode = Time.time + Random.Range(dureeCalme.x, dureeCalme.y);
    }

    void Update()
    {
        if (Time.time >= finEpisode) ChangerDeTemps();

        float pas = Time.deltaTime / Mathf.Max(0.01f, transition);
        vent = Mathf.MoveTowards(vent, TempsActuel == Temps.Vent ? 1f : TempsActuel == Temps.Pluie ? 0.5f : 0f, pas);
        pluie = Mathf.MoveTowards(pluie, TempsActuel == Temps.Pluie ? 1f : 0f, pas);
        canicule = Mathf.MoveTowards(canicule, TempsActuel == Temps.Canicule ? 1f : 0f, pas);

        Balancer();
        MettreAJourParticules();
        MettreAJourLumiere();
    }

    void ChangerDeTemps()
    {
        if (TempsActuel != Temps.Calme)
        {
            TempsActuel = Temps.Calme;
            finEpisode = Time.time + Random.Range(dureeCalme.x, dureeCalme.y);
            return;
        }
        TempsActuel = (Temps)Random.Range(1, 4);
        finEpisode = Time.time + Random.Range(dureeEpisode.x, dureeEpisode.y);
        float angle = Random.Range(0f, 360f);
        directionVent = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
    }

    // Les arbres et buissons penchent dans le sens du vent et oscillent ; une légère brise même par temps calme.
    void Balancer()
    {
        float force = 0.15f + vent;
        Vector3 axe = Vector3.Cross(Vector3.up, directionVent);
        float t = Time.time;
        foreach (Balancier b in vegetaux)
        {
            if (b.t == null) continue;
            float angle = force * b.souplesse * (2.5f + 3f * Mathf.Sin(t * (1.3f + 0.6f * vent) + b.phase));
            b.t.localRotation = Quaternion.AngleAxis(angle, axe) * b.repos;
        }
    }

    void MettreAJourParticules()
    {
        Transform suivi = Camera.main != null ? Camera.main.transform : transform;
        Vector3 centre = suivi.position;

        Placer(feuilles, centre + Vector3.up * 2f, vent * 25f, directionVent * (3f + 4f * vent) + Vector3.down * 0.5f);
        Placer(gouttes, centre + Vector3.up * 12f, pluie * 700f, Vector3.down * 18f + directionVent * 2f);
        Placer(poussiere, centre + Vector3.up * 1f, canicule * 30f, Vector3.up * 0.3f + directionVent * 0.3f);
    }

    static void Placer(ParticleSystem ps, Vector3 position, float debit, Vector3 vitesse)
    {
        if (ps == null) return;
        ps.transform.position = position;
        var emission = ps.emission;
        emission.rateOverTime = debit;
        var vel = ps.velocityOverLifetime;
        vel.x = new ParticleSystem.MinMaxCurve(vitesse.x * 0.8f, vitesse.x * 1.2f);
        vel.y = new ParticleSystem.MinMaxCurve(vitesse.y * 0.8f, vitesse.y * 1.2f + 0.3f);
        vel.z = new ParticleSystem.MinMaxCurve(vitesse.z * 0.8f, vitesse.z * 1.2f);
    }

    // Pluie : lumière plus sombre et grise. Canicule : lumière plus forte et orangée.
    void MettreAJourLumiere()
    {
        if (soleil == null) return;
        Color pluvieux = Color.Lerp(couleurSoleil, new Color(0.6f, 0.65f, 0.75f), 0.7f);
        Color chaud = couleurSoleil * new Color(1f, 0.82f, 0.6f);
        Color c = couleurSoleil;
        c = Color.Lerp(c, pluvieux, pluie);
        c = Color.Lerp(c, chaud, canicule);
        soleil.color = c;
        soleil.intensity = intensiteSoleil * (1f - 0.45f * pluie + 0.4f * canicule);
    }

    ParticleSystem CreerParticules(string nom, Color couleur, float tailleMin, float tailleMax, bool etirees)
    {
        var go = new GameObject(nom);
        go.transform.SetParent(transform, false);
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.loop = true;
        main.duration = 5f;
        main.startLifetime = etirees ? 1.2f : 6f;
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(tailleMin, tailleMax);
        main.startColor = couleur;
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = etirees ? 3000 : 400;

        var forme = ps.shape;
        forme.shapeType = ParticleSystemShapeType.Box;
        forme.scale = etirees ? new Vector3(40f, 1f, 40f) : new Vector3(30f, 6f, 30f);

        var emission = ps.emission;
        emission.rateOverTime = 0f;

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.World;

        if (!etirees)
        {
            var rot = ps.rotationOverLifetime;
            rot.enabled = true;
            rot.z = new ParticleSystem.MinMaxCurve(-3f, 3f);
            var bruit = ps.noise;
            bruit.enabled = true;
            bruit.strength = 0.6f;
            bruit.frequency = 0.4f;
        }

        var rendu = go.GetComponent<ParticleSystemRenderer>();
        rendu.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        if (etirees)
        {
            rendu.renderMode = ParticleSystemRenderMode.Stretch;
            rendu.velocityScale = 0.05f;
            rendu.lengthScale = 2f;
        }
        Material m = materiauParticules != null ? new Material(materiauParticules) : new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        m.SetColor("_BaseColor", couleur);
        rendu.sharedMaterial = m;

        ps.Play();
        return ps;
    }
}
