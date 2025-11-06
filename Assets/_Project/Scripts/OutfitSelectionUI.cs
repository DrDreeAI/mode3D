using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Mode3D.Destinations
{
	public class OutfitSelectionUI : MonoBehaviour
	{
		[Header("UI Style")]
		public Color panelColor = new Color(0f, 0f, 0f, 0.7f);
		public Color accentColor = new Color(0.15f, 0.6f, 0.9f, 1f);
		public Font uiFont;

		private Canvas canvas;
		private int currentDayIndex = 0;
		private List<OutfitType> selectedOutfits = new List<OutfitType>();
		private GameObject outfitPanel;
		// Mannequin retiré - pas d'affichage pendant la sélection
		// private OutfitShowcaseManager showcaseManager;
		
		private Action onComplete;
		private Action onBack;

		public void ShowOutfitSelection(Action onCompleteCallback, Action onBackCallback)
		{
			onComplete = onCompleteCallback;
			onBack = onBackCallback;
			currentDayIndex = 0;
			
			// Créer le gestionnaire de showcase (mannequin)
			// Mannequin retiré
			// GameObject showcaseGO = new GameObject("OutfitShowcaseManager");
			// showcaseManager = showcaseGO.AddComponent<OutfitShowcaseManager>();
			
			CreateOutfitUI();
			UpdateDayUI();
			
			// Mannequin retiré
			// if (showcaseManager != null)
			// {
			// 	showcaseManager.ShowOutfitsForDay(currentDayIndex);
			// }
		}

		private void CreateOutfitUI()
		{
			// Canvas ScreenSpaceOverlay
			GameObject canvasGO = new GameObject("OutfitSelectionCanvas");
			canvas = canvasGO.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			
			CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
			scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			scaler.referenceResolution = new Vector2(1920, 1080);
			
			canvasGO.AddComponent<GraphicRaycaster>();
			canvas.sortingOrder = 1000;

			// Panel principal moderne arrondi GRAND et AÉRÉ
			outfitPanel = UIHelper.CreateRoundedPanel(
				canvasGO,
				new Vector2(650, 550), // Plus grand et aéré
				Vector2.zero,
				new Color(0.03f, 0.03f, 0.03f, 0.95f),
				25f // Grandes marges
			);

			// Bouton retour (flèche)
			CreateBackButton(outfitPanel);
		}

		private void UpdateDayUI()
		{
			if (OutfitSelection.Instance == null || currentDayIndex >= OutfitSelection.Instance.dailyOutfits.Count)
				return;

			// Nettoyer l'ancienne UI (garder seulement le panel et le bouton retour)
			foreach (Transform child in outfitPanel.transform)
			{
				if (child.name != "BackButton")
					Destroy(child.gameObject);
			}

			DayOutfit currentDay = OutfitSelection.Instance.dailyOutfits[currentDayIndex];
			selectedOutfits = new List<OutfitType>(currentDay.outfits);

			float yPos = 235f; // Ajusté pour plus grand panel

			// Titre avec jour - GRAND
			UIHelper.CreateText(outfitPanel, $"Jour {currentDayIndex + 1} - {currentDay.date:dd MMMM}", 
				new Vector2(620, 45), new Vector2(0, yPos), 
				22, FontStyle.Bold, new Color(0.2f, 0.8f, 1f, 1f));
			yPos -= 60f;

			// Météo et température
			UIHelper.CreateText(outfitPanel, $"{currentDay.weather}  |  {currentDay.temperature:F0}°C", 
				new Vector2(620, 35), new Vector2(0, yPos), 
				18, FontStyle.Normal, new Color(0.9f, 0.9f, 0.9f, 1f));
			yPos -= 70f;

			// Instructions
			UIHelper.CreateText(outfitPanel, "Choisissez vos tenues :", 
				new Vector2(620, 30), new Vector2(0, yPos), 
				16, FontStyle.Bold, new Color(0.8f, 0.8f, 0.8f, 1f));
			yPos -= 50f;

			// Boutons des catégories
			CreateOutfitButtons(outfitPanel, ref yPos);

			yPos -= 20f;

			// Liste des tenues sélectionnées avec possibilité de supprimer
			CreateSelectedOutfitsListWithDelete(outfitPanel, ref yPos);

			// Boutons navigation
			CreateNavigationButtons(outfitPanel);
		}

		private void CreateOutfitButtons(GameObject parent, ref float yPos)
		{
			string[] outfitLabels = { "👕 Chill", "🏃 Sport", "👔 Business" };
			OutfitType[] outfitTypes = { OutfitType.Chill, OutfitType.Sport, OutfitType.Business };
			
			float buttonWidth = 130f;
			float spacing = 10f;
			float startX = -(buttonWidth + spacing);

			for (int i = 0; i < outfitTypes.Length; i++)
			{
				float xPos = startX + i * (buttonWidth + spacing);
				OutfitType type = outfitTypes[i];
				
				GameObject btnGO = CreateButton(parent, outfitLabels[i], 
					new Vector2(xPos, yPos), new Vector2(buttonWidth, 45), 
					() => ToggleOutfit(type));
				
				// Highlight si sélectionné
				if (selectedOutfits.Contains(type))
				{
					btnGO.GetComponent<Image>().color = accentColor;
				}
			}
			
			yPos -= 60f;
		}

		private void CreateSelectedOutfitsListWithDelete(GameObject parent, ref float yPos)
		{
			if (selectedOutfits.Count > 0)
			{
				// Titre
				CreateText(parent, "Tenues sélectionnées:", 
					new Vector2(0, yPos), new Vector2(400, 25), 13, FontStyle.Bold, new Color(0.8f, 1f, 0.8f, 1f));
				yPos -= 30f;

				// Afficher chaque tenue avec bouton de suppression
				foreach (var outfit in selectedOutfits.ToArray()) // ToArray pour éviter modification pendant iteration
				{
					GameObject outfitItemGO = new GameObject($"SelectedOutfit_{outfit}");
					outfitItemGO.transform.SetParent(parent.transform, false);
					
					RectTransform itemRt = outfitItemGO.AddComponent<RectTransform>();
					itemRt.anchorMin = new Vector2(0.5f, 0.5f);
					itemRt.anchorMax = new Vector2(0.5f, 0.5f);
					itemRt.sizeDelta = new Vector2(320, 35);
					itemRt.anchoredPosition = new Vector2(0, yPos);
					
					Image itemBg = outfitItemGO.AddComponent<Image>();
					itemBg.color = new Color(0.2f, 0.8f, 0.2f, 0.2f); // Vert clair
					
					// Texte de la tenue
					CreateTextChild(outfitItemGO, $"{GetOutfitIcon(outfit)} {outfit}", 
						new Vector2(-30, 0), new Vector2(240, 35), 14, FontStyle.Normal, Color.white);
					
					// Bouton supprimer (croix)
					OutfitType capturedOutfit = outfit;
					GameObject deleteBtn = CreateSmallButton(outfitItemGO, "✖", 
						new Vector2(140, 0), new Vector2(30, 30), 
						() => RemoveOutfit(capturedOutfit));
					deleteBtn.GetComponent<Image>().color = new Color(0.9f, 0.3f, 0.3f, 1f); // Rouge
					
					yPos -= 40f;
				}
			}
		}

		private void RemoveOutfit(OutfitType outfit)
		{
			selectedOutfits.Remove(outfit);
			OutfitSelection.Instance.dailyOutfits[currentDayIndex].outfits = new List<OutfitType>(selectedOutfits);
			UpdateDayUI();
			
			// Mannequin retiré
			// if (showcaseManager != null)
			// {
			// 	showcaseManager.ShowOutfitsForDay(currentDayIndex);
			// }
		}

		private void CreateTextChild(GameObject parent, string content, Vector2 pos, Vector2 size, int fontSize, FontStyle style, Color color)
		{
			GameObject textGO = new GameObject("Text");
			textGO.transform.SetParent(parent.transform, false);
			
			Text text = textGO.AddComponent<Text>();
			text.text = content;
			text.font = GetFont();
			text.fontSize = fontSize;
			text.fontStyle = style;
			text.alignment = TextAnchor.MiddleLeft;
			text.color = color;
			
			RectTransform rt = textGO.GetComponent<RectTransform>();
			rt.anchorMin = new Vector2(0.5f, 0.5f);
			rt.anchorMax = new Vector2(0.5f, 0.5f);
			rt.sizeDelta = size;
			rt.anchoredPosition = pos;
		}

		private GameObject CreateSmallButton(GameObject parent, string text, Vector2 pos, Vector2 size, Action onClick)
		{
			GameObject btnGO = new GameObject("SmallButton");
			btnGO.transform.SetParent(parent.transform, false);
			
			Image btnImg = btnGO.AddComponent<Image>();
			btnImg.color = new Color(0.9f, 0.3f, 0.3f, 1f);
			
			Button btn = btnGO.AddComponent<Button>();
			btn.onClick.AddListener(() => onClick());
			
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
			btnText.font = GetFont();
			btnText.fontSize = 16;
			btnText.fontStyle = FontStyle.Bold;
			btnText.alignment = TextAnchor.MiddleCenter;
			btnText.color = Color.white;
			
			RectTransform textRt = textGO.GetComponent<RectTransform>();
			textRt.anchorMin = Vector2.zero;
			textRt.anchorMax = Vector2.one;
			textRt.offsetMin = Vector2.zero;
			textRt.offsetMax = Vector2.zero;
			
			return btnGO;
		}

		private void CreateNavigationButtons(GameObject parent)
		{
			float yPos = -140f;
			
			// Bouton "Changer de tenue" retiré car pas de mannequin
			// CreateButton(parent, "🔄 Changer de tenue", 
			// 	new Vector2(-100, yPos), new Vector2(180, 40), 
			// 	() => {
			// 		if (showcaseManager != null) showcaseManager.ChangeOutfitVariant();
			// 	});

			yPos -= 50f;

			// Bouton "Jour suivant" ou "Voir les tenues proposées"
			bool isLastDay = currentDayIndex >= OutfitSelection.Instance.dailyOutfits.Count - 1;
			string nextLabel = isLastDay ? "👗 Voir les tenues proposées" : "➡️ Jour suivant";
			
			CreateButton(parent, nextLabel, 
				new Vector2(0, yPos), new Vector2(280, 50), 
				() => NextDay());
		}

		private void CreateBackButton(GameObject parent)
		{
			UIHelper.CreateBackButton(parent, new Vector2(-265, 255),
				() => { 
					if (onBack != null) { Destroy(canvas.gameObject); onBack(); } 
				});
		}

		private GameObject CreateButton(GameObject parent, string text, Vector2 pos, Vector2 size, Action onClick)
		{
			// Utiliser UIHelper pour boutons arrondis modernes
			return UIHelper.CreateRoundedButton(parent, text, size, pos, accentColor, onClick);
		}

		private void CreateText(GameObject parent, string content, Vector2 pos, Vector2 size, int fontSize, FontStyle style, Color color)
		{
			GameObject textGO = new GameObject("Text");
			textGO.transform.SetParent(parent.transform, false);
			
			Text text = textGO.AddComponent<Text>();
			text.text = content;
			text.font = GetFont();
			text.fontSize = fontSize;
			text.fontStyle = style;
			text.alignment = TextAnchor.MiddleCenter;
			text.color = color;
			
			RectTransform rt = textGO.GetComponent<RectTransform>();
			rt.anchorMin = new Vector2(0.5f, 0.5f);
			rt.anchorMax = new Vector2(0.5f, 0.5f);
			rt.sizeDelta = size;
			rt.anchoredPosition = pos;
		}

		private void ToggleOutfit(OutfitType outfit)
		{
			if (selectedOutfits.Contains(outfit))
			{
				selectedOutfits.Remove(outfit);
			}
			else
			{
				selectedOutfits.Add(outfit);
			}
			
			// Sauvegarder
			OutfitSelection.Instance.dailyOutfits[currentDayIndex].outfits = new List<OutfitType>(selectedOutfits);
			
			// Rafraîchir UI
			UpdateDayUI();
			// Mannequin retiré
			// if (showcaseManager != null)
			// {
			// 	showcaseManager.ShowOutfitsForDay(currentDayIndex);
			// }
		}

		private void NextDay()
		{
			// Vérifier qu'au moins une tenue est sélectionnée
			if (selectedOutfits.Count == 0)
			{
				Debug.LogWarning("Veuillez sélectionner au moins une tenue!");
				return;
			}

			currentDayIndex++;
			
			if (currentDayIndex >= OutfitSelection.Instance.dailyOutfits.Count)
			{
				// Dernier jour atteint, aller aux propositions
				// Mannequin retiré
				// if (showcaseManager != null) showcaseManager.ClearMannequin();
				Destroy(canvas.gameObject);
				if (onComplete != null) onComplete();
			}
			else
			{
				// Jour suivant - mettre à jour UI
				UpdateDayUI();
				// Mannequin retiré
				// if (showcaseManager != null)
				// {
				// 	showcaseManager.ShowOutfitsForDay(currentDayIndex);
				// }
			}
		}

		private string GetOutfitIcon(OutfitType outfit)
		{
			switch (outfit)
			{
				case OutfitType.Chill: return "👕";
				case OutfitType.Sport: return "🏃";
				case OutfitType.Business: return "👔";
				default: return "";
			}
		}

		private Font GetFont()
		{
			if (uiFont != null) return uiFont;
			
			var legacy = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
			if (legacy != null) return legacy;
			
			return Font.CreateDynamicFontFromOSFont("Arial", 14);
		}
	}
}

