global using WkeInnuvaDeveloperSummit.Wke;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddControllersWithViews()
    .AddRazorRuntimeCompilation();

var woltersKluwerIdentityConfiguration = builder.Configuration.GetSection("WkeOAuthClientConfiguration").Get<WkeOAuthClientConfigurationModel>();
var authenticationTools = new WkeAuthonticationTools();

var cookieAuthenticationEvents = new CookieAuthenticationEvents()
{
};

var openIdConnectEvents = new OpenIdConnectEvents()
{

    OnAuthorizationCodeReceived = async (context) =>
    {

        var result = await authenticationTools.ProcessAuthorizationCodeAsync(
                     context,
                     woltersKluwerIdentityConfiguration.Authority,
                     woltersKluwerIdentityConfiguration.ClientId,
                     woltersKluwerIdentityConfiguration.ClientSecret)
                 .ConfigureAwait(true);

        var accessToken = result.AccessTokenResponse.AccessToken;

        var jsonToken = (JwtSecurityToken)new JwtSecurityTokenHandler().ReadToken(accessToken);
        var tenantId = jsonToken.Claims.Single(claim => claim.Type == WkeConstants.ClaimTenant).Value;

        result.Claims.Add(new Claim(WkeConstants.ClaimTenant, tenantId));
        result.Claims.Add(new Claim(WkeConstants.ClaimAccessToken, accessToken));
        result.Claims.Add(new Claim(WkeConstants.ClaimExpiresAt, DateTime.Now.AddSeconds(result.AccessTokenResponse.ExpiresIn).ToLocalTime().ToString()));
        result.Claims.Add(new Claim(WkeConstants.ClaimRefreshToken, result.AccessTokenResponse.RefreshToken));
        result.Claims.Add(new Claim(WkeConstants.ClaimIdToken, context.ProtocolMessage.IdToken));

        jsonToken.Claims.Where(claim => claim.Type == JwtClaimTypes.Scope)
            .ToList().ForEach(
                claim => result.Claims.Add(new Claim(JwtClaimTypes.Scope, claim.Value))
            );

        jsonToken.Claims.Where(claim => claim.Type != JwtClaimTypes.Scope)
            .ToList().ForEach(
                claim =>
                {
                    if (!result.Claims.Contains(claim))
                    {
                        result.Claims.Add(claim);
                    }
                });

        ((ClaimsIdentity)context.Principal.Identity).AddClaims(
            result.Claims.Distinct(new ClaimComparer()));

        context.Success();

    }

};

builder.Services
    .AddAuthentication(
        options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
    .AddCookie(
        CookieAuthenticationDefaults.AuthenticationScheme,
        authenticationOptions =>
        {
            authenticationOptions.Cookie.Name = ".Webhook";
            authenticationOptions.ExpireTimeSpan = TimeSpan.FromDays(1);
            authenticationOptions.AccessDeniedPath = PathString.FromUriComponent("/Error");
            authenticationOptions.Events = cookieAuthenticationEvents;
        })
    .AddOpenIdConnect(
        connectOptions =>
        {
            connectOptions.Authority = woltersKluwerIdentityConfiguration.Authority;
            connectOptions.ClientId = woltersKluwerIdentityConfiguration.ClientId;
            connectOptions.ClientSecret = woltersKluwerIdentityConfiguration.ClientSecret;
            connectOptions.SignedOutRedirectUri = $"{woltersKluwerIdentityConfiguration.RedirectUrl}";
            connectOptions.AuthenticationMethod = OpenIdConnectRedirectBehavior.RedirectGet;
            connectOptions.GetClaimsFromUserInfoEndpoint = true;
            connectOptions.RequireHttpsMetadata = false;
            connectOptions.SaveTokens = true;
            connectOptions.Scope.Clear();
            connectOptions.ClaimActions.MapAll();
            foreach (var scope in woltersKluwerIdentityConfiguration.Scopes)
            {
                connectOptions.Scope.Add(scope);
            }

            connectOptions.ResponseType = woltersKluwerIdentityConfiguration.ResponseType;
            connectOptions.Events = openIdConnectEvents;
            connectOptions.TokenValidationParameters = new TokenValidationParameters
            {
                NameClaimType = WkeConstants.ClaimGivenName,
                AuthenticationType = woltersKluwerIdentityConfiguration.AuthenticationType,
                SaveSigninToken = true
            };
        });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
