namespace JWTAthorizeTesting.Models.Interfaces
{
    public interface IDefaultService<T>
    {
        T ChooseById(int? id);

        IList<T> ChooseForSearch(string title);

        IList<T> ChooseAll();

        bool Add(IBaseModel entity);

        bool Update(IBaseModel entity);

        bool Remove(int id);
    }
}
