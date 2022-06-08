using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrepareImage : MonoBehaviour
{
    // Images with resolution of 500x500
    public List<Texture2D> dataSet;

    public int subRegionSize = 10;
    public int newResolution = 100;

    private ImageProcessor processor = new ImageProcessor();

    void Start()
    {
        var vectors = new List<List<double>>();
        for (var i = 0; i < vectors.Count; i++) {
            var textures = getImageRegion(dataSet[i]);
            var resizedTextures = resize(dataSet[i], newResolution);
            textures.AddRange(getImageRegion(resizedTextures));

            foreach (var texture in textures)
            {
                vectors.Add(processor.ImageToVector(texture));
            }
        }

        Debug.Log(vectorsToSqlInsert(vectors));
    }

    /**
     * Returns segmented image.
     * Splits original image into N x N sub textures and then returns all of them as list
     */
    List<Texture2D> getImageRegion(Texture2D image)
    {
        var width = image.width;
        var height = image.height;

        var output = new List<Texture2D>();
        for (var x = 0; x < width / subRegionSize; x++) {
            for (var y = 0; y < height / subRegionSize; y++) {
                var texture = new Texture2D(subRegionSize, subRegionSize);

                for (var xi = 0; xi < subRegionSize; xi++) {
                    for (var yi = 0; yi < subRegionSize; yi++) {
                        var c = image.GetPixel(x * subRegionSize + xi, y * subRegionSize + yi);
                        texture.SetPixel(xi, yi, c);
                    }
                }

                texture.Apply();
                texture = processor.normalizeColorRange(texture);
                var i = lightestRegion(texture);
                if (i == 1) {
                    texture = flipHorizontal(texture);
                } else if (i == 2) {
                    texture = flipVertical(texture);
                } else if (i == 3) {
                    texture = flipVerticalHorizontal(texture);
                }
                output.Add(texture);
            }
        }

        return output;
    }

    /**
     * Downsizes image to specific region by getting average color of subspace
     */
    Texture2D resize(Texture2D image, int newResoltion)
    {
        var step = image.height / newResoltion;

        var output = new Texture2D(newResoltion, newResoltion);
        for (var x = 0; x < newResolution; x++) {
            for (var y = 0; y < newResoltion; y++) {
                var c = .0f;
                for (var xi = x * step; xi < x * step + step; xi++) {
                    for (var yi = y * step; yi < y * step + step; yi++) {
                        c += output.GetPixel(xi, yi).r;
                    }
                }
                c /= (step * step);

                output.SetPixel(x, y, new Color(c, c, c));
            }
        }
        return output;
    }

    Texture2D flipHorizontal(Texture2D image) {
        var output = new Texture2D(image.width, image.height);
        for (var x = 0; x < image.width; x++) {
            for (var y = 0; y < image.height; y++) {
                output.SetPixel(x, y, image.GetPixel(image.width - 1 - x, y));
            }
        }
        output.Apply();
        return output;
    }

    Texture2D flipVertical(Texture2D image) {
        var output = new Texture2D(image.width, image.height);
        for (var x = 0; x < image.width; x++) {
            for (var y = 0; y < image.height; y++) {
                output.SetPixel(x, y, image.GetPixel( x, image.height - 1 - y));
            }
        }
        output.Apply();
        return output;
    }

    Texture2D flipVerticalHorizontal(Texture2D image) {
        var output = new Texture2D(image.width, image.height);
        for (var x = 0; x < image.width; x++) {
            for (var y = 0; y < image.height; y++) {
                output.SetPixel(x, y, image.GetPixel(image.width - 1 - x, image.height - 1 - y));
            }
        }
        output.Apply();
        return output;
    }


    /**
     * Finds lightest quadrant of image
     */
    int lightestRegion(Texture2D image) {
        var half = image.height / 2;

        // Top-left
        var tl = .0f;
        for (var x = 0; x < half; x++) {
            for (var y = 0; y < half; y++) {
                tl += image.GetPixel(x, y).r;
            }
        }

        // Top-right
        var tr = .0f;
        for (var x = half; x < image.width; x++) {
            for (var y = 0; y < half; y++) {
                tr += image.GetPixel(x, y).r;
            }
        }

        // Bottom-left
        var bl = .0f;
        for (var x = 0; x < half; x++) {
            for (var y = half; y < image.height; y++) {
                bl += image.GetPixel(x, y).r;
            }
        }

        // Bottom-right
        var br = .0f;
        for (var x = half; x < image.width; x++) {
            for (var y = half; y < image.height; y++) {
                br += image.GetPixel(x, y).r;
            }
        }

        if (tl < tr && tl < bl && tl < br) {
            return 0;
        }

        if (tr < tl && tr < bl && tr < br) {
            return 1;
        }

        if (bl < tl && bl < tr && bl < br) {
            return 2;
        }

        return 3;
    }

    // Generates SQL insert for DB
    public string vectorsToSqlInsert(List<List<double>> vectors) {
        var output = "INSERT INTO cmp_set VALUES\n";
        foreach(var vector in vectors) {
            output += "(";
            for (var i = 0; i < vector.Count; i++) {
                output += vector[i];
                if (i != vector.Count - 1) {
                    output += ',';
                }
            }
            output += "),\n";
        }
        return output;
    }
}
