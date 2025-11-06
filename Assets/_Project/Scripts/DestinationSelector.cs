using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Mode3D.Destinations
{
	[Serializable]
	public class DestinationItem
	{
		public string displayName;
		public Texture2D imageTexture;
	}

	public class DestinationSelector : MonoBehaviour
	{
		[Header("Destination Data")]
		public List<DestinationItem> destinations = new List<DestinationItem>();
		[Tooltip("If true and the list is empty, tries to load textures from Resources/Destinations/{Paris,NewYork,Londres,Dubai}")]
		public bool autoLoadFromResources = true;

		[Header("Window Placement")]
		public Transform windowAnchor; // Optional: assign a transform roughly centered on the window/glass
		public float placeOffsetTowardsCamera = 0.05f; // Nudge towards camera to avoid z-fighting with the glass
		public float defaultDistanceFromCamera = 3.0f; // Used if no windowAnchor is found

		[Header("UI Style")]
		public Font fallbackFont;
		public Color panelColor = new Color(0f, 0f, 0f, 0.6f);
		public Color textColor = Color.white;

		[Header("Behavior")]
		public bool disableOtherCanvases = true;

		private const string PlayerPrefsKey = "Mode3D_SelectedDestination";
		private Canvas selectorCanvas;
		private Button validateButton;
		private List<Canvas> disabledCanvases = new List<Canvas>();
		private int selectedIndex = -1;
		private List<Button> cityItemButtons = new List<Button>();
		private Sprite uiRoundedSprite;
		private Color itemNormalColor = new Color(1f, 1f, 1f, 0.9f);
		private Color itemSelectedColor = new Color(0.2f, 0.6f, 1f, 0.25f);
	
	// Dropdown state
	private GameObject dropdownList;
	private Text dropdownLabel;
	private bool isDropdownOpen = false;

		// Date range state (for next screen)
		private DateTime? startDate;
		private DateTime? endDate;
		private Dictionary<DateTime, Image> dateCellToImage = new Dictionary<DateTime, Image>();

	private void Start()
	{
		CleanupPreviousCanvases();
		EnsureEventSystemExists();

		if (autoLoadFromResources && destinations.Count == 0)
		{
			TryAutoPopulateDestinations();
		}

		// Try find WindowAnchor by name if not assigned
		if (windowAnchor == null)
		{
			var anchorGO = GameObject.Find("WindowAnchor");
			if (anchorGO != null) windowAnchor = anchorGO.transform;
		}

		// Create UI on start
		CreateSelectorUI();

		// Restore previous selection if available
		if (PlayerPrefs.HasKey(PlayerPrefsKey))
		{
			string prev = PlayerPrefs.GetString(PlayerPrefsKey);
			int idx = destinations.FindIndex(d => string.Equals(d.displayName, prev, StringComparison.OrdinalIgnoreCase));
			if (idx >= 0)
			{
				OnDropdownItemClicked(idx);
			}
		}
	}

	/// <summary>
	/// Méthode publique pour redémarrer l'UI (appelée depuis ThankYouUI)
	/// </summary>
	public void RestartUI()
	{
		CleanupPreviousCanvases();
		selectedIndex = -1;
		startDate = null;
		endDate = null;
		
		// Effacer l'image de ville si elle existe
		GameObject cityView = GameObject.Find("WindowCityView");
		if (cityView != null)
		{
			Destroy(cityView);
		}
		
		CreateSelectorUI();
	}

		private void CreateSelectorUI()
		{
		// Canvas root - ScreenSpaceOverlay pour être fixe devant la caméra
			GameObject existing = GameObject.Find("DestinationSelectorCanvas");
			GameObject canvasGO = existing != null ? existing : new GameObject("DestinationSelectorCanvas");
			selectorCanvas = canvasGO.AddComponent<Canvas>();
			selectorCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
		
		CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
		scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		scaler.referenceResolution = new Vector2(1920, 1080);
		
			canvasGO.AddComponent<GraphicRaycaster>();
			selectorCanvas.sortingOrder = 1000;

			// Supprime tous les autres Canvas pour n'en garder qu'un
		foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
			{
				if (c != selectorCanvas)
				{
					Destroy(c.gameObject);
				}
			}

		// Panel - Taille réduite (plus petit qu'avant)
			GameObject panelGO = new GameObject("Panel");
			panelGO.transform.SetParent(canvasGO.transform, false);
			Image panelImage = panelGO.AddComponent<Image>();
			panelImage.color = panelColor;
			RectTransform panelRt = panelGO.GetComponent<RectTransform>();
			panelRt.anchorMin = new Vector2(0.5f, 0.5f);
			panelRt.anchorMax = new Vector2(0.5f, 0.5f);
		panelRt.sizeDelta = new Vector2(350, 200); // Encore plus réduit
			panelRt.anchoredPosition = Vector2.zero;

		// Title with modern styled background
		GameObject titleBgGO = new GameObject("TitleBackground");
		titleBgGO.transform.SetParent(panelGO.transform, false);
		Image titleBg = titleBgGO.AddComponent<Image>();
		titleBg.color = new Color(0.15f, 0.6f, 0.9f, 1f); // Modern blue gradient
		RectTransform titleBgRt = titleBgGO.GetComponent<RectTransform>();
		titleBgRt.anchorMin = new Vector2(0, 1f);
		titleBgRt.anchorMax = new Vector2(1, 1f);
		titleBgRt.pivot = new Vector2(0.5f, 1f);
		titleBgRt.sizeDelta = new Vector2(0, 60);
		titleBgRt.anchoredPosition = new Vector2(0, 0);

			GameObject titleGO = new GameObject("Title");
		titleGO.transform.SetParent(titleBgGO.transform, false);
			Text titleText = titleGO.AddComponent<Text>();
			titleText.text = "Choisissez votre destination";
			titleText.alignment = TextAnchor.MiddleCenter;
		titleText.color = Color.white;
			titleText.font = GetUIFont();
		titleText.fontSize = 20;
		titleText.fontStyle = FontStyle.Bold;
			RectTransform titleRt = titleGO.GetComponent<RectTransform>();
		titleRt.anchorMin = Vector2.zero;
		titleRt.anchorMax = Vector2.one;
		titleRt.offsetMin = Vector2.zero;
		titleRt.offsetMax = Vector2.zero;

		// Create a simple white sprite (avoiding missing resource errors)
		Texture2D tex = new Texture2D(1, 1);
		tex.SetPixel(0, 0, Color.white);
		tex.Apply();
		uiRoundedSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));

		// Dropdown field (collapsed by default) - taille réduite
		GameObject dropdownFieldGO = new GameObject("DropdownField");
		dropdownFieldGO.transform.SetParent(panelGO.transform, false);
		var fieldBg = dropdownFieldGO.AddComponent<Image>();
		fieldBg.color = Color.white;
		var fieldOutline = dropdownFieldGO.AddComponent<Outline>();
		fieldOutline.effectColor = new Color(0.7f, 0.7f, 0.7f, 1f);
		fieldOutline.effectDistance = new Vector2(1, -1);
		var fieldRt = dropdownFieldGO.GetComponent<RectTransform>();
		fieldRt.anchorMin = new Vector2(0.5f, 0.5f);
		fieldRt.anchorMax = new Vector2(0.5f, 0.5f);
		fieldRt.sizeDelta = new Vector2(320, 45); // Réduit
		fieldRt.anchoredPosition = new Vector2(0, 12);

		// Dropdown label
		GameObject fieldLabelGO = new GameObject("Label");
		fieldLabelGO.transform.SetParent(dropdownFieldGO.transform, false);
		dropdownLabel = fieldLabelGO.AddComponent<Text>();
		dropdownLabel.text = "Saisissez votre ville";
		dropdownLabel.font = GetUIFont();
		dropdownLabel.fontSize = 16;
		dropdownLabel.alignment = TextAnchor.MiddleLeft;
		dropdownLabel.color = new Color(0.5f, 0.5f, 0.5f, 1f); // Gray placeholder
		var fieldLabelRt = fieldLabelGO.GetComponent<RectTransform>();
		fieldLabelRt.anchorMin = Vector2.zero;
		fieldLabelRt.anchorMax = Vector2.one;
		fieldLabelRt.offsetMin = new Vector2(15, 0);
		fieldLabelRt.offsetMax = new Vector2(-40, 0);

		// Dropdown arrow icon (▼)
		GameObject arrowGO = new GameObject("Arrow");
		arrowGO.transform.SetParent(dropdownFieldGO.transform, false);
		var arrow = arrowGO.AddComponent<Text>();
		arrow.text = "▼";
		arrow.font = GetUIFont();
		arrow.fontSize = 16;
		arrow.alignment = TextAnchor.MiddleCenter;
		arrow.color = new Color(0.3f, 0.3f, 0.3f, 1f);
		var arrowRt = arrowGO.GetComponent<RectTransform>();
		arrowRt.anchorMin = new Vector2(1, 0);
		arrowRt.anchorMax = new Vector2(1, 1);
		arrowRt.sizeDelta = new Vector2(30, 0);
		arrowRt.anchoredPosition = new Vector2(-15, 0);

		// Button to toggle dropdown
		var fieldBtn = dropdownFieldGO.AddComponent<Button>();
		fieldBtn.onClick.AddListener(ToggleDropdown);
		var fieldColors = fieldBtn.colors;
		fieldColors.normalColor = Color.white;
		fieldColors.highlightedColor = new Color(0.95f, 0.95f, 0.95f, 1f);
		fieldColors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
		fieldBtn.colors = fieldColors;

		// Dropdown list container (initially hidden) - taille adaptée
		dropdownList = new GameObject("DropdownList");
		dropdownList.transform.SetParent(panelGO.transform, false);
		var listBg = dropdownList.AddComponent<Image>();
		listBg.color = Color.white;
		var listOutline = dropdownList.AddComponent<Outline>();
		listOutline.effectColor = new Color(0.7f, 0.7f, 0.7f, 1f);
		listOutline.effectDistance = new Vector2(1, -1);
		var listRt = dropdownList.GetComponent<RectTransform>();
		listRt.anchorMin = new Vector2(0.5f, 0.5f);
		listRt.anchorMax = new Vector2(0.5f, 0.5f);
		listRt.sizeDelta = new Vector2(320, 0); // Will be sized dynamically - réduit
		listRt.anchoredPosition = new Vector2(0, -50); // Below the field
		dropdownList.SetActive(false);

		// Scroll rect for dropdown list
		GameObject scrollGO = new GameObject("Scroll");
		scrollGO.transform.SetParent(dropdownList.transform, false);
			var scroll = scrollGO.AddComponent<ScrollRect>();
			var scrollRt = scrollGO.GetComponent<RectTransform>();
		scrollRt.anchorMin = Vector2.zero;
		scrollRt.anchorMax = Vector2.one;
		scrollRt.offsetMin = Vector2.zero;
		scrollRt.offsetMax = Vector2.zero;

			GameObject viewportGO = new GameObject("Viewport");
			viewportGO.transform.SetParent(scrollGO.transform, false);
			var mask = viewportGO.AddComponent<Mask>();
			mask.showMaskGraphic = false;
			var vpImg = viewportGO.AddComponent<Image>();
			vpImg.color = new Color(1,1,1,0);
			var vpRt = viewportGO.GetComponent<RectTransform>();
			vpRt.anchorMin = Vector2.zero;
			vpRt.anchorMax = Vector2.one;
		vpRt.offsetMin = new Vector2(5, 5);
		vpRt.offsetMax = new Vector2(-5, -5);

			GameObject contentGO = new GameObject("Content");
			contentGO.transform.SetParent(viewportGO.transform, false);
			var contentRt = contentGO.AddComponent<RectTransform>();
			contentRt.anchorMin = new Vector2(0, 1);
			contentRt.anchorMax = new Vector2(1, 1);
			contentRt.pivot = new Vector2(0.5f, 1);
			contentRt.sizeDelta = new Vector2(0, 0);

			scroll.content = contentRt;
			scroll.viewport = vpRt;
			scroll.horizontal = false;
		scroll.vertical = true;

			cityItemButtons.Clear();
		float itemHeight = 70f;
		float itemSpacing = 2f;
		int maxVisibleItems = 4; // Show max 4 items before scrolling
		
			for (int i = 0; i < destinations.Count; i++)
			{
				var d = destinations[i];
				GameObject item = new GameObject("CityItem_" + i);
				item.transform.SetParent(contentGO.transform, false);
				var itemRt = item.AddComponent<RectTransform>();
				itemRt.anchorMin = new Vector2(0, 1);
				itemRt.anchorMax = new Vector2(1, 1);
				itemRt.pivot = new Vector2(0.5f, 1);
			itemRt.sizeDelta = new Vector2(-10, itemHeight);
			itemRt.anchoredPosition = new Vector2(0, -(i * (itemHeight + itemSpacing)));

				var bg = item.AddComponent<Image>();
			bg.color = Color.white;

				GameObject thumbGO = new GameObject("Thumb");
				thumbGO.transform.SetParent(item.transform, false);
				var thumb = thumbGO.AddComponent<Image>();
				if (d.imageTexture != null)
				{
					thumb.sprite = Sprite.Create(d.imageTexture, new Rect(0,0,d.imageTexture.width,d.imageTexture.height), new Vector2(0.5f,0.5f));
				}
			else
			{
				// Default placeholder color
				thumb.color = new Color(0.85f, 0.85f, 0.85f, 1f);
			}
				var thumbRt = thumbGO.GetComponent<RectTransform>();
				thumbRt.anchorMin = new Vector2(0, 0.5f);
				thumbRt.anchorMax = new Vector2(0, 0.5f);
			thumbRt.sizeDelta = new Vector2(90, 55);
			thumbRt.anchoredPosition = new Vector2(50, 0);

				GameObject labelGO = new GameObject("Label");
				labelGO.transform.SetParent(item.transform, false);
				var label = labelGO.AddComponent<Text>();
				label.text = string.IsNullOrWhiteSpace(d.displayName) ? "(Sans nom)" : d.displayName;
				label.font = GetUIFont();
			label.fontSize = 16;
			label.fontStyle = FontStyle.Normal;
				label.alignment = TextAnchor.MiddleLeft;
			label.color = new Color(0.2f, 0.2f, 0.2f, 1f);
				var labelRt = labelGO.GetComponent<RectTransform>();
				labelRt.anchorMin = new Vector2(0, 0);
				labelRt.anchorMax = new Vector2(1, 1);
			labelRt.offsetMin = new Vector2(110, 0);
			labelRt.offsetMax = new Vector2(-10, 0);

				var btn = item.AddComponent<Button>();
				int captured = i;
			btn.onClick.AddListener(() => OnDropdownItemClicked(captured));
			
			// Add color transition for button
			var colors = btn.colors;
			colors.normalColor = Color.white;
			colors.highlightedColor = new Color(0.85f, 0.92f, 1f, 1f);
			colors.pressedColor = new Color(0.7f, 0.85f, 1f, 1f);
			btn.colors = colors;
			
				cityItemButtons.Add(btn);
			}
		
		// Set content size and dropdown list height
		float totalHeight = destinations.Count * (itemHeight + itemSpacing);
		contentRt.sizeDelta = new Vector2(0, totalHeight);
		
		// Limit dropdown height to max visible items
		float maxDropdownHeight = Mathf.Min(totalHeight + 10, maxVisibleItems * (itemHeight + itemSpacing) + 10);
		listRt.sizeDelta = new Vector2(320, maxDropdownHeight); // Adapté à la nouvelle largeur

		// Validate button - modern style - taille réduite
			GameObject btnGO = new GameObject("ValidateButton");
			btnGO.transform.SetParent(panelGO.transform, false);
			validateButton = btnGO.AddComponent<Button>();
			Image btnImg = btnGO.AddComponent<Image>();
		btnImg.color = new Color(0.15f, 0.6f, 0.9f, 1f); // Match title blue
			RectTransform btnRt = btnGO.GetComponent<RectTransform>();
			btnRt.anchorMin = new Vector2(0.5f, 0);
			btnRt.anchorMax = new Vector2(0.5f, 0);
		btnRt.sizeDelta = new Vector2(200, 40); // Plus petit
			btnRt.anchoredPosition = new Vector2(0, 20);
		
		// Add button color transitions
		var btnColors = validateButton.colors;
		btnColors.normalColor = new Color(0.15f, 0.6f, 0.9f, 1f);
		btnColors.highlightedColor = new Color(0.2f, 0.7f, 1f, 1f);
		btnColors.pressedColor = new Color(0.1f, 0.5f, 0.8f, 1f);
		btnColors.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.5f);
		validateButton.colors = btnColors;

			GameObject btnTextGO = new GameObject("Text");
			btnTextGO.transform.SetParent(btnGO.transform, false);
			Text btnText = btnTextGO.AddComponent<Text>();
		btnText.text = "VALIDER";
		btnText.fontSize = 18;
		btnText.fontStyle = FontStyle.Bold;
			btnText.alignment = TextAnchor.MiddleCenter;
			btnText.color = Color.white;
			btnText.font = GetUIFont();
			RectTransform btnTextRt = btnTextGO.GetComponent<RectTransform>();
			btnTextRt.anchorMin = new Vector2(0, 0);
			btnTextRt.anchorMax = new Vector2(1, 1);
			btnTextRt.sizeDelta = Vector2.zero;

			validateButton.onClick.AddListener(OnValidateClicked);
			validateButton.interactable = false;
		}

		private Font GetUIFont()
		{
			if (fallbackFont != null) return fallbackFont;
			// Unity 6: Arial.ttf n'est plus intégré. Essayer LegacyRuntime.ttf, sinon police système
			var legacy = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
			if (legacy != null) return legacy;
			string[] candidates = new string[] { "Arial", "Helvetica", "Verdana", "Tahoma" };
			foreach (var name in candidates)
			{
				try
				{
					var f = Font.CreateDynamicFontFromOSFont(name, 16);
					if (f != null) return f;
				}
				catch { }
			}
			// Dernier recours: première police du système
			var osFonts = Font.GetOSInstalledFontNames();
			if (osFonts != null && osFonts.Length > 0)
			{
				return Font.CreateDynamicFontFromOSFont(osFonts[0], 16);
			}
			return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
		}

	private void ToggleDropdown()
	{
		isDropdownOpen = !isDropdownOpen;
		if (dropdownList != null)
		{
			dropdownList.SetActive(isDropdownOpen);
		}
	}

	private void OnDropdownItemClicked(int index)
		{
			selectedIndex = index;
		
		// Update dropdown label with selected city
		if (dropdownLabel != null && index >= 0 && index < destinations.Count)
			{
			dropdownLabel.text = destinations[index].displayName;
			dropdownLabel.color = new Color(0.2f, 0.2f, 0.2f, 1f); // Dark text when selected
		}
		
		// Close dropdown
		isDropdownOpen = false;
		if (dropdownList != null)
		{
			dropdownList.SetActive(false);
			}
		
		// Enable validate button
			if (validateButton != null) validateButton.interactable = selectedIndex >= 0;
		}

		private void OnValidateClicked()
		{
			int selected = selectedIndex >= 0 ? selectedIndex : 0;
			if (selected < 0 || selected >= destinations.Count) return;
			DestinationItem item = destinations[selected];
			if (item == null) return;

			// Save selection
			PlayerPrefs.SetString(PlayerPrefsKey, item.displayName);
			PlayerPrefs.Save();

			// Apply as window view
			ApplyDestinationToWindow(item);

			// Hide and destroy UI to avoid duplicates on next loads
			if (selectorCanvas != null)
			{
				Destroy(selectorCanvas.gameObject);
			}

			// Go to date selection screen
			CreateDateSelectionUI();
		}

		private void ApplyDestinationToWindow(DestinationItem item)
		{
			if (item.imageTexture == null)
			{
				Debug.LogWarning("Destination imageTexture is not set.");
				return;
			}

			Camera cam = Camera.main;
			if (cam == null)
			{
				Debug.LogWarning("No main camera found.");
				return;
			}

			Vector3 targetPosition;
			Quaternion targetRotation;

			if (windowAnchor != null)
			{
				// Place just in front of the window anchor along the camera->window axis
				Vector3 toCam = (cam.transform.position - windowAnchor.position).normalized;
				targetPosition = windowAnchor.position + toCam * placeOffsetTowardsCamera;
				targetRotation = Quaternion.LookRotation(targetPosition - cam.transform.position, Vector3.up);
			}
			else
			{
				// Fallback: place a quad in front of the camera at a default distance
				targetPosition = cam.transform.position + cam.transform.forward * defaultDistanceFromCamera;
				targetRotation = Quaternion.LookRotation(targetPosition - cam.transform.position, Vector3.up);
			}

			GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
			quad.name = "WindowCityView";
			quad.transform.position = targetPosition;
			quad.transform.rotation = targetRotation;
			quad.transform.localScale = ComputeQuadScaleForCamera(cam, item.imageTexture);
			if (windowAnchor != null)
			{
				quad.transform.SetParent(windowAnchor, true);
			}

			Material mat = new Material(Shader.Find("Unlit/Texture"));
			mat.mainTexture = item.imageTexture;
			var renderer = quad.GetComponent<MeshRenderer>();
			renderer.sharedMaterial = mat;
		}

		private void EnsureEventSystemExists()
		{
		if (FindFirstObjectByType<EventSystem>() != null) return;
			GameObject es = new GameObject("EventSystem");
			es.AddComponent<EventSystem>();
			es.AddComponent<StandaloneInputModule>();
		}

		private void CleanupPreviousCanvases()
		{
			var existing = GameObject.Find("DestinationSelectorCanvas");
			if (existing != null)
			{
				Destroy(existing);
			}
		}

		private void TryAutoPopulateDestinations()
		{
			// Tries to load common city names from Resources/Destinations
			AddIfFound("Paris");
			AddIfFound("NewYork");
			AddIfFound("Londres");
			AddIfFound("Dubai");
		}

		private void AddIfFound(string name)
		{
			var tex = Resources.Load<Texture2D>("Destinations/" + name);
			if (tex != null)
			{
				destinations.Add(new DestinationItem { displayName = name, imageTexture = tex });
			}
		}

		private void CreateBlankScreen()
		{
			GameObject canvasGO = new GameObject("BlankScreenCanvas");
			var canvas = canvasGO.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvas.sortingOrder = 1000; // Top-level
			canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			canvasGO.AddComponent<GraphicRaycaster>();

			var panel = new GameObject("Background");
			panel.transform.SetParent(canvasGO.transform, false);
			var img = panel.AddComponent<Image>();
			img.color = new Color(0f, 0f, 0f, 0f); // Écran vierge (transparent)
			var rt = panel.GetComponent<RectTransform>();
			rt.anchorMin = Vector2.zero;
			rt.anchorMax = Vector2.one;
			rt.offsetMin = Vector2.zero;
			rt.offsetMax = Vector2.zero;
		}

		private void CreateDateSelectionUI()
		{
			GameObject canvasGO = new GameObject("DateSelectionCanvas");
			var canvas = canvasGO.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		
		CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
		scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		scaler.referenceResolution = new Vector2(1920, 1080);
		
			canvasGO.AddComponent<GraphicRaycaster>();
		canvas.sortingOrder = 1000;

		// Ensure sprite exists
		if (uiRoundedSprite == null)
		{
			Texture2D tex = new Texture2D(1, 1);
			tex.SetPixel(0, 0, Color.white);
			tex.Apply();
			uiRoundedSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
		}

		// Panel - Taille réduite (calendrier)
			GameObject panelGO = new GameObject("Panel");
			panelGO.transform.SetParent(canvasGO.transform, false);
			var panelImg = panelGO.AddComponent<Image>();
			panelImg.color = panelColor;
			var panelRt = panelGO.GetComponent<RectTransform>();
			panelRt.anchorMin = new Vector2(0.5f, 0.5f);
			panelRt.anchorMax = new Vector2(0.5f, 0.5f);
		panelRt.sizeDelta = new Vector2(400, 350); // Plus compact
			panelRt.anchoredPosition = Vector2.zero;

		// Title with modern background
		GameObject titleBgGO = new GameObject("TitleBackground");
		titleBgGO.transform.SetParent(panelGO.transform, false);
		Image titleBg = titleBgGO.AddComponent<Image>();
		titleBg.color = new Color(0.15f, 0.6f, 0.9f, 1f);
		RectTransform titleBgRt = titleBgGO.GetComponent<RectTransform>();
		titleBgRt.anchorMin = new Vector2(0, 1f);
		titleBgRt.anchorMax = new Vector2(1, 1f);
		titleBgRt.pivot = new Vector2(0.5f, 1f);
		titleBgRt.sizeDelta = new Vector2(0, 60);
		titleBgRt.anchoredPosition = new Vector2(0, 0);

			GameObject titleGO = new GameObject("Title");
		titleGO.transform.SetParent(titleBgGO.transform, false);
			var titleText = titleGO.AddComponent<Text>();
			titleText.text = "Sélectionnez vos dates";
			titleText.font = GetUIFont();
		titleText.fontSize = 20;
		titleText.fontStyle = FontStyle.Bold;
			titleText.alignment = TextAnchor.MiddleCenter;
		titleText.color = Color.white;
			var titleRt = titleGO.GetComponent<RectTransform>();
		titleRt.anchorMin = Vector2.zero;
		titleRt.anchorMax = Vector2.one;
		titleRt.offsetMin = Vector2.zero;
		titleRt.offsetMax = Vector2.zero;

		// Calendar grid (current month) - modern styling
			DateTime now = DateTime.Now;
			DateTime first = new DateTime(now.Year, now.Month, 1);
			int daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
			int startOffset = (int)first.DayOfWeek; // Sunday=0

		// Month label
		GameObject monthLabelGO = new GameObject("MonthLabel");
		monthLabelGO.transform.SetParent(panelGO.transform, false);
		var monthLabel = monthLabelGO.AddComponent<Text>();
		monthLabel.text = now.ToString("MMMM yyyy").ToUpper();
		monthLabel.font = GetUIFont();
		monthLabel.fontSize = 16;
		monthLabel.fontStyle = FontStyle.Bold;
		monthLabel.alignment = TextAnchor.MiddleCenter;
		monthLabel.color = new Color(0.2f, 0.2f, 0.2f, 1f);
		var monthLabelRt = monthLabelGO.GetComponent<RectTransform>();
		monthLabelRt.anchorMin = new Vector2(0.5f, 1f);
		monthLabelRt.anchorMax = new Vector2(0.5f, 1f);
		monthLabelRt.pivot = new Vector2(0.5f, 1f);
		monthLabelRt.sizeDelta = new Vector2(520, 30);
		monthLabelRt.anchoredPosition = new Vector2(0, -75);

			GameObject gridGO = new GameObject("CalendarGrid");
			gridGO.transform.SetParent(panelGO.transform, false);
			var gridRt = gridGO.AddComponent<RectTransform>();
			gridRt.anchorMin = new Vector2(0.5f, 0.5f);
			gridRt.anchorMax = new Vector2(0.5f, 0.5f);
		gridRt.sizeDelta = new Vector2(520, 270);
		gridRt.anchoredPosition = new Vector2(0, -20);

			int rows = 6, cols = 7;
		float cellW = gridRt.sizeDelta.x / cols - 6;
		float cellH = gridRt.sizeDelta.y / rows - 6;
			dateCellToImage.Clear();

			for (int i = 0; i < rows * cols; i++)
			{
				int row = i / cols;
				int col = i % cols;
			float x = (-gridRt.sizeDelta.x * 0.5f) + col * (cellW + 6) + cellW * 0.5f + 10;
			float y = (gridRt.sizeDelta.y * 0.5f) - row * (cellH + 6) - cellH * 0.5f - 10;

				GameObject cell = new GameObject("Cell_" + i);
				cell.transform.SetParent(gridGO.transform, false);
				var img = cell.AddComponent<Image>();
			img.color = new Color(1,1,1,0.95f);
				var rt = cell.GetComponent<RectTransform>();
				rt.sizeDelta = new Vector2(cellW, cellH);
				rt.anchoredPosition = new Vector2(x, y);
			
			// Add subtle border
			var cellOutline = cell.AddComponent<Outline>();
			cellOutline.effectColor = new Color(0.85f, 0.85f, 0.85f, 1f);
			cellOutline.effectDistance = new Vector2(1, -1);

				int dayNumber = i - startOffset + 1;
				if (dayNumber >= 1 && dayNumber <= daysInMonth)
				{
					GameObject labelGO2 = new GameObject("Label");
					labelGO2.transform.SetParent(cell.transform, false);
					var label2 = labelGO2.AddComponent<Text>();
					label2.text = dayNumber.ToString();
					label2.font = GetUIFont();
				label2.fontSize = 14;
				label2.fontStyle = FontStyle.Bold;
					label2.alignment = TextAnchor.MiddleCenter;
				label2.color = new Color(0.2f, 0.2f, 0.2f, 1f);
					var lrt = labelGO2.GetComponent<RectTransform>();
					lrt.anchorMin = Vector2.zero;
					lrt.anchorMax = Vector2.one;
					lrt.offsetMin = Vector2.zero;
					lrt.offsetMax = Vector2.zero;

				var cellBtn = cell.AddComponent<Button>();
					DateTime date = new DateTime(now.Year, now.Month, dayNumber);
				cellBtn.onClick.AddListener(() => OnCalendarDateClicked(date));
				
				// Modern button colors
				var cellColors = cellBtn.colors;
				cellColors.normalColor = Color.white;
				cellColors.highlightedColor = new Color(0.85f, 0.9f, 1f, 1f);
				cellColors.pressedColor = new Color(0.7f, 0.85f, 1f, 1f);
				cellBtn.colors = cellColors;
				
					dateCellToImage[date] = img;
				}
				else
				{
				img.color = new Color(0.95f,0.95f,0.95f,0.3f);
				cellOutline.enabled = false;
				}
			}

		// Validate button - modern style
			GameObject btnGO = new GameObject("ValidateButton");
			btnGO.transform.SetParent(panelGO.transform, false);
			var btn = btnGO.AddComponent<Button>();
			var btnImg = btnGO.AddComponent<Image>();
		btnImg.color = new Color(0.15f, 0.6f, 0.9f, 1f);
			var btnRt = btnGO.GetComponent<RectTransform>();
			btnRt.anchorMin = new Vector2(0.5f, 0);
			btnRt.anchorMax = new Vector2(0.5f, 0);
		btnRt.sizeDelta = new Vector2(250, 50);
		btnRt.anchoredPosition = new Vector2(0, 25);
		
		// Button color transitions
		var btnColors = btn.colors;
		btnColors.normalColor = new Color(0.15f, 0.6f, 0.9f, 1f);
		btnColors.highlightedColor = new Color(0.2f, 0.7f, 1f, 1f);
		btnColors.pressedColor = new Color(0.1f, 0.5f, 0.8f, 1f);
		btn.colors = btnColors;

			GameObject textGO = new GameObject("Text");
			textGO.transform.SetParent(btnGO.transform, false);
			var t = textGO.AddComponent<Text>();
		t.text = "VALIDER";
			t.font = GetUIFont();
		t.fontSize = 18;
		t.fontStyle = FontStyle.Bold;
			t.alignment = TextAnchor.MiddleCenter;
			t.color = Color.white;
			var trt = textGO.GetComponent<RectTransform>();
			trt.anchorMin = Vector2.zero;
			trt.anchorMax = Vector2.one;
			trt.offsetMin = Vector2.zero;
			trt.offsetMax = Vector2.zero;

			btn.onClick.AddListener(OnDatesValidateClicked);

		// Bouton retour
		GameObject backBtnGO = new GameObject("BackButton");
		backBtnGO.transform.SetParent(panelGO.transform, false);
		var backBtn = backBtnGO.AddComponent<Button>();
		var backBtnImg = backBtnGO.AddComponent<Image>();
		backBtnImg.color = new Color(0.6f, 0.6f, 0.6f, 1f);
		var backBtnRt = backBtnGO.GetComponent<RectTransform>();
		backBtnRt.anchorMin = new Vector2(0, 1);
		backBtnRt.anchorMax = new Vector2(0, 1);
		backBtnRt.sizeDelta = new Vector2(100, 35);
		backBtnRt.anchoredPosition = new Vector2(60, -20);

		GameObject backTextGO = new GameObject("Text");
		backTextGO.transform.SetParent(backBtnGO.transform, false);
		var backT = backTextGO.AddComponent<Text>();
		backT.text = "← Retour";
		backT.font = GetUIFont();
		backT.fontSize = 14;
		backT.fontStyle = FontStyle.Bold;
		backT.alignment = TextAnchor.MiddleCenter;
		backT.color = Color.white;
		var backTrt = backTextGO.GetComponent<RectTransform>();
		backTrt.anchorMin = Vector2.zero;
		backTrt.anchorMax = Vector2.one;
		backTrt.offsetMin = Vector2.zero;
		backTrt.offsetMax = Vector2.zero;

		backBtn.onClick.AddListener(() => {
			Destroy(canvasGO);
			// Effacer l'image de ville si elle existe
			GameObject cityView = GameObject.Find("WindowCityView");
			if (cityView != null)
			{
				Destroy(cityView);
			}
			CreateSelectorUI();
		});
		}

		private void OnCalendarDateClicked(DateTime date)
		{
			if (startDate == null || (endDate != null && date < startDate))
			{
				startDate = date;
				endDate = null;
			}
			else if (endDate == null)
			{
				if (date < startDate)
				{
					startDate = date;
				}
				else
				{
					endDate = date;
				}
			}
			else
			{
				startDate = date;
				endDate = null;
			}

			UpdateCalendarVisuals();
		}

		private void UpdateCalendarVisuals()
		{
			foreach (var kvp in dateCellToImage)
			{
				var img = kvp.Value;
			img.color = new Color(1,1,1,0.95f);
			}
			if (startDate != null)
			{
				if (endDate == null)
				{
					if (dateCellToImage.TryGetValue(startDate.Value, out var img))
					{
					img.color = new Color(0.15f, 0.6f, 0.9f, 0.6f); // Match theme blue
					}
				}
				else
				{
					DateTime d = startDate.Value;
					while (d <= endDate)
					{
						if (dateCellToImage.TryGetValue(d, out var img))
						{
						img.color = new Color(0.15f, 0.6f, 0.9f, 0.6f); // Match theme blue
						}
						d = d.AddDays(1);
					}
				}
			}
		}

		private void OnDatesValidateClicked()
		{
			if (startDate == null) return;
			if (endDate == null) endDate = startDate;
			PlayerPrefs.SetString("Mode3D_StartDate", startDate.Value.ToString("yyyy-MM-dd"));
			PlayerPrefs.SetString("Mode3D_EndDate", endDate.Value.ToString("yyyy-MM-dd"));
			PlayerPrefs.Save();

		// Fermer l'écran de dates
		GameObject dateCanvas = GameObject.Find("DateSelectionCanvas");
		if (dateCanvas != null) Destroy(dateCanvas);

		// Initialiser OutfitSelection JOUR PAR JOUR
		GameObject outfitGO = new GameObject("OutfitSelectionManager");
		OutfitSelection outfitSelection = outfitGO.AddComponent<OutfitSelection>();
		
		string destName = selectedIndex >= 0 && selectedIndex < destinations.Count ? 
			destinations[selectedIndex].displayName : "Destination";
		
		outfitSelection.InitializeDays(startDate.Value, endDate.Value, destName);

		// Afficher l'écran de sélection des tenues JOUR PAR JOUR
		GameObject outfitUIGO = new GameObject("OutfitSelectionUI");
		OutfitSelectionUI outfitUI = outfitUIGO.AddComponent<OutfitSelectionUI>();
		outfitUI.ShowOutfitSelection(
			onCompleteCallback: () => ShowOutfitProposals(),
			onBackCallback: () => { CreateDateSelectionUI(); Destroy(outfitUIGO); }
		);
	}

	private void ShowOutfitProposals()
	{
		// Aller directement aux propositions de tenues
		GameObject proposalGO = new GameObject("OutfitProposalUI");
		OutfitProposalUI proposalUI = proposalGO.AddComponent<OutfitProposalUI>();
		proposalUI.ShowProposals(
			onCompleteCallback: null, // Géré dans OutfitProposalUI
			onBackCallback: () => {
				// Retour à la sélection des tenues
				GameObject outfitUIGO = new GameObject("OutfitSelectionUI");
				OutfitSelectionUI outfitUI = outfitUIGO.AddComponent<OutfitSelectionUI>();
				outfitUI.ShowOutfitSelection(
					onCompleteCallback: () => ShowOutfitProposals(),
					onBackCallback: () => { CreateDateSelectionUI(); Destroy(outfitUIGO); }
				);
				Destroy(proposalGO);
			}
		);
	}

		private Vector3 ComputeQuadScaleForCamera(Camera cam, Texture2D texture)
		{
			// Scale the quad very large on the camera axis so it fills the view
			float distance = Vector3.Distance(cam.transform.position, transform.position);
			if (windowAnchor != null)
			{
				distance = Vector3.Distance(cam.transform.position, windowAnchor.position);
			}
			else
			{
				distance = defaultDistanceFromCamera;
			}

			// Use a large factor to make it "très grand" (e.g., 1.8x viewport height)
			float baseHeightAtDist = 2f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
			float height = baseHeightAtDist * 1.8f;
			float width = height * cam.aspect;
			if (texture != null && texture.height > 0)
			{
				float aspect = (float)texture.width / (float)texture.height;
				width = height * aspect;
			}
			return new Vector3(width, height, 1f);
		}
	}
}
