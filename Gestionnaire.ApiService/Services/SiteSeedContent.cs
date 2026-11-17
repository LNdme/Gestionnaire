using System.Text.Json;
using System.Text.Json.Serialization;
using Gestionnaire.ApiService.Model;

namespace Gestionnaire.ApiService.Services;

/// <summary>
/// Contenu initial du site vitrine. Il reprend la maquette Figma et les fonctions réellement observées
/// sur la plateforme (saisie rapide des notes hors connexion, bulletins et procès-verbaux PDF, journal des modifications…).
/// Les clés de <c>content</c> suivent les variantes du site Blazor (voir <c>CardsSection.razor</c>).
/// Ce contenu n'est écrit que dans une base vide, ou lors d'une réinitialisation explicite.
/// </summary>
public static class SiteSeedContent
{
    public static readonly Guid TeachersImageId = Guid.Parse("ae2ae491-25a4-4f4d-a0f5-582da40a89f1");
    public static readonly Guid EnrollmentsImageId = Guid.Parse("db122743-6487-4b6c-a315-3de1ab8e8430");
    public static readonly Guid NetworkImageId = Guid.Parse("94a35bc0-94c2-4522-a628-8456cc1e69b2");
    public static readonly Guid AlertsImageId = Guid.Parse("5afa2d85-1a2a-4641-9f77-c4b77a63a1df");
    public static readonly Guid ReportCardImageId = Guid.Parse("157b196d-e61d-48eb-bc52-81c603a09c5b");
    public static readonly Guid ClassReportImageId = Guid.Parse("ca9de927-adb1-494b-bfc7-89b6c001a237");
    public static readonly Guid StudentsImageId = Guid.Parse("32275514-985d-4d2a-82c6-32ffa62334ba");
    public static readonly Guid DemoVideoId = Guid.Parse("46a0d31a-af12-40d6-b9ba-e05ed4087511");
    public static readonly Guid DemoPosterId = Guid.Parse("d1a4f3dc-2192-4c67-9423-0d53dc09b700");

    /// <summary>
    /// Médias du contenu initial, copiés depuis <c>SeedMedia</c>. Ce sont des captures et la vidéo de visite
    /// de l'établissement fictif « Lycée Horizon Démo » (voir docs/demo) : aucune donnée réelle d'élève.
    /// </summary>
    public static readonly (Guid Id, string FileName, string ContentType)[] SeedMedia =
    [
        (TeachersImageId, "enseignants.png", "image/png"),
        (EnrollmentsImageId, "inscriptions.png", "image/png"),
        (NetworkImageId, "reseau.png", "image/png"),
        (AlertsImageId, "alertes.png", "image/png"),
        (ReportCardImageId, "bulletin.png", "image/png"),
        (ClassReportImageId, "proces-verbal.png", "image/png"),
        (StudentsImageId, "eleves.png", "image/png"),
        (DemoVideoId, "demo-gestionnaire.mp4", "video/mp4"),
        (DemoPosterId, "demo-vignette.jpg", "image/jpeg")
    ];

    private static readonly JsonSerializerOptions ContentJsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static string Media(Guid id) => $"/api/media/{id}";

    public static SiteSettings CreateSettings() => new()
    {
        SiteName = "Gestionnaire",
        BrandMarkText = "G",
        LogoUrl = string.Empty,
        LoginLabel = "Se connecter",
        LoginUrl = "https://smallapp.org",
        DemoActionLabel = "Demander une démonstration",
        DemoActionUrl = "/demo",
        DemoVideoUrl = Media(DemoVideoId),
        DemoVideoPosterUrl = Media(DemoPosterId),
        DemoVideoLabel = "Regarder la démo",
        DemoVideoTitle = "Visite de Gestionnaire en 1 min 40",
        ContactPhone = "+237696649233",
        ContactWhatsApp = "+237696649233",
        ContactHours = string.Empty,
        // Sert aussi de titre au bandeau d'appel à l'action des pages qui n'en définissent pas.
        FooterDescription = "L'école bien organisée, tout simplement.",
        FooterTagline = "Conçu pour les établissements d'enseignement.",
        CopyrightNotice = "Tous droits réservés."
    };

