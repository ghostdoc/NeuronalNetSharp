﻿namespace NeuronalNetSharp.Core.NeuronalNetwork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Import;
    using MathNet.Numerics.Distributions;
    using MathNet.Numerics.LinearAlgebra;
    using MathNet.Numerics.LinearAlgebra.Double;

    public class NeuronalNetwork : INeuronalNetwork
    {
        /// <summary>
        /// Initializes a new instance of a NeuronalNetwork.
        /// </summary>
        /// <param name="sizeInputLayer">The size of the input layer.</param>
        /// <param name="sizeOutputLayer">The size of the output layer.</param>
        /// <param name="numberOfHiddenLayers">The number of hidden layers.</param>
        /// <param name="lambda">The lambda value.</param>
        public NeuronalNetwork(int sizeInputLayer, int sizeOutputLayer, int numberOfHiddenLayers = 0,
            double lambda = 0.01)
        {
            SizeInputLayer = sizeInputLayer;
            SizeOutputLayer = sizeOutputLayer;
            NumberOfHiddenLayers = numberOfHiddenLayers;
            Lambda = lambda;

            Layers = new List<Matrix<double>>();
            Weights = new List<Matrix<double>>();
            BiasWeights = new List<Matrix<double>>();

            InitializeLayers();
            InitializeBiases();
            InitializeWeights();
        }

        public double Lambda { get; }

        /// <summary>
        /// The bias weights of the network.
        /// </summary>
        public IList<Matrix<double>> BiasWeights { get; }

        /// <summary>
        /// Compute the cost of the network.
        /// </summary>
        /// <param name="trainingData">The training data.</param>
        /// <param name="results">The desired results for each label.</param>
        /// <param name="lambda">The lambda value of the network.</param>
        /// <returns>The cost result set.</returns>
        public CostResultSet ComputeCostResultSet(IList<IDataset> trainingData,
            IDictionary<string, Matrix<double>> results, double lambda = 0)
        {
            var costResultSet = new CostResultSet
            {
                Cost = ComputeCost(trainingData, results, lambda),
                Gradients = ComputeGradients(trainingData, results)
            };

            return costResultSet;
        }

        public GradientResultSet ComputeNumericalGradients(IList<IDataset> trainingData,
            IDictionary<string, Matrix<double>>  results, double lambda, double epsilon)
        {
            var resultSet = new GradientResultSet
            {
                Gradients = HelperFunctions.InitializeMatricesWithSameDimensions(Weights),
                BiasGradients = HelperFunctions.InitializeMatricesWithSameDimensions(BiasWeights)
            };


                for (var k  = 0; k < Weights.Count; k++)
                {
                    for (var i = 0; i < Weights[k].RowCount; i++)
                    {
                        for (var j = 0; j < Weights[k].ColumnCount; j++)
                        {
                            Weights[k][i, j] = Weights[k][i, j] - epsilon;
                            var loss1 = ComputeCost(trainingData, results, lambda);

                            Weights[k][i, j] = Weights[k][i, j] + 2 * epsilon;
                            var loss2 = ComputeCost(trainingData, results, lambda);

                            Weights[k][i, j] = Weights[k][i, j] - epsilon;

                            resultSet.Gradients[k][i, j] = (loss2 - loss1)/(2*epsilon);
                        }
                    }
                }

            for (var k = 0; k < BiasWeights.Count; k++)
            {
                for (var i = 0; i < BiasWeights[k].RowCount; i++)
                {
                    Weights[k][i, 0] = Weights[k][i, 0] - epsilon;
                    var loss1 = ComputeCost(trainingData, results, lambda);

                    Weights[k][i, 0] = Weights[k][i, 0] + 2 * epsilon;
                    var loss2 = ComputeCost(trainingData, results, lambda);

                    Weights[k][i, 0] = Weights[k][i, 0] - epsilon;

                    resultSet.BiasGradients[k][i, 0] = (loss2 - loss1)/(2*epsilon);
                }
            }

            return resultSet;
        }

        /// <summary>
        /// Calcultes the output for a given input.
        /// </summary>
        /// <param name="input">Input for the network</param>
        /// <returns>Output for given input.</returns>
        public Matrix<double> ComputeOutput(Matrix<double> input)
        {
            if (input.RowCount != SizeInputLayer || input.ColumnCount > 1)
                throw new ArgumentException("Dimensions have to agree with the size of the input layer.");

            Layers[0] = input;

            for (var i = 0; i < Weights.Count; i++)
            {
                Layers[i + 1] = Weights[i]*Layers[i] + BiasWeights[i];
                Layers[i + 1].MapInplace(Functions.SigmoidFunction);
            }

            return Layers.Last();
        }

        /// <summary>
        /// The layers of the network.
        /// </summary>
        public IList<Matrix<double>> Layers { get; }

        /// <summary>
        /// The number of hidden layers.
        /// </summary>
        public int NumberOfHiddenLayers { get; }

        /// <summary>
        /// Set the size of a specific layer in the network.
        /// </summary>
        /// <param name="layer">The layer you want to change.</param>
        /// <param name="size">The new size.</param>
        public void SetLayerSize(int layer, int size)
        {
            if (layer == 0 || layer == Layers.Count - 1)
                throw new ArgumentException("You can't resize the input or output layer.");

            Layers[layer] = DenseMatrix.OfColumnArrays(new double[size]);

            InitializeBiases();
            InitializeWeights();
        }

        /// <summary>
        /// The size of the input layer.
        /// </summary>
        public int SizeInputLayer { get; }

        /// <summary>
        /// The size of the output layer.
        /// </summary>
        public int SizeOutputLayer { get; }

        /// <summary>
        /// The weights of the network.
        /// </summary>
        public IList<Matrix<double>> Weights { get; }

        /// <summary>
        /// Initialize the bias weiths of the network.
        /// </summary>
        private void InitializeBiases()
        {
            BiasWeights.Clear();
            var epsilon = Math.Sqrt(6)/Math.Sqrt(SizeInputLayer + SizeOutputLayer);

            for (var i = 0; i < Layers.Count - 1; i++)
                BiasWeights.Add(DenseMatrix.CreateRandom(Layers[i + 1].RowCount, 1,
                    new ContinuousUniform(-epsilon, epsilon)));
        }

        /// <summary>
        /// Initializes the layers of the network.
        /// </summary>
        private void InitializeLayers()
        {
            Layers.Add(DenseMatrix.OfColumnArrays(new double[SizeInputLayer]));

            for (var i = 0; i < NumberOfHiddenLayers; i++)
                Layers.Add(DenseMatrix.OfColumnArrays(new double[SizeInputLayer]));

            Layers.Add(DenseMatrix.OfColumnArrays(new double[SizeOutputLayer]));
        }

        /// <summary>
        /// Initilizes the weights of the network.
        /// </summary>
        private void InitializeWeights()
        {
            Weights.Clear();
            var epsilon = Math.Sqrt(6)/Math.Sqrt(SizeInputLayer + SizeOutputLayer);

            for (var i = 0; i < NumberOfHiddenLayers; i++)
                Weights.Add(DenseMatrix.CreateRandom(Layers[i + 1].RowCount, Layers[i].RowCount,
                    new ContinuousUniform(-epsilon, epsilon)));

            Weights.Add(DenseMatrix.CreateRandom(SizeOutputLayer, Layers.Reverse().Skip(1).FirstOrDefault().RowCount,
                new ContinuousUniform(-epsilon, epsilon)));
        }

        /// <summary>
        /// Computes the costs for the network.
        /// </summary>
        /// <param name="trainingData">The training data.</param>
        /// <param name="results">The desired results for the labels.</param>
        /// <param name="lambda">The lambda value.</param>
        /// <returns>The costs of the network.</returns>
        private double ComputeCost(IList<IDataset> trainingData, IDictionary<string, Matrix<double>> results,
            double lambda)
        {
            var cost = 0.0;
            var reg = 0.0;

            foreach (var dataset in trainingData)
            {
                var output = ComputeOutput(dataset.Data);
                var error = output - results[dataset.Label];
                cost += 1.0/2.0*Math.Pow(error.CalculateNorm(), 2);
            }
            cost = 1.0/trainingData.Count*cost;

            Parallel.ForEach(Weights, matrix => { reg += matrix.Map(x => Math.Pow(x, 2)).RowSums().Sum(); });
            reg = lambda/2*reg;

            return cost + reg;
        }

        /// <summary>
        /// Calculates the gradients of the weights of the network.
        /// </summary>
        /// <param name="trainingData">The traing data.</param>
        /// <param name="results">The desired results for the labels.</param>
        /// <returns>The gradients of the network weights.</returns>
        private GradientResultSet ComputeGradients(IList<IDataset> trainingData,
            IDictionary<string, Matrix<double>> results)
        {
            var deltas = HelperFunctions.InitializeMatricesWithSameDimensions(Weights);
            var biasDeltas = HelperFunctions.InitializeMatricesWithSameDimensions(BiasWeights);

            foreach (var dataset in trainingData)
            {
                var tmpDeltas = new List<Matrix<double>>();

                var output = ComputeOutput(dataset.Data);
                var error = -(results[dataset.Label] - output).PointwiseMultiply(output.Map(d => d*(1 - d)));
                tmpDeltas.Add(error);

                for (var i = Weights.Count - 1; i > 0; i--)
                {
                    var delta =
                        (Weights[i].Transpose()*tmpDeltas.Last()).PointwiseMultiply(Layers[i].Map(d => d*(1 - d)));
                    tmpDeltas.Add(delta);
                }
                tmpDeltas.Reverse();

                Parallel.For(0, tmpDeltas.Count, i =>
                {
                    deltas[i] = deltas[i] + tmpDeltas[i]*Layers[i].Transpose();
                    biasDeltas[i] = biasDeltas[i] + tmpDeltas[i];
                });
            }

            Parallel.ForEach(deltas, matrix => matrix.MapInplace(v => 1.0/trainingData.Count*v));
            Parallel.ForEach(biasDeltas, matrix => matrix.MapInplace(v => 1.0/trainingData.Count*v));

            return new GradientResultSet {BiasGradients = biasDeltas, Gradients = deltas};
        }
    }
}