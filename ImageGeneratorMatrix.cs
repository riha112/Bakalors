using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageGeneratorMatrix : MonoBehaviour
{
    public Texture2D perlinTexture;
    public Texture2D perlinTexture2;
    public int textureSize = 500;
    public float perlinStrength = 20;
    public float colorDelta = 0.8f;

    public double[] matrix1;
    public double[] matrix2;
    public double[] matrix3;

    public int sampleRange;

    public Texture2D blendableSource;

    private ImageProcessor processor = new ImageProcessor();

    Texture2D GeneratePrlinTexture(bool flip = false) {
        var pTexture = new Texture2D(textureSize, textureSize);
        for (var x = 0; x < textureSize; x++) {
            for (var y = 0; y < textureSize; y++) {
                var xCoordinate = (float) x / textureSize * perlinStrength;
                var yCoordinate = (float) y / textureSize * perlinStrength;
                var strength = Mathf.PerlinNoise((flip) ? yCoordinate : xCoordinate, (flip) ? xCoordinate : yCoordinate) * colorDelta;
                pTexture.SetPixel((flip) ? y : x, (flip) ? x : y, new Color(strength, strength, strength, 1));
            }
        }
        pTexture.Apply();
  //      pTexture = processor.sampleImage(pTexture, sampleRange);
        pTexture = processor.normalizeColorRange(pTexture);
        
        return pTexture;
    }

    public Texture2D blended;
    public void BlendImages() {
        blended = new Texture2D(textureSize, textureSize);
        for (var x = 0; x < textureSize; x++) {
            for (var y = 0; y < textureSize; y++) {
                var c1p = perlinTexture.GetPixel(x,y).r;
                var c2p = perlinTexture2.GetPixel(x,y).r;
                var c = (c2p - c1p) / 2 + (c2p * c1p);

                blended.SetPixel(x, y, new Color(c, c, c, 1));
            }
        }
        blended.Apply();
    }

    public void OnGUI() {
        if(GUILayout.Button("Generate perlin noise")) {
            perlinTexture = GeneratePrlinTexture();
        }

        if(GUILayout.Button("AplyMatrix")) {
            perlinTexture = processor.AddFilter(perlinTexture, new double[,]{
                { matrix1[0], matrix1[1], matrix1[2] },
                { matrix2[0], matrix2[1], matrix2[2] },
                { matrix3[0], matrix3[1], matrix3[2] }
            });
        }

        if(GUILayout.Button("MaxPool")) {
            perlinTexture = processor.MaxPool(perlinTexture);;
        }

        if(GUILayout.Button("Generate perlin noise 2")) {
            perlinTexture2 = GeneratePrlinTexture();
            perlinTexture2 = processor.MaxPool(perlinTexture2);
            // perlinTexture2 = processor.subImage(
            //     blendableSource,
            //     blended.width, 
            //     blended.height, 
            //     Random.Range(0, blendableSource.width - blended.width),
            //     Random.Range(0, blendableSource.height - blended.height)
            // );
           // perlinTexture2 = processor.normalizeColorRange(perlinTexture2);
           // perlinTexture2 = processor.intensify(perlinTexture2);
        }

        if(GUILayout.Button("Blend")) {
            BlendImages();
            blended = processor.normalizeColorRange(blended);
            blended = processor.perlinizeImage(blended, 8);
            blended = processor.normalizeColorRange(blended);
          //  blended = processor.intensify(blended);

            // var mask = processor.subImage(
            //     blendableSource,
            //     blended.width, 
            //     blended.height, 
            //     Random.Range(0, blendableSource.width - blended.width),
            //     Random.Range(0, blendableSource.height - blended.height)
            // );
            // blended = processor.blend(blended, mask, 1);
            // blended = processor.sampleImage(blended, sampleRange);
        }

        if(perlinTexture) {
            GUILayout.Box(perlinTexture);
        }

        if(perlinTexture2) {
            GUILayout.Box(perlinTexture2);
        }

        if(blended) {
            GUILayout.Box(blended);
        }
    }
}