    public static SiteNavigationItem[] CreateNavigation() =>
    [
        Navigation("La solution", "/solution", 1),
        Navigation("Fonctionnalités", "/fonctionnalites", 2),
        Navigation("Tarifs", "/tarifs", 3),
        Navigation("Questions", "/questions", 4),
        Navigation("À propos", "/a-propos", 5)
    ];

    public static SitePage[] CreatePages() =>
    [
        Home(),
        Solution(),
        Features(),
        Pricing(),
        Questions(),
        About(),
        Demo()
    ];

    private static SitePage Home() => Page(
        "home",
        "Gérez votre établissement sans multiplier les fichiers.",
        "Inscriptions, notes, bulletins, emplois du temps et paiements dans un seul espace simple à utiliser.",
        "Gestionnaire réunit inscriptions, notes, bulletins, emplois du temps et paiements des collèges et lycées dans une plateforme en ligne, sans installation.",
        0,
        Section("hero", "", "", new
        {
            eyebrow = "Conçu pour les collèges et lycées",
            titleHighlight = "sans multiplier les fichiers.",
            // « #video » ouvre la vidéo de démo définie dans les paramètres du site.
            primaryActionLabel = "Regarder la démo",
            primaryActionUrl = "#video",
            secondaryActionLabel = "Demander une démonstration",
            secondaryActionUrl = "/demo",
            imageUrl = Media(NetworkImageId),
            imageAlt = "Tableau de bord d'un établissement dans Gestionnaire",
            reassurance = new[] { "Sans installation", "Accompagnement inclus" }
        }),
        Section("steps", "Une information saisie. Toute l'école à jour.",
            "Gestionnaire remplace les fichiers dispersés et les doubles saisies par un parcours clair, du premier jour à la fin d'année.",
            new
            {
                variant = "flow",
                kicker = "Une journée plus simple",
                items = new[]
                {
                    Item("L'élève est inscrit", "Son dossier, sa classe et ses responsables sont enregistrés une fois pour toutes.", icon: "people"),
                    Item("Les notes sont saisies", "Chaque enseignant retrouve uniquement ses classes et ses matières.", icon: "book"),
                    Item("Le bulletin est prêt", "Moyennes, rangs et appréciations se calculent automatiquement.", icon: "report")
                }
            }),
        Section("cards", "Chacun trouve sa place, sans se perdre.", "", new
        {
            variant = "roles",
            kicker = "Un outil, plusieurs métiers",
            items = new object[]
            {
                Role("Direction", "Une vision claire, sans courir après l'information", "Pilotez l'établissement d'un coup d'œil.",
                    "Effectifs, notes saisies, recouvrement, alertes à traiter et meilleurs élèves de la séquence : l'essentiel est réuni dans un tableau de bord lisible.",
                    Media(NetworkImageId), "Tableau de bord de la direction"),
                Role("Censeur", "La pédagogie suivie classe par classe", "Sachez où en est chaque classe.",
                    "Avancement de la saisie par matière, emploi du temps, bulletins et procès-verbaux à imprimer : le censeur suit tout depuis un même écran.",
                    Media(ClassReportImageId), "Procès-verbal d'évaluation d'une classe"),
                Role("Enseignants", "Moins d'administration, plus de pédagogie", "Saisissez les notes. Le reste se calcule.",
                    "Entrée passe à l'élève suivant et chaque note est enregistrée aussitôt, même si la connexion coupe. Moyennes, rangs et appréciations sont préparés automatiquement.",
                    Media(ReportCardImageId), "Bulletin de notes calculé automatiquement"),
                Role("Secrétariat", "Chaque dossier reste complet et facile à retrouver", "Les inscriptions enfin bien rangées.",
                    "Élèves, parents, classes, listes et paiements restent à jour dans un espace partagé par les personnes autorisées.",
                    Media(StudentsImageId), "Répertoire des élèves et effectifs par classe")
            }
        }),
        Section("cards", "Tout ce qui fait tourner votre établissement.", "Les modules partagent les mêmes données : rien n'est ressaisi.", new
        {
            variant = "features",
            kicker = "L'essentiel, bien fait",
            items = new[]
            {
                Item("Notes & bulletins", "Bulletins par séquence, trimestre ou année, classés par rang et imprimés en PDF pour toute la classe.", icon: "report"),
                Item("Emplois du temps", "Grilles par classe ou par enseignant, créneaux libres visibles, import depuis vos fichiers.", icon: "calendar"),
                Item("Scolarité & paiements", "Échéancier par tranches, versements, restes à payer et recouvrement par classe.", icon: "coins"),
                Item("Tableaux de bord", "Effectifs, filles et garçons, notes saisies, absences : des indicateurs qui parlent simplement.", icon: "spark")
            },
            actionLabel = "Toutes les fonctionnalités",
            actionUrl = "/fonctionnalites"
        }),
        Section("cards", "Simple à utiliser. Sérieux sur la sécurité.",
            "Les données sont protégées, sauvegardées et accessibles uniquement aux personnes que vous autorisez.",
            new
            {
                variant = "flow",
                kicker = "Votre école, vos données",
                items = new[]
                {
                    Item("Accès par rôle", "Proviseur, censeur, enseignant, secrétariat, comptabilité : chacun voit uniquement ce qui le concerne.", icon: "lock"),
                    Item("Chaque modification tracée", "Le journal indique qui a saisi ou modifié une note, et quand.", icon: "clock"),
                    Item("Accompagnement humain", "Une équipe disponible pour vous aider à chaque étape.", icon: "people")
                }
            }),
        Section("text", "Une offre lisible",
            "Le bon niveau pour votre établissement.",
            new
            {
                description = "Le tarif dépend du nombre d'élèves. Tous les forfaits incluent l'installation, la reprise de vos données, la formation et l'assistance.",
                actionLabel = "Recevoir un tarif",
                actionUrl = "/tarifs"
            }),
        Section("faq", "Vous vous demandez peut-être…", "Une autre question ? Parlez-nous de votre établissement.", new
        {
            variant = "accordion",
            kicker = "Questions fréquentes",
            items = new[]
            {
                Faq("Faut-il installer un logiciel ?", "Non. Gestionnaire fonctionne directement dans votre navigateur, sur ordinateur, tablette ou téléphone."),
                Faq("Pouvez-vous reprendre nos données Excel ?", "Oui. Élèves, enseignants, matières et emplois du temps s'importent depuis vos fichiers Excel, CSV ou Word, avec une vérification avant tout enregistrement."),
                Faq("Que se passe-t-il si la connexion est lente ?", "L'interface reste légère. Pendant la saisie des notes, chaque note est gardée sur l'appareil si la connexion coupe, puis enregistrée."),
                Faq("Qui peut voir les informations ?", "Chaque personne dispose d'un accès adapté à son rôle. La direction garde le contrôle des autorisations.")
            },
            actionLabel = "Toutes les questions",
            actionUrl = "/questions"
        }),
        Cta("La suite est simple", "Voyez Gestionnaire avec vos propres besoins."));

