using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GetRegulationsIdctvm.locators;
using GetRegulationsIdctvm.runner;
using GetRegulationsIdctvm.utils;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace GetRegulationsIdctvm.pages
{
    public class HomePage
    {
        Utils utils;
        GeneralElements el = new GeneralElements();
        private readonly IPage page;

        public HomePage(IPage page)
        {
            this.page = page;
            utils = new Utils(page);
        }


        public async Task GetRegulation()
        {
            var summary = new Utils.DownloadSummary();


            await Task.Delay(1000);
            await utils.WriteInFrame(el.InputCnpj, "16.695.922/0001-09", "insert cnpj of IDCTVM on input cnpj at home page");
            await utils.ClickInFrame(el.ButtonContiue, "click on button continue at home page");
            await Task.Delay(1000);
            await utils.ClickInFrame(el.Redirect, "click on redirect link at home page");
            await Task.Delay(3000);
            var trLocator = page.FrameLocator("frame[name=\"Main\"]").Locator(el.Table).CountAsync();
            Console.WriteLine($"Total Funds: {await trLocator}");

            for (int i = 1; i < await trLocator; i++)
            {

                try
                {
                    string fundName = await page.FrameLocator("frame[name=\"Main\"]").Locator(el.NameFundOnTable(Convert.ToString(i))).InnerTextAsync();
                    string typeFund = await page.FrameLocator("frame[name=\"Main\"]").Locator(el.TypeFundOnTable(Convert.ToString(i))).InnerTextAsync();
                    await utils.ClickInFrame(el.NameFundOnTable(Convert.ToString(i)), $"click on table position {i} at document manager page");


                    Console.WriteLine($"Name Fund: {fundName}, Type Fund -> {typeFund}");

                    var popupTask = page.Context.WaitForPageAsync();
                    await Task.Delay(2000);


                    if (await page.FrameLocator("frame[name=\"Main\"]").Locator(el.ClickHere).IsVisibleAsync())
                    {
                        await utils.ClickInFrame(el.ClickHere, "click on link here at home page");
                        var popup = await popupTask;
                        await Task.Delay(1500);
                        await utils.Write(popup, el.RegulationsField, "Regulamento", "insert text Regulamento on regulations field at home page");
                        string referenceDate = await popup.Locator(el.ReferenceDateOnTable).InnerTextAsync();

                        var regulationData = await popup.Locator(el.FirstRegulation).InnerTextAsync() + await popup.Locator(el.FirstName).InnerTextAsync();
                        Console.WriteLine($"Info of current Download: {regulationData}");

                        //await utils.Click(popup, el.ButtonDownloadRegulation, "click on button download regulation at home page");
                        await Task.Delay(1500);
                        //await utils.ValidateDownloadAndLength(popup, el.ButtonDownloadRegulation, "validate download and length of regulation", Path.Combine(Directory.GetCurrentDirectory(), "downloads"));
                        await utils.ValidateDownloadAndLength(
                            popup,
                            el.ButtonDownloadRegulation,
                            $"validate download and length of regulation: {regulationData}",
                            typeFund,
                            fundName,
                            referenceDate,
                            summary);


                        await popup.WaitForLoadStateAsync();
                        await popup.CloseAsync();
                    }


                    await page.BringToFrontAsync();
                    await Task.Delay(200);
                    await utils.ReloadPageToResetHome();

                    await Task.Delay(1000);
                    await utils.WriteInFrame(el.InputCnpj, "16.695.922/0001-09", "insert cnpj of IDCTVM on input cnpj at home page");
                    await utils.ClickInFrame(el.ButtonContiue, "click on button continue at home page");
                    await Task.Delay(1000);
                    await utils.ClickInFrame(el.Redirect, "click on redirect link at home page");
                    await Task.Delay(3000);
                    var trLocator2 = page.FrameLocator("frame[name=\"Main\"]").Locator(el.Table).CountAsync();

                    Utils.PrintSummary(summary);
                }
                catch (Exception)
                {
                    Console.WriteLine($"Don´t possible do download of regulation to fund ");
                }

                


            }



        }


    }
}
