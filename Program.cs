using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.Playwright;
using System;
using System.Threading.Tasks;

namespace PlaywrightTokenAcquisition
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            // Setup configuration builder
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddUserSecrets<Program>()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            // Get client app related related settings
            string tenant_id = config.GetValue<string>("tenant_id");
            string client_id = config.GetValue<string>("client_id");
            string redirect_uri = config.GetValue<string>("redirect_uri");
            string scope = config.GetValue<string>("scope");

            // Define authority and login uri
            string authority = $"https://login.microsoftonline.com/{tenant_id}";
            string login_uri = $"{authority}/oauth2/v2.0/authorize?client_id={client_id}&redirect_uri={redirect_uri}&scope={scope}&response_type=code&prompt=login";

            // Create a Playwright instance
            Console.WriteLine("Creating Playwright instance");
            using var playwright = await Playwright.CreateAsync();

            // Launch and instance of Chrome
            Console.WriteLine("Launching Chrome");
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions()
            {
                Headless = true
            });

            // Create a browser page
            var page = await browser.NewPageAsync();

            // Navigate to the login screen
            Console.WriteLine($"Navigating to {login_uri}");
            await page.GotoAsync(login_uri);

            // Enter username
            Console.WriteLine("Entering username");
            await page.FillAsync("input[name='loginfmt']", config.GetValue<string>("username"));
            await page.ClickAsync("input[type=submit]");

            // Wait until page has changed and is loaded
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Enter password
            Console.WriteLine("Entering password");
            await page.FillAsync("input[name='passwd']", config.GetValue<string>("password"));
            await page.ClickAsync("input[type=submit]");

            // Wait until page has changed and is loaded
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Extract the auth code from the page we've redirected it to
            Console.WriteLine("Extract auth code");
            var authCode = await page.InnerTextAsync("#debug-view-component > div.debug__callback-header > div:nth-child(4) > p");

            // Close the browser
            await browser.CloseAsync();

            // Build an MSAL confidential client
            var app = ConfidentialClientApplicationBuilder.Create(client_id)
                .WithAuthority(authority)
                .WithRedirectUri(redirect_uri)
                .WithClientSecret(config.GetValue<string>("client_secret"))
                .Build();

            // Get access token with code exchange
            Console.WriteLine("Request access token");
            AuthenticationResult result = await app.AcquireTokenByAuthorizationCode(new string[] { scope }, authCode)
                .ExecuteAsync();

            // Get the access token from the response
            var access_token = result.AccessToken;
            Console.WriteLine("Access token retrieved:\n");
            Console.WriteLine(access_token);
            Console.WriteLine();

            // All done
            Console.WriteLine("\nProcess complete");
            Console.ReadKey();
        }
    }
}
