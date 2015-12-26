
// ciumac.sergiu@gmail.com
using System;

namespace FinancialMarketPredictor.Entities
{
    /// <summary>
    /// S&P 500 index
    /// </summary>
    public class Sp : IComparable<Sp>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="amount">SP index</param>
        /// <param name="date">Date of the index</param>
        public Sp(double amount, DateTime date)
        {
            SpIndex = amount;
            Date = date;
        }

        #region Properties
        /// <summary>
        /// S&P500 Index
        /// </summary>
        public double SpIndex { get; set; }

        /// <summary>
        /// Corresponding date
        /// </summary>
        public DateTime Date { get; set; }

        #endregion

        /// <summary>
        /// Compare the indexes by date
        /// </summary>
        /// <param name="other">Other S&P index</param>
        /// <returns>Comparison result</returns>
        public int CompareTo(Sp other)
        {
            return Date.CompareTo(other.Date);
        }
    }
}
