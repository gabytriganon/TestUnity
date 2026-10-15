using UnityEngine;

// Posé sur un objet (ou sur un parent), il change la hauteur du saut quand le personnage
// saute depuis cet objet. Utilisé par l'escalier céleste pour des sauts plus hauts.
public class PlateformeSaut : MonoBehaviour
{
    [Tooltip("Hauteur du saut depuis ces plateformes, en unités.")]
    public float hauteurSaut = 3f;
}
