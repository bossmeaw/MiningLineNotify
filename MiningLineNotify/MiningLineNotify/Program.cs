using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Management;
using WmiLight;

namespace MiningLineNotify
{
    class Program
    {
        static private HttpClient _httpClient;
        static private string _appType = "application/x-www-form-urlencoded";
        static private string _token = "XNpn3kgFnbGkTmNSa0scv3VKdamzyVI30qoXzKOrpeo";
        static private string _authScheme = "Bearer";
        static private string _postUri = "/api/notify";
        static private string _uri = "https://notify-api.line.me/api/notify";
        static private string _message;

        static void Main(string[] args)
        {
            Setup();

            RunAsync().Wait();
        }

        static async Task RunAsync()
        {
            try
            {
                while (true)
                {
                    _message = string.Format(_message, GetCpuUsage(), GetCpuTemputure(), GetRamUsage(), GetRamTemputure());

                    StringContent content = new StringContent(string.Format("message={0}", _message), Encoding.UTF8, _appType);
                    HttpResponseMessage result = await _httpClient.PostAsync(_postUri, content);

                    Thread.Sleep(10000);
                };
            }
            catch (Exception ex)
            {
            }
            Console.ReadLine();
        }

        static void Setup()
        {
            Uri URI = new Uri(_uri);

            _httpClient = new HttpClient();
            _httpClient.BaseAddress = URI;
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(_appType));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_authScheme, _token);

            _message = @"" + System.Environment.NewLine
                       + "CPU : {0} Percent" + System.Environment.NewLine
                       + "CPU TEMP : {1} Celsius" + System.Environment.NewLine
                       + "RAM : {2} Percent" + System.Environment.NewLine
                       + "RAM TEMP : {3} Celsius" + System.Environment.NewLine;
        }

        static string GetCpuUsage()
        {
            return GetWmiByScpoeAndQueryAndObjectname(
                "root\\CIMV2",
                "SELECT * FROM Win32_PerfFormattedData_Counters_ProcessorInformation",
                "PercentProcessorTime");
        }

        static string GetCpuTemputure()
        {
            return GetWmiByScpoeAndQueryAndObjectname(
                "root\\WMI",
                "SELECT * FROM Win32_TemperatureProbe",
                "CurrentTemperature");
        }

        static string GetRamUsage()
        {
            var freePhysicalMemory = GetWmiByScpoeAndQueryAndObjectname(
                "root\\CIMV2",
                "SELECT * FROM Win32_OperatingSystem",
                "FreePhysicalMemory");
            var totalVisibleMemorySize = GetWmiByScpoeAndQueryAndObjectname(
                "root\\CIMV2",
                "SELECT * FROM Win32_OperatingSystem",
                "TotalVisibleMemorySize");

            if(freePhysicalMemory.Equals("-") || totalVisibleMemorySize.Equals("-"))
                return "-";

            return Math.Round(((float.Parse(totalVisibleMemorySize) - float.Parse(freePhysicalMemory)) / float.Parse(totalVisibleMemorySize) * 100), 2).ToString();
        }

        static string GetRamTemputure()
        {
            return GetWmiByScpoeAndQueryAndObjectname(
                "root\\WMI",
                "SELECT * FROM MSAcpi_ThermalZoneTemperature",
                "CurrentTemperature");
        }

        static string GetWmiByScpoeAndQueryAndObjectname(string scope, string query, string objName)
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    if (queryObj[objName] != null)
                    {
                        return queryObj[objName].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return "-";
        }
    }
}
