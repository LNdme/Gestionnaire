# Gestionnaire — site vitrine piloté par l'API

Les textes, pages, sections, appels à l'action, liens de navigation, paramètres du site et images affichés sur le site proviennent de l'API. PostgreSQL est géré avec Entity Framework Core et Aspire.

## Organisation

- `Gestionnaire.ApiService/Model` : pages, sections, navigation, paramètres, images et demandes de démonstration. Les trois illustrations Figma sont insérées dans PostgreSQL au premier démarrage.
- `Gestionnaire.ApiService/Data/Contextbd.cs` : contexte EF Core et mapping PostgreSQL.
- `Gestionnaire.ApiService/Data/Migrations` : migrations EF Core versionnées.
- `Gestionnaire.ApiService/Services` : accès métier, validation, initialisation et routes API.
- `Gestionnaire.Web/DTO` et `Gestionnaire.Web/Services` : contrats et client HTTP du site.

## Démarrage

Prérequis : .NET 11 et Docker Desktop.

```powershell
dotnet run --project Gestionnaire.AppHost
```

Au premier démarrage, EF Core applique les migrations et insère les pages initiales (accueil, solution, fonctionnalités, tarifs, questions, à propos et démo), la navigation et les paramètres du site. Les données persistent dans le volume PostgreSQL d'Aspire.

## Contenu initial

Le contenu initial (pages, navigation, paramètres) est défini dans `Gestionnaire.ApiService/Services/SiteSeedContent.cs`. Il s'inspire de la maquette Figma et des fonctions de la plateforme. Il n'est écrit que dans une base vide.

Pour remplacer le contenu d'une base existante par ce contenu initial (les modifications faites par l'administration sont perdues ; les demandes de démo et les images sont conservées) :

```powershell
dotnet user-secrets set "SiteContent:ResetContentOnStartup" "true" --project Gestionnaire.ApiService
dotnet run --project Gestionnaire.AppHost   # attendre le démarrage, puis arrêter
dotnet user-secrets remove "SiteContent:ResetContentOnStartup" --project Gestionnaire.ApiService
```

## Authentification d'administration

Les lectures du site et l'envoi public d'une demande de démo ne nécessitent pas de clé. Toutes les opérations d'administration exigent `X-API-Key`. Configurez cette clé hors du code :

```powershell
dotnet user-secrets set "SiteContent:AdminApiKey" "REMPLACER_PAR_UNE_CLE_LONGUE_ALEATOIRE" --project Gestionnaire.ApiService
```

Sans clé configurée, l'API d'administration renvoie HTTP 503; une clé absente ou invalide renvoie HTTP 401. N'envoyez pas cette clé depuis le navigateur public.

## Routes publiques

- `GET /api/pages` et `GET /api/pages/{slug}` : pages publiées avec leurs sections ordonnées.
- `GET /api/navigation` : liens visibles de l'en-tête et du pied de page.
- `GET /api/site-settings` : nom, liens d'accès et de démo, textes de pied de page.
- `GET /api/media/{id}` : images gérées par l'API.
- `POST /api/demo-requests` : enregistre une demande de démonstration dans PostgreSQL.

## Routes d'administration (`X-API-Key`)

- Pages : `GET/POST /api/admin/pages`, `PUT/DELETE /api/admin/pages/{id}`. Une mise à jour remplace les sections de la page. Les sections contiennent leur type, titre, texte, ordre et un objet `content` libre (par exemple `items`, champs de formulaire ou `imageUrl`).
- Navigation : `GET/POST /api/admin/navigation`, `PUT/DELETE /api/admin/navigation/{id}`.
- Paramètres : `GET/PUT /api/admin/site-settings`.
- Demandes de démo : `GET/DELETE /api/admin/demo-requests`, `PUT /api/admin/demo-requests/{id}/status` (`New`, `InProgress`, `Completed`).
- Médias : `POST /api/admin/media` en multipart (`file` : JPEG/PNG/WebP jusqu'à 5 Mo, vidéo MP4/WebM jusqu'à 60 Mo), puis placer l'URL retournée (`/api/media/{id}`) dans le contenu d'une section, dans `LogoUrl` ou dans `DemoVideoUrl`/`DemoVideoPosterUrl`. `GET /api/media/{id}` accepte les requêtes partielles (`Range`) pour la lecture vidéo. Suppression par `DELETE /api/admin/media/{id}` (les références dans les sections et les paramètres sont retirées).
- Paramètres d'affichage : nom, marque/logo, URL et libellé de connexion, libellé et destination du bouton de démo, textes de pied de page et copyright sont modifiables par `PUT /api/admin/site-settings`. Les champs suivants sont facultatifs dans la requête (absents = valeur conservée) :
  - vidéo de démonstration : `DemoVideoUrl`, `DemoVideoPosterUrl`, `DemoVideoLabel` (« Regarder la démo »), `DemoVideoTitle`. Une URL vide masque le bouton ; dans une section `hero`, `primaryActionUrl = "#video"` ouvre cette vidéo ;
  - contact direct : `ContactPhone` et `ContactWhatsApp` (format international, ex. `+237696649233`, stockés sans espaces) et `ContactHours`. Un numéro vide masque les boutons « Appeler » et « WhatsApp ».
- Demandes de démo : un téléphone (`phone`) ou une adresse e-mail suffit, au moins l'un des deux.

Exemple de mise à jour d'une page et de ses sections :

```powershell
$env:SITE_CONTENT_ADMIN_API_KEY = "<clé configurée dans les secrets de l'API>"
$headers = @{ "X-API-Key" = $env:SITE_CONTENT_ADMIN_API_KEY }
$page = @{
  slug = "questions"
  title = "Vos questions"
  intro = "Les réponses pour préparer votre établissement."
  metaDescription = "Questions fréquentes sur Gestionnaire."
  isPublished = $true
  sortOrder = 4
  sections = @(@{
    type = "faq"
    heading = "Questions fréquentes"
    body = ""
    sortOrder = 0
    content = @{ items = @(@{ question = "Faut-il installer un logiciel ?"; answer = "Non, le site fonctionne dans le navigateur." }) }
  })
} | ConvertTo-Json -Depth 10
Invoke-RestMethod -Method Put -Uri "https://<api>/api/admin/pages/<page-id>" -Headers $headers -ContentType "application/json" -Body $page
```

La clé doit être fournie dans la variable d'environnement `SITE_CONTENT_ADMIN_API_KEY` à l'administration et ne doit jamais être embarquée dans le site web.

Toutes les routes `PUT` valident les tailles et formats.
