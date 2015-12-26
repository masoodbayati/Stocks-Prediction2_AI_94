// ciumac.sergiu@gmail.com
using System;

namespace FinancialMarketPredictor.Utilities
{
    /// <summary>
    /// Class for error calculation
    /// </summary>
    public class ErrorCalculation
    {
        /// <summary>
        /// The current error level.
        /// </summary>
        private double _globalError;

        /// <summary>
        /// The size of a training set.
        /// </summary>
        private int _setSize;

        /// <summary>
        /// Returns the root mean square error for a complete training set.
        /// </summary>
        /// <returns>The current error for the neural network.</returns>
        public double CalculateRMS()
        {
            return Math.Sqrt(_globalError/(_setSize));
        }

        /// <summary>
        /// Reset the error accumulation to zero.
        /// </summary>
        public void Reset()
        {
            _globalError = _setSize = 0;
        }

        /// <summary>
        /// Called to update for each number that should be checked.
        /// </summary>
        /// <param name="actual">The actual number.</param>
        /// <param name="ideal">The ideal number.</param>
        public void UpdateError(double[] actual, double[] ideal)
        {
            for (int i = 0; i < actual.Length; i++)
            {
                double delta = ideal[i] - actual[i];
                _globalError += delta * delta;
            }
            _setSize += ideal.Length;
        }


    }
}
