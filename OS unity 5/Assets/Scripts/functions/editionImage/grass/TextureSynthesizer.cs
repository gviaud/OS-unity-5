//-----------------------------------------------------------------------------
// Assets/Scripts/functions/editionImage/grass/TextureSynthesizer.cs - 05/2012 KS
// Repris de TextureSynthesize.java de Simon Hallouin (OSv3.5, 08/2009)
// Utilisé par GrassSynthesizer (attaché à aucun Objet de la scène Unity)

using UnityEngine;
using System.Collections.Generic;
using System;

public class TextureSynthesizer
{
//    private BufferedImage _in;              // L'image à partir de laquelle on synthétise la texture
    private image _in;                      // L'image à partir de laquelle on synthétise la texture
    private int _patchSize = 10;            // La taille du patch (patch carré)
    private int _nbPatchPasted = 1;         // Le nombre de patch déjà collés dans l'image de sortie (initialisation à 1)
    private int _nbPatchPerLine = 0;        // Le nombre de patch par ligne désiré
    private int _nbPatchPerColumn = 0;      // Le nombre de patch par colonne désiré
    private int _widthBorder = 4;           // La largeur du bord du patch utilisé pour le calcul de 'correspondance'
//    private int _heightBorder = _patchSize; // La hauteur du bord du patch utilisé pour le calcul de 'correspondance'
    private int _heightBorder = 10;         // La hauteur du bord du patch utilisé pour le calcul de 'correspondance'
    private int _sampleSize = 50;           // La taille du sample (= 4 fois la taille de l'image d'entrée, car on fait des miroirs)
    private int nbLinesDone = 0;            // Le nombre de lignes synthétisées
    
    private List<int[]> _tabInH  = new List<int[]>();
    private List<int[]> _tabInW  = new List<int[]>();
    private List<int[]> _tabInHD = new List<int[]>();

    private int _widthAnalysis = 0;         // La largeur de l'image moins la largeur du patch = largeur de recherche dans le sample
    private int _heightAnalysis = 0;        // La hauteur de l'image moins la hauteur du patch = hauteur de recherche dans le sample

    private int _widthFinalTexture;         // Largeur de la texture voulue
    private int _heightFinalTexture;        // Hauteur de la texture voulue

    private int _pasBalayage = 4;           // Pas de balayage dans le sample (petit = précis)

    //-----------------------------------------------------
    // Constructeur (l'image finale est 4 fois plus grande que les tailles
    // passées en paramètres : on repète la texture synthétisée 16 fois
    //  @param width La largeur de la texture synthetisée
    //  @param height La hauteur de la texture synthetisée
    public TextureSynthesizer(int width, int height)
    {
//        super();
        _widthFinalTexture  = width;
        _heightFinalTexture = height;
//        _in = new BufferedImage(_sampleSize, _sampleSize, BufferedImage.TYPE_INT_ARGB);
        _in = new image(_sampleSize, _sampleSize);
    }

    //-----------------------------------------------------
    // Fonction qui génère une image 4 fois plus grande que l image d entrée
    // en effectuant des miroirs par rapport à l image d entrée
    //  @param image L'image d entrée
    //  @return duplicateImage L'image dupliquée
//    private BufferedImage duplicateSample(BufferedImage image)
    private image duplicateSample(image img)
    {
        int[] rgb  = new int[(_sampleSize/2)*(_sampleSize/2)];
//        rgb = image.getRGB(_sampleSize/2, _sampleSize/2, rgb, 0, _sampleSize/2);
        rgb = img.getRGB(rgb);

        rgb = removeArtifacts(rgb);         // enlève les pixels trop eloignés de la moyenne du sample

//        image.setRGB(0, 0, _sampleSize/2, _sampleSize/2, rgb, 0, _sampleSize/2);
        img.setRGB(rgb);                  // image sans artefacts

        // -- Miroir par rap à Y --
//        BufferedImage _inYmirror = new BufferedImage(_sampleSize/2, _sampleSize/2, BufferedImage.TYPE_INT_ARGB);
        image _inYmirror = new image(img); // Copie
        _inYmirror.MirrorY();

//        BufferedImage _in4 = new BufferedImage(2*image.getWidth(), image.getHeight(), BufferedImage.TYPE_INT_ARGB);
        image _in4 = new image(2*img.Width(), img.Height());
//        _in4.getGraphics().drawImage(image, 0, 0, null);
        _in4.setPixels(img);
//        _in4.getGraphics().drawImage(_inYmirror, _in4.getWidth()/2, 0, null);
        _in4.setPixels(_inYmirror, _in4.Width()/2, 0);        // TODO Vérifier si le résultat est correct

        // -- Miroir par rap à X --
//        BufferedImage in5 = new BufferedImage(_sampleSize, _sampleSize/2, BufferedImage.TYPE_INT_ARGB);
        image in5 = new image(_in4);
        in5.MirrorX();

//        BufferedImage duplicateImage = new BufferedImage(_sampleSize, _sampleSize, BufferedImage.TYPE_INT_ARGB);
        image duplicateImage = new image(_sampleSize, _sampleSize);
//        duplicateImage.getGraphics().drawImage(_in4, 0, 0, null);
        duplicateImage.setPixels(_in4);
//        duplicateImage.getGraphics().drawImage(in5, 0, _sampleSize/2, null);
        duplicateImage.setPixels(in5, 0, _sampleSize/2);

        return duplicateImage;
    }


