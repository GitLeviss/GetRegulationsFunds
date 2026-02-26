// pages/HomePage.cs
using GetRegulationsIdctvm.locators;
using GetRegulationsIdctvm.utils;
using Microsoft.Playwright;

namespace GetRegulationsIdctvm.pages
{
    public class HomePage
    {
        Utils utils;
        GeneralElements el = new GeneralElements();
        private IPage page;

        public HomePage(IPage page)
        {
            this.page = page;
            utils = new Utils(page);
        }

        public async Task NavigateHomeAsync()
        {
            //await Task.Delay(1000);
            await utils.WriteInFrame(el.InputCnpj, "16.695.922/0001-09", "insert cnpj of IDCTVM on input cnpj at home page");
            await utils.ClickInFrame(el.ButtonContiue, "click on button continue at home page");
            //await Task.Delay(1000);
            await utils.ClickInFrame(el.Redirect, "click on redirect link at home page");
            await Task.Delay(3000);
        }

        public async Task<int> GetTotalAsync()
        {
            var table = page.FrameLocator("frame[name=\"Main\"]").Locator(el.Table);
            var total = await table.CountAsync();
            return total;
        }

        public async Task<List<string>> GetFundsName()
        {
            var nameInTable = page.FrameLocator("frame[name=\"Main\"]").Locator(el.NameFundsInTable);
            var count = await nameInTable.CountAsync();
            List<string> nameOfFund = new List<string>();

            for (int i = 0; i < count; i++)
            {
                var fundName = await nameInTable.Nth(i).InnerTextAsync();
                nameOfFund.Add(fundName);
                Console.WriteLine(fundName);
            }

            return nameOfFund;
        }

        public async Task ProcessRowAsync(int i, Utils.DownloadSummary summary)
        {
            string fundName = "Desconhecido";
            try
            {
                fundName = await page.FrameLocator("frame[name=\"Main\"]").Locator(el.NameFundOnTable(i.ToString())).InnerTextAsync();
                string typeFund = await page.FrameLocator("frame[name=\"Main\"]").Locator(el.TypeFundOnTable(i.ToString())).InnerTextAsync();
                string CnpjFund = await page.FrameLocator("frame[name=\"Main\"]").Locator(el.CnpjFundOnTable(i.ToString())).InnerTextAsync();

                await utils.ClickInFrame(el.NameFundOnTable(i.ToString()), $"click on table position {i} at document manager page");

                //await utils.RelevantFactFlow();

                var popupTask = page.Context.WaitForPageAsync();
                await Task.Delay(4000);

                if (!await page.FrameLocator("frame[name=\"Main\"]").Locator(el.ClickHere).IsVisibleAsync() &&
                    !await page.FrameLocator("frame[name=\"Main\"]").Locator(el.ButtonClickHere).IsVisibleAsync())
                {
                    summary.Missing++;
                    summary.FundosSemRegulamento.Add(fundName);
                    return;
                }

                if (await page.FrameLocator("frame[name=\"Main\"]").Locator(el.ClickHere).IsVisibleAsync())
                {
                    await utils.ClickInFrame(el.ClickHere, "click on link click here at home page");
                }
                else if (await page.FrameLocator("frame[name=\"Main\"]").Locator(el.ButtonClickHere).IsVisibleAsync())
                {
                    await utils.ClickInFrame(el.ButtonClickHere, "click on button click here at home page");
                    var popup1 = await popupTask;
                    await utils.Click(popup1, el.ButtonFindFund, "click on button to find fund");
                    await Task.Delay(2500);
                    await utils.Click(popup1, el.InputCnpjFund, "click on input cnpj at popup to activate the input");
                    await utils.Write(popup1, el.InputCnpjFund, CnpjFund, "insert cnpj of fund on input cnpj at popup");
                    await utils.Click(popup1, el.ButtonSearch, "click on button search fund at popup");
                    await Task.Delay(2000);
                    if (!await popup1.Locator(el.ButtonDetailsOfFund(CnpjFund)).IsVisibleAsync())
                    {
                        await popup1.GoBackAsync();
                        await utils.Click(popup1, el.ButtonFindFundNotPerform, "click on button to find fund");
                        await Task.Delay(2500);
                        await utils.Click(popup1, el.InputCnpjFund, "click on input cnpj at popup to activate the input");
                        await utils.Write(popup1, el.InputCnpjFund, CnpjFund, "insert cnpj of fund on input cnpj at popup");
                        await utils.Click(popup1, el.ButtonSearch, "click on button search fund at popup");
                        await Task.Delay(3000);
                    }
                    await utils.Click(popup1, el.ButtonDetailsOfFund(CnpjFund), "click on button details of fund at popup");
                    await utils.Click(popup1, el.NavRegulation, "click on navigation regulation at popup");
                    string dateReferente = (await popup1.Locator(el.DateReference).InnerTextAsync()).Trim();
                    await utils.Click(popup1, el.ButtonAction, "click on action button to open regulation");
                    await Task.Delay(2000);
                    await utils.ValidateDownloadAndLength(
                        popup1,
                        el.ButtonDownloadRegulationOfFund,
                        $"validate download and length of regulation: {fundName}",
                        typeFund,
                        fundName,
                        CnpjFund,
                        dateReferente,
                        summary
                    );
                    await popup1.CloseAsync();
                    return;
                }
                else
                {
                    await utils.ClickInFrame(el.ButtonClickFundsdotNet, "Click on button Fundos.NET to access regulations");
                }

                var popup = await popupTask;
                //await Task.Delay(3000);

                bool regulationsField = await page.Locator(el.RegulationsField).IsVisibleAsync();

                if (regulationsField != true)
                {
                    await page.ReloadAsync();
                }
                await utils.Write(popup, el.RegulationsField, "Regulamento", "insert text Regulamento on regulations field at home page");
                await Task.Delay(2000);
                //Adicionar trava no Xpath, baixar apenas se for do texto do regulamento
                var hasRow = await popup.Locator(el.FirstRegulation).IsVisibleAsync();
                var hasDownloadBtn = await popup.Locator(el.ButtonDownloadRegulation).IsVisibleAsync();

                if (!hasRow || !hasDownloadBtn)
                {
                    summary.Missing++;
                    summary.FundosSemRegulamento.Add(fundName);
                    await popup.CloseAsync();
                    return;
                }

                string referenceDate = await popup.Locator(el.ReferenceDateOnTable(fundName)).InnerTextAsync();
                if (referenceDate is null)
                {
                    await popup.CloseAsync();
                    return;
                }

                var regulationData = (await popup.Locator(el.FirstRegulation).InnerTextAsync())
                                   + (await popup.Locator(el.FirstName).InnerTextAsync());

                //await Task.Delay(3000);

                await utils.ValidateDownloadAndLength(
                    popup,
                    el.ButtonDownloadRegulation,
                    $"validate download and length of regulation: {regulationData}",
                    typeFund,
                    fundName,
                    CnpjFund,
                    referenceDate,
                    summary
                );

                await popup.WaitForLoadStateAsync();
                await popup.CloseAsync();
            }
            catch (Exception ex)
            {
                summary.Failed++;
                summary.FundosComFalha.Add(fundName);
            }
        }
    }
}
