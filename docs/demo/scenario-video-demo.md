# Vidéo de démonstration — scénario

Repérage fait le 16/11/2026 sur https://smallapp.org avec la session SuperAdmin (établissement « CETIC DE YAMBASSA »).

**Règle de tournage :** la vidéo et les tutoriels montrent uniquement le fonctionnement **d'un établissement**. On ne montre jamais la console SuperAdmin : Vue réseau, liste des établissements, Comptes SuperAdmin, filtre « Tout le réseau » du journal ou menu « Super administration ». Tout se filme connecté avec un compte de l'établissement de démonstration **Lycée Horizon Démo** (Administrateur, Censeur, Enseignant, Secrétariat…), jamais avec le compte SuperAdmin.

Données fictives prêtes à importer : `docs/demo/donnees-fictives/eleves-fictifs.csv` (158 élèves répartis en 6e A, 5e A, 4e A, 3e A, 2nde C et Tle D) et `enseignants-fictifs.csv` (12 enseignants, un par matière).

## État du Lycée Horizon Démo (16/11/2026)

Fait :
- 2 cycles et 6 classes (6e A, 5e A, 4e A, 3e A, 2nde C, Tle D) ;
- année 2026-2027 active ; 1er trimestre avec Séquence 1 (07/09–23/10) et Séquence 2 (26/10–18/12) ;
- 12 enseignants, 12 matières, 59 affectations matière-classe avec coefficient et professeur ;
- 158 élèves inscrits, avec leurs parents fictifs ;
- administrateur créé par le propriétaire (PIN gardé par lui).

- Notes de 3e A (classe vitrine), séquences 1 et 2, importées pour 8 matières (432 notes) : Allemand, Anglais, ECM, EPS, Français, Histoire-Géographie, Informatique, Mathématiques. Tableaux générés par `gen_notes.py` (une note par élève, matière et séquence, avec un niveau propre à chaque élève).