    /**
     * @param image
     * @return out3
     */
    public image synthesize(image img)
    {
//        BufferedImage copie = new BufferedImage(image.getWidth(), image.getHeight(), BufferedImage.TYPE_INT_ARGB);
//        copie = CubeImage.getAsCopy(image);
        image copie = new image(img);

        _in = duplicateSample(copie);

        //Nb de patch par ligne et par colonne
        _nbPatchPerLine   = _widthFinalTexture  / (_patchSize-_widthBorder) +1;
        _nbPatchPerColumn = _heightFinalTexture / (_patchSize-_widthBorder) +1;

        _nbPatchPasted = 1;
        nbLinesDone    = 0;

        _widthAnalysis  = _in.Width()  - _patchSize;
        _heightAnalysis = _in.Height() - _patchSize;

        //Initialisation de Out
//        BufferedImage out = new BufferedImage((_nbPatchPerLine)*(_patchSize-_widthBorder)+_widthBorder, (_nbPatchPerColumn)*(_patchSize-_widthBorder)+_widthBorder, BufferedImage.TYPE_INT_ARGB);
        image outImg = new image((_nbPatchPerLine)*(_patchSize-_widthBorder)+_widthBorder,
                                 (_nbPatchPerColumn)*(_patchSize-_widthBorder)+_widthBorder);
        System.Random r = new System.Random();
//        int x = r.nextInt(_widthAnalysis);
//        int y = r.nextInt(_heightAnalysis);
        int x = r.Next(_widthAnalysis);
        int y = r.Next(_heightAnalysis);

//        BufferedImage firstPatch = _in.getSubimage(x, y, _patchSize, _patchSize);
        image first_patch = _in.getSubImage(x, y, _patchSize, _patchSize);

//        out.getGraphics().drawImage(firstPatch, 0, 0, null);
        outImg.setPixels(first_patch);

        analyseIn(_in);

        for(int i=0; i<_nbPatchPerLine-1; i++)
            firstLine(_in, outImg);

        for(int i=0; i<_nbPatchPerColumn-1; i++)
        {
            nbLinesDone++;
            firstPatch(_in, outImg);
            _nbPatchPasted = 1;

            for(int k=0; k<_nbPatchPerLine-1; k++)
                otherLine(_in, outImg);
        }
//        out = out.getSubimage(0, 0, _widthFinalTexture, _heightFinalTexture);
        outImg = outImg.getSubImage(0, 0, _widthFinalTexture, _heightFinalTexture);

//        BufferedImage out2 = new BufferedImage(2*_widthFinalTexture, 2*_heightFinalTexture, BufferedImage.TYPE_INT_ARGB);
        image out2 = new image(2*_widthFinalTexture, 2*_heightFinalTexture);
//        out2.getGraphics().drawImage(out, 0, 0, null);
        out2.setPixels(outImg);
//        out2.getGraphics().drawImage(out, _widthFinalTexture, 0, null);
        out2.setPixels(outImg, _widthFinalTexture, 0);
//        out2.getGraphics().drawImage(out, 0, _heightFinalTexture, null);
        out2.setPixels(outImg, 0, _heightFinalTexture);
//        out2.getGraphics().drawImage(out, _widthFinalTexture, _heightFinalTexture, null);
        out2.setPixels(outImg, _widthFinalTexture, _heightFinalTexture);

//        BufferedImage out3 = new BufferedImage(4*_widthFinalTexture, 4*_heightFinalTexture, BufferedImage.TYPE_INT_ARGB);
        image out3 = new image(4*_widthFinalTexture, 4*_heightFinalTexture);
//        out3.getGraphics().drawImage(out2, 0, 0, null);//out2
        out3.setPixels(out2, 0, 0);//out2
//        out3.getGraphics().drawImage(out2, 2*_widthFinalTexture, 0, null);
        out3.setPixels(out2, 2*_widthFinalTexture, 0);
//        out3.getGraphics().drawImage(out2, 0, 2*_heightFinalTexture, null);
        out3.setPixels(out2, 0, 2*_heightFinalTexture);
//        out3.getGraphics().drawImage(out2, 2*_widthFinalTexture, 2*_heightFinalTexture, null);
        out3.setPixels(out2, 2*_widthFinalTexture, 2*_heightFinalTexture);
        return out3;
    } // synthesize()

