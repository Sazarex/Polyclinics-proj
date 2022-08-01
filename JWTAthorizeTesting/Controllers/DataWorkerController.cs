using JWTAthorizeTesting.Domain;
using JWTAthorizeTesting.Entities;
using JWTAthorizeTesting.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JWTAthorizeTesting.Controllers
{

    [Authorize(Roles = "Administrator")]
    public class DataWorkerController: Controller
    {

        private AppDbContext db;
        public DataWorkerController(AppDbContext _db)
        {
            db = _db;
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
        public async Task<IActionResult> EditCity(int? cityId)
        {
            if (cityId != null)
            {
               //Находим город по переданному cityId из представления
                City? city = await db.Cities.Include(c => c.Polyclinics)
                    .FirstOrDefaultAsync(c => c.CityId == cityId);
                if (city != null)
                {
                    //Создаем нашу view model
                    AdminPanelViewModel adminModel = new AdminPanelViewModel();
                    //Добавляем в неё найденный ранее город
                    adminModel.Cities.Add(city);
                    //Создаем список поликлиник для добавления в город, которые не присутствуют в этом городе
                    List<Polyclinic> polyclinicsForAdding = await db.Polyclinics.Where(p => p.CityId != city.CityId).ToListAsync();
                    //Добавляем этот список в нашу view model
                    adminModel.Polyclinics.AddRange(polyclinicsForAdding);
                    return View(adminModel);
                }
            }
            return NotFound();
        }


        /// <summary>
        /// Редактирование города
        /// </summary>
        /// <param name="adminPanelView"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditCity(AdminPanelViewModel adminPanelView)
        {
            if(adminPanelView == null)
            {
                return NotFound();
            }
            //Вытягиваем город из переданной нам модели
            var cityFromModel = adminPanelView.Cities.FirstOrDefault();
            //По id переданного города находим город из бд
            City? city = db.Cities.FirstOrDefault(c => c.CityId == cityFromModel.CityId);
            if (city == null )
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(cityFromModel.Title))
            {
                return NotFound("Проверьте название города");
            }


            if (db.Cities.Where(c => c.CityId != cityFromModel.CityId).Any(c => c.Title == cityFromModel.Title))
            {
                return NotFound("Данный город уже есть");
            }

            //В городе из бд меняем название города
            city.Title =  adminPanelView.Cities.FirstOrDefault().Title;
            await db.SaveChangesAsync();

            return Redirect($"EditCity?CityId={adminPanelView.Cities.FirstOrDefault().CityId}");
        }


        /// <summary>
        /// Добавление поликлиники в город
        /// </summary>
        /// <param name="cityId">Передается из представления через тэг хелпер</param>
        /// <param name="polyId">Передается из представления через тэг хелпер</param>
        /// <returns></returns>
        public async Task<IActionResult> AddPolyInCity(int? cityId,int? polyId)
        {
            //ищем в бд город
            var city =await db.Cities.FirstOrDefaultAsync(c => c.CityId == cityId);
            if (city == null)
            {
                return NotFound();
            }
            //ищем в бд поликлинику
            var polyToAdd =await db.Polyclinics.FirstOrDefaultAsync(p => p.Id == polyId);
            if (polyToAdd == null)
            {
                return NotFound();
            }
            //Добавляем в город поликлинику и сохраняем изменения
            city.Polyclinics.Add(polyToAdd);
            db.SaveChanges();
            return Redirect($"EditCity?CityId={cityId}");
        }


        /// <summary>
        /// Удаляем город из списка городов
        /// </summary>
        /// <param name="CityId">ID города, передается через представление</param>
        /// <returns></returns>
        public async Task<IActionResult> RemoveCity(int? CityId)
        {
            if (CityId == null || CityId == 0)
            {
                return NotFound();
            }

            //Вытаскиваем город из бд и включаем в него поликлиники
            City? city = db.Cities.Include(c=> c.Polyclinics)
                .FirstOrDefault(c => c.CityId == CityId);
            if (city == null)
            {
                return NotFound();
            }

            //Привязанным поликлиникам делаем значение внешнего ключа null
            foreach(var poly in city.Polyclinics)
            {
                poly.CityId = null;
            }

            //Удаляем город из бд и сохраняем бд
            db.Cities.Remove(city);
            await db.SaveChangesAsync();

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
        public async Task<IActionResult> AddCity(City? city)
        {
            //Проверяем на валидность, чтоб поле было заполнено
            if (!ModelState.IsValid)
            {
                return View(city);
            }
            if (city == null)
            {
                return NotFound();
            }

            //Если город есть уже в таблице, то его не добавляем
            City? checkCityInDB = await db.Cities.FirstOrDefaultAsync(c => c.Title == city.Title);
            if (checkCityInDB != null)
            {
                return RedirectToAction("AdminPanel", "Auth");
            }


            //Добавляем и сохраняем
            await db.Cities.AddAsync(city);
            await db.SaveChangesAsync();

            return RedirectToAction("AdminPanel", "Auth");
        }


    }
}
