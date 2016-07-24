﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NeuronalNetSharp.Core;
using NeuronalNetSharp.Core.NeuronalNetwork;
using NeuronalNetSharp.Core.Optimization;
using NeuronalNetSharp.Core.Performance;
using NeuronalNetSharp.Import;
using NeuronalNetSharp.WPF.Annotations;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace NeuronalNetSharp.WPF
{
    using MathNet.Numerics.LinearAlgebra;

    public class MainViewModel : INotifyPropertyChanged
    {
        private PlotModel _costFunctionPlotModel;
        private double _crossValidationError;
        private double _trainingError;
        private double _testError;
        private double _cost;

        public MainViewModel()
        {
            Alpha = 0.0001;
            Lambda = 0.0001;
            Iterations = 100;

            CostFunctionLineSeries = new LineSeries();
            CostFunctionPlotModel = new PlotModel
            {
                Axes =
                {
                    new LinearAxis {Position = AxisPosition.Left, Minimum = 0, Maximum = 30},
                    new LinearAxis {Position = AxisPosition.Bottom, Minimum = 0, Maximum = 120}
                }
            };
        }

        public double Alpha { get; set; }

        public double Cost
        {
            get { return _cost; }
            set
            {
                if (value == _cost) return;
                _cost = value;
                OnPropertyChanged(nameof(Cost));
            }
        }

        public LineSeries CostFunctionLineSeries { get; set; }

        public PlotModel CostFunctionPlotModel
        {
            get { return _costFunctionPlotModel; }
            set
            {
                if (value == _costFunctionPlotModel) return;
                _costFunctionPlotModel = value;
                OnPropertyChanged(nameof(CostFunctionPlotModel));
            }
        }

        public IList<IDataset> CrossValidationData { get; set; }

        public int CrossValidationDataToUse { get; set; }

        public double CrossValidationError
        {
            get { return _crossValidationError; }
            set
            {
                if (value == _crossValidationError) return;
                _crossValidationError = value;
                OnPropertyChanged(nameof(CrossValidationError));
            }
        }

        public int InputLayerSize { get; set; }

        public int IterationCount { get; set; }

        public int Iterations { get; set; }

        public double Lambda { get; set; }

        public IDictionary<string, Matrix<double>> Results { get; set; }

        public INeuronalNetwork Network { get; set; }

        public IOptimization Optimizer { get; set; }

        public int NumberOfHiddenLayers { get; set; }

        public int OutputLayerSize { get; set; }

        public IList<IDataset> TestData { get; set; }

        public int TestDataToUse { get; set; }

        public double TrainingError
        {
            get { return _trainingError; }
            set
            {
                if (value == _trainingError) return;
                _trainingError = value;
                OnPropertyChanged(nameof(TrainingError));
            }
        }

        public double TestError
        {
            get { return _testError; }
            set
            {
                if (value == _testError) return;
                _testError = value;
                OnPropertyChanged(nameof(TestError));
            }
        }

        public int TraingDataToUse { get; set; }

        public IList<IDataset> TrainingData { get; set; }

        public Task TrainingTask { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void CreateNewNetwork()
        {
            Network = new NeuronalNetwork(InputLayerSize, OutputLayerSize, NumberOfHiddenLayers, Lambda);
            IterationCount = 0;
        }

        public void TrainNetwork()
        {
            if(Network == null)
                throw new NullReferenceException("Network is null");

            if (TrainingTask == null || TrainingTask.IsCompleted)
            {
                TrainingTask = Task.Run(() =>
                {
                    Optimizer = new GradientDescentAlgorithm(Lambda, Alpha);
                    Optimizer.IterationFinished += UpdateCostFunctionPlot;
                    Optimizer.OptimizeNetwork(Network, TrainingData.ToList(), HelperFunctions.GetLabelMatrices(TrainingData), Iterations);
                });
            }
        }

        public void TestNetwork()
        {
            TrainingError = NetworkTester.TestNetwork(
                Network,
                TrainingData.Take(TraingDataToUse),
                Results);
        }

        public void TestNetworkWithCrossValidation()
        {
            CrossValidationError = NetworkTester.TestNetwork(Network,
                TrainingData.Skip(TraingDataToUse).Take(CrossValidationDataToUse),
                Results);
        }

        public void TestNetworkWithTestSet()
        {
            TestError = NetworkTester.TestNetwork(Network,
                TrainingData.Skip(TraingDataToUse).Skip(CrossValidationDataToUse).Take(TestDataToUse), Results);
        }

        public void UpdateCostFunctionPlot(object sender, EventArgs e)
        {
            var args = (IterationStartedEventArgs) e;
            var plotModel = new PlotModel();

            Cost = args.Cost;

            CostFunctionLineSeries.Points.Add(new DataPoint(IterationCount, args.Cost));

            plotModel.Axes.Add(new LinearAxis {Position = AxisPosition.Left, Minimum = 0, Maximum = args.Cost + 2});
            plotModel.Axes.Add(new LinearAxis {Position = AxisPosition.Bottom, Minimum = 0, Maximum = IterationCount + 5});

            CostFunctionPlotModel.Series.Clear();
            plotModel.Series.Add(CostFunctionLineSeries);
            CostFunctionPlotModel = plotModel;
            IterationCount++;
        }


        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}