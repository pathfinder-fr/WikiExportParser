# Wiki Export Parser

Cet outil permet de générer des données structurées dans différents formats à partir des données brutes XML exportées du wiki sur les règles du jeu de rôle Pathfinder du site [pathfinder-fr.org](http://www.pathfinder-fr.org).

Il se base sur le fichier d'import [wikixml.7z](http://db.pathfinder-fr.org/raw/wikixml.7z) qui contient les pages du wiki des règles pathfinder au format XML.

Il propose plusieurs commandes pour manipuler ces fichiers et en extraire des données structurées, utilisables par des outils de développement (base de données entre autre) ou des tableurs.

# Récupération et préparation des données

Ce programme travaille sur les pages du wiki extraites au format XML. Cette archive est généré de manière hebdomadaire automatiquement sur le serveur pathfinder-fr.org (dimanche soir).

Une explication sur l'origine des données et leur format peut être lue sur [le site pathfinder-fr.org](http://www.pathfinder-fr.org/Wiki/Db.MainPage.ashx).

Pour utiliser le programme, il faut télécharger ce fichier, et le décompresser dans un dossier "In".
*Important* : assurez-vous de déplacer les fichiers du dossier "Out" vers le dossier parent.
A la fin, vous devriez obtenir la structure suivante : "In\Pathfinder-RPG\Aasimar.xml" (par exemple).

# Exécution

Le programme s'exécute de la manière suivante : `wikiexportparser.exe [in] [out] [commandes ..] [paramètres...]`

* *in* : chemin du dossier "in" contenant le dossier Pathfinder-RPG.
* *out* : chemin du dossier dans lequel les fichiers générés doivent être placés
* *commandes ..* : commande(s) à exécuter, parmi la liste des commandes données ci-dessous. (exemple : spells, feats)
* *paramètres...* : liste de paramètres supplémentaires pour les commandes

Exemple : `wikiexportparser.exe In Out spells feats /csv` génère les sorts et les dons au format XML et CSV
à partir des fichiers se trouvant dans le sous dossier "In" et écrit les fichiers dans le dossier "Out".

# Commandes

Liste des commandes actuellement supportées :

## spells

Extrait et génère la base de données contenant l'ensemble des sorts du wiki

## feats

Extrait et génère la base de données contenant l'ensemble des dons du wiki

# Paramètres

* `/csv` : demande la génération des données au format CSV. Par défaut, seul le format XML est généré.
* `/log:fichier` : écrit le journal de génération dans le fichier indiqué en paramètre. exemple : `/log:spells.log`