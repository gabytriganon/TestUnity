using UnityEngine;

// Marque un objet comme créature. Touchée par une sphère lancée, elle disparaît.
public class Creature : MonoBehaviour
{
    public void Capturer()
    {
        Destroy(gameObject);
    }
}
