using UnityEngine;
using System.Collections;

public class CtxHelpTest : MonoBehaviour
{
    private CtxHelpManager m_ctxHelpMan;
    private bool           m_helpOn = true;

	//-----------------------------------------------------
	void Awake()
    {
	    m_ctxHelpMan = this.GetComponent<CtxHelpManager>();

       /* if(!PlayerPrefs.HasKey("HelpPopup"))
        {
            PlayerPrefs.SetInt("HelpPopup", 1);
            Debug.Log("Test CTX HELP : key HelpPopup not found, creating");
        }
        else
            Debug.Log("Test CTX HELP : key HelpPopup existing, value="+PlayerPrefs.GetInt("HelpPopup"));*/
	}
	
    //-----------------------------------------------------
	void OnGUI()
    {
        if(m_ctxHelpMan != null)
        {
            string[] choiceList = m_ctxHelpMan.GetPanelIDs();

            for(int i=0; i<choiceList.Length; i++)
            {
                if(GUI.Button(new Rect(50, (20*i)+50, 200, 20), choiceList[i]))
                    m_ctxHelpMan.ShowCtxHelp(choiceList[i]);
            }
        }

        bool helpOn = GUI.Toggle(new Rect(Screen.width - 120, 50, 100, 20), m_helpOn, "Help");
        if(m_helpOn != helpOn)
        {
            m_helpOn = helpOn;
            PlayerPrefs.SetInt("HelpPopup", m_helpOn ? 1 : 0);
        }
	} // OnGUI()

} // CtxHelpTest
