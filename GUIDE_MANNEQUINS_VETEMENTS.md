# 👔 Guide : Mannequins et Vêtements - Mode3D

## ✅ Système de Défilé Implémenté !

J'ai créé un système complet pour afficher un mannequin avec les tenues sélectionnées jour par jour.

---

## 🎯 Fonctionnalités Ajoutées

### 1. ✅ Effacement de l'Image de Ville
Quand vous cliquez sur "← Retour" depuis le calendrier :
- L'image de la ville (WindowCityView) est automatiquement supprimée
- Vous pouvez resélectionner une nouvelle ville

### 2. ✅ Défilé de Mannequin avec Tenues
- **Mannequin qui tourne** automatiquement pour voir les tenues sous tous les angles
- **Mise à jour en temps réel** quand vous ajoutez/supprimez des tenues
- **Change à chaque jour** quand vous passez au jour suivant
- **Nettoyage automatique** à la fin

---

## 📁 Nouveau Script : OutfitShowcaseManager.cs

### Fonctionnalités
- ✅ Crée et positionne un mannequin dans la scène
- ✅ Applique les vêtements via le système CodeFirst/WearableController
- ✅ Rotation automatique du mannequin (30°/seconde)
- ✅ Mise à jour en temps réel des tenues
- ✅ Nettoyage complet à la fin

---

## 🛠️ Configuration Requise (Structure des Dossiers)

Pour que les vêtements fonctionnent, vous devez créer cette structure :

```
Assets/
└── Resources/
    └── Characters/
        └── {NomDuPersonnage}/  (ex: "DefaultCharacter")
            └── Wearables/
                ├── CasualTop.fbx      (Tenue Chill)
                ├── SportTop.fbx       (Tenue Sport)
                ├── BusinessTop.fbx    (Tenue Business)
                └── ... autres vêtements ...
```

### Mapping Actuel (à adapter selon vos assets)

| Type de Tenue | Fichier Recherché | Type Wearable |
|---------------|-------------------|---------------|
| 👕 Chill | `CasualTop.fbx` | Top |
| 🏃 Sport | `SportTop.fbx` | Top |
| 👔 Business | `BusinessTop.fbx` | Top |

---

## 🎨 Comment Ça Fonctionne

### 1. Démarrage de la Sélection des Tenues
```
Jour 1 → Créer mannequin → Positionner à (0, 0, 3)
```

### 2. Ajout d'une Tenue
```
Clic sur [👕 Chill]
  → ToggleOutfit(Chill)
  → showcaseManager.ShowOutfitsForDay(0)
  → Charge "CasualTop.fbx"
  → Applique au mannequin
  → Mannequin tourne avec la tenue
```

### 3. Suppression d'une Tenue
```
Clic sur ✖
  → RemoveOutfit(Chill)
  → showcaseManager.ShowOutfitsForDay(0)
  → Retire le vêtement du mannequin
```

### 4. Passage au Jour Suivant
```
Clic sur [➡️ Jour suivant]
  → Jour 2 affiché
  → showcaseManager.ShowOutfitsForDay(1)
  → Retire les tenues du jour 1
  → Applique les tenues du jour 2
```

### 5. Fin du Processus
```
Dernier jour → [📋 Récapitulatif]
  → showcaseManager.ClearMannequin()
  → Supprime le mannequin
  → Affiche le récapitulatif
```

---

## 🎮 Intégration avec CodeFirst

Le système utilise le **WearableController** de CodeFirst :

### Classe Utilisée
```csharp
CharacterWearableController
```

### Méthodes Appelées
- `AddWearable(wearable)` - Ajoute un vêtement à la liste
- `ApplyWearable(wearable)` - Applique visuellement le vêtement
- `RemoveAllWearables(false)` - Retire tous les vêtements

### Types de Wearables Disponibles
```csharp
enum WearableType {
    Hair = 1,
    Top = 2,
    Bottom = 3,
    Shoes = 4,
    Jacket = 5,
    Accessory1 = 6,
    Accessory2 = 7,
    Glasses = 8,
    Shoe1 = 9,
    Shoe2 = 10
}
```

---

## 📦 ÉTAPES POUR AJOUTER VOS MANNEQUINS ET VÊTEMENTS

### Étape 1 : Créer la Structure de Dossiers

1. Dans Unity, **Projet** → **Assets** → Clic droit → **Create** → **Folder** → "Resources"
2. Dans Resources → Create → Folder → "Characters"
3. Dans Characters → Create → Folder → "DefaultCharacter" (ou votre nom)
4. Dans DefaultCharacter → Create → Folder → "Wearables"

### Étape 2 : Importer Vos Assets

