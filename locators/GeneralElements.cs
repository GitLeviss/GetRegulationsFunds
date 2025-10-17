using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetRegulationsIdctvm.locators
{
    public class GeneralElements
    {
        public string InputCnpj { get; } = "//input[@name='txtCNPJNome']";
        public string ButtonContiue { get; } = "#btnContinuar";
        public string Redirect { get; } = "//a[text()='ID CORRETORA DE TITULOS E VALORES MOBILIARIOS S.A.']";
        public string NameFundOnTable(string fundPosition) => $"(//table//tbody//tr[{fundPosition}]//td)[2]//a[@id]";
        public string TypeFundOnTable(string fundPosition) => $"(//table//tbody//tr[{fundPosition}]//td)[3]";
        public string ReferenceDateOnTable { get; } = $"(//tr//td)[5]";
        public string ClickHere { get; } = "//a[text()='aqui']";
        public string Table { get; } = "//tbody//tr";
        public string RegulationsField { get; } = "//input[@type='search']";
        public string FirstRegulation { get;  } = "(//tbody//tr)[1]//td[2]";
        public string FirstName { get; } = "(//tbody//tr)[1]//td[1]";
        public string ButtonDownloadRegulation { get; } = "(//a[@title='Download do Documento'])[1]";

    }
}
