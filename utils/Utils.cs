using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                await page.FrameLocator("frame[name=\"Main\"]").Locator(locator).FillAsync(text);
            }
            catch
            {
                throw new PlaywrightException($"Don´t possible found Locator : {locator} to Write on step: {step}");
            }
        }
        public async Task Write(string locator, string text, string step)
        {
            try
            {
                await page.Locator(locator).FillAsync(text);
            }
            catch
            {
                throw new PlaywrightException($"Don´t possible found Locator : {locator} to Write on step: {step}");
            }
        }
        public async Task Write(IPage page, string locator, string text, string step)
        {
            try
            {
                await page.Locator(locator).FillAsync(text);
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
                await page.FrameLocator("frame[name=\"Main\"]").Locator(locator).ClickAsync();
            }
            catch
            {
                throw new PlaywrightException($"Don´t possible found Locator :{locator} to Click on step: {step}");
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
                await page.Locator(locator).ClickAsync();
            }
            catch
            {
                throw new PlaywrightException($"Don´t possible found Locator :{locator} to Click on step: {step}");
            }
        }

        public async Task ValidateDownloadAndLength(IPage page, string locatorClickDownload, string step, string downloadsDir = null)
        {
            try
            {
                downloadsDir ??= Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Downloads"
                );

                // Dispara o download e captura o objeto
                var download = await page.RunAndWaitForDownloadAsync(async () =>
                {
                    var element = page.Locator(locatorClickDownload);
                    await element.WaitForAsync();
                    await element.ClickAsync();
                });

                // Nome real sugerido pelo navegador
                var fileName = download.SuggestedFilename;
                var finalPath = Path.Combine(downloadsDir, fileName);

                // Remove arquivo pré-existente com o mesmo nome
                if (File.Exists(finalPath))
                    File.Delete(finalPath);

                // Salva no destino final
                await download.SaveAsAsync(finalPath);

                // Validações
                Assert.That(File.Exists(finalPath), $"❌ File '{fileName}' Don´t save.");
                var info = new FileInfo(finalPath);
                Assert.That(info.Length, Is.GreaterThan(0), $"❌ File '{fileName}' be empty (0 bytes).");

                Console.WriteLine($"✅ Download ok: '{fileName}' | {info.Length} bytes.");

                // (Opcional) limpar depois
                // File.Delete(finalPath);
                // Console.WriteLine("ℹ️ Arquivo excluído após validação.");
            }
            catch (Exception ex)
            {
                Assert.Fail($"❌ Error to validate download on step '{step}'");
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
            public List<string> FundosAtualizados { get; } = new();
        }

        public static void PrintSummary(DownloadSummary s)
        {
            Console.WriteLine($"Resumo: Não baixados = {s.Skipped} | Atualizados = {s.Updated}");
            if (s.FundosAtualizados.Count > 0)
                Console.WriteLine("Fundos atualizados: " + string.Join(", ", s.FundosAtualizados.Distinct()));
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

                // *** INÍCIO DA CORREÇÃO ***
                // O nome "F.I.I." é inválido para pastas no Windows. Precisamos de um nome válido.
                // Usaremos uma variável separada para o nome da pasta.
                string nomePasta = tipoArquivo;
                if (tipoArquivo == "F.I.I.")
                {
                    nomePasta = "F.I.I"; // Usa um nome de pasta válido, sem o ponto final.
                }
                // *** FIM DA CORREÇÃO ***

                // Pastas
                var baseFundos = Path.Combine(raiz, "Fundos");
                // Usa a variável 'nomePasta' para criar os diretórios
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
                        // Mantém o uso do 'tipoArquivo' original nos logs para precisão
                        Console.WriteLine($"↩️ {nomeBase} ({tipoArquivo}): já existe com a mesma data {dataNova:yyyy-MM-dd}. Não baixado.");
                        return;
                    }
                    if (dataNova < dataExistente)
                    {
                        summary.Skipped++;
                        // Mantém o uso do 'tipoArquivo' original nos logs para precisão
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
                Console.WriteLine($"✅ Atualizado: '{destinoFinal}' | {info.Length} bytes.");
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
