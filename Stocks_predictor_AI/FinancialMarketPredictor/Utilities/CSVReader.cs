
// ciumac.sergiu@gmail.com

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace FinancialMarketPredictor.Utilities
{
    /// <summary>
    /// CSV Reader class
    /// </summary>
    public class CSVReader : IDisposable
    {
        /// <summary>
        /// The file to read.
        /// </summary>
        private readonly TextReader _reader;

        /// <summary>
        /// The names of all of the _columns, read from the first line of the file.
        /// </summary>
        private readonly IDictionary<String, int> _columns = new Dictionary<String, int>();

        /// <summary>
        /// The _data for the current line.
        /// </summary>
        private readonly String[] _data;


        /// <summary>
        /// Format a date/time object to the same format that we parse in.
        /// </summary>
        /// <param name="date">The date to format.</param>
        /// <returns>A formatted date and time.</returns>
        public static String DisplayDate(DateTime date)
        {
            return date.ToString();
        }

        /// <summary>
        /// Parse a date using the specified format.
        /// </summary>
        /// <param name="when">A string that contains a date in the specified format.</param>
        /// <returns>A DateTime that was parsed.</returns>
        public static DateTime ParseDate(String when)
        {
            try
            {
                return DateTime.ParseExact(when, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                return default(DateTime);
            }
        }

        /// <summary>
        /// Construct an object to read the specified CSV file.
        /// </summary>
        /// <param name="filename">The filename to read.</param>
        public CSVReader(String filename)
        {
            _reader = new StreamReader(filename);

            // read the column heads
            String line = _reader.ReadLine();
            string[] tok = line.Split(',');

            for (int index = 0; index < tok.Length; index++)
            {
                String header = tok[index];
                _columns.Add(header.ToLower(), index);
            }

            _data = new String[tok.Length];
        }

        /// <summary>
        /// Close the file.
        /// </summary>
        public void Close()
        {
            _reader.Close();
        }

        /// <summary>
        /// Get the specified column using an index.
        /// </summary>
        /// <param name="i">The zero based index of the column to read.</param>
        /// <returns>The specified column as a string.</returns>
        public String Get(int i)
        {
            return _data[i];
        }

        /// <summary>
        /// Get the specified column as a string.
        /// </summary>
        /// <param name="column">The specified column.</param>
        /// <returns>The specified column as a string.</returns>
        public String Get(String column)
        {
            if (!_columns.ContainsKey(column.ToLower()))
            {
                return null;
            }
            int i = _columns[column.ToLower()];

            return _data[i];
        }

        /// <summary>
        /// Read the specified column as a date.
        /// </summary>
        /// <param name="column">The specified column.</param>
        /// <returns>The specified column as a DateTime.</returns>
        public DateTime GetDate(String column)
        {
            String str = Get(column);
            return DateTime.Parse(str, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Get the specified column as a double.
        /// </summary>
        /// <param name="column">The column to read.</param>
        /// <returns>The specified column as a double.</returns>
        public double GetDouble(String column)
        {
            String str = Get(column);
            return double.Parse(str, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Get an integer that has the specified name.
        /// </summary>
        /// <param name="col">The column name to read.</param>
        /// <returns>The specified column as an int.</returns>
        public int GetInt(String col)
        {
            String str = Get(col);
            try
            {
                return int.Parse(str, CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                return 0;
            }
        }

        /// <summary>
        /// Read the next line.
        /// </summary>
        /// <returns>Return false if there are no more lines in the file.</returns>
        public bool Next()
        {
            String line = _reader.ReadLine();
            if (line == null)
            {
                return false;
            }

            string[] tok = line.Split(',');

            for (int i = 0; i < tok.Length; i++)
            {
                String str = tok[i];
                if (i < _data.Length)
                {
                    _data[i] = str;
                }
            }

            return true;
        }

        #region IDisposable Members

        /// <summary>
        /// Already disposed
        /// </summary>
        private bool _alreadydisposed = false;

        /// <summary>
        /// Disposes the resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            _alreadydisposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the resources
        /// </summary>
        /// <param name="isDisposing"></param>
        protected virtual void Dispose(bool isDisposing)
        {
            if(!_alreadydisposed)
            {
                //release managed resources
                if(isDisposing)
                {
                    _reader.Dispose();
                }
            }
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~CSVReader()
        {
            Dispose(false);
        }

        #endregion
    }
}
