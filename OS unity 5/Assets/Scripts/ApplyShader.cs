using UnityEngine;
using System.Collections;


public class ApplyShader : MonoBehaviour 

{

    private Material[] thisMaterial;

    private string[] shaders;

 

    void Start ()
    {
		//foreach(Transform t in this.transform)
		foreach(Transform t in GetComponentsInChildren<Transform>())
		{
			if((t.GetComponent<Renderer>()!=null)&&(t.GetComponent<Renderer>().sharedMaterials.Length>0))
			{
			
				thisMaterial = t.GetComponent<Renderer>().sharedMaterials;   
				
				
				shaders =  new string[thisMaterial.Length];

        

			        for( int i = 0; i < thisMaterial.Length; i++)
			
			        {
						if(thisMaterial[i]!=null)
							shaders[i] = thisMaterial[i].shader.name;
			        }           
			
			        
			
			        for( int i = 0; i < thisMaterial.Length; i++)
			        {  
						if(thisMaterial[i]!=null)
						{
							if (shaders[i].StartsWith("Custom/"))
							{
								if (usefullData.lowTechnologie)
								{
								//	Texture alpha = new Texture2D(1,1);
								//	bool hastextureAlpha=false; 
									if(shaders[i].Contains("2SidedTreeLeavesFast"))
									{
									/*	if(thisMaterial[i].HasProperty("_DecalTex"))
										{
											alpha = thisMaterial[i].GetTexture("_DecalTex");
											hastextureAlpha = true;
										}*/
										//thisMaterial[i].shader = Shader.Find("Transparent/Diffuse");
									
										Shader shad = (Shader)Resources.Load("shaders/Custom_alphaColor"); 
										thisMaterial[i].shader = shad;
									/*	if((hastextureAlpha) && (alpha!=null))
											thisMaterial[i].mainTexture = alpha;*/
									
									}
									else if(shaders[i].Contains("Multiply"))
									{
										thisMaterial[i].shader = Shader.Find("Diffuse");									
									}
									else
									{
										string name = shaders[i].Replace("/","_");
										Shader shad = (Shader)Resources.Load("shaders/"+name); 
											thisMaterial[i].shader = shad; //Shader.Find(shaders[i]); 									
									}	
								}
								else
								{
									string name = shaders[i].Replace("/","_");
									Shader shad = (Shader)Resources.Load("shaders/"+name); 
										thisMaterial[i].shader = shad; //Shader.Find(shaders[i]);  
								}
							}
							else if (shaders[i].StartsWith("Pointcube/"))
							{
								string name = shaders[i].Replace("/","_");
								Shader shad = (Shader)Resources.Load("shaders/"+name); 
									thisMaterial[i].shader = shad; //Shader.Find(shaders[i]);  
							}
							else
							{
								string name = shaders[i].Replace("/","_");
								if(name == "Hidden_InternalErrorShader")
								{
									Shader shad = (Shader)Resources.Load("shaders/Custom_alphaColor"); 
									thisMaterial[i].shader = shad; //Shader.Find(shaders[i]);  
									print ("Name : " + name + "|||||||||  Name2 : " + thisMaterial[i].shader .name);
								}
							}
						}
			        } 
				
			}
			
		}
		if(this.GetComponent<Renderer>()!=null)
		{
			
			thisMaterial = this.GetComponent<Renderer>().sharedMaterials;   
			shaders =  new string[thisMaterial.Length];
        

		       for( int i = 0; i < thisMaterial.Length; i++)
		
		       {
		
		           shaders[i] = thisMaterial[i].shader.name;
		
		       }           
		
		        
		
		       for( int i = 0; i < thisMaterial.Length; i++)
		
		       {
		
		           thisMaterial[i].shader = Shader.Find(shaders[i]);   
		        } 
		} 

    }

    

 

}