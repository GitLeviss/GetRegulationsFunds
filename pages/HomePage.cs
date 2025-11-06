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

            var total = await page.FrameLocator("frame[name=\"Main\"]").Locator(el.Table).CountAsync();
            Console.WriteLine($"Total Funds: {total}");

            for (int i = 1; i < total; i++)
            {
                // Declara fundName fora do try para ser acessível no catch em caso de falha
                string fundName = "Desconhecido";
                try
                {
                    fundName = await page.FrameLocator("frame[name=\"Main\"]").Locator(el.NameFundOnTable(i.ToString())).InnerTextAsync();
                    string typeFund = await page.FrameLocator("frame[name=\"Main\"]").Locator(el.TypeFundOnTable(i.ToString())).InnerTextAsync();

                    await utils.ClickInFrame(el.NameFundOnTable(i.ToString()), $"click on table position {i} at document manager page");
                    Console.WriteLine($"Name Fund: {fundName}, Type Fund -> {typeFund}");

                    var popupTask = page.Context.WaitForPageAsync();
                    await Task.Delay(3500);

                    // Se não houver link/cta para abrir gerenciador, marque como sem regulamento e continue
                    if (!await page.FrameLocator("frame[name=\"Main\"]").Locator(el.ClickHere).IsVisibleAsync())
                    {
                        summary.Missing++;
                        summary.FundosSemRegulamento.Add(fundName);
                        Console.WriteLine($"ℹ️ Sem regulamento visível para: {fundName}");
                        await ResetToHome();
                        continue;
                    }

                    await utils.ClickInFrame(el.ClickHere, "click on link here at home page");
                    var popup = await popupTask;
                    await Task.Delay(3000);

                    // Filtro "Regulamento"
                    await utils.Write(popup, el.RegulationsField, "Regulamento", "insert text Regulamento on regulations field at home page");

                    // Aguarde aparecer algo clicável ou determine ausência
                    var hasRow = await popup.Locator(el.FirstRegulation).IsVisibleAsync();
                    var hasDownloadBtn = await popup.Locator(el.ButtonDownloadRegulation).IsVisibleAsync();

                    if (!hasRow || !hasDownloadBtn)
                    {
                        summary.Missing++;
                        summary.FundosSemRegulamento.Add(fundName);
                        Console.WriteLine($"ℹ️ Nenhum regulamento listado para: {fundName}");
                        await popup.CloseAsync();
                        await ResetToHome();
                        continue;
                    }

                    string referenceDate = await popup.Locator(el.ReferenceDateOnTable(fundName)).InnerTextAsync();

                    if(referenceDate is null)
                    {
                        await popup.CloseAsync();
                        await ResetToHome();
                        continue;
                    }

                    var regulationData = (await popup.Locator(el.FirstRegulation).InnerTextAsync())
                                       + (await popup.Locator(el.FirstName).InnerTextAsync());
                    Console.WriteLine($"Info of current Download: {regulationData}");

                    await Task.Delay(3000);

                    await utils.ValidateDownloadAndLength(
                        popup,
                        el.ButtonDownloadRegulation,
                        $"validate download and length of regulation: {regulationData}",
                        typeFund,
                        fundName,
                        referenceDate,
                        summary
                    );
           

                    await popup.WaitForLoadStateAsync();
                    await popup.CloseAsync();

                    await ResetToHome();
                }
                catch (Exception ex)
                {
                    // Captura a falha e adiciona ao resumo
                    Console.WriteLine($"Falha ao processar fundo '{fundName}'. Detalhe: {ex.Message}");
                    summary.Failed++;
                    summary.FundosComFalha.Add(fundName);
                    await ResetToHome();
                }
            }

            // Chama o resumo final uma única vez, após o loop, passando o total de fundos.
            Utils.PrintSummary(summary, total);
        }

        async Task ResetToHome()
        {
            await page.BringToFrontAsync();
            await Task.Delay(200);
            await utils.ReloadPageToResetHome();
            await Task.Delay(1000);
            await utils.WriteInFrame(el.InputCnpj, "16.695.922/0001-09", "insert cnpj of IDCTVM on input cnpj at home page");
            await utils.ClickInFrame(el.ButtonContiue, "click on button continue at home page");
            await Task.Delay(1000);
            await utils.ClickInFrame(el.Redirect, "click on redirect link at home page");
            await Task.Delay(3000);
        }
    }
}
