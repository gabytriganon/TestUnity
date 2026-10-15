using UnityEngine;

// Marque un objet comme créature que l'on peut capturer avec une sphère (touche E).
public class Creature : MonoBehaviour
{
    [Tooltip("Image recadrée de la créature, affichée dans le menu une fois capturée.")]
    public Texture portrait;

    // Vrai dès qu'une sphère a commencé à la capturer (une seule sphère à la fois).
    public bool EnCapture { get; set; }
}
