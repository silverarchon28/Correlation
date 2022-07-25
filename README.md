# Correlation

This application accepts a start and end date and will return the following data points:
1. High, low, mean average USD/CAD rate for the period.
2. High, low, mean average CORRA rate for the period.
3. Pearson coefficient of correlation between USD/CAD and CORRA.


Build Instructions

Download files from https://github.com/silverarchon28/Correlation

Open Correlation.sln

(If needed)
From the Tools menu, select NuGet Package Manager, then select Package Manager Console. In the Package Manager Console window, enter the following command:

Install-Package Microsoft.AspNet.WebApi.OwinSelfHost

Install-Package Microsoft.Owin.Cors

Run Correlation.sln

Open browser to http://localhost:9000/ and submit startdate and enddate

Or Send POST request with startdate and enddate to http://localhost:9000/api/values
Ex: {"startdate": "2020-04-09", "enddate": "2020-05-12"}
