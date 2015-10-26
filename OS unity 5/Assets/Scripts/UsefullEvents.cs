using UnityEngine;
using System.Collections;

public static class UsefullEvents
{
//	Exemple
//	public static event System.Action<Type> 	Nom;
	
	// -- Evénements montage --
	public static event System.Action 			NewMontage;
	
	// -- Evénements paramétrage scène --
	public static event System.Action           ScaleChange;
	
	public static event System.Action<bool> 	NightMode;
	
	// -- Evénements fenêtre, cf. ResChangeListener.cs --
	public static event System.Action           OnResizingWindow;  // Déclenché pendant le resize (plein de fois)
    public static event System.Action           OnResizeWindowEnd; // Déclenché à la fin du resize, donne la différence
                                                                   //    de dimensions entre avant et après le resize.
	
	// -- Evenements Objets --
	public static event System.Action<GameObject,float> UpdateIosShadowScale;	
	public static event System.Action<GameObject> ReinitIosShadow;


    // -- Evénements UI --
    public static event System.Action<bool>     HideGUIforScreenshot;
    public static event System.Action<bool>     HideGUIforBeforeAfter;
	public static event System.Action<bool>		LockGuiDialogBox; //bloque la gui si la dialog box de confirmation est ouverte
	
	public static event System.Action<string,int[]> UpdateUIState;
	
	// -- Evènements Mode2D --
	public static event System.Action<bool>		mode2DStateUpdated;
	
	//--vv--Panneau Aide--vv--
	public static event System.Action			ShowHelpPanel;
	
	//=========================================================================
	#region fireEvents
	
//	public static void fireAction(Type t)
//	{
//		Debug.Log("Authentification Need Fired");
//		if(auth != null)
//			auth(Type t);
//	}
	
	//-----------------------------------------------------
	// Evénements montage
	public static void fireNewMontage()
	{
		Debug.Log("Fire Event > New Montage");
		if(NewMontage != null)
			NewMontage();
	}

	//-----------------------------------------------------
	// Evénements paramétrage scène
	public static void FireScaleChange()
	{
		if(ScaleChange != null)
			ScaleChange();
	}
	
	public static void FireNightMode(bool night)
	{
		if(NightMode != null)
			NightMode(night);
	}
	
	//-----------------------------------------------------
	// Evénements fenêtre
	public static void FireResizingWindow()
	{
		if(OnResizingWindow != null)
			OnResizingWindow();
	}

    public static void FireResizeWindowEnd()
    {
        if(OnResizingWindow != null)
            OnResizeWindowEnd();
    }

	//-----------------------------------------------------
	// Evénements Objets
	public static void FireUpdateIosShadowScale(GameObject go,float factor)
	{
		if(UpdateIosShadowScale != null)
			UpdateIosShadowScale(go,factor);
	}
	//-----------------------------------------------------
	// Evénements Objets
	public static void FireReinitIosShadow(GameObject go)
	{
		if(ReinitIosShadow != null)
			ReinitIosShadow(go);
	}

    //-----------------------------------------------------
    // Evénements Objets

    public static void FireHideGUIforScreenshot(bool hide)
    {
        if(HideGUIforScreenshot != null)
            HideGUIforScreenshot(hide);
    }

    public static void FireHideGUIforBeforeAfter(bool hide)
    {
        if(HideGUIforBeforeAfter != null)
            HideGUIforBeforeAfter(hide);
    }
	
	public static void FireLockGuiDialogBox(bool isLocked)
	{
		if(LockGuiDialogBox != null)
			LockGuiDialogBox(isLocked);
	}
	
	public static void FireUpdateUIState(string ui,int[] indexs)
	{
		//Debug
		//Debug.Log("UI>"+ui);
		//foreach(int i in indexs)
		//{
		//	Debug.Log(">>>"+i);	
		//}
		
		if(UpdateUIState != null)
			UpdateUIState(ui,indexs);
	}
	
	public static void FireUpdateMode2DState(bool active)
    {
        if(mode2DStateUpdated != null)
            mode2DStateUpdated(active);
    }
	
	public static void FireShowHelpPanel()
    {
        if(ShowHelpPanel != null)
            ShowHelpPanel();
    }
	#endregion
	
} // OneShotEvents
