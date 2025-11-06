using UnityEngine;
using UnityEngine.UI;

namespace Mode3D.UI
{
    public class BackgroundController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image backgroundImage;

        [Header("City Sprites")] 
        [SerializeField] private Sprite londonSprite;
        [SerializeField] private Sprite newYorkSprite;
        [SerializeField] private Sprite parisSprite;
        [SerializeField] private Sprite dubaiSprite;

        public void UpdateBackground(Destination destination)
        {
            if (backgroundImage == null)
            {
                backgroundImage = GetComponent<Image>();
                if (backgroundImage == null)
                {
                    Debug.LogWarning("BackgroundController: backgroundImage is not assigned and no Image found on the same object.");
                    return;
                }
            }

            backgroundImage.enabled = true;

            switch (destination)
            {
                case Destination.Londres:
                    backgroundImage.sprite = londonSprite;
                    break;
                case Destination.NewYork:
                    backgroundImage.sprite = newYorkSprite;
                    break;
                case Destination.Paris:
                    backgroundImage.sprite = parisSprite;
                    break;
                case Destination.Dubai:
                    backgroundImage.sprite = dubaiSprite;
                    break;
            }

            // Preserve aspect if using a non-9:16 sprite
            backgroundImage.preserveAspect = true;
        }
    }
}


