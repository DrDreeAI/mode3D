using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mode3D.Destinations
{
	/// <summary>
	/// Gère les tenues par activité (1 tenue 3D par activité)
	/// Activités: Chill, Sport, Business
	/// </summary>
	[Serializable]
	public class ActivityOutfit
	{
		public OutfitType activity; // L'activité (Chill, Sport, Business)
		public string outfitName; // Nom de la tenue choisie
		public string colorVariant = "Default"; // Variante de couleur
		public GameObject mannequinInstance; // Instance du mannequin 3D
	}

	public class ActivityOutfitManager : MonoBehaviour
	{
		public static ActivityOutfitManager Instance { get; private set; }

		[Header("Trip Information")]
		public string destination;
		public DateTime startDate;
		public DateTime endDate;

		[Header("Activity Outfits (1 per activity)")]
		public List<ActivityOutfit> activityOutfits = new List<ActivityOutfit>();

		[Header("Mannequin Settings")]
		public string characterFolderName = "DefaultCharacter";
		public Vector3 mannequinPosition = new Vector3(-2f, 0f, 3f);
		public float mannequinRotationSpeed = 20f;

		private void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
				DontDestroyOnLoad(gameObject);
			}
			else
			{
				Destroy(gameObject);
			}
		}

		public void Initialize(string dest, DateTime start, DateTime end)
		{
			destination = dest;
			startDate = start;
			endDate = end;

			// Initialiser 3 activités avec tenues par défaut
			activityOutfits.Clear();
			activityOutfits.Add(new ActivityOutfit { activity = OutfitType.Chill, outfitName = "CasualTop" });
			activityOutfits.Add(new ActivityOutfit { activity = OutfitType.Sport, outfitName = "SportTop" });
			activityOutfits.Add(new ActivityOutfit { activity = OutfitType.Business, outfitName = "BusinessTop" });
		}

		/// <summary>
		/// Obtient la tenue pour une activité donnée
		/// </summary>
		public ActivityOutfit GetOutfitForActivity(OutfitType activity)
		{
			return activityOutfits.Find(o => o.activity == activity);
		}

		/// <summary>
		/// Change la tenue pour une activité
		/// </summary>
		public void ChangeOutfit(OutfitType activity, string newOutfitName)
		{
			ActivityOutfit outfit = GetOutfitForActivity(activity);
			if (outfit != null)
			{
				outfit.outfitName = newOutfitName;
			}
		}

		/// <summary>
		/// Change la couleur/variante de la tenue
		/// </summary>
		public void ChangeColorVariant(OutfitType activity, string colorVariant)
		{
			ActivityOutfit outfit = GetOutfitForActivity(activity);
			if (outfit != null)
			{
				outfit.colorVariant = colorVariant;
			}
		}

		/// <summary>
		/// Crée et affiche un mannequin 3D avec la tenue pour une activité
		/// </summary>
		public GameObject ShowMannequinForActivity(OutfitType activity)
		{
			ActivityOutfit outfit = GetOutfitForActivity(activity);
			if (outfit == null) return null;

			// Nettoyer le mannequin précédent
			ClearAllMannequins();

			// Créer un mannequin simple (capsule) pour le moment
			GameObject mannequin = GameObject.CreatePrimitive(PrimitiveType.Capsule);
			mannequin.name = $"Mannequin_{activity}";
			mannequin.transform.position = mannequinPosition;
			mannequin.transform.localScale = new Vector3(0.6f, 1.2f, 0.6f);

			// Ajouter un composant pour rotation automatique
			MannequinRotator rotator = mannequin.AddComponent<MannequinRotator>();
			rotator.rotationSpeed = mannequinRotationSpeed;

			// Couleur selon l'activité (temporaire jusqu'à avoir les vrais assets)
			Renderer renderer = mannequin.GetComponent<Renderer>();
			if (renderer != null)
			{
				Material mat = new Material(Shader.Find("Standard"));
				switch (activity)
				{
					case OutfitType.Chill:
						mat.color = new Color(0.3f, 0.6f, 0.9f, 1f); // Bleu décontracté
						break;
					case OutfitType.Sport:
						mat.color = new Color(0.9f, 0.3f, 0.3f, 1f); // Rouge sportif
						break;
					case OutfitType.Business:
						mat.color = new Color(0.2f, 0.2f, 0.2f, 1f); // Noir professionnel
						break;
				}
				renderer.material = mat;
			}

			// Label au-dessus du mannequin
			CreateMannequinLabel(mannequin, outfit);

			outfit.mannequinInstance = mannequin;
			return mannequin;
		}

		private void CreateMannequinLabel(GameObject mannequin, ActivityOutfit outfit)
		{
			// Créer un Canvas World Space au-dessus du mannequin
			GameObject labelGO = new GameObject("MannequinLabel");
			labelGO.transform.SetParent(mannequin.transform);
			labelGO.transform.localPosition = new Vector3(0, 1.5f, 0);

			Canvas canvas = labelGO.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.WorldSpace;
			
			RectTransform canvasRt = labelGO.GetComponent<RectTransform>();
			canvasRt.sizeDelta = new Vector2(200, 50);
			labelGO.transform.localScale = Vector3.one * 0.005f;

			// Texte
			GameObject textGO = new GameObject("Text");
			textGO.transform.SetParent(labelGO.transform, false);
			
			UnityEngine.UI.Text text = textGO.AddComponent<UnityEngine.UI.Text>();
			text.text = $"{GetActivityIcon(outfit.activity)} {outfit.activity}\n{outfit.colorVariant}";
			text.alignment = TextAnchor.MiddleCenter;
			text.fontSize = 24;
			text.color = Color.white;
			text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
			
			RectTransform textRt = textGO.GetComponent<RectTransform>();
			textRt.anchorMin = Vector2.zero;
			textRt.anchorMax = Vector2.one;
			textRt.offsetMin = Vector2.zero;
			textRt.offsetMax = Vector2.zero;

			// Ajouter outline pour meilleure lisibilité
			UnityEngine.UI.Outline outline = textGO.AddComponent<UnityEngine.UI.Outline>();
			outline.effectColor = Color.black;
			outline.effectDistance = new Vector2(2, -2);
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

		public void ClearAllMannequins()
		{
			foreach (var outfit in activityOutfits)
			{
				if (outfit.mannequinInstance != null)
				{
					Destroy(outfit.mannequinInstance);
					outfit.mannequinInstance = null;
				}
			}
		}
	}

	/// <summary>
	/// Composant simple pour faire tourner le mannequin
	/// </summary>
	public class MannequinRotator : MonoBehaviour
	{
		public float rotationSpeed = 20f;

		void Update()
		{
			transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
		}
	}
}

