using GetRegulationsIdctvm.locators;
using Microsoft.Playwright;
using System.Globalization;
using System.Net.Http.Headers;
using static Microsoft.Playwright.Assertions;

namespace GetRegulationsIdctvm.utils
{


    public class Utils
    {
        private readonly IPage page;

        GeneralElements _el = new GeneralElements();

        public Utils(IPage page)
        {
            this.page = page;
        }

        public async Task WriteInFrame(string locator, string text, string step)
        {
            try
            {
                var frameHandle = page.FrameLocator("frame[name=\"Main\"]");
                var element = frameHandle.Locator(locator);
                await Expect(element).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 90000 });
                await Expect(element).ToBeEnabledAsync(new LocatorAssertionsToBeEnabledOptions { Timeout = 90000 });
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                await element.FocusAsync();
                await element.FillAsync(text, new LocatorFillOptions { Timeout = 90000 });
            }
            catch (Exception ex)
            {
                throw new PlaywrightException($"Don´t possible write on locator: {locator} on step: {step}, Details: {ex.Message}");
            }
        }
        public async Task Write(IPage page, string locator, string text, string step)
        {
            try
            {
                var element = page.Locator(locator);
                await Expect(element).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 90000 });
                await Expect(element).ToBeEnabledAsync(new LocatorAssertionsToBeEnabledOptions { Timeout = 90000 });
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                await element.FocusAsync();
                await element.FillAsync(text, new LocatorFillOptions { Timeout = 90000 });
            }
            catch
            {
                throw new PlaywrightException($"Don´t possible found Locator : {locator} to Write on step: {step}");
            }
        }
        public async Task ClickInFrame(string locator, string step)
        {
            try
            {
                var frameHandle = page.FrameLocator("frame[name=\"Main\"]");
                var element = frameHandle.Locator(locator);
                await Expect(element).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 90000 });
                await Expect(element).ToBeEnabledAsync(new LocatorAssertionsToBeEnabledOptions { Timeout = 90000 });
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                await element.FocusAsync();
                await element.ClickAsync(new LocatorClickOptions { Timeout = 90000 }); ;
            }
            catch (Exception ex)
            {
                throw new PlaywrightException($"Don´t possible write on locator: {locator} on step: {step}, Details: {ex.Message}");
            }
        }
        public async Task DblClickInFrame(string locator, string step)
        {
            try
            {
                var frameHandle = page.FrameLocator("frame[name=\"Main\"]");
                var element = frameHandle.Locator(locator);
                await Expect(element).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 90000 });
                await Expect(element).ToBeEnabledAsync(new LocatorAssertionsToBeEnabledOptions { Timeout = 90000 });
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                await element.FocusAsync();
                await element.DblClickAsync(new LocatorDblClickOptions { Timeout = 2000 }); ;
            }
            catch (Exception ex)
            {
                throw new PlaywrightException($"Don´t possible write on locator: {locator} on step: {step}, Details: {ex.Message}");
            }
        }

        public async Task Click(string locator, string step)
        {
            try
            {
                var element = page.Locator(locator);

                await Expect(element).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 90000 });
                await Expect(element).ToBeEnabledAsync(new LocatorAssertionsToBeEnabledOptions { Timeout = 90000 });
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                await element.ClickAsync(new LocatorClickOptions
                {
                    Timeout = 60000
                });
            }
            catch
            {
                throw new PlaywrightException($"Don´t possible found Locator :{locator} to Click on step: {step}");
            }
        }
        public async Task Click(IPage page, string locator, string step)
        {
            try
            {
                var element = page.Locator(locator);

                await Expect(element).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 90000 });
                await Expect(element).ToBeEnabledAsync(new LocatorAssertionsToBeEnabledOptions { Timeout = 90000 });
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                await element.ClickAsync(new LocatorClickOptions
                {
                    Timeout = 60000
                });
            }
            catch
            {
                throw new PlaywrightException($"Don´t possible found Locator :{locator} to Click on step: {step}");
            }
        }

        public async Task ReloadPageToResetHome()
        {
            try
            {
                await page.EvaluateAsync("location.reload(true)");
            }
            catch
            {
                throw new PlaywrightException("Don´t possible Reload Page");
            }
        }

        public sealed class DownloadSummary
        {
            public int Skipped { get; set; }
            public int Updated { get; set; }
            public int Missing { get; set; }                         // Sem regulamento
            public int Failed { get; set; }                          // Falha por exceção
            public List<string> FundosAtualizados { get; } = new();
            public List<string> FundosSemRegulamento { get; } = new();
            public List<string> FundosComFalha { get; } = new();     // Lista de nomes que falharam
        }

        public static void PrintSummary(DownloadSummary s, int totalFounds)
        {
            Console.WriteLine("\n--- RESUMO FINAL DA EXECUÇÃO ---");
            Console.WriteLine($"Total de Fundos Encontrados: {totalFounds}");
            Console.WriteLine($"Quantidade com Sucesso (Baixados): {s.Updated}");

            // Combina os fundos sem regulamento e os que falharam por exceção
            int totalFalhas = s.Missing + s.Failed;
            Console.WriteLine($"Quantidade com Falha ou Sem Regulamento: {totalFalhas}");

            // Cria uma lista única com todos os nomes de fundos que não tiveram sucesso
            var listaFalhas = s.FundosSemRegulamento.Concat(s.FundosComFalha).Distinct().ToList();

            if (listaFalhas.Any())
            {
                Console.WriteLine("\nLista de Fundos que Falharam ou Não Possuem Regulamento:");
                foreach (var fundo in listaFalhas)
                {
                    Console.WriteLine($"- {fundo}");
                }
            }
            else
            {
                Console.WriteLine("\nTodos os fundos encontrados tiveram seus regulamentos baixados ou já estavam atualizados.");
            }
            Console.WriteLine("----------------------------------\n");
        }

        public async Task ValidateDownloadAndLength(
            IPage page,
            string locatorClickDownload,
            string step,
            string tipoArquivo,      // "FI", "FIDC", "F.I.I.", "FIAGRO", "FIP", "FUNCINE"
            string nomeBase,         // ex: "Regulamento_FIDC_ABC" (nome do fundo)
            string cnpjFund,
            string dataReferencia,   // ex: "2025-10-17" (ou "17-10-2025")
            DownloadSummary summary, // acumula totais da execução
                                     //string raiz = @"C:\Users\LeviAlves\ID CTVM Dropbox\PUBLICO\SITE - POLÍTICAS PUBLICADAS\01. FUNDOS"
            string raiz = @"C:\RegulamentosIDCTVM"

        )
        {
            string Sanitize(string s)
            {
                var invalid = Path.GetInvalidFileNameChars();
                return new string(s.Where(c => !invalid.Contains(c)).ToArray()).Trim();
            }

            static DateTime? TryParseDateFlexible(string s)
            {
                var formats = new[]
                {
                    "yyyy-MM-dd", "dd-MM-yyyy", "dd/MM/yyyy", "yyyyMMdd", "ddMMyyyy", "dd_MM_yyyy", "yyyy_MM_dd"
                };
                if (DateTime.TryParseExact(s, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                    return dt;
                if (DateTime.TryParse(s, out dt)) return dt;
                return null;
            }

            static string GetUniquePath(string path)
            {
                if (!File.Exists(path)) return path;
                var dir = Path.GetDirectoryName(path)!;
                var name = Path.GetFileNameWithoutExtension(path);
                var ext = Path.GetExtension(path);
                int i = 1;
                string candidate;
                do { candidate = Path.Combine(dir, $"{name} ({i++}){ext}"); }
                while (File.Exists(candidate));
                return candidate;
            }

            try
            {
                var tipos = new[] { "FI", "FIDC", "F.I.I.", "FIAGRO", "FIP", "FUNCINE" };
                if (!tipos.Contains(tipoArquivo))
                    throw new ArgumentException($"Tipo inválido: {tipoArquivo}");

                // Nome do tipo para uso em arquivo (F.I.I. → FII para evitar problemas no nome do arquivo)
                string tipoParaArquivo = tipoArquivo == "F.I.I." ? "FII" : tipoArquivo;

                var dataNova = TryParseDateFlexible(dataReferencia);
                if (dataNova == null)
                    throw new ArgumentException($"Data de referência inválida: {dataReferencia}");

                var nomeFundoSafe = Sanitize(nomeBase);
                var dataRefFormatada = dataNova.Value.ToString("yyyy-MM-dd");

                // Estrutura: raiz (Dropbox) / Nome do Fundo / 01. REGULAMENTO / dataReferencia_TipoFundo.pdf
                var dirFundo = Path.Combine(raiz.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), nomeFundoSafe);
                var dirRegulamento = Path.Combine(dirFundo, "01. REGULAMENTO");

                Directory.CreateDirectory(dirRegulamento);

                var nomeArquivo = $"{dataRefFormatada}_{tipoParaArquivo}.pdf";
                var destinoNovo = Path.Combine(dirRegulamento, nomeArquivo);

                // Verificar se já existe arquivo do mesmo tipo com mesma data ou mais recente
                var pattern = $"*_{tipoParaArquivo}.pdf";
                var existentes = Directory.EnumerateFiles(dirRegulamento, pattern, SearchOption.TopDirectoryOnly).ToList();
                DateTime? dataExistente = null;

                foreach (var arq in existentes)
                {
                    var nomeArq = Path.GetFileNameWithoutExtension(arq);
                    var idx = nomeArq.IndexOf('_');
                    if (idx >= 0)
                    {
                        var datePart = nomeArq[..idx];
                        var dt = TryParseDateFlexible(datePart);
                        if (dt != null && (dataExistente == null || dt > dataExistente))
                            dataExistente = dt;
                    }
                }

                if (dataExistente != null)
                {
                    if (dataNova == dataExistente)
                    {
                        summary.Skipped++;
                        Console.WriteLine($"↩️ {nomeBase} ({tipoArquivo}): já existe com a mesma data {dataNova:yyyy-MM-dd}. Não baixado.");
                        return;
                    }
                    if (dataNova < dataExistente)
                    {
                        summary.Skipped++;
                        Console.WriteLine($"↩️ {nomeBase} ({tipoArquivo}): já existe versão mais recente ({dataExistente:yyyy-MM-dd}). Não baixado.");
                        return;
                    }
                }

                // Baixar e salvar em: raiz / Nome do Fundo / 01. REGULAMENTO / dataReferencia_TipoFundo.pdf
                var download = await page.RunAndWaitForDownloadAsync(async () =>
                {
                    var element = page.Locator(locatorClickDownload);
                    await element.WaitForAsync();
                    await element.ClickAsync();
                });

                var destinoFinal = GetUniquePath(destinoNovo);
                await download.SaveAsAsync(destinoFinal);

                Assert.That(File.Exists(destinoFinal), $"File '{destinoFinal}' did save.");
                var info = new FileInfo(destinoFinal);
                Assert.That(info.Length, Is.GreaterThan(0), $"File '{destinoFinal}' is not empty (0 bytes).");

                summary.Updated++;
                summary.FundosAtualizados.Add(nomeBase);

                // Envio para N8N
                using var httpClient = new HttpClient();
                byte[] bytes = await File.ReadAllBytesAsync(destinoFinal);
                using var formData = new MultipartFormDataContent();
                using var fileContent = new ByteArrayContent(bytes);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                formData.Add(fileContent, "data", Path.GetFileName(destinoFinal));

                var resposta = await httpClient.PostAsync("https://n8n.zitec.ai/webhook/contrato-fundo", formData);
                var conteudoResposta = await resposta.Content.ReadAsStringAsync();

                Console.WriteLine("Resposta N8N:" + conteudoResposta);
                Console.WriteLine("para o fundo " + nomeFundoSafe);
            }
            catch
            {
                Assert.Fail($"❌ Error to validate download on step '{step}'");
            }
        }

        public async Task RelevantFactFlow()
        {
            if (!await page.FrameLocator("frame[name=\"Main\"]").Locator(_el.ButtonRelevantFact).IsVisibleAsync())
            {
                return;
            }

            await ClickInFrame(_el.ButtonRelevantFact, "Click on Relevant Fact to redirect to list of able documents to download");
            await Task.Delay(2500);
            string referenceDate = await page.FrameLocator("frame[name=\"Main\"]").Locator(_el.ReferenceDateFact).InnerTextAsync();
            referenceDate = referenceDate.Trim()
                .Replace("/", "");
            await ClickInFrame(_el.ButtonToRedirectDownloaderFact, "Click on link to redirect to page with button to download relevant fact");
            await Task.Delay(1000);
            for (var i = 0; i < 2; i++)
            {
                try
                {
                    var frame = page.FrameLocator("iframe");
                    var saveButton = frame
                        .Locator("pdf-viewer")
                        .Locator("viewer-download-controls")
                        .Locator("#save");


                    await saveButton.ClickAsync();
                    //await Click(_el.ButtonDownloadFact, "Click on button to download relevant fact");
                }
                catch
                {
                    continue;
                }
            }

        }

    }
}


