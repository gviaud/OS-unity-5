using UnityEngine;
using System.Collections;
using System.Threading;

namespace Pointcube.Utils
{
    public static class ImgUtils
    {
        #region rescale
        public class ThreadData
        {
            public int start;
            public int end;
            public ThreadData (int s, int e)
            {
                start = s;
                end = e;
            }
        }

        private static Color[] texColors;
        private static Color[] newColors;
        private static int w;
        private static float ratioX;
        private static float ratioY;
        private static int w2;
        private static int finishCount;
        private static Mutex mutex;

        //-------------------------------------------------
        public static void Point (Texture2D tex, int newWidth, int newHeight, bool noThread)
        {
            ThreadedScale (tex, newWidth, newHeight, false, noThread);
        }

        //-------------------------------------------------
        public static void Bilinear (Texture2D tex, int newWidth, int newHeight, bool noThread)
        {
            ThreadedScale (tex, newWidth, newHeight, true, noThread);
        }

        //-------------------------------------------------
        private static void ThreadedScale (Texture2D tex, int newWidth, int newHeight, bool useBilinear, bool noThread)
        {
			if (!noThread)
           		texColors = tex.GetPixels();
            newColors = new Color[newWidth * newHeight];
            if (useBilinear)
            {
                ratioX = 1.0f / (float)newWidth / (tex.width-1);
                ratioY = 1.0f / (float)newHeight / (tex.height-1);
            }
            else
            {
                ratioX = ((float)tex.width) / newWidth;
                ratioY = ((float)tex.height) / newHeight;
            }
            w = tex.width;
            w2 = newWidth;
            var cores = Mathf.Min(SystemInfo.processorCount, newHeight);
            var slice = newHeight/cores;

            finishCount = 0;
            if (mutex == null)
            {
                mutex = new Mutex(false);
            }
			if(!noThread)
			{
	            if (cores > 1)
	            {
	                int i = 0;
	                ThreadData threadData;
	                for (i = 0; i < cores-1; i++) {
	                       threadData = new ThreadData(slice * i, slice * (i + 1));
	                       ParameterizedThreadStart ts = useBilinear ? new ParameterizedThreadStart(BilinearScale) : new ParameterizedThreadStart(PointScale);
	                    Thread thread = new Thread(ts);
	                    thread.Start(threadData);
	                }
	                threadData = new ThreadData(slice*i, newHeight);
	                if (useBilinear)
	                   {
	                    BilinearScale(threadData);
	                }
	                else
	                   {
	                    PointScale(threadData);
	                }
	                while (finishCount < cores)
	                   {
	                       Thread.Sleep(1);
	                   }
	            }
	            else
	               {
	                ThreadData threadData = new ThreadData(0, newHeight);
	                if (useBilinear)
	                   {
	                    BilinearScale(threadData);
	                }
	                else
	                   {
	                    PointScale(threadData);
	                }
	            }
            }
			if (useBilinear)
			{
				BilinearDirect(0, newHeight, 0, newWidth, tex);
			}
			else
			{
				PointScaleDirect(0, newHeight, 0, newWidth, tex);
			}

            tex.Resize(newWidth, newHeight);
            tex.SetPixels(newColors);
            tex.Apply();
        }
		

        //-------------------------------------------------
        public static void BilinearScale (System.Object obj)
        {
            ThreadData threadData = (ThreadData) obj;
            for (var y = threadData.start; y < threadData.end; y++)
               {
                int yFloor = (int)Mathf.Floor(y * ratioY);
                var y1 = yFloor * w;
                var y2 = (yFloor+1) * w;
                var yw = y * w2;

                for (var x = 0; x < w2; x++) {
                    int xFloor = (int)Mathf.Floor(x * ratioX);
                    var xLerp = x * ratioX-xFloor;
                    newColors[yw + x] = ColorLerpUnclamped(ColorLerpUnclamped(texColors[y1 + xFloor], texColors[y1 + xFloor+1], xLerp),
                                                           ColorLerpUnclamped(texColors[y2 + xFloor], texColors[y2 + xFloor+1], xLerp),
                                                           y*ratioY-yFloor);
                }
            }

            mutex.WaitOne();
            finishCount++;
            mutex.ReleaseMutex();
        }
		
        //-------------------------------------------------
        public static void BilinearDirect (int ymin, int ymax, int xmin, int xmax, Texture2D textureScale)
        {
            for (int y = ymin; y < ymax; y++)
               {
                int yFloor = (int)Mathf.Floor(y * ratioY);
                var y1 = yFloor * w;
                var y2 = (yFloor+1) * w;
//                var yw = y * w2;

                for (int x = xmin; x < xmax; x++) {
//              		int thisX = (int)(ratioX * x) ;
                    int xFloor = (int)Mathf.Floor(x * ratioX);
                    var xLerp = x * ratioX-xFloor;
                    newColors[(y)*xmax + x] = ColorLerpUnclamped(
						ColorLerpUnclamped(texColors[y1 + xFloor], texColors[y1 + xFloor+1], xLerp),
                                                          
						ColorLerpUnclamped(texColors[y2 + xFloor], texColors[y2 + xFloor+1], xLerp),
                                                          y*ratioY-yFloor);                
				}
            }
        }