    /**
    *
    * @param in
    */
    private void analyseIn(image img)
    {
        int index = 0;

        for (int i=0; i<_widthAnalysis; i++)
        {
            for (int j=0; j<_heightAnalysis; j++)
            {
                _tabInH.Add(new int[_widthBorder*_heightBorder]);
//                img.getRGB(i, j, _widthBorder, _heightBorder, _tabInH[index], 0, _widthBorder);
                img.getRGB(_tabInH[index], i, j, _widthBorder, _heightBorder);

                _tabInW.Add(new int[_widthBorder*_heightBorder]);
//                img.getRGB(i, j, _heightBorder, _widthBorder, _tabInW[index], 0, _heightBorder);
                img.getRGB(_tabInW[index], i, j, _heightBorder, _widthBorder);

                _tabInHD.Add(new int[_widthBorder*_heightBorder]);
//                img.getRGB(i+_patchSize-4, j, _widthBorder, _heightBorder, _tabInHD[index], 0, _widthBorder);
                img.getRGB(_tabInHD[index], i+_patchSize-4, j, _widthBorder, _heightBorder);

                index++;
            }
        }
    } // analyseIn


    /**
    * Le premier patch pour les nouvelles lignes
    * @param in
    * @param out
    */
    private void firstPatch(image imgIn, image imgOut)
    {
        //Pour les bords du haut
        int[] rgbOutW = new int[_widthBorder*_heightBorder];

        //Tableau qui contiendra toutes les distances (en se positionnant en chaque point de imgIn)
        float[] tabDistanceW = new float[_widthAnalysis*_heightAnalysis];
        //Liste qui contiendra les index (numeros de pixels) dont la distance avec le dernier patch collé est
        //inferieure à la distance max
        float[] tabDistInf  = new float[200];
        int[] tabDistInfIndex  = new int[tabDistInf.Length];

        for (int i = 0; i < tabDistInf.Length; i++)
        {
            tabDistInf[i] = float.MaxValue;
            tabDistInfIndex[i] = -1;
        }

        //Les valeurs rgb du bord de out (en bas)
//        imgOut.getRGB(0, nbLinesDone*(_patchSize-_widthBorder), _heightBorder, _widthBorder, rgbOutW, 0, _heightBorder);
       imgOut.getRGB(rgbOutW, 0, nbLinesDone*(_patchSize-_widthBorder), _heightBorder, _widthBorder);

        float dMax = 0;
        dMax = get_dMax(rgbOutW);
    

        int index = 0;
        int distMin = int.MaxValue;
        int indexMin = int.MaxValue;
        //Parcours de l image (moins les deux bords en bas et à droite)
        for (int i=0; i<_widthAnalysis*_heightAnalysis; i=i+_pasBalayage)
        {
            // L'ensemble des distances en positionnant le patch sur chaque pixel de l image
//            tabDistanceW[index] = distance(_tabInW.get(index), rgbOutW);
            tabDistanceW[index] = distance(_tabInW[index], rgbOutW);

            if(tabDistanceW[index] < dMax)
            {
                float distmax = -1;
                int indexTabdistInf = -1;
    
                for(int k=0; k<tabDistInf.Length; k++)
                {
                    if(tabDistanceW[index] < tabDistInf[k])
                    {
                        if(tabDistInf[k] > distmax)
                        {
                            distmax = tabDistInf[k];
                            indexTabdistInf = k;
                        }
                    }
                }
                if(indexTabdistInf!=-1)
                {
                    tabDistInfIndex[indexTabdistInf] = index;
                    tabDistInf[indexTabdistInf] = tabDistanceW[index];
                }
            }
        
    
            //Distance min
            if(tabDistanceW[index]<distMin)
            {
                distMin = (int)tabDistanceW[index];
                indexMin = index;
            }
            index = index+_pasBalayage;
        }
    
        //On ajoute la plus petite distance si aucune n etait inférieure à  dMax
        int l = 0;
        while(l < tabDistInf.Length && tabDistInfIndex[l] != -1) l++;
    
        if(tabDistInf[0]==float.MaxValue)    //tableau vide
        {
            tabDistInf[0] = distMin;
            tabDistInfIndex[0] = indexMin;
            l = 1;
        }

        //Un index dans la liste des distance inférieures au seuil (aleatoirement)
        System.Random r = new System.Random();
        int random = r.Next(l);
    
        // On copie le patch choisi aleatoirement dans l'image out
//        Graphics2D g2d = (Graphics2D)out.getGraphics();

        // Le patch qu'on colle dans Out
//        BufferedImage patch = null;
        image patch = null;

        patch = imgIn.getSubImage((tabDistInfIndex[random]%_widthAnalysis),
                                tabDistInfIndex[random]/_widthAnalysis, _patchSize, _patchSize);

        // Le tableau de pixels du bord du patch
        int[] rgbPatchBorderW = new int[_patchSize*_patchSize];
//        patch.getRGB(0, 0, _heightBorder, _widthBorder, rgbPatchBorderW, 0, _heightBorder);
        patch.getRGB(rgbPatchBorderW, 0, 0, _heightBorder, _widthBorder);

        //On colle le patch par dessus le dernier patch collé dans Out
//        g2d.drawImage(patch,0,  nbLinesDone*(_patchSize-_widthBorder), null);
        imgOut.setPixels(patch, 0, nbLinesDone*(_patchSize-_widthBorder));

//        out = borderW(rgbOutW, rgbPatchBorderW, out, true);
        imgOut = borderW(rgbOutW, rgbPatchBorderW, imgOut, true);
    } // firstPatch()

