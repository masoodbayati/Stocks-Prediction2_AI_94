// ciumac.sergiu@gmail.com

using System;
using System.Collections.Generic;
using System.IO;
using FinancialMarketPredictor.Utilities;

namespace FinancialMarketPredictor.Entities
{
    /// <summary>
    /// Financial predictor manager
    /// </summary>
    public sealed class FinancialPredictorManager
    {
        #region Private Members
        /// <summary>
        /// Prime interest rates
        /// </summary>
        private List<InterestRate> _rates = new List<InterestRate>();

        /// <summary>
        /// Dow indexes
        /// </summary>
        private List<DowIndex> _dowIndex = new List<DowIndex>();

        /// <summary>
        /// Nasdaq indexes
        /// </summary>
        private List<NasdaqIndex> _nasdaqIndex = new List<NasdaqIndex>();

        /// <summary>
        /// S&P500 indexes
        /// </summary>
        private List<Sp> _spIndexes = new List<Sp>();
        
        /// <summary>
        /// Samples
        /// </summary>
        private readonly List<FinancialIndexes> _samples = new List<FinancialIndexes>();

        /// <summary>
        /// Input size
        /// </summary>
        private readonly int _inputSize;

        /// <summary>
        /// Output size [% move]
        /// </summary>
        private readonly int _outputSize;

        #endregion

        #region Properties
        /// <summary>
        /// Maximum Dow index, as gathered from training set
        /// </summary>
        public double MaxDow { get; private set; }

        /// <summary>
        /// Minimum Dow index, as gathered from training set
        /// </summary>
        public double MinDow { get; private set; }

        /// <summary>
        /// Maximum Nasdaq index, as gathered from training set
        /// </summary>
        public double MaxNasdaq { get; private set; }

        /// <summary>
        /// Minimum Nasdaq index, as gathered from training set
        /// </summary>
        public double MinNasdaq { get; private set; }

        /// <summary>
        /// Maximum S&P500 index, as gathered from training set
        /// </summary>
        public double MaxSp500 { get; private set; }

        /// <summary>
        /// Minimum S&P index, as gathered from training set
        /// </summary>
        public double MinSp500 { get; private set; }

        /// <summary>
        /// Maximum Prime Interest Rate, as gathered from training set
        /// </summary>
        public double MaxPrimeRate { get; private set; }

        /// <summary>
        /// Minimum Prime Interest Rate, as gathered from training set
        /// </summary>
        public double MinPrimeRate { get; private set; }

        /// <summary>
        /// Max date for the training set
        /// </summary>
        public DateTime MaxDate { get; private set; }

        /// <summary>
        /// Min date for the training set
        /// </summary>
        public DateTime MinDate { get; private set; }
        #endregion

        private const string DATE_HEADER = "date";
        private const string ADJ_CLOSE_HEADER = "adj close";

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inputSize">Input size</param>
        /// <param name="outputSize">Output size</param>
        public FinancialPredictorManager(int inputSize, int outputSize)
        {
            if (inputSize <= 0)
                throw new ArgumentException("inputSize cannot be less than 0");
            if (outputSize <= 0)
                throw new ArgumentException("outputSize cannot be less than 0");
            _inputSize = inputSize;
            _outputSize = outputSize;
            MaxDow = MaxNasdaq = MaxPrimeRate = MaxSp500 = Double.MinValue;
            MinDow = MinNasdaq = MinPrimeRate = MinSp500 = Double.MaxValue;
            MaxDate = DateTime.MaxValue;
            MinDate = DateTime.MinValue;
        }
        #endregion

        /// <summary>
        /// Get input data - S&P 500 Index, Prime Interest Rate, Dow index, Nasdaq index
        /// </summary>
        /// <param name="offset">Start index of input data</param>
        /// <param name="input">Array to be populated</param>
        /// <remarks>
        /// According to the <c>offset</c> parameter, first <c>_inputSize</c> values are drawn from the dataset 
        /// </remarks>
        public void GetInputData(int offset, double[] input)
        {
            // get SP500, prime data, NASDAQ, Dow
            for (int i = 0; i < _inputSize; i++)
            {
                FinancialIndexes sample = _samples[offset + i];
                input[i*4]       = sample.Sp;
                input[i*4 + 1]   = sample.PrimeInterestRate;
                input[i*4 + 2]   = sample.DowIndex;
                input[i*4 + 3]   = sample.NasdaqIndex;
            }
        }

