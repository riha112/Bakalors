using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Layer = System.Collections.Generic.List<double>;
using Network = System.Collections.Generic.List<System.Collections.Generic.List<double>>;

public class ConvolutionalLayer : MonoBehaviour
{
    public List<double[,]> filters;
    public bool doMaxPool = false;

    private ImageProcessor processor = new ImageProcessor();

    public void init(List<double[,]> filters, bool doMaxPool = false)
    {
        this.filters = filters;
        this.doMaxPool = doMaxPool;
    }

    public Texture2D[] feedForward(Texture2D input)
    {
        var output = new Texture2D[filters.Count];

        for (var f = 0; f < filters.Count; f++) {
            output[f] = processor.AddFilter(input, filters[f]);
            if (doMaxPool) {
                output[f] = processor.MaxPool(output[f]);
            }
        }

        return output;
    }

}
