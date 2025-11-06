using System;
using UnityEngine;
using UnityEngine.UI;

namespace Mode3D.Destinations
{
	/// <summary>
	/// Récapitulatif du voyage avec les 3 tenues par activité
	/// </summary>
	public class ActivityRecapUI : MonoBehaviour
	{
		[Header("UI Style")]
		public Color panelColor = new Color(0f, 0f, 0f, 0.75f);
		public Color accentColor = new Color(0.15f, 0.6f, 0.9f, 1f);

		private Canvas canvas;
		private Action onBack;

		public void ShowRecap(Action onBackCallback)
		{
			onBack = onBackCallback;
			CreateRecapUI();
		}

		private void CreateRecapUI()
		{
			// Canvas
			GameObject canvasGO = new GameObject("ActivityRecapCanvas");
			canvas = canvasGO.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			
			CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
			scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			scaler.referenceResolution = new Vector2(1920, 1080);
			
			canvasGO.AddComponent<GraphicRaycaster>();
			canvas.sortingOrder = 1000;

			// Panel principal
			GameObject panel = new GameObject("RecapPanel");
			panel.transform.SetParent(canvasGO.transform, false);
			Image panelBg = panel.AddComponent<Image>();
			panelBg.color = panelColor;
			RectTransform panelRt = panel.GetComponent<RectTransform>();
			panelRt.anchorMin = new Vector2(0.5f, 0.5f);
			panelRt.anchorMax = new Vector2(0.5f, 0.5f);
			panelRt.sizeDelta = new Vector2(500, 450);
			panelRt.anchoredPosition = Vector2.zero;

			// Titre background
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

			CreateText(titleBg, "📋 Récapitulatif de votre voyage", 
				Vector2.zero, new Vector2(500, 55), 20, FontStyle.Bold, Color.white);

			float yPos = 170f;

			// Destination et dates
			if (ActivityOutfitManager.Instance != null)
			{
				string destination = ActivityOutfitManager.Instance.destination;
				DateTime start = ActivityOutfitManager.Instance.startDate;
				DateTime end = ActivityOutfitManager.Instance.endDate;
				int nbJours = (end - start).Days + 1;

				CreateText(panel, $"🏙️ {destination}", 
					new Vector2(0, yPos), new Vector2(480, 30), 18, FontStyle.Bold, new Color(0.8f, 1f, 0.8f, 1f));
				yPos -= 35f;

				CreateText(panel, $"📅 Du {start:dd/MM/yyyy} au {end:dd/MM/yyyy} ({nbJours} jours)", 
					new Vector2(0, yPos), new Vector2(480, 25), 14, FontStyle.Normal, Color.white);
				yPos -= 50f;

				// Section tenues
				CreateText(panel, "Vos tenues sélectionnées:", 
					new Vector2(0, yPos), new Vector2(480, 30), 16, FontStyle.Bold, Color.white);
				yPos -= 45f;

				// Afficher les 3 activités
				foreach (var activityOutfit in ActivityOutfitManager.Instance.activityOutfits)
				{
					CreateActivityOutfitDisplay(panel, activityOutfit, ref yPos);
					yPos -= 15f;
				}
			}

			// Boutons
			CreateButton(panel, "← Retour", 
				new Vector2(-200, 200), new Vector2(90, 35), 
				() => { Destroy(canvas.gameObject); if (onBack != null) onBack(); });

			CreateButton(panel, "✓ VALIDER LE VOYAGE", 
				new Vector2(0, -200), new Vector2(280, 55), 
				() => {
					Debug.Log("=== VOYAGE VALIDÉ ===");
					if (ActivityOutfitManager.Instance != null)
					{
						Debug.Log($"Destination: {ActivityOutfitManager.Instance.destination}");
						Debug.Log($"Dates: {ActivityOutfitManager.Instance.startDate:dd/MM} - {ActivityOutfitManager.Instance.endDate:dd/MM}");
						foreach (var ao in ActivityOutfitManager.Instance.activityOutfits)
						{
							Debug.Log($"  {ao.activity}: {ao.outfitName} ({ao.colorVariant})");
						}
					}
					Destroy(canvas.gameObject);
				});
		}

		private void CreateActivityOutfitDisplay(GameObject parent, ActivityOutfit outfit, ref float yPos)
		{
			GameObject itemGO = new GameObject($"Activity_{outfit.activity}");
			itemGO.transform.SetParent(parent.transform, false);
			
			RectTransform itemRt = itemGO.AddComponent<RectTransform>();
			itemRt.anchorMin = new Vector2(0.5f, 0.5f);
			itemRt.anchorMax = new Vector2(0.5f, 0.5f);
			itemRt.sizeDelta = new Vector2(460, 50);
			itemRt.anchoredPosition = new Vector2(0, yPos);
			
			Image itemBg = itemGO.AddComponent<Image>();
			itemBg.color = new Color(0.15f, 0.6f, 0.9f, 0.25f);

			// Texte
			string text = $"{GetActivityIcon(outfit.activity)} {outfit.activity}:  {outfit.outfitName} - {outfit.colorVariant}";
			CreateText(itemGO, text, 
				Vector2.zero, new Vector2(450, 50), 15, FontStyle.Normal, Color.white);
			
			yPos -= 55f;
		}

		private string GetActivityIcon(OutfitType activity)
		{
			switch (activity)
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
			text.alignment = TextAnchor.MiddleCenter;
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
			btnText.fontSize = 14;
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
	}
}

