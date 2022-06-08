using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    NeuralNetwork network;
 
    public Texture2D image;
    public int ocatve = 1;
    public int bias = 1;
    public Texture2D imagePerlin;

    void Start()
    {
        network = GetComponent<NeuralNetwork>();
        network.init(new int[]{ 2, 5, 5, 1 });
        
        int size = 1000000;

        List<double>[] input = new List<double>[size];
        List<double>[] target = new List<double>[size];

        for(var i = 0; i < size; i++) {
            input[i] = new List<double>();
            target[i] = new List<double>();

            double x = ((int)Mathf.Floor(Random.Range(-50, 60)) / 100.0);
            double y = ((int)Mathf.Floor(Random.Range(-50, 60)) / 100.0);
            double z = x + y;

            input[i].Add(x);
            input[i].Add(y);
            target[i].Add(z);
        }

        network.train(input, target);


    }

    int valX, valY;
    void OnGUI()
    {
        valX = int.Parse(GUILayout.TextField("" + valX));
        valY = int.Parse(GUILayout.TextField("" + valY)); 
        if(GUILayout.Button("Sum")){
            var data = new List<double>();
            data.Add(valX / 100.0);
            data.Add(valY / 100.0);
            Debug.Log(System.Math.Round(network.feedForward(data)[0] * 100));
        }

        if(GUILayout.Button("Perlin")){
            var perlin = new PerlinNoise(image, ocatve, bias);
            var perlinData = perlin.GetPerlinNoise();

            imagePerlin = new Texture2D(image.width, image.height);
            for (var x = 0; x < image.width; x++) {
                for (var y = 0; y < image.height; y++) {
                    var c = perlinData[x][y];
                    imagePerlin.SetPixel(x, y, new Color(c, c, c, 1.0f));
                }
            }
            imagePerlin.Apply();
        }

        if (imagePerlin) {
            GUILayout.Box(imagePerlin);
        }
    }
}
