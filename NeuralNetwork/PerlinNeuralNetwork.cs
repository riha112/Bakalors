using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class PerlinNeuralNetwork : MonoBehaviour
{
    NeuralNetwork network;
 
    public List<Texture2D> input;
    public List<Texture2D> target;

    public Texture2D fallback;

    public Texture2D perlinTexture;
    public Texture2D generatedTexture;

    public float PERLIN_STRENGTH = 20;
    public float COLOR_DELTA = 0.8f;

    private const string DATA_FILENAME = "network.dat";

    public bool hide = false;

    public int lId = 0;
    public int wId = 1;
    public int dId = 1;
    public int gId = 1;

    public int trainCount = 10;

    public int imgSize = 10;

    public int textureSize = 500;

    List<List<double>> inputData;
    List<List<double>> targetData;

    private ImageProcessor processor = new ImageProcessor();

    private ConvolutionalLayer convolutionalLayer;

    void Start()
    {
        network = GetComponent<NeuralNetwork>();
        convolutionalLayer = GetComponent<ConvolutionalLayer>();

        GeneratePrlinTexture();

        InitNet();
        TrainNetwork();

        GenerateNetTexture();
    }

    void TrainNetwork() {
        var start = (int)Random.Range(0, inputData.Count - 1);
        var end = (int)Random.Range(start, inputData.Count);

        var subInput = new List<double>[end - start + 1];
        var subTarget = new List<double>[end - start + 1];

        for (var i = start; i < end + 1 && i < inputData.Count; i++) {
            subInput[i - start] = inputData[i];
            subTarget[i - start] = targetData[i];
        }

        network.train(subInput, subTarget);
    }

    void GeneratePrlinTexture() {
        if (fallback != null) {
            perlinTexture = fallback;
            return;
        }

        perlinTexture = new Texture2D(textureSize, textureSize);
        for (var x = 0; x < textureSize; x++) {
            for (var y = 0; y < textureSize; y++) {
                var xCoordinate = (float) x / textureSize * PERLIN_STRENGTH;
                var yCoordinate = (float) y / textureSize * PERLIN_STRENGTH;
                var strength = Mathf.PerlinNoise(xCoordinate, yCoordinate) * COLOR_DELTA;
                perlinTexture.SetPixel(x, y, new Color(strength, strength, strength, 1));
            }
        }
        perlinTexture.Apply();
        perlinTexture = processor.normalizeColorRange(perlinTexture);
    }
    public int sampleRange;
    void GenerateNetTexture() {
        List<double> inputData = new List<double>();
        generatedTexture = new Texture2D(perlinTexture.width, perlinTexture.height);
        var step = (int)(imgSize * 0.8f);

        for (var x = 0; x < perlinTexture.width / step; x++) {
            for (var y = 0; y < perlinTexture.height / step; y++) {
                var inputTexture = processor.subImage(perlinTexture, imgSize, imgSize, x * step, y * step);
                var dataTextures = convolutionalLayer.feedForward(inputTexture);

               // inputTexture = processor.MaxPool(inputTexture);

                inputData = processor.ImageToVector(dataTextures[FilterTextureId]);
                var data = network.feedForward(inputData);

                for(var xi = 0; xi < imgSize; xi++)
                    for(var yi = 0; yi < imgSize; yi++) {
                        var c = (float)data[xi * imgSize + yi];
                        generatedTexture.SetPixel(xi + x * step, yi + y * step, new Color(c, c, c, 1));
                    }       
            }
        }
        generatedTexture.Apply();
        generatedTexture = processor.normalizeColorRange(generatedTexture);
        processor.blur(generatedTexture, 2);
        generatedTexture = processor.sampleImage(generatedTexture, sampleRange);
        generatedTexture = processor.normalizeColorRange(generatedTexture);
    }

    public int FilterTextureId = 0;

    void InitNet() {
        network.init(new int[]{ 
            (int)((imgSize - 2) * (imgSize - 2) * 0.25), 
            imgSize, 
            imgSize, 
            imgSize * imgSize 
        }, true);
        
        inputData = new List<List<double>>();
        targetData = new List<List<double>>();

        convolutionalLayer.init(new List<double[,]>{
            new double[,]{
                {-1, 0, 1},
                {-1, 0, 1},
                {-1, 0, 1}
            },
            new double[,]{
                {1, 0, -1},
                {1, 0, -1},
                {1, 0, -1}
            },
            new double[,]{
                {-1, -1, -1},
                {0, 0, 0},
                {1, 1, 1}
            },
            new double[,]{
                {1, 1, 1},
                {0, 0, 0},
                {-1, -1, -1}
            },
            new double[,]{
                {-2, -1, 0},
                {-1, 1, 1},
                {0, 1, 2}
            },
            new double[,]{
                {-1, -1, -1},
                {-1, 8, -1},
                {-1, -1, -1}
            },
            new double[,]{
                {0.5, 0, 0.5},
                {0, 0.5, 0},
                {0.5, 0, 0.5}
            }
        }, true);

        var step = (int)(imgSize * 0.8f);

        for (var i = 0; i < input.Count; i++) {
            for (var x = 0; x < input[i].width / step; x++) {
                for (var y = 0; y < input[i].height / step; y++) {
                    var subTexture = processor.subImage(input[i], imgSize, imgSize, x * step, y * step);
                    var targetTexture = processor.subImage(target[i], imgSize, imgSize, x * step, y * step);

                    var targetDataTexture = processor.ImageToVector(targetTexture);
                    var dataTextures = convolutionalLayer.feedForward(subTexture);
                    for (var f = 0; f < dataTextures.Length; f++) {
                        var inputDataTexture = processor.ImageToVector(dataTextures[f]);
                        inputData.Add(inputDataTexture);
                        targetData.Add(targetDataTexture);
                    }
                }
            }
        }

        // for (var i = 0; i < input.Count; i++) {
        //     var sizeY = (int)Mathf.Floor((input[i].height / imgSize) / 1.5f);
        //     for (var r = 0; r < sizeY; r++) {
        //         for (var x = 0; x < input[i].width; x++) {

        //             var id = new List<double>();
        //             var td = new List<double>();

        //             for (var y = 0; y < imgSize; y++) {
        //                 id.Add(input[i].GetPixel(x,y + r * sizeY).r);
        //                 td.Add(target[i].GetPixel(x,y + r * sizeY).r);
        //             }

        //             if (x == 0) {
        //                 for (var y = 0; y < imgSize; y++) {
        //                     id.Add(0);
        //                 }
        //             } else {
        //                 for (var y = 0; y < imgSize; y++) {
        //                     id.Add(target[i].GetPixel(x - 1,y + r * sizeY).r);
        //                 }
        //             }

        //             inputData.Add(id);
        //             targetData.Add(td);
        //         }
        //     }
        // }
    }

    void OnGUI()
    {
        if(GUILayout.Button("Re init") || Input.GetKeyUp(KeyCode.R)) {
            for(var i = 0; i < trainCount; i++) {
                InitNet();
            }
        }

        if(GUILayout.Button("Train") || Input.GetKeyUp(KeyCode.T)) {
            for(var i = 0; i < trainCount; i++) {
                TrainNetwork();
            }
        }
        if(GUILayout.Button("Adjust")) {
            for(var i = 0; i < trainCount; i++) {
                network.AdjustWeights();
            }
        }
        if(GUILayout.Button("Generate PN") || Input.GetKeyUp(KeyCode.P)) {
            GeneratePrlinTexture();
        }
        if(GUILayout.Button("Generate NN") || Input.GetKeyUp(KeyCode.N)) {
            GenerateNetTexture();
        }
        if (perlinTexture) {
            GUI.Box(new Rect(Screen.width / 2 - textureSize, Screen.height / 2 - textureSize / 2, textureSize, textureSize), perlinTexture);
        }
        if (generatedTexture) {
            GUI.Box(new Rect(Screen.width / 2, Screen.height / 2 - textureSize / 2, textureSize, textureSize), generatedTexture);
        }
    }

}
