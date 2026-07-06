using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PiscAtlas.Models.Models;

namespace PiscAtlas.WebApp.Pages.Conta
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<Utilizador> _signInManager;

        public LogoutModel(SignInManager<Utilizador> signInManager)
        {
            _signInManager = signInManager;
        }

        // Apanha quando o utilizador clica no botão (Form POST)
        public async Task<IActionResult> OnPostAsync()
        {
            await _signInManager.SignOutAsync();
            return Redirect("~/");
        }

        // Apanha caso o browser tente aceder diretamente ao link (GET)
        public async Task<IActionResult> OnGetAsync()
        {
            await _signInManager.SignOutAsync();
            return Redirect("~/");
        }
    }
}