using System.Collections.Generic;
using UnityEngine;

public enum Biome { Aucun, Eau, Herbe, Route, Montagne, Partout }

// Dit dans quel biome se trouve un point de la carte (eau, herbe, route ou montagne),
// à partir des objets de la scène : zones d'eau, cases « Chemin » et « Herbe » de la Carte,
// et blocs du groupe « Montagnes ».
public static class Biomes
{
    // Hauteur de la surface de l'eau.
    public const float NiveauEau = 0f;

    static readonly List<Bounds> eau = new List<Bounds>();
    static readonly List<Bounds> eauProfonde = new List<Bounds>();
    static readonly List<Bounds> chemins = new List<Bounds>();
    static readonly List<Bounds> herbes = new List<Bounds>();
    static Bounds limites;
    static bool pret;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reinitialiser()
    {
        pret = false;
    }

    public static Bounds Limites { get { Preparer(); return limites; } }
    public static IReadOnlyList<Bounds> ZonesEauProfonde { get { Preparer(); return eauProfonde; } }

    static void Preparer()
    {
        if (pret) return;
        pret = true;
        eau.Clear(); eauProfonde.Clear(); chemins.Clear(); herbes.Clear();

        foreach (ZoneEau z in Object.FindObjectsByType<ZoneEau>(FindObjectsSortMode.None))
            eau.Add(z.GetComponent<Collider>().bounds);
        foreach (ZoneEauProfonde z in Object.FindObjectsByType<ZoneEauProfonde>(FindObjectsSortMode.None))
        {
            Bounds b = z.GetComponent<Collider>().bounds;
            eau.Add(b);
            eauProfonde.Add(b);
        }

        bool premier = true;
        GameObject carte = GameObject.Find("Carte");
        if (carte != null)
        {
            foreach (Renderer r in carte.GetComponentsInChildren<Renderer>())
            {
                if (r.name.StartsWith("Chemin")) chemins.Add(r.bounds);
                else if (r.name.StartsWith("Herbe")) herbes.Add(r.bounds);
                else continue;
                if (premier) { limites = r.bounds; premier = false; }
                else limites.Encapsulate(r.bounds);
            }
        }
        foreach (Bounds b in eau)
        {
            if (premier) { limites = b; premier = false; }
            else limites.Encapsulate(b);
        }
    }

    static bool DansXZ(List<Bounds> zones, Vector3 p)
    {
        foreach (Bounds b in zones)
            if (p.x >= b.min.x && p.x <= b.max.x && p.z >= b.min.z && p.z <= b.max.z)
                return true;
        return false;
    }

    static bool SousObjet(Transform t, string nom)
    {
        for (; t != null; t = t.parent)
            if (t.name == nom) return true;
        return false;
    }

    // Le sol (objet solide le plus haut) sous le point p, en ignorant les créatures, le héros et les sphères.
    public static bool Sol(Vector3 p, out float y, out Transform objet)
    {
        y = 0f;
        objet = null;
        RaycastHit[] impacts = Physics.RaycastAll(new Vector3(p.x, 80f, p.z), Vector3.down, 200f, ~0,
            QueryTriggerInteraction.Ignore);
        float meilleur = float.NegativeInfinity;
        foreach (RaycastHit h in impacts)
        {
            if (h.point.y <= meilleur) continue;
            if (h.collider.GetComponentInParent<Creature>() != null) continue;
            if (h.collider.GetComponentInParent<DeplacementBonhomme>() != null) continue;
            if (h.collider.GetComponent<SphereLancee>() != null || h.collider.GetComponent<SphereDepart>() != null) continue;
            meilleur = h.point.y;
            objet = h.collider.transform;
        }
        if (objet == null) return false;
        y = meilleur;
        return true;
    }

    // Biome du point p ; ySol reçoit la hauteur où poser une créature (surface de l'eau pour l'eau).
    public static Biome Classer(Vector3 p, out float ySol)
    {
        Preparer();
        bool solide = Sol(p, out ySol, out Transform objet);

        if (DansXZ(eau, p) && (!solide || ySol < 0.2f))
        {
            ySol = NiveauEau;
            return Biome.Eau;
        }
        if (!solide) return Biome.Aucun;
        if (ySol > 0.3f && SousObjet(objet, "Montagnes")) return Biome.Montagne;
        if (!SousObjet(objet, "Carte")) return Biome.Aucun;
        if (DansXZ(chemins, p)) return Biome.Route;
        if (DansXZ(herbes, p)) return Biome.Herbe;
        return Biome.Aucun;
    }

    // Vrai si le biome trouvé convient au biome voulu (Partout = n'importe quelle terre ferme).
    public static bool Convient(Biome voulu, Biome trouve)
    {
        if (voulu == Biome.Partout)
            return trouve == Biome.Herbe || trouve == Biome.Route || trouve == Biome.Montagne;
        return voulu == trouve;
    }
}