    /**
     *
     * @param in
     * @param out
     */
    private void firstLine(image imgIn, image imgOut)
    {
        int[] rgbOut = new int[_widthBorder*_heightBorder];

        //Tableau qui contiendra toutes les distances (en se positionnant en chaque point de In)
        float[] tabDistance = new float[_widthAnalysis*_heightAnalysis];
        //Liste qui contiendra les index (numeros de pixels) dont la distance avec le dernier patch collé est
        //inferieure à la distance max
        float[] tabDistInf  = new float[200];
        int[] tabDistInfIndex  = new int[tabDistInf.Length];

        for (int i = 0; i < tabDistInf.Length; i++)
        {
            tabDistInf[i] = float.MaxValue;
            tabDistInfIndex[i] = -1;
        }

        //Les valeurs rgb du bord de _out (à droite)
//        out.getRGB(_nbPatchPasted*(_patchSize-_widthBorder), 0, _widthBorder, _heightBorder, rgbOut, 0, _widthBorder);
        imgOut.getRGB(rgbOut, _nbPatchPasted*(_patchSize-_widthBorder), 0, _widthBorder, _heightBorder);

        int distMin = int.MaxValue;
        int indexMin = int.MaxValue;
        int index = 0;
        float dMax = get_dMax(rgbOut);

        for (int i=0; i<_widthAnalysis*_heightAnalysis; i=i+_pasBalayage)
        {
            // L'ensemble des distances en positionnant le patch sur chaque pixel de l'image
            tabDistance[index] = distance(_tabInH[index], rgbOut);
    
            if(tabDistance[index] < dMax)
            {
                float distmax = -1;
                int indexTabdistInf = -1;
    
                for(int k=0; k < tabDistInf.Length; k++)
                {
                    if(tabDistance[index] < tabDistInf[k])
                    {
                        if(tabDistInf[k] > distmax)
                        {
                            distmax = tabDistInf[k];
                            indexTabdistInf = k;
                        }
                    }
                }
                if(indexTabdistInf!=-1)
                {
                    tabDistInfIndex[indexTabdistInf] = index;
                    tabDistInf[indexTabdistInf] = tabDistance[index];
                }
            }
    
            //Distance min
            if(tabDistance[index]<distMin)
            {
                distMin = (int)tabDistance[index];
                indexMin = index;
            }
            index=index+_pasBalayage;
        }
        
        // On ajoute la plus petite distance si aucune n etait inférieure à  dMax
        int l = 0;
        while(l<tabDistInf.Length && tabDistInfIndex[l]!=-1) l++;
        
        if(tabDistInf[0]==float.MaxValue)
        {  //tableau vide
            tabDistInf[0] = distMin;
            tabDistInfIndex[0] = indexMin;
            l = 1;
        }
        
        // Un indice dans la liste des distance inférieures au seuil (aléatoirement)
        System.Random r = new System.Random();
        int random = r.Next(l);
        
        //On copie le patch choisi aleatoirement dans l image out
//        Graphics2D g2d = (Graphics2D)out.getGraphics();
        //Le patch qu on colle dans Out
//        BufferedImage patch = null;
        image patch = null;

        patch = imgIn.getSubImage((tabDistInfIndex[random]%_widthAnalysis),
                                   tabDistInfIndex[random]/_widthAnalysis, _patchSize, _patchSize);

        //Le tableau de pix du bord du patch
        int[] rgbPatchBorder = new int[_patchSize*_patchSize];
//        patch.getRGB(0, 0, _widthBorder, _heightBorder, rgbPatchBorder, 0, _widthBorder);
        patch.getRGB(rgbPatchBorder, 0, 0, _widthBorder, _heightBorder);

        //On colle le patch par dessus le dernier patch collé dans Out
//        g2d.drawImage(patch, _nbPatchPasted*(_patchSize-_widthBorder), 0, null);
        imgOut.setPixels(patch, _nbPatchPasted*(_patchSize-_widthBorder), 0);

        //On arrange la liaison entre les deux patch
//        out = borderH(rgbOut, rgbPatchBorder, out);
        imgOut = borderH(rgbOut, rgbPatchBorder, imgOut);

        _nbPatchPasted++;
    } // firstLine()


