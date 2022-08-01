using JWTAthorizeTesting.Domain;
using JWTAthorizeTesting.Entities;
using JWTAthorizeTesting.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JWTAthorizeTesting.Controllers
{

    [Authorize(Roles = "Administrator")]
    public class DataWorkerPolyController : Controller
    {
        private AppDbContext db;
        public DataWorkerPolyController(AppDbContext _db)
        {
            db = _db;
        }


        /// <summary>
        /// Выводим вкладку с поликлиниками
        /// </summary>
        /// <returns></returns>
        public IActionResult Polyclinics()
        {
            //View Model с данными отправляем в представление
            AdminPanelViewModel adminModel = new AdminPanelViewModel()
            {
                Doctors = db.Doctors.ToList(),
                Specializations = db.Specializations.ToList(),
                Cities = db.Cities.ToList(),
                Experiences = db.Experiences.ToList(),
                Polyclinics = db.Polyclinics.Include(p => p.Doctors).ToList()
            };
            return View(adminModel);
        }


        /// <summary>
        /// Изменяем поликлинику
        /// </summary>
        /// <param name="polyId">Передается из представления</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> EditPoly(int? polyId)
        {
            if (polyId == null || polyId == 0)
            {
                return NotFound();
            }

            //Вытаскиваем из бд поликлиником по polyId и включаем в поликлинику докторов и города
            Polyclinic? poly = await db.Polyclinics.Include(p => p.Doctors)
                .Include(p => p.City)
                .FirstOrDefaultAsync(p => p.Id == polyId);
            if (poly == null)
            {
                return NotFound();
            }

            //Выбираем докторов, которых нет в данной поликлинике, но которых можно добавить
            List<Doctor> doctorsToAdd = db.Doctors.Where(d => !d.Polyclinics.Contains(poly)).ToList();

            //Создаем view model
            AdminPanelViewModel adminPanelViewModel = new AdminPanelViewModel();

            //В модель добавляем поликлинику
            adminPanelViewModel.Polyclinics.Add(poly);
            //В модель добавляем докторов, которых можно добавить в поликлинику
            adminPanelViewModel.Doctors.AddRange(doctorsToAdd);

            return View(adminPanelViewModel);
        }


        /// <summary>
        /// Добавляем доктора в поликлинику
        /// </summary>
        /// <param name="docId">из представления</param>
        /// <param name="polyId">из представления</param>
        /// <returns></returns>
        public async Task<IActionResult> AddDocInPoly(int? docId, int? polyId)
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

            return Redirect($"EditPoly?polyId={polyId}");
        }


        /// <summary>
        /// Удаляем врачей из поликлиники
        /// </summary>
        /// <param name="docId">из представления</param>
        /// <param name="polyId">из представления</param>
        /// <returns></returns>
        public async Task<IActionResult> RemoveDocInPoly(int? docId, int? polyId)
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

            return Redirect($"EditPoly?polyId={polyId}");
        }


        /// <summary>
        /// Редактирование поликлиники
        /// </summary>
        /// <param name="adminViewModel">из представления</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditPoly(AdminPanelViewModel adminViewModel)
        {
            //вытягиваем поликлинику из представления и из бд
            Polyclinic? polyFromForm = adminViewModel.Polyclinics.FirstOrDefault();
            Polyclinic? polyFromDb = await db.Polyclinics.FirstOrDefaultAsync(p => p.Id == polyFromForm.Id);
            if (polyFromDb == null || polyFromForm== null || string.IsNullOrWhiteSpace(polyFromForm.Title))
            {
                return NotFound();
            }

            //Проверяем, чтобы не редактировали название поликлиники на существующее название поликлиники
            if (db.Polyclinics.Where(p => p.Id != polyFromForm.Id).Any(p => p.Title == polyFromForm.Title))
            {
                return NotFound("Данная поликлиника уже существует");
            }

            polyFromDb.Title = polyFromForm.Title;
            polyFromDb.Phone = polyFromForm.Phone;
            polyFromDb.Adress = polyFromForm.Adress;

            //Если фото загружено, то загружаем его в wwwroot и в бд (через относительный пусть)
            if (adminViewModel.Photo != null)
            {
                var fileName = Path.GetFileName(adminViewModel.Photo.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\images", fileName);

                //Относительный путь к изображению в папке проекта
                var titleOfFileToDb = Path.Combine(@"\images\", fileName);
                polyFromDb.Photo = titleOfFileToDb;
                //Загрузка изображения в wwwroot
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await adminViewModel.Photo.CopyToAsync(fileStream);
                }
            }

            db.SaveChanges();

            return Redirect($"EditPoly?polyId={polyFromForm.Id}");
        }
        

        /// <summary>
        /// Удаление поликлиники
        /// </summary>
        /// <param name="polyId">из представления</param>
        /// <returns></returns>
        public async Task<IActionResult> RemovePoly(int? polyId)
        {
            if (polyId == null || polyId == 0)
            {
                return NotFound();
            }


            //Качаем из бд поликлинику по ИД и включаем в неё всех врачей
            Polyclinic? poly = await db.Polyclinics.Include(p => p.Doctors)
                .FirstOrDefaultAsync(p => p.Id == polyId);

            //Удаление фото
            if (poly.Photo != null)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot" + poly.Photo);

                FileInfo fileInfo = new FileInfo(filePath);
                fileInfo.Delete();
            }

            if (poly == null)
            {
                return NotFound();
            }

            //У врачей из этой поликлиники удаляем эту поликлинику
            foreach (var doc in poly.Doctors.ToList())
            {
                doc.Polyclinics.Remove(poly);
            }
            //Удаляем поликлинику
            db.Polyclinics.Remove(poly);

            await db.SaveChangesAsync();

            return RedirectToAction("Polyclinics", "DataWorkerPoly");

        }


        /// <summary>
        /// Показываем представление, отвечающее за добавление поликлиники
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult AddPoly()
        {
            return View();
        }


        /// <summary>
        /// Добавление поликлиники
        /// </summary>
        /// <param name="adminPanelView">из представления</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AddPoly(AdminPanelViewModel adminPanelView)
        {

            //Проверяем на пустое название и чтобы название в бд не повторялось
            if (string.IsNullOrWhiteSpace(adminPanelView.Polyclinics[0]?.Title) || db.Polyclinics.Any(p => p.Title == adminPanelView.Polyclinics[0].Title))
            {
                return NotFound("Проверьте название поликлиники, врзможно она уже существует, или название пустое.");
            }

            //поликлиника из модели
            Polyclinic polyFromForm = adminPanelView.Polyclinics.FirstOrDefault();
            //новая поликлиника
            Polyclinic newPoly = new Polyclinic();
            
            //в новую поликлинику добавляем записи из модели
            newPoly.Title = polyFromForm.Title;
            newPoly.Adress = polyFromForm.Adress;
            newPoly.Phone = polyFromForm.Phone;


            //Если фото загружено, то загружаем его в wwwroot и в бд (через относительный пусть)
            if (adminPanelView.Photo != null)
            {
                var fileName = Path.GetFileName(adminPanelView.Photo.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\images", fileName);

                //Относительный путь к изображению в папке проекта
                var titleOfFileToDb = Path.Combine(@"\images\", fileName);
                newPoly.Photo = titleOfFileToDb;
                //Загрузка изображения в wwwroot
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await adminPanelView.Photo.CopyToAsync(fileStream);
                }
            }

            await db.Polyclinics.AddAsync(newPoly);
            await db.SaveChangesAsync();

            return RedirectToAction("Polyclinics", "DataWorkerPoly");
        }
    }
}
