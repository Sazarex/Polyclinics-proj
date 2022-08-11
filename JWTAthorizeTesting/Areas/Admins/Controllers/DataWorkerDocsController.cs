using JWTAthorizeTesting.Domain;
using JWTAthorizeTesting.Models;
using JWTAthorizeTesting.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using JWTAthorizeTesting.Services.Interfaces;

namespace JWTAthorizeTesting.Areas.Admins.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class DataWorkerDocsController : Controller
    {
        readonly IDocService _docService;
        private AppDbContext db;
        public DataWorkerDocsController(AppDbContext _db, IDocService docService)
        {
            db = _db;
            _docService = docService;
        }

        /// <summary>
        /// Выводим всех врачей в таблицу в админ панеле
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Doctors(int page = 1)
        {
            HttpContext.Session.Clear();
            ViewBag.SearchedDoc = null;

            var source = _docService.ChooseAll();

            int pageSize = 3;
            var count = source.Count();
            var items = source.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            DocViewModel docModel = new DocViewModel();
            docModel.pageViewModel = new PageViewModel(count, page, pageSize);
            docModel.Doctors = items;

            return View(docModel);
        }


        /// <summary>
        /// Редактируем определенного врача
        /// </summary>
        /// <param name="docId">передается через тег хелпер в представлении</param>
        /// <returns></returns>
        public async Task<IActionResult> EditDoc(int docId)
        {

            if (docId == 0)
            {
                return NotFound("Ошибка. Неверный ID.");
            }

            var doc = _docService.ChooseById(docId);

            if (doc == null)
            {
                return NotFound("Ошибка определения доктора. Объект пуст.");
            }

            DocViewModel docModel = new DocViewModel()
            {
                Id = doc.Id,
                FIO = doc.FIO,
                FullDesc = doc.FullDesc,
                ShortDesc = doc.ShortDesc,
                Phone = doc.Phone,
                Photo = doc.Photo,
                Price = doc.Price,
                OtherPolyclinics = _docService.ChooseOtherPoly(docId),
                OtherSpecializations = _docService.ChooseOtherSpec(docId),
                Polyclinics = doc.Polyclinics,
                Specializations = doc.Specializations
            };

            return View(docModel);
        }


        [HttpPost]
        public async Task<IActionResult> EditDoc(DocViewModel docModel)
        {

            var doc = new Doctor();
            doc.Id = docModel.Id;
            doc.FIO = docModel.FIO;
            doc.FullDesc = docModel.FullDesc;
            doc.ShortDesc = docModel.ShortDesc;
            doc.Price = docModel.Price;
            doc.Phone = docModel.Phone;

            if (!_docService.Update(doc,docModel.PhotoToUpload))
            {
                return NotFound("Ошибка редактирования");
            }

            return Redirect($"EditDoc?docId={docModel.Id}");
        }


        public async Task<IActionResult> AddDocInPoly(int polyId, int docId)
        {
            //Проверяемся на пустоту
            if (docId == 0 || docId == null || polyId == 0 || polyId == null)
            {
                return NotFound("Ошибка ID.");
            }

            if (!_docService.AddPoly(docId, polyId))
            {
                return NotFound("Ошибка добавления доктора в поликлинику.");
            }

            return Redirect($"EditDoc?docId={docId}");
        }


        public async Task<IActionResult> RemoveDocInPoly(int polyId, int docId)
        {

            //Проверяемся на пустоту
            if (docId == 0 || polyId == 0)
            {
                return NotFound("Ошибка ID.");
            }

            if (!_docService.RemovePoly(docId, polyId))
            {
                return NotFound("Ошибка удаления доктора в поликлинике");
            }

            return Redirect($"EditDoc?docId={docId}");
        }


        public async Task<IActionResult> RemoveDoc(int docId)
        {
            if (docId == 0)
            {
                return NotFound("Ошибка ID");
            }

            if (!_docService.Remove(docId))
            {
                return NotFound("Ошибка удаления врача.");
            }

            return RedirectToAction("SearchByTitle", "DataWorkerDocs");

        }


        [HttpGet]
        public IActionResult AddDoc()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> AddDoc(DocViewModel docModel)
        {
            if (docModel == null)
            {
                return NotFound("Ошибка добавления врача. Пустая модель.");
            }

            var doc = new Doctor();
            doc.Id = docModel.Id;
            doc.FIO = docModel.FIO;
            doc.FullDesc = docModel.FullDesc;
            doc.ShortDesc = docModel.ShortDesc;
            doc.Price = docModel.Price;
            doc.Phone = docModel.Phone;

            if (!_docService.Add(doc,docModel.PhotoToUpload))
            {
                return NotFound("Ошибка добавления врача. Возможно поле ФИО пустое или такой врач уже есть.");
            }

            return RedirectToAction("Doctors", "DataWorkerDocs");
        }


        public async Task<IActionResult> AddSpecToDoc(int specId, int docId)
        {
            if (specId == 0 || docId == 0)
            {
                return NotFound("Ошибка ID.");
            }

            if (!_docService.AddSpec(docId, specId))
            {
                return NotFound("Ошибка добавления специализации доктору.");
            }

            return Redirect($"EditDoc?docId={docId}");
        }


        public async Task<IActionResult> RemoveSpecInDoc(int specId, int docId)
        {
            if (specId == 0 || docId == 0)
            {
                return NotFound();
            }

            if (!_docService.RemoveSpec(docId, specId))
            {
                return NotFound("Ошибка удаления специализации у врача.");
            }
            return Redirect($"EditDoc?docId={docId}");

        }

        public IActionResult SearchByTitle(DocViewModel docModel, int page=1)
        {
            //Проверяю на пустоту название в моделе и пустоту сессии
            if (string.IsNullOrWhiteSpace(docModel.FIO) && !HttpContext.Session.Keys.Contains("searchDoc"))
            {
                return RedirectToAction("Doctors", "DataWorkerDocs");
            }

            //Если название из модели не пустое
            if (!string.IsNullOrWhiteSpace(docModel.FIO))
            {
                docModel.Doctors = _docService.ChooseForSearch(docModel.FIO);
            }
            //Если сессия содержит ключ
            if (HttpContext.Session.Keys.Contains("searchDoc"))
            {
                if (!string.IsNullOrWhiteSpace(docModel.FIO))
                {
                    HttpContext.Session.SetString("searchDoc", docModel.FIO);
                }

                //Выбираем для поиска по значению сессии
                docModel.Doctors= _docService.ChooseForSearch(HttpContext.Session.GetString("searchDoc"));
                ViewBag.SearchedDoc = HttpContext.Session.GetString("searchDoc");
            }
            //Если в сессии нет ключа 
            if (!HttpContext.Session.Keys.Contains("searchDoc"))
            {
                //Назначаем сессии значение из значения поиска
                HttpContext.Session.SetString("searchDoc", docModel.FIO);
                ViewBag.SearchedDoc = HttpContext.Session.GetString("searchDoc");
            }

            //Пагинация
            int pageSize = 3;
            var count = docModel.Doctors.Count();
            var items = docModel.Doctors.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            //В инкапсулированную модель нашей модель добавляем значения через конструктор
            docModel.pageViewModel = new PageViewModel(count, page, pageSize);
            docModel.Doctors = items;

            return View(docModel);
        }

        //Очистка поиска, чистим сессию и редирект на страницу городов
        public IActionResult ClearSearch()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Doctors", "DataWorkerDocs");

        }
    }
}