    private static SitePage Solution() => Page(
        "solution",
        "Une seule base de travail. Toute l'école reste à jour.",
        "Gestionnaire relie les inscriptions, la pédagogie, la scolarité et le pilotage pour éviter les doubles saisies et les fichiers dispersés.",
        "Comment Gestionnaire relie inscriptions, notes, bulletins, emplois du temps et paiements dans un établissement scolaire.",
        1,
        Section("cards", "Commencez par vos données existantes.",
            "Nous reprenons avec vous les élèves, enseignants, classes et matières déjà présents dans vos fichiers. Votre équipe ne repart pas de zéro.",
            new
            {
                variant = "split",
                items = new[]
                {
                    Item("Import de vos fichiers", "Excel, CSV ou Word : les colonnes sont reconnues automatiquement."),
                    Item("Vérification avant la mise en service", "Doublons, classes inconnues et informations manquantes sont signalés."),
                    Item("Formation des responsables", "Direction, censeurs et secrétariat sont formés avant le démarrage.")
                },
                imageUrl = Media(EnrollmentsImageId),
                imageAlt = "Gestion des inscriptions"
            }),
        Section("cards", "Chaque personne retrouve uniquement son travail.",
            "La direction garde une vision globale. Les enseignants voient leurs classes et matières. Le secrétariat gère les dossiers sans accéder aux éléments pédagogiques qui ne le concernent pas.",
            new
            {
                variant = "split",
                items = new[]
                {
                    Item("Proviseur et administration", "Vue d'ensemble, comptes de l'équipe, journal des modifications."),
                    Item("Censeur", "Avancement des notes, emplois du temps, bulletins, procès-verbaux, discipline."),
                    Item("Enseignants", "Leurs classes, leurs matières, leurs notes et les statistiques de chaque séquence."),
                    Item("Secrétariat et comptabilité", "Inscriptions, parents, listes de classe, paiements et trésorerie.")
                },
                imageUrl = Media(TeachersImageId),
                imageAlt = "Enseignants et matières par classe"
            }),
        Section("cards", "La direction décide avec une information fiable.",
            "Effectifs, résultats, recouvrement et activité sont consolidés automatiquement. Les alertes utiles remontent sans produire un nouveau tableau Excel.",
            new
            {
                variant = "split",
                items = new[]
                {
                    Item("Alertes à traiter", "Notes manquantes par matière, élèves en difficulté sur la séquence en cours."),
                    Item("Meilleurs élèves", "Le top de chaque séquence, prêt pour le tableau d'honneur."),
                    Item("Activité récente", "Les dernières inscriptions, saisies et modifications de l'établissement.")
                },
                imageUrl = Media(AlertsImageId),
                imageAlt = "Alertes à traiter, meilleurs élèves et activité récente"
            }),
        Cta("Découvrir en situation", "Voyez ce que Gestionnaire peut simplifier chez vous."));

