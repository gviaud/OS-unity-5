using Pointcube.InputEvents;

namespace Pointcube.Global
{
    public static class PC
    {
        // -- Debug --
        public static readonly bool   DEBUG        = true;     // Affiche les msg de debug des scripts qui sont en DEBUG
        public static readonly bool   DEBUGALL     = false;    // Affiche tous les messages de debug
        public static readonly string MISSING_REF  = " reference not set, please set it in the inspector.";
        public static readonly string MISSING_RES  = " resource not found, does it actually exist ?";
        public static readonly string WRONG_OBJ    = " Script attached to wrong GameObject. It should be attached to : ";

        // -- Input --
#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
        public static CursorInput In =  TouchInput.GetInstance();
#elif (UNITY_STANDALONE_OSX || UNITY_EDITOR)
        public static CursorInput In = MouseInput.GetInstance();
#else
        public static CursorInput In = WinTouchInput.GetInstance();
#endif
//        public static CursorInput In  = InT;
//        public static bool        tIn = true;

        // -- Aides contextuelles --
        public static CtxHelpManager ctxHlp;                 // Initialisé dans CtxHelpManager.Awake()

        // -- Transitions animées --
        public static readonly int  T_ANIM_LEN   = 20;       // Durée standard des transitions animées
        public static readonly int  T_ANIM_SHORT = 30;       // Durée des transitions animées plus courtes

    }
}