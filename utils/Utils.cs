using Microsoft.Playwright;
using System;
using System.Collections.Generic;
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






    }
}