        /// <summary>
        /// Get output data - S&P 500 Index, Prime Interest Rate, Dow index, Nasdaq index
        /// </summary>
        /// <param name="offset">Start index of output data</param>
        /// <param name="output">Output array to be populated</param>
        /// <remarks>
        /// The value of <c>offset + _inputSize</c> indexes value are drawn from the samples data set.
        /// E.g. Consider the <c>offset</c> parameter equal to 12581. Input parameters to the network will be
        /// values from [12581..12590]. The actual values will be equal to the parameters stored in the <code>12581 + _inputSize</code>
        /// place => 12591 index.
        /// </remarks>
        public void GetOutputData(int offset, double[] output)
        {
            FinancialIndexes sample = _samples[offset + _inputSize];
            output[0]     = sample.Sp;
            output[1] = sample.PrimeInterestRate;
            output[2] = sample.DowIndex;
            output[3] = sample.NasdaqIndex;
            
        }

        #region Get indexes

        /// <summary>
        /// Get S&P500 index from a specific date
        /// </summary>
        /// <param name="date">Date at which to return the S&P index</param>
        /// <returns>S&P500 index</returns>
        /// <remarks>
        /// The method works fine only on sorted by date indexes.
        /// </remarks>
        public double GetSpIndex(DateTime date)
        {
            double currentsp = 0;

            foreach (Sp item in _spIndexes)
            {
                if (item.Date.CompareTo(date) >= 0)
                {
                    return currentsp;
                }
                currentsp = item.SpIndex;
            }
            return currentsp;
        }
        

        /// <summary>
        /// Get prime interest rate at a specific date
        /// </summary>
        /// <param name="date">Date at which to return the prime interest rate</param>
        /// <returns>Prime interest rate</returns>
        /// <remarks>
        /// The method works fine only on sorted by date indexes.
        /// </remarks>
        public double GetPrimeRate(DateTime date)
        {
            double currentRate = 0;

            foreach (InterestRate rate in _rates)
            {
                if (rate.Date.CompareTo(date) >= 0)
                {
                    return currentRate;
                }
                currentRate = rate.Rate;
            }
            return currentRate;
        }

        /// <summary>
        /// Get Dow index from a specific date
        /// </summary>
        /// <param name="date">Date searched</param>
        /// <returns>Dow index</returns>
        /// <remarks>
        /// The method works fine only on sorted by date indexes.
        /// </remarks>
        public double GetDowIndex(DateTime date)
        {
            double currentAmount = 0;

            foreach (DowIndex index in _dowIndex)
            {
                if (index.Date.CompareTo(date) >= 0)
                {
                    return currentAmount;
                }
                currentAmount = index.Amount;
            }
            return currentAmount;
        }

        /// <summary>
        /// Get Nasdaq index from a specific date 
        /// </summary>
        /// <param name="date">Date searched</param>
        /// <returns>Nasdaq index</returns>
        /// <remarks>
        /// The method works fine only on sorted by date indexes.
        /// </remarks>
        public double GetNasdaqIndex(DateTime date)
        {
            double currentAmount = 0;

            foreach (NasdaqIndex index in _nasdaqIndex)
            {
                if (index.Date.CompareTo(date) >= 0)
                {
                    return currentAmount;
                }
                currentAmount = index.Amount;
            }
            return currentAmount;
        }

        #endregion

        /// <summary>
        /// Get financial samples
        /// </summary>
        public IList<FinancialIndexes> Samples
        {
            get { return _samples; }
        }

