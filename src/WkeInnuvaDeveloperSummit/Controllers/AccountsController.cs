using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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

    }
}