Reste à faire :
- les notes de 3e A en Physique-Chimie et SVT, puis éventuellement les autres classes (la page Notes refuse l'accès au profil Administrateur : il faut la session SuperAdmin, ou un compte Censeur ou Enseignant) ;
- l'échéancier de paiement et des versements ;
- l'emploi du temps ;
- les comptes de démo Censeur, Enseignant et Secrétariat ;
- les 2e et 3e trimestres.

Aucun compte de connexion n'a été créé automatiquement pendant les imports.

## Ce qu'il faut préparer avant de tourner

1. **Un établissement de démonstration fictif** (ex. « Lycée de Démonstration », ville fictive). Les écrans actuels montrent de vrais élèves (mineurs), de vrais enseignants et le nom d'un établissement client : ils ne doivent pas apparaître sur le site public.
2. **Des données fictives complètes** dans cet établissement :
   - environ 300 élèves dans 8 à 10 classes ;
   - les notes de la séquence 1 saisies presque partout, pour que les bulletins, le procès-verbal et les statistiques soient pleins ;
   - un échéancier de paiement et quelques versements. Aujourd'hui, Paiements et Trésorerie affichent 0 FCFA partout ;
   - l'emploi du temps rempli pour au moins une classe.
3. **Un compte de démonstration par rôle** (Administrateur, Censeur, Enseignant, Secrétariat, Comptabilité), connexion par code et PIN. Le compte SuperAdmin ne doit jamais apparaître à l'écran.
4. **Masquer le pied de page « Thème Sneat (ThemeSelection, MIT) »** dans les écrans filmés, ou le garder seulement là où la licence l'exige.
5. Corriger le libellé « Trimestre 1' » (apostrophe en trop) visible dans Bulletins et Procès-verbal.

## Vidéo principale « Regarder la démo » (≈ 90 secondes)

Format : 1920×1080, sans voix (sous-titres incrustés), musique légère. Elle s'affiche sur le site dans une fenêtre qui s'ouvre au clic sur « Regarder la démo ».

| # | Durée | Écran (URL) | Action à l'écran | Texte affiché |
|---|-------|-------------|------------------|---------------|
| 1 | 0:00–0:06 | Page de connexion | Connexion à l'établissement | « Toute votre école dans un seul outil. » |
| 2 | 0:06–0:20 | Tableau de bord de l'école (`/etablissement`) | Survol des indicateurs : élèves, classes, enseignants, recouvrement, notes saisies | « Le proviseur voit l'essentiel d'un coup d'œil. » |
| 3 | 0:20–0:28 | Même page, bas de page | Zoom sur « Alertes à traiter » et « Top 5 élèves » | « Les alertes utiles remontent toutes seules. » |
| 4 | 0:28–0:44 | Notes & discipline → une classe (`/notes?classe=…`) | Saisie de 4 ou 5 notes avec la touche Entrée ; l'appréciation (Passable, Bien…) apparaît automatiquement | « L'enseignant saisit. Chaque note est enregistrée aussitôt, même hors connexion. » |
| 5 | 0:44–0:52 | Même page, statistiques | Défilement vers « Statistiques de la séquence » | « Moyennes, taux de réussite, filles/garçons : calculés pour vous. » |
| 6 | 0:52–1:06 | Bulletins → la même classe (`/bulletins?classe=…`) | Classement par rang, clic sur un élève, aperçu du bulletin, puis « Imprimer la sélection » | « Les bulletins de toute la classe, prêts en un clic. » |
| 7 | 1:06–1:14 | Emploi du temps (`/emploi-du-temps`) | Grille colorée par matière, passage de la vue par classe à la vue par enseignant | « Le censeur organise les cours sans conflit. » |
| 8 | 1:14–1:22 | Paiements (`/paiements`) | Taux de recouvrement et montants restants par classe | « L'économe suit chaque versement. » |
| 9 | 1:22–1:30 | Logo + bouton | Écran de fin | « Gestionnaire — Demandez votre accès. » |

## Tutoriels par rôle (2 à 3 minutes chacun)

Ils pourront être proposés sur une page « Guides » du site ou envoyés aux nouveaux clients.

### Chef d'établissement (Proviseur)

- **Tableau de bord** : indicateurs, évolution des inscriptions, répartition par cycle, effectifs par classe.
- **Alertes à traiter** : notes manquantes, élèves en difficulté. **Top 5 élèves** de la séquence.
- **Activité récente et Journal des modifications** (`/journal`, limité à l'établissement) : qui a saisi ou modifié quoi, et quand.
- **Procès-verbal** (`/proces-verbal`) : PV d'évaluation par classe, imprimable et téléchargeable en PDF, avec la liste des matières non saisies.
- **Accès de l'équipe** : seulement si l'écran existe côté administrateur d'établissement. Sinon, on ne le montre pas.

### Censeur (administration pédagogique)

- **Notes & discipline** : vue par classe avec l'avancement de la saisie (« 3/13 matières complètes », « Il reste 155 notes »).
- **Discipline** : onglet de la même page.
- **Emploi du temps** : grilles par classe, par enseignant et par créneaux, import Excel/CSV/Word, export Excel, impression.
- **Bulletins** : par séquence, trimestre ou année, classement par rang, impression de toute la classe en PDF.
- **Listes de classe** : générer, imprimer et exporter en CSV.

### Enseignant

- Connexion par « Connexion à votre établissement ».
- **Mes classes**, puis le choix de la matière et de la séquence.
- **Saisie rapide** : Entrée passe à l'élève suivant, l'enregistrement est immédiat, les notes sont gardées sur l'appareil si la connexion coupe, et le bouton ABS marque un absent.
- **Statistiques de la séquence** : moyenne, taux de réussite, meilleure et plus basse note, écart filles/garçons.

### Secrétariat / Économat

- **Élèves et Inscriptions** : import Excel/CSV/Word, inscription d'un élève, fiche parents.
- **Paiements** : échéancier par tranches, enregistrement d'un versement, reçu PDF, situation d'un élève.
- **Trésorerie** : entrées, dépenses, transferts, fonds et comptes.

## Captures à produire pour le site (1440×900, données fictives)

| Fichier | Écran | Section du site |
|---------|-------|-----------------|
| `tableau-de-bord.png` | Tableau de bord de l'école, haut de page | Accueil (image principale) et onglet « Direction » |
| `alertes.png` | Alertes, Top 5 élèves, actions rapides | Accueil, « Une vision claire » |
| `saisie-notes.png` | Saisie des notes d'une classe | Onglet « Enseignants » |
| `bulletin.png` | Aperçu d'un bulletin | Fonctionnalités, « Notes & bulletins » |
| `emploi-du-temps.png` | Grille colorée d'une classe | Fonctionnalités, « Emplois du temps » |
| `paiements.png` | Paiements avec recouvrement rempli | Fonctionnalités, « Scolarité & paiements » |
| `inscriptions.png` | Élèves et graphique des effectifs | Onglet « Secrétariat » |
| `journal.png` | Journal des modifications | Section « Sécurité » |

## Médias produits le 16/11/2026

Générés par `outils/captures/capture.mjs` (Playwright), connecté comme administrateur du Lycée Horizon Démo. Le script ne fait que naviguer, défiler et ouvrir des vues : il ne saisit et n'enregistre rien. Il s'arrête si le mot « SuperAdmin » apparaît à l'écran.

- `medias/video/demo-gestionnaire.mp4` : visite de 1 min 40 en 1920×1080, sans son, avec légendes incrustées (tableau de bord, alertes, élèves, inscriptions, enseignants, bulletin de la 3e A, procès-verbal), plus sa vignette `demo-vignette.jpg`.
- `medias/captures/*.png` : 17 captures en 1440×900 (rendu ×2), et leur version pleine page dans `pleine-page/`.

À refaire une fois les données complétées : `emploi-du-temps.png` (grille vide) et `paiements.png` / `tresorerie.png` (0 FCFA).
**Journal exclu** : il affiche « dan · SuperAdmin » comme auteur des notes importées. Pour le filmer, il faudra des saisies faites par un compte de l'établissement.

Relancer : `node capture.mjs login <etat.json>` (l'utilisateur se connecte lui-même), puis `shots <etat.json> ../../medias/captures` et `video <etat.json> ../../medias/video`, et convertir en MP4 avec ffmpeg (couper le début indiqué dans `debut.txt`). Supprimer `etat.json` ensuite : il contient les jetons de session.
