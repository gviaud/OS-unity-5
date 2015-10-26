=== Pool Designer ===

== Description ==

Module intégré à OneShot3D Revolution permettant de générer une piscine 3D
à partir d'un tracé 2D prédéfini ou créé par l'utilisateur.

== Hierarchie ==

PoolDesigner
    + 2DDrawer --> fonctions de dessin des différents éléments pour affichage
	|   + ArcDrawer
	|	+ EdgeDrawer
	|	+ PolygonDrawer --> classe de dessin du polygone, gère les interactions utilisateurs 
	|                       avec le polygone (click point ou segment, insertion, ajout de points, etc.)
	|
	+ Controller
	|   + ArchedPoint2       --> classe de représentation d'une courbe 
	|   + Edge2              --> classe de représentation d'un segment
	|   + PlanTransformation --> classe gérant les transformations global du plan (translation, échelle)
	|   + Point2             --> classe de représentation d'un point
	|   + Polygon            --> classe de représentation d'un polygone
	|   + Snapper            --> classe gérant les différents type de magnétisme
	|
	+ Geometry Tools --> Outils géométrique 2D (représentation de droite, intersections, etc.)
	|   + GeometryTools 
	|
	+ GUI
	|   + PoolDesignerUI --> interface principale
	|
	+ Mapping --> classe d'application de coordonnées de mapping
	|   + LinerScatteringMapper --> mapping dégradé pour le liner
	|   + PlannarMapper
	|   + PolygonExtruderMapper
	|   + UVChannel
	| 
	+ Models
	|   + LoopedList     --> liste permettant un accès à ses éléments en dehors des limites
	|   + Point2Data     --> classe représentant un point du polygone pour la sauvegarde et polygones prédéfinis dans le script
	|   + PolygonRawData --> LoopedList de Vector2
	|
	+ PolygonOperation --> opérations réalisables sur un polygone (voir commentaires dans les fichiers)
	|   + MergedPolygon
	|   + MutuallyVisibleVertices
	|   + PolygonExtruder
	|   + PolygonLofter
	|   + PolygonOperation
	|   + PoolGenerator
	|   + RimGenerator
	|   + RimSubdivider
	|   + SideWalkExtruder
	|   + Triangulator
	
    
	+ Scripts/functions/Function_PoolDesigner
	
== Création de la librairie ==

1. Créer un Gameobject vide.
2. Ajouter le script Function_PoolDesigner
3. Remplir le champs _uiName avec "PoolDesignerUI"
4. Pour créer une piscine prédéfini, dans le champs pointsData (polygone de référence pour générer la piscine) : 
    a. Préciser le nombre de point du polygone dans size
    b. Pour chaque point reporter les informations de position, type de point et rayon.

On peut éventuellement effectuer les étapes suivantes mais ce n'est pas obligatoire.
5. Ajouter un RigidBody
6. Ajouter un MeshCollider
	
N-B : Ne pas remplir les autres champs.