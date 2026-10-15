using UnityEngine;

// Marque une zone d'eau profonde : un collider « déclencheur » (Is Trigger) qui va de la
// surface jusqu'au fond. Le personnage y flotte et peut plonger avec la touche A.
[RequireComponent(typeof(Collider))]
public class ZoneEauProfonde : MonoBehaviour
{
    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }
}
