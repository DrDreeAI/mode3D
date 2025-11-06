using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mode3D.Destinations
{
	/// <summary>
	/// Présentation 3D des tenues sélectionnées
	/// Parcourt chaque tenue (il peut y en avoir plusieurs par jour)
	/// </summary>
	public class OutfitProposalUI : MonoBehaviour
	{
		[Header("UI Style")]
		public Color panelColor = new Color(0f, 0f, 0f, 0.75f);
		public Color accentColor = new Color(0.15f, 0.6f, 0.9f, 1f);

		private Canvas canvas;
		private Action onComplete;
		private Action onBack;
		
		// Liste de toutes les tenues à présenter
		private List<OutfitPresentation> allOutfits = new List<OutfitPresentation>();
		private int currentOutfitIndex = 0;
		
		private string[] availableMaterials = GhostOutfitDisplay.GetAvailableMaterials();
		private int currentMaterialIndex = 0;
		
		private GhostOutfitDisplay ghostDisplay; // Affichage vêtements "fantôme"

		[Serializable]
		public class OutfitPresentation
		{
			public int dayNumber;
			public DateTime date;
			public OutfitType category;
			public string weather;
			public float temperature;
			public string selectedMaterial = "Matière 1"; // Matière par défaut
		}

		public void ShowProposals(Action onCompleteCallback, Action onBackCallback)
		{
			onComplete = onCompleteCallback;
			onBack = onBackCallback;
			
			// Construire la liste de toutes les tenues à présenter
			BuildOutfitList();
			
			if (allOutfits.Count == 0)
			{
				Debug.LogWarning("Aucune tenue sélectionnée!");
				if (onBack != null) onBack();
				return;
			}
			
			// Créer le gestionnaire d'affichage "fantôme"
			GameObject displayGO = new GameObject("GhostOutfitDisplay");
			ghostDisplay = displayGO.AddComponent<GhostOutfitDisplay>();
			
			currentOutfitIndex = 0;
			CreateUI();
			UpdateOutfitDisplay();
		}

		private void BuildOutfitList()
		{
			allOutfits.Clear();
			
			if (OutfitSelection.Instance == null) return;
			
			for (int dayIndex = 0; dayIndex < OutfitSelection.Instance.dailyOutfits.Count; dayIndex++)
			{
				DayOutfit day = OutfitSelection.Instance.dailyOutfits[dayIndex];
				
				// Pour chaque catégorie de tenue sélectionnée ce jour
				foreach (OutfitType category in day.outfits)
				{
					OutfitPresentation outfit = new OutfitPresentation
					{
						dayNumber = dayIndex + 1,
						date = day.date,
						category = category,
						weather = day.weather,
						temperature = day.temperature,
						selectedMaterial = "Matière 1" // Matière par défaut
					};
					allOutfits.Add(outfit);
				}
			}
			
			Debug.Log($"[OutfitProposal] {allOutfits.Count} tenues à présenter");
		}

		private void CreateUI()
		{
			// Canvas
			GameObject canvasGO = new GameObject("OutfitProposalCanvas");
			canvas = canvasGO.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			
			CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
			scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			scaler.referenceResolution = new Vector2(1920, 1080);
			
			canvasGO.AddComponent<GraphicRaycaster>();
			canvas.sortingOrder = 1000;
		}

		private void UpdateOutfitDisplay()
		{
			// Nettoyer l'UI précédente
			foreach (Transform child in canvas.transform)
			{
				Destroy(child.gameObject);
			}

			if (currentOutfitIndex >= allOutfits.Count) return;

			OutfitPresentation current = allOutfits[currentOutfitIndex];

			// Panel principal
			GameObject panel = CreatePanel();

			// Bouton retour
			CreateBackButton(panel);

			float yPos = 150f;

			// Progression
			CreateText(panel, $"Tenue {currentOutfitIndex + 1} / {allOutfits.Count}", 
				new Vector2(0, yPos), new Vector2(400, 30), 16, FontStyle.Bold, new Color(0.8f, 1f, 0.8f, 1f));
			yPos -= 45f;

			// Jour et date
			CreateText(panel, $"📅 Jour {current.dayNumber} - {current.date:dd MMMM}", 
				new Vector2(0, yPos), new Vector2(400, 30), 16, FontStyle.Normal, Color.white);
			yPos -= 35f;

			// Météo
			CreateText(panel, $"{current.weather} | {current.temperature:F0}°C", 
				new Vector2(0, yPos), new Vector2(400, 25), 14, FontStyle.Normal, Color.white);
			yPos -= 50f;

			// Catégorie de tenue
			CreateText(panel, $"{GetCategoryIcon(current.category)} Catégorie: {current.category}", 
				new Vector2(0, yPos), new Vector2(400, 35), 18, FontStyle.Bold, Color.white);
			yPos -= 55f;

			// Matière actuelle
			string currentMaterial = availableMaterials[currentMaterialIndex];
			CreateText(panel, $"Matière: {currentMaterial}", 
				new Vector2(0, yPos), new Vector2(400, 30), 16, FontStyle.Normal, new Color(1f, 0.9f, 0.6f, 1f));
			yPos -= 50f;

			// Boutons changement de matière
			CreateButton(panel, "◄ Matière Précédente", 
				new Vector2(-125, yPos), new Vector2(160, 40), 
				() => ChangeToPreviousMaterial());
			
			CreateButton(panel, "Matière Suivante ►", 
				new Vector2(125, yPos), new Vector2(160, 40), 
				() => ChangeToNextMaterial());
			
			yPos -= 70f;

			// Bouton valider la tenue
			bool isLastOutfit = currentOutfitIndex >= allOutfits.Count - 1;

			if (!isLastOutfit)
			{
				CreateButton(panel, "✓ Valider et Tenue Suivante", 
					new Vector2(0, yPos), new Vector2(240, 50), 
					() => ValidateAndNext());
			}
			else
			{
				// Dernier outfit : bouton valider final
				CreateButton(panel, "✓ VALIDER TOUT", 
					new Vector2(0, yPos), new Vector2(220, 50), 
					() => ValidateAll());
			}

			// Afficher le mannequin 3D
			ShowMannequinFor(current);
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
			panelRt.sizeDelta = new Vector2(450, 420);
			panelRt.anchoredPosition = Vector2.zero;

			// Titre
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

			CreateText(titleBg, "👗 Présentation des Tenues", 
				Vector2.zero, new Vector2(450, 50), 18, FontStyle.Bold, Color.white);

			return panel;
		}

		private void CreateBackButton(GameObject parent)
		{
			CreateButton(parent, "← Retour au Récap", 
				new Vector2(0, 190), new Vector2(150, 35), 
				() => {
					ClearMannequin();
					if (ghostDisplay != null) Destroy(ghostDisplay.gameObject);
					Destroy(canvas.gameObject);
					if (onBack != null) onBack();
				});
		}

		private void ChangeToPreviousMaterial()
		{
			currentMaterialIndex--;
			if (currentMaterialIndex < 0) currentMaterialIndex = availableMaterials.Length - 1;
			
			// Sauvegarder la matière
			allOutfits[currentOutfitIndex].selectedMaterial = availableMaterials[currentMaterialIndex];
			
			// Re-afficher la tenue avec la nouvelle matière
			if (ghostDisplay != null)
			{
				OutfitPresentation current = allOutfits[currentOutfitIndex];
				ghostDisplay.ShowGhostOutfit(current.category, availableMaterials[currentMaterialIndex]);
			}
			
			UpdateOutfitDisplay();
		}

		private void ChangeToNextMaterial()
		{
			currentMaterialIndex++;
			if (currentMaterialIndex >= availableMaterials.Length) currentMaterialIndex = 0;
			
			// Sauvegarder la matière
			allOutfits[currentOutfitIndex].selectedMaterial = availableMaterials[currentMaterialIndex];
			
			// Changer la matière des vêtements fantômes
			if (ghostDisplay != null)
			{
				OutfitPresentation current = allOutfits[currentOutfitIndex];
				ghostDisplay.ShowGhostOutfit(current.category, availableMaterials[currentMaterialIndex]);
			}
			
			UpdateOutfitDisplay();
		}

		// Navigation entre tenues supprimée - une seule tenue à la fois
		
		private void ValidateAndNext()
		{
			// Valider la tenue actuelle et passer à la suivante
			if (currentOutfitIndex < allOutfits.Count - 1)
			{
				currentOutfitIndex++;
				currentMaterialIndex = GetMaterialIndex(allOutfits[currentOutfitIndex].selectedMaterial);
				UpdateOutfitDisplay();
			}
		}

		private void ValidateAll()
		{
			// Toutes les tenues validées → Aller à la préparation de valise
			Debug.Log("=== TOUTES LES TENUES VALIDÉES ===");
			foreach (var outfit in allOutfits)
			{
				Debug.Log($"Jour {outfit.dayNumber} - {outfit.category}: Matière {outfit.selectedMaterial}");
			}
			
			ClearMannequin();
			if (ghostDisplay != null) Destroy(ghostDisplay.gameObject);
			Destroy(canvas.gameObject);
			
			// Créer l'écran de préparation de valise
			ShowSuitcasePreparation();
		}

		private void ShowSuitcasePreparation()
		{
			GameObject suitcaseGO = new GameObject("SuitcasePreparationUI");
			SuitcasePreparationUI suitcaseUI = suitcaseGO.AddComponent<SuitcasePreparationUI>();
			suitcaseUI.ShowSuitcase(
				outfitList: allOutfits,
				onPaymentComplete: () => {
					Destroy(suitcaseGO);
					ShowThankYou();
				},
				onBackCallback: () => {
					Destroy(suitcaseGO);
					if (onBack != null) onBack();
				}
			);
		}

		private void ShowThankYou()
		{
			GameObject thankYouGO = new GameObject("ThankYouUI");
			ThankYouUI thankYouUI = thankYouGO.AddComponent<ThankYouUI>();
			thankYouUI.Show(() => {
				// Retour à l'accueil (sélection ville)
				Destroy(thankYouGO);
				RestartApplication();
			});
		}

		private void RestartApplication()
		{
			// Nettoyer tout et revenir au début
			if (OutfitSelection.Instance != null)
				Destroy(OutfitSelection.Instance.gameObject);
			
			// Nettoyer tous les canvas
			Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
			foreach (Canvas c in allCanvases)
			{
				Destroy(c.gameObject);
			}
			
			// Forcer le DestinationSelector à recréer son UI
			DestinationSelector selector = FindFirstObjectByType<DestinationSelector>();
			if (selector != null)
			{
				// Utiliser SendMessage pour appeler une méthode publique
				selector.SendMessage("RestartUI", SendMessageOptions.DontRequireReceiver);
			}
		}

		private void ShowMannequinFor(OutfitPresentation outfit)
		{
			if (ghostDisplay == null) return;

			// Afficher les vêtements "fantôme" (suspendus sans corps)
			ghostDisplay.ShowGhostOutfit(outfit.category, availableMaterials[currentMaterialIndex]);
		}


		private void ClearMannequin()
		{
			if (ghostDisplay != null)
			{
				ghostDisplay.ClearOutfit();
			}
		}

		private int GetMaterialIndex(string materialName)
		{
			for (int i = 0; i < availableMaterials.Length; i++)
			{
				if (availableMaterials[i] == materialName) return i;
			}
			return 0;
		}

		private string GetCategoryIcon(OutfitType category)
		{
			switch (category)
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
			btnText.fontSize = 13;
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