        //-------------------------------------------------
        public static void PointScale (System.Object obj)
        {
            ThreadData threadData = (ThreadData) obj;
            for (var y = threadData.start; y < threadData.end; y++)
               {
                var thisY = (int)(ratioY * y) * w;
                var yw = y * w2;
                for (var x = 0; x < w2; x++) {
                    newColors[yw + x] = texColors[(int)(thisY + ratioX*x)];
                }
            }
        
            mutex.WaitOne();
            finishCount++;
            mutex.ReleaseMutex();
        } 
		//-------------------------------------------------
        public static void PointScaleDirect (int ymin, int ymax, int xmin, int xmax, Texture2D textureScale)
        {
            for (int y = ymin; y < ymax; y++)
            {
                int thisY = (int)(ratioY * y) ;
                for (int x = xmin; x < xmax; x++) {
              		 int thisX = (int)(ratioX * x) ;
                    newColors[(y)*xmax + x] = textureScale.GetPixel(thisX, thisY);
                }
            }        
        }
        //-------------------------------------------------
        private static Color ColorLerpUnclamped (Color c1, Color c2, float value)
        {
            return new Color (c1.r + (c2.r - c1.r)*value,
                          c1.g + (c2.g - c1.g)*value,
                          c1.b + (c2.b - c1.b)*value,
                          c1.a + (c2.a - c1.a)*value);
        }
        #endregion

        //-----------------------------------------------------
        public static Rect ResizeImagePreservingRatio(float imgW, float imgH, float displayW,
                                                                                     float displayH)
        {
            float _x = 0;
            float _y = 0;

    //       Debug.Log("ResizeImagePreservingRatio : "+imgW+", "+imgH+", "+displayW+", "+displayH);
            if (imgW>=imgH)
            {
                float _factor = displayW/imgW;

                if(imgH*_factor < displayH)
                {
                    imgH *= _factor;
                    imgW = displayW;
                    _y = (displayH - imgH)/2;
                }
                else
                {
                    _factor = displayH/imgH;
                    imgW *= _factor;
                    imgH = displayH;
                    _x = (displayW - imgW)/2;
                }
            }
            else if (imgH>=imgW)
            {
                float _factor = displayH/imgH;

                if(imgW*_factor < displayW)
                {
                    imgW *= _factor;
                    imgH = displayH;
                    _x = (displayW - imgW)/2;
                }
                else
                {
                    _factor = displayW/imgW;
                    imgH *= _factor;
                    imgW = displayW;
                    _y = (displayH - imgH)/2;
                }
            }

            return new Rect(Mathf.RoundToInt(_x), Mathf.RoundToInt(_y),
                            Mathf.RoundToInt(imgW), Mathf.RoundToInt(imgH));
        } // ResizeImagePreservingRatio()

        //-----------------------------------------------------
        // Tiles an <areaToFill> with a <texture> which is scaled to the
        // size of a <tile> using a given <scaleMode>.
        // Author: Isaac Manning Dart
        // http://answers.unity3d.com/questions/14802/guiskin-tiling-instead-of-stretching-textures.html
        public static void TileTexture(Texture texture, Rect tile, Rect areaToFill, ScaleMode scaleMode)
        {
            for (float y = areaToFill.y; y < areaToFill.y + areaToFill.height; y = y + tile.height)
            {
                for (float x = areaToFill.x; x < areaToFill.x + areaToFill.width; x = x + tile.width)
                {
                    tile.x = x; tile.y = y;
                    GUI.DrawTexture(tile, texture, scaleMode);
                }
            }
        } // TileTexture()

        //-----------------------------------------------------
        // Tiles an <areaToFill> with a <texture> which is scaled to the
        // size of a <tile> using a given <scaleMode>.
        // the initial texture is draw at the initial position offsetWidth, offsetHeight
        public static void TileTextureWithOffset(Texture texture, Rect tile, Rect areaToFill,
                                        ScaleMode scaleMode, float offsetWidth, float offsetHeight)
        {
            Rect newTile = new Rect(0, 0, tile.width, tile.height);
            for (float y = areaToFill.y+offsetHeight-tile.height; y < areaToFill.y+offsetHeight; y = y + newTile.height)
            {
                for (float x = areaToFill.x+offsetWidth-tile.width; x < areaToFill.x + areaToFill.width; x = x + newTile.width)
                {
                    newTile.x = x; newTile.y = y;
                    GUI.DrawTexture(newTile, texture, scaleMode);
                }
            }
            for (float y = areaToFill.y+offsetHeight-tile.height; y < areaToFill.y + areaToFill.height; y = y + newTile.height)
            {
                for (float x = areaToFill.x+offsetWidth-tile.width; x < areaToFill.x+offsetWidth; x = x + newTile.width)
                {
                    newTile.x = x; newTile.y = y;
                    GUI.DrawTexture(newTile, texture, scaleMode);
                }
            }
            for (float y = areaToFill.y+offsetHeight; y < areaToFill.y + areaToFill.height; y = y + tile.height)
            {
                for (float x = areaToFill.x+offsetWidth; x < areaToFill.x + areaToFill.width; x = x + tile.width)
                {
                    tile.x = x; tile.y = y;
                    GUI.DrawTexture(tile, texture, scaleMode);
                }
            }
        } // TileTextureWithOffset()

    } // class ImgUtils

} // Namespace Pointcube.Utils
