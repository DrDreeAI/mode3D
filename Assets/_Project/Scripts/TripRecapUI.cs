using System;
using UnityEngine;
using UnityEngine.UI;

namespace Mode3D.Destinations
{
	public class TripRecapUI : MonoBehaviour
	{
		[Header("UI Style")]
		public Color panelColor = new Color(0f, 0f, 0f, 0.7f);
		public Color accentColor = new Color(0.15f, 0.6f, 0.9f, 1f);
		public Font uiFont;

		private Canvas canvas;
		private Action onBack;

		public void ShowRecap(Action onBackCallback)
		{
			onBack = onBackCallback;
			CreateRecapUI();
		}

		private void CreateRecapUI()
		{
			// Canvas ScreenSpaceOverlay
			GameObject canvasGO = new GameObject("RecapCanvas");
			canvas = canvasGO.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			
			CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
			scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			scaler.referenceResolution = new Vector2(1920, 1080);
			
			canvasGO.AddComponent<GraphicRaycaster>();
			canvas.sortingOrder = 1000;

			// Panel principal - taille réduite
			GameObject panel = new GameObject("RecapPanel");
			panel.transform.SetParent(canvasGO.transform, false);
			Image panelBg = panel.AddComponent<Image>();
			panelBg.color = panelColor;
			RectTransform panelRt = panel.GetComponent<RectTransform>();
			panelRt.anchorMin = new Vector2(0.5f, 0.5f);
			panelRt.anchorMax = new Vector2(0.5f, 0.5f);
			panelRt.sizeDelta = new Vector2(450, 420); // Plus compact
			panelRt.anchoredPosition = Vector2.zero;

			// Titre
			float yPos = 210f;
			CreateText(panel, "📋 Récapitulatif de votre voyage", 
				new Vector2(0, yPos), new Vector2(500, 40), 20, FontStyle.Bold, Color.white);
			yPos -= 50f;

			// Destination et dates
			if (OutfitSelection.Instance != null)
			{
				string destination = OutfitSelection.Instance.selectedDestination;
				DateTime start = OutfitSelection.Instance.startDate;
				DateTime end = OutfitSelection.Instance.endDate;
				
				CreateText(panel, $"🏙️ Destination: {destination}", 
					new Vector2(0, yPos), new Vector2(430, 25), 16, FontStyle.Bold, new Color(0.8f, 1f, 0.8f, 1f));
				yPos -= 30f;
				
				CreateText(panel, $"📅 Du {start:dd/MM/yyyy} au {end:dd/MM/yyyy}", 
					new Vector2(0, yPos), new Vector2(430, 25), 14, FontStyle.Normal, Color.white);
				yPos -= 35f;

				// Récapitulatif des tenues
				string outfitSummary = CalculateOutfitSummary();
				CreateText(panel, outfitSummary, 
					new Vector2(0, yPos), new Vector2(430, 40), 13, FontStyle.Normal, new Color(1f, 0.9f, 0.6f, 1f));
				yPos -= 55f;

				// Zone scrollable pour la liste des jours
				CreateScrollableList(panel, yPos);
			}

			// Bouton retour
			CreateButton(panel, "← Retour", 
				new Vector2(-210, 220), new Vector2(100, 35), 
				() => { Destroy(canvas.gameObject); if (onBack != null) onBack(); });

			// Bouton Proposition des tenues (NOUVEAU!)
			CreateButton(panel, "👗 Proposition des tenues", 
				new Vector2(0, -210), new Vector2(280, 50), 
				() => {
					Destroy(canvas.gameObject);
					ShowOutfitProposals();
				});
		}

