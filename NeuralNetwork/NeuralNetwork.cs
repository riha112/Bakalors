using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Layer = System.Collections.Generic.List<double>;
using Network = System.Collections.Generic.List<System.Collections.Generic.List<double>>;

[System.Serializable]
public class NeuralNetwork : MonoBehaviour
{
    public Network nodeNetwork;
    public Network gradientNetwork;

    public double[][][] weights;
    public double[][][] deltaWeights;

    const double ETA = 0.15;
    const double ALPHA = 0.5;

    public double recentAverrageError, recentAverrageErrorSmoothingFactor = 100;

    private bool adjust = false;
    public void init(int[] topology, bool adjust = false)
    {
        this.adjust = adjust;

        // Initializes network nodes
        nodeNetwork = new Network(topology.Length);
        gradientNetwork = new Network(topology.Length);

        for (var i = 0; i < topology.Length; i++) {
            nodeNetwork.Add(new Layer(topology[i]));
            gradientNetwork.Add(new Layer(topology[i]));
            for (var n = 0; n <= topology[i]; n++) {
                nodeNetwork[i].Add(1);
                gradientNetwork[i].Add(0);
            }
        }

        // Initializs network weights for fully connected network
        // mening N(L(n,1))->N(L(n+1,y)) 
        weights = new double[topology.Length - 1][][];
        deltaWeights = new double[topology.Length - 1][][];

        for (var i = 0; i < topology.Length - 1; i++) {
            weights[i] = new double[topology[i] + 1][];
            deltaWeights[i] = new double[topology[i] + 1][];
            for (var x = 0; x < topology[i] + 1; x++) {
                weights[i][x] = new double[topology[i+1] + 1];
                deltaWeights[i][x] = new double[topology[i+1] + 1];

                for (var y = 0; y < topology[i+1] + 1; y++) {
                    weights[i][x][y] = ((double)Random.Range(-1000000,1000000) / 1000000.0);
                    deltaWeights[i][x][y] = 0;
                }
            }
        }

        AdjustWeights();
    }

    public void AdjustWeights()
    {
        for (var i = 0; i < nodeNetwork[0].Count; i++) {
            nodeNetwork[0][i] = 1;
        }

        for (var l = 1; l < nodeNetwork.Count; l++) {
            for (var n = 0; n < nodeNetwork[l].Count - 1; n++) {
                double sum = 0.0;
                for (var pn = 0; pn < nodeNetwork[l-1].Count; pn++) {
                    sum += nodeNetwork[l-1][pn] * weights[l-1][pn][n];
                }
                if (sum > 1 || sum < -1) {
                    for (var pn = 0; pn < nodeNetwork[l-1].Count; pn++) {
                        var c = (double)nodeNetwork[l-1].Count;
                        var a = System.Math.Abs(sum) + 1;
                        double div = a / c;
                        weights[l-1][pn][n] *= 0.92;
                    }
                }
                nodeNetwork[l][n] = System.Math.Tanh(sum);
            }
        }

    }

    public Layer? feedForward(Layer input)
    {
        if (input.Count != nodeNetwork[0].Count - 1) {
            Debug.Log("Incorrect input count!");
            return null;
        }

        for (var i = 0; i < input.Count; i++) {
            nodeNetwork[0][i] = input[i];
        }

        for (var l = 1; l < nodeNetwork.Count; l++) {
            for (var n = 0; n < nodeNetwork[l].Count - 1; n++) {
                double sum = 0.0;
                for (var pn = 0; pn < nodeNetwork[l-1].Count; pn++) {
                    sum += nodeNetwork[l-1][pn] * weights[l-1][pn][n];
                }
                // if(sum > 1 || sum < -1) {
                //     double part = sum - ((int)System.Math.Floor(sum) * ((sum > 1) ? 1 : -1));
                //     if ((int)System.Math.Floor(sum) % 2 == 0) {
                //         sum = 1 - part;
                //     } else {
                //         sum = part;
                //     }
                // }
                nodeNetwork[l][n] = System.Math.Tanh(sum);
            }
        }

        return nodeNetwork[nodeNetwork.Count - 1];
    }

