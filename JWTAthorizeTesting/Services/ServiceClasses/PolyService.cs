using JWTAthorizeTesting.Domain;
using JWTAthorizeTesting.Entities;
using JWTAthorizeTesting.Models;
using JWTAthorizeTesting.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JWTAthorizeTesting.Services.ServiceClasses
{
    public class PolyService : IPolyService
    {
        public bool Add(IBaseModel entity)
        {
            if (entity != null && entity is PolyViewModel polyModel)
            {
                using (var db = new AppDbContext())
                {
                    if (db.Polyclinics.Any(p => p.Title == polyModel.Title) || string.IsNullOrWhiteSpace(polyModel.Title))
                    {
                        return false;
                    }

                    Polyclinic poly = new Polyclinic()
                    {
                        Title = polyModel.Title,
                        Adress = polyModel.Adress,
                        Phone = polyModel.Phone
                    };

                    if (polyModel.PhotoToUpload != null)
                    {

                        var fileName = Path.GetFileName(polyModel.PhotoToUpload.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\images", fileName);

                        //Относительный путь к изображению в папке проекта
                        var titleOfFileToDb = Path.Combine(@"\images\", fileName);
                        poly.Photo = titleOfFileToDb;
                        //Загрузка изображения в wwwroot
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            polyModel.PhotoToUpload.CopyTo(fileStream);
                        }

                    }

                    db.Polyclinics.Add(poly);
                    db.SaveChanges();

                }
                return true;
            }
            return false;
        }

        public bool AddDoc(int docId, int polyId)
        {
            if (docId == 0 || polyId == 0)
            {
                return false;
            }

            using (var db = new AppDbContext())
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

        public IList<Polyclinic> ChooseAll()
        {
            using (var db = new AppDbContext())
            {
                var polyclinics = db.Polyclinics
                    .Include(p => p.City)
                    .Include(p => p.Doctors)
                    .ToList();
                return polyclinics;
            }
        }

        public Polyclinic ChooseById(int? id)
        {
            if (id == 0)
            {
                return null;
            }

            using (var db = new AppDbContext())
            {
                var polyclinic = db.Polyclinics
                    .Include(p => p.Doctors)
                    .Include(p => p.City)
                    .FirstOrDefault(p => p.Id == id);

                return polyclinic;
            }
        }

        public IList<Doctor> ChooseOtherDocsInPoly (int polyId)
        {
            using (var db = new AppDbContext())
            {
                Polyclinic poly = db.Polyclinics.FirstOrDefault(p => p.Id == polyId);

                if (poly == null)
                {
                    return null;
                }

                var otherDoctors = db.Doctors.Where(p => !p.Polyclinics.Contains(poly)).ToList();
                

                return otherDoctors;
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
                var poly = db.Polyclinics
                    .Include(p => p.Doctors)
                    .FirstOrDefault(p => p.Id == id);

                var docsInPoly = poly.Doctors.ToList();
                foreach (var doc in docsInPoly)
                {
                    doc.Polyclinics.Remove(poly);
                }

                if (poly.Photo != null)
                {
                    var filePathOfPhoto = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot" + poly.Photo);

                    FileInfo fileInfo = new FileInfo(filePathOfPhoto);
                    fileInfo.Delete();
                }

                db.Polyclinics.Remove(poly);
                db.SaveChanges();
            }
            return true;
        }

        public bool RemoveDoc(int docId, int polyId)
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
                Polyclinic? poly = db.Polyclinics
                    .Include(p => p.Doctors)
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

        public bool Update(IBaseModel entity)
        {
            if (entity != null && entity is PolyViewModel polyModel)
            {
                using (var db = new AppDbContext())
                {

                    if (db.Polyclinics.Where(p => p.Id != polyModel.Id).Any(p => p.Title == polyModel.Title) || polyModel.Title == null)
                    {
                        return false;
                    }

                    var polyclinic = db.Polyclinics
                        .Include(p => p.Doctors)
                        .Include(p => p.City)
                        .FirstOrDefault(p => p.Id == polyModel.Id);

                    polyclinic.Title = polyModel.Title;
                    polyclinic.Adress = polyModel.Adress;
                    polyclinic.Phone = polyModel.Phone;

                    if (polyModel.PhotoToUpload != null)
                    {
                        //Удаление фото
                        if (polyclinic.Photo != null)
                        {
                            var filePathOfPhoto = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot" + polyclinic.Photo);

                            FileInfo fileInfo = new FileInfo(filePathOfPhoto);
                            fileInfo.Delete();
                        }


                        var fileName = Path.GetFileName(polyModel.PhotoToUpload.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\images", fileName);

                        //Относительный путь к изображению в папке проекта
                        var titleOfFileToDb = Path.Combine(@"\images\", fileName);
                        polyclinic.Photo = titleOfFileToDb;
                        //Загрузка изображения в wwwroot
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            polyModel.PhotoToUpload.CopyTo(fileStream);
                        }
                    }

                    db.SaveChanges();
                }
                return true;
            }
            return false;
        }
    }
}
