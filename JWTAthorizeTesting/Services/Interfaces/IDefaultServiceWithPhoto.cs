namespace JWTAthorizeTesting.Models.Interfaces
{
    public interface IDefaultServiceWithPhoto<T>
    {
        T ChooseById(int? id);

        IList<T> ChooseForSearch(string title);

        IList<T> ChooseAll();

        bool Add(T entity, IFormFile photoToUpload);

        bool Update(T entity, IFormFile photoToUpload);

        bool Remove(int id);
    }
}
