using JWTAthorizeTesting.Entities;
using Microsoft.EntityFrameworkCore;

namespace JWTAthorizeTesting.Domain
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }


        public DbSet<City> Cities { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Experience> Experiences { get; set; }
        public DbSet<Polyclinic> Polyclinics { get; set; }
        public DbSet<Specialization> Specializations { get; set; }


        public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
        {
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            #region Seeds

            #region Роли и юзеры
            Role admin = new Role() { RoleId = 1, Title = "Administrator" };
            Role user = new Role() { RoleId = 2, Title = "User" };


            modelBuilder.Entity<Role>().HasData(new Role[]
            {
                admin,user
            });

            modelBuilder.Entity<User>().HasData(new User[]
            {
                new User()
                {
                    Id=1,
                    Login="Admin",
                    Password="olUO6rBySmkRksoTmC5uvQ==",//qazwsxedc
                    RoleId=1
                },
                new User()
                {
                    Id=2,
                    Login="User1",
                    Password="2FeO34RYzgb7xbt2pYxcpA==",//qwerty
                    RoleId=2
                }

            });
            #endregion

            #region Города
            City Moscow = new City()
            {
                CityId = 1,
                Title = "Москва"
            };

            City StPeterburg = new City()
            {
                CityId = 2,
                Title = "Санкт-Петербург"
            };

            City Omsk = new City()
            {
                CityId = 3,
                Title = "Омск"
            };

            City Murmansk = new City()
            {
                CityId = 4,
                Title = "Мурманск"
            };



            modelBuilder.Entity<City>().HasData(new City[]
            {
                Moscow,
                StPeterburg,
                Omsk,
                Murmansk
            });
            #endregion

            #region Поликлиники
            Polyclinic MoscowPolyN1 = new Polyclinic()
            {
                Id = 1,
                CityId = Moscow.CityId,
                Title = "Первая Московская поликлиника",
                Adress = "ул. Каскадная 43",
                Phone = "+30454587632"
            };

            Polyclinic MoscowPolyN2 = new Polyclinic()
            {
                Id = 2,
                CityId = Moscow.CityId,
                Title = "Вторая Московская поликлиника",
                Adress = "ул. Ленина 113",
                Phone = "+30354997032"
            };

            Polyclinic OmskPoly = new Polyclinic()
            {
                Id = 3,
                CityId = Omsk.CityId,
                Title = "Омская поликлиника им.Селуянова",
                Adress = "ул. Карла Маркса 20",
                Phone = "+30454581132"
            };


            modelBuilder.Entity<Polyclinic>().HasData(new Polyclinic[]
            {
                MoscowPolyN1,
                MoscowPolyN2,
                OmskPoly
            });

            #endregion

            #region Врачи
            Doctor Loboda = new Doctor()
            {
                Id = 1,
                FIO = "Лобода Антон Викторович",
                Phone = "37377788787",
                Price = 10000,
                FullDesc = "Антон Васильевич занимается лечением " +
                "заболеваний терапевтического профиля, лечением болевых " +
                "синдромов. Проводит ксенонотерапия с целью нормализации сна, " +
                "коррекции тревоги, повышения жизненного тонуса.",
                ShortDesc = "Антон Васильевич занимается лечением " +
                "заболеваний терапевтического профиля, лечением болевых " +
                "синдромов. "
            };


            Doctor Kirianova = new Doctor()
            {
                Id = 2,
                FIO = "Кирьянова Александра Игоревна",
                Phone = "37377088787",
                Price = 12000,
                FullDesc = "Александра Игоревна занимается лечением заболеваний " +
                "сердечно-сосудистой системы, комплексным восстановлением организма " +
                "после сосудистых катастроф. Выясняет причины повышенного давления и" +
                " помогает справиться с неконтролируемой гипертонической болезнью. " +
                "Лечит и назначает обследование при болезнях органов дыхания, пищеварения, " +
                "мочевыделительной системы, суставной патологии.",
                ShortDesc = "Александра Игоревна занимается лечением заболеваний " +
                "сердечно-сосудистой системы, комплексным восстановлением организма " +
                "после сосудистых катастроф."
            };


            Doctor Abramova = new Doctor()
            {
                Id = 3,
                FIO = "Абрамова Елена Викторовна",
                Phone = "37377080087",
                Price = 7500,
                FullDesc = "Елена Викторовна занимается диагностикой " +
                "и лечением кожных заболеваний (псориаз, атопический " +
                "дерматит, крапивница), вирусных и бактериальных, а " +
                "также грибковых инфекций кожи, косметологией (инъекции " +
                "ботокса, диспорта, гиалуроновой кислоты, мезотерапия и проч.). " +
                "Осуществляет удаление кожных образований.",
                ShortDesc = "Елена Викторовна занимается диагностикой " +
                "и лечением кожных заболеваний."
            };

            modelBuilder.Entity<Doctor>().HasData(new Doctor[]
            {
                Abramova, Loboda,Kirianova
            });

            #endregion

            #region Специализации
            Specialization dermatovener = new Specialization()
            {
                SpecializationId = 1,
                Title = "Дерматовенеролог"
            };
            Specialization psycho = new Specialization()
            {
                SpecializationId = 2,
                Title = "Психиатр"
            };
            Specialization gastro = new Specialization()
            {
                SpecializationId = 3,
                Title = "Гастроэнтеролог"
            };
            Specialization terapevt = new Specialization()
            {
                SpecializationId = 4,
                Title = "Терапевт"
            };
            Specialization ginekolog = new Specialization()
            {
                SpecializationId = 5,
                Title = "Гинеколог"
            };

            modelBuilder.Entity<Specialization>().HasData(new Specialization[]
            {
                dermatovener, psycho, gastro, terapevt, ginekolog
            });
            #endregion


            #endregion


            base.OnModelCreating(modelBuilder);
        }
    }
}
