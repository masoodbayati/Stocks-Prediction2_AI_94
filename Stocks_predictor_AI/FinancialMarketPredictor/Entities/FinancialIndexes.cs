// ciumac.sergiu@gmail.com
using System;

namespace FinancialMarketPredictor.Entities
{
    /// <summary>
    /// All four financial indexes are stitched in this class
    /// </summary>
    public class FinancialIndexes : IComparable<FinancialIndexes>
    {
        /// <summary>
        /// Public constructor
        /// </summary>
        /// <param name="dowIndex">Dow index</param>
        /// <param name="nasdaqIndex">Nasdaq index</param>
        /// <param name="spIndex">S&P index</param>
        /// <param name="pirIndex">Prime interest rate index</param>
        /// <param name="date">Date</param>
        public FinancialIndexes(double dowIndex, double nasdaqIndex, double spIndex, double pirIndex, DateTime date)
        {
            DowIndex = dowIndex;
            NasdaqIndex = nasdaqIndex;
            Sp = spIndex;
            PrimeInterestRate = pirIndex;
            Date = date;
        }

        /// <summary>
        /// Dow Jones index
        /// </summary>
        public double DowIndex { get; set; }

        /// <summary>
        /// NASDAQ index
        /// </summary>
        public double NasdaqIndex { get; set; }

        /// <summary>
        /// The value of Adj Close of S&P500 index
        /// </summary>
        public double Sp { get; set; }

        /// <summary>
        /// Prime interest rate
        /// </summary>
        public double PrimeInterestRate { get; set; }

        /// <summary>
        /// Date with corresponding S&P500 Adj Close and Prime Interest PrimeInterestRate
        /// </summary>
        public DateTime Date { get; set; }

        #region IComparable<FinancialIndexes> Members
        /// <summary>
        /// Compare by date
        /// </summary>
        /// <param name="other">Other financial pair</param>
        /// <returns></returns>
        public int CompareTo(FinancialIndexes other)
        {
            return Date.CompareTo(other.Date);
        }

        #endregion
    }
}