		private void CreateScrollableList(GameObject parent, float startY)
		{
			// ScrollView
			GameObject scrollGO = new GameObject("ScrollView");
			scrollGO.transform.SetParent(parent.transform, false);
			
			RectTransform scrollRt = scrollGO.AddComponent<RectTransform>();
			scrollRt.anchorMin = new Vector2(0.5f, 0.5f);
			scrollRt.anchorMax = new Vector2(0.5f, 0.5f);
			scrollRt.sizeDelta = new Vector2(490, 280);
			scrollRt.anchoredPosition = new Vector2(0, startY - 140);
			
			Image scrollBg = scrollGO.AddComponent<Image>();
			scrollBg.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
			
			ScrollRect scroll = scrollGO.AddComponent<ScrollRect>();
			scroll.horizontal = false;
			scroll.vertical = true;

			// Viewport
			GameObject viewport = new GameObject("Viewport");
			viewport.transform.SetParent(scrollGO.transform, false);
			RectTransform vpRt = viewport.AddComponent<RectTransform>();
			vpRt.anchorMin = Vector2.zero;
			vpRt.anchorMax = Vector2.one;
			vpRt.offsetMin = new Vector2(5, 5);
			vpRt.offsetMax = new Vector2(-5, -5);
			
			Mask mask = viewport.AddComponent<Mask>();
			mask.showMaskGraphic = false;
			Image vpImg = viewport.AddComponent<Image>();
			vpImg.color = new Color(1, 1, 1, 0);

			// Content
			GameObject content = new GameObject("Content");
			content.transform.SetParent(viewport.transform, false);
			RectTransform contentRt = content.AddComponent<RectTransform>();
			contentRt.anchorMin = new Vector2(0, 1);
			contentRt.anchorMax = new Vector2(1, 1);
			contentRt.pivot = new Vector2(0.5f, 1);
			
			scroll.content = contentRt;
			scroll.viewport = vpRt;

			// Remplir avec les jours
			if (OutfitSelection.Instance != null)
			{
				float itemHeight = 60f;
				float itemSpacing = 5f;
				float currentY = 0f;

				for (int i = 0; i < OutfitSelection.Instance.dailyOutfits.Count; i++)
				{
					DayOutfit day = OutfitSelection.Instance.dailyOutfits[i];
					CreateDayItem(content, day, i + 1, currentY, 470f, itemHeight);
					currentY -= (itemHeight + itemSpacing);
				}

				contentRt.sizeDelta = new Vector2(0, Mathf.Abs(currentY));
			}
		}

		private void CreateDayItem(GameObject parent, DayOutfit day, int dayNum, float yPos, float width, float height)
		{
			GameObject itemGO = new GameObject($"Day{dayNum}");
			itemGO.transform.SetParent(parent.transform, false);
			
			RectTransform itemRt = itemGO.AddComponent<RectTransform>();
			itemRt.anchorMin = new Vector2(0.5f, 1);
			itemRt.anchorMax = new Vector2(0.5f, 1);
			itemRt.pivot = new Vector2(0.5f, 1);
			itemRt.sizeDelta = new Vector2(width, height);
			itemRt.anchoredPosition = new Vector2(0, yPos);
			
			Image itemBg = itemGO.AddComponent<Image>();
			itemBg.color = new Color(0.15f, 0.6f, 0.9f, 0.3f);

			// Texte du jour
			string dayText = $"Jour {dayNum} - {day.date:dd MMM}  |  {day.weather}  {day.temperature:F0}°C\n";
			
			if (day.outfits.Count > 0)
			{
				dayText += "Tenues: ";
				foreach (var outfit in day.outfits)
				{
					dayText += GetOutfitIcon(outfit) + GetOutfitName(outfit) + "  ";
				}
			}
			else
			{
				dayText += "Aucune tenue sélectionnée";
			}

			CreateText(itemGO, dayText, 
				Vector2.zero, new Vector2(width - 20, height - 10), 12, FontStyle.Normal, Color.white);
		}

		private GameObject CreateButton(GameObject parent, string text, Vector2 pos, Vector2 size, Action onClick)
		{
			GameObject btnGO = new GameObject("Button_" + text);
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
			btnText.font = GetFont();
			btnText.fontSize = 14;
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

		private string GetOutfitName(OutfitType outfit)
		{
			return outfit.ToString();
		}

		private string CalculateOutfitSummary()
		{
			if (OutfitSelection.Instance == null || OutfitSelection.Instance.dailyOutfits.Count == 0)
				return "";

			int totalChill = 0;
			int totalSport = 0;
			int totalBusiness = 0;

			foreach (var day in OutfitSelection.Instance.dailyOutfits)
			{
				foreach (var outfit in day.outfits)
				{
					switch (outfit)
					{
						case OutfitType.Chill: totalChill++; break;
						case OutfitType.Sport: totalSport++; break;
						case OutfitType.Business: totalBusiness++; break;
					}
				}
			}

			return $"👕 Chill: {totalChill}  |  🏃 Sport: {totalSport}  |  👔 Business: {totalBusiness}";
		}

		private void ShowOutfitProposals()
		{
			// Créer l'écran de présentation 3D des tenues
			GameObject proposalGO = new GameObject("OutfitProposalUI");
			OutfitProposalUI proposalUI = proposalGO.AddComponent<OutfitProposalUI>();
			proposalUI.ShowProposals(
				onCompleteCallback: () => {
					// Retour au récap après validation
					Destroy(proposalGO);
					RecreateRecap();
				},
				onBackCallback: () => {
					// Retour au récap
					Destroy(proposalGO);
					RecreateRecap();
				}
			);
		}

		private void RecreateRecap()
		{
			// Recréer le récap (helper method)
			CreateRecapUI();
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

