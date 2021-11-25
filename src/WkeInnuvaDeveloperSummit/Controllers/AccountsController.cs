using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace WkeInnuvaDeveloperSummit.Controllers
{
    public class AccountsController : WkeBaseController
    {
        public AccountsController(
            IConfiguration configuration,
            ILogger<HomeController> logger) 
            : base(configuration)
        {
        }

        // GET: AccountsController
        public async Task<IActionResult> Index(string companyCorrelationId)
        {

            using (var client = this.CreateHttpClient(companyCorrelationId))
            {
                var accountsUrl = $"{this.wkeDeveloperPortalConfiguration.AccountingEndPoint}api/accounts?pageNumber=1&pageSize=1000";
                var accountsRequest = await client.GetAsync(accountsUrl);

                accountsRequest.EnsureSuccessStatusCode();

                var result = await accountsRequest.Content.ReadAsStringAsync();
                var accounts = JsonConvert.DeserializeObject<List<AccountModel>>(result);

                ViewBag.CompanyCorrelationId = companyCorrelationId;

                return View(accounts);
            }

        }

        // GET: AccountsController/Details/5
        public async Task<IActionResult> Details(string companyCorrelationId, string accountCorrelationId)
        {
            using (var client = this.CreateHttpClient(companyCorrelationId))
            {
                var accountsUrl = $"{this.wkeDeveloperPortalConfiguration.AccountingEndPoint}api/accounts/{accountCorrelationId}";
                var accountsRequest = await client.GetAsync(accountsUrl);

                accountsRequest.EnsureSuccessStatusCode();

                var result = await accountsRequest.Content.ReadAsStringAsync();
                var accounts = JsonConvert.DeserializeObject<AccountModel>(result);

                ViewBag.CompanyCorrelationId = companyCorrelationId;

                return View(accounts);
            }
        }

        // POST: AccountsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CorrelationId,Code,Description")] CreateAccountCommand createAccountCommand, string companyCorrelationId)
        {

            using (var client = this.CreateHttpClient(companyCorrelationId))
            {
                var accountsUrl = $"{this.wkeDeveloperPortalConfiguration.AccountingEndPoint}api/accounts";
                var json = JsonConvert.SerializeObject(createAccountCommand);
                var command = new StringContent(json, Encoding.UTF8, "application/json");
                var createAccountRequest = await client.PostAsync(accountsUrl, command);

                ViewBag.CompanyCorrelationId = companyCorrelationId;

                return RedirectToAction("Index", new { companyCorrelationId = companyCorrelationId });

            }

        }

    }
}
