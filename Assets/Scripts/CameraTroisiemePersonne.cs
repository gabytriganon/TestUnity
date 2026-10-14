using UnityEngine;

// Caméra à la troisième personne : reste derrière et au-dessus de la cible et la regarde.
public class CameraTroisiemePersonne : MonoBehaviour
{
    [Tooltip("L'objet à suivre (le bonhomme).")]
    public Transform cible;

    [Tooltip("Position de la caméra par rapport à la cible.")]
    public Vector3 decalage = new Vector3(0f, 3f, -6f);

    [Tooltip("Plus la valeur est grande, plus la caméra rattrape vite la cible.")]
    public float douceur = 8f;

    void LateUpdate()
    {
        if (cible == null) return;

        Vector3 positionVoulue = cible.position + decalage;
        transform.position = Vector3.Lerp(transform.position, positionVoulue, douceur * Time.deltaTime);
        transform.LookAt(cible.position + Vector3.up * 0.5f);
    }
}
