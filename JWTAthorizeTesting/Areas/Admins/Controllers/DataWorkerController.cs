using JWTAthorizeTesting.Domain;
using JWTAthorizeTesting.Entities;
using JWTAthorizeTesting.Models;
using JWTAthorizeTesting.Models.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JWTAthorizeTesting.Areas.Admins.Controllers
{

    [Authorize(Roles = "Administrator")]
    public class DataWorkerController : Controller
    {
        readonly ICityService _cityService;

        private AppDbContext db;
        public DataWorkerController(AppDbContext _db, ICityService cityService)
        {
            db = _db;
            _cityService = cityService;
        }

        public IActionResult SearchByTitle(CityViewModel cityModel, int page=1)
        {
            //Проверяю на пустоту название в моделе и пустоту сессии
            if (string.IsNullOrWhiteSpace(cityModel.Title) && !HttpContext.Session.Keys.Contains("searchCity"))
            {
                return RedirectToAction("AdminPanel", "DataWorker");
            }


            //Если название из модели не пустое
            if (!string.IsNullOrWhiteSpace(cityModel.Title))
            {
                cityModel.Cities = _cityService.ChooseForSearch(cityModel.Title);
            }
            //Если сессия содержит ключ
            if (HttpContext.Session.Keys.Contains("searchCity"))
            {
                if (!string.IsNullOrWhiteSpace(cityModel.Title))
                {
                    HttpContext.Session.SetString("searchCity", cityModel.Title);
                }

                //Выбираем для поиска по значению сессии
                cityModel.Cities = _cityService.ChooseForSearch(HttpContext.Session.GetString("searchCity"));
                ViewBag.SearchedCity = HttpContext.Session.GetString("searchCity");
            }
            //Если в сессии нет ключа 
            if (!HttpContext.Session.Keys.Contains("searchCity"))
            {
                //Назначаем сессии значение из значения поиска
                HttpContext.Session.SetString("searchCity", cityModel.Title);
                ViewBag.SearchedCity = HttpContext.Session.GetString("searchCity");
            }

            //Пагинация
            int pageSize = 3;
            var count = cityModel.Cities.Count();
            var items = cityModel.Cities.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            //В инкапсулированную модель нашей модель добавляем значения через конструктор
            cityModel.pageViewModel = new PageViewModel(count, page, pageSize);
            cityModel.Cities = items;

            return View(cityModel);
        }


        /// <summary>
        /// Открывается админ панель на странице с городами
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult AdminPanel(int page=1)
        {
            HttpContext.Session.Clear();

            var source = _cityService.ChooseAll();

            int pageSize = 3;
            var count = source.Count();
            var items = source.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            CityViewModel cityModel = new CityViewModel();
            cityModel.pageViewModel = new PageViewModel(count, page, pageSize);
            cityModel.Cities = items;

            return View(cityModel);

        }


        /// <summary>
        /// Удаление поликлиники из города
        /// </summary>
        /// <param name="polyId"></param>
        /// <returns></returns>
        public IActionResult RemovePolyInCity(int polyId, int cityId)
        {
            //По polyId из представления ищем поликлинику из дбконтекста и назначаем внешн ключу
            //в таблице поликлиники значение null
            if (polyId == 0 || cityId == 0)
            {
                return NotFound("Ошибка ID.");
            }

            if (!_cityService.RemovePolyclinic(cityId,polyId))
            {
                return NotFound("Ошибка удаления поликлиники в городе");
            }


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


            var city = new City();
            city.CityId = cityModel.CityId;
            city.Title = cityModel.Title;
            

            if (!_cityService.Update(city))
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
        public async Task<IActionResult> AddPolyInCity(int cityId, int polyId)
        {
            if (!_cityService.AddPolyclinic(cityId, polyId))
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

            return RedirectToAction("SearchByTitle", "DataWorker");


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

            var city = new City();
            city.CityId = cityModel.CityId;
            city.Title = cityModel.Title;

            if (!_cityService.Add(city))
            {
                return NotFound("Ошибка добавления. Возможно такой город уже существует.");
            }

            return RedirectToAction("AdminPanel", "DataWorker");
        }

        //Очистка поиска, чистим сессию и редирект на страницу городов
        public IActionResult ClearSearch()
        {
            HttpContext.Session.Clear();
            
            return RedirectToAction("AdminPanel", "DataWorker");

        }


    }
}
