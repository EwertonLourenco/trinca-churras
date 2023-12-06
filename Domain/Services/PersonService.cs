using Domain.Entities; // Entidades do domínio
using Domain.Events; // Eventos do domínio
using Domain.Repositories; // Repositórios do domínio
using Eveneum; // Biblioteca para lidar com snapshots
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services
{
    public class PersonService : IPersonService
    {
        private readonly IPersonRepository _personRepository; // Repositório de pessoa
        private readonly IBbqRepository _bbqRepository; // Repositório de churrasco

        // Construtor da classe
        public PersonService(IPersonRepository personRepository, IBbqRepository bbqRepository)
        {
            _personRepository = personRepository;
            _bbqRepository = bbqRepository;
        }

        // Método para aceitar um convite por ID de pessoa, ID de convite e se é veg ou não
        public async Task<Person?> AcceptInvite(string id, string inviteId, bool isVeg)
        {
            var person = await this.GetAsync(id);

            if (person == null)
                throw new Exception("Person not found.");

            if (person.Invites.Any(x => x.Id == inviteId && x.Status == InviteStatus.Accepted))
                throw new Exception("Invite already accepted");

            person.Apply(new InviteWasAccepted { InviteId = inviteId, IsVeg = isVeg, PersonId = person.Id });
            await this.SaveAsync(person);

            Bbq? bbq = await _bbqRepository.GetAsync(inviteId);
            if (bbq == null)
                throw new Exception("Bbq not found.");

            var @event = new PersonHasConfirmed { BbqID = bbq.Id, PersonID = person.Id, IsVeg = isVeg };
            bbq.Apply(@event);
            await _bbqRepository.SaveAsync(bbq);

            return person;
        }

        // Método para recusar um convite por ID de pessoa e ID de convite
        public async Task<Person?> DeclineInvite(string id, string inviteId)
        {
            var person = await this.GetAsync(id);

            if (person == null)
                throw new Exception("Person not found");

            person.Apply(new InviteWasDeclined { InviteId = inviteId, PersonId = person.Id });
            await this.SaveAsync(person);

            Bbq? bbq = await _bbqRepository.GetAsync(inviteId);
            if (bbq == null)
                throw new Exception("Bbq not found");

            var @event = new InviteWasDeclined() { InviteId = inviteId, PersonId = person.Id };
            bbq.Apply(@event);
            await _bbqRepository.SaveAsync(bbq);

            return person;
        }

        // Método para obter uma pessoa por ID
        public async Task<Person?> GetAsync(string streamId)
        {
            return await _personRepository.GetAsync(streamId);
        }

        // Método para obter o cabeçalho de uma pessoa por ID
        public async Task<StreamHeaderResponse> GetHeaderAsync(string streamId)
        {
            return await _personRepository.GetHeaderAsync(streamId);
        }

        // Método para obter convites de um usuário por ID
        public async Task<Person?> GetInvitesByUserId(string id)
        {
            var person = await this.GetAsync(id);

            if (person == null)
                throw new Exception("Person not found");

            return person;
        }

        // Método para salvar uma pessoa
        public async Task SaveAsync(Person entity)
        {
            await _personRepository.SaveAsync(entity);
        }
    }
}
