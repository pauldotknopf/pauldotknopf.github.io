using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Blog.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Octokit;

namespace Blog.Controllers
{
	public class LitController : Controller
	{
		private async Task<SessionReportModel> GetSessionReport()
		{
            var tempModel = JsonConvert.DeserializeObject<SessionReportModel>(System.IO.File.ReadAllText("/Users/paul.knopf/git/pauldotknopf/ikitesurf-bot/report.json"), new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            return tempModel;

            var apiKey = Environment.GetEnvironmentVariable("IKSBOT_KEY");
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("IKSBOT_KEY environment variable not set.");
            }
            using (var httpClient = new HttpClient())
            {
                var byteArray = Encoding.ASCII.GetBytes($"x-access-token:{apiKey}");
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                var httpResponseMessage = await httpClient.GetAsync("https://raw.githubusercontent.com/pauldotknopf/ikitesurf-bot/reports/report.json").ConfigureAwait(false);
                var response = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                var model = JsonConvert.DeserializeObject<SessionReportModel>(response, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
                return model;
            }
        }

		public async Task<ActionResult> Index()
		{
            var model = await GetSessionReport();

            foreach(var item in model.SessionSpots)
            {
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById(item.TimeZoneName);
                item.From = item.From.ToOffset(timeZone.GetUtcOffset(item.From));
                item.To = item.To.ToOffset(timeZone.GetUtcOffset(item.To));
                foreach (var data in item.Data)
                {
                    data.ModelTime = data.ModelTime.ToOffset(timeZone.GetUtcOffset(data.ModelTime));
                }
            }

            return View(model);
		}
	}
}