1. Trouvez vos fichiers .fbx de mannequins et vêtements
2. Glissez-déposez le **mannequin** dans `Assets/Resources/Characters/DefaultCharacter/`
3. Glissez-déposez les **vêtements** dans `Assets/Resources/Characters/DefaultCharacter/Wearables/`

### Étape 3 : Renommer les Fichiers (Important!)

Renommez vos vêtements pour correspondre au mapping :
- Un vêtement "décontracté" → `CasualTop.fbx`
- Un vêtement "sport" → `SportTop.fbx`
- Un vêtement "business" → `BusinessTop.fbx`

### Étape 4 : Créer un Prefab de Mannequin (Optionnel)

1. Glissez votre mannequin .fbx dans la scène
2. Assurez-vous qu'il a un **SkinnedMeshRenderer**
3. Glissez-le dans le dossier Prefabs pour créer un Prefab
4. Dans la Hiérarchie, trouvez "OutfitShowcaseManager" pendant le jeu
5. Assignez votre prefab dans l'Inspector

### Étape 5 : Tester

1. Lancez le jeu
2. Sélectionnez ville et dates
3. Sur l'écran des tenues :
   - Un mannequin apparaît (ou capsule si pas de prefab)
   - Il tourne automatiquement
   - Cliquez sur une tenue → Elle apparaît sur le mannequin
   - Cliquez ✖ → Elle disparaît du mannequin

---

## 🔧 Personnalisation

### Changer le Nom du Dossier Character

Dans `OutfitShowcaseManager.cs` :
```csharp
public string characterFolderName = "DefaultCharacter";
```
Changez en votre nom de dossier.

### Changer les Noms de Fichiers de Vêtements

Dans `OutfitShowcaseManager.cs`, méthode `GetWearableNameForOutfit` :
```csharp
switch (outfit)
{
    case OutfitType.Chill: return "VotreVetementChill";
    case OutfitType.Sport: return "VotreVetementSport";
    case OutfitType.Business: return "VotreVetementBusiness";
}
```

### Ajouter Plus de Types de Vêtements

Vous pouvez mapper à différents WearableTypes :
```csharp
case OutfitType.Chill: 
    // Top + Bottom + Shoes
    return WearableType.Top; // ou Bottom, Shoes, etc.
```

---

## 🎭 Mannequin par Défaut

Si vous n'avez pas encore de mannequin .fbx :
- Une **capsule simple** est créée automatiquement
- Elle tourne pour montrer qu'elle représente le mannequin
- Vous pouvez la remplacer plus tard par un vrai mannequin

---

## ⚙️ Configuration dans l'Inspector (Optionnel)

Pendant que le jeu tourne, sélectionnez "OutfitShowcaseManager" dans la Hiérarchie :

```
OutfitShowcaseManager (Script)
├─ Mannequin Prefab: [Assignez votre prefab]
├─ Showcase Position: [Transform où placer le mannequin]
├─ Default Position: (0, 0, 3)
├─ Rotation Speed: 30
└─ Character Folder Name: "DefaultCharacter"
```

---

## 📝 Exemple Complet

### Structure de Dossiers
```
Assets/
└── Resources/
    └── Characters/
        └── MyCharacter/
            ├── Mannequin.fbx
            └── Wearables/
                ├── CasualTop.fbx
                ├── CasualBottom.fbx
                ├── SportTop.fbx
                ├── SportBottom.fbx
                ├── BusinessSuit.fbx
                └── Shoes.fbx
```

### Résultat
- Jour 1 : Sélectionner Chill → Mannequin porte CasualTop
- Jour 2 : Sélectionner Sport → Mannequin porte SportTop  
- Jour 3 : Sélectionner Business → Mannequin porte BusinessSuit

---

## 🚀 Pour Tester Sans Assets (Maintenant)

Le système fonctionne déjà ! Même sans vêtements :
1. Lance le jeu
2. Un mannequin **capsule** apparaît et tourne
3. Sélectionnez des tenues (les vêtements ne s'appliqueront pas visuellement)
4. Le système est prêt pour vos vrais assets !

### Quand Vous Ajouterez Vos Assets FBX
- Créez juste la structure de dossiers
- Mettez vos .fbx aux bons endroits
- Relancez → Les vêtements s'appliqueront automatiquement !

---

## ✨ Améliorations Futures Possibles

- 🎨 Plusieurs mannequins (homme/femme)
- 👗 Vêtements complets (haut + bas + chaussures)
- 🎨 Couleurs de vêtements personnalisables
- 📸 Capture d'écran du mannequin habillé
- 🔄 Rotation manuelle avec la souris

---

**Le système de défilé est prêt ! 🎉**
Ajoutez vos assets .fbx et tout fonctionnera automatiquement !

---

Date : 5 novembre 2025

