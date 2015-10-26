using UnityEngine;
using System.Collections;
using System.IO;
using System;

#if UNITY_EDITOR
    using UnityEditor;
//elif UNITY_STANDALONE_WIN
//    using System.Windows.Forms;
#endif

namespace Pointcube.Utils
{
    public static class IOutils
    {
        private static readonly string s_tmpSavePath = UnityEngine.Application.persistentDataPath+"/tmp/";

        // -- Debug --
        private static readonly string DEBUGTAG = "IOutils ";

//        #region Platform_Specific
//        //-------------------------------------------------
//        public enum platform {win, unix};
//        public static platform GetCurPlatform()
//        {
//            if(UnityEngine.Application.platform == RuntimePlatform.WindowsEditor ||
//               UnityEngine.Application.platform == RuntimePlatform.WindowsPlayer)
//                return platform.win;
//            else if(UnityEngine.Application.platform == RuntimePlatform.OSXEditor     ||
//                    UnityEngine.Application.platform == RuntimePlatform.OSXPlayer     ||
//                    UnityEngine.Application.platform == RuntimePlatform.IPhonePlayer  ||
//                    UnityEngine.Application.platform == RuntimePlatform.Android)
//                return platform.unix;
//            else
//            {
//                Debug.LogError(DEBUGTAG+"GetCurPlatform : Unsupported platform, assuming it's UNIX");
//                return platform.unix;
//            }
//        }
//
//        //-------------------------------------------------
//        private static string s_platformSep = "";
//        public static string GetPlatformSeparator()
//        {
//            if(s_platformSep.Length == 0)
//                s_platformSep = (GetCurPlatform() == platform.win) ? "\\" : "/";
//
//            return s_platformSep;
//        }
//        #endregion

        #region IO_montage
        //-------------------------------------------------
        // usefullData.SavePath +m_client+"/"+ name + usefullData.SaveNewFileExtention
        public static string CreateTmpMontageFile(MemoryStream stream)
        {
            int count=Directory.Exists(s_tmpSavePath)? Directory.GetFiles(s_tmpSavePath).Length : 0;
            string autoPath = s_tmpSavePath+TextManager.GetText("Files.TmpName")+"_"+count+
                                                                   usefullData.SaveNewFileExtention;
            CreateMontageFile(stream, autoPath);
            return autoPath;
        }

        //-------------------------------------------------
        // usefullData.SavePath +m_client+"/"+ name + usefullData.SaveNewFileExtention
        public static void CreateMontageFile(MemoryStream stream, string path)
        {
            string dir = path.Substring(0, path.LastIndexOf('/'));
//            string filename = path.Substring(path.LastIndexOf('/')+1);
			FileStream fs=null;
			try{
            if(!Directory.Exists(dir))
	                Directory.CreateDirectory(dir);
	
	            fs = new FileStream(path, FileMode.Create);
	            fs.Write (stream.ToArray(), 0, stream.ToArray().Length);
				Debug.Log ("Montage File sucessfully created");
			}
			catch(System.Exception e){
				Debug.Log ("errorxd creating montage file :"+e.ToString());
			}
			finally{
				if(fs == null)
				{
					GameObject.Find("MainScene").GetComponent<GUIMenuMain>().messError = 7.0f;
				}
				else
           		 fs.Close();
			}
//            DbgUtils.Log(DEBUGTAG, "Created Montage File : "+path);
        }

