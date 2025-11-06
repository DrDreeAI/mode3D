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

		public void Show(Action onReturnHomeCallback)
		{
			onReturnHome = onReturnHomeCallback;
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

			// Panel
			GameObject panel = new GameObject("ThankYouPanel");
			panel.transform.SetParent(canvasGO.transform, false);
			Image panelBg = panel.AddComponent<Image>();
			panelBg.color = panelColor;
			RectTransform panelRt = panel.GetComponent<RectTransform>();
			panelRt.anchorMin = new Vector2(0.5f, 0.5f);
			panelRt.anchorMax = new Vector2(0.5f, 0.5f);
			panelRt.sizeDelta = new Vector2(550, 400);
			panelRt.anchoredPosition = Vector2.zero;

			// Titre avec icône succès
			float yPos = 120f;
			CreateText(panel, "✅", 
				new Vector2(0, yPos), new Vector2(100, 80), 60, FontStyle.Normal, new Color(0.3f, 1f, 0.3f, 1f));
			yPos -= 100f;

			// Message principal
			CreateText(panel, "Merci pour votre commande !", 
				new Vector2(0, yPos), new Vector2(500, 50), 24, FontStyle.Bold, Color.white);
			yPos -= 60f;

			// Message secondaire
			CreateText(panel, "Votre valise est prête pour le voyage.", 
				new Vector2(0, yPos), new Vector2(500, 35), 16, FontStyle.Normal, new Color(0.9f, 0.9f, 0.9f, 1f));
			yPos -= 40f;

			CreateText(panel, "Bon voyage ! 🌍✈️", 
				new Vector2(0, yPos), new Vector2(500, 30), 15, FontStyle.Normal, new Color(0.8f, 0.8f, 1f, 1f));

			// Bouton retour à l'accueil
			CreateButton(panel, "🏠 Retour à l'accueil", 
				new Vector2(0, -150), new Vector2(280, 55), 
				() => {
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
			GameObject btnGO = new GameObject($"Button_{text}");
			btnGO.transform.SetParent(parent.transform, false);
			
			Image btnImg = btnGO.AddComponent<Image>();
			btnImg.color = accentColor;
			
			Button btn = btnGO.AddComponent<Button>();
			btn.onClick.AddListener(() => onClick());
			
			var colors = btn.colors;
			colors.normalColor = accentColor;
			colors.highlightedColor = new Color(0.2f, 0.7f, 1f, 1f);
			colors.pressedColor = new Color(0.1f, 0.5f, 0.8f, 1f);
			btn.colors = colors;
			
			RectTransform btnRt = btnGO.GetComponent<RectTransform>();
			btnRt.anchorMin = new Vector2(0.5f, 0.5f);
			btnRt.anchorMax = new Vector2(0.5f, 0.5f);
			btnRt.sizeDelta = size;
			btnRt.anchoredPosition = pos;
			
			// Text
			GameObject textGO = new GameObject("Text");
			textGO.transform.SetParent(btnGO.transform, false);
			Text btnText = textGO.AddComponent<Text>();
			btnText.text = text;
			btnText.fontSize = 16;
			btnText.fontStyle = FontStyle.Bold;
			btnText.alignment = TextAnchor.MiddleCenter;
			btnText.color = Color.white;
			btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
			
			RectTransform textRt = textGO.GetComponent<RectTransform>();
			textRt.anchorMin = Vector2.zero;
			textRt.anchorMax = Vector2.one;
			textRt.offsetMin = Vector2.zero;
			textRt.offsetMax = Vector2.zero;
			
			return btnGO;
		}
	}
}

