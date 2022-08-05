using JWTAthorizeTesting.Domain;
using JWTAthorizeTesting.Models;
using JWTAthorizeTesting.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using JWTAthorizeTesting.Services.Interfaces;

namespace JWTAthorizeTesting.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class DataWorkerDocsController: Controller
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
        public async Task<IActionResult> Doctors()
        {
            DocViewModel docModel = new DocViewModel();
            docModel.Doctors = _docService.ChooseAll();

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
            if (!_docService.Update(docModel))
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

            if (!_docService.AddPoly(docId,polyId))
            {
                return NotFound("Ошибка добавления доктора в поликлинику.");
            }

            return Redirect($"EditDoc?docId={docId}");
        }


        public async Task<IActionResult> RemoveDocInPoly(int polyId, int docId)
        {

            //Проверяемся на пустоту
            if (docId == 0 || polyId == 0 )
            {
                return NotFound("Ошибка ID.");
            }

            if (!_docService.RemovePoly(docId,polyId))
            {
                return NotFound("Ошибка удаления доктора в поликлинике");
            }

            return Redirect($"EditDoc?docId={docId}");
        }


        public async Task<IActionResult> RemoveDoc(int docId)
        {
            if ( docId == 0)
            {
                return NotFound("Ошибка ID");
            }

            if (!_docService.Remove(docId))
            {
                return NotFound("Ошибка удаления врача.");
            }

            return RedirectToAction("Doctors", "DataWorkerDocs");

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

            if (!_docService.Add(docModel))
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

            if (!_docService.AddSpec(docId,specId))
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
    }
}