        //-------------------------------------------------
        // Copie un .cub externe dans les dossiers clients de OS3D
        // Parameter filePath : the client folder the montage should be copied to.
        // Returns the imported file path (ie. filePath + montageName)
        public static string ImportMontageFile(string filePath)
        {
            string sOpenPath = "";
//            DbgUtils.Log (DEBUGTAG+"ImportMontageFile", filePath);
            #if UNITY_EDITOR
                //Debug.Log("UNITY_EDITOR : ");
                string importMontage;
                if(usefullData._edition == usefullData.OSedition.Lite)
                    importMontage = TextManager.GetText("Dialog.ImportMontageFileExpress");
                else
                    importMontage = TextManager.GetText("Dialog.ImportMontageFile");

                sOpenPath = EditorUtility.OpenFilePanel(importMontage,"","cub");
                if(sOpenPath.Length<4)
                    return filePath;

                string sOpenPathName = sOpenPath.Substring(sOpenPath.LastIndexOf('/')+1);
                filePath += "/" + sOpenPathName;
                filePath = SuffixIfAlreadyExists(filePath);

                File.Copy(sOpenPath, filePath, true);

            #elif UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN
                string[] extensions = new string[] {"cub"};
//			    DbgUtils.Log (DEBUGTAG+"ImportMontageFile", "Asking Path...");
                sOpenPath = NativePanels.OpenFileDialog(extensions);
//			    DbgUtils.Log (DEBUGTAG+"ImportMontageFile", "Asked Path="+sOpenPath);

                if(sOpenPath.Length >= 4)
                {
                    Debug.Log("file ready to open: " + sOpenPath);
#if UNITY_STANDALONE_WIN
					if((sOpenPath != ""))
                    {
                        sOpenPath = sOpenPath.Replace("\\","/"); 
					}
#endif
					
                    string sOpenPathName = sOpenPath.Substring(sOpenPath.LastIndexOf('/')+1);
			       // DbgUtils.Log (DEBUGTAG+"ImportMontageFile", "FileName="+sOpenPathName);
                    filePath += "/" + sOpenPathName;
                    filePath = SuffixIfAlreadyExists(filePath);
			        //DbgUtils.Log (DEBUGTAG+"ImportMontageFile", "SuffixedPath="+filePath);
                    File.Copy(sOpenPath, filePath, true);
                }
//            elif UNITY_STANDALONE_WIN
//                System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
//                openFileDialog.InitialDirectory = UnityEngine.Application.dataPath;
//                openFileDialog.Filter = TextManager.GetText("Dialog.MontageFiles")+"|*.cube;*.cub";
//                openFileDialog.FilterIndex = 1;
//                openFileDialog.Title = TextManager.GetText("Dialog.ImportMontage");
//    
//                if(openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
//                {
//                    if((openFileDialog.FileName != ""))
//                    {
//                        sOpenPath = openFileDialog.FileName.Replace("\\","/");
////                        DbgUtils.Log(DEBUGTAG+"ImportMontageFile", "openpath = "+sOpenPath);
//                        string sOpenPathName = sOpenPath.Substring(sOpenPath.LastIndexOf('/')+1);
//                        filePath += "/" + sOpenPathName;
//                        filePath = SuffixIfAlreadyExists(filePath);
//
//                        File.Copy(sOpenPath, filePath, true);
//                    }
//                }
            #endif

            return filePath;
        } // ImportMontage()

        //-----------------------------------------------------
        // Copie un .cub à partir du dossier client
        // Paramètre montagePath : chemin du montage à exporter (avec ou sans l'extension ".cub")
        public static void ExportMontageFile(string montagePath)
        {
//            DbgUtils.Log (DEBUGTAG+"ExportMontageFile", "Exporting : "+montagePath);
            if(montagePath.EndsWith(usefullData.SaveNewFileExtention) ||
               montagePath.EndsWith(usefullData.SaveFileExtention))
                montagePath = montagePath.Substring(0, montagePath.LastIndexOf("."));

            string saveMontageFile;
            string montageFile;
            if(usefullData._edition == usefullData.OSedition.Lite)
            {
                saveMontageFile = TextManager.GetText("Dialog.SaveMontageFileExpress");
                montageFile = TextManager.GetText("Dialog.MontageFilesExpress");
            }
            else
            {
                saveMontageFile = TextManager.GetText("Dialog.SaveMontageFile");
                montageFile = TextManager.GetText("Dialog.MontageFiles");
            }

            string savePath = AskPathDialog(saveMontageFile, "cub", montageFile);
//            DbgUtils.Log(DEBUGTAG+"ExportMontageFile", "Dialog path = "+savePath);

            if(savePath.EndsWith(usefullData.SaveNewFileExtention))
                savePath = savePath.Substring(0, savePath.LastIndexOf("."));
            if(savePath != "")
            {
                if(File.Exists(savePath+usefullData.SaveNewFileExtention))
                    File.Delete(savePath+usefullData.SaveNewFileExtention);

                string extension = "";
                if(File.Exists(montagePath+usefullData.SaveFileExtention))
                {
                    extension = usefullData.SaveFileExtention;
                    File.Copy(montagePath+extension, savePath+extension);
//                    DbgUtils.Log(DEBUGTAG+"ExportMontageFile", "Copied : "+montagePath+extension+" to "+savePath+extension);
                }
                else if(File.Exists(montagePath+usefullData.SaveNewFileExtention))
                {
                    extension = usefullData.SaveNewFileExtention;
                    File.Copy(montagePath+extension, savePath+extension);
//                    DbgUtils.Log(DEBUGTAG+"ExportMontageFile", "Copied : "+montagePath+extension+" to "+savePath+extension);
                }
                else
                {
                    Debug.LogError(DEBUGTAG+"ExportMontageFile : File not found "+montagePath+", can't copy");
//                    DbgUtils.Log(DEBUGTAG+"ExportMontageFile", "File not found "+montagePath+", can't copy");
                }
            }
//            else  DbgUtils.Log (DEBUGTAG+"ExportMontageFile", " Cancelled ! ");
            // else annulation
    
        } // ExportFile(path)

