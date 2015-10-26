using UnityEngine;
using System.Collections;

public class affichage : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
		// This will reset to default POT Language
		TextManager.LoadLanguage(null);
		 
		// This will load filename-en.po
		TextManager.LoadLanguage("strings_en");
		 
		// Considering the PO file has a msgid "Ola Mundo" and msgstr "Hello World" this will print "Hello World"
		Debug.Log(TextManager.GetText("test.title"));
		Debug.Log(TextManager.GetText("test.msg"));
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
