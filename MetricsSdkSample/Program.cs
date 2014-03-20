using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Common;
using Microsoft.WindowsAzure.Management.Monitoring.Metrics;
using Microsoft.WindowsAzure.Management.Monitoring.Metrics.Models;
using Microsoft.WindowsAzure.Management.Monitoring.Utilities;
using Microsoft.WindowsAzure.Management.WebSites;
using Microsoft.WindowsAzure.Management.WebSites.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetricsSdkSample
{
    static class Program
    {
        static string _subId = "YOUR SUBSCRIPTION ID HERE";
        
        static void Main(string[] args)
        {
            //Switch between Certificate or Token based authentication
            //var cred = CertificateCredentialHelper.CreateCredential(_subId, "YOUR PFX FILENAME HERE", "YOUR PFX PASSWORD HERE");
            var cred = TokenCredentialHelper.SignIn(_subId, "YOUR TENANT HERE", "YOUR CLIENT ID HERE", "YOUR REDIRECTURI HERE");
            ShowWebSiteMetrics(cred);

            Console.WriteLine("Finished");
            Console.ReadLine();
        }

        private static void ShowWebSiteMetrics(SubscriptionCloudCredentials cred)
        {
            using (var webSitesMgmtClient = new WebSiteManagementClient(cred))
            {
                using (var metricsClient = new MetricsClient(cred))
                {
                    var webSiteResourceIds = GetWebSiteResourceIds(cred, webSitesMgmtClient);

                    webSiteResourceIds.ForEach(webSiteResourceId =>
                    {
                        Console.WriteLine(webSiteResourceId);

                        var metricDefinitions = ListResourceMetrics(cred, metricsClient, webSiteResourceId);

                        metricDefinitions.OrderBy(x => x.DisplayName).ToList().ForEach(metric =>
                        {
                            var metricValue = GetMetricValue(
                                cred,
                                metricsClient,
                                webSiteResourceId,
                                metric.Name);

                            Console.WriteLine("{0} ({1}): {2} ({3})",
                                metric.DisplayName,
                                metric.Name,
                                ((metric.Unit == "Bytes")
                                    ? metricValue.BytesToString()
                                    : (metric.Unit == "Milliseconds")
                                        ? metricValue.MillisecondsToHours()
                                        : metricValue.ToString()),
                                metric.Unit);
                        });
                    });
                }
            }
        }

        static List<string> GetWebSiteResourceIds(SubscriptionCloudCredentials cred,
            WebSiteManagementClient webSitesMgmtClient)
        {
            var ret = new List<string>();

            var webSpaceListResult = webSitesMgmtClient.WebSpaces.List();

            foreach (var webSpace in webSpaceListResult.WebSpaces)
            {
                var webSiteListResult = webSitesMgmtClient.WebSpaces.ListWebSites(webSpace.Name, new WebSiteListParameters { });

                foreach (var webSite in webSiteListResult.WebSites)
                {
                    var webSiteResourceId =
                        ResourceIdBuilder.BuildWebSiteResourceId(webSpace.Name, webSite.Name);

                    ret.Add(webSiteResourceId);
                }
            }

            return ret;
        }

        static double GetMetricValue(SubscriptionCloudCredentials cred,
            MetricsClient metricsClient,
            string webSiteResourceId,
            string metricName)
        {
            double requestCount = 0;

            var metricValueResult = metricsClient.MetricValues.List(
                                    webSiteResourceId,
                                    new List<string> { metricName },
                                    "",
                                    TimeSpan.FromHours(1),
                                    DateTime.UtcNow - TimeSpan.FromDays(1),
                                    DateTime.UtcNow
                                    );

            var values = metricValueResult.MetricValueSetCollection;

            foreach (var value in values.Value)
            {
                foreach (var total in value.MetricValues)
                {
                    if (total.Total.HasValue)
                        requestCount += total.Total.Value;
                }
            }

            return requestCount;
        }

        static List<MetricDefinition> ListResourceMetrics(SubscriptionCloudCredentials cred,
            MetricsClient metricsClient,
            string webSiteResourceId)
        {
            var metricListResponse = metricsClient.MetricDefinitions.List(webSiteResourceId,
                null, // don't filter the list of metric definitions available
                null  // no need to pass anything here
                );

            var metricCollection = metricListResponse.MetricDefinitionCollection.Value;
            return metricCollection.ToList();
        }

        #region Formatting Helper Methods

        public static string BytesToString(this double bytes, string format = "#,##0.00")
        {
            var unitstr = new string[] { "B", "KB", "MB", "GB", "TB" };
            var bytesd = Convert.ToDouble(bytes);
            var unit = 0;

            while (bytesd / 1024D > 1 && unit < unitstr.Length)
            {
                unit++; bytesd /= 1024D;
            }

            return string.Format("{0:" + format + "}{1}", bytesd, unitstr[unit]);
        }

        public static string MillisecondsToHours(this double ms)
        {
            int SECOND = 1000;
            int MINUTE = 60 * SECOND;
            int HOUR = 60 * MINUTE;
            int DAY = 24 * HOUR;

            StringBuilder text = new StringBuilder();
            if (ms > DAY)
            {
                text.Append(ms / DAY);
                text.Append(" Days");
                ms %= DAY;
            }
            else if (ms > HOUR)
            {
                text.Append(ms / HOUR);
                text.Append(" Hours");
                ms %= HOUR;
            }
            else if (ms > MINUTE)
            {
                text.Append(ms / MINUTE);
                text.Append(" Minutes");
                ms %= MINUTE;
            }
            else if (ms > SECOND)
            {
                text.Append(ms / SECOND);
                text.Append(" Seconds");
                ms %= SECOND;
            }
            return text.ToString();
        }

        #endregion

    }
}
