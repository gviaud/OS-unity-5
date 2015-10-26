using UnityEngine;
using System.Collections;

namespace Pointcube.Utils
{
    // Lib de gestion des couleurs HSV
    public class ColorHSVutils
    {
        //-----------------------------------------------------
        public static Color SetHueTo(Color col, float newHue)
        {
            ColorHSV hsv = new ColorHSV(col);
            hsv.SetH(newHue);
            return hsv.ToColor();
        }
    
        //-----------------------------------------------------
        public static float GetValue(Color col)
        {
            ColorHSV hsv = new ColorHSV(col);
            return hsv.GetV();
        }
    
        //-----------------------------------------------------
        public static Color SetValueTo(Color col, float newValue)
        {
            ColorHSV hsv = new ColorHSV(col);
            hsv.SetV(newValue);
            return hsv.ToColor();
        }
    
        //-----------------------------------------------------
        public static Color LowerValue(Color col, float stren)
        {
            ColorHSV hsv = new ColorHSV(col);
            hsv.SetV(hsv.GetV() - (hsv.GetV()*stren));
            return hsv.ToColor();
        }
    
        //-----------------------------------------------------
        public static Color Desaturate(Color col, float stren)
        {
            ColorHSV hsv = new ColorHSV(col);
            hsv.SetS(hsv.GetS() - (hsv.GetS()*stren));
            return hsv.ToColor();
        }

    } // class ColorHSVutils

} // namespace Pointcube.Utils
