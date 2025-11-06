using GetRegulationsIdctvm.locators;
using GetRegulationsIdctvm.utils;
using Microsoft.Playwright;

namespace GetRegulationsIdctvm.pages;

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
                string CnpjFund = await page.FrameLocator("frame[name=\"Main\"]").Locator(el.CnpjFundOnTable(i.ToString())).InnerTextAsync();

                await utils.ClickInFrame(el.NameFundOnTable(i.ToString()), $"click on table position {i} at document manager page");
                Console.WriteLine($"Name Fund: {fundName}, Type Fund -> {typeFund}");

                var popupTask = page.Context.WaitForPageAsync();
                await Task.Delay(4000);


                // Se nenhum dos dois estiver visível, marca como sem regulamento e continua
                if (!await page.FrameLocator("frame[name=\"Main\"]").Locator(el.ClickHere).IsVisibleAsync() &&
                    !await page.FrameLocator("frame[name=\"Main\"]").Locator(el.ButtonClickHere).IsVisibleAsync())
                {
                    summary.Missing++;
                    summary.FundosSemRegulamento.Add(fundName);
                    Console.WriteLine($"ℹ️ Sem regulamento visível para: {fundName}");
                    await ResetToHome();
                    continue;
                }

                // Se o link estiver visível, clica nele
                if (await page.FrameLocator("frame[name=\"Main\"]").Locator(el.ClickHere).IsVisibleAsync())
                {
                    await utils.ClickInFrame(el.ClickHere, "click on link click here at home page");
                }
                // Se o botão estiver visível, clica nele
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
                        dateReferente,
                        summary
                    );
                    await popup1.CloseAsync();
                    await ResetToHome();
                    continue;
                }
                else
                {
                    await utils.ClickInFrame(el.ButtonClickFundsdotNet, "Click on button Fundos.NET to access regulations");
                }


                //await utils.ClickInFrame(el.ClickHere, "click on link here at home page");
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

                if (referenceDate is null)
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
