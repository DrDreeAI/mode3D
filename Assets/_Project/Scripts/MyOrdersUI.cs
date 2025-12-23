using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mode3D.Destinations
{
	/// <summary>
	/// Écran "Mes commandes" - affiche la dernière commande passée
	/// </summary>
	public class MyOrdersUI : MonoBehaviour
	{
		private Canvas canvas;
		private Action onClose;
		private GameObject detailsPanel;

		public void Show(Action onCloseCallback)
		{
			onClose = onCloseCallback;
			CreateUI();
		}

	private void CreateUI()
	{
		// Canvas
		GameObject canvasGO = new GameObject("MyOrdersCanvas");
		canvas = canvasGO.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		
		CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
		scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		scaler.referenceResolution = new Vector2(1920, 1080);
		
		canvasGO.AddComponent<GraphicRaycaster>();
		canvas.sortingOrder = 1001; // Au-dessus de l'écran principal

		// Panel principal liste des commandes (à gauche)
		GameObject panel = UIHelper.CreateRoundedPanel(
			canvasGO,
			new Vector2(600, 700),
			new Vector2(-250, 0), // Décalé à gauche
			new Color(0.03f, 0.03f, 0.03f, 0.95f),
			25f
		);

		// Bouton croix pour fermer (en haut à droite)
		GameObject closeBtn = UIHelper.CreateRoundedButton(
			panel,
			"✕",
			new Vector2(50, 50),
			new Vector2(270, 320),
			new Color(0.8f, 0.2f, 0.2f, 1f),
			() => {
				Destroy(canvas.gameObject);
				Destroy(gameObject);
				if (onClose != null) onClose();
			}
		);

	// Titre
	float yPos = 310f;
	UIHelper.CreateText(panel, "📦 Mes commandes",
		new Vector2(550, 50), new Vector2(0, yPos),
		26, FontStyle.Bold, new Color(0.2f, 0.8f, 1f, 1f));
	yPos -= 70f;

	// Charger toutes les commandes
	List<OrderManager.OrderData> orders = OrderManager.LoadAllOrders();

		if (orders == null || orders.Count == 0)
		{
			// Aucune commande
			UIHelper.CreateText(panel, "Aucune commande passée",
				new Vector2(650, 40), new Vector2(0, yPos),
				18, FontStyle.Normal, new Color(0.7f, 0.7f, 0.7f, 1f));
		}
	else
	{
		// Calculer hauteur dynamique pour la liste des commandes
		float orderHeight = 65f;
		float maxScrollHeight = 500f;
		float contentHeight = orders.Count * orderHeight;
		float scrollHeight = Mathf.Min(contentHeight + 20f, maxScrollHeight);

		// Conteneur scroll pour la liste - POSITION FIXE
		float scrollYPos = 50f;
		
		GameObject scrollContainer = UIHelper.CreateRoundedPanel(
			panel,
			new Vector2(550, scrollHeight),
			new Vector2(0, scrollYPos),
			new Color(0.08f, 0.08f, 0.08f, 0.7f),
			10f
		);

			// ScrollRect
			ScrollRect scroll = scrollContainer.AddComponent<ScrollRect>();
			scroll.horizontal = false;
			scroll.vertical = true;
			scroll.scrollSensitivity = 20f;
			scroll.movementType = ScrollRect.MovementType.Clamped;

			// Viewport avec Image pour le Mask
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

		// Créer les items de la liste de commandes
		float itemY = 0f;
		foreach (var order in orders)
		{
			CreateOrderSummaryItem(content, order, itemY);
			itemY -= orderHeight;
		}

		contentRt.sizeDelta = new Vector2(0, Mathf.Abs(itemY));
		}
	}

	private void CreateOrderSummaryItem(GameObject parent, OrderManager.OrderData order, float yPos)
	{
		// Item cliquable avec les infos principales
		GameObject itemGO = new GameObject($"OrderSummary_{order.orderNumber}");
		itemGO.transform.SetParent(parent.transform, false);
		
		RectTransform itemRt = itemGO.AddComponent<RectTransform>();
		itemRt.anchorMin = new Vector2(0.5f, 1f);
		itemRt.anchorMax = new Vector2(0.5f, 1f);
		itemRt.pivot = new Vector2(0.5f, 1f);
		itemRt.sizeDelta = new Vector2(510, 60);
		itemRt.anchoredPosition = new Vector2(0, yPos);
		
		Image itemBg = itemGO.AddComponent<Image>();
		itemBg.sprite = UIHelper.GetRoundedSprite();
		itemBg.type = Image.Type.Sliced;
		itemBg.color = new Color(0.12f, 0.12f, 0.12f, 0.8f);

		// Bouton pour cliquer sur la commande
		Button btn = itemGO.AddComponent<Button>();
		btn.onClick.AddListener(() => ShowOrderDetails(order));
		
		var colors = btn.colors;
		colors.normalColor = new Color(0.12f, 0.12f, 0.12f, 0.8f);
		colors.highlightedColor = new Color(0.2f, 0.5f, 0.8f, 0.8f);
		colors.pressedColor = new Color(0.15f, 0.4f, 0.7f, 0.9f);
		btn.colors = colors;

		// Nom du client (à gauche, en haut)
		GameObject nameGO = new GameObject("CustomerName");
		nameGO.transform.SetParent(itemGO.transform, false);
		Text nameText = nameGO.AddComponent<Text>();
		nameText.text = $"👤 {order.customerName}";
		nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
		nameText.fontSize = 14;
		nameText.fontStyle = FontStyle.Bold;
		nameText.color = Color.white;
		nameText.alignment = TextAnchor.MiddleLeft;

		RectTransform nameRt = nameGO.GetComponent<RectTransform>();
		nameRt.anchorMin = new Vector2(0f, 1f);
		nameRt.anchorMax = new Vector2(0f, 1f);
		nameRt.pivot = new Vector2(0f, 1f);
		nameRt.sizeDelta = new Vector2(250, 25);
		nameRt.anchoredPosition = new Vector2(15, -8);

		// Destination (à gauche, en bas)
		GameObject destGO = new GameObject("Destination");
		destGO.transform.SetParent(itemGO.transform, false);
		Text destText = destGO.AddComponent<Text>();
		destText.text = $"🏙️ {order.destination}";
		destText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
		destText.fontSize = 12;
		destText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
		destText.alignment = TextAnchor.MiddleLeft;

		RectTransform destRt = destGO.GetComponent<RectTransform>();
		destRt.anchorMin = new Vector2(0f, 0f);
		destRt.anchorMax = new Vector2(0f, 0f);
		destRt.pivot = new Vector2(0f, 0f);
		destRt.sizeDelta = new Vector2(250, 20);
		destRt.anchoredPosition = new Vector2(15, 8);

		// Numéro de commande (à droite, en haut)
		GameObject numGO = new GameObject("OrderNumber");
		numGO.transform.SetParent(itemGO.transform, false);
		Text numText = numGO.AddComponent<Text>();
		numText.text = order.orderNumber;
		numText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
		numText.fontSize = 12;
		numText.fontStyle = FontStyle.Bold;
		numText.color = new Color(0.3f, 0.8f, 1f, 1f);
		numText.alignment = TextAnchor.MiddleRight;

		RectTransform numRt = numGO.GetComponent<RectTransform>();
		numRt.anchorMin = new Vector2(1f, 1f);
		numRt.anchorMax = new Vector2(1f, 1f);
		numRt.pivot = new Vector2(1f, 1f);
		numRt.sizeDelta = new Vector2(230, 22);
		numRt.anchoredPosition = new Vector2(-15, -8);

		// Date et heure (à droite, en bas)
		GameObject dateGO = new GameObject("OrderDate");
		dateGO.transform.SetParent(itemGO.transform, false);
		Text dateText = dateGO.AddComponent<Text>();
		dateText.text = $"📅 {order.orderDate}";
		dateText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
		dateText.fontSize = 11;
		dateText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
		dateText.alignment = TextAnchor.MiddleRight;

		RectTransform dateRt = dateGO.GetComponent<RectTransform>();
		dateRt.anchorMin = new Vector2(1f, 0f);
		dateRt.anchorMax = new Vector2(1f, 0f);
		dateRt.pivot = new Vector2(1f, 0f);
		dateRt.sizeDelta = new Vector2(230, 18);
		dateRt.anchoredPosition = new Vector2(-15, 8);
	}

	private void ShowOrderDetails(OrderManager.OrderData order)
	{
		// Fermer le détails précédent s'il existe
		if (detailsPanel != null)
		{
			Destroy(detailsPanel);
		}

		// Créer le panneau de détails
		GameObject detailsGO = new GameObject("OrderDetailsUI");
		detailsGO.transform.SetParent(canvas.transform, false);
		OrderDetailsUI detailsUI = detailsGO.AddComponent<OrderDetailsUI>();
		detailsUI.Show(order, canvas.gameObject, () => {
			detailsPanel = null;
		});
	}

		private string GetCategoryIcon(string category)
		{
			switch (category)
			{
				case "Chill": return "🎽";
				case "Sport": return "🏃";
				case "Business": return "💼";
				default: return "👕";
			}
		}
	}
}

