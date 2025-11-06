using UnityEngine;
using UnityEngine.UI;

namespace Mode3D.UI
{
    // Stretches a UI Image to full Canvas while preserving aspect
    public class BackgroundFitter : MonoBehaviour
    {
        private void Awake()
        {
            var rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                rect.anchoredPosition = Vector2.zero;
            }

            var img = GetComponent<Image>();
            if (img != null)
            {
                img.preserveAspect = true;
                img.raycastTarget = false;
            }
        }
    }
}


