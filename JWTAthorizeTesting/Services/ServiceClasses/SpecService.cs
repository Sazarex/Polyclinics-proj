using JWTAthorizeTesting.Domain;
using JWTAthorizeTesting.Entities;
using JWTAthorizeTesting.Models;
using JWTAthorizeTesting.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace JWTAthorizeTesting.Services.ServiceClasses
{
    public class SpecService : ISpecService
    {
        public bool Add(Specialization entity)
        {
            if (entity != null && entity is Specialization specModel)
            {
                using (var db = new AppDbContext())
                {
                    if (specModel == null || string.IsNullOrWhiteSpace(specModel.Title) ||
                        db.Specializations.Any(s => s.Title == specModel.Title))
                    {
                        return false;
                    }

                    Specialization spec = new Specialization();
                    spec.Title = specModel.Title;
                    
                    db.Specializations.Add(spec);
                    db.SaveChanges();
                }
                return true;
            }
            return false;
        }

        public bool AddDoc(int docId, int specId)
        {
            if (docId == 0 || specId == 0)
            {
                return false;
            }

            using (var db = new AppDbContext())
            {
                Specialization? spec = db.Specializations.FirstOrDefault(s => s.SpecializationId == specId);
                if (spec == null)
                {
                    return false;
                }

                Doctor? doc = db.Doctors.FirstOrDefault(d => d.Id == docId);
                if (doc == null)
                {
                    return false;
                }

                doc.Specializations.Add(spec);
                db.SaveChanges();
            }
            return true;
        }

        public IList<Specialization> ChooseAll()
        {
            using (var db = new AppDbContext())
            {
                var specializations = db.Specializations
                    .Include(s => s.Doctors)
                    .ToList();

                return specializations;
            }
        }

        public Specialization ChooseById(int? id)
        {
            if (id == 0)
            {
                return null;
            }

            using (var db = new AppDbContext())
            {
                Specialization? spec = db.Specializations
                   .Include(s => s.Doctors)
                   .FirstOrDefault(s => s.SpecializationId == id);

                return spec;
            }
        }

        public IList<Specialization> ChooseForSearch(string title)
        {

            if (string.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            using (var db = new AppDbContext())
            {
                Regex regex = new Regex($@"\w*{title}\w*", RegexOptions.IgnoreCase);
                var titlesOfSpecs = db.Specializations.Select(s => s.Title).ToList();
                var doctors = db.Specializations.SelectMany(s => s.Doctors).Distinct().ToList();

                List<Specialization> specs = new List<Specialization>();

                foreach (var titleOfspec in titlesOfSpecs)
                {
                    if (regex.IsMatch(titleOfspec))
                    {
                        Specialization spec = db.Specializations
                            .Include(s => s.Doctors)
                            .FirstOrDefault(s => s.Title == titleOfspec);
                        specs.Add(spec);
                    }

                }


                foreach (var doc in doctors)
                {
                    if (regex.IsMatch(doc.FIO))
                    {
                        var specWithDoc = db.Specializations
                            .Include(s => s.Doctors)
                            .Where(s => s.Doctors.Any(d => d.FIO == doc.FIO)).ToList();
                        specs.AddRange(specWithDoc);
                    }

                }

                specs = specs.Distinct().ToList();

                return specs;
            }
        }

        public IList<Doctor> ChooseOtherDocs(int specId)
        {
            if (specId == 0)
            {
                return null;
            }

            using (var db = new AppDbContext())
            {
                var spec = db.Specializations.FirstOrDefault(s => s.SpecializationId == specId);
                var otherDocs = db.Doctors.Where(d => !d.Specializations.Contains(spec)).ToList();

                return otherDocs;
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
                Specialization? spec = db.Specializations.Include(s => s.Doctors)
                    .FirstOrDefault(s => s.SpecializationId == id);

                foreach (var doc in spec.Doctors.ToList())
                {
                    doc.Specializations.Remove(spec);
                }
                db.Specializations.Remove(spec);

                db.SaveChanges();
            }
            return true;
        }

        public bool RemoveDoc(int docId, int specId)
        {
            if (docId == 0 || specId == 0)
            {
                return false;
            }

            using (var db = new AppDbContext())
            {
                Specialization? spec = db.Specializations.FirstOrDefault(s => s.SpecializationId == specId);
                if (spec == null)
                {
                    return false;
                }

                Doctor? doc = db.Doctors
                    .Include(d => d.Specializations)
                    .FirstOrDefault(d => d.Id == docId);
                if (doc == null)
                {
                    return false;
                }

                doc.Specializations.Remove(spec);

                db.SaveChanges();
            }
            return true;
        }

        public bool Update(Specialization entity)
        {
            if (entity != null && entity is Specialization specModel)
             {

                using (var db = new AppDbContext())
                {
                    Specialization? spec = db.Specializations.FirstOrDefault(s => s.SpecializationId == specModel.SpecializationId);

                    if (spec == null || string.IsNullOrWhiteSpace(specModel.Title))
                    {
                        return false;
                    }

                    //Проверяем, чтобы не редактировали название поликлиники на существующее название специализации
                    if (db.Specializations.Where(s => s.SpecializationId != specModel.SpecializationId).Any(s => s.Title == specModel.Title))
                    {
                        return false;
                    }

                    spec.Title = specModel.Title;

                    db.SaveChanges();
                }
                return true;
            }
            return false;
        }
    }
}
