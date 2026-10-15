using UnityEngine;

// Caméra à la troisième personne : reste toujours derrière et au-dessus de la cible,
// tourne avec elle quand elle tourne, et se rapproche si un mur ou un plafond la gêne.
public class CameraTroisiemePersonne : MonoBehaviour
{
    [Tooltip("L'objet à suivre (le bonhomme).")]
    public Transform cible;

    [Tooltip("Position de la caméra par rapport à la cible, dans le repère de la cible (z négatif = derrière).")]
    public Vector3 decalage = new Vector3(0f, 3f, -6f);

    [Tooltip("Plus la valeur est grande, plus la caméra rattrape vite la cible.")]
    public float douceur = 8f;

    [Tooltip("Distance gardée entre la caméra et un obstacle.")]
    public float marge = 0.3f;

    DeplacementBonhomme deplacement;

    void Start()
    {
        if (cible != null)
            deplacement = cible.GetComponent<DeplacementBonhomme>();
    }

    void LateUpdate()
    {
        if (cible == null) return;

        // Orientation de la cible : celle du script de déplacement, sinon sa rotation horizontale.
        Quaternion orientation = deplacement != null
            ? deplacement.Orientation
            : Quaternion.Euler(0f, cible.eulerAngles.y, 0f);

        Vector3 pointVise = cible.position + Vector3.up * 0.5f;
        Vector3 positionVoulue = cible.position + orientation * decalage;

        // Un obstacle entre la cible et la caméra ? On rapproche la caméra.
        Vector3 vers = positionVoulue - pointVise;
        if (Physics.SphereCast(pointVise, 0.2f, vers.normalized, out RaycastHit choc, vers.magnitude,
                ~0, QueryTriggerInteraction.Ignore))
            positionVoulue = pointVise + vers.normalized * Mathf.Max(choc.distance - marge, 0.5f);

        transform.position = Vector3.Lerp(transform.position, positionVoulue, douceur * Time.deltaTime);
        transform.LookAt(pointVise);
    }
}
