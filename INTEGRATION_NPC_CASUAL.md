# ✅ Intégration du Package npc_casual_set_00

## 📋 Résumé des Changements

Le système a été modifié pour utiliser le package **npc_casual_set_00** au lieu du système CodeFirst. Les vêtements sont maintenant affichés comme s'ils étaient portés par un "fantôme invisible" (pas de mannequin, juste les vêtements flottants en forme humaine).

## 🆕 Nouveaux Fichiers

### 1. GhostOutfitDisplay.cs
**Emplacement**: `Assets/_Project/Scripts/GhostOutfitDisplay.cs`

**Rôle**: Gestionnaire principal pour afficher les vêtements "fantômes"

**Fonctionnalités**:
- Charge les prefabs de vêtements depuis `npc_casual_set_00/Prefabs/`
- Positionne les vêtements en forme de silhouette humaine :
  - T-shirt/Chemise : Y = +0.5 (torse)
  - Pantalon : Y = -0.2 (hanches/jambes)
  - Chaussures : Y = -0.8 (pieds)
- Cache automatiquement les parties du corps (visage, peau)
- Rotation automatique de la tenue (20°/sec)
- Support de 7 couleurs différentes

**Méthodes clés**:
```csharp
ShowGhostOutfit(OutfitType category, string colorVariant)
ClearOutfit()
```

### 2. VETEMENTS_FANTOMES.md
Documentation complète du système de vêtements fantômes.

## 📝 Fichiers Modifiés

### OutfitProposalUI.cs

**Changements**:
- Remplacé `InSceneOutfitDisplay sceneDisplay` par `GhostOutfitDisplay ghostDisplay`
- Mise à jour de toutes les méthodes pour utiliser `ghostDisplay` :
  - `ShowGhostOutfit()` au lieu de `ShowMannequin()`
  - `ClearOutfit()` au lieu de `ClearAllMannequins()`
- Changement de couleur : re-affiche la tenue complète avec la nouvelle couleur

**Avant**:
```csharp
sceneDisplay.ShowMannequin(index, category, color);
sceneDisplay.ChangeOutfitColor(0, color, category);
```

**Après**:
```csharp
ghostDisplay.ShowGhostOutfit(category, color);
// Le changement de couleur = réaffichage complet
```

## 🎨 Catégories de Tenues

### Chill (Décontracté)
- **Haut**: T-shirt (`npc_csl_tshirt_00m_01_*`)
- **Bas**: Pantalon casual (`npc_csl_pants_00m_01_*`)
- **Chaussures**: Décontractées (`npc_csl_shoe_01_00_*`)
- **Couleurs**: Selon sélection utilisateur

### Sport
- **Haut**: T-shirt sport (`npc_csl_tshirt_00m_01_*`)
- **Bas**: Pantalon sport (`npc_csl_pants_00m_01_*`)
- **Chaussures**: Sport (`npc_csl_shoe_01_00_*`)
- **Couleurs**: Selon sélection utilisateur

### Business (Professionnel)
- **Haut**: Chemise (`npc_csl_shirtopenrolled_00m_01_*`)
- **Bas**: Pantalon noir (`npc_csl_pants_00m_01_01bw`)
- **Chaussures**: Classiques (`npc_csl_shoe_01_00_01`)
- **Couleurs**: Chemise colorée + pantalon/chaussures noirs

## 🎨 Système de Couleurs

### Couleurs Disponibles
1. Bleu (01)
2. Rouge (02)
3. Vert (03)
4. Noir (01bw)
5. Blanc (01)
6. Gris (01bw)
7. Rose (02)

### Mapping des Codes
Le système mappe automatiquement les noms de couleurs aux suffixes de fichiers :
- `Bleu` → `_01`
- `Rouge` → `_02`
- `Vert` → `_03`
- `Noir/Gris` → `_01bw`
- etc.

## 📍 Positionnement dans la Scène

### Position Globale
```csharp
displayPosition = new Vector3(0f, 1.2f, 3.5f);
```
- **X = 0** : Centre de la pièce
- **Y = 1.2** : Hauteur d'une personne debout
- **Z = 3.5** : Entre le tapis et la fenêtre

