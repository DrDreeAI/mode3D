---
description: Guide pour publier le projet sur l'Unity Asset Store
---

# Guide de Publication sur l'Unity Asset Store

## Étape 1 : Créer et configurer votre compte Unity Publisher

### 1.1 Inscription comme Publisher
1. Allez sur [Unity Asset Store Publisher Portal](https://publisher.unity.com/)
2. Connectez-vous avec votre Unity ID
3. Acceptez les termes et conditions de Publisher
4. Complétez votre profil Publisher :
   - Nom de l'éditeur
   - Description
   - Logo/Avatar
   - Informations fiscales (W-8/W-9 pour les paiements)
   - Informations bancaires pour les revenus

### 1.2 Vérification
- Unity vérifiera votre compte (peut prendre quelques jours)
- Vous recevrez une confirmation par email

---

## Étape 2 : Préparer votre Package Unity

### 2.1 Organisation du projet
```
YourAsset/
├── Scripts/
├── Prefabs/
├── Materials/
├── Models/
├── Textures/
├── Scenes/ (exemples)
├── Documentation/
│   ├── README.txt
│   └── UserGuide.pdf
└── Examples/
```

### 2.2 Nettoyer le projet
- Supprimez tous les fichiers inutiles
- Supprimez les assets de test non nécessaires
- Vérifiez qu'il n'y a pas de dépendances externes non documentées
- Supprimez le dossier `Library/` (il se régénère automatiquement)

### 2.3 Créer le package
1. Dans Unity, sélectionnez tous les dossiers/fichiers à inclure dans le Project
2. Clic droit → **Export Package...**
3. Décochez "Include dependencies" si vous voulez contrôler manuellement
4. Cochez "Include dependencies" pour auto-inclure les dépendances
5. Cliquez sur **Export** et sauvegardez le fichier `.unitypackage`

### 2.4 Vérifications importantes
- [ ] Testez le package dans un nouveau projet Unity vierge
- [ ] Vérifiez qu'il n'y a pas d'erreurs de console
- [ ] Assurez-vous que tous les fichiers nécessaires sont inclus
- [ ] Vérifiez la compatibilité avec différentes versions d'Unity
- [ ] Documentez les versions Unity supportées (ex: 2021.3+)

---

## Étape 3 : Préparer les Assets Marketing

### 3.1 Images requises
- **Icon** : 160x160 px (PNG, fond transparent)
- **Card Image** : 420x280 px (JPG/PNG) - Image principale
- **Cover Image** : 1950x1300 px (JPG/PNG) - Bannière
- **Screenshots** : Au moins 3-5 images (1920x1080 px recommandé)
- **Social Media Image** : 1200x630 px (optionnel)

### 3.2 Vidéo (optionnel mais recommandé)
- Lien YouTube ou Vimeo
- Démonstration de 1-3 minutes
- Montrez les fonctionnalités principales

### 3.3 Documentation
- **README.txt** : Inclus dans le package
- **Description détaillée** : Pour la page du store
- **User Guide** : PDF ou lien vers documentation web
- **API Reference** : Si applicable
- **Changelog** : Pour les futures mises à jour

---

## Étape 4 : Créer votre Asset sur le Publisher Portal

### 4.1 Nouvelle soumission
1. Allez sur [Publisher Portal](https://publisher.unity.com/)
2. Cliquez sur **"Submit New Package"**
3. Choisissez la catégorie appropriée :
   - 3D Models
   - Scripts
   - Tools
   - Templates
   - VFX
   - Complete Projects
   - etc.

### 4.2 Remplir les informations

#### Informations de base
- **Nom du package** : Clair et descriptif
- **Version** : 1.0.0 (suivre semantic versioning)
- **Description courte** : 1-2 phrases accrocheuses
- **Description complète** : 
  - Qu'est-ce que c'est ?
  - Fonctionnalités principales
  - Comment l'utiliser ?
  - Ce qui est inclus
  - Support et mises à jour

#### Détails techniques
- **Versions Unity supportées** : Min et Max
- **Render Pipelines** : Built-in, URP, HDRP
- **Plateformes** : Windows, Mac, Linux, Mobile, WebGL, Console
- **Taille du package**
- **Dépendances** : Listez tous les packages requis

#### Tags et catégorisation
- Ajoutez des tags pertinents (max 5)
- Choisissez la bonne catégorie et sous-catégorie

### 4.3 Upload des fichiers
1. Uploadez le fichier `.unitypackage`
2. Uploadez toutes les images marketing
3. Ajoutez le lien vidéo si disponible
4. Uploadez la documentation supplémentaire

### 4.4 Prix
- **Gratuit** : $0
- **Payant** : Définissez votre prix (Unity prend 30% de commission)
- Possibilité de changer le prix plus tard

---

## Étape 5 : Soumission et Review

### 5.1 Révision finale
Avant de soumettre, vérifiez :
- [ ] Toutes les images sont de bonne qualité
- [ ] La description est complète et sans fautes
- [ ] Le package a été testé dans un projet propre
- [ ] La documentation est claire et complète
- [ ] Les tags et catégories sont appropriés
- [ ] Les informations techniques sont exactes

### 5.2 Soumettre pour review
1. Cliquez sur **"Submit for Review"**
2. Unity examinera votre soumission (peut prendre 1-3 semaines)
3. Vous recevrez des notifications par email

### 5.3 Process de review
Unity vérifie :
- Qualité technique du code/assets
- Absence de contenu inapproprié
- Conformité aux guidelines
- Fonctionnalité comme décrit
- Documentation adéquate

### 5.4 Résultats possibles
- ✅ **Approuvé** : Votre asset est publié !
- ❌ **Rejeté** : Vous recevrez des feedbacks à corriger
- ⚠️ **Modifications requises** : Petits ajustements demandés

---

## Étape 6 : Après la Publication

### 6.1 Promotion
- Partagez sur les réseaux sociaux
- Créez un devlog/blogpost
- Participez aux forums Unity
- Répondez aux questions des utilisateurs

### 6.2 Support
- Répondez rapidement aux questions
- Corrigez les bugs signalés
- Publiez des mises à jour régulières

### 6.3 Mises à jour
Pour publier une mise à jour :
1. Créez un nouveau package avec les changements
2. Incrémentez le numéro de version (ex: 1.0.0 → 1.1.0)
3. Rédigez un changelog détaillé
4. Uploadez via le Publisher Portal
5. Les clients existants recevront la notification

### 6.4 Analytics
- Consultez vos statistiques dans le Publisher Portal
- Suivez les téléchargements et revenus
- Analysez les retours utilisateurs

---

## 📚 Resources Utiles

- [Publisher Documentation Officielle](https://support.unity.com/hc/en-us/categories/201268913-Unity-Asset-Store)
- [Submission Guidelines](https://unity.com/legal/as-provider/submission-guidelines)
- [Publisher Forum](https://forum.unity.com/forums/asset-store-publishing.524/)
- [Asset Store Best Practices](https://assetstore.unity.com/publishing/submission-guidelines)

---

## ⚠️ Points d'Attention

### Droits et Licences
- Assurez-vous d'avoir les droits sur tous les assets inclus
- Si vous utilisez des assets tiers, vérifiez les licences
- Ne redistribuez pas de contenu protégé par copyright

### Qualité
- Code propre et commenté
- Performance optimisée
- Testé sur différentes versions Unity
- Documentation professionnelle

### Support
- Préparez-vous à fournir du support
- Créez un email ou forum de support
- Soyez réactif aux questions

---

## 🎯 Checklist Finale

Avant soumission :
- [ ] Compte Publisher configuré et vérifié
- [ ] Package créé et testé dans un projet vierge
- [ ] Toutes les images marketing créées (icon, card, cover, screenshots)
- [ ] Vidéo démo créée (optionnel)
- [ ] Description complète rédigée
- [ ] Documentation incluse et à jour
- [ ] Tags et catégories sélectionnés
- [ ] Prix défini
- [ ] Versions Unity compatibles testées
- [ ] Changelog initial créé
- [ ] Informations fiscales/bancaires configurées
- [ ] Soumission finale relue

Bonne chance avec votre publication ! 🚀
