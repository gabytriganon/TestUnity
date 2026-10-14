using UnityEngine;
using UnityEngine.InputSystem;

// Déplace le bonhomme avec Z (avancer), Q (gauche), S (reculer) et D (droite).
// Les touches sont lues par leur position physique sur le clavier :
// sur un clavier AZERTY, ce sont bien Z Q S D (et W A S D sur un QWERTY).
[RequireComponent(typeof(Rigidbody))]
public class DeplacementBonhomme : MonoBehaviour
{
    [Tooltip("Vitesse de déplacement en unités par seconde.")]
    public float vitesse = 5f;

    [Tooltip("Caméra utilisée pour savoir où est « devant ». Par défaut : la caméra principale.")]
    public Transform cameraReference;

    Rigidbody corps;
    Vector3 direction;

    void Awake()
    {
        corps = GetComponent<Rigidbody>();
        corps.freezeRotation = true;
        if (cameraReference == null && Camera.main != null)
            cameraReference = Camera.main.transform;
    }

    void Update()
    {
        var clavier = Keyboard.current;
        if (clavier == null)
        {
            direction = Vector3.zero;
            return;
        }

        float avant = 0f;
        float cote = 0f;
        if (clavier.wKey.isPressed) avant += 1f; // Z sur AZERTY
        if (clavier.sKey.isPressed) avant -= 1f; // S
        if (clavier.aKey.isPressed) cote -= 1f;  // Q sur AZERTY
        if (clavier.dKey.isPressed) cote += 1f;  // D

        // « Devant » et « à droite » sont ceux de la caméra, ramenés à l'horizontale.
        Vector3 devant = Vector3.forward;
        Vector3 droite = Vector3.right;
        if (cameraReference != null)
        {
            devant = Vector3.ProjectOnPlane(cameraReference.forward, Vector3.up).normalized;
            droite = Vector3.ProjectOnPlane(cameraReference.right, Vector3.up).normalized;
        }

        direction = Vector3.ClampMagnitude(devant * avant + droite * cote, 1f);
    }

    void FixedUpdate()
    {
        Vector3 v = direction * vitesse;
        v.y = corps.linearVelocity.y; // garde la gravité
        corps.linearVelocity = v;
    }
}
