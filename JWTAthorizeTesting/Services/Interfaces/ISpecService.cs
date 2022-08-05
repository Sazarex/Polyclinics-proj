using JWTAthorizeTesting.Entities;
using JWTAthorizeTesting.Models;
using JWTAthorizeTesting.Models.Interfaces;

namespace JWTAthorizeTesting.Services.Interfaces
{
    public interface ISpecService : IDefaultService<Specialization>
    {
        IList<Doctor> ChooseOtherDocs(int specId);
        bool AddDoc(int docId, int specId);
        bool RemoveDoc(int docId, int specId);
    }
}
