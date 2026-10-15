using UnityEngine;
using UnityEngine.InputSystem;

// Près d'une sphère de départ, la touche A ouvre une fenêtre
// « Choisir cette créature ? » avec son image et les réponses Oui / Non.
public class ChoixDepart : MonoBehaviour
{
    SphereDepart proposee;
    GUIStyle styleTitre;

    void Update()
    {
        var clavier = Keyboard.current;
        if (clavier == null || proposee != null) return;

        // Touche A (position du Q sur un clavier QWERTY), comme pour plonger.
        if (clavier.qKey.wasPressedThisFrame)
            proposee = SphereDepart.LaPlusProche(transform.position);
    }

    void OnGUI()
    {
        if (proposee == null) return;

        float echelle = Screen.height / 1080f;
        float largeur = 420f * echelle;
        float hauteur = 360f * echelle;
        var fenetre = new Rect((Screen.width - largeur) / 2f, (Screen.height - hauteur) / 2f, largeur, hauteur);

        if (styleTitre == null)
            styleTitre = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
        styleTitre.fontSize = Mathf.RoundToInt(28 * echelle);
        var styleBouton = new GUIStyle(GUI.skin.button) { fontSize = Mathf.RoundToInt(24 * echelle) };

        GUI.Box(fenetre, GUIContent.none);
        GUI.Box(fenetre, GUIContent.none); // deux fois pour un fond plus opaque

        float m = 16f * echelle;
        GUI.Label(new Rect(fenetre.x, fenetre.y + m, largeur, 40f * echelle), "Choisir cette créature ?", styleTitre);

        float tailleImage = 180f * echelle;
        var zoneImage = new Rect(fenetre.x + (largeur - tailleImage) / 2f, fenetre.y + 70f * echelle, tailleImage, tailleImage);
        if (proposee.creature != null)
            GUI.DrawTexture(zoneImage, proposee.creature, ScaleMode.ScaleToFit);

        float lb = 140f * echelle, hb = 50f * echelle;
        float yb = fenetre.yMax - hb - m;
        if (GUI.Button(new Rect(fenetre.center.x - lb - m / 2f, yb, lb, hb), "Oui", styleBouton))
            Choisir();
        else if (GUI.Button(new Rect(fenetre.center.x + m / 2f, yb, lb, hb), "Non", styleBouton))
            proposee = null;
    }

    // La créature va dans le premier emplacement libre du menu ; les autres sphères deviennent inactives.
    void Choisir()
    {
        MenuCreatures menu = FindAnyObjectByType<MenuCreatures>();
        if (menu != null) menu.Ajouter(proposee.creature);
        SphereDepart.ChoixFait = true;
        Destroy(proposee.gameObject);
        proposee = null;
    }
}