        /// <summary>
        /// Load both S&P500 and Prime Interest PrimeInterestRate file
        /// </summary>
        /// <param name="sp500Filename">S&P500 filename</param>
        /// <param name="primeFilename">Prime interest rates filename</param>
        /// <param name="pathToDow">Path to DOW indexes</param>
        /// <param name="pathToNasdaq">Path to NASDAQ indexes</param>
        public void Load(String sp500Filename, String primeFilename, String pathToDow, String pathToNasdaq)
        {
            if (!File.Exists(sp500Filename))
                throw new ArgumentException("sp500Filename targets an invalid file");
            if (!File.Exists(primeFilename))
                throw new ArgumentException("primeFilename targets an invalid file");
            if (!File.Exists(pathToDow))
                throw new ArgumentException("pathToDow targets an invalid file");
            if (!File.Exists(pathToNasdaq))
                throw new ArgumentException("pathToNasdaq targets an invalid file");
            try
            {
                LoadSp500(sp500Filename);
            }
            catch
            {
                throw new NotSupportedException("Loading SP500 file failed. Not supported file format. Make sure \"date\" and \"adj close\" column headers are written in the file");
            }
            try
            {
                LoadPrimeInterestRates(primeFilename);
            }
            catch
            {
                throw new NotSupportedException("Loading Prime Interest Rate file failed. Not supported file format. Make sure \"date\" and \"prime\" column headers are written in the file");
            }
            try
            {
                LoadDowIndexes(pathToDow);
            }
            catch
            {
                throw new NotSupportedException("Loading Dow indexes file failed. Not supported file format. Make sure \"date\" and \"adj close\" column headers are written in the file");
            }
            try
            {
                LoadNasdaqIndexes(pathToNasdaq);
            }
            catch
            {
                throw new NotSupportedException("Loading Nasdaq indexes file failed. Not supported file format. Make sure \"date\" and \"adj close\" column headers are written in the file");
            }
            MaxDate = MaxDate.Subtract(new TimeSpan(_inputSize, 0, 0, 0)); /*Subtract 10 last days*/
            StitchFinancialIndexes();
            _samples.Sort();            /*Sort by date*/
            NormalizeData();
        }

        #region Load .csv files region
        /// <summary>
        /// Load Dow indexes
        /// </summary>
        /// <param name="pathToDow">Path to .csv with Dow indexes</param>
        public void LoadDowIndexes(String pathToDow)
        {
            if (_dowIndex == null) _dowIndex = new List<DowIndex>();
            else if (_dowIndex.Count > 0) _dowIndex.Clear();
            using (CSVReader csv = new CSVReader(pathToDow))
            {
                while (csv.Next())
                {
                    DateTime date = csv.GetDate(DATE_HEADER);
                    double amount = csv.GetDouble(ADJ_CLOSE_HEADER);
                    DowIndex sample = new DowIndex(amount, date);
                    _dowIndex.Add(sample);
                    if (amount > MaxDow) MaxDow = amount;
                    if (amount < MinDow) MinDow = amount;
                }
                csv.Close();
                _dowIndex.Sort();
            }
            if(_dowIndex.Count > 0)
            {
                if (MinDate < _dowIndex[0].Date)                   //after sorting the indexes at 0 position is the lowest date in the range
                    MinDate = _dowIndex[0].Date;
                if (MaxDate > _dowIndex[_dowIndex.Count - 1].Date) //Maximal date
                    MaxDate = _dowIndex[_dowIndex.Count - 1].Date; 
            }
        }

        /// <summary>
        /// Load Nasdaq indexes
        /// </summary>
        /// <param name="pathToNasdaq">Path to .csv with Nasdaq indexes</param>
        public void LoadNasdaqIndexes(String pathToNasdaq)
        {
            if (_nasdaqIndex == null) _nasdaqIndex = new List<NasdaqIndex>();
            else if (_nasdaqIndex.Count > 0) _nasdaqIndex.Clear();
            using (CSVReader csv = new CSVReader(pathToNasdaq))
            {
                while (csv.Next())
                {
                    DateTime date = csv.GetDate(DATE_HEADER);
                    double amount = csv.GetDouble(ADJ_CLOSE_HEADER);
                    NasdaqIndex sample = new NasdaqIndex(amount, date);
                    _nasdaqIndex.Add(sample);
                    if (amount > MaxNasdaq) MaxNasdaq = amount;
                    if (amount < MinNasdaq) MinNasdaq = amount;
                }
                csv.Close();
                _nasdaqIndex.Sort();
            }
            if (_nasdaqIndex.Count > 0)
            {
                if (MinDate < _nasdaqIndex[0].Date)                   //after sorting the indexes at 0 position is the lowest date in the range
                    MinDate = _nasdaqIndex[0].Date;
                if (MaxDate > _nasdaqIndex[_nasdaqIndex.Count - 1].Date) //Maximal date
                    MaxDate = _nasdaqIndex[_nasdaqIndex.Count - 1].Date;
            }
        }

