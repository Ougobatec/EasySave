<div align="center">

[![EasySave](https://github.com/Ougobatec/EasySave/blob/team1/Assets/logo.svg)](https://easysave.axel-cal.fr/)
 
[![.NET](https://img.shields.io/badge/-.NET%208.0-blueviolet?logo=dotnet)](https://easysave.axel-cal.fr/)
[![language](https://img.shields.io/badge/language-C%23-239120)](https://learn.microsoft.com/fr-fr/dotnet/csharp/tour-of-csharp/overview)
[![GitHub release](https://img.shields.io/github/v/release/Ougobatec/EasySave)](#)
[![GitHub release date](https://img.shields.io/github/release-date/Ougobatec/EasySave)](#)
[![GitHub last commit](https://img.shields.io/github/last-commit/Ougobatec/EasySave)](#)

[![getting started](https://img.shields.io/badge/getting_started-guide-1D76DB)](https://easysave.axel-cal.fr/)
[![Free](https://img.shields.io/badge/free_for_non_commercial_use-brightgreen)](#-license)

</div>


# EasySave - Solution de Sauvegarde Automatisée

## Description📜
EasySave est une application de sauvegarde permettant aux utilisateurs de définir et d'exécuter des travaux de sauvegarde de fichiers et dossiers. Conçue initialement en mode console, elle évolue en une application graphique avec des fonctionnalités avancées de gestion de sauvegarde, de cryptage et de supervision à distance.

## Versions et Evolutions🔜

### Version 1.0
- Application Console développée en .NET Core.
- Gestion jusqu'à 5 travaux de sauvegarde (complète ou différentielle).
- Compatibilité multilingue (Anglais/Français).
- Exécution des sauvegardes via ligne de commande.
- Prise en charge des disques locaux, externes et lecteurs réseaux.
- Fichier log journalier au format JSON enregistrant les actions de sauvegarde.
- Enregistrement en temps réel de l'état des travaux dans un fichier JSON.
- Dynamic Link Library (DLL) pour la gestion des logs.

### Version 1.1
- Ajout de la possibilité de choisir le format des fichiers logs (JSON ou XML).

### Version 2.0
- Passage à une interface graphique en WPF ou Avalonia.
- Nombre de travaux de sauvegarde illimité.
- Intégration de la fonctionnalité de cryptage via CryptoSoft.
- Ajout du temps de cryptage dans le fichier log.
- Blocage des sauvegardes si un logiciel métier est détecté.

### Version 3.0
- Sauvegarde en parallèle des fichiers.
- Gestion des fichiers prioritaires (les fichiers prioritaires sont sauvegardés en premier).
- Limitation des transferts simultanés de fichiers volumineux pour optimiser la bande passante.
- Interaction temps réel avec chaque travail de sauvegarde (Pause, Play, Stop).
- Mise en pause automatique des sauvegardes si un logiciel métier est en cours d'exécution.
- Supervision à distance via une console déportée utilisant des sockets.
- Modification de CryptoSoft pour le rendre mono-instance.
- Réduction dynamique des travaux parallèles en fonction de la charge réseau.

## Fichiers de Logs
- **Log journalier:** Contient l'historique des sauvegardes effectuées.
- **Fichier d'état:** Contient l'état actuel des sauvegardes en cours.
- **Format:** JSON ou XML en fonction du paramétrage utilisateur.

## Auteurs👨‍👩‍👧‍👦
- [Axel Calendreau](https://github.com/calaxo)
- [Maxime Lirio](https://github.com/MaximeLIRIO)
- [Mikael Rouffiat](https://github.com/mickalol)
- [Hugo Battaglia](https://github.com/Ougobatec)
