using JWTAthorizeTesting.Entities;

namespace JWTAthorizeTesting.Models.Interfaces
{
    public interface ICityService: IDefaultService<City>
    {
        //Все поликлиники для города
        IList<IEnumerable<Polyclinic>> ChooseAllPolyInCity(int cityId);
        //Все поликлиники, невходящие в город
        IList<Polyclinic> ChooseOtherPolyInCity(int cityId);
        //Все поликлиники, входящие в город

        bool AddPolyclinic(int cityId, int polyId);
        bool RemovePolyclinic(int cityId, int polyId);


    }
}