        //-------------------------------------------------
        // Parameter montagePath : chemin du montage à mettre en pièce jointe, avec ou sans extension
        public static void EmailMontageFile(string montagePath)
        {
#if !UNITY_STANDALONE
            if(montagePath.EndsWith(usefullData.SaveNewFileExtention) ||
               montagePath.EndsWith(usefullData.SaveFileExtention))
                montagePath = montagePath.Substring(0, montagePath.LastIndexOf("."));

            string extension = "";
            if(File.Exists(montagePath+usefullData.SaveFileExtention))
                extension = usefullData.SaveFileExtention;
            else if(File.Exists(montagePath+usefullData.SaveNewFileExtention))
                extension = usefullData.SaveNewFileExtention;
            else
            {
                Debug.LogError(DEBUGTAG+"EmailMontageFile : File not found "+montagePath+", can't attach to email");
                return;
            }

//            string name = montagePath.Substring(0, montagePath.LastIndexOf('/'/*GetPlatformSeparator()*/));
//            DbgUtils.Log(DEBUGTAG+"EmailMontage","FILENAME = "+name);

            #if UNITY_IPHONE
                EtceteraManager.setPrompt(true);
                EtceteraBinding.showMailComposerWithAttachment(montagePath+extension, "",
                                                       TextManager.GetText("Dialog.MailTitle")+extension, "",
                                                       TextManager.GetText("Dialog.MailTitle"),
                                                       TextManager.GetText("ExplorerV2.BodyMail"),
                                                       false);
            #elif UNITY_ANDROID
                // EtceteraAndroid.setPrompt(true);
                EtceteraAndroid.showEmailComposer("", "ONESHOT 3D REVOLUTION", "", true, montagePath+extension);
            #endif
#endif
        }

        //-------------------------------------------------
        // Cacher la GUI iPad après envoi d'email + supprimer autosave si besoin
        public static void EndMail(string result)
        {
//            Debug.Log("Mail Sender : "+result);
            #if UNITY_IPHONE
                EtceteraManager.setPrompt(false);
            #endif
         }

        //-------------------------------------------------
        public static void CleanTmpFiles()
        {
            if(Directory.Exists(s_tmpSavePath))
            {
                string[] tmpFiles = Directory.GetFiles(s_tmpSavePath);
//                DbgUtils.Log(DEBUGTAG, " "+tmpFiles.Length+" File(s) to delete");
                foreach(string tmpFile in tmpFiles)
                {
                    File.Delete(tmpFile);
//                    DbgUtils.Log(DEBUGTAG," "+tmpFile+" deleted !");
                }
            }
        }

        //-------------------------------------------------
        // path : chemin avec ou sans extension (.cube ou .cub)
        // output : true si fichier trouvé et supprimé, false si pas trouvé.
        public static bool DeleteMontageFile(string path)
        {
            if(path.EndsWith(usefullData.SaveNewFileExtention) ||
               path.EndsWith(usefullData.SaveFileExtention))
            {
                path = path.Substring(0,path.LastIndexOf("."));
            }

            if(File.Exists(path + usefullData.SaveFileExtention))
            {
                File.Delete(path + usefullData.SaveFileExtention);
//                DbgUtils.Log (DEBUGTAG+"DeleteMontageFile", path+usefullData.SaveFileExtention);
                return true;
            }
            else if(File.Exists(path + usefullData.SaveNewFileExtention))
            {
                File.Delete(path + usefullData.SaveNewFileExtention);
//                DbgUtils.Log (DEBUGTAG+"DeleteMontage", path+usefullData.SaveNewFileExtention);
                return true;
            }
            else
            {
//                DbgUtils.LogError(DEBUGTAG+"DeleteMontage", "cannot find "+path+usefullData.SaveNewFileExtention);
                Debug.LogError(DEBUGTAG+"DeleteMontage : cannot find "+path+usefullData.SaveNewFileExtention);
                return false;
            }
        }
		//-------------------------------------------------
        // path : chemin du dossier
        // output : true si dossier trouvé et renommé, false si pas trouvé.
        public static bool RenameDirectory(string path,string newName)
        {			
			string oldName = path.Substring(path.LastIndexOf('/')+1);
			string newPath = path.Replace(oldName,newName);
			
			if(Directory.Exists(path))
			{
				Directory.Move(path,newPath);
				return true;
			}
			else
				return false;			
        }
		
