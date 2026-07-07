using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PiscAtlas.WebApp.Pages.Home
{
    public class ErroModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int? StatusCode { get; set; }

        public string MensagemErro { get; set; } = string.Empty;
        public string CodigoErro { get; set; } = string.Empty;

        public void OnGet()
        {
            if (StatusCode.HasValue && StatusCode.Value == 404)
            {
                MensagemErro = "O peixe escapou! A página que procura não existe ou foi movida.";
                CodigoErro = "404";
            }
            else
            {
                MensagemErro = "Ocorreu um problema nas nossas águas. Tente novamente mais tarde.";
                CodigoErro = StatusCode?.ToString() ?? "500";
            }
        }
    }
}