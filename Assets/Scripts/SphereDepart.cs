using System.Collections.Generic;
using UnityEngine;

// Une des 3 sphères posées au sol au départ, avec une créature dedans.
// Près d'elle, la touche A propose de choisir cette créature (voir ChoixDepart).
public class SphereDepart : MonoBehaviour
{
    // Distance (centre à centre) à laquelle le héros peut interagir avec une sphère.
    public const float DistanceInteraction = 2f;

    [Tooltip("Image de la créature contenue dans la sphère.")]
    public Texture creature;

    // Vrai une fois une créature de départ choisie : les autres sphères deviennent inactives.
    public static bool ChoixFait;

    static readonly List<SphereDepart> toutes = new List<SphereDepart>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reinitialiser()
    {
        ChoixFait = false;
        toutes.Clear();
    }

    void OnEnable() { toutes.Add(this); }
    void OnDisable() { toutes.Remove(this); }

    // La sphère de départ la plus proche à portée, ou null (toujours null après le choix).
    public static SphereDepart LaPlusProche(Vector3 position)
    {
        if (ChoixFait) return null;
        SphereDepart meilleure = null;
        float meilleureDistance = DistanceInteraction;
        foreach (SphereDepart s in toutes)
        {
            float d = Vector3.Distance(position, s.transform.position);
            if (d <= meilleureDistance)
            {
                meilleure = s;
                meilleureDistance = d;
            }
        }
        return meilleure;
    }
}
