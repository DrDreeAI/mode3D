using UnityEngine;
using UnityEngine.UI;

namespace Mode3D.UI
{
    public class PanelStyler : MonoBehaviour
    {
        [Header("Layout")] 
        [SerializeField] private Vector2 panelSize = new Vector2(1100f, 600f);
        [SerializeField] private float anchoredY = 700f;

        [Header("Style")] 
        [SerializeField] private Color backgroundColor = new Color(0.18f, 0.18f, 0.2f, 0.6f); // gris, semi-transparent

        private void Awake()
        {
            var rect = GetComponent<RectTransform>();
            if (rect == null)
            {
                rect = gameObject.AddComponent<RectTransform>();
            }

            // Ancrage centre, taille fixe, position Y
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = panelSize;
            var pos = rect.anchoredPosition;
            pos.x = 0f;
            pos.y = anchoredY;
            rect.anchoredPosition = pos;

            // Fond visuel
            var img = GetComponent<Image>();
            if (img == null)
            {
                img = gameObject.AddComponent<Image>();
            }
            img.color = backgroundColor;
            img.raycastTarget = true;

            // Optionnel: layout vertical simple si pas présent
            var layout = GetComponent<VerticalLayoutGroup>();
            if (layout == null)
            {
                layout = gameObject.AddComponent<VerticalLayoutGroup>();
                layout.childAlignment = TextAnchor.UpperCenter;
                layout.spacing = 12f;
                layout.padding = new RectOffset(24, 24, 24, 24);
            }
        }
    }
}