    private static SitePage Features() => Page(
        "fonctionnalites",
        "Tout le cycle de l'année scolaire, dans un outil cohérent.",
        "Chaque fonction répond à une tâche concrète. Elles partagent les mêmes données pour éviter les oublis, les écarts et les doubles saisies.",
        "Fonctionnalités de Gestionnaire : inscriptions, notes, bulletins, procès-verbaux, emplois du temps, paiements, trésorerie et tableaux de bord.",
        2,
        Section("cards", "", "", new
        {
            variant = "editorial",
            items = new[]
            {
                Group("Admissions et dossiers", "Enregistrez les élèves, leurs responsables, leurs documents et leur affectation sans ressaisie.",
                    "Inscriptions", "Fiches élèves", "Parents et contacts", "Listes de classe"),
                Group("Pédagogie et résultats", "Donnez aux enseignants un espace simple pour saisir les notes et préparer les conseils de classe.",
                    "Saisie rapide, même hors connexion", "Moyennes, rangs et appréciations", "Bulletins et procès-verbaux PDF", "Statistiques de séquence"),
                Group("Organisation scolaire", "Centralisez ce qui rythme la journée de l'établissement et repérez les conflits plus tôt.",
                    "Emplois du temps", "Matières et coefficients", "Absences et discipline", "Trimestres et séquences"),
                Group("Scolarité et pilotage", "Suivez les versements, les échéances et les indicateurs essentiels dans un même tableau de bord.",
                    "Paiements et reçus", "Restes à payer", "Trésorerie", "Journal des modifications")
            }
        }),
        Section("cards", "Une interface familière dès la première utilisation.",
            "Des libellés clairs, des actions visibles et aucun vocabulaire technique inutile. L'équipe peut commencer à travailler après une courte prise en main.",
            new
            {
                variant = "split",
                items = new[]
                {
                    Item("Imports guidés", "Chaque import montre les colonnes reconnues et vérifie les lignes avant d'enregistrer."),
                    Item("Pensé pour le terrain", "Fonctionne sur téléphone et avec une connexion modeste.")
                },
                imageUrl = Media(ReportCardImageId),
                imageAlt = "Bulletin d'un élève, classé par rang"
            }),
        Cta("Découvrir en situation", "Voyez ce que Gestionnaire peut simplifier chez vous."));

