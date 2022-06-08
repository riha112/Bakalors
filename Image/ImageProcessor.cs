using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageProcessor
{
    // 1 - white
    // 0 - black

    public Texture2D subImage(Texture2D input, int w, int h, int x = 0, int y = 0) {
        var output = new Texture2D(w, h);
        for (var xi = x; xi < x + w; xi++) {
            for (var yi = y; yi < y + h; yi++) {
                var c = input.GetPixel(xi, yi).r;
                output.SetPixel(xi - x, yi - y, new Color(c,c,c,1));
            }
        }
        output.Apply();

        return output;
    }

    public Texture2D intensify(Texture2D input, float strengt = 1) {
        var output = new Texture2D(input.width, input.height);
        for (var x = 0; x < input.width; x++)
            for (var y = 0; y < input.height; y++) {
                var c = input.GetPixel(x, y).r;
                c *= (c/strengt);
                output.SetPixel(x, y, new Color(c, c, c, 1));
            }
        output.Apply();

        return output;
    }

    public Texture2D blendByTexture(Texture2D origin, Texture2D mask, Texture2D blend, float strength) {
        var output = new Texture2D(origin.width, origin.height);
        for (var x = 0; x < origin.width; x++)
            for (var y = 0; y < origin.height; y++) {
                var z = origin.GetPixel(x, y).r;
                var n = mask.GetPixel(x, y).r;
                var k = blend.GetPixel(x, y).r;

                var c = z - (n * k) / strength;

                output.SetPixel(x, y, new Color(c, c, c, 1));
            }
        output.Apply();

        return output;
    }

    public Texture2D blend(Texture2D origin, Texture2D mask, float strength) {
        var output = new Texture2D(origin.width, origin.height);
        for (var x = 0; x < origin.width; x++)
            for (var y = 0; y < origin.height; y++) {
                var z = origin.GetPixel(x, y).r;
                var n = mask.GetPixel(x, y).r;
                var c = z * n / strength;

                output.SetPixel(x, y, new Color(c, c, c, 1));
            }
        output.Apply();

        return output;
    }

    public void blur(Texture2D input, int radius) {
        for (var x = radius; x < input.width - radius; x++)
            for (var y = radius; y < input.height - radius; y++) {
                var sum = 0.0f;
                var i = 0;
                for (var xr = -radius; xr <= radius; xr++) {
                    for (var yr = -radius; yr <= radius; yr++) {
                        if (xr * xr + yr * yr > radius * radius) {
                            continue;
                        }
                        i++;
                        sum += input.GetPixel(x + xr, y + yr).r;
                    }
                }
                var c = sum / i;
                input.SetPixel(x, y, new Color(c, c, c, 1));
            }
        input.Apply();
    }

    // public static Texture2D smoothen(Texture2D input, int strength) {
        
    // }

    public Texture2D perlinizeImage(Texture2D input, int octave = 7, float zoom = 1.0f) {
        var perlin = new PerlinNoise(input, octave, zoom);
        var perlinData = perlin.GetPerlinNoise();

        var output = new Texture2D(input.width, input.height);
        for (var x = 0; x < input.width; x++)
            for (var y = 0; y < input.height; y++) {
                var c = perlinData[x][y];
                output.SetPixel(x, y, new Color(c, c, c, 1));
            }
        output.Apply();

        return output;
    }

    public Texture2D normalizeColorRange(Texture2D input) {
        var max = 0.0f;
        var min = 1.0f;
        for (var x = 0; x < input.width; x++)
            for (var y = 0; y < input.height; y++) {
                var v = input.GetPixel(x, y).r;
                if (v > max) max = v;
                if (v < min) min = v;
            }

        max -= min;

        var output = new Texture2D(input.width, input.height);
        for (var x = 0; x < input.width; x++)
            for (var y = 0; y < input.height; y++) {
                var c = (input.GetPixel(x, y).r - min) / max;
                output.SetPixel(x, y, new Color(c, c, c, 1));
            }
        output.Apply();

        return output;
    } 

    public Texture2D sampleImage(Texture2D input, int sampleRange) {
        var output = new Texture2D(input.width - sampleRange * 2 - 1,  input.height - sampleRange * 2 - 1);
        output.filterMode = FilterMode.Point;
        for (var x = sampleRange; x < input.width - sampleRange; x++) {
            for (var y = sampleRange; y < input.height - sampleRange; y++) {
                var mmin = 0.8f;
                var mmax = 0.2f;

                for (var xi = -sampleRange; xi <= sampleRange; xi++) {
                    for (var yi = -sampleRange; yi <= sampleRange; yi++) {
                        var dif = Random.Range(-3, 2) + sampleRange;
                        if (xi * xi + yi * yi > dif * dif) {
                            continue;
                        }
                        var om = input.GetPixel(x + xi, y + yi).r;
                        if (om > mmax) mmax = om;
                        if (om < mmin) mmin = om;
                    }
                }

                var c = input.GetPixel(x, y).r * mmax * mmin;
                output.SetPixel(x - sampleRange,y - sampleRange,new Color(c, c, c, 1));
            }
        }
        output.Apply();

        return output;
    }

    public Texture2D AddFilter(Texture2D image, double[,] matrix, float power = 1) {
        var output = new Texture2D(image.width - 2, image.height - 2);
        output.filterMode = FilterMode.Point;

        for (var x = 1; x < image.width - 1; x++)
        {
            for (var y = 1; y < image.height - 1; y++)
            {
                float dot = 0;
                for (var xi = -1; xi <= 1; xi++)
                {
                    for (var yi = -1; yi <= 1; yi++)
                    {
                        var ic = image.GetPixel(x + yi, y + xi).r;
                        dot += ic * (float)matrix[yi + 1,xi + 1];
                    }
                }

                var c = dot * power;
                output.SetPixel(x - 1, y - 1, new Color(c, c, c, 1));
            }
        }

        output.Apply();
        return output;
    }

    public Texture2D MaxPool(Texture2D input, int size = 2) {
        var output = new Texture2D(input.width / size, input.height / size);

        for (var x = 0; x < output.width; x++) {
            for (var y = 0; y < output.height; y++) {

                var max = 0.0f;
                for (var xi = 0; xi < size; xi++) {
                    for (var yi = 0; yi < size; yi++) {
                        var mxc = input.GetPixel(x * size + xi, y * size + yi).r;
                        if (mxc > max) max = mxc;
                    }
                }

                output.SetPixel(x, y, new Color(max, max, max, 1));
            }
        }

        output.Apply();
        return output;
    }

    public List<double> ImageToVector(Texture2D input) {
        var output = new List<double>();
        for (var x = 0; x < input.width; x++) {
            for (var y = 0; y < input.height; y++) {
                output.Add(input.GetPixel(x, y).r);
            }
        }
        return output;
    }
}
