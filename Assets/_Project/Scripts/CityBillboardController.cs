using UnityEngine;

namespace Mode3D.UI
{
    // Contrôle un panneau 3D (quad/plane) qui affiche la ville sélectionnée
    // - S'oriente vers la caméra (billboard)
    // - S'étire sur l'axe Z
    public class CityBillboardController : MonoBehaviour
    {
        [Header("Affichage 3D")] 
        [SerializeField] private Renderer targetRenderer; // MeshRenderer ou SpriteRenderer via .material/mainTexture
        [SerializeField] private float zScale = 50f;
        [SerializeField] private Vector2 xyScale = new Vector2(10f, 6f);

        [Header("Textures des villes")] 
        [SerializeField] private Texture londonTexture;
        [SerializeField] private Texture newYorkTexture;
        [SerializeField] private Texture parisTexture;
        [SerializeField] private Texture dubaiTexture;

        private Camera mainCamera;

        private void Awake()
        {
            if (targetRenderer == null)
            {
                targetRenderer = GetComponent<Renderer>();
            }
            mainCamera = Camera.main;
            ApplyScale();
        }

        private void LateUpdate()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
            if (mainCamera == null) return;

            // Billboard face caméra (rotation uniquement, pas de look-at du point)
            transform.rotation = Quaternion.LookRotation(mainCamera.transform.forward, Vector3.up);
        }

        public void UpdateBillboard(Destination destination)
        {
            if (targetRenderer == null) return;

            var mat = targetRenderer.material;
            switch (destination)
            {
                case Destination.Londres:
                    mat.mainTexture = londonTexture;
                    break;
                case Destination.NewYork:
                    mat.mainTexture = newYorkTexture;
                    break;
                case Destination.Paris:
                    mat.mainTexture = parisTexture;
                    break;
                case Destination.Dubai:
                    mat.mainTexture = dubaiTexture;
                    break;
            }
            // Assure un rendu lumineux constant
            mat.color = Color.white;
        }

        public void ApplyScale()
        {
            transform.localScale = new Vector3(xyScale.x, xyScale.y, zScale);
        }
    }
}


