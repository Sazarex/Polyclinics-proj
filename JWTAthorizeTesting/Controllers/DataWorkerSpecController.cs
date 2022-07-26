using Microsoft.AspNetCore.Mvc;
using JWTAthorizeTesting.Domain;
using JWTAthorizeTesting.Entities;
using JWTAthorizeTesting.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace JWTAthorizeTesting.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class DataWorkerSpecController : Controller
    {

        private AppDbContext db;
        public DataWorkerSpecController(AppDbContext _db)
        {
            db = _db;
        }

        public async Task<IActionResult> Specializations()
        {
            //View Model с данными отправляем в представление
            AdminPanelViewModel adminModel = new AdminPanelViewModel()
            {
                Doctors = db.Doctors.ToList(),
                Specializations = db.Specializations
                .Include(s => s.Doctors)
                .ToList(),
                Cities = db.Cities.ToList(),
                Experiences = db.Experiences.ToList(),
                Polyclinics = db.Polyclinics.Include(p => p.Doctors).ToList()
            };
            return View(adminModel);
        }


        [HttpGet]
        public async Task<IActionResult> EditSpec(int? specId)
        {
            if (specId == null || specId == 0)
            {
                return NotFound();
            }

            Specialization? spec = await db.Specializations
                .Include(s => s.Doctors)
                .FirstOrDefaultAsync(s => s.SpecializationId == specId);

            if (spec == null)
            { 
                return NotFound();
            }

            //Вытягиваем всех врачей, у котороых в специализации отсутствует текущая специализация
            List<Doctor> docsToAdd =await db.Doctors.Where(d => !d.Specializations.Contains(spec)).ToListAsync();

            AdminPanelViewModel adminPanelView = new AdminPanelViewModel();
            adminPanelView.Specializations.Add(spec);

            adminPanelView.Doctors.AddRange(docsToAdd);

            return View(adminPanelView);


        }


        public async Task<IActionResult> AddDocInSpec( int? docId, int? specId)
        {
            //Проверяемся на пустоту
            if (docId == 0 || docId == null || specId == 0 || specId == null)
            {
                return NotFound();
            }

            Specialization? spec = await db.Specializations.FirstOrDefaultAsync(p => p.SpecializationId == specId);
            if (spec == null)
            {
                return NotFound();
            }

            //качаем из бд доктора с данным id
            Doctor? doc = await db.Doctors.FirstOrDefaultAsync(d => d.Id == docId);
            if (doc == null)
            {
                return NotFound();
            }

            spec.Doctors.Add(doc);
            await db.SaveChangesAsync();

            return Redirect($"EditSpec?specId={specId}");
        }


        public async Task<IActionResult> RemoveDocInSpec(int? docId, int? specId)
        {
            if (docId == 0 || docId == null || specId == 0 || specId == null)
            {
                return NotFound();
            }

            Specialization? spec = await db.Specializations
                .Include(s => s.Doctors)
                .FirstOrDefaultAsync(s => s.SpecializationId == specId);

            if (spec == null)
            {
                return NotFound();
            }

            Doctor? doc = await db.Doctors
                .Include(d => d.Specializations)
                .FirstOrDefaultAsync(d => d.Id == docId);


            if (doc == null)
            {
                return NotFound();
            }

            spec.Doctors.Remove(doc);

            await db.SaveChangesAsync();

            return Redirect($"EditSpec?specId={specId}");
        }


        [HttpPost]
        public async Task<IActionResult> EditSpec(AdminPanelViewModel adminViewModel)
        {
                Specialization? specFromForm = adminViewModel.Specializations.FirstOrDefault();
                Specialization? specFromDb = await db.Specializations.FirstOrDefaultAsync(s => s.SpecializationId == specFromForm.SpecializationId);
                if (specFromDb == null || specFromDb == null || string.IsNullOrWhiteSpace(specFromForm.Title))
                {
                    return NotFound();
                }

                //Проверяем, чтобы не редактировали название поликлиники на существующее название специализации
                if (db.Specializations.Where(s => s.SpecializationId != specFromForm.SpecializationId).Any(s => s.Title == specFromForm.Title))
                {
                    return NotFound();
                }

                specFromDb.Title = specFromForm.Title;


                await db.SaveChangesAsync();

                return Redirect($"EditSpec?specId={specFromForm.SpecializationId}");


        }


        public async Task<IActionResult> RemoveSpec(int? specId)
        {
            if (specId == null || specId == 0)
            {
                return NotFound();
            }

            Specialization? spec = await db.Specializations.Include(s => s.Doctors)
                .FirstOrDefaultAsync(s => s.SpecializationId == specId);

            if (specId == null)
            {
                return NotFound();
            }

            foreach (var doc in spec.Doctors.ToList())
            {
                doc.Specializations.Remove(spec);
            }
            db.Specializations.Remove(spec);

            await db.SaveChangesAsync();

            return RedirectToAction("Specializations", "DataWorkerSpec");

        }


        [HttpGet]
        public IActionResult AddSpec()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> AddSpec(Specialization? spec)
        {
            if (spec == null || string.IsNullOrWhiteSpace(spec.Title) ||
                db.Specializations.Any(s => s.Title == spec.Title))
            {
                return NotFound();
            }

            await db.Specializations.AddAsync(spec);
            await db.SaveChangesAsync();

            return RedirectToAction("Specializations", "DataWorkerSpec");
        }
    }
}
