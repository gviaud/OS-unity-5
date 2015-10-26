using UnityEngine;
using System.Collections.Generic;

// Represent a text with multilanguages support
public class MultiLanguages
{
	protected Dictionary<string, string> _languages = new Dictionary<string, string> ();
	
	protected string _default = "";
	
	public MultiLanguages ()
	{}
	
	public MultiLanguages (MultiLanguages languages)
	{
		_languages = new Dictionary<string, string> (languages._languages);
		_default = languages._default;
	}
	
	public void SetLanguages (MultiLanguages languages)
	{
		_languages = new Dictionary<string, string> (languages._languages);
		_default = languages._default;
	}
	
	public Dictionary<string, string> GetLanguagesDictionary()
	{
		return _languages;
	}
	public void SetLanguagesDictionary(Dictionary<string, string> dic)
	{
		 _languages = dic;
	}
	
	public void SetDefaultLanguage (string lang)
	{
		if (_languages.ContainsKey (lang))
			_default = lang;
	}
	
	public string GetDefaultLanguage ()
	{
		return _default;	
	}
	
	public string GetDefaultText ()
	{
		if (_languages.ContainsKey (_default))
			return _languages[_default];
		else
			return "";
	}
	
	public void AddText (string lang, string text, bool defaultLang)
	{
		//Debug.Log("key : " + lang + " | " + text);
		_languages[lang] = text;
		
		if (defaultLang)
			_default = lang;
	}
	
	public string GetText (string lang)
	{
		if (_languages.ContainsKey (lang))
			return _languages[lang];
		else if(lang.Contains("_parent"))
			return "";
		else
			return /*_languages[_default]*/GetDefaultText ();
	}

    public int GetLangCount()
    {
        return _languages.Values.Count;
    }
}