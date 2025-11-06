using System;
using UnityEngine;
using UnityEngine.UI;

namespace Mode3D.Destinations
{
	/// <summary>
	/// Interface pour sélectionner 1 tenue 3D par activité
	/// Avec possibilité de changer tenue et couleur
	/// </summary>
	public class ActivityOutfitSelectionUI : MonoBehaviour
	{
		[Header("UI Style")]
		public Color panelColor = new Color(0f, 0f, 0f, 0.75f);
		public Color accentColor = new Color(0.15f, 0.6f, 0.9f, 1f);

		private Canvas canvas;
		private int currentActivityIndex = 0;
		private OutfitType[] activities = { OutfitType.Chill, OutfitType.Sport, OutfitType.Business };
		private string[] availableOutfits = { "Tenue 1", "Tenue 2", "Tenue 3" }; // Noms des tenues disponibles
		private string[] availableColors = { "Bleu", "Rouge", "Vert", "Noir", "Blanc" };
		
		private int currentOutfitIndex = 0;
		private int currentColorIndex = 0;
		
		private Action onComplete;
		private Action onBack;

		public void ShowActivityOutfitSelection(Action onCompleteCallback, Action onBackCallback)
		{
			onComplete = onCompleteCallback;
			onBack = onBackCallback;
			currentActivityIndex = 0;
			
			CreateUI();
			UpdateActivityUI();
		}

		private void CreateUI()
		{
			// Canvas
			GameObject canvasGO = new GameObject("ActivityOutfitCanvas");
			canvas = canvasGO.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			
			CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
			scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			scaler.referenceResolution = new Vector2(1920, 1080);
			
			canvasGO.AddComponent<GraphicRaycaster>();
			canvas.sortingOrder = 1000;
		}

		private void UpdateActivityUI()
		{
			// Nettoyer ancienne UI
			foreach (Transform child in canvas.transform)
			{
				Destroy(child.gameObject);
			}

			OutfitType currentActivity = activities[currentActivityIndex];
			ActivityOutfit outfit = ActivityOutfitManager.Instance.GetOutfitForActivity(currentActivity);

			// Panel principal
			GameObject panel = CreatePanel();

			// Bouton retour
			CreateBackButton(panel);

			float yPos = 140f;

			// Titre : Activité actuelle
			CreateText(panel, $"{GetActivityIcon(currentActivity)} Activité: {currentActivity}", 
				new Vector2(0, yPos), new Vector2(400, 40), 20, FontStyle.Bold, Color.white);
			yPos -= 60f;

			// Instruction
			CreateText(panel, "Choisissez votre tenue:", 
				new Vector2(0, yPos), new Vector2(400, 30), 16, FontStyle.Normal, Color.white);
			yPos -= 50f;

			// Affichage tenue actuelle
			string currentOutfitName = availableOutfits[currentOutfitIndex];
			CreateText(panel, $"Tenue: {currentOutfitName}", 
				new Vector2(0, yPos), new Vector2(400, 35), 18, FontStyle.Bold, new Color(0.8f, 1f, 0.8f, 1f));
			yPos -= 50f;

			// Boutons pour changer de tenue
			CreateButton(panel, "◄ Tenue Précédente", 
				new Vector2(-110, yPos), new Vector2(150, 40), 
				() => ChangeToPreviousOutfit());
			
			CreateButton(panel, "Tenue Suivante ►", 
				new Vector2(110, yPos), new Vector2(150, 40), 
				() => ChangeToNextOutfit());
			
			yPos -= 60f;

			// Affichage couleur actuelle
			string currentColor = availableColors[currentColorIndex];
			CreateText(panel, $"Couleur: {currentColor}", 
				new Vector2(0, yPos), new Vector2(400, 30), 16, FontStyle.Normal, Color.white);
			yPos -= 45f;

			// Boutons pour changer de couleur
			CreateButton(panel, "◄ Couleur", 
				new Vector2(-110, yPos), new Vector2(140, 38), 
				() => ChangeToPreviousColor());
			
			CreateButton(panel, "Couleur ►", 
				new Vector2(110, yPos), new Vector2(140, 38), 
				() => ChangeToNextColor());
			
			yPos -= 70f;

			// Bouton Valider
			bool isLastActivity = currentActivityIndex >= activities.Length - 1;
			string validateLabel = isLastActivity ? "✓ VALIDER ET CONTINUER" : "✓ ACTIVITÉ SUIVANTE";
			
			CreateButton(panel, validateLabel, 
				new Vector2(0, yPos), new Vector2(260, 50), 
				() => ValidateAndNext());

			// Afficher le mannequin 3D
			if (ActivityOutfitManager.Instance != null)
			{
				ActivityOutfitManager.Instance.ShowMannequinForActivity(currentActivity);
			}
		}

