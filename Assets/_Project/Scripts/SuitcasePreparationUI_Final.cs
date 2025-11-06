using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mode3D.Destinations
{
	/// <summary>
	/// Écran final "Préparer ma valise" avec icône caddie minimisable
	/// </summary>
	public class SuitcasePreparationUI_Final : MonoBehaviour
	{
		private Canvas canvas;
		private GameObject mainPanel;
		private GameObject cartIcon; // Icône caddie
		private List<OutfitProposalUI.OutfitPresentation> outfits;
		private Action onPaymentComplete;
		private Action onBack;
		private CircularOutfitDisplay circularDisplay;
		private bool isMinimized = false;

		public void ShowSuitcase(List<OutfitProposalUI.OutfitPresentation> outfitList, Action onPaymentComplete, Action onBackCallback)
		{
			this.outfits = outfitList;
			this.onPaymentComplete = onPaymentComplete;
			this.onBack = onBackCallback;
			
			// Créer l'affichage circulaire des tenues
			GameObject circularGO = new GameObject("CircularOutfitDisplay");
			circularDisplay = circularGO.AddComponent<CircularOutfitDisplay>();
			circularDisplay.ShowAllOutfitsInCircle(outfits);
			
			CreateUI();
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

			// Panel principal GRAND et AÉRÉ
			mainPanel = UIHelper.CreateRoundedPanel(
				canvasGO, 
				new Vector2(700, 600), // GRAND et aéré
				Vector2.zero, 
				new Color(0.03f, 0.03f, 0.03f, 0.95f), 
				25f // Grandes marges
			);

			CreateMainContent();
			CreateCartIcon(canvasGO);
		}

		private void CreateMainContent()
		{
			float yPos = 270f;

			// Titre grand et visible
			UIHelper.CreateText(mainPanel, "🧳 Préparation de votre valise",
				new Vector2(650, 50), new Vector2(0, yPos), 
				26, FontStyle.Bold, new Color(0.2f, 0.8f, 1f, 1f));
			yPos -= 70f;

			// Info destination
			if (OutfitSelection.Instance != null)
			{
				UIHelper.CreateText(mainPanel, $"🏙️ {OutfitSelection.Instance.selectedDestination}",
					new Vector2(650, 30), new Vector2(0, yPos),
					18, FontStyle.Bold, new Color(0.8f, 1f, 0.8f, 1f));
				yPos -= 45f;

				int nbJours = (OutfitSelection.Instance.endDate - OutfitSelection.Instance.startDate).Days + 1;
				UIHelper.CreateText(mainPanel, $"📅 {nbJours} jours • {outfits.Count} tenues sélectionnées",
					new Vector2(650, 25), new Vector2(0, yPos),
					15, FontStyle.Normal, new Color(0.7f, 0.7f, 0.7f, 1f));
				yPos -= 60f;
			}

			// Liste des tenues avec scroll
			CreateOutfitList(mainPanel, ref yPos);

			yPos -= 50f;

			// Prix total dans un cadre stylé
			float totalPrice = CalculateTotalPrice();
			GameObject priceBox = UIHelper.CreateRoundedPanel(
				mainPanel,
				new Vector2(650, 65),
				new Vector2(0, yPos),
				new Color(0.1f, 0.6f, 0.3f, 0.5f),
				15f
			);

			UIHelper.CreateText(priceBox, $"💰 TOTAL : {totalPrice:F2} €",
				new Vector2(620, 65), Vector2.zero,
				24, FontStyle.Bold, new Color(1f, 1f, 0.9f, 1f));

			yPos -= 90f;

			// Boutons en bas avec espacement
			CreateBottomButtons(mainPanel, yPos);
		}

		private void CreateOutfitList(GameObject parent, ref float yPos)
		{
			// Titre section
			UIHelper.CreateText(parent, "📋 Liste de vos tenues :",
				new Vector2(650, 30), new Vector2(0, yPos),
				16, FontStyle.Bold, new Color(0.9f, 0.9f, 0.9f, 1f),
				TextAnchor.MiddleLeft);
			yPos -= 45f;

			// Conteneur scroll arrondi
			GameObject scrollContainer = UIHelper.CreateRoundedPanel(
				parent,
				new Vector2(650, 180), // Grande zone scrollable
				new Vector2(0, yPos),
				new Color(0.08f, 0.08f, 0.08f, 0.7f),
				10f
			);

			// ScrollRect
			ScrollRect scroll = scrollContainer.AddComponent<ScrollRect>();
			scroll.horizontal = false;
			scroll.vertical = true;
			scroll.scrollSensitivity = 20f;

			// Viewport
			GameObject viewport = new GameObject("Viewport");
			viewport.transform.SetParent(scrollContainer.transform, false);
			RectTransform vpRt = viewport.AddComponent<RectTransform>();
			vpRt.anchorMin = Vector2.zero;
			vpRt.anchorMax = Vector2.one;
			vpRt.offsetMin = new Vector2(10, 10);
			vpRt.offsetMax = new Vector2(-10, -10);
			viewport.AddComponent<Mask>().showMaskGraphic = false;

			// Content
			GameObject content = new GameObject("Content");
			content.transform.SetParent(viewport.transform, false);
			RectTransform contentRt = content.AddComponent<RectTransform>();
			contentRt.anchorMin = new Vector2(0, 1);
			contentRt.anchorMax = new Vector2(1, 1);
			contentRt.pivot = new Vector2(0.5f, 1);

			scroll.content = contentRt;
			scroll.viewport = vpRt;

			// Créer les items
			float itemY = 0f;
			foreach (var outfit in outfits)
			{
				CreateOutfitItem(content, outfit, itemY);
				itemY -= 45f; // Espacement entre items
			}

			contentRt.sizeDelta = new Vector2(0, Mathf.Abs(itemY));
			yPos -= 185f;
		}

		private void CreateOutfitItem(GameObject parent, OutfitProposalUI.OutfitPresentation outfit, float yPos)
		{
			float price = GetPriceForCategory(outfit.category);

			// Item arrondi
			GameObject itemGO = UIHelper.CreateRoundedPanel(
				parent,
				new Vector2(610, 38),
				new Vector2(0, yPos),
				new Color(0.12f, 0.12f, 0.12f, 0.8f),
				8f
			);

			// Texte de la tenue
			string itemText = $"{GetCategoryIcon(outfit.category)} Jour {outfit.dayNumber} - {outfit.category} • {outfit.selectedMaterial}";
			UIHelper.CreateText(itemGO, itemText,
				new Vector2(420, 38), new Vector2(-80, 0),
				14, FontStyle.Normal, Color.white,
				TextAnchor.MiddleLeft);

			// Prix
			UIHelper.CreateText(itemGO, $"{price:F2} €",
				new Vector2(100, 38), new Vector2(230, 0),
				15, FontStyle.Bold, new Color(0.6f, 1f, 0.6f, 1f));
		}

		private void CreateBottomButtons(GameObject parent, float yPos)
		{
			// Bouton Retour (gauche)
			UIHelper.CreateBackButton(parent, new Vector2(-260, yPos),
				() => { 
					CleanupDisplays();
					Destroy(canvas.gameObject); 
					if (onBack != null) onBack(); 
				});

			// Bouton Payer (droite)
			UIHelper.CreateRoundedButton(parent, "💳 PAYER",
				new Vector2(220, 55), new Vector2(100, yPos),
				new Color(0.2f, 0.8f, 0.4f, 1f),
				() => {
					CleanupDisplays();
					Destroy(canvas.gameObject);
					if (onPaymentComplete != null) onPaymentComplete();
				});
		}

		private void CreateCartIcon(GameObject canvasGO)
		{
			// Icône caddie en bas à droite (cliquable pour minimiser/maximiser)
			cartIcon = UIHelper.CreateRoundedPanel(
				canvasGO,
				new Vector2(80, 80),
				new Vector2(860, -450), // Bas droite
				new Color(0.2f, 0.8f, 0.4f, 0.95f),
				10f
			);

			// Texte caddie
			UIHelper.CreateText(cartIcon, "🛒",
				new Vector2(60, 60), Vector2.zero,
				40, FontStyle.Normal, Color.white);

			// Rendre cliquable
			Button cartBtn = cartIcon.AddComponent<Button>();
			cartBtn.onClick.AddListener(ToggleMinimize);

			var colors = cartBtn.colors;
			colors.normalColor = new Color(0.2f, 0.8f, 0.4f, 0.95f);
			colors.highlightedColor = new Color(0.3f, 0.9f, 0.5f, 1f);
			colors.pressedColor = new Color(0.15f, 0.7f, 0.35f, 1f);
			cartBtn.colors = colors;
		}

		private void ToggleMinimize()
		{
			isMinimized = !isMinimized;

			if (isMinimized)
			{
				// Cacher le panel principal
				mainPanel.SetActive(false);
				
				// Ajouter un badge de nombre sur le caddie
				UpdateCartBadge();
			}
			else
			{
				// Restaurer le panel
				mainPanel.SetActive(true);
			}

			Debug.Log($"[Suitcase] {(isMinimized ? "Minimisé" : "Restauré")}");
		}

		private void UpdateCartBadge()
		{
			// Ajouter un petit badge avec le nombre de tenues
			GameObject existingBadge = cartIcon.transform.Find("Badge")?.gameObject;
			if (existingBadge != null) Destroy(existingBadge);

			GameObject badge = UIHelper.CreateRoundedPanel(
				cartIcon,
				new Vector2(30, 30),
				new Vector2(25, 25),
				new Color(1f, 0.3f, 0.3f, 1f),
				3f
			);
			badge.name = "Badge";

			UIHelper.CreateText(badge, outfits.Count.ToString(),
				new Vector2(30, 30), Vector2.zero,
				14, FontStyle.Bold, Color.white);
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

		/// <summary>
		/// Récupère le CircularDisplay pour le passer à l'écran suivant
		/// </summary>
		public CircularOutfitDisplay GetCircularDisplay()
		{
			return circularDisplay;
		}

		private void CleanupDisplays()
		{
			// NE PAS détruire circularDisplay ici - il sera passé à ThankYouUI
			// if (circularDisplay != null)
			// {
			// 	circularDisplay.ClearAllOutfits();
			// 	Destroy(circularDisplay.gameObject);
			// }
		}

		void OnDestroy()
		{
			// Le circularDisplay est géré par ThankYouUI maintenant
		}
	}
}

