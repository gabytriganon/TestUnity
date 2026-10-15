using UnityEngine;

// Menu affiché en bas à droite de l'écran : 6 emplacements ronds pour les créatures
// que le héros a sur lui. C'est seulement un affichage (pas un élément du jeu).
public class MenuCreatures : MonoBehaviour
{
    [Tooltip("Image de la créature dans chaque emplacement (vide = emplacement libre).")]
    public Texture[] creatures = new Texture[6];

    [Tooltip("Diamètre d'un emplacement, en pixels pour un écran de 1080 de haut.")]
    public float taille = 72f;

    [Tooltip("Espace entre deux emplacements et avec le bord de l'écran.")]
    public float marge = 12f;

    public Color couleurFond = new Color(0f, 0f, 0f, 0.45f);
    public Color couleurBord = new Color(1f, 1f, 1f, 0.9f);

    Texture2D disque;
    Texture2D anneau;

    void Awake()
    {
        disque = CreerCercle(128, 0f);
        anneau = CreerCercle(128, 0.86f);
    }

    // Texture ronde : un disque plein, ou un anneau si rayonInterieur > 0 (en part du rayon).
    static Texture2D CreerCercle(int cote, float rayonInterieur)
    {
        var t = new Texture2D(cote, cote, TextureFormat.RGBA32, false);
        t.wrapMode = TextureWrapMode.Clamp;
        float r = cote / 2f;
        for (int y = 0; y < cote; y++)
        for (int x = 0; x < cote; x++)
        {
            float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r)) / r;
            float exterieur = Mathf.Clamp01((1f - d) * r);              // bord adouci
            float interieur = rayonInterieur > 0f ? Mathf.Clamp01((d - rayonInterieur) * r) : 1f;
            t.SetPixel(x, y, new Color(1f, 1f, 1f, exterieur * interieur));
        }
        t.Apply();
        return t;
    }

    // Vrai s'il reste au moins un emplacement vide.
    public bool APlaceLibre => IndexLibre() >= 0;

    int IndexLibre()
    {
        if (creatures == null || creatures.Length < 6)
            System.Array.Resize(ref creatures, 6);
        for (int i = 0; i < 6; i++)
            if (creatures[i] == null) return i;
        return -1;
    }

    // Met l'image d'une créature capturée dans le premier emplacement libre.
    public bool Ajouter(Texture image)
    {
        int i = IndexLibre();
        if (i < 0 || image == null) return false;
        creatures[i] = image;
        return true;
    }

    void OnGUI()
    {
        float echelle = Screen.height / 1080f;
        float d = taille * echelle;
        float m = marge * echelle;

        // 2 rangées de 3, collées au coin bas droit.
        for (int i = 0; i < 6; i++)
        {
            int colonne = i % 3;
            int rangee = i / 3;
            float x = Screen.width - m - d - (2 - colonne) * (d + m);
            float y = Screen.height - m - d - (1 - rangee) * (d + m);
            var zone = new Rect(x, y, d, d);

            GUI.color = couleurFond;
            GUI.DrawTexture(zone, disque);
            if (creatures != null && i < creatures.Length && creatures[i] != null)
            {
                GUI.color = Color.white;
                var interieur = new Rect(zone.x + d * 0.12f, zone.y + d * 0.12f, d * 0.76f, d * 0.76f);
                GUI.DrawTexture(interieur, creatures[i], ScaleMode.ScaleToFit);
            }
            GUI.color = couleurBord;
            GUI.DrawTexture(zone, anneau);
        }
        GUI.color = Color.white;
    }
}
