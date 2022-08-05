using JWTAthorizeTesting.Domain;
using JWTAthorizeTesting.Entities;
using JWTAthorizeTesting.Models;
using JWTAthorizeTesting.Models.Interfaces;
using JWTAthorizeTesting.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JWTAthorizeTesting.Controllers
{
    public class HomeController:Controller
    {
        private AppDbContext db;
        readonly ISpecService _specService;
        readonly ICityService _cityService;
        readonly IDocService _docService;

        public HomeController(AppDbContext _db, ISpecService specService, ICityService cityService, IDocService docService)
        {
            db = _db;
            _specService = specService;
            _cityService = cityService;
            _docService = docService;
        }
        public async Task<IActionResult> Index()
        {
            //Создаем модель
            MainPageViewModel mainModel = new MainPageViewModel();

            //Если в сессии нет ключа cityId
            if (!HttpContext.Session.Keys.Contains("cityId"))
            {


                //Устанавливаем в модель данные о всех специализациях
                mainModel.AllSpecializations = _specService.ChooseAll();
                //В модель добавляем все города, которые есть, чтобы вывести в выпадающем меню
                mainModel.AllCities = _cityService.ChooseAll();
            }
            //Если в сессии есть ключ cityId
            else
            {
                //устанавливаем в переменную значение сессии с этим ключем
                int? cityId = HttpContext.Session.GetInt32("cityId");

                // Загружаем город с id из переменной сессии, включаем в этот город всех его врачей и поликлиники,а у врачей их специализации
                var city = _cityService.ChooseById(cityId);

                mainModel.CityId = city.CityId;
                mainModel.Title = city.Title;
                mainModel.Polyclinics = city.Polyclinics;

                //Назначаем во вьюбаг название нашего города, чтобы выводить в представлении
                ViewBag.City = city.Title;

                //из города выбираем все поликлиники в коллекцию
                var polyOfCity = city.Polyclinics;
                //из коллекции поликлиник выбираем всех врачей тоже в коллекцию
                //Через SelectMany делаем, т.к. polyOfCity у нас является коллекцией,а внутри неё ещё коллекция из врачей
                mainModel.DocsOfCity = polyOfCity.SelectMany(p => p.Doctors).ToList();
                mainModel.AllSpecializations = mainModel.DocsOfCity.SelectMany(d => d.Specializations).ToList();
                mainModel.AllSpecializations = mainModel.AllSpecializations.Distinct().ToList();
                mainModel.AllCities = _cityService.ChooseAll();
                //model.Cities = db.Cities.ToList();

            }

            return View(mainModel);

        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        /// <summary>
        /// Выбираем город
        /// </summary>
        /// <param name="cityId">передается из выпадающего списка представления</param>
        /// <returns></returns>
        public IActionResult ChooseCity(int cityId)
        {
            if (cityId == null)
            {
                NotFound("Ошибка ID.");
            }
            //Устанавливаем в сессию ключ "cityId" значение cityId
            HttpContext.Session.SetInt32("cityId", cityId);
            return RedirectToAction("Index", "Home");
        }


        /// <summary>
        /// Чистим сессию
        /// </summary>
        /// <returns></returns>
        public IActionResult AllCitiesOnMainPage()
        {
            
            if (HttpContext.Session.Keys.Contains("cityId"))
            {
                //Чистим сессию
                HttpContext.Session.Clear();
                ViewBag.City = "";
            }

            return RedirectToAction("Index", "Home");
        }


        public async Task<IActionResult> SpecificSpec(int? specId)
        {
            if (specId == null || specId == 0)
            {
                return NotFound("Ошибка ID.");
            }
            //Определяем специализацию из бд по id
            Specialization spec = _specService.ChooseById(specId);

            MainPageSpecViewModel specModel = new MainPageSpecViewModel();
            specModel.SpecializationId = spec.SpecializationId;
            specModel.Title = spec.Title;
            

            if (HttpContext.Session.Keys.Contains("cityId"))
            {

                int? cityId = HttpContext.Session.GetInt32("cityId");

                specModel.CityOfSpec = _cityService.ChooseById(cityId);

                var doctorsFormCity = specModel.CityOfSpec.Polyclinics.SelectMany(p => p.Doctors).ToList();
                doctorsFormCity = doctorsFormCity.Distinct().ToList();

                var doctorsFormCityWithSpec = doctorsFormCity.Where(d => d.Specializations.Any(s => s.SpecializationId == spec.SpecializationId)).ToList();

                if (doctorsFormCityWithSpec.Count > 0)
                {
                    specModel.Doctors = doctorsFormCityWithSpec;
                }
            }
            else
            {

                //var doctorsFormCityWithSpec = await db.Doctors
                //    .Include(d => d.Specializations)
                //    .Where(d => d.Specializations.Contains(spec)).ToListAsync();

                if (spec.Doctors.Count > 0)
                {
                    specModel.Doctors = spec.Doctors;
                }
            }

            return View(specModel);
        }


        public async Task<IActionResult> SpecificDoc(int? docId)
        {
            if (docId == 0)
            {
                return NotFound("Ошибка ID.");
            }

            Doctor doc = _docService.ChooseById(docId);

            if (doc == null)
            {
                return NotFound("Ошибка вывода. Доктора не существует или неверный ID.");
            }

            MainPageDocViewModel docModel = new MainPageDocViewModel();
            docModel.Id = doc.Id;
            docModel.FIO = doc.FIO;
            docModel.Phone = doc.Phone;
            docModel.Photo = doc.Photo;
            docModel.Price = doc.Price;
            docModel.FullDesc = doc.FullDesc;
            docModel.ShortDesc = doc.ShortDesc;

            if (HttpContext.Session.Keys.Contains("cityId"))
            {
                int? cityId = HttpContext.Session.GetInt32("cityId");

                docModel.Polyclinics = doc.Polyclinics.Where(p => p.CityId == cityId).ToList();

                //doc = await db.Doctors
                //.Include(d => d.Specializations)
                //.Include(d => d.Polyclinics.Where(p => p.CityId == cityId))
                //.FirstOrDefaultAsync(d => d.Id == docId);
            }
            else
            {
                docModel.Polyclinics = doc.Polyclinics.ToList();

                //doc = await db.Doctors
                //.Include(d => d.Specializations)
                //.Include(d => d.Polyclinics)
                //.FirstOrDefaultAsync(d => d.Id == docId);
            }

            return View(docModel);

        }





        //Вытягиваем все специализации из бд
        private async Task<List<Specialization>> OutputAllSpecAsync()
        {
            List<Specialization> collectionToOutput = new List<Specialization>();
            collectionToOutput = await db.Specializations
                .Include(s => s.Doctors).ToListAsync();

            return collectionToOutput;
        }
    }
}
