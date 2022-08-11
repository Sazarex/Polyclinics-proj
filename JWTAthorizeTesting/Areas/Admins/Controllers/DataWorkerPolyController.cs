using JWTAthorizeTesting.Domain;
using JWTAthorizeTesting.Entities;
using JWTAthorizeTesting.Models;
using JWTAthorizeTesting.Services.Interfaces;
using JWTAthorizeTesting.Services.ServiceClasses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JWTAthorizeTesting.Areas.Admins.Controllers
{

    [Authorize(Roles = "Administrator")]
    public class DataWorkerPolyController : Controller
    {
        readonly IPolyService _polyService;
        private AppDbContext db;
        public DataWorkerPolyController(AppDbContext _db, IPolyService polyService)
        {
            db = _db;
            _polyService = polyService;
        }


        /// <summary>
        /// Выводим вкладку с поликлиниками
        /// </summary>
        /// <returns></returns>
        public IActionResult Polyclinics(int page=1)
        {

            HttpContext.Session.Clear();
            ViewBag.SearchedPoly = null;

            var source = _polyService.ChooseAll();

            int pageSize = 3;
            var count = source.Count();
            var items = source.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            PolyViewModel polyModel = new PolyViewModel();
            polyModel.pageViewModel = new PageViewModel(count, page, pageSize);
            polyModel.Polyclinics = items;

            return View(polyModel);

        }


        /// <summary>
        /// Изменяем поликлинику
        /// </summary>
        /// <param name="polyId">Передается из представления</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> EditPoly(int polyId)
        {
            if (polyId == null || polyId == 0)
            {
                return NotFound("Неверный ID");
            }

            var poly = _polyService.ChooseById(polyId);

            if (poly == null)
            {
                return NotFound("Ошибка. Возможно такого объекта не существует.");
            }

            PolyViewModel polyModel = new PolyViewModel()
            {
                Id = poly.Id,
                Title = poly.Title,
                Adress = poly.Adress,
                Phone = poly.Phone,
                Photo = poly.Photo,
                City = poly.City,
                Doctors = poly.Doctors,
                CityId = poly.CityId,
                OtherDoctors = _polyService.ChooseOtherDocsInPoly(polyId)
            };

            return View(polyModel);
        }


        /// <summary>
        /// Добавляем доктора в поликлинику
        /// </summary>
        /// <param name="docId">из представления</param>
        /// <param name="polyId">из представления</param>
        /// <returns></returns>
        public async Task<IActionResult> AddDocInPoly(int docId, int polyId)
        {
            //Проверяемся на пустоту
            if (docId == 0 || docId == null || polyId == 0 || polyId == null)
            {
                return NotFound("Ошибка добавления доктора в поликлинику. Проверьте ID.");
            }

            if (!_polyService.AddDoc(docId, polyId))
            {
                return NotFound("Ошибка добавления доктора в поликлинику.");
            }

            return Redirect($"EditPoly?polyId={polyId}");
        }


        /// <summary>
        /// Удаляем врачей из поликлиники
        /// </summary>
        /// <param name="docId">из представления</param>
        /// <param name="polyId">из представления</param>
        /// <returns></returns>
        public async Task<IActionResult> RemoveDocInPoly(int docId, int polyId)
        {

            //Проверяемся на пустоту
            if (docId == 0 || docId == null || polyId == 0 || polyId == null)
            {
                return NotFound("Неверные ID врача или (и) поликлиники");
            }

            if (!_polyService.RemoveDoc(docId, polyId))
            {
                return NotFound("Ошибка удаления доктора из поликлиники.");
            }

            return Redirect($"EditPoly?polyId={polyId}");
        }


        /// <summary>
        /// Редактирование поликлиники
        /// </summary>
        /// <param name="adminViewModel">из представления</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditPoly(PolyViewModel polyModel)
        {
            if (polyModel == null || string.IsNullOrWhiteSpace(polyModel.Title))
            {
                return NotFound("Ошибка редактирования. Возможно пустое название поликлиники.");
            }

            var polyclinic = new Polyclinic();
            polyclinic.Id = polyModel.Id;
            polyclinic.Title = polyModel.Title;
            polyclinic.Phone = polyModel.Phone;
            polyclinic.Adress = polyModel.Adress;
            

            if (!_polyService.Update(polyclinic,polyModel.PhotoToUpload))
            {
                return NotFound("Ошибка редактирования. Возможно пустое название поликлиники.");
            }

            return Redirect($"EditPoly?polyId={polyModel.Id}");
        }


        /// <summary>
        /// Удаление поликлиники
        /// </summary>
        /// <param name="polyId">из представления</param>
        /// <returns></returns>
        public async Task<IActionResult> RemovePoly(int polyId)
        {
            if (polyId == null || polyId == 0)
            {
                return NotFound();
            }
            if (!_polyService.Remove(polyId))
            {
                return NotFound("Ошибка удаления");
            }

            return RedirectToAction("SearchByTitle", "DataWorkerPoly");

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
        public async Task<IActionResult> AddPoly(PolyViewModel polyModel)
        {

            var polyclinic = new Polyclinic();
            polyclinic.Id = polyModel.Id;
            polyclinic.Title = polyModel.Title;
            polyclinic.Phone = polyModel.Phone;
            polyclinic.Adress = polyModel.Adress;

            if (!_polyService.Add(polyclinic,polyModel.PhotoToUpload))
            {
                return NotFound("Ошибка добавления. Возможно название поликлиники пустое или повторяется.");
            }

            return RedirectToAction("Polyclinics", "DataWorkerPoly");
        }


        public IActionResult SearchByTitle(PolyViewModel polyModel, int page=1)
        {
            //Проверяю на пустоту название в моделе и пустоту сессии
            if (string.IsNullOrWhiteSpace(polyModel.Title) && !HttpContext.Session.Keys.Contains("searchPoly"))
            {
                return RedirectToAction("Polyclinics", "DataWorkerPoly");
            }

            //Если название из модели не пустое
            if (!string.IsNullOrWhiteSpace(polyModel.Title))
            {
                polyModel.Polyclinics= _polyService.ChooseForSearch(polyModel.Title);
            }
            //Если сессия содержит ключ
            if (HttpContext.Session.Keys.Contains("searchPoly"))
            {
                if (!string.IsNullOrWhiteSpace(polyModel.Title))
                {
                    HttpContext.Session.SetString("searchPoly", polyModel.Title);
                }

                //Выбираем для поиска по значению сессии
                polyModel.Polyclinics = _polyService.ChooseForSearch(HttpContext.Session.GetString("searchPoly"));
                ViewBag.SearchedPoly = HttpContext.Session.GetString("searchPoly");
            }
            //Если в сессии нет ключа 
            if (!HttpContext.Session.Keys.Contains("searchPoly"))
            {
                //Назначаем сессии значение из значения поиска
                HttpContext.Session.SetString("searchPoly", polyModel.Title);
                ViewBag.SearchedPoly = HttpContext.Session.GetString("searchPoly");
            }

            //Пагинация
            int pageSize = 3;
            var count = polyModel.Polyclinics.Count();
            var items = polyModel.Polyclinics.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            //В инкапсулированную модель нашей модель добавляем значения через конструктор
            polyModel.pageViewModel = new PageViewModel(count, page, pageSize);
            polyModel.Polyclinics = items;

            return View(polyModel);
        }


        //Очистка поиска, чистим сессию и редирект на страницу городов
        public IActionResult ClearSearch()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Polyclinics", "DataWorkerPoly");

        }
    }
}
