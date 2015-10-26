using UnityEngine;
using System.Collections;

namespace Pointcube.Utils
{
    public class DbgMsg
    {
        public   enum               verbosity { info = 0, warning = 1, error = 2 }

        private  string             m_msg;
        private  string             m_tag;
        private  System.DateTime    m_date;
        private  verbosity          m_lvl;

        //-------------------------------------------------
        public DbgMsg(string tag, string msg, int verbLevel)
        {
            m_tag = tag;
            m_msg = msg;
            m_date = System.DateTime.Now;
            m_lvl  = (verbosity) verbLevel;
        }

        //-------------------------------------------------
        public DbgMsg(string tag, string msg, verbosity verbLevel)
        {
            m_tag = tag;
            m_msg = msg;
            m_date = System.DateTime.Now;
            m_lvl  = verbLevel;
        }


        //-------------------------------------------------
        public override string ToString()
        {
            string date = m_date.Hour + ":" + m_date.Minute + ":" +
                                                           m_date.Second + ":" + m_date.Millisecond;

            return (date+" | "+m_tag+" : "+m_msg);
        }

        //-------------------------------------------------
        public int GetVerbLvl()
        {
            return (int) m_lvl;
        }

    } // class Debug Message

} // Namespace Pointcube.Utils
