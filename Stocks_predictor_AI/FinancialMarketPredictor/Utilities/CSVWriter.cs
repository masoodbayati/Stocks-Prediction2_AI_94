// ciumac.sergiu@gmail.com

using System.IO;
using System.Text;

namespace FinancialMarketPredictor.Utilities
{
    /// <summary>
    /// Class for writing any object values in comma separated file
    /// </summary>
    public class CSVWriter
    {
        /// <summary>
        /// Char separator
        /// </summary>
        private const char SEPARATOR = ',';

        /// <summary>
        /// Path to file to be written
        /// </summary>
        private readonly string _pathToFile;

        /// <summary>
        /// Stream writer
        /// </summary>
        private StreamWriter _writer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pathToFile">Path to filename</param>
        public CSVWriter(string pathToFile)
        {
            _pathToFile = pathToFile;
        }

        /// <summary>
        /// Write the data into csv
        /// </summary>
        /// <param name="data">Data to be written</param>
        [System.Security.Permissions.FileIOPermission(System.Security.Permissions.SecurityAction.Demand)]
        public void Write(object[,] data)
        {
            using (_writer = new StreamWriter(_pathToFile))
            {
                int cols = data.GetLength(1);
                for (int i = 0, n = data.GetLength(0); i < n; i++)
                {
                    StringBuilder builder = new StringBuilder();
                    for (int j = 0; j < cols; j++)
                    {
                        builder.Append(data[i, j]);
                        if (j != cols - 1)
                            builder.Append(SEPARATOR);
                    }
                    _writer.WriteLine(builder.ToString());
                }
                _writer.Close();
            }
        }
    }

}