// runner/TestBase.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;

namespace GetRegulationsIdctvm.runner
{
    public class TestBase
    {
        private IPlaywright? playwright;
        private IBrowser? browser;
        private IBrowserContext? context;

        protected async Task<IPage> OpenBrowserAsync()
        {
            playwright = await Playwright.CreateAsync();
            var launchOptions = new BrowserTypeLaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox", "--disable-dev-shm-usage" }
            };
            var contextOptions = new BrowserNewContextOptions()
            {
                ViewportSize = new ViewportSize() { Width = 1920, Height = 1080 },
                IgnoreHTTPSErrors = true,
                AcceptDownloads = true
            };

            browser = await playwright.Chromium.LaunchAsync(launchOptions);
            context = await browser.NewContextAsync(contextOptions);
            var page = await context.NewPageAsync();

            var config = new ConfigurationManager();
            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            var linkCtvm = config["Links:Ctvm"];
            await page.GotoAsync(linkCtvm);
            return page;
        }

        protected async Task CloseBrowserAsync()
        {
            if (context != null) await context.CloseAsync();
            if (browser != null) await browser.CloseAsync();
            playwright?.Dispose();
        }
    }
}
