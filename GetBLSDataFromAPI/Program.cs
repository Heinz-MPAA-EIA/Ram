using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace GetBLSDataFromAPI
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new RestClient("http://api.bls.gov");
            var request = new RestRequest("publicAPI/v1/timeseries/data/", Method.POST);
            short yearCode = 2013;
            string seriesId = "ENUUS000105512110";
            request.RequestFormat = DataFormat.Json;
            request.AddBody(new
            {
                seriesid = new string[] { seriesId },
                startyear = yearCode.ToString(),
                endyear = yearCode.ToString()
            });

            IRestResponse response = client.Execute(request);
            if (response.ResponseStatus == ResponseStatus.Completed && response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                JObject json = JObject.Parse(client.Execute(request).Content);
                //Console.WriteLine(json.ToString());


                if (json["status"].ToString() != "REQUEST_SUCCEEDED")
                {
                    Console.WriteLine("No results");
                    Console.WriteLine(json["message"].ToString());
                }
                else
                {
                    JObject results = JObject.Parse(json.Value<string>("Results"));
                    Console.WriteLine(results.ToString());

                    if (json["message"].ToString().Contains("Series not exist for Series " + seriesId))
                    {
                        Console.WriteLine("Invalid series identifier.....");
                        return;
                    }
                    else if (json["message"].ToString().Contains("No Data Available for Series " + seriesId + " Year: " + yearCode))
                    {
                        Console.WriteLine("No Data Available for Series " + seriesId + " Year: " + yearCode);
                        return;
                    }
                    
                    var data = results["series"].Single(x => ((string)x["seriesID"]) == seriesId)["data"];

                    var annualData = data.SingleOrDefault(x => ((string)x["periodName"]) == "Annual");
                    if (annualData == null)
                    {
                        Console.WriteLine("Annual data does not exists");
                        return;
                    }
                    var value = annualData["value"].ToString();
                    if (value == "-")
                    {
                        Console.WriteLine("Non disclosable");
                    }
                    else
                    {
                        Console.WriteLine("Value = " + value);
                    }
                }    
            }
            else
            {
                Console.WriteLine("Error in connecting : check the internet connectivity");
            }

            //foreach (var annualData in res)
            //{
            //    Console.WriteLine(annualData["value"].ToString());    
            //}
            
        }
    }
}
