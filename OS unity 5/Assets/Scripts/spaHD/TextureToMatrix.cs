using UnityEngine;
using System.Collections;

using Pointcube.Utils;

//=============================================================================
// Script à attacher à l'objet de l'eau : waterSpec
public class TextureToMatrix : MonoBehaviour
{
    private Material    mMatSpaWater;                   // Matériau de l'eau (qui contient la texture à traiter)
    private Texture2D   mLastTexture;                   // Dernière texture traitée

    private  float[,]   mMatrixTex;                     // Matrice correspondant à la texture
    private  Vector4    mMatrixTmp;

    // -- Static --
    private readonly static string sShaderName = "Pointcube/spaHDWater_iOS_disp";
    private readonly static int    sMatrixDim  = 8;

    // -- Debug et erreurs --
    private readonly static string DEBUGTAG    = "TextureToMatrix : ";
    private readonly static string ERROR_MAT   = " No material assigned to object renderer, or bad shader assigned to material.";

	//-----------------------------------------------------
	void Start ()
    {
        mMatrixTex = new float[sMatrixDim,sMatrixDim];
        mMatrixTmp = new Vector4();

        mMatSpaWater = this.GetComponent<Renderer>().material;
        if(mMatSpaWater == null || !mMatSpaWater.shader.name.Equals(sShaderName))
            { DeactivateWithLogError(ERROR_MAT); return; }

    } // Start()

    //-----------------------------------------------------
    private void DeactivateWithLogError(string msg)
    {
        Debug.LogError(DEBUGTAG+msg+" Deactivating Script.");
        this.enabled = false;
    }

	//-----------------------------------------------------
	void FixedUpdate()
    {
		if(usefullData.lowTechnologie)
			return;
        Texture2D newTexture = (Texture2D) mMatSpaWater.GetTexture("_BumpMap");
        if(newTexture != mLastTexture)
        {
            Color col;
            // Transformer la nouvelle image en matrice
            for(int i=0; i<sMatrixDim; i++)
            {
                for(int j=0; j<sMatrixDim; j++)
                {
                    col = newTexture.GetPixel(newTexture.width/sMatrixDim*i, newTexture.height/sMatrixDim*j);
                    mMatrixTex[i,j] = 1f-ColorHSVutils.GetValue(col);
                    if(j < 4)
                    {
                        mMatrixTmp[j] = mMatrixTex[i, j];
                        if(j==3)
                            mMatSpaWater.SetVector("_DispVect0"+i, mMatrixTmp);
                    }
                    else // if(j>=4)
                    {
                        mMatrixTmp[j-4] = mMatrixTex[i, j];
                        if(j==7)
                            mMatSpaWater.SetVector("_DispVect1"+i, mMatrixTmp);
                    } // Note : généraliser ça avec un modulo 4 ou un truc comme ça, si besoin d'une matrice plus grande
                    
                } // pour chaque j

            } // pour chaque i
            mLastTexture = newTexture;

        } // if texture has changed

	} // FixedUpdate()


}   // class TextureToMatrix
