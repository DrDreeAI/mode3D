# 📐 Positionnement des Écrans - Mode3D

## ✅ Configuration Actuelle

Tous les écrans sont maintenant positionnés à l'emplacement exact que vous avez spécifié.

---

## 🎯 Position des Écrans

### Coordonnées Fixes
- **Position X** : `0` (centré)
- **Position Y** : `50` (hauteur)
- **Position Z** : `50` (profondeur)
- **Rotation** : `(0°, 180°, 0°)` - Tourné vers l'origine (Z négatif)
- **Échelle** : `0.01` - Taille visible en World Space

### Visualisation
```
Vue de dessus (Y fixé à 50) :

                 Z
                 ↑
                 |
            50   |   📺 Écrans
                 |   (tournés vers ↓)
                 |
        ─────────┼─────────→ X
                 |
                 |
            0    |   🎮 Origine
                 |
```

---

## 📺 Écrans Concernés

### 1. Écran Sélection Ville
- **Canvas** : `DestinationSelectorCanvas`
- **Taille** : 400x300 pixels
- **Position** : (0, 50, 50)

### 2. Écran Sélection Dates
- **Canvas** : `DateSelectionCanvas`
- **Taille** : 450x380 pixels
- **Position** : (0, 50, 50)

### 3. Écran Sélection Tenues
- **Canvas** : `OutfitSelectionCanvas`
- **Taille** : 500x400 pixels
- **Position** : (0, 50, 50)

### 4. Écran Récapitulatif
- **Canvas** : `RecapCanvas`
- **Taille** : 550x500 pixels
- **Position** : (0, 50, 50)

---

## 🔧 Configuration Technique

### Mode de Rendu
```csharp
canvas.renderMode = RenderMode.WorldSpace;
```

### Transformation
```csharp
canvasGO.transform.position = new Vector3(0f, 50f, 50f);
canvasGO.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
canvasGO.transform.localScale = Vector3.one * 0.01f;
```

### Pourquoi Rotation Y=180° ?
L'écran est tourné de 180° sur l'axe Y pour faire face à l'origine (vers Z négatif). Si votre caméra est à Z < 50, l'écran sera visible de face.

---

## 🎮 Pour Tester

### 1. Positionnement de la Caméra
Pour voir les écrans correctement :
- Placez votre caméra à **Z < 50** (par exemple Z = 0 ou Z = 30)
- À une hauteur proche de **Y = 50** (par exemple Y = 45-55)
- L'écran sera visible devant vous

### 2. Vérification Visuelle
1. **Lancez le jeu**
2. Si vous êtes à l'origine (0, 0, 0) :
   - Regardez vers **Z positif** (devant)
   - Levez légèrement la tête (Y = 50 est en hauteur)
   - L'écran devrait être visible

### 3. Dans la Hiérarchie Unity
Pendant le jeu, vous pouvez vérifier :
1. Cherchez `DestinationSelectorCanvas` dans la Hiérarchie
2. Sélectionnez-le
3. Dans l'Inspector, vérifiez Transform :
   - Position : (0, 50, 50) ✓
   - Rotation : (0, 180, 0) ✓
   - Scale : (0.01, 0.01, 0.01) ✓

---

## ⚙️ Ajustements Possibles

### Si l'écran est trop haut/bas
Modifiez la valeur **Y** dans les scripts :
```csharp
// Actuellement Y = 50
canvasGO.transform.position = new Vector3(0f, 50f, 50f);

// Pour baisser (exemple Y = 30) :
canvasGO.transform.position = new Vector3(0f, 30f, 50f);
```

### Si l'écran est trop loin/proche
Modifiez la valeur **Z** :
```csharp
// Actuellement Z = 50
canvasGO.transform.position = new Vector3(0f, 50f, 50f);

// Pour rapprocher (exemple Z = 30) :
canvasGO.transform.position = new Vector3(0f, 50f, 30f);
```

### Si l'écran est trop petit/grand
Modifiez l'**échelle** :
```csharp
// Actuellement scale = 0.01
canvasGO.transform.localScale = Vector3.one * 0.01f;

// Pour agrandir (exemple 0.015) :
canvasGO.transform.localScale = Vector3.one * 0.015f;
```

---

## 📍 Position de Votre Caméra

Pour que les écrans soient visibles, assurez-vous que :
- Votre caméra est à **Z < 50** (pour regarder vers l'écran)
- Votre caméra est à une hauteur proche de **Y = 50**
- Votre caméra regarde vers **Z positif** (direction +Z)

### Positions Recommandées
```
Option 1 (proche) :
Camera.position = (0, 50, 0)
Camera.rotation = (0, 0, 0) - regarde vers +Z

Option 2 (moyen) :
Camera.position = (0, 50, 20)
Camera.rotation = (0, 0, 0)

Option 3 (loin) :
Camera.position = (0, 50, -10)
Camera.rotation = (0, 0, 0)
```

---

## ✅ Résumé

**Tous les écrans sont maintenant fixés à la position exacte** :
- X = 0
- Y = 50
- Z = 50

Ils ne bougent plus et restent à cet emplacement dans la scène, peu importe où se trouve la caméra.

---

Date de mise à jour : 5 novembre 2025

