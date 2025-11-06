using UnityEngine;
using UnityEditor;

namespace Mode3D.Editor
{
	/// <summary>
	/// Outil éditeur pour vérifier que les prefabs npc_casual_set_00 sont accessibles
	/// Menu: Tools/Mode3D/Vérifier Prefabs NPC Casual
	/// </summary>
	public class PrefabVerifier : EditorWindow
	{
		[MenuItem("Tools/Mode3D/Vérifier Prefabs NPC Casual")]
		public static void VerifyPrefabs()
		{
			Debug.Log("=== VÉRIFICATION DES PREFABS NPC CASUAL ===");
			
			string[] testPaths = new string[]
			{
				"Assets/npc_casual_set_00/Prefabs/npc_csl_tshirt_00m_01_01.prefab",
				"Assets/npc_casual_set_00/Prefabs/npc_csl_tshirt_00m_01_02.prefab",
				"Assets/npc_casual_set_00/Prefabs/npc_csl_tshirt_00m_01_03.prefab",
				"Assets/npc_casual_set_00/Prefabs/npc_csl_pants_00m_01_01.prefab",
				"Assets/npc_casual_set_00/Prefabs/npc_csl_pants_00m_01_01bw.prefab",
				"Assets/npc_casual_set_00/Prefabs/npc_csl_shirtopenrolled_00m_01_01.prefab",
				"Assets/npc_casual_set_00/Prefabs/npc_csl_shoe_01_00_01.prefab"
			};
			
			int foundCount = 0;
			int missingCount = 0;
			
			foreach (string path in testPaths)
			{
				GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
				
				if (prefab != null)
				{
					Debug.Log($"✅ TROUVÉ : {path}");
					foundCount++;
					
					// Afficher les composants
					var renderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
					Debug.Log($"   → {renderers.Length} SkinnedMeshRenderer(s)");
				}
				else
				{
					Debug.LogWarning($"❌ MANQUANT : {path}");
					missingCount++;
				}
			}
			
			Debug.Log($"\n=== RÉSUMÉ ===");
			Debug.Log($"Prefabs trouvés : {foundCount}/{testPaths.Length}");
			Debug.Log($"Prefabs manquants : {missingCount}/{testPaths.Length}");
			
			if (missingCount == 0)
			{
				Debug.Log("🎉 Tous les prefabs sont accessibles !");
			}
			else
			{
				Debug.LogWarning("⚠️ Certains prefabs sont manquants. Vérifiez le package npc_casual_set_00.");
			}
		}
	}
}

