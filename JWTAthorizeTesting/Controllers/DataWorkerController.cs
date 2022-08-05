using JWTAthorizeTesting.Domain;
using JWTAthorizeTesting.Entities;
using JWTAthorizeTesting.Models;
using JWTAthorizeTesting.Models.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JWTAthorizeTesting.Controllers
{

    [Authorize(Roles = "Administrator")]
    public class DataWorkerController: Controller
    {
        readonly ICityService _cityService;

        private AppDbContext db;
        public DataWorkerController(AppDbContext _db, ICityService cityService)
        {
            db = _db;
            _cityService = cityService;
        }


        /// <summary>
        /// Удаление поликлиники из города
        /// </summary>
        /// <param name="polyId"></param>
        /// <returns></returns>
        public IActionResult RemovePolyInCity(int? polyId,int? cityId)
        {
            //По polyId из представления ищем поликлинику из дбконтекста и назначаем внешн ключу
            //в таблице поликлиники значение null
            var poly = db.Polyclinics.FirstOrDefault(p => p.Id == polyId);
            if (poly == null)
            {
                return NotFound();
            }

            poly.CityId = null;


            db.SaveChanges();
            return Redirect($"EditCity?CityId={cityId}");

        }


        /// <summary>
        /// Отправляем HttpGet-запросом View Model.
        /// </summary>
        /// <param name="cityId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> EditCity(int cityId)
        {
            if (cityId != null)
            {
                #region comment

                //   //Находим город по переданному cityId из представления
                //    City? city = await db.Cities.Include(c => c.Polyclinics)
                //        .FirstOrDefaultAsync(c => c.CityId == cityId);
                //    if (city != null)
                //    {
                //        //Создаем нашу view model
                //        AdminPanelViewModel adminModel = new AdminPanelViewModel();
                //        //Добавляем в неё найденный ранее город
                //        adminModel.Cities.Add(city);
                //        //Создаем список поликлиник для добавления в город, которые не присутствуют в этом городе
                //        List<Polyclinic> polyclinicsForAdding = await db.Polyclinics.Where(p => p.CityId != city.CityId).ToListAsync();
                //        //Добавляем этот список в нашу view model
                //        adminModel.Polyclinics.AddRange(polyclinicsForAdding);
                //        return View(adminModel);
                //    }
                #endregion

                City? city = _cityService.ChooseById(cityId);

                if (city == null)
                {
                    return NotFound("Неверный ID города, или города несуществует.");
                }

                var polyclinicsToAdd = _cityService.ChooseOtherPolyInCity(cityId);

                CityViewModel cityModel = new CityViewModel()
                {
                    CityId = cityId,
                    Title = city.Title,
                    OtherPolyclinics = polyclinicsToAdd,
                    PolyclinicsOfCity = city.Polyclinics.ToList()
                };

                return View(cityModel);
            }
            return NotFound();
        }


        /// <summary>
        /// Редактирование города
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> EditCity(CityViewModel cityModel)
        {
            if (!ModelState.IsValid)
            {
                return NotFound("Проверьте поля.");
            }

                if (!_cityService.Update(cityModel))
                {
                    return NotFound("Ошибка обновления.");
                }

                return Redirect($"EditCity?CityId={cityModel.CityId}");
            
        }


        /// <summary>
        /// Добавление поликлиники в город
        /// </summary>
        /// <param name="cityId">Передается из представления через тэг хелпер</param>
        /// <param name="polyId">Передается из представления через тэг хелпер</param>
        /// <returns></returns>
        public async Task<IActionResult> AddPolyInCity(int cityId,int polyId)
        {
           if (!_cityService.AddPolyclinic(cityId,polyId))
            {
                return NotFound("Ошибка добавления поликлиники в город");
            }

            return Redirect($"EditCity?CityId={cityId}");
        }


        /// <summary>
        /// Удаляем город из списка городов
        /// </summary>
        /// <param name="CityId">ID города, передается через представление</param>
        /// <returns></returns>
        public async Task<IActionResult> RemoveCity(int CityId)
        {
            if (!_cityService.Remove(CityId))
            {
                return NotFound("Ошибка удаления города.");
            }

            return RedirectToAction("AdminPanel", "Auth");

        }


        /// <summary>
        /// Добавление города, выводим представление добавления
        /// </summary>
        /// <returns></returns>
        public IActionResult AddCity()
        {
            return View();
        }


        /// <summary>
        /// Добавление города
        /// </summary>
        /// <param name="city">Город, передается через представление моделью</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AddCity(CityViewModel cityModel)
        {
            //Проверяем на валидность, чтоб поле было заполнено
            if (!ModelState.IsValid)
            {
                return View(cityModel);
            }

            if (!_cityService.Add(cityModel))
            {
                return NotFound("Ошибка добавления. Возможно такой город уже существует.");
            }

            return RedirectToAction("AdminPanel", "Auth");
        }


    }
}
