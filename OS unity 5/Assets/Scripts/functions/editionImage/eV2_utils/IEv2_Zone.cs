using UnityEngine;
using System.Collections.Generic;

//-----------------------------------------------------
public struct GrassV2zone
{
    public List<MaskPixRGB> m_pixels;
    public Vector4          m_bounds;
    public int              m_textureID;
    public Texture2D        m_texGen;

    public GrassV2zone(List<MaskPixRGB> pixels, float minX, float maxX, float minY, float maxY,
                                                                                      int tex = -1)
    {
        m_pixels  = pixels;
        m_bounds  = new Vector4(minX, maxX, minY, maxY);
        m_textureID = tex;
        m_texGen = null;
    }

    public GrassV2zone(List<MaskPixRGB> pixels, Vector4 bounds, int tex = -1)
    {
        m_pixels = pixels;
        m_bounds = bounds;
        m_textureID = tex;
        m_texGen = null;
    }

    public GrassV2zone(GrassV2zone model)
    {
        m_pixels = new List<MaskPixRGB>();
        for(int i=0; i<model.m_pixels.Count; i++)
            m_pixels.Add(new MaskPixRGB(model.m_pixels[i]));

        m_bounds = model.m_bounds;
        m_textureID = model.m_textureID;
        m_texGen = model.m_texGen;
    }
} // struct GrassV2zone

public interface IEv2_Zone
{
    void          Draw(float cursorX, float cursorY, bool drawTmp);
    void          Draw(float cursorX, float cursorY, bool drawTmp, Rect bgImgRect);
    void          SetMainColor(Color newColor);
    Color         GetMainColor();
    List<MaskPix> Rasterize(Rect bgRect);
    GrassV2zone   RasterizeRGB(Rect bgRect);
    float[]       GetSaveData();
    void          LoadFromData(float[] data);
    IEv2_Zone     Clone();

} // Ev2_Zone
