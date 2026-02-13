using Microsoft.AspNetCore.Mvc;
using MvcCoreProceduresEF.Models;
using MvcCoreProceduresEF.Repositories;

namespace MvcCoreProceduresEF.Controllers
{
    public class DoctoresController : Controller
    {
        private RepositoryDoctores repo;

        public DoctoresController(RepositoryDoctores repo)
        {
            this.repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            List<string> especialidades =
                await this.repo.GetEspecialidadesAsync();
            ViewData["ESPECIALIDADES"] = especialidades;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> 
            Index(string especialidad, int incremento
            , string accion)
        {
            if (accion.ToLower() == "incrementar")
            {
                await this.repo
                    .UpdateDoctorEspecialidadAsync(especialidad
                    , incremento);
            }
            if (accion.ToLower() == "incrementaref")
            {
                await this.repo
                    .UpdateDoctoresEspecialidadEFAsync(especialidad
                    , incremento);
            }
            List<string> especialidades =
                await this.repo.GetEspecialidadesAsync();
            ViewData["ESPECIALIDADES"] = especialidades;
            List<Doctor> doctores =
                await this.repo
                .GetDoctoresEspecialidadAsync(especialidad);
            return View(doctores);
        }
    }
}
