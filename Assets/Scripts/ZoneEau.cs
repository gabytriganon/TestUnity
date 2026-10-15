using UnityEngine;

// Marque une zone d'eau : un collider « déclencheur » (Is Trigger) posé sur l'eau.
// Quand le personnage s'y trouve, il s'enfonce à moitié et avance moins vite.
[RequireComponent(typeof(Collider))]
public class ZoneEau : MonoBehaviour
{
    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }
}
