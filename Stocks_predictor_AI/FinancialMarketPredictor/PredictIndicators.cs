// ciumac.sergiu@gmail.com
// #define LOG_DATASET
using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using FinancialMarketPredictor.Entities;
using FinancialMarketPredictor.Utilities;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Layers;
using Encog.Neural.Networks.Training;
using Encog.Neural.Data.Basic;
using Encog.Neural.Networks.Training.LMA;
using Encog.Neural.Networks.Training.Propagation.Resilient;
using Encog.Neural.Networks.Training.Anneal;
using Encog.Neural.Networks.Training.Strategy;
using Encog.Neural.Activation;
using Encog.Persist.Persistors;

namespace FinancialMarketPredictor
{
    /// <summary>
    /// Training algorithm
    /// </summary>
    public enum TrainingAlgorithm
    {
        /// <summary>
        /// Backpropagation learning
        /// </summary>
        Resilient,

        /// <summary>
        /// Simulated annealing
        /// </summary>
        Annealing,

        /// <summary>
        /// Evolutionary learning
        /// </summary>
        Evolutionary
    }
    /// <summary>
    /// Training status delegate
    /// </summary>
    /// <param name="iteration">Epoch number</param>
    /// <param name="error">Error</param>
    /// <param name="algorithm">Training algorithm</param>
    public delegate void TrainingStatus(int iteration, double error, TrainingAlgorithm algorithm);

    /// <summary>
    /// Class for prediction
    /// </summary>
    public sealed class PredictIndicators
    {
        #region Constants
        /// <summary>
        /// Indexes to consider
        /// </summary>
        /// <remarks>
        /// Dow index, Prime interest rate, Nasdaq index, S&P500 index
        /// </remarks>
        private const int INDEXES_TO_CONSIDER = 4;

        /// <summary>
        /// Input Tuples. Each tuple consist of a pair: <c>S&P500</c> index and prime interest rate PIR
        /// </summary>
        /// <remarks>
        /// The total amount of input synapses equals <c>InputTupples * IndexesToConsider</c>
        /// </remarks>
        private const int INPUT_TUPLES = 10;

        /// <summary>
        /// The size of network's output
        /// </summary>
        private const int OUTPUT_SIZE = 4;

        /// <summary>
        /// Maximal error
        /// </summary>
        private const double MAX_ERROR = 0.00005;


        #endregion

        #region Private Members

        /// <summary>
        /// Network to be trained
        /// </summary>
        private BasicNetwork _network;

        /// <summary>
        /// Input data S&P, Prime Interest Rate, Nasdaq, Dow indexes
        /// </summary>
        private double[][] _input;

        /// <summary>
        /// Desired output
        /// </summary>
        private double[][] _ideal;

        /// <summary>
        /// Financial market predictor
        /// </summary>
        private FinancialPredictorManager _manager;

        /// <summary>
        /// Training tread
        /// </summary>
        private Thread _trainThread;

        /// <summary>
        /// Path to S&P index CSV
        /// </summary>
        private string _pathtosp;

        /// <summary>
        /// Path to Prime Interest Rates CSV
        /// </summary>
        private string _pathtorates;

        /// <summary>
        /// Path to Dow indexes
        /// </summary>
        private string _pathToDow;

        /// <summary>
        /// Path to Nasdaq indexes
        /// </summary>
        private string _pathToNasdaq;

        /// <summary>
        /// Size of the training data
        /// </summary>
        private int _trainingSize = 1000;

        #endregion

        /// <summary>
        /// Gets the information about the predictor
        /// </summary>
        public bool Loaded { get; private set; }

        /// <summary>
        /// Hidden layers
        /// </summary>
        public int HiddenLayers { get; private set; }

        /// <summary>
        /// Hidden units
        /// </summary>
        public int HiddenUnits { get; private set; }

