// tests/RegulationsTests.cs
using GetRegulationsIdctvm.pages;
using GetRegulationsIdctvm.runner;
using GetRegulationsIdctvm.utils;

namespace GetRegulationsIdctvm.tests
{
    public class RegulationsTests : TestBase
    {
        [Test]
        public async Task GetRegulationsIdctvm()
        {
            var summary = new Utils.DownloadSummary();
            var firstPage = await OpenBrowserAsync();
            var home = new HomePage(firstPage);
            await home.NavigateHomeAsync();
            var total = await home.GetTotalAsync();
            await CloseBrowserAsync();

            for (int i = 1; i < total; i++)
            {
                var page = await OpenBrowserAsync();
                var run = new HomePage(page);
                await run.NavigateHomeAsync();
                await run.ProcessRowAsync(i, summary);
                await CloseBrowserAsync();
            }

            Utils.PrintSummary(summary, total);
        }
    }
}
