using UnityEngine;
using System.Collections;

public class PergolaAutoFeetEvents : MonoBehaviour
{
	public static event System.Action 			rebuild;
	public static event System.Action<int> 		selectedModuleChange;
	public static event System.Action<bool> 	bladesDirChange;
	public static event System.Action<bool> 	needScreenExtension;
	public static event System.Action<string> 	toggleUIVisibility;
	public static event System.Action<string> 	pergolaTypeChange;
	public static event System.Action<bool>		nightModeChange;
	
	#region fireEvents
	public static void FireRebuild()
	{
		if(rebuild != null)
			rebuild();
	}
	
	public static void FireSelectedModuleChanged(int i)
	{
		if(selectedModuleChange != null)
			selectedModuleChange(i);
	}
	
	public static void FireBladesDirChange(bool b)
	{
		if(bladesDirChange != null)
			bladesDirChange(b);
		
//		FireRebuild();
	}
	
	public static void FireNeedScreenExtension(bool b)
	{
		if(needScreenExtension != null)
			needScreenExtension(b);
	}
	
	public static void FireToggleUIVisibility(string s)
	{
		if(toggleUIVisibility!=null)	
			toggleUIVisibility(s);
	}
	
	public static void FirePergolaTypeChange(string s)
	{
		if(pergolaTypeChange!=null)	
			pergolaTypeChange(s);
	}
	
	public static void FireNightModeChange(bool b)
	{
		if(nightModeChange!=null)	
			nightModeChange(b);
	}
	#endregion
}
