﻿namespace JWTAthorizeTesting.Models.Interfaces
{
    public interface IDefaultService<T>
    {
        T ChooseById(int? id);

        IList<T> ChooseForSearch(string title);

        IList<T> ChooseAll();

        bool Add(T entity);

        bool Update(T entity);

        bool Remove(int id);
    }
}
