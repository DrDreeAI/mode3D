using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gère l'affichage des mannequins avec les tenues jour par jour
/// Compatible avec le système CodeFirst WearableController
/// </summary>
public class OutfitShowcaseManager : MonoBehaviour
{
	[Header("Mannequin Settings")]
	public GameObject mannequinPrefab; // Prefab du mannequin
	public Transform showcasePosition; // Position où afficher le mannequin
	public Vector3 defaultPosition = new Vector3(0f, 0f, 3f);
	public float rotationSpeed = 30f; // Vitesse de rotation du mannequin

	[Header("Character Settings")]
	public string characterFolderName = "DefaultCharacter"; // Nom du dossier dans Resources/Characters

	private GameObject currentMannequin;
	private CharacterWearableController wearableController;
	private int currentDayIndex = 0;
	private int currentOutfitVariant = 0; // Pour changer de couleur/variante

	/// <summary>
	/// Affiche le défilé pour une journée spécifique
	/// </summary>
	public void ShowOutfitsForDay(int dayIndex)
		{
		currentDayIndex = dayIndex;
		currentOutfitVariant = 0;
		
		if (Mode3D.Destinations.OutfitSelection.Instance == null || dayIndex >= Mode3D.Destinations.OutfitSelection.Instance.dailyOutfits.Count)
			return;

		Mode3D.Destinations.DayOutfit day = Mode3D.Destinations.OutfitSelection.Instance.dailyOutfits[dayIndex];
		
		// Créer ou réutiliser le mannequin
		SetupMannequin();
		
		// Appliquer les tenues du jour
		ApplyDayOutfits(day);
	}

	/// <summary>
	/// Change la variante/couleur de la tenue actuelle
	/// </summary>
	public void ChangeOutfitVariant()
	{
		currentOutfitVariant++;
		if (currentOutfitVariant > 2) currentOutfitVariant = 0; // 3 variantes max
		
		if (Mode3D.Destinations.OutfitSelection.Instance != null && currentDayIndex < Mode3D.Destinations.OutfitSelection.Instance.dailyOutfits.Count)
		{
			Mode3D.Destinations.DayOutfit day = Mode3D.Destinations.OutfitSelection.Instance.dailyOutfits[currentDayIndex];
			ApplyDayOutfits(day);
		}
	}

		private void SetupMannequin()
		{
			if (currentMannequin != null)
			{
				// Nettoyer les vêtements existants
				if (wearableController != null)
				{
					wearableController.RemoveAllWearables(false);
				}
				return;
			}

			// Créer un nouveau mannequin si nécessaire
			if (mannequinPrefab != null)
			{
				Vector3 spawnPos = showcasePosition != null ? showcasePosition.position : defaultPosition;
				currentMannequin = Instantiate(mannequinPrefab, spawnPos, Quaternion.identity);
				currentMannequin.name = "ShowcaseMannequin";
			}
			else
			{
				// Créer un mannequin simple (cube pour le moment)
				currentMannequin = GameObject.CreatePrimitive(PrimitiveType.Capsule);
				currentMannequin.name = "ShowcaseMannequin";
				Vector3 spawnPos = showcasePosition != null ? showcasePosition.position : defaultPosition;
				currentMannequin.transform.position = spawnPos;
				currentMannequin.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
			}

			// Ajouter le WearableController si le mannequin a un SkinnedMeshRenderer
			wearableController = currentMannequin.GetComponent<CharacterWearableController>();
			if (wearableController == null)
			{
				wearableController = currentMannequin.AddComponent<CharacterWearableController>();
			}
		}

		private void ApplyDayOutfits(Mode3D.Destinations.DayOutfit day)
		{
			if (wearableController == null || day.outfits.Count == 0)
			{
				Debug.Log($"[OutfitShowcase] Jour {currentDayIndex + 1} - Aucune tenue ou pas de contrôleur");
				return;
			}

		// Mapper les types de tenues aux types de wearables
		foreach (var outfitType in day.outfits)
		{
			string wearableName = GetWearableNameForOutfit(outfitType, currentOutfitVariant);
			WearableType wType = GetWearableType(outfitType);
			
			// Créer et appliquer le wearable
			Wearable wearable = Wearable.CreateWearable();
			wearable.Name = wearableName;
			wearable.CharacterFolderName = characterFolderName;
			wearable.WearableType = wType;
			
			wearableController.AddWearable(wearable);
			wearableController.ApplyWearable(wearable);
			
			Debug.Log($"[OutfitShowcase] Appliqué: {outfitType} → {wearableName} (variante {currentOutfitVariant})");
		}
	}

	private string GetWearableNameForOutfit(Mode3D.Destinations.OutfitType outfit, int variant)
	{
		// Mapper les types de tenues aux noms de fichiers dans Resources
		// Avec variantes de couleur (0, 1, 2)
		string baseName = "";
		
		switch (outfit)
		{
			case Mode3D.Destinations.OutfitType.Chill: baseName = "CasualTop"; break;
			case Mode3D.Destinations.OutfitType.Sport: baseName = "SportTop"; break;
			case Mode3D.Destinations.OutfitType.Business: baseName = "BusinessTop"; break;
			default: baseName = "DefaultTop"; break;
		}
		
		// Ajouter le suffixe de variante si disponible
		if (variant > 0)
		{
			baseName += $"_v{variant}"; // Ex: CasualTop_v1, CasualTop_v2
		}
		
		return baseName;
	}

	private WearableType GetWearableType(Mode3D.Destinations.OutfitType outfit)
		{
			// Tous sont des "tops" pour simplifier
			return WearableType.Top;
		}

		public void HideMannequin()
		{
			if (currentMannequin != null)
			{
				currentMannequin.SetActive(false);
			}
		}

		public void ShowMannequin()
		{
			if (currentMannequin != null)
			{
				currentMannequin.SetActive(true);
			}
		}

		public void ClearMannequin()
		{
			if (currentMannequin != null)
			{
				Destroy(currentMannequin);
				currentMannequin = null;
				wearableController = null;
			}
		}

		private void Update()
		{
			// Faire tourner le mannequin pour mieux voir les tenues
			if (currentMannequin != null && currentMannequin.activeSelf)
			{
				currentMannequin.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
			}
		}
	}

