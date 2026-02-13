using Microsoft.AspNetCore.Mvc;
using MvcCoreProceduresEF.Models;
using MvcCoreProceduresEF.Repositories;

namespace MvcCoreProceduresEF.Controllers
{
    public class TrabajadoresController : Controller
    {
        private RepositoryEmpleados repo;

        public TrabajadoresController(RepositoryEmpleados repo)
        {
            this.repo = repo;
        }
        public async Task<IActionResult> Index()
        {
            TrabajadoresModel model = await repo.GetTrabajadoresModelAsync();
            List<string> oficios = await repo.GetOficiosAsync();
            ViewData["OFICIOS"] = oficios;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(string oficio)
        {
            TrabajadoresModel model = await repo.GetTrabajadoresModelsAsync(oficio);
            List<string> oficios = await repo.GetOficiosAsync();
            ViewData["OFICIOS"] = oficios;
            return View(model);
        }
    }
}
