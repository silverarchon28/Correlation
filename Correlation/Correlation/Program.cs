using System;
using Microsoft.Owin.Hosting;

namespace Correlation
{
    public class Program
    {
        public static string uri = "http://localhost:9000/";
        public static string API_URL = "http://localhost:9000/api/values";

        static void Main(string[] args)
        {
            // Start OWIN host
            using (WebApp.Start<Startup>(url: uri))
            {
                Console.WriteLine("Server is running");
                Console.WriteLine(string.Format("To begin please open browser to {0}", uri));
                Console.WriteLine(string.Format("Or send POST request with startdate and enddate to {0}", API_URL));
                Console.WriteLine(@"Ex: {""startdate"": ""2020-04-09"", ""enddate"": ""2020-05-12""} ");

                Console.ReadLine();
            }

        }
    }
}
