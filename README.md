# Merjane — traitement des commandes

Cette application ASP.NET Core expose une API qui traite les commandes et met à jour la disponibilité des produits qu’elles contiennent. Le projet sert également de support à un exercice de refactoring autour des règles métier, de la séparation des responsabilités et des tests.

## Fonctionnement

L’endpoint de traitement charge une commande, applique la règle correspondant au type de chaque produit, puis enregistre les changements de stock.

- **NORMAL** : si le produit est en stock, une unité est retirée. En rupture, un délai est annoncé lorsqu’un délai de réapprovisionnement est renseigné.
- **SEASONAL** : le produit peut être vendu pendant sa saison. En rupture, un délai est annoncé s’il permet une disponibilité avant la fin de la saison; sinon, l’indisponibilité est signalée. Un produit hors saison n’est pas vendu et son indisponibilité est signalée.
- **EXPIRABLE** : le produit peut être vendu jusqu’au jour de sa date d’expiration. Après cette date, son stock disponible est mis à zéro.

Les notifications sont demandées par les règles métier via `INotificationService`. L’implémentation actuelle de `NotificationService` est un point d’extension dont les méthodes sont vides; aucun envoi externe n’est effectué.

## API

### Traiter une commande

```http
POST /orders/{orderId}/processOrder
```

Réponses principales :

- `200 OK` : la commande a été traitée; le corps contient son identifiant (`id`).
- `404 Not Found` : aucune commande ne correspond à l’identifiant.
- `422 Unprocessable Entity` : les données métier sont invalides, notamment si un type de produit n’est pas reconnu ou si une date requise manque.

En environnement de développement, la documentation Swagger est disponible au démarrage de l’application.

## Organisation du code

Le code de production se trouve dans `Refacto.DotNet.Controllers/` :

- `Controllers/` : endpoints HTTP et conversion des résultats en réponses.
- `Services/` : `OrderService` orchestre le chargement et l’enregistrement des commandes; `ProductService` applique les règles produit.
- `Interfaces/` : contrats de services, dont le service de notification.
- `Entities/` : entités persistées par Entity Framework Core.
- `Data/` : contexte Entity Framework Core.
- `Dtos/Orders/` : modèles de réponse de l’API.
- `Exceptions/` et `Filters/` : erreurs métier et leur traduction en réponses HTTP.

`Refacto.DotNet.Controllers.Tests/` contient les tests unitaires des services et les tests d’intégration du contrat HTTP.

## Exécution

Prérequis : SDK .NET compatible avec la cible `net7.0`.

Depuis la racine du dépôt :

```bash
dotnet run --project Refacto.DotNet.Controllers/Refacto.DotNet.Controllers.csproj
```

L’application utilise actuellement une base de données **InMemory** configurée dans `Program.cs`. Les données ne sont donc pas conservées comme elles le seraient dans une base de production.

## Tests

Pour exécuter toute la suite :

```bash
dotnet test
```

## Consignes de l’exercice

- Ignorer les migrations de base de données.
- Ne pas modifier les classes portant le commentaire `// WARN: Should not be changed during the exercise`.
