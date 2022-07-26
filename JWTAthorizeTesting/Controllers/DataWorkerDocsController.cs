using JWTAthorizeTesting.Domain;
using JWTAthorizeTesting.Models;
using JWTAthorizeTesting.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace JWTAthorizeTesting.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class DataWorkerDocsController: Controller
    {
        private AppDbContext db;
        public DataWorkerDocsController(AppDbContext _db)
        {
            db = _db;
        }


        public async Task<IActionResult> Doctors()
        {
            AdminPanelViewModel adminPanelModel = new AdminPanelViewModel();
            adminPanelModel.Doctors = await db.Doctors
                .Include(d => d.Polyclinics)
                .Include(d => d.Specializations)
                .Include(d => d.Experience)
                .ToListAsync();

            return View(adminPanelModel);
        }


        public async Task<IActionResult> EditDoc(int? docId)
        {

            if (docId == null || docId == 0)
            {
                return NotFound();
            }

            Doctor? doc = await db.Doctors.Include(d => d.Specializations)
                .Include(d => d.Experience)
                .Include(d => d.Polyclinics)
                .FirstOrDefaultAsync(d => d.Id == docId);

            if (doc == null)
            {
                return NotFound();
            }

            List<Polyclinic> polyToAdd = db.Polyclinics.Where(p => !p.Doctors.Contains(doc)).ToList();
            List<Specialization> specToAdd = db.Specializations.Where(s => !s.Doctors.Contains(doc)).ToList();

            //Создаем view model
            AdminPanelViewModel adminPanelViewModel = new AdminPanelViewModel();

            adminPanelViewModel.Doctors.Add(doc);
            adminPanelViewModel.Specializations.AddRange(specToAdd);
            adminPanelViewModel.Polyclinics.AddRange(polyToAdd);

            return View(adminPanelViewModel);
        }


        [HttpPost]
        public async Task<IActionResult> EditDoc(AdminPanelViewModel adminViewModel)
        {
            Doctor? docFromForm = adminViewModel.Doctors.FirstOrDefault();
            Doctor? docFromDb = await db.Doctors.FirstOrDefaultAsync(d => d.Id == docFromForm.Id);
            if (docFromDb == null || docFromForm == null || string.IsNullOrWhiteSpace(docFromForm.FIO))
            {
                return NotFound();
            }


            if (db.Doctors.Where(d => d.Id != docFromForm.Id).Any(d => d.FIO == docFromForm.FIO))
            {
                return NotFound();
            }

            docFromDb.FIO = docFromForm.FIO;
            docFromDb.Phone = docFromForm.Phone;
            docFromDb.Price = docFromForm.Price;
            docFromDb.ShortDesc = docFromForm.ShortDesc;
            docFromDb.FullDesc= docFromForm.FullDesc;

            //Если фото загружено, то загружаем его в wwwroot и в бд (через относительный пусть)
            if (adminViewModel.Photo != null)
            {
                var fileName = Path.GetFileName(adminViewModel.Photo.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\images", fileName);

                //Относительный путь к изображению в папке проекта
                var titleOfFileToDb = Path.Combine(@"\images\", fileName);
                docFromDb.Photo = titleOfFileToDb;
                //Загрузка изображения в wwwroot
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await adminViewModel.Photo.CopyToAsync(fileStream);
                }
            }

            db.SaveChanges();

            return Redirect($"EditDoc?docId={docFromForm.Id}");
        }


        public async Task<IActionResult> AddDocInPoly(int? polyId, int? docId)
        {
            //Проверяемся на пустоту
            if (docId == 0 || docId == null || polyId == 0 || polyId == null)
            {
                return NotFound();
            }

            //качаем из бд поликлинику с данным id
            Polyclinic? polyclinic = await db.Polyclinics.FirstOrDefaultAsync(p => p.Id == polyId);
            if (polyclinic == null)
            {
                return NotFound();
            }

            //качаем из бд доктора с данным id
            Doctor? doc = await db.Doctors.FirstOrDefaultAsync(d => d.Id == docId);
            if (doc == null)
            {
                return NotFound();
            }

            //В поликлинику добавляем врача
            polyclinic.Doctors.Add(doc);
            await db.SaveChangesAsync();

            return Redirect($"EditDoc?docId={docId}");
        }


        public async Task<IActionResult> RemoveDocInPoly(int? polyId, int? docId)
        {

            //Проверяемся на пустоту
            if (docId == 0 || docId == null || polyId == 0 || polyId == null)
            {
                return NotFound();
            }
            //Достаем из бд врача по ид
            Doctor? doc = await db.Doctors.FirstOrDefaultAsync(d => d.Id == docId);
            if (doc == null)
            {
                return NotFound();
            }
            //достаем из бд поликлинику по ид и включаем в linq-запрос всех  врачей
            Polyclinic? poly = await db.Polyclinics.Include(p => p.Doctors)
                .FirstOrDefaultAsync(p => p.Id == polyId);
            if (poly == null)
            {
                return NotFound();
            }

            //удаляем врача
            poly.Doctors.Remove(doc);
            await db.SaveChangesAsync();

            return Redirect($"EditDoc?docId={docId}");
        }

        public async Task<IActionResult> RemoveDoc(int? docId)
        {
            if (docId == null || docId == 0)
            {
                return NotFound();
            }

            //Качаем из бд поликлинику по ИД и включаем в неё всех врачей
            Doctor? doc = await db.Doctors.Include(d => d.Polyclinics)
                .FirstOrDefaultAsync(p => p.Id == docId);

            if (doc == null)
            {
                return NotFound();
            }

            //У врачей из этой поликлиники удаляем эту поликлинику
            foreach (var poly in doc.Polyclinics.ToList())
            {
                poly.Doctors.Remove(doc);
            }
            //Удаляем поликлинику
            db.Doctors.Remove(doc);

            await db.SaveChangesAsync();

            return RedirectToAction("Doctors", "DataWorkerDocs");

        }


        [HttpGet]
        public IActionResult AddDoc()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddDoc(AdminPanelViewModel adminPanelView)
        {
            //Проверяем на пустое название и чтобы название в бд не повторялось
            if (string.IsNullOrWhiteSpace(adminPanelView.Doctors[0]?.FIO) || db.Doctors.Any(d => d.FIO == adminPanelView.Doctors[0].FIO))
            {
                return NotFound();
            }

            Doctor docFromForm = adminPanelView.Doctors.FirstOrDefault();
            Doctor newDoc = new Doctor();

            //в новую поликлинику добавляем записи из модели
            newDoc.FIO = docFromForm.FIO;
            newDoc.Price = docFromForm.Price;
            newDoc.Phone = docFromForm.Phone;
            newDoc.ShortDesc = docFromForm.ShortDesc;
            newDoc.FullDesc = docFromForm.FullDesc;


            //Если фото загружено, то загружаем его в wwwroot и в бд (через относительный пусть)
            if (adminPanelView.Photo != null)
            {
                var fileName = Path.GetFileName(adminPanelView.Photo.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\images", fileName);

                //Относительный путь к изображению в папке проекта
                var titleOfFileToDb = Path.Combine(@"\images\", fileName);
                newDoc.Photo = titleOfFileToDb;
                //Загрузка изображения в wwwroot
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await adminPanelView.Photo.CopyToAsync(fileStream);
                }
            }

            await db.Doctors.AddAsync(newDoc);
            await db.SaveChangesAsync();

            return RedirectToAction("Doctors", "DataWorkerDocs");
        }


        public async Task<IActionResult> AddSpecToDoc(int? specId, int? docId)
        {
            if (specId == 0 || docId == 0)
            {
                return NotFound();
            }

            Specialization? spec = await db.Specializations.FirstOrDefaultAsync(s => s.SpecializationId == specId);
            if (spec == null)
            {
                return NotFound();
            }

            Doctor? doc = await db.Doctors.FirstOrDefaultAsync(d => d.Id == docId);
            if (doc == null)
            {
                return NotFound();
            }

            doc.Specializations.Add(spec);
            await db.SaveChangesAsync();
            return Redirect($"EditDoc?docId={docId}");
        }


        public async Task<IActionResult> RemoveSpecInDoc(int? specId, int? docId)
        {
            if (specId == 0 || docId == 0)
            {
                return NotFound();
            }

            Specialization? spec = await db.Specializations.FirstOrDefaultAsync(s => s.SpecializationId == specId);
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

            doc.Specializations.Remove(spec);

            await db.SaveChangesAsync();



            return Redirect($"EditDoc?docId={docId}");

        }
    }
}
