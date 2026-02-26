namespace GetRegulationsIdctvm.locators
{
    public class GeneralElements
    {
        public string InputCnpj { get; } = "//input[@name='txtCNPJNome']";
        public string ButtonContiue { get; } = "#btnContinuar";
        public string Redirect { get; } = "//a[text()='ID CORRETORA DE TITULOS E VALORES MOBILIARIOS S.A.']";
        public string NameFundOnTable(string fundPosition) => $"(//table//tbody//tr[{fundPosition}]//td)[2]//a[@id]";
        public string TypeFundOnTable(string fundPosition) => $"(//table//tbody//tr[{fundPosition}]//td)[3]";
        public string CnpjFundOnTable(string fundPosition) => $"(//table//tbody//tr[{fundPosition}]//td//a)[1]";
        public string ReferenceDateOnTable(string fundName) => $"(//td[normalize-space(text())='{fundName}'][1]/ancestor::tr//td[6])[1]";
        public string ClickHere { get; } = "//a[text()='aqui']";
        public string ButtonClickHere { get; } = "//a[text()='clique aqui']";
        public string ButtonClickFundsdotNet { get; } = "//a[text()=' Fundos.NET ']";
        public string Table { get; } = "//tbody//tr";
        public string NameFundsInTable { get; } = "//tbody//tr//td[2]";
        public string RegulationsField { get; } = "//input[@type='search']";
        public string FirstRegulation { get; } = "(//tbody//tr)[1]//td[2]";
        public string FirstName { get; } = "(//tbody//tr)[1]//td[1]";
        public string ButtonDownloadRegulation { get; } = "(//b[text()='Regulamento']/ancestor::tr//a[@title='Download do Documento'])[1]";
        public string ButtonFindFund { get; } = "//span[text()='Consultar Fundo']"; //Consultar Fundo não Adaptado
        public string ButtonFindFundNotPerform { get; } = "//span[text()='Consultar Fundo não Adaptado']"; //Consultar Fundo não Adaptado
        public string InputCnpjFund { get; } = "#txtCnpj";
        public string ButtonSearch { get; } = "//span[text()='Pesquisar']";
        public string ButtonDetailsOfFund(string cnpjFund) => $"//tr//td[normalize-space(text()='{cnpjFund}')][3]/ancestor::tr//td[8]//a[1]";
        public string NavRegulation { get; } = "//ul//div[normalize-space(text())='Regulamento']";
        public string DateReference { get; } = "//td[normalize-space(text())='Ativo']/ancestor::tr//td[2]";
        public string ButtonAction { get; } = "//td[normalize-space(text())='Ativo']/ancestor::tr//td[5]//a";
        public string ButtonDownloadRegulationOfFund { get; } = "//div[@data-ng-show]//a[@title='Download do Arquivo']";
        public string ButtonRelevantFact { get; } = "//font[normalize-space(text())='Fato Relevante']";
        public string ButtonToRedirectDownloaderFact { get; } = "(//a[normalize-space(text())='FATO RELEVANTE'])[1]";
        public string ReferenceDateFact { get; } = "((//td//b)[2]/ancestor::tbody//tr//td)[5]";
        public string ButtonDownloadFact { get; } = "#save";

        //await page.GoBackAsync(); voltar na pagina




    }
}
