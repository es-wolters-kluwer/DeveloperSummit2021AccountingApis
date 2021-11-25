using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace WkeInnuvaDeveloperSummit.Controllers
{
    public abstract class WkeBaseController : Controller
    {

        protected readonly IConfiguration Configuration;
        protected readonly WkeDeveloperPortalConfigurationModel wkeDeveloperPortalConfiguration;

        public WkeBaseController(
            IConfiguration configuration
            )
        {
            this.Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.wkeDeveloperPortalConfiguration = this.Configuration.GetSection("WkeDeveloperPortalConfiguration").Get<WkeDeveloperPortalConfigurationModel>();
        }

        protected HttpClient CreateHttpClient(string context = "none")
        {

            var client = new HttpClient();
            var authorization = this.User.Claims.First(r => r.Type == WkeConstants.ClaimAccessToken).Value;

            client.BaseAddress = new Uri(this.wkeDeveloperPortalConfiguration.BaseUrl);
            client.DefaultRequestHeaders.Add("Authorization", $"bearer {authorization}");
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", this.wkeDeveloperPortalConfiguration.SubscriptionKey);
            client.DefaultRequestHeaders.Add("Ocp-Apim-Trace", "true");

            var contextToAdd = !string.IsNullOrEmpty(context) ? "{ \"clientId\": \"" + context + "\" }" : "none";

            client.DefaultRequestHeaders.Add("context", $"{contextToAdd}");

            return client;
        }

        public JsonSerializer GetSerializer()
        {
            return JsonSerializer.Create(this.GetCommonJsonSerializerSettings());
        }

        private JsonSerializerSettings GetCommonJsonSerializerSettings()
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore,
                DateParseHandling = DateParseHandling.DateTime,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                TypeNameHandling = TypeNameHandling.Auto,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize
            };

            return settings;
        }

    }
}