    private static SitePage Pricing() => Page(
        "tarifs",
        "Un tarif adapté à la taille de votre établissement.",
        "Pas de modules à acheter séparément. Le nombre d'élèves détermine le tarif, tandis que l'équipe et les fonctionnalités restent complètes.",
        "Tarifs de Gestionnaire : une offre établissement complète, au nombre d'élèves, avec reprise des données, formation et assistance incluses.",
        3,
        Section("cards", "Offre établissement", "Tout Gestionnaire, pour toute votre équipe.", new
        {
            variant = "pricing",
            description = "Une proposition claire est préparée après un échange sur vos effectifs et votre organisation actuelle.",
            listTitle = "Toujours inclus",
            items = new[]
            {
                Item("Utilisateurs illimités", "Direction, censeurs, enseignants et personnel administratif."),
                Item("Toutes les fonctionnalités", "Sans niveau artificiellement bloqué."),
                Item("Reprise de vos données", "Import et vérification avec notre équipe."),
                Item("Formation au démarrage", "Pour les responsables et les utilisateurs clés."),
                Item("Assistance continue", "Une équipe disponible lorsque vous en avez besoin.")
            },
            actionLabel = "Recevoir une proposition",
            actionUrl = "/demo"
        }),
        Section("text", "Ce qui fait varier le prix", "Le nombre d'élèves, et rien de difficile à comprendre.", new
        {
            description = "Nous ne facturons pas chaque enseignant ni chaque bulletin. Tout l'établissement peut travailler sur la plateforme."
        }),
        Cta("Découvrir en situation", "Recevez une proposition adaptée à votre établissement."));

    private static SitePage Questions() => Page(
        "questions",
        "Les réponses utiles avant de commencer.",
        "Installation, reprise des données, sécurité ou accompagnement : voici les points que les établissements nous demandent le plus souvent.",
        "Questions fréquentes sur Gestionnaire : installation, import Excel, connexion lente, rôles, bulletins, paiements et assistance.",
        4,
        Section("faq", "Questions fréquentes", "", new { variant = "list", items = AllFaq() }),
        Cta("Une autre question ?", "Parlez-nous de votre établissement."));

    private static SitePage About() => Page(
        "a-propos",
        "Construire un outil scolaire que l'on utilise vraiment.",
        "Gestionnaire part d'une conviction simple : la technologie doit alléger le travail des équipes éducatives, pas leur demander de devenir techniciennes.",
        "À propos de Gestionnaire : une plateforme pensée avec les collèges et lycées pour réduire les fichiers, les ressaisies et les erreurs.",
        5,
        Section("text", "Notre engagement",
            "Écouter le fonctionnement réel des établissements, puis rendre les tâches importantes plus simples et plus fiables.",
            new { variant = "statement" }),
        Section("cards", "", "", new
        {
            variant = "values",
            items = new[]
            {
                Item("Clarté", "Une action doit être compréhensible sans mode d'emploi interminable."),
                Item("Fiabilité", "Les équipes doivent pouvoir faire confiance aux calculs et aux données affichées."),
                Item("Proximité", "La mise en place et l'assistance restent des échanges entre personnes.")
            }
        }),
        Section("text", "Pourquoi Gestionnaire", "Moins de fichiers. Moins de ressaisies. Plus de temps pour l'école.", new
        {
            description = "Le produit a été pensé pour les collèges et lycées qui travaillent encore avec plusieurs cahiers, feuilles de calcul et échanges informels. Notre rôle est de réunir ces opérations dans un cadre simple, progressif et adapté aux habitudes des équipes."
        }),
        Cta("Découvrir en situation", "Voyez ce que Gestionnaire peut simplifier chez vous."));

    private static SitePage Demo() => Page(
        "demo",
        "Parlons de votre établissement.",
        "En 30 minutes, nous vous montrons les parcours qui correspondent à votre organisation et répondons à vos questions.",
        "Demandez une démonstration personnalisée de Gestionnaire pour votre collège ou lycée. Sans engagement.",
        6,
        Section("demo-form", "", "", new
        {
            kicker = "Démonstration personnalisée",
            highlights = new[]
            {
                "Présentation adaptée à votre établissement",
                "Estimation claire du calendrier et du tarif",
                "Aucun engagement"
            },
            formTitle = "Demander une démonstration",
            fullNameLabel = "Nom complet",
            fullNamePlaceholder = "Votre nom",
            establishmentLabel = "Établissement",
            establishmentPlaceholder = "Nom de l'établissement",
            emailLabel = "Adresse e-mail",
            emailPlaceholder = "direction@votre-ecole.cm",
            studentCountLabel = "Nombre approximatif d'élèves",
            studentCountOptions = new[] { "Moins de 300", "300 à 700", "700 à 1 500", "Plus de 1 500" },
            messageLabel = "Votre besoin principal",
            messagePlaceholder = "Parlez-nous brièvement de votre organisation actuelle.",
            submitLabel = "Envoyer ma demande",
            note = "Réponse sous un jour ouvré. Vos informations restent confidentielles.",
            successTitle = "Demande reçue",
            successHeading = "Merci pour votre message.",
            successMessage = "Notre équipe vous recontactera prochainement pour convenir d'un échange."
        }));