    protected void backPropagation(Layer target)
    {
        if (target.Count != nodeNetwork[nodeNetwork.Count - 1].Count - 1) {
            Debug.Log("Incorrect target count!");
            return;
        }

        // STEP 1. Calculates net error RMS
        Layer outputLayer = nodeNetwork[nodeNetwork.Count - 1];
        double error = 0.0;

        for (var n = 0; n < outputLayer.Count - 1; n++)
        {
            double delta = target[n] - outputLayer[n];
            error += delta * delta;
        }

        error /= outputLayer.Count - 1;
        error = System.Math.Sqrt(error); // RMS

       // Debug.Log($"error: {error}");

        // STEP 1.1. Recent average - debuging
        recentAverrageError = 
            (recentAverrageError * recentAverrageErrorSmoothingFactor + error) /
            (recentAverrageErrorSmoothingFactor + 1.0);

        // STEP 2.  Calculate output layer gradients
        for(var n = 0 ; n < outputLayer.Count - 1; ++n){
            double delta = target[n] - outputLayer[n];
            double value = delta * transferFunctionDerivative(outputLayer[n]);
            gradientNetwork[gradientNetwork.Count - 1][n] = value;// != 0 ? value : target[n];
            
        }

        // STEP 3. Calculate gradients for hidden layers
        // From right to left, without output layer
        for (var l = nodeNetwork.Count - 2; l > 0; l--)
        {
            Layer hiddenLayer = nodeNetwork[l];
            Layer nextLayer = nodeNetwork[l + 1];

            for(var n = 0; n < hiddenLayer.Count - 1; n++) {
                double dow = 0.0;

                for (var nl = 0; nl < nextLayer.Count; nl++) {
                    dow += weights[l][n][nl] * gradientNetwork[l + 1][nl];
                }

                gradientNetwork[l][n] = dow * transferFunctionDerivative(hiddenLayer[n]);
            }
        }

        // Step 4. Updates connection weights
        for(var l = nodeNetwork.Count - 1; l > 0; l--)
        {
            Layer layer = nodeNetwork[l];
            Layer prevLayer = nodeNetwork[l - 1];

            for (var n = 0; n < layer.Count - 1; n++) {
                for (var pn = 0; pn < prevLayer.Count; pn++) {
                    double oldDeltaWeight = deltaWeights[l - 1][pn][n];

                    // Individual input, magnified by the gradient and train rate:
                    double eta = ETA *	// eta - overall net learning rate: 0.0 - slow learner, 0.2 - medium learner, 1.0 - reckless learner
                        prevLayer[pn] *
                        gradientNetwork[l][n];

                    // Also add momentum = a fraction of the previous delta weight
                    double alpha = ALPHA *  // alpha - momentum: 0.0 - no momentum, 0.5 - moderate momentum 
                        oldDeltaWeight;

                    double newDeltaWeight = eta + alpha;

                   // if(n == 10 && pn % 30 == 1)
                   // Debug.Log($"delta w: {newDeltaWeight} | {eta} | {alpha} |" + prevLayer[pn] + ".." + gradientNetwork[l][n]);
                    deltaWeights[l - 1][pn][n] = newDeltaWeight;
                    weights[l - 1][pn][n] += newDeltaWeight;
                }
            }
        }
    }

    public void train(Layer[] input, Layer[] target)
    {
        for (var i = 0; i < input.Length; i++) {
            // if(i%3 == 0 && this.adjust){
            //     AdjustWeights();
            // }
            
            if (input == null) { Debug.Log("Skip:" + i); continue; }
            var output = feedForward(input[i]);
            backPropagation(target[i]);
            if (i > input.Length - 100) {
           //     Debug.Log(input[i][0] + " + " + input[i][1] + " is " + output[50] +  $" | {recentAverrageError}");
            }
        }
    }

    private double transferFunctionDerivative(double value) {
        return 1.0 - value * value;
        // System.Math.Tanh(value) * System.Math.Tanh(value);
    }
}
