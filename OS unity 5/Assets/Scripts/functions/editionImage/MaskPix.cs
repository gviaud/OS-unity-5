//-----------------------------------------------------------------------------
// Assets/Scripts/functions/editionImage/MaskPix.cs - 04/2012 KS
// Attaché à aucun élément de la scène Unity

//-----------------------------------------------------------------------------
// Classe représentant un pixel, utilisée par EraserMask (outil gomme) et
// InpaintMain (outil inpainting). (X, Y, A seulement)
public class MaskPix : System.ICloneable
{
    protected ushort m_x, m_y;
    protected byte   m_alpha;

    public MaskPix(ushort x, ushort y, byte alpha)
    {
        m_x     = x;
        m_y     = y;
        m_alpha = alpha;
    }

    public MaskPix(int x, int y, int alpha)
    {
        m_x     = (ushort)x;
        m_y     = (ushort)y;
        m_alpha = (byte)alpha;
    }

    public MaskPix(int x, int y, float alpha)
    {
        m_x     = (ushort)x;
        m_y     = (ushort)y;
        m_alpha = (byte)(alpha*255);
    }

    public int GetX()
    {
        return (int) m_x;
    }

    public int GetY()
    {
        return (int) m_y;
    }

    public float GetA()
    {
        return (float)m_alpha/255f;
    }

    public System.Object Clone()
    {
		return DoClone();
      //  return (System.Object) new MaskPix(this.m_x, this.m_y, this.m_alpha);		
    }
	
	public virtual System.Object DoClone()
	{
        return (System.Object) new MaskPix(this.m_x, this.m_y, this.m_alpha);		
	}
    
} // class MaskPix
