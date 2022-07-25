using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;

namespace Correlation
{

    /// <summary>
    /// Class that calculates high, low, mean average of USDCAD and CORRA rates.
    /// Downloads USDCAD and CORRA rate from Bank of Canada valet API
    /// </summary>
    public class Correlation
    {
        HttpClient client;

        // Series id for CORRA rate used in Bank of Canada's valet API call
        private const string CORRA = "AVG.INTWO";

        // Series id for USDCAD rate used in Bank of Canada's valet API call
        private const string FXUSDCAD = "FXUSDCAD";

        // Rates are listed under OBSERVATIONS section of API response.
        private const string OBSERVATIONS = "OBSERVATIONS";

        // Key is date, value is CORRA rate
        Dictionary<string, double> corraRates;
        
        // Key is date, value is USDCAD rate
        Dictionary<string, double> usdcadRates;

        public Correlation(string startDate, string endDate)
        {
            DateTime startDateTime;
            DateTime endDateTime;

            if(!DateTime.TryParse(startDate, out startDateTime))
                throw new Exception("Error: Invalid Start Date");

            if (!DateTime.TryParse(endDate, out endDateTime))
                throw new Exception("Error: Invalid End Date");

            // Compare will return greater than 0 if startDate is later than endDate.
            if (DateTime.Compare(startDateTime, endDateTime) > 0)
                throw new Exception("Error: The End date must be greater than the Start date"); 

            // Create HttpCient and make a request to api/values
            client = new HttpClient();
            
            // Get CSV response of rates from Bank of Canada valet API
            string corraCSV = downloadRates(CORRA, startDate, endDate);
            string usdcadCSV = downloadRates(FXUSDCAD, startDate, endDate);

            // Store rates into dictionary with date as the key
            corraRates = parseRates(corraCSV);
            usdcadRates = parseRates(usdcadCSV);
        }

        public double CalculateCoefficient()
        {
            return calculatePearsonCoefficient(corraRates, usdcadRates);
        }

        public void CalculateUSDCADRateInfo(out double average, out double high, out double low)
        {
            calcRateData(usdcadRates, out average, out high, out low);
        }

        public void CalculateCORRARateInfo(out double average, out double high, out double low)
        {
            calcRateData(corraRates, out average, out high, out low);
        }

        /// <summary>
        /// Take CORRA and USDCAD raes to calculate Pearson Coefficient.
        /// The formula being used is:​ r= ( ∑(X− Xavg) * ∑(Y− Yavg) ) / ( (∑(X− Xavg)^2 * ∑(Y−Yavg)^2)^1/2 )
        /// where: r = correlation coefficient
        /// Xavg = average of observations of X
        /// Yavg = average of observations of Y
        /// https://www.investopedia.com/trading/using-currency-correlations-advantage/
        /// </summary>
        private double calculatePearsonCoefficient(Dictionary<string, double> corraRates, Dictionary<string, double> usdcadRates)
        {
            if (corraRates.Count != usdcadRates.Count)
            {
                Console.WriteLine("Warning: Number of rates are different");
            }

            double corraRatesAvg, usdcadRatesAvg, high, low;

            calcRateData(corraRates, out corraRatesAvg, out high, out low);
            calcRateData(usdcadRates, out usdcadRatesAvg, out high, out low);

            double numerator = 0;

            double sumSqauresCorra = 0;
            double sumSqauresUSDCAD = 0;

            foreach (KeyValuePair<string, double> kvp in corraRates)
            {
                string date = kvp.Key;
                double corraRate = kvp.Value;

                double usdcadRate = 0;

                if (!usdcadRates.TryGetValue(date, out usdcadRate))
                    Console.WriteLine("Error missing usdcadRate for " + date);

                numerator += (corraRate - corraRatesAvg) * (usdcadRate - usdcadRatesAvg);

                sumSqauresCorra += Math.Pow((corraRate - corraRatesAvg), 2);
                sumSqauresUSDCAD += Math.Pow((usdcadRate - usdcadRatesAvg), 2);
            }

            double stdDeviationCorra = Math.Sqrt(sumSqauresCorra);
            double stdDeviationUSDCAD = Math.Sqrt(sumSqauresUSDCAD);

            return numerator / (stdDeviationCorra * stdDeviationUSDCAD);
        }

        /// <summary>
        /// Returns high, low, mean, average of a series for the time period specified when Correlation was instantiated
        /// </summary>
        private void calcRateData(Dictionary<string, double> rates, out double average, out double high, out double low)
        {
            average = 0;
            high = 0;
            low = -1;

            if (rates.Count == 0)
            {
                Console.WriteLine("Error: No rates defined");
                return;
            }

            double rateSum = 0;
            foreach (double rate in rates.Values)
            {
                // Set the lowest rate for the period. If uninitialized as -1, set the first rate to be the lowest.
                if (low == -1 || rate < low)
                    low = rate;

                // Set the highest rate for the period
                if (rate > high)
                    high = rate;

                rateSum += rate;
            }

            average = rateSum / rates.Count;
        }

        /// <summary>
        /// Call Bank of Canada's valet API with SeriesName, StartDate and EndDate. Return the string response in CSV format.
        /// https://www.bankofcanada.ca/valet/docs#valet_api
        /// </summary>
        private string downloadRates(string seriesName, string startDate, string endDate)
        {
            string response = "";

            try
            {
                response = client.GetStringAsync(string.Format("https://www.bankofcanada.ca/valet/observations/{0}/csv?start_date={1}&end_date={2}", seriesName, startDate, endDate)).Result;
            }
            catch (Exception e)
            {
                Console.WriteLine(string.Format("Error: Exception caught downloading {0} rate - {1}", seriesName, e.Message));
            }

            return response;
        }

        /// <summary>
        /// Parse CSV response from Bank of Canada's API. 
        /// Return a dictionary of date to rates.
        /// </summary>
        private Dictionary<string, double> parseRates(string csvResponse)
        {
            // Key is date, value is rate
            Dictionary<string, double> rates = new Dictionary<string, double>();

            try
            {
                using (StringReader reader = new StringReader(csvResponse))
                {
                    // If reached end of file reader.Peek() returns -1.
                    // Skip until we reach the observations section
                    while (reader.Peek() != -1 && !reader.ReadLine().Contains(OBSERVATIONS))
                    {
                        continue;
                    }

                    // Skips the column header ex: "date, AVG.INTWO"
                    reader.ReadLine();

                    // Continue reading rates until the end of the file. 
                    while (reader.Peek() != -1)
                    {
                        string line = reader.ReadLine();

                        // Handle whitespace at end of file and if there's any in between rows
                        if (string.IsNullOrEmpty(line))
                            continue;

                        string[] values = line.Split(',');

                        // Need to strip quotes out
                        string date = values[0].Replace("\"", "");
                        double rate = Double.Parse(values[1].Replace("\"", ""));

                        if (!rates.ContainsKey(date))
                            rates.Add(date, rate);
                        else
                            Console.WriteLine(string.Format("Warning: Multiple rates on {0}", date));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(string.Format("Error: Exception caught parsing CSV - {0}", e.Message));
            }

            return rates;
        }
    }
}
