using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;



/// <summary>
/// Inter process.
/// Communication entre processus et chargement de scènes grâce aux PlayerPrefs
/// </summary>
public  class InterProcess :MonoBehaviour{

    private  Montage _montage;
    private  GUIStart _guistart;
    private PluginPhotoManagerGUI _plugPhoto;
    private string _askLoad=null;
    private bool _askAutosave = false;
    private GUIDialogBox _cb;
    private bool m_loadingScene=false;

    void Awake(){
        _montage = GameObject.Find("MainScene").GetComponent<Montage>();
        _guistart = GameObject.Find("MainScene").GetComponent<GUIStart>();
        _cb = GetComponent<GUIDialogBox>();
        _plugPhoto = GameObject.Find("MainScene").GetComponent<PluginPhotoManagerGUI>();
		
#if UNITY_ANDROID	
        //s'il y à un fichier autosave.cub enregistré, affichage boîte de dialogue proposant de charger cette scène
		if (PlayerPrefs.GetInt("autosave") == 1)
        {
            Debug.Log(" autosave à 1");
            string path = usefullData.SavePath + "autosave" + usefullData.SaveNewFileExtention;
            if (File.Exists(path))
            {
                _guistart.enabled = false;
                _askAutosave = true;
                _cb.showMe(true, GUI.depth);
                _cb.setBtns(TextManager.GetText("GUIMenuRight.Yes"),
                      TextManager.GetText("GUIMenuRight.No"));
                _cb.setText(TextManager.GetText("GUIMenuRight.AskAutoSave1") + "\n" + TextManager.GetText("GUIMenuRight.AskAutoSave2"));

            }
            PlayerPrefs.SetInt("autosave", 0);
        }
#endif
    }

	
    void Start()
    {
        //OSX : ouverture de la scène dont le chemin est noté dans openFile.txt
#if UNITY_STANDALONE_OSX
        PlayerPrefs.SetString("fileToOpen", " ");
		string filepath=usefullData.SavePath.Replace("/Caches/Pointcube/OneShotRevolution/save/","/Application Support/Oneshot3D/openFile.txt");
		if (System.IO.File.Exists(filepath)){
			Debug.Log("The file exists");
			try{
				System.IO.StreamReader myFile =new System.IO.StreamReader(filepath);
				string myString = myFile.ReadToEnd();
				PlayerPrefs.SetString("fileToOpen", myString);
				myFile.Close();
				System.IO.File.Delete(filepath);
			}
			catch(System.Exception e){
				Debug.Log("Error reading openFile.txt :"+e.Message);
			}
		}	
#endif
        //Update playerprefs pour signifier qu'une instance de Oneshot est déja lancé (utile pour windows seulement pour l'instant)
        PlayerPrefs.SetInt("oneshotRunning", 1);
        PlayerPrefs.Save();
      

    }
	




