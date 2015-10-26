using UnityEngine;
using System.Collections;
using System.Threading;

namespace Pointcube.Utils
{
    public static class _3Dutils
    {
		
		//-----------------------------------------------------
		public static Vector3 RaycastFromScreen(Vector2 screenCoords)
		{
			return RaycastFromScreen(screenCoords.x, screenCoords.y);
		}
		
        //-----------------------------------------------------
        public static Vector3 RaycastFromScreen(float pixelX, float pixelY)
        {
            RaycastHit newhitm = new RaycastHit();
            Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3(pixelX, pixelY, 0f)),
                                                                        out newhitm, Mathf.Infinity,9);
            Vector2 detectedPoint = new Vector2();
            if(newhitm.rigidbody)
            {
                detectedPoint.x = newhitm.point.x;
                detectedPoint.y = newhitm.point.z;
            }
            return detectedPoint;
        }
		
		//-----------------------------------------------------
        public static Vector3 ScreenMoveTo3Dmove(Vector2 screenMove, Vector3 objLocation, bool lockY = true)
        {
            Vector3 output = new Vector3(objLocation.x, objLocation.y, objLocation.z);
            Vector3 objScreenPos3 = Camera.main.WorldToScreenPoint(objLocation- new Vector3(0,objLocation.y,0));//QUICKFIX du pb de déplacement 'haie de buis"
            Vector2 objScreenPos2 = new Vector2(objScreenPos3.x, objScreenPos3.y);
            objScreenPos2 += screenMove;

            RaycastHit newhitm = new RaycastHit();
            Physics.Raycast(Camera.main.ScreenPointToRay(objScreenPos2), out newhitm, Mathf.Infinity);
            if(newhitm.rigidbody != null || newhitm.collider != null)
            {
                output = newhitm.point;
                if(lockY)
                    output.y = objLocation.y;
            }
            return output;
        }
		
        //-----------------------------------------------------
        public static Vector3 ScreenMoveTo3Dmove(Vector2 screenMove, Vector3 objLocation, int layerMask, bool lockY = true)
        {
            Vector3 output = new Vector3(objLocation.x, objLocation.y, objLocation.z);
            Vector3 objScreenPos3 = Camera.main.WorldToScreenPoint(objLocation- new Vector3(0,objLocation.y,0));//QUICKFIX du pb de déplacement 'haie de buis"
            Vector2 objScreenPos2 = new Vector2(objScreenPos3.x, objScreenPos3.y);
            objScreenPos2 += screenMove;

            RaycastHit newhitm = new RaycastHit();
            Physics.Raycast(Camera.main.ScreenPointToRay(objScreenPos2), out newhitm, Mathf.Infinity, layerMask);
            if(newhitm.rigidbody != null || newhitm.collider != null)
            {
                output = newhitm.point;
                if(lockY)
                    output.y = objLocation.y;
            }
            return output;
        }

        //-----------------------------------------------------
        // Retourne la boîte englobante de la hiérarchie donnée
        public static Bounds getMeshBounds(GameObject root)
        {
            //Get all the mesh filters in the tree.
            MeshFilter[] filters = root.GetComponentsInChildren<MeshFilter> ();

            //Construct an empty bounds object w/o an extant.
            Bounds bounds = new Bounds (Vector3.zero, Vector3.zero);
            bool firstTime = true;

            //Debug.Log("filters "+filters.Length);

            //For each mesh filter...
            foreach (MeshFilter mf in filters)
            {
                //Pull its bounds into the overall bounds.  Bounds are given in
                //the local space of the mesh, but we want them in world space,
                //so, tranform the max and min points by the xform of the object
                //containing the mesh.
                Vector3 maxWorld = mf.transform.TransformPoint (mf.mesh.bounds.max);
                Vector3 minWorld = mf.transform.TransformPoint (mf.mesh.bounds.min);

                //If no bounds have been set yet...
                if (firstTime)
                {
                    firstTime = false;
                    //Set the bounding box to encompass the current mesh, bounds,
                    //but in world coordinates.
                    //center
                    bounds = new Bounds ((maxWorld + minWorld) / 2, maxWorld - minWorld);
                    //extent
                //We've started a bounding box.  Make sure it ecapsulates
                } else {
                    //the current mesh extrema.
                    bounds.Encapsulate (maxWorld);
                    bounds.Encapsulate (minWorld);
                }
            }
            //Return the bounds just computed.
            return bounds;
        } // getMeshBounds()

        //-----------------------------------------------------
        //Renvoie la diagonale de la bounding box
        public static float GetBoundDiag(Bounds bbox)
        {
            Vector3 dim = bbox.size;
               // Calculer la taille idéale du champ de vision de la caméra (+projecteur) : la plus petite possible contenant l'objet
        
            float  bigDim = 0, bigDim2=0, bigI=0;
               for(int i=0; i<3; i++)      // Récupérer la dimension la plus grande
               {
                   if( (dim[i]>0 ? dim[i] : -dim[i]) > bigDim)
                   {
                       bigDim = (dim[i]>0 ? dim[i] : -dim[i]);
                       bigI = i;
                   }
               }
               for(int i=0; i<3; i++)      // Récupérer la 2e dimension la plus grande
               {
                   if( (dim[i]>0 ? dim[i] : -dim[i]) > bigDim2 && i!=bigI)
                       bigDim2 = (dim[i]>0 ? dim[i] : -dim[i]);
               }
        
        //       Debug.Log("IOSBigDim = "+bigDim+", BigDim2="+bigDim2);
               bigDim = bigDim/2;// * scl;
               bigDim2 = bigDim2/2;// * scl;
        
               return Mathf.Sqrt(bigDim*bigDim + bigDim2*bigDim2);
        }

    } // class _3Dutils

} // Namespace Pointcube.Utils
