using System;
using UnityEngine;
using UnityEngine.UI;

namespace Mode3D.Destinations
{
	/// <summary>
	/// Panneau de détails d'une commande (s'affiche à droite)
	/// </summary>
	public class OrderDetailsUI : MonoBehaviour
	{
		private GameObject panel;
		private Action onClose;

		public void Show(OrderManager.OrderData order, GameObject canvasParent, Action onCloseCallback)
		{
			onClose = onCloseCallback;
			CreateDetailsPanel(order, canvasParent);
		}

		private void CreateDetailsPanel(OrderManager.OrderData order, GameObject canvasParent)
		{
			// Panel détails à droite
			panel = UIHelper.CreateRoundedPanel(
				canvasParent,
				new Vector2(600, 700),
				new Vector2(450, 0), // À droite
				new Color(0.05f, 0.05f, 0.05f, 0.95f),
				25f
			);

		// === TOUTES LES POSITIONS SONT FIXES POUR ÉVITER CHEVAUCHEMENT ===
		
		// Bouton fermer
		UIHelper.CreateRoundedButton(panel, "✕",
			new Vector2(50, 50), new Vector2(270, 315),
			new Color(0.8f, 0.2f, 0.2f, 1f),
			() => {
				Destroy(panel);
				if (onClose != null) onClose();
			});

		// Titre
		UIHelper.CreateText(panel, "Détails de la commande",
			new Vector2(550, 50), new Vector2(0, 315),
			24, FontStyle.Bold, new Color(0.2f, 0.8f, 1f, 1f));

		// Numéro de commande cliquable avec symbole camion
		UIHelper.CreateRoundedButton(panel, $"🚚 {order.orderNumber}",
			new Vector2(280, 40), new Vector2(0, 250),
			new Color(0.2f, 0.6f, 1f, 1f),
			() => { Debug.Log($"Commande {order.orderNumber} cliquée"); });

		// Label Nom
		UIHelper.CreateText(panel, "Nom complet :",
			new Vector2(540, 25), new Vector2(-5, 195),
			14, FontStyle.Bold, new Color(0.9f, 0.9f, 0.9f, 1f), TextAnchor.MiddleLeft);

		// Champ Nom modifiable
		InputField nameInput = CreateInputField(panel, order.customerName, new Vector2(0, 165));

		// Label Adresse
		UIHelper.CreateText(panel, "Adresse de livraison :",
			new Vector2(540, 25), new Vector2(-5, 120),
			14, FontStyle.Bold, new Color(0.9f, 0.9f, 0.9f, 1f), TextAnchor.MiddleLeft);

		// Champ Adresse modifiable
		InputField addressInput = CreateInputField(panel, order.deliveryAddress, new Vector2(0, 90));

		// Bouton sauvegarder les modifications
		UIHelper.CreateRoundedButton(panel, "💾 Sauvegarder",
			new Vector2(200, 40), new Vector2(0, 40),
			new Color(0.2f, 0.7f, 0.9f, 1f),
			() => {
				// TODO: Mettre à jour la commande dans PlayerPrefs
				Debug.Log($"Nom: {nameInput.text}, Adresse: {addressInput.text}");
			});

		// Titre liste - POSITION FIXE
		UIHelper.CreateText(panel, "Liste des vêtements:",
			new Vector2(540, 25), new Vector2(-5, -10),
			14, FontStyle.Bold, new Color(0.9f, 0.9f, 0.9f, 1f), TextAnchor.MiddleLeft);

		// Liste des vêtements avec hauteur réduite - POSITION FIXE
		float itemHeight = 45f;
		float maxScrollHeight = 140f; // Réduit pour bien tenir dans le panel
		float contentHeight = order.items.Count * itemHeight;
		float scrollHeight = Mathf.Min(contentHeight + 20f, maxScrollHeight);

		float scrollYPos = -90f; // Position fixe bien en dessous
		
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

			// Viewport
			GameObject viewport = new GameObject("Viewport");
			viewport.transform.SetParent(scrollContainer.transform, false);
			RectTransform vpRt = viewport.AddComponent<RectTransform>();
			vpRt.anchorMin = Vector2.zero;
			vpRt.anchorMax = Vector2.one;
			vpRt.offsetMin = new Vector2(10, 10);
			vpRt.offsetMax = new Vector2(-30, -10);
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

			// Créer les items
			float itemY = 0f;
			foreach (var item in order.items)
			{
				CreateOrderItem(content, item, itemY);
				itemY -= itemHeight;
			}

		contentRt.sizeDelta = new Vector2(0, Mathf.Abs(itemY));

		// Prix total - POSITION FIXE EN BAS
		float priceYPos = -230f;
		GameObject priceBox = UIHelper.CreateRoundedPanel(
			panel,
			new Vector2(550, 60),
			new Vector2(0, priceYPos),
			new Color(0.1f, 0.6f, 0.3f, 0.5f),
			15f
		);

			UIHelper.CreateText(priceBox, $"💰 TOTAL : {order.totalPrice:F2} €",
				new Vector2(520, 60), Vector2.zero,
				22, FontStyle.Bold, new Color(1f, 1f, 0.9f, 1f));
		}

