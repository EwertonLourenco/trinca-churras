using Domain.Entities;
using System.Threading.Tasks;

namespace Domain.Services
{
    public interface IPersonService : IBaseService<Person>
    {
        Task<Person?> AcceptInvite(string id, string inviteId, bool isVeg);
        Task<Person?> DeclineInvite(string id, string inviteId);
        Task<Person?> GetInvitesByUserId(string id);
    }
}