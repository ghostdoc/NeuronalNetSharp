﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuronalNetSharp.Core
{
    using System.ComponentModel;
    using Import;
    using Interfaces;
    using MathNet.Numerics.LinearAlgebra.Double;

    public static class NetworkTester
    {
        public static double TestNetwork(INeuronalNetwork network, IEnumerable<IDataset> data, IDictionary<string, Matrix> labelMatrices)
        {
            var testResult = new NetworkTestResult();
            var t = 0.0;
            var f = 0.0;

            foreach (var dataset in data)
            {
                var result = network.ComputeOutput(dataset.Data);
                var max = 0.0;
                var maxIndex = 0;

                for (var i = 0; i < result.RowCount; i++)
                {
                    var d = result[i, 0];
                    if (result[i, 0] > max)
                    {
                        max = result[i, 0];
                        maxIndex = i;
                    }
                }

                if (labelMatrices[dataset.Label][maxIndex, 0] >= 1)
                    t += 1.0;
                else
                    f += 1.0;
            }

            return f/(f+t);
        }
    }
}