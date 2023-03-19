using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Blog.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Blog.Controllers
{
	public class LitController : Controller
	{
		public async Task<ActionResult> Index()
		{
			using(var httpClient = new HttpClient())
			{
				var content = await httpClient.GetStringAsync("https://raw.githubusercontent.com/pauldotknopf/ikitesurf-bot/reports/report.json");
				var model = JsonConvert.DeserializeObject<SessionReportModel>(content, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
				return View(model);
            }
		}
	}
}

