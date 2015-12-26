// ciumac.sergiu@gmail.com
using System;

namespace FinancialMarketPredictor.Entities
{
    /// <summary>
    /// Class which represents Dow index item
    /// </summary>
    public class DowIndex : IComparable<DowIndex>
    {
        /// <summary>
        /// Constructor for Dow index
        /// </summary>
        /// <param name="amount">Amount of the Dow index</param>
        /// <param name="date">Date of the index</param>
        public DowIndex(double amount, DateTime date)
        {
            Amount = amount;
            Date = date;
        }

        /// <summary>
        /// Date of the index
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Amount of the index
        /// </summary>
        public double Amount { get; set; }

        #region IComparable<DowIndex> Members
        /// <summary>
        /// Compare 2 Dow indexes by date
        /// </summary>
        /// <param name="other">Other Dow index</param>
        /// <returns>Date comparison result</returns>
        public int CompareTo(DowIndex other)
        {
            return Date.CompareTo(other.Date);
        }

        #endregion
    }
}
