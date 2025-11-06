using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mode3D.Destinations
{
	/// <summary>
	/// Affiche les mannequins avec vêtements dans la scène 3D
	/// Positionnés entre le tapis et la vitre dans la chambre d'hôtel
	/// </summary>
	public class InSceneOutfitDisplay : MonoBehaviour
	{
		[Header("Positioning")]
		public Vector3 basePosition = new Vector3(0f, 0f, 2.5f); // Entre tapis et vitre
		public float mannequinSpacing = 1.2f; // Espacement entre mannequins
		public float mannequinHeight = 0f; // Hauteur Y
		
		[Header("Character Settings")]
		public string characterFolderName = "DefaultCharacter";
		public GameObject characterPrefab; // Mannequin de base (optionnel)
		
		private List<GameObject> activeMannequins = new List<GameObject>();
		private List<CharacterWearableController> wearableControllers = new List<CharacterWearableController>();

		/// <summary>
		/// Affiche un mannequin à une position donnée avec un outfit
		/// </summary>
		public GameObject ShowMannequin(int index, OutfitType category, string color)
		{
			// Calculer position (aligner horizontalement)
			float xOffset = (index - 1.5f) * mannequinSpacing; // Centre sur 0 pour 3 mannequins
			Vector3 position = new Vector3(xOffset, mannequinHeight, basePosition.z);

			// Créer le mannequin
			GameObject mannequin = CreateBaseMannequin(position, category);
			
			// Ajouter WearableController
			CharacterWearableController controller = mannequin.AddComponent<CharacterWearableController>();
			wearableControllers.Add(controller);
			activeMannequins.Add(mannequin);

			// Créer et appliquer l'outfit CodeFirst
			ApplyCodeFirstOutfit(controller, category, color, index);

			// Rotation lente pour présentation
			MannequinRotator rotator = mannequin.AddComponent<MannequinRotator>();
			rotator.rotationSpeed = 15f;

			// Label au-dessus
			CreateLabel(mannequin, category, color);

			return mannequin;
		}

		private GameObject CreateBaseMannequin(Vector3 position, OutfitType category)
		{
			GameObject mannequin;

			// Essayer de charger un prefab depuis Resources
			if (characterPrefab != null)
			{
				mannequin = Instantiate(characterPrefab, position, Quaternion.identity);
			}
			else
			{
				// Tenter de charger depuis Resources/Characters
				GameObject loadedPrefab = Resources.Load<GameObject>($"Characters/{characterFolderName}/Mannequin");
				
				if (loadedPrefab != null)
				{
					mannequin = Instantiate(loadedPrefab, position, Quaternion.identity);
					Debug.Log($"[InSceneDisplay] Mannequin chargé depuis Resources");
				}
				else
				{
					// Fallback : Créer une silhouette simple
					mannequin = CreateSimpleSilhouette(position, category);
				}
			}

			mannequin.name = $"InSceneMannequin_{category}";
			return mannequin;
		}

		private GameObject CreateSimpleSilhouette(Vector3 position, OutfitType category)
		{
			// Créer une silhouette humanoïde simple avec primitives
			GameObject silhouette = new GameObject($"Silhouette_{category}");
			silhouette.transform.position = position;

			// Corps (capsule)
			GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
			body.name = "Body";
			body.transform.SetParent(silhouette.transform);
			body.transform.localPosition = new Vector3(0, 1f, 0);
			body.transform.localScale = new Vector3(0.5f, 0.75f, 0.3f);

			// Tête (sphère)
			GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			head.name = "Head";
			head.transform.SetParent(silhouette.transform);
			head.transform.localPosition = new Vector3(0, 2f, 0);
			head.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

			// Jambes (capsules)
			GameObject legL = GameObject.CreatePrimitive(PrimitiveType.Capsule);
			legL.name = "LegL";
			legL.transform.SetParent(silhouette.transform);
			legL.transform.localPosition = new Vector3(-0.15f, 0.4f, 0);
			legL.transform.localScale = new Vector3(0.15f, 0.4f, 0.15f);

			GameObject legR = GameObject.CreatePrimitive(PrimitiveType.Capsule);
			legR.name = "LegR";
			legR.transform.SetParent(silhouette.transform);
			legR.transform.localPosition = new Vector3(0.15f, 0.4f, 0);
			legR.transform.localScale = new Vector3(0.15f, 0.4f, 0.15f);

			// Couleur de base selon catégorie
			Color baseColor = GetCategoryBaseColor(category);
			SetSilhouetteColor(silhouette, baseColor);

			return silhouette;
		}

		private void SetSilhouetteColor(GameObject silhouette, Color color)
		{
			Renderer[] renderers = silhouette.GetComponentsInChildren<Renderer>();
			Material mat = new Material(Shader.Find("Standard"));
			mat.color = color;
			
			foreach (Renderer r in renderers)
			{
				r.material = mat;
			}
		}

		private Color GetCategoryBaseColor(OutfitType category)
		{
			switch (category)
			{
				case OutfitType.Chill: return new Color(0.3f, 0.6f, 0.9f, 1f); // Bleu
				case OutfitType.Sport: return new Color(0.9f, 0.3f, 0.3f, 1f); // Rouge
				case OutfitType.Business: return new Color(0.2f, 0.2f, 0.2f, 1f); // Noir
				default: return Color.gray;
			}
		}

		private void ApplyCodeFirstOutfit(CharacterWearableController controller, OutfitType category, string color, int index)
		{
			// Créer un outfit CodeFirst
			WearableOutfit outfit = WearableOutfit.CreateWearableOutfit();
			outfit.Name = $"Outfit_{category}_{color}_{index}";

			// Ajouter les wearables selon la catégorie
			List<Guid> wearableIds = new List<Guid>();

			switch (category)
			{
				case OutfitType.Chill:
					wearableIds.Add(AddWearableToController(controller, $"CasualTop_{color}", WearableType.Top));
					wearableIds.Add(AddWearableToController(controller, $"Jeans_{color}", WearableType.Bottom));
					break;

				case OutfitType.Sport:
					wearableIds.Add(AddWearableToController(controller, $"SportTop_{color}", WearableType.Top));
					wearableIds.Add(AddWearableToController(controller, $"SportBottom_{color}", WearableType.Bottom));
					wearableIds.Add(AddWearableToController(controller, $"Sneakers_{color}", WearableType.Shoes));
					break;

				case OutfitType.Business:
					wearableIds.Add(AddWearableToController(controller, $"BusinessShirt_{color}", WearableType.Top));
					wearableIds.Add(AddWearableToController(controller, $"BusinessPants_{color}", WearableType.Bottom));
					wearableIds.Add(AddWearableToController(controller, $"DressShoes_{color}", WearableType.Shoes));
					wearableIds.Add(AddWearableToController(controller, $"BusinessJacket_{color}", WearableType.Jacket));
					break;
			}

			// Ajouter les IDs à l'outfit
			foreach (Guid id in wearableIds)
			{
				outfit.AddWearable(id);
			}

			// Ajouter et appliquer l'outfit
			controller.AddOutfit(outfit);
			
			try
			{
				controller.ApplyOutfit(outfit.Name);
				Debug.Log($"[InSceneDisplay] Outfit appliqué: {outfit.Name}");
			}
			catch (Exception e)
			{
				Debug.LogWarning($"[InSceneDisplay] Assets manquants pour {outfit.Name}, utilise silhouette. Ajoutez les .fbx dans Resources/Characters/{characterFolderName}/Wearables/");
			}
		}

		private Guid AddWearableToController(CharacterWearableController controller, string wearableName, WearableType type)
		{
			Wearable w = Wearable.CreateWearable();
			w.Name = wearableName;
			w.CharacterFolderName = characterFolderName;
			w.WearableType = type;
			
			controller.AddWearable(w);
			
			return w.Id;
		}

		private void CreateLabel(GameObject mannequin, OutfitType category, string color)
		{
			GameObject labelGO = new GameObject("Label");
			labelGO.transform.SetParent(mannequin.transform);
			labelGO.transform.localPosition = new Vector3(0, 2.5f, 0);

			Canvas canvas = labelGO.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.WorldSpace;
			
			RectTransform canvasRt = labelGO.GetComponent<RectTransform>();
			canvasRt.sizeDelta = new Vector2(200, 80);
			labelGO.transform.localScale = Vector3.one * 0.003f;
			labelGO.transform.rotation = Quaternion.Euler(0, 180f, 0); // Face caméra

			GameObject textGO = new GameObject("Text");
			textGO.transform.SetParent(labelGO.transform, false);
			
			UnityEngine.UI.Text text = textGO.AddComponent<UnityEngine.UI.Text>();
			text.text = $"{GetCategoryIcon(category)}\n{category}\n{color}";
			text.alignment = TextAnchor.MiddleCenter;
			text.fontSize = 28;
			text.fontStyle = FontStyle.Bold;
			text.color = Color.white;
			text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
			
			RectTransform textRt = textGO.GetComponent<RectTransform>();
			textRt.anchorMin = Vector2.zero;
			textRt.anchorMax = Vector2.one;
			textRt.offsetMin = Vector2.zero;
			textRt.offsetMax = Vector2.zero;

			UnityEngine.UI.Outline outline = textGO.AddComponent<UnityEngine.UI.Outline>();
			outline.effectColor = Color.black;
			outline.effectDistance = new Vector2(3, -3);
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

		/// <summary>
		/// Change la couleur de tous les vêtements d'un mannequin
		/// </summary>
		public void ChangeOutfitColor(int mannequinIndex, string newColor, OutfitType category)
		{
			if (mannequinIndex < 0 || mannequinIndex >= activeMannequins.Count) return;
			if (mannequinIndex >= wearableControllers.Count) return;

			GameObject mannequin = activeMannequins[mannequinIndex];
			CharacterWearableController controller = wearableControllers[mannequinIndex];

			// Retirer tous les wearables actuels
			controller.RemoveAllWearables(false);

			// Réappliquer avec nouvelle couleur
			ApplyCodeFirstOutfit(controller, category, newColor, mannequinIndex);

			// Mettre à jour le label
			UpdateLabel(mannequin, category, newColor);

			// Si pas d'assets 3D, changer couleur de la silhouette
			Renderer[] renderers = mannequin.GetComponentsInChildren<Renderer>();
			if (renderers.Length > 0)
			{
				Color color = GetColorFromName(newColor);
				foreach (Renderer r in renderers)
				{
					if (r.material != null)
					{
						r.material.color = color;
					}
				}
			}
		}

		private void UpdateLabel(GameObject mannequin, OutfitType category, string color)
		{
			Transform labelTransform = mannequin.transform.Find("Label/Text");
			if (labelTransform != null)
			{
				UnityEngine.UI.Text text = labelTransform.GetComponent<UnityEngine.UI.Text>();
				if (text != null)
				{
					text.text = $"{GetCategoryIcon(category)}\n{category}\n{color}";
				}
			}
		}

		private Color GetColorFromName(string colorName)
		{
			switch (colorName)
			{
				case "Bleu": return new Color(0.2f, 0.4f, 0.9f, 1f);
				case "Rouge": return new Color(0.9f, 0.2f, 0.2f, 1f);
				case "Vert": return new Color(0.2f, 0.8f, 0.3f, 1f);
				case "Noir": return new Color(0.1f, 0.1f, 0.1f, 1f);
				case "Blanc": return new Color(0.95f, 0.95f, 0.95f, 1f);
				case "Gris": return new Color(0.5f, 0.5f, 0.5f, 1f);
				case "Rose": return new Color(0.9f, 0.4f, 0.7f, 1f);
				default: return Color.gray;
			}
		}

		/// <summary>
		/// Nettoie tous les mannequins de la scène
		/// </summary>
		public void ClearAllMannequins()
		{
			foreach (GameObject mannequin in activeMannequins)
			{
				if (mannequin != null)
				{
					Destroy(mannequin);
				}
			}
			
			activeMannequins.Clear();
			wearableControllers.Clear();
		}

		/// <summary>
		/// Affiche 3 mannequins côte à côte (un par catégorie)
		/// </summary>
		public void ShowAllCategories(string chillColor, string sportColor, string businessColor)
		{
			ClearAllMannequins();

			ShowMannequin(0, OutfitType.Chill, chillColor);
			ShowMannequin(1, OutfitType.Sport, sportColor);
			ShowMannequin(2, OutfitType.Business, businessColor);
		}
	}
}

