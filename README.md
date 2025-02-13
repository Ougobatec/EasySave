# EasySave - Solution de Sauvegarde Automatis�e

## Description
EasySave est une application de sauvegarde permettant aux utilisateurs de d�finir et d'ex�cuter des travaux de sauvegarde de fichiers et dossiers. Con�ue initialement en mode console, elle �volue en une application graphique avec des fonctionnalit�s avanc�es de gestion de sauvegarde, de cryptage et de supervision � distance.

## Versions et Evolutions

### Version 1.0
- Application Console d�velopp�e en .NET Core.
- Gestion jusqu'� 5 travaux de sauvegarde (compl�te ou diff�rentielle).
- Compatibilit� multilingue (Anglais/Fran�ais).
- Ex�cution des sauvegardes via ligne de commande.
- Prise en charge des disques locaux, externes et lecteurs r�seaux.
- Fichier log journalier au format JSON enregistrant les actions de sauvegarde.
- Enregistrement en temps r�el de l'�tat des travaux dans un fichier JSON.
- Dynamic Link Library (DLL) pour la gestion des logs.

### Version 1.1
- Ajout de la possibilit� de choisir le format des fichiers logs (JSON ou XML).

### Version 2.0
- Passage � une interface graphique en WPF ou Avalonia.
- Nombre de travaux de sauvegarde illimit�.
- Int�gration de la fonctionnalit� de cryptage via CryptoSoft.
- Ajout du temps de cryptage dans le fichier log.
- Blocage des sauvegardes si un logiciel m�tier est d�tect�.

### Version 3.0
- Sauvegarde en parall�le des fichiers.
- Gestion des fichiers prioritaires (les fichiers prioritaires sont sauvegard�s en premier).
- Limitation des transferts simultan�s de fichiers volumineux pour optimiser la bande passante.
- Interaction temps r�el avec chaque travail de sauvegarde (Pause, Play, Stop).
- Mise en pause automatique des sauvegardes si un logiciel m�tier est en cours d'ex�cution.
- Supervision � distance via une console d�port�e utilisant des sockets.
- Modification de CryptoSoft pour le rendre mono-instance.
- R�duction dynamique des travaux parall�les en fonction de la charge r�seau.

## Fichiers de Logs
- **Log journalier:** Contient l'historique des sauvegardes effectu�es.
- **Fichier d'�tat:** Contient l'�tat actuel des sauvegardes en cours.
- **Format:** JSON ou XML en fonction du param�trage utilisateur.

## Auteurs
- [Axel Calendreau](https://github.com/calaxo)
- [Maxime Lirio](https://github.com/MaximeLIRIO)
- [Mikael Rouffiat](https://github.com/mickalol)
- [Hugo Battaglia](https://github.com/Ougobatec)
