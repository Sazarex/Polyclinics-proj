using JWTAthorizeTesting.Domain;
using JWTAthorizeTesting.Entities;
using JWTAthorizeTesting.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JWTAthorizeTesting.Controllers
{
    public class HomeController:Controller
    {
        private AppDbContext db;

        public HomeController(AppDbContext _db)
        {
            db = _db;
        }
        public async Task<IActionResult> Index()
        {
            //Создаем модель
            AdminPanelViewModel model = new AdminPanelViewModel();

            //Если в сессии нет ключа cityId
            if (!HttpContext.Session.Keys.Contains("cityId"))
            {
                //Выводим все специализации из бд
                List<Specialization> specializations = await OutputAllSpecAsync();

                //Устанавливаем в модель данные о всех специализациях
                model.Specializations = specializations;
                //В модель добавляем все города, которые есть, чтобы вывести в выпадающем меню
                model.Cities = await db.Cities.ToListAsync();
            }
            //Если в сессии есть ключ cityId
            else
            {
                //устанавливаем в переменную значение сессии с этим ключем
                int? cityId = HttpContext.Session.GetInt32("cityId");
                
                // Загружаем город с id из переменной сессии, включаем в этот город всех его врачей и поликлиники,а у врачей их специализации
                City? city = await db.Cities
                    .Include(c => c.Polyclinics)
                    .ThenInclude(p => p.Doctors)
                    .ThenInclude(d => d.Specializations)
                    .FirstOrDefaultAsync(c => c.CityId == cityId);
                //Назначаем в вьюбаг название нашего города, чтобы выводить в представлении
                ViewBag.City = city.Title;

                //из города выбираем все поликлиники в коллекцию
                var polyOfCity = city.Polyclinics.ToList();
                //из коллекции поликлиник выбираем всех врачей тоже в коллекцию
                //Через SelectMany делаем, т.к. polyOfCity у нас является коллекцией,а внутри неё ещё коллекция из врачей
                var docsOfCity = polyOfCity.SelectMany(p => p.Doctors).ToList();
                var specOfCity = docsOfCity.SelectMany(d => d.Specializations).ToList();

                specOfCity = specOfCity.Distinct().ToList() ;
                //Добавляем докторов в модель
                model.Doctors.AddRange(docsOfCity);

                model.Specializations.AddRange(specOfCity);

                model.Cities = db.Cities.ToList();

            }

            return View(model);

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
                NotFound();
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
                return NotFound();
            }
            //Определяем специализацию из бд по id
            Specialization? spec = await db.Specializations
                .Include(s => s.Doctors)
                .FirstOrDefaultAsync(s => s.SpecializationId == specId);

            AdminPanelViewModel adminPanelViewModel = new AdminPanelViewModel();
            adminPanelViewModel.Specializations.Add(spec);

            if (HttpContext.Session.Keys.Contains("cityId"))
            {

                int? cityId = HttpContext.Session.GetInt32("cityId");

                City city = await db.Cities
                    .Include(c => c.Polyclinics)
                    .ThenInclude(p => p.Doctors)
                    .ThenInclude(d => d.Specializations)
                    .FirstOrDefaultAsync(c => c.CityId == cityId);

                adminPanelViewModel.Cities.Add(city);

                var doctorsFormCity = city.Polyclinics.SelectMany(p => p.Doctors).ToList();
                doctorsFormCity = doctorsFormCity.Distinct().ToList();

                var doctorsFormCityWithSpec = doctorsFormCity.Where(d => d.Specializations.Contains(spec)).ToList();


                if (doctorsFormCityWithSpec.Count > 0)
                {
                    adminPanelViewModel.Doctors.AddRange(doctorsFormCityWithSpec);
                }
            }
            else
            {
                var doctorsFormCityWithSpec = await db.Doctors
                    .Include(d => d.Specializations)
                    .Where(d => d.Specializations.Contains(spec)).ToListAsync();

                if (doctorsFormCityWithSpec.Count > 0)
                {
                    adminPanelViewModel.Doctors.AddRange(doctorsFormCityWithSpec);
                }
            }


            return View(adminPanelViewModel);
        }


        public async Task<IActionResult> SpecificDoc(int? docId)
        {
            if (docId == 0)
            {
                return NotFound();
            }

            Doctor? doc = new Doctor();

            if (HttpContext.Session.Keys.Contains("cityId"))
            {
                int? cityId = HttpContext.Session.GetInt32("cityId");

                doc = await db.Doctors
                .Include(d => d.Specializations)
                .Include(d => d.Polyclinics.Where(p => p.CityId == cityId))
                .FirstOrDefaultAsync(d => d.Id == docId);
            }
            else
            {
                doc = await db.Doctors
                .Include(d => d.Specializations)
                .Include(d => d.Polyclinics)
                .FirstOrDefaultAsync(d => d.Id == docId);
            }


            if (doc == null)
            {
                return NotFound();
            }

            return View(doc);

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
