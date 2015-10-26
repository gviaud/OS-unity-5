//-----------------------------------------------------------------------------
// Assets/Scripts/functions/editionImage/MaskPixRGB.cs - 05/2012 KS
// Attaché à aucun élément de la scène Unity

using UnityEngine;

//-----------------------------------------------------------------------------
// Classe complétant la classe MaskPix pour l'outil matière, qui nécessite
// l'information de couleur en plus (X, Y, RGBA)
// TODO Utiliser des bytes au lieu de int
public class MaskPixRGB :  MaskPix
{
    protected byte m_r;
    protected byte m_g;
    protected byte m_b;

    public MaskPixRGB(ushort x, ushort y, byte r, byte g, byte b, byte a) : base(x, y, a)
    {
        m_r = r;
        m_g = g;
        m_b = b;
    }

    // RGBA : 0 -> 255
    public MaskPixRGB(int x, int y, int r, int g, int b, int a) : base(x, y, a)
    {
        m_r = (byte)r;
        m_g = (byte)g;
        m_b = (byte)b;
    }

    // RGBA : 0f -> 1f
    public MaskPixRGB(int x, int y, float r, float g, float b, float a) : base(x, y, a)
    {
        m_r = (byte)(r*255);
        m_g = (byte)(g*255);
        m_b = (byte)(b*255);
    }

    public MaskPixRGB(int x, int y, Color col) : this(x, y, col.r, col.g, col.b, col.a)
    { }

    public MaskPixRGB(int x, int y, float a) : this(x, y, (byte)0, (byte)0, (byte)0, a)
    { }

    public MaskPixRGB(MaskPixRGB model) : this(model.m_x, model.m_y,
                                               model.m_r, model.m_g, model.m_b, model.m_alpha)
    { }

    public void SetColor(Color col)
    {
        m_r = (byte)(col.r*255);
        m_g = (byte)(col.g*255);
        m_b = (byte)(col.b*255);
        m_alpha = (byte)(col.a*255);
    }

    public Color GetColor()
    {
        return new Color((float)m_r/255f, (float)m_g/255f, (float)m_b/255f, (float)m_alpha/255f);
    }

    public override System.Object DoClone()
    {
        return (System.Object) new MaskPixRGB(this.m_x, this.m_y, this.m_r, this.m_g, this.m_b,
                                                                                      this.m_alpha);
    }
   /* public System.Object Clone()
    {
        return (System.Object) new MaskPixRGB(this.m_x, this.m_y, this.m_r, this.m_g, this.m_b, this.m_alpha);
    }*/
    
} // class MaskPixRGB
