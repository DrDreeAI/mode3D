using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mode3D.Destinations
{
	/// <summary>
	/// Écran final "Merci pour votre commande"
	/// </summary>
	public class ThankYouUI : MonoBehaviour
	{
		[Header("UI Style")]
		public Color panelColor = new Color(0f, 0f, 0f, 0.8f);
		public Color accentColor = new Color(0.15f, 0.6f, 0.9f, 1f);

	private Canvas canvas;
	private Action onReturnHome;
	private CircularOutfitDisplay circularDisplay; // Garder les tenues affichées
	private string orderNumber;
	private List<OutfitProposalUI.OutfitPresentation> outfits;
	private float totalPrice;
	private string destination;
	private InputField nameInput;
	private InputField addressInput;

	public void Show(Action onReturnHomeCallback, CircularOutfitDisplay existingDisplay = null, 
					 List<OutfitProposalUI.OutfitPresentation> orderOutfits = null, float price = 0f, string dest = "")
	{
		onReturnHome = onReturnHomeCallback;
		circularDisplay = existingDisplay;
		outfits = orderOutfits;
		totalPrice = price;
		destination = dest;
		
		// Générer un numéro de commande
		orderNumber = OrderManager.GenerateOrderNumber();
		
		CreateUI();
	}

		private void CreateUI()
		{
			// Canvas
			GameObject canvasGO = new GameObject("ThankYouCanvas");
			canvas = canvasGO.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			
			CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
			scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			scaler.referenceResolution = new Vector2(1920, 1080);
			
			canvasGO.AddComponent<GraphicRaycaster>();
			canvas.sortingOrder = 1000;

		// Panel moderne arrondi GRAND
		GameObject panel = UIHelper.CreateRoundedPanel(
			canvasGO,
			new Vector2(700, 650), // Plus grand pour les champs
			Vector2.zero,
			new Color(0.03f, 0.03f, 0.03f, 0.95f),
			30f // Très grandes marges
		);

		float yPos = 290f;
		
		// Numéro de commande cliquable
		GameObject orderNumBtn = UIHelper.CreateRoundedButton(panel, $"📋 Numéro: {orderNumber}",
			new Vector2(320, 45), new Vector2(0, yPos),
			new Color(0.2f, 0.6f, 1f, 1f),
			() => { Debug.Log($"Commande {orderNumber} cliquée"); });
		yPos -= 65f;

		// Message principal
		UIHelper.CreateText(panel, "Finalisez votre commande", 
			new Vector2(650, 50), new Vector2(0, yPos), 
			24, FontStyle.Bold, Color.white);
		yPos -= 70f;

		// Label Nom avec même marge que le champ
		UIHelper.CreateText(panel, "Nom complet :", 
			new Vector2(580, 25), new Vector2(-10, yPos), 
			16, FontStyle.Bold, new Color(0.9f, 0.9f, 0.9f, 1f), TextAnchor.MiddleLeft);
		yPos -= 35f;
		
		// Champ Nom
		nameInput = CreateInputField(panel, "Entrez votre nom", new Vector2(0, yPos));
		yPos -= 60f;

		// Label Adresse avec même marge que le champ
		UIHelper.CreateText(panel, "Adresse de livraison :", 
			new Vector2(580, 25), new Vector2(-10, yPos), 
			16, FontStyle.Bold, new Color(0.9f, 0.9f, 0.9f, 1f), TextAnchor.MiddleLeft);
		yPos -= 35f;
		
		// Champ Adresse
		addressInput = CreateInputField(panel, "Entrez votre adresse complète", new Vector2(0, yPos));
		yPos -= 80f;

		// Bouton Valider la commande
		UIHelper.CreateRoundedButton(panel, "✅ VALIDER LA COMMANDE", 
			new Vector2(320, 60), new Vector2(0, yPos),
			new Color(0.2f, 0.8f, 0.4f, 1f),
			() => ValidateOrder());
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

	private InputField CreateInputField(GameObject parent, string placeholder, Vector2 position)
	{
		GameObject fieldGO = UIHelper.CreateRoundedPanel(parent,
			new Vector2(600, 45), position,
			new Color(0.15f, 0.15f, 0.15f, 0.9f), 5f);

		InputField inputField = fieldGO.AddComponent<InputField>();
		
		// Texte de l'input
		GameObject textGO = new GameObject("Text");
		textGO.transform.SetParent(fieldGO.transform, false);
		Text text = textGO.AddComponent<Text>();
		text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
		text.fontSize = 16;
		text.color = Color.white;
		text.alignment = TextAnchor.MiddleLeft;
		text.supportRichText = false;
		
		RectTransform textRt = textGO.GetComponent<RectTransform>();
		textRt.anchorMin = Vector2.zero;
		textRt.anchorMax = Vector2.one;
		textRt.offsetMin = new Vector2(15, 0);
		textRt.offsetMax = new Vector2(-15, 0);
		
		// Placeholder
		GameObject placeholderGO = new GameObject("Placeholder");
		placeholderGO.transform.SetParent(fieldGO.transform, false);
		Text placeholderText = placeholderGO.AddComponent<Text>();
		placeholderText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
		placeholderText.fontSize = 16;
		placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
		placeholderText.alignment = TextAnchor.MiddleLeft;
		placeholderText.text = placeholder;
		
		RectTransform placeholderRt = placeholderGO.GetComponent<RectTransform>();
		placeholderRt.anchorMin = Vector2.zero;
		placeholderRt.anchorMax = Vector2.one;
		placeholderRt.offsetMin = new Vector2(15, 0);
		placeholderRt.offsetMax = new Vector2(-15, 0);
		
		inputField.textComponent = text;
		inputField.placeholder = placeholderText;
		
		return inputField;
	}

	private void ValidateOrder()
	{
		string customerName = nameInput != null ? nameInput.text.Trim() : "";
		string deliveryAddress = addressInput != null ? addressInput.text.Trim() : "";

		if (string.IsNullOrEmpty(customerName) || string.IsNullOrEmpty(deliveryAddress))
		{
			Debug.LogWarning("Veuillez remplir tous les champs !");
			return;
		}

		// Sauvegarder la commande complète
		OrderManager.SaveOrder(orderNumber, customerName, deliveryAddress, outfits, totalPrice, destination);

		// Afficher message de confirmation
		ShowConfirmation();
	}

	private void ShowConfirmation()
	{
		// Cacher les champs de saisie et afficher la confirmation
		foreach (Transform child in canvas.transform)
		{
			Destroy(child.gameObject);
		}

		GameObject panel = UIHelper.CreateRoundedPanel(
			canvas.gameObject,
			new Vector2(700, 500),
			Vector2.zero,
			new Color(0.03f, 0.03f, 0.03f, 0.95f),
			30f
		);

		float yPos = 180f;
		UIHelper.CreateText(panel, "✅", 
			new Vector2(120, 100), new Vector2(0, yPos), 
			80, FontStyle.Normal, new Color(0.3f, 1f, 0.3f, 1f));
		yPos -= 120f;

		UIHelper.CreateText(panel, "Merci pour votre commande !", 
			new Vector2(650, 60), new Vector2(0, yPos), 
			28, FontStyle.Bold, Color.white);
		yPos -= 70f;

		UIHelper.CreateText(panel, $"Commande {orderNumber} confirmée", 
			new Vector2(650, 40), new Vector2(0, yPos), 
			18, FontStyle.Normal, new Color(0.9f, 0.9f, 0.9f, 1f));
		yPos -= 55f;

		UIHelper.CreateText(panel, "Bon voyage ! 🌍✈️", 
			new Vector2(650, 35), new Vector2(0, yPos), 
			17, FontStyle.Normal, new Color(0.8f, 0.8f, 1f, 1f));
		yPos -= 80f;

		UIHelper.CreateRoundedButton(panel, "🏠 Retour à l'accueil", 
			new Vector2(320, 60), new Vector2(0, yPos),
			new Color(0.2f, 0.8f, 0.4f, 1f),
			() => {
				if (circularDisplay != null)
				{
					circularDisplay.ClearAllOutfits();
					Destroy(circularDisplay.gameObject);
				}
				Destroy(canvas.gameObject);
				if (onReturnHome != null) onReturnHome();
			});
	}
}
}

