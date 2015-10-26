using UnityEngine;
using System.Collections;

// Source : http://forum.unity3d.com/threads/12031-create-random-colors

namespace Pointcube.Utils
{

/**
*   A color in HSV space
*/
public class ColorHSV
{
    private float mH;
    private float mS;
    private float mV;
    private float mA;

    /**
    * Construct with alpha
    */
    public ColorHSV(float h, float s, float v, float a)
    {
        mH = h;
        mS = s;
        mV = v;
        mA = a;
    }

    /**
    * Construct without alpha (which defaults to 1)
    */
    public ColorHSV(float h, float s, float v) : this(h, s, v, 1.0f)
    {
    }

    /**
    * Create from an RGBA color object
    */
    public ColorHSV(Color colorRGB)
    {
        float min = Mathf.Min(Mathf.Min(colorRGB.r, colorRGB.g), colorRGB.b);
        float max = Mathf.Max(Mathf.Max(colorRGB.r, colorRGB.g), colorRGB.b);
        float delta = max - min;

        // value is our max color
        mV = max;

        // saturation is percent of max
        if(!Mathf.Approximately(max, 0))
            mS = delta / max;
        else
        {
            // all colors are zero, no saturation and hue is undefined
            mS = 0;
            mH = -1;
            return;
        }
 
        // grayscale image if min and max are the same
        if(Mathf.Approximately(min, max))
        {
            mV = max;
            mS = 0;
            mH = -1;
            return;
        }
       
        // hue depends which color is max (this creates a rainbow effect)
        if( colorRGB.r == max )
            mH = ( colorRGB.g - colorRGB.b ) / delta;         // between yellow & magenta
        else if( colorRGB.g == max )
            mH = 2 + ( colorRGB.b - colorRGB.r ) / delta; // between cyan & yellow
        else
            mH = 4 + ( colorRGB.r - colorRGB.g ) / delta; // between magenta & cyan
 
        // turn hue into 0-360 degrees
        mH *= 60;
        if(mH < 0 )
            mH += 360;
    }

    /**
    * Return an RGBA color object
    */
    public Color ToColor()
    {
        // no saturation, we can return the value across the board (grayscale)
        if(mS == 0 )
            return new Color(mV, mV, mV, mA);
 
        // which chunk of the rainbow are we in?
        float sector = mH / 60;
       
        // split across the decimal (ie 3.87 into 3 and 0.87)
        int i = (int) Mathf.Floor(sector);
        float f = sector - i;

        float v = mV;
        float p = v * ( 1 - mS );
        float q = v * ( 1 - mS * f );
        float t = v * ( 1 - mS * ( 1 - f ) );
       
        // build our rgb color
        Color color = new Color(0, 0, 0, mA);
       
        switch(i)
        {
            case 0:
                color.r = v;
                color.g = t;
                color.b = p;
                break;
            case 1:
                color.r = q;
                color.g = v;
                color.b = p;
                break;
            case 2:
                color.r  = p;
                color.g  = v;
                color.b  = t;
                break;
            case 3:
                color.r  = p;
                color.g  = q;
                color.b  = v;
                break;
            case 4:
                color.r  = t;
                color.g  = p;
                color.b  = v;
                break;
            default:
                color.r  = v;
                color.g  = p;
                color.b  = q;
                break;
        }
       
        return color;
    } // ToColor() (RGBA)
   
    /**
    * Format nicely
    */
//    public string ToString()
//    {
//        return string.Format("h: {0:0.00}, s: {1:0.00}, v: {2:0.00}, a: {3:0.00}", h, s, v, a);
//    }

    //-----------------------------------------------------
    public float GetH()
    {
        return mH;
    }

    //-----------------------------------------------------
    public float GetS()
    {
        return mS;
    }

    //-----------------------------------------------------
    public float GetV()
    {
        return mV;
    }

    //-----------------------------------------------------
    public float GetA()
    {
        return mA;
    }

    //-----------------------------------------------------
    public void SetH(float newH)
    {
        mH = newH;
    }

    //-----------------------------------------------------
    public void SetS(float newS)
    {
        mS = newS;
    }

    //-----------------------------------------------------
    public void SetV(float newV)
    {
        mV = newV;
    }

    //-----------------------------------------------------
    public void SetA(float newA)
    {
        mA = newA;
    }

} // class ColorHSV

} // namespace Pointcube.Utils
