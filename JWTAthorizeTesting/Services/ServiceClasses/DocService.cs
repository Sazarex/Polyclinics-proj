using JWTAthorizeTesting.Domain;
using JWTAthorizeTesting.Entities;
using JWTAthorizeTesting.Models;
using JWTAthorizeTesting.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace JWTAthorizeTesting.Services.ServiceClasses
{
    public class DocService : IDocService
    {
        public bool Add(Doctor entity, IFormFile photoToUpload)
        {
            if (entity != null && entity is Doctor docModel)
            {
                using (var db = new AppDbContext())
                {
                    //Проверяем на пустое название и чтобы название в бд не повторялось
                    if (string.IsNullOrWhiteSpace(docModel.FIO) || db.Doctors.Any(d => d.FIO == docModel.FIO))
                    {
                        return false;
                    }

                    Doctor newDoc = new Doctor();

                    //в новую поликлинику добавляем записи из модели
                    newDoc.FIO = docModel.FIO;
                    newDoc.Price = docModel.Price;
                    newDoc.Phone = docModel.Phone;
                    newDoc.ShortDesc = docModel.ShortDesc;
                    newDoc.FullDesc = docModel.FullDesc;


                    //Если фото загружено, то загружаем его в wwwroot и в бд (через относительный пусть)
                    if (photoToUpload != null)
                    {
                        var fileName = Path.GetFileName(photoToUpload.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\images", fileName);

                        //Относительный путь к изображению в папке проекта
                        var titleOfFileToDb = Path.Combine(@"\images\", fileName);
                        newDoc.Photo = titleOfFileToDb;
                        //Загрузка изображения в wwwroot
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            photoToUpload.CopyToAsync(fileStream);
                        }
                    }

                    db.Doctors.Add(newDoc);
                    db.SaveChanges();
                }
                return true;
            }
            return false;
        }

        public bool AddPoly(int docId, int polyId)
        {

            if (docId == 0 || polyId == 0)
            {
                return false;
            }

            using (var db= new AppDbContext())
            {
                //качаем из бд поликлинику с данным id
                Polyclinic? polyclinic = db.Polyclinics.FirstOrDefault(p => p.Id == polyId);
                if (polyclinic == null)
                {
                    return false;
                }

                //качаем из бд доктора с данным id
                Doctor? doc = db.Doctors.FirstOrDefault(d => d.Id == docId);
                if (doc == null)
                {
                    return false;
                }

                //В поликлинику добавляем врача
                polyclinic.Doctors.Add(doc);
                db.SaveChanges();
            }
            return true;
            
        }

        public bool AddSpec(int docId, int specId)
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

        public IList<Doctor> ChooseAll()
        {
            using (var db = new AppDbContext())
            {
                var doctors = db.Doctors
                    .Include(d => d.Specializations)
                    .Include(d => d.Polyclinics)
                    .ToList();

                return doctors;
            }
        }
        
        public Doctor ChooseById(int? id)
        {
            if (id == 0)
            {
                return null;
            }

            using (var db = new AppDbContext())
            {
                Doctor? doc = db.Doctors
                    .Include(d => d.Polyclinics)
                    .Include(d => d.Specializations)
                    .FirstOrDefault(d => d.Id == id);

                return doc;
            }
        }

        public IList<Doctor> ChooseForSearch(string title)
        {

            if (string.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            using (var db = new AppDbContext())
            {
                Regex regex = new Regex($@"\w*{title}\w*", RegexOptions.IgnoreCase);
                var FIOofDocs = db.Doctors.Select(d => d.FIO).ToList();
                List<Doctor> docs = new List<Doctor>();

                foreach (var FIOofDoc in FIOofDocs)
                {
                    if (regex.IsMatch(FIOofDoc))
                    {
                        Doctor doc = db.Doctors
                            .Include(d => d.Polyclinics)
                            .Include(d => d.Specializations)
                            .FirstOrDefault(d => d.FIO == FIOofDoc);
                        docs.Add(doc);
                    }

                }

                return docs;
            }
        }

        public IList<Polyclinic> ChooseOtherPoly(int docId)
        {
            if (docId == 0)
            {
                return null;
            }

            using (var db = new AppDbContext())
            {
                Doctor? doc = db.Doctors.FirstOrDefault(d => d.Id == docId);

                var polyToAdd = db.Polyclinics.Where(p => !p.Doctors.Contains(doc)).ToList();

                return polyToAdd;
            }
        }

        public IList<Specialization> ChooseOtherSpec(int docId)
        {
            if (docId == 0)
            {
                return null;
            }

            using (var db = new AppDbContext())
            {
                Doctor? doc = db.Doctors.FirstOrDefault(d => d.Id == docId);

                var specToAdd = db.Specializations.Where(s => !s.Doctors.Contains(doc)).ToList();

                return specToAdd;
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
                //Качаем из бд поликлинику по ИД и включаем в неё всех врачей
                Doctor? doc = db.Doctors
                    .Include(d => d.Specializations)
                    .Include(d => d.Polyclinics)
                    .FirstOrDefault(p => p.Id == id);

                if (doc == null)
                {
                    return false;
                }

                //Удаление фото
                if (doc.Photo != null)
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot" + doc.Photo);

                    FileInfo fileInfo = new FileInfo(filePath);
                    fileInfo.Delete();
                }

                foreach (var poly in doc.Polyclinics)
                {
                    poly.Doctors.Remove(doc);
                }

                foreach (var spec in doc.Specializations)
                {
                    spec.Doctors.Remove(doc);
                }

                db.Doctors.Remove(doc);

                db.SaveChanges();
            }
            return true;
        }

        public bool RemovePoly(int docId, int polyId)
        {
            if (docId == 0 || polyId == 0)
            {
                return false;
            }

            using (var db = new AppDbContext())
            {
                //Достаем из бд врача по ид
                Doctor? doc = db.Doctors.FirstOrDefault(d => d.Id == docId);
                if (doc == null)
                {
                    return false;
                }
                //достаем из бд поликлинику по ид и включаем в linq-запрос всех  врачей
                Polyclinic? poly = db.Polyclinics.Include(p => p.Doctors)
                    .FirstOrDefault(p => p.Id == polyId);
                if (poly == null)
                {
                    return false;
                }

                //удаляем врача
                poly.Doctors.Remove(doc);
                db.SaveChanges();
            }
            return true;
        }

        public bool RemoveSpec(int docId, int specId)
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

        public bool Update(Doctor entity, IFormFile photoToUpload)
        {
            if (entity != null && entity is Doctor docModel)
            {
                using (var db = new AppDbContext())
                {
                    if (db.Doctors.Where(d => d.Id != docModel.Id).Any(d => d.FIO == docModel.FIO) || docModel.FIO == null)
                    {
                        return false;
                    }

                    //Ищем врача по id
                    Doctor? doc = db.Doctors
                        .Include(d => d.Specializations)
                        .Include(d => d.Polyclinics)
                        .FirstOrDefault(d => d.Id == docModel.Id);

                    if (doc == null)
                    {
                        return false;
                    }

                    if (photoToUpload != null)
                    {
                        //Удаление фото
                        if (doc.Photo != null)
                        {
                            var filePathOfPhoto = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot" + doc.Photo);

                            FileInfo fileInfo = new FileInfo(filePathOfPhoto);
                            fileInfo.Delete();
                        }


                        var fileName = Path.GetFileName(photoToUpload.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\images", fileName);

                        //Относительный путь к изображению в папке проекта
                        var titleOfFileToDb = Path.Combine(@"\images\", fileName);
                        doc.Photo = titleOfFileToDb;
                        //Загрузка изображения в wwwroot
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            photoToUpload.CopyTo(fileStream);
                        }
                    }

                    doc.FIO = docModel.FIO;
                    doc.FullDesc = docModel.FullDesc;
                    doc.ShortDesc = docModel.ShortDesc;
                    doc.Price = docModel.Price;
                    doc.Phone = docModel.Phone;

                    db.SaveChanges();
                }
                return true;
            }
            return false;
        }
    }
}
