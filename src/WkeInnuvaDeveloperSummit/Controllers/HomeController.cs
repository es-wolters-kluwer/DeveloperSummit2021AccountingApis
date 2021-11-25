using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using WkeInnuvaDeveloperSummit.Models;

namespace WkeInnuvaDeveloperSummit.Controllers
{
    [Authorize]
    public class HomeController : WkeBaseController
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IConfiguration configuration,
            ILogger<HomeController> logger)
            : base(configuration)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {

            using (var client = this.CreateHttpClient())
            {
                var companiesUrl = $"{this.wkeDeveloperPortalConfiguration.AccountingEndPoint}api/companies?pageNumber=1&pageSize=100";
                var companiesRequest = await client.GetAsync(companiesUrl);

                companiesRequest.EnsureSuccessStatusCode();

                var result = await companiesRequest.Content.ReadAsStringAsync();
                var companies = JsonConvert.DeserializeObject<List<CompanyModel>>(result);

                return View(companies);
            }

        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}