		private void CreateOrderItem(GameObject parent, OrderManager.OrderItem item, float yPos)
		{
			GameObject itemGO = new GameObject($"OrderItem_{item.dayNumber}");
			itemGO.transform.SetParent(parent.transform, false);
			
			RectTransform itemRt = itemGO.AddComponent<RectTransform>();
			itemRt.anchorMin = new Vector2(0.5f, 1f);
			itemRt.anchorMax = new Vector2(0.5f, 1f);
			itemRt.pivot = new Vector2(0.5f, 1f);
			itemRt.sizeDelta = new Vector2(510, 38);
			itemRt.anchoredPosition = new Vector2(0, yPos);
			
			Image itemBg = itemGO.AddComponent<Image>();
			itemBg.sprite = UIHelper.GetRoundedSprite();
			itemBg.type = Image.Type.Sliced;
			itemBg.color = new Color(0.12f, 0.12f, 0.12f, 0.8f);

			string categoryIcon = GetCategoryIcon(item.category);
			string itemText = $"{categoryIcon} Jour {item.dayNumber} - {item.category} • {item.material}";
			GameObject textGO = new GameObject("ItemText");
			textGO.transform.SetParent(itemGO.transform, false);
			Text text = textGO.AddComponent<Text>();
			text.text = itemText;
			text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
			text.fontSize = 13;
			text.fontStyle = FontStyle.Normal;
			text.color = Color.white;
			text.alignment = TextAnchor.MiddleLeft;
			
			RectTransform textRt = textGO.GetComponent<RectTransform>();
			textRt.anchorMin = new Vector2(0f, 0.5f);
			textRt.anchorMax = new Vector2(0f, 0.5f);
			textRt.pivot = new Vector2(0f, 0.5f);
			textRt.sizeDelta = new Vector2(350, 38);
			textRt.anchoredPosition = new Vector2(15, 0);

			GameObject priceGO = new GameObject("ItemPrice");
			priceGO.transform.SetParent(itemGO.transform, false);
			Text priceText = priceGO.AddComponent<Text>();
			priceText.text = $"{item.price:F2} €";
			priceText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
			priceText.fontSize = 14;
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

		private InputField CreateInputField(GameObject parent, string initialText, Vector2 position)
		{
			GameObject fieldGO = UIHelper.CreateRoundedPanel(parent,
				new Vector2(550, 40), position,
				new Color(0.15f, 0.15f, 0.15f, 0.9f), 5f);

			InputField inputField = fieldGO.AddComponent<InputField>();
			
			// Texte de l'input
			GameObject textGO = new GameObject("Text");
			textGO.transform.SetParent(fieldGO.transform, false);
			Text text = textGO.AddComponent<Text>();
			text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
			text.fontSize = 15;
			text.color = Color.white;
			text.alignment = TextAnchor.MiddleLeft;
			text.supportRichText = false;
			text.text = initialText; // Texte initial
			
			RectTransform textRt = textGO.GetComponent<RectTransform>();
			textRt.anchorMin = Vector2.zero;
			textRt.anchorMax = Vector2.one;
			textRt.offsetMin = new Vector2(15, 0);
			textRt.offsetMax = new Vector2(-15, 0);
			
			inputField.textComponent = text;
			
			return inputField;
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

