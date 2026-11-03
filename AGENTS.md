# Conventions de code — équipe Merjane

Conventions appliquées à tout le projet `Refacto.DotNet.Controllers/`.

## Robustesse

- **Fail fast** : une donnée invalide n'est jamais ignorée silencieusement. Un produit dont le
  type n'est pas reconnu est une erreur de données : lever une exception métier explicite
  (par exemple `UnknownProductTypeException`) plutôt que de passer au produit suivant.
- Les exceptions métier sont traduites en réponse HTTP par un filtre d'exception global
  (`IExceptionFilter`, 422 Unprocessable Entity), jamais dans les controllers.

## Conception

- YAGNI : pas de classe ni d'interface par type de produit. La logique métier d'un agrégat vit
  dans un seul service (`ProductService` pour les produits, `OrderService` pour les commandes) ;
  la variation selon le type de produit se traite dans ce service avec un `switch` exhaustif.
- Les entités EF Core ne connaissent pas la date courante : toute règle qui dépend de la date du
  jour vit dans les services.

## Documentation

- Chaque classe et chaque méthode publique porte un commentaire de documentation XML (`///`) qui
  décrit son contrat : rôle, paramètres, valeur de retour, exceptions levées.

## Tests

- Pyramide de tests : les règles métier sont testées par des tests unitaires des services ;
  les tests d'intégration du controller vérifient uniquement le contrat HTTP (statut, corps de
  la réponse, traduction des erreurs).
- Les cas d'une même règle sont regroupés dans un test paramétré (`[Theory]` avec
  `[InlineData]`) par type de produit.
