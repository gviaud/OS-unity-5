import System.IO;

static var platforms = [BuildTarget.StandaloneWindows];
//static var platforms = [BuildTarget.iPhone];
//static var platforms = [BuildTarget.Android];

static var extensions = ["win"];
//static var extensions = ["ios"];
//static var extensions = ["android"];

@MenuItem("Assets/Build AssetBundle From Selection - Track dependencies (win)")
static function ExportResource () 
{
	// Bring up save panel
	var path = EditorUtility.SaveFilePanel ("Save Resource", "", "New Resource", "unity3d");
	
	Debug.Log(path);
	/*if (path.Length != 0)
	{
		for (var i = 0; i < platforms.length; ++i)
		{  
			// Build the resource file from the active selection.
			var selection = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
			
			var filename = Path.GetFileNameWithoutExtension (path);
			
			var fileIndex = path.IndexOf  (filename);
			var platformPath = path.Substring (0, fileIndex) + filename + "_" + extensions [i] + ".unity3d";
			//Temporaire, plus rapide car on n'a que l'export ios a faire.
			//var platformPath = path.Substring (0, fileIndex) + filename + ".unity3d";

			BuildPipeline.BuildAssetBundle(Selection.activeObject, 
										   selection, 
										   platformPath, 
										   BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets,
										   platforms [i]);
										   	
			Selection.objects = selection;
		}
	}*/
}

@MenuItem("Assets/Build AssetBundle From Selection - No dependency tracking")
static function ExportResourceNoTrack () 
{
	// Bring up save panel
	var path = EditorUtility.SaveFilePanel ("Save Resource", "", "New Resource", "unity3d");
	if (path.Length != 0)
	{
		for (var i = 0; i < platforms.length; ++i)
		{  
			// Build the resource file from the active selection.
			var selection = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
			
			var filename = Path.GetFileNameWithoutExtension (path);
			
			var fileIndex = path.IndexOf  (filename);
			var platformPath = path.Substring (0, fileIndex) + filename + "_" + extensions [i] + ".unity3d";
			
			// Build the resource file from the active selection.
			BuildPipeline.BuildAssetBundle(Selection.activeObject, 
										   Selection.objects, 
										   platformPath, 
										   BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets,
										   platforms [i]);
		}
	}
}