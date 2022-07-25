using Newtonsoft.Json;
using System;
using System.Web.Http;

namespace Correlation
{
    public class ValuesController : ApiController
    {
        // GET api/values 
        public void Get()
        {
        }

        // POST api/values 
        public string Post([FromBody] DateRange value)
        {
            string response = "";

            string startDate = value.startdate;
            string endDate = value.enddate;

            double USDCADAvg, USDCADHigh, USDCADLow;
            double CORRAAVg, CORRAHigh, CORRALow;
            double coefficient;

            try
            {
                // Calculate Avg, High, Low for USD/CAD and CORRA rates and Pearson Coefficient
                Correlation correlation = new Correlation(startDate, endDate);

                correlation.CalculateUSDCADRateInfo(out USDCADAvg, out USDCADHigh, out USDCADLow);
                correlation.CalculateCORRARateInfo(out CORRAAVg, out CORRAHigh, out CORRALow);
                coefficient = correlation.CalculateCoefficient();

                // Put data into object to serialize to JSON
                CorrelationInfo correlationInfo = new CorrelationInfo(USDCADAvg, USDCADHigh, USDCADLow, CORRAAVg, CORRAHigh, CORRALow, coefficient);
                response = JsonConvert.SerializeObject(correlationInfo);
            }
            catch (Exception e)
            {
                response = e.Message;
            }

            return response;
        }
    }

    /// <summary>
    /// Used to get POST data
    /// </summary>
    public class DateRange
    {
        public string startdate { get; set; }
        public string enddate { get; set; }
    }

    /// <summary>
    /// Used to store required display data and serialize to JSON response
    /// </summary>
    public class CorrelationInfo
    {
        public double USDCAD_Avg, USDCAD_High, USDCAD_Low;
        public double CORRA_Avg, CORRA_High, CORRA_Low;
        public double Coefficient;

        public CorrelationInfo(double USDCADAvg, double USDCADHigh, double USDCADLow, double CORRAAVg, double CORRAHigh, double CORRALow, double coefficient)
        {
            this.USDCAD_Avg = USDCADAvg;
            this.USDCAD_High = USDCADHigh;
            this.USDCAD_Low = USDCADLow;
            this.CORRA_Avg = CORRAAVg;
            this.CORRA_High = CORRAHigh;
            this.CORRA_Low = CORRALow;
            this.Coefficient = coefficient;
        }

    }
}
