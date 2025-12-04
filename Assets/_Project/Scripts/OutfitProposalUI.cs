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

		// Panel principal (bouton retour créé dans CreatePanel)
		GameObject panel = CreatePanel();

		float yPos = 210f; // Ajusté pour grand panel

			// Progression GRANDE
			UIHelper.CreateText(panel, $"Tenue {currentOutfitIndex + 1} / {allOutfits.Count}", 
				new Vector2(650, 35), new Vector2(0, yPos), 
				18, FontStyle.Bold, new Color(0.8f, 1f, 0.8f, 1f));
			yPos -= 55f;

			// Jour et date
			UIHelper.CreateText(panel, $"📅 Jour {current.dayNumber} - {current.date:dd MMMM}", 
				new Vector2(650, 35), new Vector2(0, yPos), 
				18, FontStyle.Normal, new Color(0.9f, 0.9f, 0.9f, 1f));
			yPos -= 45f;

			// Météo
			UIHelper.CreateText(panel, $"{current.weather} | {current.temperature:F0}°C", 
				new Vector2(650, 30), new Vector2(0, yPos), 
				16, FontStyle.Normal, new Color(0.8f, 0.8f, 0.8f, 1f));
			yPos -= 60f;

			// Catégorie de tenue GRANDE
			UIHelper.CreateText(panel, $"{GetCategoryIcon(current.category)} Catégorie: {current.category}", 
				new Vector2(650, 40), new Vector2(0, yPos), 
				20, FontStyle.Bold, Color.white);
			yPos -= 65f;

			// Matière actuelle
			string currentMaterial = availableMaterials[currentMaterialIndex];
			UIHelper.CreateText(panel, $"🎨 Matière: {currentMaterial}", 
				new Vector2(650, 35), new Vector2(0, yPos), 
				18, FontStyle.Normal, new Color(1f, 0.9f, 0.6f, 1f));
			yPos -= 60f;

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
		// Panel arrondi moderne GRAND et AÉRÉ
		GameObject panel = UIHelper.CreateRoundedPanel(
			canvas.gameObject,
			new Vector2(700, 600), // Plus grand et aéré
			Vector2.zero,
			new Color(0.03f, 0.03f, 0.03f, 0.95f),
			25f // Grandes marges
		);

		// Titre GRAND
		UIHelper.CreateText(panel, "👗 Présentation des Tenues",
			new Vector2(650, 50), new Vector2(0, 270),
			26, FontStyle.Bold, new Color(0.2f, 0.8f, 1f, 1f));
		
		// Bouton retour au-dessus à gauche du panel
		UIHelper.CreateBackButton(canvas.gameObject, new Vector2(-240, 330),
			() => { 
				CleanupDisplays();
				if (onBack != null) { Destroy(canvas.gameObject); onBack(); } 
			});

		return panel;
	}

	private void CleanupDisplays()
	{
		ClearMannequin();
		if (ghostDisplay != null) Destroy(ghostDisplay.gameObject);
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

		private SuitcasePreparationUI_Final currentSuitcaseUI; // Garder la référence

		private void ShowSuitcasePreparation()
		{
			GameObject suitcaseGO = new GameObject("SuitcasePreparationUI_Final");
			currentSuitcaseUI = suitcaseGO.AddComponent<SuitcasePreparationUI_Final>();
			currentSuitcaseUI.ShowSuitcase(
				outfitList: allOutfits,
				onPaymentComplete: () => {
					// Récupérer le circularDisplay avant de détruire
					CircularOutfitDisplay circDisplay = currentSuitcaseUI.GetCircularDisplay();
					Destroy(suitcaseGO);
					ShowThankYou(circDisplay);
				},
				onBackCallback: () => {
					Destroy(suitcaseGO);
					if (onBack != null) onBack();
				}
			);
		}

	private void ShowThankYou(CircularOutfitDisplay circDisplay)
	{
		GameObject thankYouGO = new GameObject("ThankYouUI");
		ThankYouUI thankYouUI = thankYouGO.AddComponent<ThankYouUI>();
		
		// Calculer prix total
		float totalPrice = CalculateTotalPrice();
		string destination = OutfitSelection.Instance != null ? OutfitSelection.Instance.selectedDestination : "";
		
		thankYouUI.Show(() => {
			// Retour à l'accueil (sélection ville)
			Destroy(thankYouGO);
			RestartApplication();
		}, circDisplay, allOutfits, totalPrice, destination);
	}
	
	private float CalculateTotalPrice()
	{
		float total = 0f;
		foreach (var outfit in allOutfits)
		{
			total += GetPriceForCategory(outfit.category);
		}
		return total;
	}
	
	private float GetPriceForCategory(OutfitType category)
	{
		switch (category)
		{
			case OutfitType.Chill: return 45.99f;
			case OutfitType.Sport: return 65.99f;
			case OutfitType.Business: return 120.00f;
			default: return 50.00f;
		}
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
			// Utiliser UIHelper pour boutons arrondis modernes
			return UIHelper.CreateRoundedButton(parent, text, size, pos, accentColor, onClick);
		}
	}
}


