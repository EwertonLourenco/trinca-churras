using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Services
{
    public interface IBbqService : IBaseService<Bbq>
    {
        Task<Bbq?> CreateNew(DateTime date, string reason, bool isTrincasPaying);
        Task<List<object>> GetAllNotRejectedOrAvailable(string userId);
        Task<Bbq?> GetShoppingListByBbqId(string userId, string bbqId);
        Task<Bbq?> Moderate(string id, bool gonnaHappen, bool trincaWillPay);
    }
}