        /// <summary>
        /// Maximum date for training and prediction
        /// </summary>
        public DateTime MaxIndexDate
        {
            get
            {
                return _manager == null ? DateTime.MinValue : _manager.MaxDate;
            }
        }

        /// <summary>
        /// Minimum date for training and prediction
        /// </summary>
        public DateTime MinIndexDate
        {
            get
            {
                return _manager == null ? DateTime.MaxValue : _manager.MinDate;
            }
        }
        #region Constructors

        /// <summary>
        /// Constructor for Prediction class
        /// </summary>
        /// <param name="pathToSP500">Path to CSV with S&P500 index rates</param>
        /// <param name="pathToPrimeRates">Path to CSV with prime interest rates</param>
        /// <param name="pathToDow">Path to DOW index file</param>
        /// <param name="pathToNasdaq">Path to Nasdaq</param>
        /// <param name="hiddenUnits">Number of hidden units per hidden layer</param>
        /// <param name="hiddenLayers">Number of hidden layers</param>
        public PredictIndicators(string pathToSP500, string pathToPrimeRates, string pathToDow, string pathToNasdaq, int hiddenUnits, int hiddenLayers)
        {
            if (!File.Exists(pathToSP500))
                throw new ArgumentException("pathToSP500 targets an invalid file");
            if (!File.Exists(pathToPrimeRates))
                throw new ArgumentException("pathToPrimeRates targets an invalid file");
            if (!File.Exists(pathToDow))
                throw new ArgumentException("pathToDow targets an invalid file");
            if (!File.Exists(pathToNasdaq))
                throw new ArgumentException("pathToNasdaq targets an invalid file");

            _pathtosp = pathToSP500;
            _pathtorates = pathToPrimeRates;
            _pathToDow = pathToDow;
            _pathToNasdaq = pathToNasdaq;

            CreateNetwork(hiddenUnits, hiddenLayers);                                                       /*Create new network*/
            _manager = new FinancialPredictorManager(INPUT_TUPLES, OUTPUT_SIZE);     /*Create new financial predictor manager*/
            _manager.Load(_pathtosp, _pathtorates, _pathToDow, _pathToNasdaq);     /*Load S&P 500 and prime interest rates*/
            Loaded = true;
            HiddenLayers = hiddenLayers;
            HiddenUnits = hiddenUnits;
        }

        #endregion

        /// <summary>
        /// Reload indexes
        /// </summary>
        /// <param name="pathToSp500">S&P500 date</param>
        /// <param name="pathToPrimeRates">Prime interest rate</param>
        /// <param name="pathToDow">DOW index indicator</param>
        /// <param name="pathToNasdaq">NASDAQ index</param>
        public void ReloadFiles(string pathToSp500, string pathToPrimeRates, string pathToDow, string pathToNasdaq)
        {
            if (!File.Exists(pathToSp500))
                throw new ArgumentException("pathToSP500 targets an invalid file");
            if (!File.Exists(pathToPrimeRates))
                throw new ArgumentException("pathToPrimeRates targets an invalid file");
            if (!File.Exists(pathToDow))
                throw new ArgumentException("pathToDow targets an invalid file");
            if (!File.Exists(pathToNasdaq))
                throw new ArgumentException("pathToNasdaq targets an invalid file");
            Loaded = false;
            _pathtosp = pathToSp500;
            _pathtorates = pathToPrimeRates;
            _pathToDow = pathToDow;
            _pathToNasdaq = pathToNasdaq;
            _manager = new FinancialPredictorManager(INPUT_TUPLES, OUTPUT_SIZE);     /*Create new financial predictor manager*/
            _manager.Load(_pathtosp, _pathtorates, _pathToDow, _pathToNasdaq);     /*Load S&P 500 and prime interest rates*/
            _ideal = _input = null;
            Loaded = true;
        }

