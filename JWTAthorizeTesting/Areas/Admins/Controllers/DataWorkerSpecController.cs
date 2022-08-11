using Microsoft.AspNetCore.Mvc;
using JWTAthorizeTesting.Domain;
using JWTAthorizeTesting.Entities;
using JWTAthorizeTesting.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JWTAthorizeTesting.Services.Interfaces;

namespace JWTAthorizeTesting.Areas.Admins.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class DataWorkerSpecController : Controller
    {
        readonly ISpecService _specService;

        private AppDbContext db;
        public DataWorkerSpecController(AppDbContext _db, ISpecService specService)
        {
            db = _db;
            _specService = specService;
        }

        public async Task<IActionResult> Specializations(int page=1)
        {
            HttpContext.Session.Clear();
            ViewBag.SearchedSpec = null;

            var source = _specService.ChooseAll();

            int pageSize = 3;
            var count = source.Count();
            var items = source.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            SpecViewModel specModel = new SpecViewModel();
            specModel.pageViewModel = new PageViewModel(count, page, pageSize);
            specModel.Specializations= items;

            return View(specModel);

        }


        [HttpGet]
        public async Task<IActionResult> EditSpec(int specId)
        {
            if (specId == 0)
            {
                return NotFound("Ошибка ID");
            }

            var spec = _specService.ChooseById(specId);

            if (spec == null)
            {
                return NotFound("Ошибка выбора специализации. Объект пуст. Возможно его не существует");
            }

            SpecViewModel specModel = new SpecViewModel();
            specModel.SpecializationId = spec.SpecializationId;
            specModel.Title = spec.Title;
            specModel.OtherDoctors = _specService.ChooseOtherDocs(specId);
            specModel.Doctors = spec.Doctors;

            return View(specModel);
        }


        public async Task<IActionResult> AddDocInSpec(int docId, int specId)
        {
            //Проверяемся на пустоту
            if (docId == 0 || specId == 0)
            {
                return NotFound("Ошибка ID.");
            }

            if (!_specService.AddDoc(docId, specId))
            {
                NotFound("Ошибка добавления специализации врачу.");
            }

            return Redirect($"EditSpec?specId={specId}");
        }


        public async Task<IActionResult> RemoveDocInSpec(int docId, int specId)
        {
            if (docId == 0 || specId == 0)
            {
                return NotFound("Ошибка ID.");
            }

            if (!_specService.RemoveDoc(docId, specId))
            {
                NotFound("Ошибка удаления специализации у врача.");
            }

            return Redirect($"EditSpec?specId={specId}");
        }


        [HttpPost]
        public async Task<IActionResult> EditSpec(SpecViewModel specModel)
        {
            var spec = new Specialization();
            spec.SpecializationId = specModel.SpecializationId;
            spec.Title = specModel.Title;
            

            if (!_specService.Update(spec))
            {
                return NotFound("Ошибка редактирования. Возможно поле с названием пусто или такая специализация повторяется");
            }

            return Redirect($"EditSpec?specId={specModel.SpecializationId}");


        }


        public async Task<IActionResult> RemoveSpec(int specId)
        {
            if (specId == 0)
            {
                return NotFound("Ошибка ID.");
            }

            if (!_specService.Remove(specId))
            {
                return NotFound("Ошибка удаления специализации.");
            }

            return RedirectToAction("SearchByTitle", "DataWorkerSpec");

        }


        [HttpGet]
        public IActionResult AddSpec()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> AddSpec(SpecViewModel specModel)
        {

            var spec = new Specialization();
            spec.SpecializationId = specModel.SpecializationId;
            spec.Title = specModel.Title;

            if (!_specService.Add(spec))
            {
                return NotFound("Ошибка добавления специализации. Возможно поле с название пустое или такая специализация уже есть");
            }

            return RedirectToAction("Specializations", "DataWorkerSpec");
        }

        public IActionResult SearchByTitle(SpecViewModel specModel,int page=1)
        {
            //Проверяю на пустоту название в моделе и пустоту сессии
            if (string.IsNullOrWhiteSpace(specModel.Title) && !HttpContext.Session.Keys.Contains("searchSpec"))
            {
                return RedirectToAction("Specializations", "DataWorkerSpec");
            }

            //Если название из модели не пустое
            if (!string.IsNullOrWhiteSpace(specModel.Title))
            {
                specModel.Specializations = _specService.ChooseForSearch(specModel.Title);
            }
            //Если сессия содержит ключ
            if (HttpContext.Session.Keys.Contains("searchSpec"))
            {
                if (!string.IsNullOrWhiteSpace(specModel.Title))
                {
                    HttpContext.Session.SetString("searchSpec", specModel.Title);
                }

                //Выбираем для поиска по значению сессии
                specModel.Specializations= _specService.ChooseForSearch(HttpContext.Session.GetString("searchSpec"));
                ViewBag.SearchedSpec = HttpContext.Session.GetString("searchSpec");
            }
            //Если в сессии нет ключа 
            if (!HttpContext.Session.Keys.Contains("searchSpec"))
            {
                //Назначаем сессии значение из значения поиска
                HttpContext.Session.SetString("searchSpec", specModel.Title);
                ViewBag.SearchedSpec = HttpContext.Session.GetString("searchSpec");
            }

            //Пагинация
            int pageSize = 3;
            var count = specModel.Specializations.Count();
            var items = specModel.Specializations.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            //В инкапсулированную модель нашей модель добавляем значения через конструктор
            specModel.pageViewModel = new PageViewModel(count, page, pageSize);
            specModel.Specializations = items;

            return View(specModel);
        }

        //Очистка поиска, чистим сессию и редирект на страницу городов
        public IActionResult ClearSearch()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Specializations", "DataWorkerSpec");
        }
    }
}