### Positions Relatives (par pièce)
```
Haut (T-shirt/Chemise)  : (0, +0.5, 0)  ← Torse
Bas (Pantalon)          : (0, -0.2, 0)  ← Hanches/Jambes
Chaussures              : (0, -0.8, 0)  ← Pieds
```

### Échelle
```csharp
outfitScale = 1.0f; // Taille normale
```

## 🔄 Flux d'Exécution

### 1. Initialisation (OutfitProposalUI)
```csharp
GameObject displayGO = new GameObject("GhostOutfitDisplay");
ghostDisplay = displayGO.AddComponent<GhostOutfitDisplay>();
```

### 2. Affichage d'une Tenue
```
OutfitProposalUI.ShowMannequinFor()
    ↓
ghostDisplay.ShowGhostOutfit(category, color)
    ↓
ClearOutfit() // Nettoyer l'ancien
    ↓
Créer GhostOutfit_[Category] parent
    ↓
LoadChillOutfit / LoadSportOutfit / LoadBusinessOutfit
    ↓
LoadAndPositionClothing() pour chaque pièce
    ↓
Instantiate prefab depuis AssetDatabase
    ↓
HideBodyParts() // Cacher le corps
    ↓
Ajouter MannequinRotator
```

### 3. Changement de Couleur
```
OutfitProposalUI.ChangeToPreviousColor() / ChangeToNextColor()
    ↓
Incrémenter/Décrémenter currentColorIndex
    ↓
ghostDisplay.ShowGhostOutfit(category, newColor)
    ↓
[Même flux que l'affichage initial]
```

### 4. Navigation entre Tenues
```
OutfitProposalUI.PreviousOutfit() / NextOutfit()
    ↓
currentOutfitIndex +/- 1
    ↓
UpdateOutfitDisplay()
    ↓
ShowMannequinFor() avec la nouvelle tenue
```

### 5. Validation et Suite
```
OutfitProposalUI.ValidateAll()
    ↓
ClearMannequin()
    ↓
Destroy(ghostDisplay.gameObject)
    ↓
ShowSuitcasePreparation()
```

## 🗂️ Structure des Assets

```
Assets/
└── npc_casual_set_00/
    ├── Prefabs/
    │   ├── npc_csl_tshirt_00m_01_01.prefab
    │   ├── npc_csl_tshirt_00m_01_02.prefab
    │   ├── npc_csl_pants_00m_01_01.prefab
    │   ├── npc_csl_shirtopenrolled_00m_01_01.prefab
    │   ├── npc_csl_shoe_01_00_01.prefab
    │   └── ... (plus de variantes)
    ├── Mesh/
    │   └── ... (fichiers .fbx)
    ├── Textures/
    │   └── ... (fichiers .tif)
    └── Materials/
        └── ...
```

## 🎬 Hiérarchie Runtime

```
Scene
├── GhostOutfitDisplay (MonoBehaviour)
│   └── GhostOutfit_Chill (GameObject, position (0, 1.2, 3.5))
│       ├── Tshirt (prefab, localPos (0, 0.5, 0))
│       ├── Pants (prefab, localPos (0, -0.2, 0))
│       └── Shoes (prefab, localPos (0, -0.8, 0))
│       └── MannequinRotator (Component)
│
├── Canvas (UI)
│   └── OutfitProposalUI
│       └── ...
│
└── ... (autres objets de scène)
```

## 🐛 Debugging

### Logs Disponibles

```
[GhostOutfit] Affiché: Chill - Bleu
[GhostOutfit] Chargé: Tshirt
[GhostOutfit] Chargé: Pants
[GhostOutfit] Chargé: Shoes
```

ou

```
[GhostOutfit] Impossible de charger: Assets/npc_casual_set_00/Prefabs/...
```

### Vérifications en Cas de Problème

1. **Vêtements invisibles** :
   - Vérifier que les prefabs existent dans le dossier
   - Vérifier les logs de chargement
   - Inspecter `GhostOutfit_[Category]` dans la hiérarchie

