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
	private GameObject backButton; // Bouton retour
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
			new Vector2(700, 700), // PLUS GRAND pour plus d'espace
			Vector2.zero, 
			new Color(0.03f, 0.03f, 0.03f, 0.95f), 
			25f // Grandes marges
		);

		// Flèche retour au-dessus à gauche du panel
		backButton = UIHelper.CreateBackButton(canvasGO, new Vector2(-240, 375),
			() => { 
				CleanupDisplays();
				Destroy(canvas.gameObject); 
				if (onBack != null) onBack(); 
			});

		CreateMainContent();
		CreateCartIcon(canvasGO);
		}

	private void CreateMainContent()
	{
		float yPos = 320f;

		// Titre grand et visible
		UIHelper.CreateText(mainPanel, "🧳 Préparation de votre valise",
			new Vector2(650, 50), new Vector2(0, yPos), 
			26, FontStyle.Bold, new Color(0.2f, 0.8f, 1f, 1f));
		yPos -= 65f;

		// Info destination
		if (OutfitSelection.Instance != null)
		{
			UIHelper.CreateText(mainPanel, $"🏙️ {OutfitSelection.Instance.selectedDestination}",
				new Vector2(650, 30), new Vector2(0, yPos),
				18, FontStyle.Bold, new Color(0.8f, 1f, 0.8f, 1f));
			yPos -= 40f;

			int nbJours = (OutfitSelection.Instance.endDate - OutfitSelection.Instance.startDate).Days + 1;
			UIHelper.CreateText(mainPanel, $"📅 {nbJours} jours • {outfits.Count} tenues sélectionnées",
				new Vector2(650, 25), new Vector2(0, yPos),
				15, FontStyle.Normal, new Color(0.7f, 0.7f, 0.7f, 1f));
			yPos -= 50f;
		}

		// Liste des tenues avec scroll - POSITION FIXE AU CENTRE
		float scrollYPos = 40f; // Position centrale fixe pour le scroll
		CreateOutfitList(mainPanel, scrollYPos);

		// Prix total en bas
		float priceYPos = -160f; // Position fixe au-dessus des boutons
		float totalPrice = CalculateTotalPrice();
		GameObject priceBox = UIHelper.CreateRoundedPanel(
			mainPanel,
			new Vector2(650, 65),
			new Vector2(0, priceYPos),
			new Color(0.1f, 0.6f, 0.3f, 0.5f),
			15f
		);

		UIHelper.CreateText(priceBox, $"💰 TOTAL : {totalPrice:F2} €",
			new Vector2(620, 65), Vector2.zero,
			24, FontStyle.Bold, new Color(1f, 1f, 0.9f, 1f));

		// Boutons en bas - POSITION FIXE
		float buttonsYPos = -270f; // Position fixe en bas
		CreateBottomButtons(mainPanel, buttonsYPos);
	}

	private void CreateOutfitList(GameObject parent, float centerYPos)
	{
		// Titre section au-dessus du scroll
		float titleYPos = centerYPos + 150f;
		UIHelper.CreateText(parent, "📋 Liste de vos tenues :",
			new Vector2(650, 30), new Vector2(0, titleYPos),
			16, FontStyle.Bold, new Color(0.9f, 0.9f, 0.9f, 1f),
			TextAnchor.MiddleLeft);

		// Calculer la hauteur nécessaire (max 240px pour le scroll)
		float itemHeight = 45f;
		float maxScrollHeight = 240f;
		float contentHeight = outfits.Count * itemHeight;
		float scrollHeight = Mathf.Min(contentHeight + 20f, maxScrollHeight);

		// Conteneur scroll arrondi avec hauteur dynamique - POSITION FIXE
		GameObject scrollContainer = UIHelper.CreateRoundedPanel(
			parent,
			new Vector2(650, scrollHeight),
			new Vector2(0, centerYPos),
			new Color(0.08f, 0.08f, 0.08f, 0.7f),
			10f
		);

		// ScrollRect
		ScrollRect scroll = scrollContainer.AddComponent<ScrollRect>();
		scroll.horizontal = false;
		scroll.vertical = true;
		scroll.scrollSensitivity = 20f;
		scroll.movementType = ScrollRect.MovementType.Clamped;
		scroll.inertia = true;
		scroll.decelerationRate = 0.135f;

		// Viewport
		GameObject viewport = new GameObject("Viewport");
		viewport.transform.SetParent(scrollContainer.transform, false);
		RectTransform vpRt = viewport.AddComponent<RectTransform>();
		vpRt.anchorMin = Vector2.zero;
		vpRt.anchorMax = Vector2.one;
		vpRt.offsetMin = new Vector2(10, 10);
		vpRt.offsetMax = new Vector2(-30, -10); // Espace pour scrollbar
		Image vpImg = viewport.AddComponent<Image>();
		vpImg.color = Color.white;
		vpImg.raycastTarget = false;
		Mask mask = viewport.AddComponent<Mask>();
		mask.showMaskGraphic = false;

		// Content
		GameObject content = new GameObject("Content");
		content.transform.SetParent(viewport.transform, false);
		RectTransform contentRt = content.AddComponent<RectTransform>();
		contentRt.anchorMin = new Vector2(0, 1);
		contentRt.anchorMax = new Vector2(1, 1);
		contentRt.pivot = new Vector2(0.5f, 1);

		scroll.content = contentRt;
		scroll.viewport = vpRt;

		// Scrollbar verticale (visible seulement si nécessaire)
		if (contentHeight > scrollHeight - 20f)
		{
			GameObject scrollbarGO = new GameObject("Scrollbar Vertical");
			scrollbarGO.transform.SetParent(scrollContainer.transform, false);
			RectTransform scrollbarRt = scrollbarGO.AddComponent<RectTransform>();
			scrollbarRt.anchorMin = new Vector2(1, 0);
			scrollbarRt.anchorMax = new Vector2(1, 1);
			scrollbarRt.pivot = new Vector2(1, 0.5f);
			scrollbarRt.sizeDelta = new Vector2(15, 0);
			scrollbarRt.anchoredPosition = new Vector2(-5, 0);

			Scrollbar scrollbar = scrollbarGO.AddComponent<Scrollbar>();
			scrollbar.direction = Scrollbar.Direction.BottomToTop;

			// Handle de la scrollbar
			GameObject handleArea = new GameObject("Sliding Area");
			handleArea.transform.SetParent(scrollbarGO.transform, false);
			RectTransform handleAreaRt = handleArea.AddComponent<RectTransform>();
			handleAreaRt.anchorMin = Vector2.zero;
			handleAreaRt.anchorMax = Vector2.one;
			handleAreaRt.offsetMin = new Vector2(0, 5);
			handleAreaRt.offsetMax = new Vector2(0, -5);

			GameObject handle = new GameObject("Handle");
			handle.transform.SetParent(handleArea.transform, false);
			RectTransform handleRt = handle.AddComponent<RectTransform>();
			handleRt.sizeDelta = new Vector2(10, 20);
			Image handleImg = handle.AddComponent<Image>();
			handleImg.color = new Color(0.4f, 0.8f, 1f, 0.8f);

			scrollbar.handleRect = handleRt;
			scrollbar.targetGraphic = handleImg;
			scroll.verticalScrollbar = scrollbar;
			scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
		}

		// Créer les items
		float itemY = 0f;
		foreach (var outfit in outfits)
		{
			CreateOutfitItem(content, outfit, itemY);
			itemY -= itemHeight;
		}

		contentRt.sizeDelta = new Vector2(0, Mathf.Abs(itemY));
	}

	private void CreateOutfitItem(GameObject parent, OutfitProposalUI.OutfitPresentation outfit, float yPos)
	{
		float price = GetPriceForCategory(outfit.category);

		// Item arrondi avec anchors corrects pour scroll content
		GameObject itemGO = new GameObject($"OutfitItem_{outfit.dayNumber}");
		itemGO.transform.SetParent(parent.transform, false);
		
		RectTransform itemRt = itemGO.AddComponent<RectTransform>();
		itemRt.anchorMin = new Vector2(0.5f, 1f); // Ancré en haut
		itemRt.anchorMax = new Vector2(0.5f, 1f);
		itemRt.pivot = new Vector2(0.5f, 1f);
		itemRt.sizeDelta = new Vector2(610, 38);
		itemRt.anchoredPosition = new Vector2(0, yPos);
		
		Image itemBg = itemGO.AddComponent<Image>();
		itemBg.sprite = UIHelper.GetRoundedSprite();
		itemBg.type = Image.Type.Sliced;
		itemBg.color = new Color(0.12f, 0.12f, 0.12f, 0.8f);

		// Texte de la tenue
		string itemText = $"{GetCategoryIcon(outfit.category)} Jour {outfit.dayNumber} - {outfit.category} • {outfit.selectedMaterial}";
		GameObject textGO = new GameObject("ItemText");
		textGO.transform.SetParent(itemGO.transform, false);
		Text text = textGO.AddComponent<Text>();
		text.text = itemText;
		text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
		text.fontSize = 14;
		text.fontStyle = FontStyle.Normal;
		text.color = Color.white;
		text.alignment = TextAnchor.MiddleLeft;
		
		RectTransform textRt = textGO.GetComponent<RectTransform>();
		textRt.anchorMin = new Vector2(0f, 0.5f);
		textRt.anchorMax = new Vector2(0f, 0.5f);
		textRt.pivot = new Vector2(0f, 0.5f);
		textRt.sizeDelta = new Vector2(420, 38);
		textRt.anchoredPosition = new Vector2(15, 0);

		// Prix
		GameObject priceGO = new GameObject("ItemPrice");
		priceGO.transform.SetParent(itemGO.transform, false);
		Text priceText = priceGO.AddComponent<Text>();
		priceText.text = $"{price:F2} €";
		priceText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
		priceText.fontSize = 15;
		priceText.fontStyle = FontStyle.Bold;
		priceText.color = new Color(0.6f, 1f, 0.6f, 1f);
		priceText.alignment = TextAnchor.MiddleRight;
		
		RectTransform priceRt = priceGO.GetComponent<RectTransform>();
		priceRt.anchorMin = new Vector2(1f, 0.5f);
		priceRt.anchorMax = new Vector2(1f, 0.5f);
		priceRt.pivot = new Vector2(1f, 0.5f);
		priceRt.sizeDelta = new Vector2(100, 38);
		priceRt.anchoredPosition = new Vector2(-15, 0);
	}

	private void CreateBottomButtons(GameObject parent, float yPos)
	{
		// Bouton Payer (centré)
		UIHelper.CreateRoundedButton(parent, "💳 PAYER",
			new Vector2(220, 55), new Vector2(0, yPos),
			new Color(0.2f, 0.8f, 0.4f, 1f),
			() => {
				// Ne pas sauvegarder ici, ThankYouUI le fera après saisie nom/adresse
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
			// Cacher le panel principal et le bouton retour
			mainPanel.SetActive(false);
			if (backButton != null) backButton.SetActive(false);
			
			// Ajouter un badge de nombre sur le caddie
			UpdateCartBadge();
		}
		else
		{
			// Restaurer le panel et le bouton retour
			mainPanel.SetActive(true);
			if (backButton != null) backButton.SetActive(true);
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