        /// <summary>
        /// Create a new network
        /// </summary>
        private void CreateNetwork(int hiddenUnits, int hiddenLayers)
        {
            _network = new BasicNetwork {Name = "Financial Predictor", Description = "Network for prediction analysis"};
            _network.AddLayer(new BasicLayer(INPUT_TUPLES * INDEXES_TO_CONSIDER));                             /*Input*/
            for (int i = 0; i < hiddenLayers; i++)
                _network.AddLayer(new BasicLayer(new ActivationTANH(), true, hiddenUnits));                 /*Hidden layer*/
            _network.AddLayer(new BasicLayer(new ActivationTANH(), true, OUTPUT_SIZE));                      /*Output of the network*/
            _network.Structure.FinalizeStructure();                                                         /*Finalize network structure*/
            _network.Reset();                                                                               /*Randomize*/
        }

        /// <summary>
        /// Create Training sets for the neural network to be trained
        /// </summary>
        /// <param name="trainFrom">Initial date, from which to gather indexes</param>
        /// <param name="trainTo">Final date, to which to gather indexes</param>
        public void CreateTrainingSets(DateTime trainFrom, DateTime trainTo)
        {
            // find where we are starting from
            int startIndex = -1;
            int endIndex = -1;
            foreach (FinancialIndexes sample in _manager.Samples)
            {
                if (sample.Date.CompareTo(trainFrom) < 0)
                    startIndex++;
                if (sample.Date.CompareTo(trainTo) < 0)
                    endIndex++;
            }
            // create a sample factor across the training area
            _trainingSize = endIndex - startIndex;
            _input = new double[_trainingSize][];
            _ideal = new double[_trainingSize][];

            // grab the actual training data from that point
            for (int i = startIndex; i < endIndex; i++)
            {
                _input[i - startIndex] = new double[INPUT_TUPLES * INDEXES_TO_CONSIDER];
                _ideal[i - startIndex] = new double[OUTPUT_SIZE];
                _manager.GetInputData(i, _input[i - startIndex]);
                _manager.GetOutputData(i, _ideal[i - startIndex]);
            }
#if LOG_DATASET
            using (StreamWriter writer = new StreamWriter("dataset.csv"), ideal = new StreamWriter("ideal.csv"))
            {
                for (int i = 0; i < _input.Length; i++)
                {
                    StringBuilder builder = new StringBuilder();
                    for (int j = 0; j < _input[0].Length; j++)
                    {
                        builder.Append(_input[i][j]);
                        if (j != _input[0].Length - 1)
                            builder.Append(",");
                    }
                    writer.WriteLine(builder.ToString());

                    StringBuilder idealData = new StringBuilder();
                    for (int j = 0; j < _ideal[0].Length; j++)
                    {
                        idealData.Append(_ideal[i][j]);
                        if (j != _ideal[0].Length - 1)
                            idealData.Append(",");
                    }
                    ideal.WriteLine(idealData.ToString());
                }
            }
#endif

        }

        /// <summary>
        /// Train the network using Backpropagation and SimulatedAnnealing methods
        /// </summary>
        /// <param name="trainTo">Train until a specific date</param>
        /// <param name="status">Callback function invoked on each _epoch</param>
        /// <param name="trainFrom">Initial date, from which to gather training data</param>
        public void TrainNetworkAsync(DateTime trainFrom, DateTime trainTo, TrainingStatus status)
        {
            Action<DateTime, DateTime, TrainingStatus> action = TrainNetwork;
            action.BeginInvoke(trainFrom, trainTo, status, action.EndInvoke, action);
        }

