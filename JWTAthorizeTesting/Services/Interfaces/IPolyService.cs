using JWTAthorizeTesting.Entities;
using JWTAthorizeTesting.Models.Interfaces;

namespace JWTAthorizeTesting.Services.Interfaces
{
    public interface IPolyService: IDefaultService<Polyclinic>
    {
        IList<Doctor> ChooseOtherDocsInPoly(int polyId);

        bool AddDoc(int docId, int polyId);
        bool RemoveDoc(int docId, int polyId);
    }
}
