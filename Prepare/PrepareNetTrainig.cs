using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class PrepareNetTrainig : MonoBehaviour
{
    public Texture2D target;
    public Texture2D input;

    int imgSize = 25;

    void Start() {
        MakeInputTexture();
        Save();
    }


    void MakeInputTexture() {

        AddFilter(new double[3,3]{{0.0625, 0.125, 0.0625},{0.125, 0.25, 0.125}, {0.0625, 0.125, 0.0625}}, target);

        
        // for (var i = 0; i < 1; i++) {
        //     AddFilter(new double[3,3]{{0, -1, 0}, {-1, 4, -1}, {0, -1, 0}}, input, 55);
        // }

        // 
        // for(var i = 0; i < 1; i++) {
        //     AddFilter(new double[3,3]{{0.0625, 0.125, 0.0625},{0.125, 0.25, 0.125}, {0.0625, 0.125, 0.0625}}, input);
        // }
        CorrectColorRange();
        var perlin = new PerlinNoise(input, 8, 1);
        var perlinData = perlin.GetPerlinNoise();


        input = new Texture2D(target.width, target.height);

        float min = 1;
        float max = 0;
        for (var x = 0; x < target.width; x++) {
            for (var y = 0; y < target.height; y++) {
                var c = perlinData[x][y];
                if (c > max) max = c;
                if (c < min) min = c;
            }
        }

        max -= min;

        for (var x = 0; x < target.width; x++) {
            for (var y = 0; y < target.height; y++) {
                var c = perlinData[x][y] - min;
                c = c / max;
                input.SetPixel(x, y, new Color(c, c, c, 1.0f));
            }
        }

        input.Apply();

        // for (var i = 0; i < 1; i++) {
        //     AddFilter(new double[3,3]{{-1, 0, 1}, {-1, 0, 1}, {-1, 0, 1}}, input, 55);
        // }
        // for (var i = 0; i < 1; i++) {
        //     AddFilter(new double[3,3]{{-2, -1, 0}, {-1, 1, 1}, {0, 1, 2}}, input);
        // }
    }

    void CorrectColorRange() {
        var size = input.width;
        for (var x = 0; x < size; x+=1)
        {
            for (var y = 0; y < size; y+=1)
            {
                var i = input.GetPixel(x, y).r;
                float clamp = (int)(i* 15) / 15.0f;
                input.SetPixel(x, y, new Color(clamp, clamp, clamp, 1));
            }
        }
        input.Apply();
    }

    void AddFilter(double[,] matrix, Texture2D source, float mult = 1) {
        var size = source.width;

        var texture2 = new Texture2D(size, size);
        texture2.filterMode = FilterMode.Point;

        for (var x = 0; x < size - 3; x+=1)
        {
            for (var y = 0; y < size -3; y+=1)
            {
                float s = 0;
                float so = 0;

                for (var xi = 0; xi < 3; xi++)
                {
                    for (var yi = 0; yi < 3; yi++)
                    {
                        Color c = source.GetPixel(x + yi, y + xi);
                        s += c.r * (float)matrix[yi,xi];
                        so += c.r;
                    }
                }
                var inten = s * mult;//texture.GetPixel(x + 1, y + 1).r * (s * multi / so);
                Color cc = new Color(inten, inten, inten, 1);

                texture2.SetPixel(x + 1, y + 1, cc);
            }
        }

        texture2.Apply();
        input = texture2;
    }

    void OnGUI() {
        if (input) {
            GUI.Box(new Rect(0, 300, 500, 500), target);
            GUI.Box(new Rect(500, 300, 500, 500), input);
        }
    }

    void Save() {
        var newTextureI = new Texture2D(target.width - 100, target.height - 100);
        var newTextureT = new Texture2D(target.width - 100, target.height - 100);
        for(var x = 50; x < target.width - 50; x++) {
            for (var y = 50; y < target.height - 50; y++) {
                newTextureI.SetPixel(x - 50, y - 50, input.GetPixel(x,y));
                newTextureT.SetPixel(x - 50, y - 50, target.GetPixel(x,y));
            }
        }
        newTextureI.Apply();
        newTextureT.Apply();
        SaveImage(newTextureI, $"input2_{0}", "./Training/");
        SaveImage(newTextureT, $"target2_{0}", "./Training/");

        // int step = imgSize - 5;
        // for(var x = 0; x < (target.width - 100) / step ; x++) {
        //     for (var y = 0; y < (target.height - 100) / step; y++) {

        //         var newTextureT = new Texture2D(imgSize, imgSize);
        //         var newTextureI = new Texture2D(imgSize, imgSize);

        //         for (var xi = 0; xi < imgSize; xi++) {
        //             for (var yi = 0; yi < imgSize; yi++) {
        //                 newTextureT.SetPixel(xi,yi,target.GetPixel(50 + x * step + xi,50 + y * step + yi));
        //                 newTextureI.SetPixel(xi,yi,input.GetPixel(50 + x * step + xi, 50+ y * step + yi));

        //             }
        //         }




        //     }
        // }
    }

    void SaveImage(Texture2D image, string name, string path) {
        byte[] bytes = image.EncodeToPNG();
        var dirPath = Application.dataPath + path;
        if(!Directory.Exists(dirPath)) {
            Directory.CreateDirectory(dirPath);
        }
        File.WriteAllBytes(dirPath + name + ".png", bytes);
    }
}
