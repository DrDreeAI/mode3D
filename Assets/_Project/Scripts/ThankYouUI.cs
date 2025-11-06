using System;
using UnityEngine;
using UnityEngine.UI;

namespace Mode3D.Destinations
{
	/// <summary>
	/// Écran final "Merci pour votre commande"
	/// </summary>
	public class ThankYouUI : MonoBehaviour
	{
		[Header("UI Style")]
		public Color panelColor = new Color(0f, 0f, 0f, 0.8f);
		public Color accentColor = new Color(0.15f, 0.6f, 0.9f, 1f);

		private Canvas canvas;
		private Action onReturnHome;
		private CircularOutfitDisplay circularDisplay; // Garder les tenues affichées

		public void Show(Action onReturnHomeCallback, CircularOutfitDisplay existingDisplay = null)
		{
			onReturnHome = onReturnHomeCallback;
			
			// Garder l'affichage circulaire existant
			circularDisplay = existingDisplay;
			
			CreateUI();
		}

		private void CreateUI()
		{
			// Canvas
			GameObject canvasGO = new GameObject("ThankYouCanvas");
			canvas = canvasGO.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			
			CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
			scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			scaler.referenceResolution = new Vector2(1920, 1080);
			
			canvasGO.AddComponent<GraphicRaycaster>();
			canvas.sortingOrder = 1000;

			// Panel moderne arrondi GRAND
			GameObject panel = UIHelper.CreateRoundedPanel(
				canvasGO,
				new Vector2(700, 500), // Grand et aéré
				Vector2.zero,
				new Color(0.03f, 0.03f, 0.03f, 0.95f),
				30f // Très grandes marges
			);

			// Titre avec icône succès GRAND
			float yPos = 180f;
			UIHelper.CreateText(panel, "✅", 
				new Vector2(120, 100), new Vector2(0, yPos), 
				80, FontStyle.Normal, new Color(0.3f, 1f, 0.3f, 1f));
			yPos -= 120f;

			// Message principal GRAND
			UIHelper.CreateText(panel, "Merci pour votre commande !", 
				new Vector2(650, 60), new Vector2(0, yPos), 
				28, FontStyle.Bold, Color.white);
			yPos -= 80f;

			// Message secondaire
			UIHelper.CreateText(panel, "Votre valise est prête pour le voyage.", 
				new Vector2(650, 40), new Vector2(0, yPos), 
				18, FontStyle.Normal, new Color(0.9f, 0.9f, 0.9f, 1f));
			yPos -= 55f;

			UIHelper.CreateText(panel, "Bon voyage ! 🌍✈️", 
				new Vector2(650, 35), new Vector2(0, yPos), 
				17, FontStyle.Normal, new Color(0.8f, 0.8f, 1f, 1f));
			yPos -= 80f;

			// Bouton retour à l'accueil GRAND
			UIHelper.CreateRoundedButton(panel, "🏠 Retour à l'accueil", 
				new Vector2(320, 60), new Vector2(0, yPos),
				new Color(0.2f, 0.8f, 0.4f, 1f),
				() => {
					// Nettoyer l'affichage circulaire
					if (circularDisplay != null)
					{
						circularDisplay.ClearAllOutfits();
						Destroy(circularDisplay.gameObject);
					}
					Destroy(canvas.gameObject);
					if (onReturnHome != null) onReturnHome();
				});
		}

		private void CreateText(GameObject parent, string content, Vector2 pos, Vector2 size, int fontSize, FontStyle style, Color color)
		{
			GameObject textGO = new GameObject("Text");
			textGO.transform.SetParent(parent.transform, false);
			
			Text text = textGO.AddComponent<Text>();
			text.text = content;
			text.fontSize = fontSize;
			text.fontStyle = style;
			text.alignment = TextAnchor.MiddleCenter;
			text.color = color;
			text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
			
			RectTransform rt = textGO.GetComponent<RectTransform>();
			rt.anchorMin = new Vector2(0.5f, 0.5f);
			rt.anchorMax = new Vector2(0.5f, 0.5f);
			rt.sizeDelta = size;
			rt.anchoredPosition = pos;
		}

		private GameObject CreateButton(GameObject parent, string text, Vector2 pos, Vector2 size, Action onClick)
		{
			// Utiliser UIHelper pour boutons arrondis modernes
			return UIHelper.CreateRoundedButton(parent, text, size, pos, new Color(0.2f, 0.8f, 0.4f, 1f), onClick);
		}
	}
}

