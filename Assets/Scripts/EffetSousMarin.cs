using UnityEngine;

// À mettre sur la caméra : quand elle passe sous la surface de l'eau,
// un brouillard bleu donne l'impression d'être sous l'eau.
public class EffetSousMarin : MonoBehaviour
{
    [Tooltip("Hauteur de la surface de l'eau.")]
    public float niveauEau = 0f;

    public Color couleurEau = new Color(0.05f, 0.18f, 0.35f);

    [Tooltip("Épaisseur du brouillard sous l'eau.")]
    public float densite = 0.09f;

    bool brouillardAvant;
    Color couleurAvant;
    float densiteAvant;
    FogMode modeAvant;
    bool sousLEau;

    void Start()
    {
        brouillardAvant = RenderSettings.fog;
        couleurAvant = RenderSettings.fogColor;
        densiteAvant = RenderSettings.fogDensity;
        modeAvant = RenderSettings.fogMode;
    }

    void LateUpdate()
    {
        bool dessous = transform.position.y < niveauEau;
        if (dessous == sousLEau) return;
        sousLEau = dessous;

        if (sousLEau)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = couleurEau;
            RenderSettings.fogDensity = densite;
        }
        else
        {
            RenderSettings.fog = brouillardAvant;
            RenderSettings.fogMode = modeAvant;
            RenderSettings.fogColor = couleurAvant;
            RenderSettings.fogDensity = densiteAvant;
        }
    }

    void OnDisable()
    {
        if (!sousLEau) return;
        sousLEau = false;
        RenderSettings.fog = brouillardAvant;
        RenderSettings.fogMode = modeAvant;
        RenderSettings.fogColor = couleurAvant;
        RenderSettings.fogDensity = densiteAvant;
    }
}
