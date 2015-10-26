using System;
using System.IO;
using System.Collections;
public interface Function_OS3D
{
	string GetFunctionName();
	string GetFunctionParameterName();
	
	int GetFunctionId();
	
	void DoAction();
	
	//  sauvegarde/chargement
	
	void save(BinaryWriter buf);
	
	void load(BinaryReader buf);
	
	void setGUIItem(GUIItemV2 _guiItem);
	
	//Set L'ui si besoin
	
	void setUI(FunctionUI_OS3D ui);
	
	//public void setVisible();
	
	//similaire au Save/Load mais utilisé en interne d'un objet a un autre (swap)
	ArrayList getConfig();
	
	void setConfig(ArrayList config);
	

}