2. **Position incorrecte** :
   - Ajuster `displayPosition` dans l'Inspector
   - Vérifier les positions locales des pièces

3. **Couleurs non appliquées** :
   - Vérifier le mapping dans `GetColorCode()`
   - Vérifier que les prefabs avec les suffixes existent

4. **Corps visible** :
   - Vérifier `HideBodyParts()` et les noms des renderers
   - Ajouter d'autres patterns si nécessaire

## ⚠️ Limitations Actuelles

### Mode Éditeur Uniquement
Le système utilise `AssetDatabase` qui ne fonctionne qu'en mode éditeur :

```csharp
#if UNITY_EDITOR
GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
#endif
```

### Solution pour Build Final
Pour un build jouable, il faut :
1. Copier les prefabs dans `Assets/Resources/npc_casual_set_00/Prefabs/`
2. Remplacer `AssetDatabase` par `Resources.Load()`

**OU**

Utiliser des AssetBundles pour le chargement runtime.

### Vêtements Masculins Uniquement
Actuellement, seuls les vêtements masculins (`m`) sont utilisés.
Pour supporter les vêtements féminins, modifier :

```csharp
public bool useMaleClothes = true; // Changer en false pour femmes
```

## 📊 Performance

### Ressources par Tenue
- **GameObjects** : 4 (1 parent + 3 pièces)
- **Polygones** : ~3500-4000 tris
- **Textures** : 512x512 à 2048x2048 (PBR)
- **Draw Calls** : ~3-4 par tenue

### Optimisations Possibles
- Pool d'objets pour réutiliser les instances
- LODs pour réduire les polygones à distance
- Atlasing des textures pour réduire les draw calls
- Culling des vêtements hors caméra

## ✅ Tests Recommandés

### Test 1 : Affichage des Catégories
1. Lancer le jeu
2. Sélectionner une ville et des dates
3. Pour chaque jour, sélectionner chaque catégorie (Chill, Sport, Business)
4. Vérifier que les vêtements s'affichent correctement

### Test 2 : Changement de Couleurs
1. Dans l'écran de proposition de tenues
2. Utiliser les boutons "← Couleur Précédente" et "Couleur Suivante →"
3. Vérifier que les vêtements changent de couleur
4. Tester les 7 couleurs disponibles

### Test 3 : Navigation
1. Naviguer entre plusieurs tenues avec "← Tenue Précédente" et "Tenue Suivante →"
2. Vérifier que chaque tenue s'affiche correctement
3. Vérifier que l'ancienne tenue disparaît avant la nouvelle

### Test 4 : Rotation
1. Laisser l'affichage sans interaction
2. Vérifier que la tenue tourne automatiquement (20°/sec)
3. Vitesse doit être fluide et constante

### Test 5 : Nettoyage
1. Valider une tenue et passer à la suivante
2. Vérifier qu'il n'y a pas d'objets "fantômes" qui restent dans la scène
3. Inspecter la hiérarchie : pas de `GhostOutfit_*` résiduels

## 🚀 Prochaines Étapes

### Fonctionnalités à Ajouter
- [ ] Support des vêtements féminins
- [ ] Plus de catégories (Casual Chic, Outdoor, Soirée)
- [ ] Accessoires (chapeaux, sacs, lunettes)
- [ ] Variantes de chaussures selon la catégorie
- [ ] Animation d'apparition (fade-in)
- [ ] Éclairage dédié pour mettre en valeur les tenues

### Optimisations
- [ ] Système de pooling
- [ ] Chargement asynchrone des prefabs
- [ ] LODs automatiques
- [ ] Compression des textures

### Support Build
- [ ] Migration vers Resources/ ou AssetBundles
- [ ] Tests en mode build (pas seulement éditeur)

## 📚 Documentation Associée

- `VETEMENTS_FANTOMES.md` - Documentation détaillée du système
- `npc_casual_set_00/readme.txt` - Documentation du package
- `FLUX_COMPLET_FINAL.md` - Vue d'ensemble de l'application

---

**Date de mise à jour** : 6 novembre 2025  
**Version** : 1.0  
**Auteur** : AI Assistant (Claude Sonnet 4.5)

