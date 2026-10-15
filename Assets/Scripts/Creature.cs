using UnityEngine;

// Marque un objet comme créature que l'on peut capturer avec une sphère (touche E).
public class Creature : MonoBehaviour
{
    [Tooltip("Image recadrée de la créature, affichée dans le menu une fois capturée.")]
    public Texture portrait;

    void Awake()
    {
        // Si l'image n'est pas réglée dans la scène, on la charge depuis Assets/Resources/Creatures
        // (fichier portant le même nom que l'objet, par exemple Groudon.png).
        if (portrait == null)
            portrait = Resources.Load<Texture2D>("Creatures/" + gameObject.name);
    }

    // Vrai dès qu'une sphère a commencé à la capturer (une seule sphère à la fois).
    public bool EnCapture { get; set; }
}
