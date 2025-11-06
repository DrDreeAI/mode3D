using System.Collections.Generic;
using UnityEngine;

namespace Mode3D.Destinations
{
	/// <summary>
	/// Affiche les vêtements "suspendus" comme portés par un fantôme invisible
	/// Utilise les assets de npc_casual_set_00
	/// </summary>
	public class GhostOutfitDisplay : MonoBehaviour
	{
		[Header("Positioning")]
		public Vector3 displayPosition = new Vector3(0f, 0.5f, 3.5f); // Position basse pour ne pas toucher le plafond
		public float outfitScale = 1.0f; // Échelle de l'ensemble
		
		[Header("Outfit Settings")]
		public bool useMaleClothes = true; // 'm' ou 'f'
		
		private GameObject currentOutfitParent;
		private List<GameObject> currentClothingPieces = new List<GameObject>();

		/// <summary>
		/// Affiche une tenue "fantôme" pour une catégorie et matière données
		/// </summary>
		public void ShowGhostOutfit(OutfitType category, string materialVariant)
		{
			ClearOutfit();

			// Créer le parent pour tous les vêtements
			currentOutfitParent = new GameObject($"GhostOutfit_{category}");
			currentOutfitParent.transform.position = displayPosition;
			currentOutfitParent.transform.localScale = Vector3.one * outfitScale;

			// Charger et positionner les vêtements selon la catégorie
			switch (category)
			{
				case OutfitType.Chill:
					LoadChillOutfit(materialVariant);
					break;
				case OutfitType.Sport:
					LoadSportOutfit(materialVariant);
					break;
				case OutfitType.Business:
					LoadBusinessOutfit(materialVariant);
					break;
			}

			// Rotation automatique
			MannequinRotator rotator = currentOutfitParent.AddComponent<MannequinRotator>();
			rotator.rotationSpeed = 20f;

			Debug.Log($"[GhostOutfit] Affiché: {category} - Matière {materialVariant}");
		}

		private void LoadChillOutfit(string materialVariant)
		{
			// Tenue décontractée : T-shirt + Pantalon
			string gender = useMaleClothes ? "m" : "f";
			
			// Charger toujours le prefab _01, puis changer les matériaux
			// T-shirt (partie haute du corps) - positions resserrées
			string tshirtPath = $"npc_casual_set_00/Prefabs/npc_csl_tshirt_00{gender}_01_01";
			GameObject tshirt = LoadAndPositionClothing(tshirtPath, new Vector3(0, 0.15f, 0), "Tshirt");
			ApplyMaterial(tshirt, "tshirt", gender, materialVariant);

			// Pantalon (partie basse du corps)
			string pantsPath = $"npc_casual_set_00/Prefabs/npc_csl_pants_00{gender}_01_01";
			GameObject pants = LoadAndPositionClothing(pantsPath, new Vector3(0, -0.15f, 0), "Pants");
			ApplyMaterial(pants, "pants", gender, materialVariant);

			// Chaussures (pieds)
			string shoePath = $"npc_casual_set_00/Prefabs/npc_csl_shoe_01_00_01";
			GameObject shoes = LoadAndPositionClothing(shoePath, new Vector3(0, -0.35f, 0), "Shoes");
			ApplyMaterial(shoes, "shoe", "", materialVariant);
		}

		private void LoadSportOutfit(string materialVariant)
		{
			// Pour Sport, on utilise aussi les t-shirts et pantalons
			// avec des variantes différentes
			string gender = useMaleClothes ? "m" : "f";
			
			string tshirtPath = $"npc_casual_set_00/Prefabs/npc_csl_tshirt_00{gender}_01_01";
			GameObject tshirt = LoadAndPositionClothing(tshirtPath, new Vector3(0, 0.15f, 0), "SportTop");
			ApplyMaterial(tshirt, "tshirt", gender, materialVariant);

			string pantsPath = $"npc_casual_set_00/Prefabs/npc_csl_pants_00{gender}_01_01";
			GameObject pants = LoadAndPositionClothing(pantsPath, new Vector3(0, -0.15f, 0), "SportPants");
			ApplyMaterial(pants, "pants", gender, materialVariant);

			string shoePath = $"npc_casual_set_00/Prefabs/npc_csl_shoe_01_00_01";
			GameObject shoes = LoadAndPositionClothing(shoePath, new Vector3(0, -0.35f, 0), "SportShoes");
			ApplyMaterial(shoes, "shoe", "", materialVariant);
		}

		private void LoadBusinessOutfit(string materialVariant)
		{
			// Tenue business : Chemise + Pantalon + Chaussures
			string gender = useMaleClothes ? "m" : "f";
			
			// Chemise (partie haute)
			string shirtPath = $"npc_casual_set_00/Prefabs/npc_csl_shirtopenrolled_00{gender}_01_01";
			GameObject shirt = LoadAndPositionClothing(shirtPath, new Vector3(0, 0.15f, 0), "Shirt");
			ApplyMaterial(shirt, "shirtopenrolled", gender, materialVariant);

			// Pantalon (partie basse) - même matière que la chemise pour cohérence
			string pantsPath = $"npc_casual_set_00/Prefabs/npc_csl_pants_00{gender}_01_01";
			GameObject pants = LoadAndPositionClothing(pantsPath, new Vector3(0, -0.15f, 0), "BusinessPants");
			ApplyMaterial(pants, "pants", gender, materialVariant);

			// Chaussures (pieds)
			string shoePath = $"npc_casual_set_00/Prefabs/npc_csl_shoe_01_00_01";
			GameObject shoes = LoadAndPositionClothing(shoePath, new Vector3(0, -0.35f, 0), "DressShoes");
			ApplyMaterial(shoes, "shoe", "", materialVariant);
		}

