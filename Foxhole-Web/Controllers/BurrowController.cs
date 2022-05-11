using Foxhole_Web.Models;
using Microsoft.AspNetCore.Mvc;
using RestSharp;

namespace Foxhole_Web.Controllers
{
	public class BurrowController : Controller
	{
		public async Task<IActionResult> Index(string id)
		{
			RestClient rest_client = new(new RestClientOptions($"https://{Config.API_Host}/Burrow"));
			RestRequest rest_request = new();

			rest_request.AddHeader("Content-Type", "application/json");

			rest_request.AddJsonBody
			(
				new
				{
					Burrow_ID = id.Replace('_', '/').Replace('-', '+')
				}
			);

			RestResponse response = await rest_client.PostAsync(rest_request);

			return View();
		}
	}
}
