using Microsoft.Playwright;
using System.Globalization;

namespace GetRegulationsIdctvm.utils
{


    public class Utils
    {
        private readonly IPage page;

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
                await element.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
                await element.FillAsync(text);
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
                await element.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
                await element.FillAsync(text);
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
                await element.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
                await element.ClickAsync();
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
                await page.Locator(locator).ClickAsync();
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
                await element.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
                await element.ClickAsync();
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
            string nomeBase,         // ex: "Regulamento_FIDC_ABC"
            string dataReferencia,   // ex: "2025-10-17" (ou "17-10-2025")
            DownloadSummary summary, // acumula totais da execução
            string raiz = @"C:\RegulamentosIDCTVM"
        )
        {
            try
            {
                var tipos = new[] { "FI", "FIDC", "F.I.I.", "FIAGRO", "FIP", "FUNCINE" };
                if (!tipos.Contains(tipoArquivo))
                    throw new ArgumentException($"Tipo inválido: {tipoArquivo}");

                // O nome "F.I.I." é inválido para pastas no Windows. Precisamos de um nome válido.
                string nomePasta = tipoArquivo;
                if (tipoArquivo == "F.I.I.")
                {
                    nomePasta = "F.I.I"; // Usa um nome de pasta válido, sem o ponto final.
                }

                // Pastas
                var baseFundos = Path.Combine(raiz, "Fundos");
                var dirAtualizados = Path.Combine(baseFundos, "RegulamentosAtualizados", nomePasta);
                var dirAntigos = Path.Combine(baseFundos, "RegulamentosAntigos", nomePasta);

                Directory.CreateDirectory(dirAtualizados);
                Directory.CreateDirectory(dirAntigos);

                // Sanitização e caminhos
                string Sanitize(string s)
                {
                    var invalid = Path.GetInvalidFileNameChars();
                    return new string(s.Where(c => !invalid.Contains(c)).ToArray()).Trim();
                }

                var nomeBaseSafe = Sanitize(nomeBase);
                var dataRefSafe = Sanitize(dataReferencia);
                var nomeNovo = $"{dataRefSafe}_{nomeBaseSafe}.pdf";
                var destinoNovo = Path.Combine(dirAtualizados, nomeNovo);

                // Detectar arquivo existente do mesmo fundo (mesmo nomeBase, data variável)
                var pattern = $"{nomeBaseSafe}_*.pdf";
                var existentes = Directory.EnumerateFiles(dirAtualizados, pattern, SearchOption.TopDirectoryOnly).ToList();

                // Extrair maior data existente
                DateTime? dataExistente = null;
                string? caminhoExistenteMaisRecente = null;

                foreach (var arq in existentes)
                {
                    var dt = TryParseDateFromFileName(arq, nomeBaseSafe);
                    if (dt != null && (dataExistente == null || dt > dataExistente))
                    {
                        dataExistente = dt;
                        caminhoExistenteMaisRecente = arq;
                    }
                }

                // Converter data de referência informada
                var dataNova = TryParseDateFlexible(dataReferencia);
                if (dataNova == null)
                    throw new ArgumentException($"Data de referência inválida: {dataReferencia}");

                // Regras:
                // - Igual: não baixa (Skipped++)
                // - Nova < existente: não baixa (Skipped++)
                // - Nova > existente: move existente -> Antigos e baixa novo (Updated++)
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

                // Se há arquivo anterior e a nova data é mais recente, arquivar o anterior
                if (caminhoExistenteMaisRecente != null && File.Exists(caminhoExistenteMaisRecente))
                {
                    var destinoArquivoAntigo = GetUniquePath(Path.Combine(dirAntigos, Path.GetFileName(caminhoExistenteMaisRecente)));
                    File.Move(caminhoExistenteMaisRecente, destinoArquivoAntigo);
                }

                // Baixar e salvar novo
                var download = await page.RunAndWaitForDownloadAsync(async () =>
                {
                    var element = page.Locator(locatorClickDownload);
                    await element.WaitForAsync();
                    await element.ClickAsync();
                });




                var destinoFinal = GetUniquePath(destinoNovo); // evita colisão acidental
                await download.SaveAsAsync(destinoFinal);

                Assert.That(File.Exists(destinoFinal), $"❌ File '{destinoFinal}' didn't save.");
                var info = new FileInfo(destinoFinal);
                Assert.That(info.Length, Is.GreaterThan(0), $"❌ File '{destinoFinal}' is empty (0 bytes).");

                summary.Updated++;
                summary.FundosAtualizados.Add(nomeBase);

                //HttpClient httpClient = new HttpClient();

                //byte[] bytes = await File.ReadAllBytesAsync(destinoFinal);
                //using var conteudo = new ByteArrayContent(bytes);
                //conteudo.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

                //HttpResponseMessage resposta = await httpClient.PostAsync("https://n8n.zitec.ai/webhook/FundoParametros", conteudo);

                //string conteudoResposta = await resposta.Content.ReadAsStringAsync();

                //Console.WriteLine("Resposta N8N:" + conteudoResposta);
                //Console.WriteLine();
                //Console.WriteLine("para o fundo" + nomeBaseSafe);

            }
            catch
            {
                Assert.Fail($"❌ Error to validate download on step '{step}'");
            }

            // === Helpers ===
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

            static DateTime? TryParseDateFromFileName(string fullPath, string nomeBaseSafe)
            {
                // Formato esperado: "<nomeBaseSafe>_<data>.pdf"
                var file = Path.GetFileNameWithoutExtension(fullPath);
                var idx = file.LastIndexOf('_');
                if (idx < 0) return null;
                var datePart = file[(idx + 1)..];

                return TryParseDateFlexible(datePart);
            }

            static DateTime? TryParseDateFlexible(string s)
            {
                // Aceita vários formatos comuns: 2025-10-17, 17-10-2025, 17/10/2025, 20251017 etc.
                var formats = new[]
                {
            "yyyy-MM-dd","dd-MM-yyyy","dd/MM/yyyy","yyyyMMdd","ddMMyyyy","dd_MM_yyyy","yyyy_MM_dd"
        };
                if (DateTime.TryParseExact(s, formats, CultureInfo.InvariantCulture,
                                           DateTimeStyles.None, out var dt))
                    return dt;
                // fallback
                if (DateTime.TryParse(s, out dt)) return dt;
                return null;
            }
        }
    }
}