		private GameObject LoadAndPositionClothing(string prefabPath, Vector3 localPos, string pieceName)
		{
			// Charger le prefab depuis Assets/ (en mode runtime, il faut qu'il soit dans Resources)
			// Pour l'instant, on va les charger via Resources en créant des références
			
			// Utilisation directe du nom de fichier
			string fullPath = $"Assets/{prefabPath}.prefab";
			
#if UNITY_EDITOR
			// En mode éditeur, on peut utiliser AssetDatabase
			GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
			
			if (prefab != null)
			{
				GameObject piece = Instantiate(prefab);
				piece.name = pieceName;
				piece.transform.SetParent(currentOutfitParent.transform);
				piece.transform.localPosition = localPos;
				piece.transform.localRotation = Quaternion.identity;
				
				// Cacher le corps si présent (on veut juste les vêtements)
				HideBodyParts(piece);
				
				currentClothingPieces.Add(piece);
				
				Debug.Log($"[GhostOutfit] Chargé: {pieceName}");
				return piece;
			}
			else
			{
				Debug.LogWarning($"[GhostOutfit] Impossible de charger: {fullPath}");
			}
#else
			Debug.LogWarning($"[GhostOutfit] Chargement runtime non disponible. Utilisez Resources/ ou AssetBundles.");
#endif
			return null;
		}

		private void HideBodyParts(GameObject clothingPiece)
		{
			// Parcourir tous les mesh renderers
			SkinnedMeshRenderer[] renderers = clothingPiece.GetComponentsInChildren<SkinnedMeshRenderer>();
			
			foreach (SkinnedMeshRenderer renderer in renderers)
			{
				// Si c'est un corps/peau, le désactiver
				if (renderer.name.Contains("body") || renderer.name.Contains("face") || 
				    renderer.name.Contains("hmn") || renderer.name.Contains("skin"))
				{
					renderer.enabled = false; // Cacher le corps, garder les vêtements
				}
			}
		}

		/// <summary>
		/// Applique un matériau URP spécifique à un vêtement
		/// </summary>
		private void ApplyMaterial(GameObject clothingPiece, string clothingType, string gender, string materialVariant)
		{
			if (clothingPiece == null) return;

			string materialCode = GetMaterialCode(materialVariant);
			
			// Construire le chemin du matériau
			// Format: mtl_npc_csl_[type]_00[gender]_01_[code].mat
			string materialPath = "";
			
			if (clothingType == "shoe")
			{
				// Chaussures n'ont pas de genre
				materialPath = $"npc_casual_set_00/MaterialsUPR/mtl_npc_csl_shoe_01_00_{materialCode}";
			}
			else
			{
				// Autres vêtements ont un genre
				materialPath = $"npc_casual_set_00/MaterialsUPR/mtl_npc_csl_{clothingType}_00{gender}_01_{materialCode}";
			}

#if UNITY_EDITOR
			Material material = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>($"Assets/{materialPath}.mat");
			
			if (material != null)
			{
				// Appliquer le matériau à tous les SkinnedMeshRenderer
				SkinnedMeshRenderer[] renderers = clothingPiece.GetComponentsInChildren<SkinnedMeshRenderer>();
				
				foreach (SkinnedMeshRenderer renderer in renderers)
				{
					// Ne pas changer les matériaux du corps/visage
					if (!renderer.name.Contains("body") && !renderer.name.Contains("face") && 
					    !renderer.name.Contains("hmn") && !renderer.name.Contains("skin"))
					{
						renderer.material = material;
						Debug.Log($"[GhostOutfit] Matériau appliqué: {materialPath}");
					}
				}
			}
			else
			{
				Debug.LogWarning($"[GhostOutfit] Matériau introuvable: {materialPath}");
			}
#else
			Debug.LogWarning($"[GhostOutfit] Changement de matériau non disponible en runtime.");
#endif
		}

		private string GetMaterialCode(string materialName)
		{
			// Mapper les noms de matière aux codes de prefab
			// Chaque code correspond à un prefab avec ses propres matériaux
			switch (materialName.ToLower())
			{
				case "matière 1":
				case "material 1":
				case "style 1":
					return "01"; // Premier style de matière
				case "matière 2":
				case "material 2":
				case "style 2":
					return "02"; // Deuxième style
				case "matière 3":
				case "material 3":
				case "style 3":
					return "03"; // Troisième style
				case "noir et blanc":
				case "black white":
				case "bw":
					return "01bw"; // Variante noir/blanc
				default:
					return "01"; // Défaut
			}
		}
		
		/// <summary>
		/// Obtient la liste des variantes de matières disponibles
		/// </summary>
		public static string[] GetAvailableMaterials()
		{
			return new string[] { "Matière 1", "Matière 2", "Matière 3", "Noir et Blanc" };
		}

		public void ClearOutfit()
		{
			foreach (GameObject piece in currentClothingPieces)
			{
				if (piece != null)
				{
					Destroy(piece);
				}
			}
			currentClothingPieces.Clear();

			if (currentOutfitParent != null)
			{
				Destroy(currentOutfitParent);
				currentOutfitParent = null;
			}
		}

		void OnDestroy()
		{
			ClearOutfit();
		}
	}
}

