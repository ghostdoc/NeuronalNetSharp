﻿using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;
using NeuronalNetSharp.Core.Interfaces;
using NeuronalNetSharp.Import.Interfaces;

namespace NeuronalNetSharp.Core.LearningAlgorithms
{
    // TODO
    // Label Matrix erstellen
    // 
    public class BackpropagationLearningAlgorithm
    {
        public BackpropagationLearningAlgorithm(INeuronalNetwork neuronalNetwork, ICollection<IDataset> traningData)
        {
            NeuronalNetwork = neuronalNetwork;
            LabelMatrieMatrices = new Dictionary<string, Matrix>();
            TrainingData = traningData;

            // Initialize Label Matrices
            var distinctLabels = traningData.Select(x => x.Label).Distinct().ToList();
            for (var i = 0; i < distinctLabels.Count; i++)
            {
                var matrix = DenseMatrix.OfColumnArrays(new double[distinctLabels.Count()]);
                matrix[i, 0] = 1;
                LabelMatrieMatrices.Add(distinctLabels[i], matrix);
            }
        }

        public INeuronalNetwork NeuronalNetwork { get; set; }

        public IEnumerable<IDataset> TrainingData { get; set; }

        public IDictionary<string, Matrix> LabelMatrieMatrices { get; set; }

        public INeuronalNetwork TrainNetwork(int iterations)
        {
            throw new NotImplementedException();
        }

        public double ComputeCostRegularized(double lambda)
        {
            // Calculate cost.
            var cost = 0.0;
            foreach (var dataset in TrainingData)
            {
                var result = NeuronalNetwork.ComputeOutput(dataset.Data);
                var labelmatrix = LabelMatrieMatrices[dataset.Label];

                var tmpCost =
                    -labelmatrix.PointwiseMultiply(result.Map(Math.Log)) -
                    (1 - labelmatrix).PointwiseMultiply(result.Map(d => Math.Log(1 - d)));
                cost = tmpCost.RowSums().Sum();
            }

            // Calculate regularization term.
            var reg = 0.0;
            foreach (var weightVector in NeuronalNetwork.Weights)
            {
                reg += weightVector.SubMatrix(0, weightVector.RowCount, 1, weightVector.ColumnCount - 1).Map(d => Math.Pow(d, 2)).ColumnSums().Sum();
            }

            reg = reg*(lambda/(2*TrainingData.Count()));

            return cost + reg;
        }

        #region MapIndexed

        //NeuronalNetwork.Weights[0].MapIndexed((i, i1, arg3) =>

        //{
        //    return arg3;
        //});

        #endregion
    }
}