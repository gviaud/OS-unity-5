using UnityEngine;
using System.Collections;

public interface GUIInterface
{	
	void updateGUI(GUIItemV2 itm,int val,bool reset);
	
	void setVisibility(bool b);
	
	void canDisplay(bool b);
	
	bool isOnUI();
	
	bool isVisible();
	
	void CreateGui();
}
