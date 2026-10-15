using UnityEngine;

// Fait scintiller doucement tous les objets enfants (plateformes de l'escalier céleste) :
// leur lueur monte et descend, avec un léger décalage d'une plateforme à l'autre.
public class Scintillement : MonoBehaviour
{
    [Tooltip("Couleur de la lueur.")]
    public Color couleur = new Color(0.6f, 1.2f, 1.6f);

    [Tooltip("Vitesse du scintillement.")]
    public float vitesse = 2f;

    [Tooltip("Décalage du scintillement entre deux plateformes.")]
    public float decalage = 0.45f;

    Renderer[] rendus;
    MaterialPropertyBlock bloc;
    static readonly int Emission = Shader.PropertyToID("_EmissionColor");

    void Start()
    {
        rendus = GetComponentsInChildren<Renderer>();
        bloc = new MaterialPropertyBlock();
    }

    void Update()
    {
        for (int i = 0; i < rendus.Length; i++)
        {
            float k = 0.55f + 0.45f * Mathf.Sin(Time.time * vitesse + i * decalage);
            rendus[i].GetPropertyBlock(bloc);
            bloc.SetColor(Emission, couleur * k);
            rendus[i].SetPropertyBlock(bloc);
        }
    }
}
