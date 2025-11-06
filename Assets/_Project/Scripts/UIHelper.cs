using UnityEngine;
using UnityEngine.UI;
using System;

namespace Mode3D.Destinations
{
	/// <summary>
	/// Utilitaires pour créer une UI moderne avec bords arrondis
	/// </summary>
	public static class UIHelper
	{
		private static Sprite roundedSprite;

		/// <summary>
		/// Obtient ou crée un sprite arrondi pour l'UI
		/// </summary>
		public static Sprite GetRoundedSprite()
		{
			if (roundedSprite == null)
			{
				// Créer un sprite arrondi personnalisé
				Texture2D tex = new Texture2D(64, 64);
				Color[] pixels = new Color[64 * 64];
				
				// Créer une forme arrondie
				for (int y = 0; y < 64; y++)
				{
					for (int x = 0; x < 64; x++)
					{
						float dx = (x - 31.5f) / 32f;
						float dy = (y - 31.5f) / 32f;
						
						// Distance du centre (0-1)
						float dist = Mathf.Sqrt(dx * dx + dy * dy);
						
						// Arrondi aux coins
						float cornerRadius = 0.3f; // Rayon d'arrondi
						float alpha = 1f;
						
						if (Mathf.Abs(dx) > (1f - cornerRadius) && Mathf.Abs(dy) > (1f - cornerRadius))
						{
							float cdx = Mathf.Abs(dx) - (1f - cornerRadius);
							float cdy = Mathf.Abs(dy) - (1f - cornerRadius);
							float cornerDist = Mathf.Sqrt(cdx * cdx + cdy * cdy);
							alpha = cornerDist < cornerRadius ? 1f : 0f;
						}
						
						pixels[y * 64 + x] = new Color(1f, 1f, 1f, alpha);
					}
				}
				
				tex.SetPixels(pixels);
				tex.Apply();
				
				// Créer le sprite avec border pour slicing
				roundedSprite = Sprite.Create(
					tex, 
					new Rect(0, 0, 64, 64), 
					new Vector2(0.5f, 0.5f), 
					100, 
					0, 
					SpriteMeshType.FullRect, 
					new Vector4(16, 16, 16, 16) // Border pour slicing
				);
			}
			return roundedSprite;
		}

		/// <summary>
		/// Crée un panneau arrondi avec marges
		/// </summary>
		public static GameObject CreateRoundedPanel(GameObject parent, Vector2 size, Vector2 position, Color bgColor, float padding = 10f)
		{
			GameObject panel = new GameObject("RoundedPanel");
			panel.transform.SetParent(parent.transform, false);

			RectTransform rt = panel.AddComponent<RectTransform>();
			rt.anchorMin = new Vector2(0.5f, 0.5f);
			rt.anchorMax = new Vector2(0.5f, 0.5f);
			rt.sizeDelta = size;
			rt.anchoredPosition = position;

			Image img = panel.AddComponent<Image>();
			img.sprite = GetRoundedSprite();
			img.type = Image.Type.Sliced;
			img.color = bgColor;

			return panel;
		}

		/// <summary>
		/// Crée un bouton moderne arrondi
		/// </summary>
		public static GameObject CreateRoundedButton(GameObject parent, string text, Vector2 size, Vector2 position, Color btnColor, Action onClick)
		{
			GameObject btnGO = new GameObject("Button_" + text);
			btnGO.transform.SetParent(parent.transform, false);

			RectTransform btnRt = btnGO.AddComponent<RectTransform>();
			btnRt.anchorMin = new Vector2(0.5f, 0.5f);
			btnRt.anchorMax = new Vector2(0.5f, 0.5f);
			btnRt.sizeDelta = size;
			btnRt.anchoredPosition = position;

			Image btnImg = btnGO.AddComponent<Image>();
			btnImg.sprite = GetRoundedSprite();
			btnImg.type = Image.Type.Sliced;
			btnImg.color = btnColor;

			Button btn = btnGO.AddComponent<Button>();
			btn.onClick.AddListener(() => onClick());

			var colors = btn.colors;
			colors.normalColor = btnColor;
			colors.highlightedColor = new Color(
				Mathf.Min(btnColor.r + 0.1f, 1f),
				Mathf.Min(btnColor.g + 0.1f, 1f),
				Mathf.Min(btnColor.b + 0.1f, 1f),
				1f
			);
			colors.pressedColor = new Color(
				Mathf.Max(btnColor.r - 0.1f, 0f),
				Mathf.Max(btnColor.g - 0.1f, 0f),
				Mathf.Max(btnColor.b - 0.1f, 0f),
				1f
			);
			btn.colors = colors;

			// Texte du bouton
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
		/// Crée un bouton retour arrondi standard
		/// </summary>
		public static GameObject CreateBackButton(GameObject parent, Vector2 position, Action onClick)
		{
			return CreateRoundedButton(
				parent,
				"← Retour",
				new Vector2(120, 40),
				position,
				new Color(0.3f, 0.3f, 0.3f, 0.9f),
				onClick
			);
		}

		/// <summary>
		/// Crée un texte
		/// </summary>
		public static GameObject CreateText(GameObject parent, string content, Vector2 size, Vector2 position, int fontSize, FontStyle style, Color color, TextAnchor alignment = TextAnchor.MiddleCenter)
		{
			GameObject textGO = new GameObject("Text");
			textGO.transform.SetParent(parent.transform, false);

			Text text = textGO.AddComponent<Text>();
			text.text = content;
			text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
			text.fontSize = fontSize;
			text.fontStyle = style;
			text.alignment = alignment;
			text.color = color;

			RectTransform rt = textGO.GetComponent<RectTransform>();
			rt.anchorMin = new Vector2(0.5f, 0.5f);
			rt.anchorMax = new Vector2(0.5f, 0.5f);
			rt.sizeDelta = size;
			rt.anchoredPosition = position;

			return textGO;
		}
	}
}

