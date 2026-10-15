using UnityEngine;

// Rendu 2.5D : l'image (un sprite posé sur un quad) se tourne toujours vers la caméra,
// en restant bien droite (rotation seulement autour de l'axe vertical).
public class PanneauFaceCamera : MonoBehaviour
{
    [Tooltip("Caméra à regarder. Par défaut : la caméra principale.")]
    public Transform cameraCible;

    void LateUpdate()
    {
        if (cameraCible == null)
        {
            if (Camera.main == null) return;
            cameraCible = Camera.main.transform;
        }

        Vector3 dir = transform.position - cameraCible.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
    }
}
