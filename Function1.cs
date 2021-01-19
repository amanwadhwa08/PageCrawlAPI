using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;

namespace PageCrawlAPI
{
    public static class PageCrawlAPI
    {
        public static string StringStorage = "";

        public static string ExtractHref(string URL)
        {
            var urlhost=new Uri(URL).Host;
            var protocol = new Uri(URL).Scheme;
            Console.WriteLine(protocol + urlhost);
            HtmlWeb web = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc = web.Load(URL);

            var uri = "";
            List<string> Result = new List<string>();

            StringStorage = StringStorage + "\"" + URL + "\"" + ",";
            // extracting all links
            var linkss = doc.DocumentNode.SelectNodes("//a[@href]");
            try
            {

                Console.WriteLine(linkss.Count);
                foreach (HtmlNode link in linkss)
                {
                    Console.WriteLine(link.Attributes["href"].Value);
                    HtmlAttribute att = link.Attributes["href"];
                    var linkhost = urlhost;
                    // showing output
                    uri = att.Value;
                    
                    if (!uri.StartsWith("/"))
                    {
                        try
                        {
                            linkhost = new Uri(uri).Host;
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("link failed on no host");
                        }
                    }

                    if (uri.StartsWith("/") || linkhost.Contains(urlhost))
                    {
                        if (uri.StartsWith("/"))
                        {
                            uri = protocol + "://www." + linkhost + uri;

                            Result.Add(uri);
                            Console.WriteLine(uri);
                        }
                    }
                    }

                }
            
            catch (Exception)
            {
                Console.WriteLine("3");
            }   
            var noDupes = Result.Distinct().ToList();
            var Output1 = "[";
            foreach (var lst in noDupes)
            {
                Output1 += "\""+lst +"\""+ ",";
            }
            
            if (Output1.Contains(","))
                Output1 = Output1.Remove(Output1.LastIndexOf(","));
            Output1 = Output1 + "]";
            var results = String.Join(", ", noDupes.ToArray());
            return Output1;
        }
        [FunctionName("PageCrawlAPI")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string url = req.Query["url"];
            if (!url.Contains("://"))
                url = "http://" + url;
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            url = url ?? data?.url;
            Console.WriteLine(url);
           var output= ExtractHref(url);
           
            return new OkObjectResult(output);
        }
    }
}
