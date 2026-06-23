// ADICIONAR estes métodos dentro da class ContaController (em ContaController.cs)
// Coloca-os antes do último } da classe.

// --- PERFIL DO UTILIZADOR ---
[Authorize]
[HttpGet]
public async Task<IActionResult> Perfil()
{
    var user = await _userManager.GetUserAsync(User);
    if (user == null) return Challenge();

    var capturas = await _context.Capturas   // Necessita injetar ApplicationDbContext
        .Include(c => c.Especie)
        .Include(c => c.Pesqueiro)
        .Where(c => c.UtilizadorId == user.Id)
        .OrderByDescending(c => c.DataCaptura)
        .Take(6)
        .ToListAsync();

    var vm = new PerfilViewModel
    {
        User     = user,
        Capturas = capturas
    };

    return View(vm);
}

[Authorize]
[HttpGet]
public async Task<IActionResult> EditarPerfil()
{
    var user = await _userManager.GetUserAsync(User);
    if (user == null) return Challenge();

    var model = new EditarPerfilViewModel
    {
        PrimeiroNome   = user.PrimeiroNome,
        UltimoNome     = user.UltimoNome,
        NomeUtilizador = user.NomeUtilizador,
        FotoAtual      = user.FotografiaPerfilUrl
    };
    return View(model);
}

[Authorize]
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> EditarPerfil(EditarPerfilViewModel model)
{
    if (!ModelState.IsValid) return View(model);

    var user = await _userManager.GetUserAsync(User);
    if (user == null) return Challenge();

    if (model.FotoFile != null && model.FotoFile.Length > 0)
    {
        var fileName  = Guid.NewGuid() + Path.GetExtension(model.FotoFile.FileName);
        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/perfis");
        if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);
        using var stream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create);
        await model.FotoFile.CopyToAsync(stream);
        user.FotografiaPerfilUrl = "/uploads/perfis/" + fileName;
    }

    user.PrimeiroNome   = model.PrimeiroNome;
    user.UltimoNome     = model.UltimoNome;
    user.NomeUtilizador = model.NomeUtilizador;

    var result = await _userManager.UpdateAsync(user);
    if (result.Succeeded)
    {
        TempData["Sucesso"] = "Perfil atualizado com sucesso!";
        return RedirectToAction(nameof(Perfil));
    }

    foreach (var error in result.Errors)
        ModelState.AddModelError(string.Empty, error.Description);

    return View(model);
}