        /// <summary>
        /// Load prime interest rates
        /// </summary>
        /// <param name="primeFilename">Prime interest rates file</param>
        public void LoadPrimeInterestRates(String primeFilename)
        {
            if (_rates == null) _rates = new List<InterestRate>();
            else if (_rates.Count > 0) _rates.Clear();
            using (CSVReader csv = new CSVReader(primeFilename))
            {
                while (csv.Next())
                {
                    DateTime date = csv.GetDate(DATE_HEADER);
                    double rate = csv.GetDouble("prime");
                    InterestRate ir = new InterestRate(date, rate);
                    _rates.Add(ir);
                    if (rate > MaxPrimeRate) MaxPrimeRate = rate;
                    if (rate < MinPrimeRate) MinPrimeRate = rate;
                }

                csv.Close();
                _rates.Sort();
            }
            if (_rates.Count > 0)
            {
                if (MinDate < _rates[0].Date)                   //after sorting the indexes at 0 position is the lowest date in the range
                    MinDate = _rates[0].Date;
                /*No need to infer the maximum date as the last PIR value will be used as a reference to current date*/
            }

        }

        /// <summary>
        /// Load S&P500 indexes file
        /// </summary>
        /// <param name="sp500Filename">File to load</param>
        public void LoadSp500(String sp500Filename)
        {
            if (_spIndexes == null) _spIndexes = new List<Sp>();
            else if (_spIndexes.Count > 0) _spIndexes.Clear();
            using (CSVReader csv = new CSVReader(sp500Filename))
            {
                while (csv.Next())
                {
                    DateTime date = csv.GetDate(DATE_HEADER);
                    double amount = csv.GetDouble(ADJ_CLOSE_HEADER);
                    Sp sample = new Sp(amount, date);
                    _spIndexes.Add(sample);
                    if (amount > MaxSp500) MaxSp500 = amount;
                    if (amount < MinSp500) MinSp500 = amount;
                }
                csv.Close();
                _spIndexes.Sort();
            }
            if (_spIndexes.Count > 0)
            {
                if (MinDate < _spIndexes[0].Date)                //after sorting the indexes at 0 position is the lowest date in the range
                    MinDate = _spIndexes[0].Date;
                if (MaxDate > _spIndexes[_spIndexes.Count - 1].Date) //Maximal date
                    MaxDate = _spIndexes[_spIndexes.Count - 1].Date;
            }
        }
        #endregion

        /// <summary>
        /// Get financial samples size
        /// </summary>
        public int Size
        {
            get { return _samples.Count; }
        }

        /// <summary>
        /// Stitch S&P 500 indexes to Prime Interest Rates according to the date parameter
        /// </summary>
        public void StitchFinancialIndexes()
        {
            foreach (Sp item in _spIndexes)
            {
                double rate = GetPrimeRate(item.Date);
                double dowIndex = GetDowIndex(item.Date);
                double nasdaqIndex = GetNasdaqIndex(item.Date);
                double spIndex = GetSpIndex(item.Date);
                _samples.Add(new FinancialIndexes(dowIndex, nasdaqIndex, spIndex, rate, item.Date));
            }          
        }

        /// <summary>
        /// Normalize input data
        /// </summary>
        public void NormalizeData()
        {
            foreach (FinancialIndexes t in _samples)
            {
                t.DowIndex = (t.DowIndex - MinDow) / (MaxDow - MinDow);
                t.NasdaqIndex = (t.NasdaqIndex - MinNasdaq) / (MaxNasdaq - MinNasdaq);
                t.PrimeInterestRate = (t.PrimeInterestRate - MinPrimeRate) / (MaxPrimeRate - MinPrimeRate);
                t.Sp = (t.Sp - MinSp500) / (MaxSp500 - MinSp500);
            }
        }
    }
}
