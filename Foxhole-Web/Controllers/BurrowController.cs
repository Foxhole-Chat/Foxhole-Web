using Foxhole_Web.Models;
using Microsoft.AspNetCore.Mvc;
using RestSharp;

namespace Foxhole_Web.Controllers
{
	public class BurrowController : Controller
	{
		public IActionResult Index(string id)
		{
			return View();
		}
	}
}
