using GetRegulationsIdctvm.pages;
using GetRegulationsIdctvm.runner;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetRegulationsIdctvm.tests
{
    public class RegulationsTests : TestBase
    {
        private IPage page;
        [SetUp]
        public async Task Setup()
        {
            page = await OpenBrowserAsync();
        }

        [TearDown]
        public async Task Teardown()
        {
            await CloseBrowserAsync();
        }
        [Test]
        public async Task GetRegulationsIdctvm()
        {
            HomePage homePage = new HomePage(page);
            await homePage.GetRegulation();
        }




    }
}
