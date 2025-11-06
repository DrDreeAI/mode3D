using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mode3D.Destinations
{
	/// <summary>
	/// Écran "Préparer ma valise" avec liste des vêtements et prix
	/// </summary>
	public class SuitcasePreparationUI : MonoBehaviour
	{
		[Header("UI Style")]
		public Color panelColor = new Color(0f, 0f, 0f, 0.75f);
		public Color accentColor = new Color(0.15f, 0.6f, 0.9f, 1f);

		private Canvas canvas;
		private List<OutfitProposalUI.OutfitPresentation> outfits;
		private Action onPaymentComplete;
		private Action onBack;
		private GhostOutfitDisplay ghostDisplay1; // Premier mannequin
		private GhostOutfitDisplay ghostDisplay2; // Deuxième mannequin
		private int currentDisplayIndex = 0; // Index de départ pour les tenues

		public void ShowSuitcase(List<OutfitProposalUI.OutfitPresentation> outfitList, Action onPaymentComplete, Action onBackCallback)
		{
			this.outfits = outfitList;
			this.onPaymentComplete = onPaymentComplete;
			this.onBack = onBackCallback;
			
			// Créer 2 gestionnaires d'affichage 3D côte à côte
			GameObject displayGO1 = new GameObject("GhostOutfitDisplay_Suitcase_1");
			ghostDisplay1 = displayGO1.AddComponent<GhostOutfitDisplay>();
			ghostDisplay1.displayPosition = new Vector3(-0.6f, 0.5f, 3.5f); // Gauche
			ghostDisplay1.outfitScale = 0.5f;
			
			GameObject displayGO2 = new GameObject("GhostOutfitDisplay_Suitcase_2");
			ghostDisplay2 = displayGO2.AddComponent<GhostOutfitDisplay>();
			ghostDisplay2.displayPosition = new Vector3(0.6f, 0.5f, 3.5f); // Droite
			ghostDisplay2.outfitScale = 0.5f;
			
			CreateUI();
			StartDisplayCycle();
		}

		private void CreateUI()
		{
			// Canvas
			GameObject canvasGO = new GameObject("SuitcaseCanvas");
			canvas = canvasGO.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			
			CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
			scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			scaler.referenceResolution = new Vector2(1920, 1080);
			
			canvasGO.AddComponent<GraphicRaycaster>();
			canvas.sortingOrder = 1000;

			// Panel
			GameObject panel = new GameObject("SuitcasePanel");
			panel.transform.SetParent(canvasGO.transform, false);
			Image panelBg = panel.AddComponent<Image>();
			panelBg.color = panelColor;
			RectTransform panelRt = panel.GetComponent<RectTransform>();
			panelRt.anchorMin = new Vector2(0.5f, 0.5f);
			panelRt.anchorMax = new Vector2(0.5f, 0.5f);
			panelRt.sizeDelta = new Vector2(500, 500);
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
			titleRt.sizeDelta = new Vector2(0, 55);
			titleRt.anchoredPosition = Vector2.zero;

			CreateText(titleBg, "🧳 Préparer ma valise", 
				Vector2.zero, new Vector2(500, 55), 20, FontStyle.Bold, Color.white);

			float yPos = 190f;

			// Destination et dates
			if (OutfitSelection.Instance != null)
			{
				CreateText(panel, $"🏙️ {OutfitSelection.Instance.selectedDestination}", 
					new Vector2(0, yPos), new Vector2(480, 25), 16, FontStyle.Bold, new Color(0.8f, 1f, 0.8f, 1f));
				yPos -= 30f;

				int nbJours = (OutfitSelection.Instance.endDate - OutfitSelection.Instance.startDate).Days + 1;
				CreateText(panel, $"📅 {nbJours} jours - {outfits.Count} tenues", 
					new Vector2(0, yPos), new Vector2(480, 22), 14, FontStyle.Normal, Color.white);
				yPos -= 50f;
			}

			// Liste des vêtements avec prix
			CreateOutfitList(panel, ref yPos);

			yPos -= 30f;

			// Prix total
			float totalPrice = CalculateTotalPrice();
			GameObject priceBox = new GameObject("PriceBox");
			priceBox.transform.SetParent(panel.transform, false);
			Image priceBoxImg = priceBox.AddComponent<Image>();
			priceBoxImg.color = new Color(0.2f, 0.8f, 0.4f, 0.3f);
			RectTransform priceBoxRt = priceBox.GetComponent<RectTransform>();
			priceBoxRt.anchorMin = new Vector2(0.5f, 0.5f);
			priceBoxRt.anchorMax = new Vector2(0.5f, 0.5f);
			priceBoxRt.sizeDelta = new Vector2(460, 45);
			priceBoxRt.anchoredPosition = new Vector2(0, yPos);

			CreateText(priceBox, $"💰 TOTAL: {totalPrice:F2} €", 
				Vector2.zero, new Vector2(460, 45), 18, FontStyle.Bold, new Color(1f, 1f, 0.6f, 1f));

			// Boutons
			CreateButton(panel, "← Retour", 
				new Vector2(-200, 220), new Vector2(90, 35), 
				() => { Destroy(canvas.gameObject); if (onBack != null) onBack(); });

			CreateButton(panel, "💳 PAYER", 
				new Vector2(0, -230), new Vector2(250, 55), 
				() => {
					Destroy(canvas.gameObject);
					if (onPaymentComplete != null) onPaymentComplete();
				});
		}

		private void CreateOutfitList(GameObject parent, ref float yPos)
		{
			// ScrollView
			GameObject scrollGO = new GameObject("ScrollView");
			scrollGO.transform.SetParent(parent.transform, false);
			
			RectTransform scrollRt = scrollGO.AddComponent<RectTransform>();
			scrollRt.anchorMin = new Vector2(0.5f, 0.5f);
			scrollRt.anchorMax = new Vector2(0.5f, 0.5f);
			scrollRt.sizeDelta = new Vector2(480, 200);
			scrollRt.anchoredPosition = new Vector2(0, yPos - 100);
			
			Image scrollBg = scrollGO.AddComponent<Image>();
			scrollBg.color = new Color(0.1f, 0.1f, 0.1f, 0.4f);
			
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

			// Remplir avec les tenues
			float itemHeight = 45f;
			float currentY = 0f;

			for (int i = 0; i < outfits.Count; i++)
			{
				var outfit = outfits[i];
				float price = GetPriceForCategory(outfit.category);
				
				CreateOutfitItem(content, outfit, price, currentY, 460f, itemHeight);
				currentY -= (itemHeight + 3f);
			}

			contentRt.sizeDelta = new Vector2(0, Mathf.Abs(currentY));
			yPos -= 220f;
		}

		private void CreateOutfitItem(GameObject parent, OutfitProposalUI.OutfitPresentation outfit, float price, float yPos, float width, float height)
		{
			GameObject itemGO = new GameObject($"OutfitItem_{outfit.dayNumber}_{outfit.category}");
			itemGO.transform.SetParent(parent.transform, false);
			
			RectTransform itemRt = itemGO.AddComponent<RectTransform>();
			itemRt.anchorMin = new Vector2(0.5f, 1);
			itemRt.anchorMax = new Vector2(0.5f, 1);
			itemRt.pivot = new Vector2(0.5f, 1);
			itemRt.sizeDelta = new Vector2(width, height);
			itemRt.anchoredPosition = new Vector2(0, yPos);
			
			Image itemBg = itemGO.AddComponent<Image>();
			itemBg.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);

			string itemText = $"{GetCategoryIcon(outfit.category)} Jour {outfit.dayNumber} - {outfit.category} ({outfit.selectedMaterial})";
			CreateText(itemGO, itemText, 
				new Vector2(-90, 0), new Vector2(280, height), 13, FontStyle.Normal, Color.white);

			CreateText(itemGO, $"{price:F2} €", 
				new Vector2(180, 0), new Vector2(80, height), 14, FontStyle.Bold, new Color(0.6f, 1f, 0.6f, 1f));
		}

		private float GetPriceForCategory(OutfitType category)
		{
			// Prix fictifs selon catégorie
			switch (category)
			{
				case OutfitType.Chill: return 45.99f;
				case OutfitType.Sport: return 65.99f;
				case OutfitType.Business: return 120.00f;
				default: return 50.00f;
			}
		}

		private float CalculateTotalPrice()
		{
			float total = 0f;
			foreach (var outfit in outfits)
			{
				total += GetPriceForCategory(outfit.category);
			}
			return total;
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
			text.alignment = TextAnchor.MiddleLeft;
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

		/// <summary>
		/// Démarre le cycle d'affichage des tenues en 3D
		/// </summary>
		private void StartDisplayCycle()
		{
			if (outfits == null || outfits.Count == 0) return;
			
			// Afficher les 2 premières tenues
			ShowTwoOutfits(currentDisplayIndex);
			
			// Démarrer la coroutine de rotation automatique
			StartCoroutine(CycleThroughOutfits());
		}

		private void ShowTwoOutfits(int startIndex)
		{
			// Afficher la première tenue sur le mannequin 1
			if (startIndex < outfits.Count && ghostDisplay1 != null)
			{
				var outfit1 = outfits[startIndex];
				ghostDisplay1.ShowGhostOutfit(outfit1.category, outfit1.selectedMaterial);
			}
			
			// Afficher la deuxième tenue sur le mannequin 2
			int nextIndex = (startIndex + 1) % outfits.Count;
			if (outfits.Count > 1 && ghostDisplay2 != null)
			{
				var outfit2 = outfits[nextIndex];
				ghostDisplay2.ShowGhostOutfit(outfit2.category, outfit2.selectedMaterial);
			}
			else if (outfits.Count == 1 && ghostDisplay2 != null)
			{
				// Si une seule tenue, ne rien afficher sur le deuxième mannequin
				ghostDisplay2.ClearOutfit();
			}
		}

		private System.Collections.IEnumerator CycleThroughOutfits()
		{
			while (true)
			{
				// Attendre 3 secondes avant de changer
				yield return new WaitForSeconds(3f);
				
				// Passer aux 2 tenues suivantes
				currentDisplayIndex = (currentDisplayIndex + 2) % outfits.Count;
				ShowTwoOutfits(currentDisplayIndex);
			}
		}

		void OnDestroy()
		{
			// Nettoyer les gestionnaires d'affichage
			if (ghostDisplay1 != null)
			{
				ghostDisplay1.ClearOutfit();
				Destroy(ghostDisplay1.gameObject);
			}
			
			if (ghostDisplay2 != null)
			{
				ghostDisplay2.ClearOutfit();
				Destroy(ghostDisplay2.gameObject);
			}
		}
	}
}

