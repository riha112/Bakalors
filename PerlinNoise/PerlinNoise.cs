using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoise
{
    float[][] noiseSeed;
    float[][] perlinNoise;
    
    int outputWidth = 256;
    int outputHeight = 256;

    public PerlinNoise(Texture2D input, int octave, float bias) {
        outputHeight = input.height;
        outputWidth = input.width;

        noiseSeed = new float[outputWidth][];
        perlinNoise = new float[outputWidth][];
        for (var i = 0; i < outputWidth; i++){
            noiseSeed[i] = new float[outputHeight];
            perlinNoise[i] = new float[outputHeight];
        };

        for (var x = 0; x < outputWidth; x++) {
            for (var y = 0; y < outputHeight; y++) {
                noiseSeed[x][y] = input.GetPixel(x, y).r;
            }
        }

        Interpolate(octave, bias);
    }

    public float[][] GetPerlinNoise() {
        return perlinNoise;
    }

    protected void Interpolate(int octave, float bias) {
        for (var x = 0; x < outputWidth; x++) {
            for (var y = 0; y < outputHeight; y++) {
                float fNoise = 0.0f;
				float fScaleAcc = 0.0f;
				float fScale = 1.0f;
                
				for (int o = 0; o < octave; o++)
				{
					int nPitch = outputWidth >> o;
					int nSampleX1 = (x / nPitch) * nPitch;
					int nSampleY1 = (y / nPitch) * nPitch;
					
					int nSampleX2 = (nSampleX1 + nPitch) % outputWidth;					
					int nSampleY2 = (nSampleY1 + nPitch) % outputWidth;

					float fBlendX = (float)(x - nSampleX1) / (float)nPitch;
					float fBlendY = (float)(y - nSampleY1) / (float)nPitch;

					float fSampleT = (1.0f - fBlendX) * noiseSeed[nSampleX1][nSampleY1] + fBlendX * noiseSeed[nSampleX2][nSampleY1];
					float fSampleB = (1.0f - fBlendX) * noiseSeed[nSampleX1][nSampleY2] + fBlendX * noiseSeed[nSampleX2][nSampleY2];

					fScaleAcc += fScale;
					fNoise += (fBlendY * (fSampleB - fSampleT) + fSampleT) * fScale;
					fScale = fScale / bias;
				}

				// Scale to seed range
				perlinNoise[x][y] = fNoise / fScaleAcc;
                var vv = noiseSeed[x][y];
            }
        }
    }

}
