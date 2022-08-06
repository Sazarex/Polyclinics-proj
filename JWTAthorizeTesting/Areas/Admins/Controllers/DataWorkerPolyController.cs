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
        public IActionResult Polyclinics()
        {
            var polyclinics = _polyService.ChooseAll();

            PolyViewModel cityModel = new PolyViewModel()
            {
                Polyclinics = polyclinics
            };

            return View(cityModel);
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


            if (!_polyService.Update(polyModel))
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
        public async Task<IActionResult> AddPoly(PolyViewModel polyModel)
        {
            if (!_polyService.Add(polyModel))
            {
                return NotFound("Ошибка добавления. Возможно название поликлиники пустое или повторяется.");
            }

            return RedirectToAction("Polyclinics", "DataWorkerPoly");
        }


        public IActionResult SearchByTitle(PolyViewModel polyModel)
        {
            if (string.IsNullOrWhiteSpace(polyModel.Title))
            {
                return RedirectToAction("Polyclinics", "DataWorkerPoly");
            }

            polyModel.Polyclinics = _polyService.ChooseForSearch(polyModel.Title);

            if (polyModel.Polyclinics == null)
            {
                return RedirectToAction("Polyclinics", "DataWorkerPoly");
            }

            return View(polyModel);
        }
    }
}