    /**
      * Les lignes suivantes
      * @param in
      * @param out
      */
    private void otherLine(image imgIn, image imgOut)
    {
        int[] rgbOutH = new int[_widthBorder*_heightBorder];
        //Pour les bords du haut
        int[] rgbOutW = new int[_widthBorder*_heightBorder];

        int[] rgbOutHD = new int[_widthBorder*_heightBorder];

        //Tableau qui contiendra toutes les distances (en se positionnant en chaque point de In)
        float[] tabDistanceH = new float[_widthAnalysis*_heightAnalysis];
        float[] tabDistanceW = new float[_widthAnalysis*_heightAnalysis];

        float[] tabDistanceHD = new float[_widthAnalysis*_heightAnalysis];
        //Liste qui contiendra les index (numeros de pixels) dont la distance avec le dernier patch collé est
        //inferieure à la distance max
        float[] tabDistInf  = new float[200];
        int[] tabDistInfIndex  = new int[tabDistInf.Length];

        for (int i = 0; i < tabDistInf.Length; i++)
        {
            tabDistInf[i] = float.MaxValue;
            tabDistInfIndex[i] = -1;
        }

        //Les valeurs rgb du bord de _out (à droite)

//        out.getRGB(((_nbPatchPasted)*(_patchSize-_widthBorder)),  nbLinesDone*(_patchSize-_widthBorder), _widthBorder, _heightBorder, rgbOutH, 0, _widthBorder);
        imgOut.getRGB(rgbOutH, ((_nbPatchPasted)*(_patchSize-_widthBorder)),
                      nbLinesDone*(_patchSize-_widthBorder), _widthBorder, _heightBorder);
//        out.getRGB(0,  nbLinesDone*(_patchSize-_widthBorder), _widthBorder, _heightBorder, rgbOutHD, 0, _widthBorder);
        imgOut.getRGB(rgbOutHD, 0,  nbLinesDone*(_patchSize-_widthBorder), _widthBorder, _heightBorder);

        //Les valeurs rgb du bord de out (en bas)
//        out.getRGB((_nbPatchPasted)*(_patchSize-_widthBorder), nbLinesDone*(_patchSize-_widthBorder), _heightBorder, _widthBorder, rgbOutW, 0, _heightBorder);
        imgOut.getRGB(rgbOutW, (_nbPatchPasted)*(_patchSize-_widthBorder),
                      nbLinesDone*(_patchSize-_widthBorder), _heightBorder, _widthBorder);

        float dMax = 0;
        if(_nbPatchPasted == _nbPatchPerLine-1)
            dMax = get_dMax(rgbOutH) + get_dMax(rgbOutW) + get_dMax(rgbOutHD);
        else
            dMax = get_dMax(rgbOutH) + get_dMax(rgbOutW);

        int index = 0;
        int distMin = int.MaxValue;
        int indexMin = int.MaxValue;

        //Parcours de l image (moins les deux bords en bas et à droite)
        for (int i=0; i<_widthAnalysis*_heightAnalysis; i+=_pasBalayage)
        {
            //L ensemble des distances en positionnant le patch sur chaque pixel de l image
            tabDistanceH[index]  = distance(_tabInH[index], rgbOutH);

            tabDistanceHD[index] = distance(_tabInHD[index], rgbOutHD);

            tabDistanceW[index]  = distance(_tabInW[index], rgbOutW);
    
            //On fait la somme des deux distances
            tabDistanceW[index] += tabDistanceH[index];
    
            if(_nbPatchPasted == _nbPatchPerLine-1)
                tabDistanceW[index] += tabDistanceHD[index];
    
            if(tabDistanceW[index] < dMax)
            {
                float distmax = -1;
                int indexTabdistInf = -1;
    
                for(int k=0; k<tabDistInf.Length; k++)
                {
                    if(tabDistanceW[index] < tabDistInf[k])
                    {
                        if(tabDistInf[k] > distmax)
                        {
                            distmax = tabDistInf[k];
                            indexTabdistInf = k;
                        }
                    }
                }
                if(indexTabdistInf!=-1)
                {
                    tabDistInfIndex[indexTabdistInf] = index;
                    tabDistInf[indexTabdistInf] = tabDistanceW[index];
                }
            }
    
            //Distance min
            if(tabDistanceW[index]<distMin)
            {
                distMin = (int)tabDistanceW[index];
                indexMin = index;
            }
            index=index+_pasBalayage;
        }
    
        //On ajoute la plus petite distance si aucune n etait inférieure à  dMax
        int l = 0;
        while(l<tabDistInf.Length && tabDistInfIndex[l]!=-1) l++;
    
        if(tabDistInf[0]==float.MaxValue)   //tableau vide
        {
            tabDistInf[0] = distMin;
            tabDistInfIndex[0] = indexMin;
            l = 1;
        }
    
        //Un index dans la liste des distance inférieures au seuil (aleatoirement)
        System.Random r = new System.Random();
        int random = r.Next(l);
    
        //On copie le patch choisi aleatoirement dans l image out
//        Graphics2D g2d = (Graphics2D)out.getGraphics();
        //Le patch qu on colle dans Out
        image patch = null;

        patch = imgIn.getSubImage((tabDistInfIndex[random]%_widthAnalysis),
                                tabDistInfIndex[random]/_widthAnalysis, _patchSize, _patchSize);

        //Le tableau de pix du bord du patch
        int[] rgbPatchBorderH = new int[_patchSize*_patchSize];
        int[] rgbPatchBorderW = new int[_patchSize*_patchSize];
//        patch.getRGB(0, 0, _widthBorder, _heightBorder, rgbPatchBorderH, 0, _widthBorder);
        patch.getRGB(rgbPatchBorderH, 0, 0, _widthBorder, _heightBorder);
//        patch.getRGB(0, 0, _heightBorder, _widthBorder, rgbPatchBorderW, 0, _heightBorder);
        patch.getRGB(rgbPatchBorderW, 0, 0, _heightBorder, _widthBorder);


        //On colle le patch par dessus le dernier patch collé dans Out
//        g2d.drawImage(patch,_nbPatchPasted*(_patchSize-_widthBorder),  nbLinesDone*(_patchSize-_widthBorder), null);
        imgOut.setPixels(patch, _nbPatchPasted*(_patchSize-_widthBorder), nbLinesDone*(_patchSize-_widthBorder));

        //On arrange la liaison entre les deux patch
//        out = borderH(rgbOutH, rgbPatchBorderH, out);
        imgOut = borderH(rgbOutH, rgbPatchBorderH, imgOut);
//        out = borderW(rgbOutW, rgbPatchBorderW, out, false);
        imgOut = borderW(rgbOutW, rgbPatchBorderW, imgOut, false);

        _nbPatchPasted++;
    } // otherLine()

