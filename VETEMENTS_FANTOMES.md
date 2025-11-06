# 👻 Système de Vêtements "Fantômes"

## Vue d'ensemble

Le système utilise maintenant le package **npc_casual_set_00** pour afficher les vêtements comme s'ils étaient portés par un "fantôme invisible" (pas de mannequin visible, juste les vêtements flottants).

## Architecture

### GhostOutfitDisplay.cs

**Emplacement**: `Assets/_Project/Scripts/GhostOutfitDisplay.cs`

**Fonctionnalités**:
- Charge les prefabs de vêtements depuis `Assets/npc_casual_set_00/Prefabs/`
- Affiche uniquement les vêtements (cache les parties du corps/visage)
- Supporte 3 catégories : Chill, Sport, Business
- Rotation automatique des tenues

**Méthodes principales**:
```csharp
ShowGhostOutfit(OutfitType category, string colorVariant)
ClearOutfit()
```

## Assets Disponibles

### 📂 npc_casual_set_00/

Le package contient:
- **T-shirts** (`npc_csl_tshirt_00[m/f]_01_[01/02/03/01bw]`)
- **Pantalons** (`npc_csl_pants_00[m/f]_01_[01/02/03/01bw]`)
- **Chemises** (`npc_csl_shirtopenrolled_00[m/f]_01_[01/02/01bw]`)
- **Chaussures** (`npc_csl_shoe_01_00_[01/02/03/01bw]`)
- **Cheveux** (`npc_haircut_*`)

### Variantes de Couleur

| Code | Description |
|------|-------------|
| `01` | Variante 1 (couleur par défaut) |
| `02` | Variante 2 (rouge/rose) |
| `03` | Variante 3 (verte) |
| `01bw` | Noir et blanc |

## Catégories de Tenues

### Chill (Décontracté)
- T-shirt
- Pantalon casual
- Chaussures décontractées

### Sport
- T-shirt sport
- Pantalon sport
- Chaussures de sport

### Business (Professionnel)
- Chemise boutonnée
- Pantalon noir (01bw)
- Chaussures classiques

## Positionnement

**Position par défaut**: `(0, 1.2, 3.5)`
- X = 0 : Centre horizontal
- Y = 1.2 : Hauteur d'une personne debout
- Z = 3.5 : Entre le tapis et la fenêtre

**Échelle**: 1.0 (taille normale)

## Mapping des Couleurs

Le système mappe les noms de couleurs aux codes de fichiers:

| Nom de Couleur | Code Fichier |
|----------------|--------------|
| Bleu / Blue | 01 |
| Rouge / Red | 02 |
| Vert / Green | 03 |
| Noir / Black | 01bw |
| Blanc / White | 01 |
| Gris / Gray | 01bw |
| Rose / Pink | 02 |

## Intégration avec le Flux

### OutfitProposalUI

Le système `GhostOutfitDisplay` remplace l'ancien système `InSceneOutfitDisplay`:

```csharp
// Création
ghostDisplay = displayGO.AddComponent<GhostOutfitDisplay>();

// Affichage
ghostDisplay.ShowGhostOutfit(outfit.category, colorName);

// Nettoyage
ghostDisplay.ClearOutfit();
```

### Changement de Couleur

Quand l'utilisateur change de couleur:
1. `ChangeToPreviousColor()` ou `ChangeToNextColor()` est appelé
2. `ghostDisplay.ShowGhostOutfit()` est rappelé avec la nouvelle couleur
3. L'ancienne tenue est détruite et la nouvelle est instantanée

## Fonctionnalités Spéciales

### Masquage du Corps

La méthode `HideBodyParts()` parcourt tous les `SkinnedMeshRenderer` et désactive ceux qui correspondent à:
- `body`
- `face`
- `hmn` (human)
- `skin`

Cela garantit que seuls les vêtements sont visibles.

### Rotation Automatique

Chaque tenue reçoit un composant `MannequinRotator` qui la fait tourner automatiquement:
- Vitesse de rotation: 20°/seconde
- Axe: Y (vertical)

## Chargement des Assets

### Mode Éditeur

Les prefabs sont chargés via `AssetDatabase.LoadAssetAtPath()`:
```csharp
#if UNITY_EDITOR
GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
#endif
```

### Mode Runtime

⚠️ **Important**: Pour un build final, les assets doivent être:
1. Placés dans un dossier `Resources/` OU
2. Chargés via AssetBundles

Actuellement, le système fonctionne uniquement en mode éditeur.

## Hiérarchie des GameObjects

```
GhostOutfitDisplay (GameObject)
└── GhostOutfit_[Category] (Parent)
    ├── Tshirt/Shirt (prefab instantié)
    ├── Pants (prefab instantié)
    └── Shoes (prefab instantié)
    └── MannequinRotator (Component)
```

## Debugging

### Logs

Le système produit des logs pour faciliter le debug:
```
[GhostOutfit] Affiché: Chill - Bleu
[GhostOutfit] Chargé: Tshirt
[GhostOutfit] Impossible de charger: [path]
```

### Vérifications

1. **Prefab introuvable**: Vérifiez que le chemin est correct
2. **Vêtements invisibles**: Vérifiez que `HideBodyParts()` ne cache pas tout
3. **Position incorrecte**: Ajustez `displayPosition` dans l'Inspector

## Améliorations Futures

### À Implémenter

- [ ] Support runtime (Resources ou AssetBundles)
- [ ] Plus de variantes de vêtements (vestes, accessoires)
- [ ] Système de taille (petit/moyen/grand)
- [ ] Animation d'apparition/disparition
- [ ] Support des vêtements femmes (`f`) en plus des hommes (`m`)
- [ ] Système de "layering" (superposer plusieurs pièces)

### Optimisations

- [ ] Pool d'objets pour éviter les Instantiate/Destroy répétés
- [ ] LODs (Level of Detail) pour les vêtements distants
- [ ] Batching des meshes pour réduire les draw calls

## Notes Techniques

### Performance

- Chaque tenue = 3-4 GameObjects (top, bottom, shoes)
- Polygones: ~3500-4000 tris par ensemble
- Textures: 512x512 à 2048x2048 (TIFF)

### Compatibilité

- Unity 6.0+
- Pipeline URP (Universal Render Pipeline)
- Shaders PBR Metallic

## Références

- Package: `npc_casual_set_00`
- Documentation: `readme.txt` dans le package
- Prefabs: `Assets/npc_casual_set_00/Prefabs/`
- Meshes: `Assets/npc_casual_set_00/Mesh/`
- Textures: `Assets/npc_casual_set_00/Textures/`

---

**Dernière mise à jour**: 6 novembre 2025
**Version**: 1.0

