using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mode3D.Destinations
{
	/// <summary>
	/// Écran moderne "Préparer ma valise" avec 2 tenues 3D miniatures
	/// Version moderne avec bords arrondis et marges
	/// </summary>
	public class SuitcasePreparationUI_Modern : MonoBehaviour
	{
		private Canvas canvas;
		private List<OutfitProposalUI.OutfitPresentation> outfits;
		private Action onPaymentComplete;
		private Action onBack;
		private GhostOutfitDisplay ghostDisplay1;
		private GhostOutfitDisplay ghostDisplay2;
		private int currentDisplayIndex = 0;

		public void ShowSuitcase(List<OutfitProposalUI.OutfitPresentation> outfitList, Action onPaymentComplete, Action onBackCallback)
		{
			this.outfits = outfitList;
			this.onPaymentComplete = onPaymentComplete;
			this.onBack = onBackCallback;
			
			// Créer 2 gestionnaires d'affichage 3D côte à côte
			GameObject displayGO1 = new GameObject("GhostOutfitDisplay_Suitcase_1");
			ghostDisplay1 = displayGO1.AddComponent<GhostOutfitDisplay>();
			ghostDisplay1.displayPosition = new Vector3(-0.6f, 0.5f, 3.5f);
			ghostDisplay1.outfitScale = 0.5f;
			
			GameObject displayGO2 = new GameObject("GhostOutfitDisplay_Suitcase_2");
			ghostDisplay2 = displayGO2.AddComponent<GhostOutfitDisplay>();
			ghostDisplay2.displayPosition = new Vector3(0.6f, 0.5f, 3.5f);
			ghostDisplay2.outfitScale = 0.5f;
			
			CreateModernUI();
			StartDisplayCycle();
		}

		private void CreateModernUI()
		{
			// Canvas
			GameObject canvasGO = new GameObject("SuitcaseCanvas");
			canvas = canvasGO.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			
			CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
			scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			scaler.referenceResolution = new Vector2(1920, 1080);
			
			canvasGO.AddComponent<GraphicRaycaster>();

			// CONTENEUR PRINCIPAL avec marges (plus petit en hauteur)
			GameObject mainPanel = UIHelper.CreateRoundedPanel(
				canvasGO, 
				new Vector2(550, 380), // Réduit en hauteur de 500 → 380
				Vector2.zero, 
				new Color(0.05f, 0.05f, 0.05f, 0.95f), 
				15f
			);

			float yPos = 160f; // Ajusté pour nouvelle hauteur

			// Titre
			UIHelper.CreateText(mainPanel, "🧳 Préparer ma valise",
				new Vector2(520, 40), new Vector2(0, yPos), 
				22, FontStyle.Bold, new Color(0.2f, 0.8f, 1f, 1f));
			yPos -= 50f;

			// Liste compacte des tenues
			CreateCompactOutfitList(mainPanel, ref yPos);

			yPos -= 35f;

			// Prix total dans un beau cadre
			GameObject priceBox = UIHelper.CreateRoundedPanel(
				mainPanel,
				new Vector2(500, 50),
				new Vector2(0, yPos),
				new Color(0.15f, 0.7f, 0.3f, 0.4f),
				10f
			);

			float totalPrice = CalculateTotalPrice();
			UIHelper.CreateText(priceBox, $"💰 TOTAL : {totalPrice:F2} €",
				new Vector2(480, 50), Vector2.zero,
				20, FontStyle.Bold, new Color(1f, 1f, 0.8f, 1f));

			yPos -= 70f;

			// Boutons en bas
			CreateBottomButtons(mainPanel, yPos);
		}

		private void CreateCompactOutfitList(GameObject parent, ref float yPos)
		{
			// Titre section
			UIHelper.CreateText(parent, "📋 Vos tenues sélectionnées :",
				new Vector2(520, 25), new Vector2(0, yPos),
				14, FontStyle.Bold, new Color(0.9f, 0.9f, 0.9f, 1f),
				TextAnchor.MiddleLeft);
			yPos -= 35f;

			// Scroll view pour la liste
			GameObject scrollGO = new GameObject("ScrollView");
			scrollGO.transform.SetParent(parent.transform, false);
			
			RectTransform scrollRt = scrollGO.AddComponent<RectTransform>();
			scrollRt.anchorMin = new Vector2(0.5f, 0.5f);
			scrollRt.anchorMax = new Vector2(0.5f, 0.5f);
			scrollRt.sizeDelta = new Vector2(520, 120); // Liste compacte
			scrollRt.anchoredPosition = new Vector2(0, yPos);

			ScrollRect scroll = scrollGO.AddComponent<ScrollRect>();
			scroll.horizontal = false;
			scroll.vertical = true;
			scroll.scrollSensitivity = 20f;

			// Viewport
			GameObject vpGO = new GameObject("Viewport");
			vpGO.transform.SetParent(scrollGO.transform, false);
			RectTransform vpRt = vpGO.AddComponent<RectTransform>();
			vpRt.anchorMin = Vector2.zero;
			vpRt.anchorMax = Vector2.one;
			vpRt.offsetMin = Vector2.zero;
			vpRt.offsetMax = Vector2.zero;
			vpGO.AddComponent<Mask>().showMaskGraphic = false;

			// Content
			GameObject content = new GameObject("Content");
			content.transform.SetParent(vpGO.transform, false);
			RectTransform contentRt = content.AddComponent<RectTransform>();
			contentRt.anchorMin = new Vector2(0, 1);
			contentRt.anchorMax = new Vector2(1, 1);
			contentRt.pivot = new Vector2(0.5f, 1);

			scroll.content = contentRt;
			scroll.viewport = vpRt;

			// Remplir avec les tenues (compact)
			float itemY = 0f;
			foreach (var outfit in outfits)
			{
				CreateCompactOutfitItem(content, outfit, itemY);
				itemY -= 32f; // Items compacts
			}

			contentRt.sizeDelta = new Vector2(0, Mathf.Abs(itemY));
			yPos -= 125f;
		}

		private void CreateCompactOutfitItem(GameObject parent, OutfitProposalUI.OutfitPresentation outfit, float yPos)
		{
			float price = GetPriceForCategory(outfit.category);

			GameObject itemGO = UIHelper.CreateRoundedPanel(
				parent,
				new Vector2(500, 28),
				new Vector2(0, yPos),
				new Color(0.15f, 0.15f, 0.15f, 0.6f),
				5f
			);

			// Texte de la tenue
			string itemText = $"{GetCategoryIcon(outfit.category)} Jour {outfit.dayNumber} - {outfit.category} ({outfit.selectedMaterial})";
			UIHelper.CreateText(itemGO, itemText,
				new Vector2(350, 28), new Vector2(-60, 0),
				12, FontStyle.Normal, Color.white,
				TextAnchor.MiddleLeft);

			// Prix
			UIHelper.CreateText(itemGO, $"{price:F2} €",
				new Vector2(80, 28), new Vector2(180, 0),
				13, FontStyle.Bold, new Color(0.6f, 1f, 0.6f, 1f));
		}

		private void CreateBottomButtons(GameObject parent, float yPos)
		{
			// Bouton Retour (gauche)
			UIHelper.CreateBackButton(parent, new Vector2(-180, yPos),
				() => { 
					CleanupDisplays();
					Destroy(canvas.gameObject); 
					if (onBack != null) onBack(); 
				});

			// Bouton Payer (centre-droit)
			UIHelper.CreateRoundedButton(parent, "💳 PAYER",
				new Vector2(180, 50), new Vector2(50, yPos),
				new Color(0.2f, 0.8f, 0.4f, 1f),
				() => {
					CleanupDisplays();
					Destroy(canvas.gameObject);
					if (onPaymentComplete != null) onPaymentComplete();
				});
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
				case OutfitType.Chill: return "🎽";
				case OutfitType.Sport: return "🏃";
				case OutfitType.Business: return "💼";
				default: return "👕";
			}
		}

		private void StartDisplayCycle()
		{
			if (outfits == null || outfits.Count == 0) return;
			
			ShowTwoOutfits(currentDisplayIndex);
			StartCoroutine(CycleThroughOutfits());
		}

		private void ShowTwoOutfits(int startIndex)
		{
			if (startIndex < outfits.Count && ghostDisplay1 != null)
			{
				var outfit1 = outfits[startIndex];
				ghostDisplay1.ShowGhostOutfit(outfit1.category, outfit1.selectedMaterial);
			}
			
			int nextIndex = (startIndex + 1) % outfits.Count;
			if (outfits.Count > 1 && ghostDisplay2 != null)
			{
				var outfit2 = outfits[nextIndex];
				ghostDisplay2.ShowGhostOutfit(outfit2.category, outfit2.selectedMaterial);
			}
			else if (outfits.Count == 1 && ghostDisplay2 != null)
			{
				ghostDisplay2.ClearOutfit();
			}
		}

		private System.Collections.IEnumerator CycleThroughOutfits()
		{
			while (true)
			{
				yield return new WaitForSeconds(3f);
				currentDisplayIndex = (currentDisplayIndex + 2) % outfits.Count;
				ShowTwoOutfits(currentDisplayIndex);
			}
		}

		private void CleanupDisplays()
		{
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

		void OnDestroy()
		{
			CleanupDisplays();
		}
	}
}

