using JWTAthorizeTesting.Domain;
using JWTAthorizeTesting.Entities;
using JWTAthorizeTesting.Models;
using JWTAthorizeTesting.Models.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace JWTAthorizeTesting.Services.ServiceClasses
{
    public class CityService : ICityService
    {

        public bool Add(IBaseModel entity)
        {
            if (entity != null && entity is CityViewModel cityModel)
            {
                using (var db = new AppDbContext())
                {
                    if (db.Cities.Any(c => c.Title == cityModel.Title))
                    {
                        return false;
                    }

                    City city = new City()
                    {
                        Title = cityModel.Title
                    };

                    db.Cities.Add(city);
                    db.SaveChanges();

                }
                return true;
            }
            return false;
        }

        public bool RemovePolyclinic(int cityId, int polyId)
        {
            if (cityId == 0 || polyId == 0)
            {
                return false;
            }

            using (var db = new AppDbContext())
            {
                City? city = db.Cities.FirstOrDefault(c => c.CityId == cityId);

                Polyclinic? poly = db.Polyclinics.FirstOrDefault(p => p.Id == polyId);

                if (city == null || poly == null || poly.CityId != cityId)
                {
                    return false;
                }

                city.Polyclinics.Remove(poly);
                db.SaveChanges();

            }
            return true;
        }

        public bool AddPolyclinic(int cityId, int polyId)
        {
            if (cityId == 0 || polyId == 0)
            {
                return false;
            }

            using (var db = new AppDbContext())
            {
                City? city = db.Cities.FirstOrDefault(c => c.CityId == cityId);

                Polyclinic? poly = db.Polyclinics.FirstOrDefault(p => p.Id == polyId);

                if (city == null || poly == null || poly.CityId == cityId)
                {
                    return false;
                }

                city.Polyclinics.Add(poly);
                db.SaveChanges();

            }
            return true;
        }

        public IList<City> ChooseAll()
        {
            //Выбираю все города из бд и включаю связанные поликлиники
            using (var db = new AppDbContext())
            {
                var cities = db.Cities
                    .Include(_ => _.Polyclinics)
                    .ToList();

                return cities;
            }
        }

        //??удалить??
        public IList<IEnumerable<Polyclinic>> ChooseAllPolyInCity(int cityId)
        {

            using (var db = new AppDbContext())
            {
                //Проецирую только поликлиники из таблицы городов, и выбираю от туда поликлиники с id города
                var allPolyclinics = db.Cities.Select(_ => _.Polyclinics.Where(p => p.CityId == cityId));

                return allPolyclinics.ToList();

            }
        }

        public City ChooseById(int? id)
        {
            using (var db = new AppDbContext())
            {
                var city = db.Cities
                    .Include(c => c.Polyclinics)
                    .ThenInclude(p => p.Doctors)
                    .ThenInclude(d => d.Specializations)
                    .FirstOrDefault(_ => _.CityId == id);

                return city;
            }
        }

        public IList<Polyclinic> ChooseOtherPolyInCity(int cityId)
        {
            using (var db = new AppDbContext())
            {
                var otherPolyclinics = db.Polyclinics.Where(p => p.CityId != cityId).ToList();

                return otherPolyclinics;
            }

        }

        public bool Remove(int id)
        {
            if (id == 0)
            {
                return false;
            }

            using (var db = new AppDbContext())
            {

                City? city = db.Cities
                    .Include(c => c.Polyclinics)
                    .FirstOrDefault(c => c.CityId == id);
                if (city == null)
                {
                    return false;
                }

                foreach (var poly in city.Polyclinics)
                {
                    poly.CityId = null;
                }

                db.Cities.Remove(city);
                db.SaveChanges();

            }
            return true;
        }

        public bool Update(IBaseModel entity)
        {
            if (entity != null && entity is CityViewModel cityModel)
            {
                using (var db = new AppDbContext())
                {
                    City? city = db.Cities
                        .Include(c => c.Polyclinics)
                        .FirstOrDefault(c => c.CityId == cityModel.CityId);
                    if (city == null || db.Cities.Where(c => c.CityId != city.CityId).Any(c => c.Title == cityModel.Title))
                    {
                        return false;
                    }

                    city.Title = cityModel.Title;
                    db.SaveChanges();
                }

                return true;
            }
            return false;
        }

        public IList<City> ChooseForSearch(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            using (var db = new AppDbContext())
            {
                Regex regex = new Regex($@"\w*{title}\w*", RegexOptions.IgnoreCase);
                var titlesOfcities = db.Cities.Select(c => c.Title).ToList();
                List<City> cities = new List<City>();

                foreach (var titleOfCity in titlesOfcities)
                {
                    if (regex.IsMatch(titleOfCity))
                    {
                        City city = db.Cities
                            .Include(c => c.Polyclinics)
                            .FirstOrDefault(c => c.Title == titleOfCity);
                        cities.Add(city);
                    }

                }

                return cities;
            }
        }
    }
}