    //Sur pause : sauvegarde la scène dans autosave.cub si l'utilisateur n'est pas dans le menu de départ
    void OnApplicationPause(bool pauseStatus)
    {
#if UNITY_ANDROID		
        if (pauseStatus)
        {
            Debug.Log("Paused");
            if (!_guistart.Started)
            {
                Debug.Log("Saving the scene");
              if( _montage.SaveWithName("autosave")){
	                PlayerPrefs.SetInt("autosave", 1);
	                PlayerPrefs.Save();
				}
				else{
					PlayerPrefs.SetInt("autosave", 0);
               	 	PlayerPrefs.Save();
				}
            }
            else
            {
                PlayerPrefs.SetInt("autosave", 0);
                PlayerPrefs.Save();
            }
        }
#endif		
    }
    
    
    
    
    void Update()
    {
        //Boîte de dialogue de l'autosave
        if (_askAutosave && _cb.isVisible())
        {
            if (_cb.getConfirmation())
            {
                string path = usefullData.SavePath + "autosave" + usefullData.SaveNewFileExtention;
                Debug.Log("Chargement scène située : " + path);
                _montage.IsLoading = true;
                _montage.AskToLoadScene(path);
                _guistart.showStart(false);
                _askAutosave = false;
                _guistart.enabled = true;
                _cb.showMe(false, GUI.depth);
            }
            if (_cb.getCancel())
            {
                _cb.showMe(false, GUI.depth);
                _askAutosave = false;
                _guistart.enabled = true;
            }
        }
        else
        {


#if UNITY_ANDROID

            //Changement du background si c'est nécéssaire
            string imageToOpen = PlayerPrefs.GetString("imageToOpen");

            if (imageToOpen != null && imageToOpen != " ")
            {
                Debug.Log("image toOpen not null");
                Debug.Log("montage.IsLoading=" + _montage.IsLoading);
                //Pas d'image retournée après prise de photo ou gallery 
                if (imageToOpen.Equals("OS3D_ImgCancel"))
                {
                    Debug.Log("img choice cancel");
                    _plugPhoto.ImageChoiceCancel();
                    PlayerPrefs.SetString("imageToOpen", " ");
                    PlayerPrefs.Save();
                }
                else
                {
                    //Si photo bien retourné, on vérifie qu'une scène n'est pas en cours de chargement
                    if (!_montage.IsLoading)
                    {
                        Debug.Log("imagetoOpen!=null => changement background");
                        StartCoroutine(_plugPhoto.AlbumChoiceSuccessPathOnly(imageToOpen));
                        PlayerPrefs.SetString("imageToOpen", " ");
                        PlayerPrefs.Save();
                        m_loadingScene = false;
                    }
                }

            }
#endif

            //Boite de dialogue demandant à l'utilisateur s'il veut abandonner sa scène pour charger la nouvelle
            if (_askLoad != null && _cb.isVisible())
            {
                if (_cb.getConfirmation())
                {
                    _cb.showMe(false, GUI.depth);
                    _montage.AskToLoadScene(_askLoad);
                    _askLoad = null;
                }

                if (_cb.getCancel())
                {
                    _cb.showMe(false, GUI.depth);
                    _askLoad = null;
                }
            }

            //Lecure de la clé fileToOpen
            string fileToOpen = PlayerPrefs.GetString("fileToOpen");
            if (fileToOpen != null && fileToOpen != " ")
            {
                if (System.IO.File.Exists(fileToOpen) && fileToOpen.Contains(".cub"))
                {
                    Debug.Log("filetoopen!=null => opening the scene from file " + fileToOpen);
                    //Si le menu de démarrage est affiché, chargement de la scène et masquage du menu
                    if (_guistart.Started)
                    {
                        _montage.AskToLoadScene(fileToOpen);
                        _guistart.showStart(false);
                    }
                    else
                    {
                        //Sinon affichage de la boite de dialogie
                        _askLoad = fileToOpen;
                        _cb.showMe(true, GUI.depth);
                        _cb.setBtns(TextManager.GetText("GUIMenuRight.OK"),
                              TextManager.GetText("GUIMenuRight.cancel"));

                        _cb.setText(TextManager.GetText("GUIMenuRight.CloseSceneToOpen") + "\n" + System.IO.Path.GetFileName(_askLoad) + " ?");

                    }
                }
                PlayerPrefs.SetString("fileToOpen", " ");
                PlayerPrefs.Save();


                //string filepath=usefullData.SavePath.Replace("/save/","/");
                //if(System.IO.File.Exists(filepath+"openFile.txt")){
                //System.IO.File.Delete(filepath+"openFile.txt");
                //}
            }
        }
    }
	

    void OnDestroy()
    {
        PlayerPrefs.SetInt("oneshotRunning", 0);
        //PlayerPrefs.SetInt("autosave", 0);
    }


	/// <summary>
	/// Fonction ne fonctionnant pas sous IOS
	/// Récupère le fichier passé en ligne de commande et met à jour les playerPref afin de charger la scène
	/// </summary>
	/// <returns>
	/// return true si une autre instance de Oneshot susceptible d'ouvrir le fichier est déja ouverte
	/// </returns>
    public static bool FileToOpen()
    {
#if UNITY_STANDALONE_WIN
        if (PlayerPrefs.GetInt("oneshotRunning")==1)
        {
            Debug.Log("oneshot running =1");
            Debug.Log("so fileToOpen now put to " + System.Environment.GetCommandLineArgs()[1]);
            PlayerPrefs.SetString("fileToOpen", System.Environment.GetCommandLineArgs()[1]);
            PlayerPrefs.Save();
            return true;
        }
#endif
        return false;
    }
	
	/// <summary>
	/// A appeller lorque Oneshot se ferme
	/// </summary>
    public static void Stop()
    {
        PlayerPrefs.SetInt("oneshotRunning", 0);
        PlayerPrefs.SetInt("autosave", 0);

    }

    void OnGUI()
    {

    }


    /*
    private static void run()
    {
        Debug.Log("interprocess server running");
        while(!stop){
            string fileToOpen = PlayerPrefs.GetString("fileToOpen");
            if (fileToOpen != null)
            {
                Debug.Log("filetoopen!=null => opening the scene from file "+fileToOpen);
                montage.AskToLoadScene(fileToOpen);
                PlayerPrefs.SetString("fileToOpen", null);
                PlayerPrefs.Save();
            }
        }
    }
    */


}
