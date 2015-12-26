
// ciumac.sergiu@gmail.com
using System;

namespace FinancialMarketPredictor.Entities
{
    /// <summary>
    /// Prediction results
    /// </summary>
    public class PredictionResults
    {
        #region Properties
        /// <summary>
        /// Date of the prediction
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Actual percentage move
        /// </summary>
        public double ActualSp {get; set; }
   
        /// <summary>
        /// Predicted percentage move
        /// </summary>
        public double PredictedSp {get; set; }

        /// <summary>
        /// Actual Dow index
        /// </summary>
        public double ActualDow { get; set; }

        /// <summary>
        /// Predicted Dow index
        /// </summary>
        public double PredictedDow { get; set; }

        /// <summary>
        /// Actual Nasdaq index
        /// </summary>
        public double ActualNasdaq { get; set; }

        /// <summary>
        /// Predicted Nasdaq index
        /// </summary>
        public double PredictedNasdaq { get; set; }

        /// <summary>
        /// Actual Prime Interest Rate
        /// </summary>
        public double ActualPir { get; set; }

        /// <summary>
        /// Predicted Prime Interest Rate
        /// </summary>
        public double PredictedPir { get; set; }

        /// <summary>
        /// Error between predicted and actual values
        /// </summary>
        public double Error { get; set; }

        #endregion
    }
}
