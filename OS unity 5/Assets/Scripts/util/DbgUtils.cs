using UnityEngine;
using System.Collections.Generic;

namespace Pointcube.Utils
{
    public static class DbgUtils
    {
        private static List<DbgMsg> s_log         = new List<DbgMsg>();
		private static bool         s_guiDebug    = false;
//        private static readonly string DEBUGTAG = "DbgUtils - ";
		
        //-------------------------------------------------
        public static void Log(string tag, string message)
        {
            s_log.Add(new DbgMsg(tag, message, DbgMsg.verbosity.info));
        }

        //-------------------------------------------------
        public static void LogWarning(string tag, string message)
        {
            s_log.Add(new DbgMsg(tag, message, DbgMsg.verbosity.warning));
        }

        //-------------------------------------------------
        public static void LogError(string tag, string message)
        {
            s_log.Add(new DbgMsg(tag, message, DbgMsg.verbosity.error));
        }

        //-------------------------------------------------
        public static void ClearLog()
        {
            s_log.Clear();
        }

        //-------------------------------------------------
        public static string GetLog(int verbLvl)
        {
            string output = "";
            foreach(DbgMsg msg in s_log)
            {
                if(msg.GetVerbLvl() == verbLvl)
                    output += (msg.ToString() + "\n");
            }

            return output;
        }

		//-------------------------------------------------
		public static bool GUIdebugEnabled()
		{
			return s_guiDebug;	
		}

		//-------------------------------------------------
		public static void EnableGUIdebug()
		{
			s_guiDebug = true;
		}

        //-------------------------------------------------
        public static void DisableGUIdebug()
        {
            s_guiDebug = false;
        }

        //-------------------------------------------------
        public static void ToggleGUIdebug()
        {
            s_guiDebug = !s_guiDebug;
        }
    } // class Debug Utils

} // Namespace Pointcube.Utils
