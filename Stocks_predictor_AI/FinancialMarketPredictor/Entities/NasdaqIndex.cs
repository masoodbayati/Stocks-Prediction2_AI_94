
// ciumac.sergiu@gmail.com
using System;

namespace FinancialMarketPredictor.Entities
{
    /// <summary>
    /// Class which encapsulates an Nasdaq index item
    /// </summary>
    public class NasdaqIndex : IComparable<NasdaqIndex>
    {
        /// <summary>
        /// Nasdaq index constructor
        /// </summary>
        /// <param name="amount">Index amount</param>
        /// <param name="date">Index date</param>
        public NasdaqIndex(double amount, DateTime date)
        {
            Amount = amount;
            Date = date;
        }

        #region Properties

        /// <summary>
        /// Nasdaq index amount
        /// </summary>
        public double Amount {get; set;}

        /// <summary>
        /// Date of the index
        /// </summary>
        public DateTime Date {get; set;}

        #endregion

        #region IComparable<NasdaqIndex> Members

        /// <summary>
        /// Compare 2 indexes by date
        /// </summary>
        /// <param name="other">Other NASDAQ index</param>
        /// <returns>Date comparison result</returns>
        public int CompareTo(NasdaqIndex other)
        {
            return Date.CompareTo(other.Date);
        }

        #endregion
    }
}