        /// <summary>
        // Train network
        /// </summary>
        /// <param name="status">Delegate to be invoked</param>
        /// <param name="trainFrom">Train from</param>
        /// <param name="trainTo">Train to</param>
        private void TrainNetwork(DateTime trainFrom, DateTime trainTo, TrainingStatus status)
        {
            if(_input == null || _ideal == null)
                CreateTrainingSets(trainFrom, trainTo);         /*Create training sets, according to input parameters*/
            _trainThread = Thread.CurrentThread;
            int epoch = 1;
            ITrain train = null;
            try
            {
               
                var trainSet = new BasicNeuralDataSet(_input, _ideal);
                train = new ResilientPropagation(_network, trainSet);
                double error;
                do
                {
                    train.Iteration();
                    error = train.Error;
                    if (status != null)
                        status.Invoke(epoch, error, TrainingAlgorithm.Resilient);
                    epoch++;
                } while (error > MAX_ERROR);
            }
            catch (ThreadAbortException) {/*Training aborted*/ _trainThread = null; }
            finally
            {
                train.FinishTraining();
            }
            _trainThread = null;
        }

        /// <summary>
        /// Abort training
        /// </summary>
        public void AbortTraining()
        {
            if (_trainThread != null) _trainThread.Abort();
        }



        /// <summary>
        /// Export neural network
        /// </summary>
        /// <param name="path"></param>
        [System.Security.Permissions.FileIOPermission(System.Security.Permissions.SecurityAction.Demand)]
        public void ExportNeuralNetwork(string path)
        {
            if (_network == null)
                throw new NullReferenceException("Network reference is set to null. Nothing to export.");
            Encog.Util.SerializeObject.Save(path, _network);
        }

        /// <summary>
        /// Load neural network
        /// </summary>
        /// <param name="path">Path to previously serialized object</param>
        public void LoadNeuralNetwork(string path)
        {
            _network = (BasicNetwork)Encog.Util.SerializeObject.Load(path);
            HiddenLayers = _network.Structure.Layers.Count - 2 /*1 input, 1 output*/;
            HiddenUnits = _network.Structure.Layers[1].NeuronCount;
        }

        /// <summary>
        /// Predict the results
        /// </summary>
        /// <returns>List with the prediction results</returns>
        public List<PredictionResults> Predict(DateTime predictFrom, DateTime predictTo)
        {
            List<PredictionResults> results = new List<PredictionResults>();
            double[] present = new double[INPUT_TUPLES * INDEXES_TO_CONSIDER];
            double[] actualOutput = new double[OUTPUT_SIZE];
            int index = 0;
            foreach (var sample in _manager.Samples)
            {
                if (sample.Date.CompareTo(predictFrom) > 0 && sample.Date.CompareTo(predictTo) < 0)
                {
                    PredictionResults result = new PredictionResults();
                    _manager.GetInputData(index - INPUT_TUPLES, present);
                    _manager.GetOutputData(index - INPUT_TUPLES, actualOutput);
                    var data = new BasicNeuralData(present);
                    var predict = _network.Compute(data);
                    result.ActualSp = actualOutput[0] * (_manager.MaxSp500 - _manager.MinSp500) + _manager.MinSp500;
                    result.PredictedSp = predict[0] * (_manager.MaxSp500 - _manager.MinSp500) + _manager.MinSp500;
                    result.ActualPir = actualOutput[1] * (_manager.MaxPrimeRate - _manager.MinPrimeRate) + _manager.MinPrimeRate;
                    result.PredictedPir = predict[1] * (_manager.MaxPrimeRate - _manager.MinPrimeRate) + _manager.MinPrimeRate;
                    result.ActualDow = actualOutput[2] * (_manager.MaxDow - _manager.MinDow) + _manager.MinDow;
                    result.PredictedDow = predict[2] * (_manager.MaxDow - _manager.MinDow) + _manager.MinDow;
                    result.ActualNasdaq = actualOutput[3] * (_manager.MaxNasdaq - _manager.MinNasdaq) + _manager.MinNasdaq;
                    result.PredictedNasdaq = predict[3] * (_manager.MaxNasdaq - _manager.MinNasdaq) + _manager.MinNasdaq;
                    result.Date = sample.Date;
                    ErrorCalculation error = new ErrorCalculation();
                    error.UpdateError(actualOutput, predict.Data);
                    result.Error = error.CalculateRMS();
                    results.Add(result);
                }
                index++;
            }
            return results;
        }
    }
}
