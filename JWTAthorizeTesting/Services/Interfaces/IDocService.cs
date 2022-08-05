using JWTAthorizeTesting.Entities;
using JWTAthorizeTesting.Models.Interfaces;

namespace JWTAthorizeTesting.Services.Interfaces
{
    public interface IDocService: IDefaultService<Doctor>
    {
        IList<Specialization> ChooseOtherSpec(int docId);
        IList<Polyclinic> ChooseOtherPoly(int docId);
        bool AddSpec(int docId, int specId);
        bool RemoveSpec(int docId, int specId);
        bool AddPoly(int docId, int polyId);
        bool RemovePoly(int docId, int polyId);
    }
}