    /**
    * Calcule la distance max
    * @param rgb
    * @return dMax
    */
    private float get_dMax(int[] rgb)
    {
        float dMax = 0;

        for(int i=0; i<rgb.Length; i++)
        {
            dMax +=  (float) (0.2f*((rgb[i] >> 16) & 0xff) * ((rgb[i] >> 16) & 0xff))
                            + 0.2f*((rgb[i] >> 8)  & 0xff) * ((rgb[i] >> 8)  & 0xff)
                            + 0.2f*( rgb[i]        & 0xff) * ( rgb[i]        & 0xff);
        }

        dMax = (float) Mathf.Sqrt(dMax/rgb.Length);
        return dMax;
    }

    /**
    * Fonction de calcul de distance euclidienne entre deux tableaux contenant
    * des valeurs de pixels
    * @param rgb1
    * @param rgb2
    * @return d La distance
    */
    private float distance(int[] rgb1, int[] rgb2)
    {
        float d = 0;
        for(int i=0; i<rgb1.Length; i++)
        {
            d += ((rgb1[i] >> 16) & 0xff - (rgb2[i] >> 16) & 0xff)*((rgb1[i] >> 16) & 0xff - (rgb2[i] >> 16) & 0xff)
            +((rgb1[i] >> 8)  & 0xff - (rgb2[i] >> 8)  & 0xff)*((rgb1[i] >> 8)  & 0xff - (rgb2[i] >> 8)  & 0xff)
            +( rgb1[i]        & 0xff -  rgb2[i]        & 0xff)*( rgb1[i]        & 0xff -  rgb2[i]        & 0xff);
        }
        d = (float) Mathf.Sqrt(d / rgb1.Length);
        return d;
    }


    /** // TODO à tester
    * Renvoie l image 'miroir' par rapport à la verticale
    * @param image L image d origine
    * @return result Le mirroir
    */
//    private image yMirror(image img)
//    {
////        BufferedImage result = new BufferedImage(image.getWidth(), image.getHeight(), BufferedImage.TYPE_INT_ARGB);
//        image result = new image(img.Width(), img.Height());
//        result.MirrorY();
////        AffineTransform yMiror = new AffineTransform(new float[] {-1.0f,0.0f,0.0f,1.0f});
////        AffineTransform yMiror = new AffineTransform(new float[] {-1.0f,0.0f,0.0f,1.0f});
////        yMiror.translate(-image.getWidth(), 0.0);
////        Graphics2D g2d = (Graphics2D)result.getGraphics();
////        g2d.setTransform(yMiror);
////        g2d.drawImage(image.getSubimage(0, 0, image.getWidth(), image.getHeight()), 0, 0, null);
//
//        return result;
//    }
//
//    /**
//    * Renvoie l image 'miroir' par rapport à l horizontale
//    * @param image L image d origine
//    * @return result Le mirroir
//    */
//    private image xMirror(image img)
//    {
////        BufferedImage result = new BufferedImage(image.getWidth(), image.getHeight(), BufferedImage.TYPE_INT_ARGB);
//        image result = new image(img.Height(), img.Width());
//        result.MirrorX();
////        AffineTransform xMiror = new AffineTransform(new float[] {1.0f,0.0f,0.0f,-1.0f});
////        xMiror.translate(0.0, -image.getHeight());
////        Graphics2D g2d = (Graphics2D)result.getGraphics();
////        g2d.setTransform(xMiror);
////        g2d.drawImage(image.getSubimage(0, 0, image.getWidth(), image.getHeight()), 0, 0, null);
//
//        return result;
//    }

