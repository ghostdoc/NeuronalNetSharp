﻿namespace NeuronalNetSharp.Console
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Core;
    using Core.EventArgs;
    using Core.LearningAlgorithms;
    using Import;
    using MathNet.Numerics;
    using MathNet.Numerics.Distributions;
    using MathNet.Numerics.LinearAlgebra.Double;
    using MathNet.Numerics.Providers.LinearAlgebra.Mkl;

    internal class Program
    {
        private static void Main()
        {
            //Control.NativeProviderPath = "x64/";

            //Control.NativeProviderPath = "x64";
            Control.LinearAlgebraProvider = new MklLinearAlgebraProvider();
            Control.UseNativeMKL();

            var importer = new MinstImporter();
            var rawData = importer.ImportData(@"train-images-idx3-ubyte",
                @"train-labels-idx1-ubyte").ToList();

            var network = new NeuronalNetwork(784, 1, 10);
            var backprob = new BackpropagationLearningAlgorithm(network);

            backprob.IterationFinished += Test;

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var test = backprob.TrainNetwork(100, 0.01, 0, rawData.Take(100).ToList());
            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed);



            var tmp1 = rawData[6000].Label;
            var tmp2 = rawData[5323].Label;

            var test1 = network.ComputeOutput(rawData[25].Data);
            var test2 = network.ComputeOutput(rawData[33].Data);
            var test3 = network.ComputeOutput(rawData[37].Data);
            var test4 = network.ComputeOutput(rawData[12].Data);
            var test5 = network.ComputeOutput(rawData[6000].Data);
            var test6 = network.ComputeOutput(rawData[25].Data);
            var test7 = network.ComputeOutput(rawData[33].Data);
            var test8 = network.ComputeOutput(rawData[37].Data);
            var test9 = network.ComputeOutput(rawData[12].Data);
            var test10 = network.ComputeOutput(rawData[5323].Data);


            Console.WriteLine(Directory.GetCurrentDirectory());
            Console.ReadLine();
        }

        public static void Test(object sender, EventArgs e)
        {
            var args = (IterationFinishedEventArgs) e;
            Console.WriteLine(args.Cost);
        }
    }
}