        //-------------------------------------------------
        // path : chemin avec ou sans extension (.cube ou .cub)
        // output : bytes du fichier dans un MemoryStream, null si fichier inexistant
        public static MemoryStream LoadMontageFile(string path)
        {
            MemoryStream output = null;

            if(path.EndsWith(usefullData.SaveNewFileExtention) ||
               path.EndsWith(usefullData.SaveFileExtention))
            {
                path = path.Substring(0, path.LastIndexOf("."));
            }

            if(File.Exists(path + usefullData.SaveFileExtention))
                output = new MemoryStream(File.ReadAllBytes(path + usefullData.SaveFileExtention));
            else if(File.Exists(path + usefullData.SaveNewFileExtention))
                output = new MemoryStream(File.ReadAllBytes(path + usefullData.SaveNewFileExtention));
            else
            {
//                DbgUtils.LogError(DEBUGTAG+"LoadMontageFile","Cannot find file "+path+" (either .cub or .cube)");
                Debug.LogError(DEBUGTAG+"LoadMontageFile : Cannot find file "+path+" (either .cub or .cube)");
            }

            return output;
        }


        #endregion

        #region IO_img
        public static void SaveImageFile(Texture2D img, string path)
        {
			byte[] imgBytes = img.EncodeToJPG();
            System.IO.File.WriteAllBytes(path, imgBytes);
        }

        #endregion

        //-------------------------------------------------
        // Fonction de récuperation d'un path (saveFileDialogBox)
        public static string AskPathDialog(string dialogTitle, string fileExtension,
                                                                                string fileExtDescr)
        {
            string sOpenPath = "";
            #if UNITY_EDITOR && UNITY_STANDALONE_WIN
                //Debug.Log("UNITY_EDITOR : ");
                sOpenPath = EditorUtility.SaveFilePanel(dialogTitle,"","", fileExtension);
            #elif UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN
//                DbgUtils.Log (DEBUGTAG+"AskPathDialog", "Asking mac...");
                sOpenPath = NativePanels.SaveFileDialog(TextManager.GetText("Files.TmpName"),
                                                                                     fileExtension);
//                DbgUtils.Log (DEBUGTAG+"AskPathDialog", "Asked : "+sOpenPath);
//           elif UNITY_STANDALONE_WIN && !UNITY_EDITOR
//                System.Windows.Forms.SaveFileDialog openFileDialog = new System.Windows.Forms.SaveFileDialog();
//                openFileDialog.InitialDirectory = UnityEngine.Application.dataPath;
//                //openFileDialog.Filter = "Fichiers de montage OneShot3D REVOLUTION "(*.cub)|*.cub"; // TODO traduction
//                openFileDialog.Filter = fileExtDescr+"|*."+fileExtension+"|All Files|*.*";
//                openFileDialog.FilterIndex = 1;
//                openFileDialog.Title = dialogTitle;   // TODO traduction
//                //openFileDialog.RestoreDirectory = false;
//                if(openFileDialog.ShowDialog() == DialogResult.OK)
//                {
//                    if((openFileDialog.FileName != ""))
//                    {
//                        sOpenPath = openFileDialog.FileName;
//                        //return sOpenPath;
//                    }
//                    else { sOpenPath = ""; } //ANULATION
//                }
//                else { sOpenPath = ""; } //ANNULATION
            #endif
            return sOpenPath;
        }

        //-------------------------------------------------
        private static string SuffixIfAlreadyExists(string path)
        {
//            DbgUtils.Log(DEBUGTAG+"Suffixing ",path);
            string filename = path.Substring(path.LastIndexOf('/')+1);
//            DbgUtils.Log(DEBUGTAG+"Suffixing : name=",filename);
            string filedir  = path.Substring(0, path.LastIndexOf('/'));
//            DbgUtils.Log(DEBUGTAG+"Suffixing : dir=",filedir);
            string fileext  = filename.Substring(filename.LastIndexOf('.'));
//            DbgUtils.Log(DEBUGTAG+"Suffixing : ext=",fileext);
            filename = filename.Substring(0, filename.LastIndexOf('.'));
//            DbgUtils.Log(DEBUGTAG+"Suffixing : name no ext=",filename);

            int i = 1;
            string num = "";
//            DbgUtils.Log(DEBUGTAG+"Suffixing : checking ",filedir+"/"+filename+num+fileext);
            while(File.Exists(filedir+"/"+filename+num+fileext))
            {
                i++;
                num = (i==1)? "" : "("+i.ToString()+")";
            }

            string output = filedir+"/"+filename+num+fileext;

//            DbgUtils.Log(DEBUGTAG+"return : ",output);
            return output;

        } // SuffixIfAlreadyExists()

    } // class IO Utils

} // Namespace Pointcube.Utils
