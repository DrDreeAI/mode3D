using System.Collections.Generic;
using UnityEngine;

namespace Mode3D.Destinations
{
	/// <summary>
	/// Affiche toutes les tenues en cercle autour du tapis
	/// </summary>
	public class CircularOutfitDisplay : MonoBehaviour
	{
		[Header("Circle Settings")]
		public Vector3 circleCenter = new Vector3(0f, 0.3f, 0f); // Centre du cercle (tapis)
		public float circleRadius = 2.5f; // Rayon du cercle
		public float outfitScale = 0.4f; // Échelle des tenues

		private List<GhostOutfitDisplay> outfitDisplays = new List<GhostOutfitDisplay>();

		/// <summary>
		/// Affiche toutes les tenues en cercle
		/// </summary>
		public void ShowAllOutfitsInCircle(List<OutfitProposalUI.OutfitPresentation> outfits)
		{
			ClearAllOutfits();

			if (outfits == null || outfits.Count == 0) return;

			// Calculer l'angle entre chaque tenue
			float angleStep = 360f / outfits.Count;

			for (int i = 0; i < outfits.Count; i++)
			{
				// Calculer la position en cercle
				float angle = i * angleStep * Mathf.Deg2Rad;
				Vector3 position = circleCenter + new Vector3(
					Mathf.Sin(angle) * circleRadius,
					0f,
					Mathf.Cos(angle) * circleRadius
				);

				// Créer un gestionnaire pour cette tenue
				GameObject displayGO = new GameObject($"GhostOutfit_Circle_{i}");
				GhostOutfitDisplay display = displayGO.AddComponent<GhostOutfitDisplay>();
				display.displayPosition = position;
				display.outfitScale = outfitScale;

				// Afficher la tenue
				display.ShowGhostOutfit(outfits[i].category, outfits[i].selectedMaterial);

				// Faire tourner vers le centre
				displayGO.transform.rotation = Quaternion.Euler(0f, -i * angleStep, 0f);

				outfitDisplays.Add(display);
			}

			Debug.Log($"[CircularDisplay] {outfits.Count} tenues affichées en cercle");
		}

		/// <summary>
		/// Nettoie toutes les tenues affichées
		/// </summary>
		public void ClearAllOutfits()
		{
			foreach (var display in outfitDisplays)
			{
				if (display != null)
				{
					display.ClearOutfit();
					Destroy(display.gameObject);
				}
			}
			outfitDisplays.Clear();
		}

		void OnDestroy()
		{
			ClearAllOutfits();
		}
	}
}

