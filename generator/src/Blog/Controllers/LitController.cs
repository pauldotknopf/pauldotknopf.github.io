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
		public async Task<ActionResult> Index()
		{
			var apiKey = Environment.GetEnvironmentVariable("IKSBOT_KEY");
			if (string.IsNullOrEmpty(apiKey))
			{
				throw new Exception("IKSBOT_KEY environment variable not set.");
			}
			using(var httpClient = new HttpClient())
			{
				var byteArray = Encoding.ASCII.GetBytes($"x-access-token:{apiKey}");
				httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

				var httpResponseMessage = await httpClient.GetAsync("https://raw.githubusercontent.com/pauldotknopf/ikitesurf-bot/reports/report.json").ConfigureAwait(false);
				var response = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                
				var model = JsonConvert.DeserializeObject<SessionReportModel>(response, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
				return View(model);
            }
		}
	}
}