    private static object[] AllFaq() =>
    [
        Faq("Faut-il installer quelque chose ?", "Non. Gestionnaire fonctionne dans un navigateur sur ordinateur, tablette et téléphone."),
        Faq("Pouvons-nous récupérer nos fichiers Excel ?", "Oui. Élèves, enseignants, matières, notes et emplois du temps s'importent depuis Excel, CSV ou Word. Rien n'est enregistré avant une vérification ligne par ligne."),
        Faq("Combien de temps faut-il pour démarrer ?", "Le calendrier dépend du volume de données. La préparation et la formation sont organisées avec un interlocuteur dédié."),
        Faq("La connexion internet doit-elle être très rapide ?", "Non. L'interface est volontairement légère, et la saisie des notes continue même si la connexion coupe un instant."),
        Faq("Les enseignants voient-ils toutes les données ?", "Non. Chaque enseignant voit ses classes et ses matières. Les droits sont adaptés au rôle de chacun et contrôlés par la direction."),
        Faq("Quels rôles peut-on créer ?", "Administrateur, proviseur, censeur, enseignant, secrétariat, comptabilité, surveillant, infirmerie et parent. Une même personne peut cumuler plusieurs rôles."),
        Faq("Comment les bulletins sont-ils produits ?", "Les moyennes, rangs et appréciations sont calculés à partir des notes et des coefficients. Les bulletins s'impriment en PDF par séquence, trimestre ou année, pour un élève ou toute la classe."),
        Faq("Peut-on suivre les paiements de scolarité ?", "Oui. Vous définissez un échéancier par tranches, enregistrez les versements et suivez les restes à payer et le recouvrement par classe."),
        Faq("Sait-on qui a modifié une note ?", "Oui. Le journal des modifications garde la trace de chaque création, modification ou suppression, avec son auteur et sa date."),
        Faq("Comment fonctionne l'assistance ?", "Votre équipe peut contacter notre support pour les questions d'utilisation et les difficultés rencontrées, pendant toute l'année.")
    ];

    private static SitePage Page(string slug, string title, string intro, string metaDescription, int order, params SitePageSection[] sections)
    {
        for (var index = 0; index < sections.Length; index++)
        {
            sections[index].SortOrder = index;
        }

        return new SitePage
        {
            Slug = slug,
            Title = title,
            Intro = intro,
            MetaDescription = metaDescription,
            SortOrder = order,
            Sections = sections.ToList()
        };
    }

    private static SitePageSection Section(string type, string heading, string body, object content) => new()
    {
        Type = type,
        Heading = heading,
        Body = body,
        Content = JsonSerializer.Serialize(content, ContentJsonOptions)
    };

    private static SitePageSection Cta(string kicker, string heading) =>
        Section("cta", heading, "", new { kicker, actionLabel = "Demander une démonstration", actionUrl = "/demo" });

    private static object Item(string title, string description, string? icon = null) => new { title, description, icon };

    private static object Group(string title, string description, params string[] items) => new { title, description, items };

    private static object Role(string title, string eyebrow, string headline, string description, string imageUrl, string imageAlt) =>
        new { title, eyebrow, headline, description, imageUrl, imageAlt, actionLabel = "Voir cette vue en démonstration", actionUrl = "/demo" };

    private static object Faq(string question, string answer) => new { question, answer };

    private static SiteNavigationItem Navigation(string label, string url, int order) =>
        new() { Label = label, Url = url, SortOrder = order };
}
