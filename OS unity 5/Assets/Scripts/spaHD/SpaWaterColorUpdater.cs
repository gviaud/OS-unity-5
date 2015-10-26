using UnityEngine;
using System.Collections;

//=============================================================================
// Classe pour mettre à jour la couleur du shader d'eau et adapter l'intensité
// des spots lumineux des spas (-> luumière plus intense pour coques foncées).
public class SpaWaterColorUpdater : MonoBehaviour
{
	private Transform         mSpa;                           // GameObject racine du spa
	private Material          mLinerMat;                      // Matériau de la cique du spa
	
	private Function_SpaConfigLight mConfLight;               // Script de configuration de la lumière du spa
	private Function_SpaConfigWater mConfWater;               // Script de configuration de l'eau du spa
	
	private string            mOldLinerTextureName;           // Nom de la texture de la coque à l'update précédent
	
	//-----------------------------------------------------
	private static readonly string     sLinerObjectName      = "coque";     // nom de l'objet coque du Spa     /!\ A respecter dans la scène
	
	private static readonly string     DEBUGTAG              = "SpaWaterColorUpdater : ";
	
	//-----------------------------------------------------
	// Initialisation
	void Start()
	{
		mLinerMat = null;
		
		// -- Récupération des références vers les objets à manipuler -- 
		mSpa = transform;
        Transform child;
        for(int i=0; i<mSpa.GetChildCount(); i++)
        {
			child = mSpa.GetChild(i);
			if(child.name == sLinerObjectName)
				mLinerMat = child.GetComponent<Renderer>().material;                  // Coque
        } // pour chaque sous-objet du spa
		
		mConfLight = mSpa.GetComponent<Function_SpaConfigLight>();
		
		// -- Si un objet est absent, afficher une erreur et désactiver le script --
        if(mLinerMat == null)
            { DeactivateWithLogError(sLinerObjectName);          return; }
        if(mConfLight == null)
            { DeactivateWithLogError("Function_SpaConfigLight"); return; }
		
		mOldLinerTextureName = mLinerMat.GetTexture("_MainTex").name;
	} // Start()
	
	//-----------------------------------------------------
    private void DeactivateWithLogError(string objectName)
    {
        Debug.LogError(DEBUGTAG+" object \""+objectName+"\" not found. Deactivating Script.");
        this.enabled = false;
    }
	
	//-----------------------------------------------------
	void Update ()
	{
		// -- Si changement de texture du matériau de la coque, mettre à jour le spa --
		if(!mOldLinerTextureName.Equals(mLinerMat.GetTexture("_MainTex").name) && mLinerMat.GetTexture("_MainTex").name != "")
		{
			string name = mLinerMat.GetTexture("_MainTex").name;
			
			// -- Récupération de la couleur moyenne de la texture actuelle (en hexa dans le nom du fichier) --
			float r = System.Int32.Parse(name.Substring(name.Length-6, 2), System.Globalization.NumberStyles.HexNumber);
			float g = System.Int32.Parse(name.Substring(name.Length-4, 2), System.Globalization.NumberStyles.HexNumber);
			float b = System.Int32.Parse(name.Substring(name.Length-2, 2), System.Globalization.NumberStyles.HexNumber);
			Color c = new Color(r/255f, g/255f, b/255f);
			//Debug.Log(DEBUGTAG+"-------------------------------- R="+r+", G="+g+", B="+b);

			mConfLight.UpdateWaterColor(c);    // MAJ de la couleur de l'eau
			mConfLight.UpdateSpotIntensity(c); // MAJ de l'intensité des spots lumineux
			
			mOldLinerTextureName = name;
		}
	} // Update()
	
} // class SpaWaterColorUpdater