    /**
    * Fonction qui modifie les pixels de l image si leur valeur est trop
    * éloignée de la moyenne de l image
    * @param rgb Tableau de pixels
    * @return rgbResult Tableau de pixels modifié
    */
    private int[] removeArtifacts(int[] rgb)
    {
        int a = 0;
        int r = 0;
        int g = 0;
        int b = 0;
        int aResult = 0;
        int rResult = 0;
        int gResult = 0;
        int bResult = 0;

        int[] rgbResult = new int[rgb.Length];

        for(int i=0; i<rgb.Length; i++)
            rgbResult[i] = rgb[i];

        for(int i=0; i<rgb.Length; i++)
        {
            a += (rgb[i] >> 24) & 0xff;
            r += (rgb[i] >> 16) & 0xff;
            g += (rgb[i] >> 8)  & 0xff;
            b +=  rgb[i]        & 0xff;
        }

        //La moyenne des composantes
        a = a/rgb.Length;
        r = r/rgb.Length;
        g = g/rgb.Length;
        b = b/rgb.Length;

        System.Random rand = new System.Random();

        for(int i=0; i<rgb.Length; i++)
        {
            // L'écart max toléré par rapport à la moyenne
            int tolerance = 40;
            //Définit l intervalle (l ecart par rapport à la moyenne) pour la
            //correction des pixels trop éloignés de la moyenne
            int ecartMoyenne = 20;
    
            
            if( ((rgb[i] >> 16) & 0xff) > r+tolerance || ((rgb[i] >> 16) & 0xff) < r-tolerance )
            rResult = r + rand.Next(2*ecartMoyenne)-ecartMoyenne;
            else
            rResult = (rgb[i] >> 16) & 0xff;

            if( ((rgb[i] >> 8)  & 0xff) > g+tolerance || ((rgb[i] >> 8)  & 0xff) < g-tolerance )
            gResult = g + rand.Next(2*ecartMoyenne)-ecartMoyenne;
            else
            gResult = (rgb[i] >> 8) & 0xff;

            if( ( rgb[i]        & 0xff) > b+tolerance || ( rgb[i]        & 0xff) < b-tolerance )
            bResult = b + rand.Next(2*ecartMoyenne)-ecartMoyenne;
            else
            bResult = rgb[i] & 0xff;

            if( ((rgb[i] >> 24) & 0xff) > a+tolerance || ((rgb[i] >> 24) & 0xff) < a-tolerance )
            aResult = a;
            else
            aResult = (rgb[i] >> 24) & 0xff;

            rgbResult[i] = (aResult << 24) | (rResult << 16) | (gResult << 8) | bResult;
        }
    
        return rgbResult;
    } // removeArtifacts

/**
  *
  * @param rgb1 patch deja collé dans out
  * @param rgb2 patch de in
  * @param out
  * @return out
  */
    private image borderH(int[] rgb1, int[] rgb2, image imgOut)
    {
        int[] rgbRes = new int [_widthBorder*_heightBorder];
        for(int i=0; i<rgb1.Length-3; i++)
        {
            rgbRes[i] = (  (int)(0.75*((rgb1[i] >> 16) & 0xff ) + 0.25*((rgb2[i] >> 16) & 0xff))<<16  //1ere colonne
                         | (int)(0.75*((rgb1[i] >> 8)  & 0xff ) + 0.25*((rgb2[i] >> 8)  & 0xff))<<8
                         | (int)(0.75*( rgb1[i]        & 0xff ) + 0.25*( rgb2[i]        & 0xff))
                         | (int)((rgb1[i] >> 24) & 0xff )<<24);
            i++;
            rgbRes[i] = (  (int)(0.5*((rgb1[i] >> 16) & 0xff ) + 0.5*((rgb2[i] >> 16) & 0xff))<<16
                         | (int)(0.5*((rgb1[i] >> 8)  & 0xff ) + 0.5*((rgb2[i] >> 8)  & 0xff))<<8
                         | (int)(0.5*( rgb1[i]        & 0xff ) + 0.5*( rgb2[i]        & 0xff))
                         | (int)((rgb1[i] >> 24) & 0xff )<<24);
            i++;
            rgbRes[i] = (  (int)(0.5*((rgb1[i] >> 16) & 0xff ) + 0.5*((rgb2[i] >> 16) & 0xff))<<16
                         | (int)(0.5*((rgb1[i] >> 8)  & 0xff ) + 0.5*((rgb2[i] >> 8)  & 0xff))<<8
                         | (int)(0.5*( rgb1[i]        & 0xff ) + 0.5*( rgb2[i]        & 0xff))
                         | (int)((rgb1[i] >> 24) & 0xff )<<24);
            i++;
            rgbRes[i] = (  (int)(0.25*((rgb1[i] >> 16) & 0xff ) + 0.75*((rgb2[i] >> 16) & 0xff))<<16
                         | (int)(0.25*((rgb1[i] >> 8)  & 0xff ) + 0.75*((rgb2[i] >> 8)  & 0xff))<<8
                         | (int)(0.25*( rgb1[i]        & 0xff ) + 0.75*( rgb2[i]        & 0xff))
                         | (int)((rgb1[i] >> 24) & 0xff )<<24);
        }

//        out.setRGB(_nbPatchPasted*(_patchSize-_widthBorder), nbLinesDone*(_patchSize-_widthBorder), _widthBorder, _heightBorder, rgbRes, 0, _widthBorder);
        imgOut.setRGB(rgbRes, _nbPatchPasted*(_patchSize-_widthBorder),
                      nbLinesDone*(_patchSize-_widthBorder), _widthBorder, _heightBorder, _widthBorder);


        return imgOut;
    }

/**
  * Pour arranger les bords du haut
  * @param rgb1
  * @param rgb2
  * @param out
  * @param startLine
  * @return out
  */
    private image borderW(int[] rgb1, int[] rgb2, image imgOut, bool startLine)
    {
        int[] rgbRes = new int[_widthBorder*_heightBorder];
    
        for(int i=0; i<rgb1.Length/4; i++)
        {
            rgbRes[i] = ((int)(0.75*((rgb1[i] >> 16) & 0xff ) + 0.25*((rgb2[i] >> 16) & 0xff))<<16  //1ere colonne
                          | (int)(0.75*((rgb1[i] >> 8)  & 0xff ) + 0.25*((rgb2[i] >> 8)  & 0xff))<<8
                          | (int)(0.75*( rgb1[i]        & 0xff ) + 0.25*( rgb2[i]        & 0xff))
                          | (int)((rgb1[i] >> 24) & 0xff )<<24);
        }
        for(int i=rgb1.Length/4; i<2*rgb1.Length/4; i++)
        {
            rgbRes[i] = ((int)(0.5*((rgb1[i] >> 16) & 0xff ) + 0.5*((rgb2[i] >> 16) & 0xff))<<16
                          | (int)(0.5*((rgb1[i] >> 8)  & 0xff ) + 0.5*((rgb2[i] >> 8)  & 0xff))<<8
                          | (int)(0.5*( rgb1[i]        & 0xff ) + 0.5*( rgb2[i]        & 0xff))
                          | (int)((rgb1[i] >> 24) & 0xff )<<24);
        }
        for(int i=2*rgb1.Length/4; i<3*rgb1.Length/4; i++)
        {
            rgbRes[i] = ((int)(0.5*((rgb1[i] >> 16) & 0xff ) + 0.5*((rgb2[i] >> 16) & 0xff))<<16
                          | (int)(0.5*((rgb1[i] >> 8)  & 0xff ) + 0.5*((rgb2[i] >> 8)  & 0xff))<<8
                          | (int)(0.5*( rgb1[i]        & 0xff ) + 0.5*( rgb2[i]        & 0xff))
                          | (int)((rgb1[i] >> 24) & 0xff )<<24);
        }
        for(int i=3*rgb1.Length/4; i<rgb1.Length; i++)
        {
            rgbRes[i] = ((int)(0.25*((rgb1[i] >> 16) & 0xff ) + 0.75*((rgb2[i] >> 16) & 0xff))<<16
                          | (int)(0.25*((rgb1[i] >> 8)  & 0xff ) + 0.75*((rgb2[i] >> 8)  & 0xff))<<8
                          | (int)(0.25*( rgb1[i]        & 0xff ) + 0.75*( rgb2[i]        & 0xff))
                          | (int)((rgb1[i] >> 24) & 0xff )<<24);
        }

        if(startLine)
//            imgOut.setRGB(0, nbLinesDone*(_patchSize-_widthBorder), _heightBorder, _widthBorder, rgbRes, 0, _heightBorder);
            imgOut.setRGB(rgbRes, 0, nbLinesDone*(_patchSize-_widthBorder), _heightBorder, _widthBorder, _heightBorder);
        else
//            imgOut.setRGB(_nbPatchPasted*(_patchSize-_widthBorder), nbLinesDone*(_patchSize-_widthBorder), _heightBorder, _widthBorder, rgbRes, 0, _heightBorder);
            imgOut.setRGB(rgbRes, _nbPatchPasted*(_patchSize-_widthBorder),
                          nbLinesDone*(_patchSize-_widthBorder), _heightBorder, _widthBorder, _heightBorder);
        return imgOut;
    } // borderW()

    /**
      * Change la largeur de la texture générée
      * @param finalTexture La nouvelle largeur
      */
    public /*synchronized*/ void set_widthFinalTexture(int finalTexture)
    {
        _widthFinalTexture = finalTexture;
    }


    /**
    * Change la hauteur de la texture générée
    * @param finalTexture La nouvelle hauteur
    */
    public /*synchronized*/ void set_heightFinalTexture(int finalTexture)
    {
        _heightFinalTexture = finalTexture;
    }


    /**
    * Méthode d accès à la taille su sample
    * @return _sampleSize
    */
    public /*synchronized*/ int get_sampleSize()
    {
        return _sampleSize;
    }
} // class TextureSynthesizer
