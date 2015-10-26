=== Pool Designer ===

== Description ==

Module int�gr� � OneShot3D Revolution permettant de g�n�rer une piscine 3D
� partir d'un trac� 2D pr�d�fini ou cr�� par l'utilisateur.

== Hierarchie ==

PoolDesigner
    + 2DDrawer --> fonctions de dessin des diff�rents �l�ments pour affichage
	|   + ArcDrawer
	|	+ EdgeDrawer
	|	+ PolygonDrawer --> classe de dessin du polygone, g�re les interactions utilisateurs 
	|                       avec le polygone (click point ou segment, insertion, ajout de points, etc.)
	|
	+ Controller
	|   + ArchedPoint2       --> classe de repr�sentation d'une courbe 
	|   + Edge2              --> classe de repr�sentation d'un segment
	|   + PlanTransformation --> classe g�rant les transformations global du plan (translation, �chelle)
	|   + Point2             --> classe de repr�sentation d'un point
	|   + Polygon            --> classe de repr�sentation d'un polygone
	|   + Snapper            --> classe g�rant les diff�rents type de magn�tisme
	|
	+ Geometry Tools --> Outils g�om�trique 2D (repr�sentation de droite, intersections, etc.)
	|   + GeometryTools 
	|
	+ GUI
	|   + PoolDesignerUI --> interface principale
	|
	+ Mapping --> classe d'application de coordonn�es de mapping
	|   + LinerScatteringMapper --> mapping d�grad� pour le liner
	|   + PlannarMapper
	|   + PolygonExtruderMapper
	|   + UVChannel
	| 
	+ Models
	|   + LoopedList     --> liste permettant un acc�s � ses �l�ments en dehors des limites
	|   + Point2Data     --> classe repr�sentant un point du polygone pour la sauvegarde et polygones pr�d�finis dans le script
	|   + PolygonRawData --> LoopedList de Vector2
	|
	+ PolygonOperation --> op�rations r�alisables sur un polygone (voir commentaires dans les fichiers)
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
	
== Cr�ation de la librairie ==

1. Cr�er un Gameobject vide.
2. Ajouter le script Function_PoolDesigner
3. Remplir le champs _uiName avec "PoolDesignerUI"
4. Pour cr�er une piscine pr�d�fini, dans le champs pointsData (polygone de r�f�rence pour g�n�rer la piscine) : 
    a. Pr�ciser le nombre de point du polygone dans size
    b. Pour chaque point reporter les informations de position, type de point et rayon.

On peut �ventuellement effectuer les �tapes suivantes mais ce n'est pas obligatoire.
5. Ajouter un RigidBody
6. Ajouter un MeshCollider
	
N-B : Ne pas remplir les autres champs.