		private GameObject CreatePanel()
		{
			GameObject panel = new GameObject("Panel");
			panel.transform.SetParent(canvas.transform, false);
			
			Image panelBg = panel.AddComponent<Image>();
			panelBg.color = panelColor;
			
			RectTransform panelRt = panel.GetComponent<RectTransform>();
			panelRt.anchorMin = new Vector2(0.5f, 0.5f);
			panelRt.anchorMax = new Vector2(0.5f, 0.5f);
			panelRt.sizeDelta = new Vector2(450, 380);
			panelRt.anchoredPosition = Vector2.zero;

			// Titre background
			GameObject titleBg = new GameObject("TitleBg");
			titleBg.transform.SetParent(panel.transform, false);
			Image titleBgImg = titleBg.AddComponent<Image>();
			titleBgImg.color = accentColor;
			RectTransform titleRt = titleBg.GetComponent<RectTransform>();
			titleRt.anchorMin = new Vector2(0, 1);
			titleRt.anchorMax = new Vector2(1, 1);
			titleRt.pivot = new Vector2(0.5f, 1);
			titleRt.sizeDelta = new Vector2(0, 50);
			titleRt.anchoredPosition = Vector2.zero;

			CreateText(titleBg, $"Sélection des Tenues ({currentActivityIndex + 1}/{activities.Length})", 
				Vector2.zero, new Vector2(450, 50), 18, FontStyle.Bold, Color.white);

			return panel;
		}

		private void CreateBackButton(GameObject parent)
		{
			CreateButton(parent, "← Retour", 
				new Vector2(-180, 165), new Vector2(90, 35), 
				() => {
					if (ActivityOutfitManager.Instance != null)
						ActivityOutfitManager.Instance.ClearAllMannequins();
					Destroy(canvas.gameObject);
					if (onBack != null) onBack();
				});
		}

		private void ChangeToPreviousOutfit()
		{
			currentOutfitIndex--;
			if (currentOutfitIndex < 0) currentOutfitIndex = availableOutfits.Length - 1;
			
			UpdateCurrentOutfit();
			UpdateActivityUI();
		}

		private void ChangeToNextOutfit()
		{
			currentOutfitIndex++;
			if (currentOutfitIndex >= availableOutfits.Length) currentOutfitIndex = 0;
			
			UpdateCurrentOutfit();
			UpdateActivityUI();
		}

		private void ChangeToPreviousColor()
		{
			currentColorIndex--;
			if (currentColorIndex < 0) currentColorIndex = availableColors.Length - 1;
			
			UpdateCurrentColor();
			UpdateActivityUI();
		}

		private void ChangeToNextColor()
		{
			currentColorIndex++;
			if (currentColorIndex >= availableColors.Length) currentColorIndex = 0;
			
			UpdateCurrentColor();
			UpdateActivityUI();
		}

		private void UpdateCurrentOutfit()
		{
			if (ActivityOutfitManager.Instance != null)
			{
				OutfitType activity = activities[currentActivityIndex];
				string outfitName = availableOutfits[currentOutfitIndex];
				ActivityOutfitManager.Instance.ChangeOutfit(activity, outfitName);
			}
		}

		private void UpdateCurrentColor()
		{
			if (ActivityOutfitManager.Instance != null)
			{
				OutfitType activity = activities[currentActivityIndex];
				string color = availableColors[currentColorIndex];
				ActivityOutfitManager.Instance.ChangeColorVariant(activity, color);
			}
		}

		private void ValidateAndNext()
		{
			// Sauvegarder la tenue et couleur actuelles
			UpdateCurrentOutfit();
			UpdateCurrentColor();

			currentActivityIndex++;

			if (currentActivityIndex >= activities.Length)
			{
				// Toutes les activités configurées, aller au récap
				if (ActivityOutfitManager.Instance != null)
					ActivityOutfitManager.Instance.ClearAllMannequins();
				
				Destroy(canvas.gameObject);
				if (onComplete != null) onComplete();
			}
			else
			{
				// Activité suivante
				currentOutfitIndex = 0;
				currentColorIndex = 0;
				UpdateActivityUI();
			}
		}

		private string GetActivityIcon(OutfitType activity)
		{
			switch (activity)
			{
				case OutfitType.Chill: return "👕";
				case OutfitType.Sport: return "🏃";
				case OutfitType.Business: return "👔";
				default: return "";
			}
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
			btnText.fontSize = 14;
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

