using CrossCutting; // Funcionalidades auxiliares
using Domain.Entities; // Entidades do domínio
using Domain.Events; // Eventos do domínio
using Domain.Repositories; // Repositórios do domínio
using Eveneum; // Biblioteca para lidar com snapshots
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Services
{
    public class BbqService : IBbqService
    {
        private readonly SnapshotStore _snapshots; // Armazenamento de snapshots
        private readonly IBbqRepository _bbqRepository; // Repositório de churrasco
        private readonly IPersonRepository _personRepository; // Repositório de pessoa

        // Construtor da classe
        public BbqService(SnapshotStore snapshotStore, IBbqRepository bbqRepository, IPersonRepository personRepository)
        {
            _bbqRepository = bbqRepository;
            _personRepository = personRepository;
            _snapshots = snapshotStore;
        }

        // Método para obter todos os churrascos não rejeitados ou disponíveis para um usuário
        public async Task<List<object>> GetAllNotRejectedOrAvailable(string userId)
        {
            var snapshots = new List<object>();
            var moderator = await _personRepository.GetAsync(userId);
            foreach (var bbqId in moderator.Invites.Where(i => i.Date > DateTime.Now).Select(o => o.Id).ToList())
            {
                var bbq = await this.GetAsync(bbqId);
                if (bbq == null)
                    throw new Exception("Bbq not found.");

                if (bbq.Status != BbqStatus.ItsNotGonnaHappen)
                    snapshots.Add(bbq.TakeSnapshot());
            }

            return snapshots;
        }

        // Método para obter um churrasco por ID
        public async Task<Bbq?> GetAsync(string streamId)
        {
            return await _bbqRepository.GetAsync(streamId);
        }

        // Método para obter o cabeçalho de um churrasco por ID
        public async Task<StreamHeaderResponse> GetHeaderAsync(string streamId)
        {
            return await _bbqRepository.GetHeaderAsync(streamId);
        }

        // Método para obter a lista de compras de um churrasco por ID
        public async Task<Bbq?> GetShoppingListByBbqId(string userId, string bbqId)
        {
            Person? person = await _personRepository.GetAsync(userId);
            if (person == null)
                throw new NullReferenceException("Person not found.");

            if (!person.IsCoOwner)
                throw new Exception("Person is not 'CoOwner'");

            Bbq? bbq = await this.GetAsync(bbqId);
            if (bbq == null)
                throw new Exception("Bbq not found.");

            return bbq;
        }

        // Método para moderar um churrasco (alterar o status e enviar convites ou rejeitar convites)
        public async Task<Bbq?> Moderate(string id, bool gonnaHappen, bool trincaWillPay)
        {
            var bbq = await this.GetAsync(id);
            if (bbq == null)
                throw new Exception("Bbq not found.");

            bbq.Apply(new BbqStatusUpdated(gonnaHappen, trincaWillPay));

            var lookups = await _snapshots.AsQueryable<Lookups>("Lookups").SingleOrDefaultAsync();

            // Não enviar convites quando o churrasco não vai acontecer.
            if (!(bbq.Status == BbqStatus.ItsNotGonnaHappen))
                await SendInvites(bbq, lookups);
            else
                await RejectInvites(bbq, lookups);

            await this.SaveAsync(bbq);
            return bbq;
        }

        // Método para rejeitar convites de um churrasco
        private async Task RejectInvites(Bbq? bbq, Lookups lookups)
        {
            // Rejeita todos os convites
            foreach (var personId in lookups.PeopleIds)
            {
                var person = await _personRepository.GetAsync(personId);
                Invite invite = person.Invites.FirstOrDefault(x => x.Id == bbq.Id);
                if (invite != null)
                {
                    var @event = new InviteWasDeclined()
                    {
                        PersonId = personId,
                        InviteId = invite.Id
                    };

                    person.Apply(@event);
                    await _personRepository.SaveAsync(person);
                }
            }
        }

        // Método para enviar convites para um churrasco
        private async Task SendInvites(Bbq? bbq, Lookups lookups)
        {
            // Envia convites
            foreach (var personId in lookups.PeopleIds)
            {
                var person = await _personRepository.GetAsync(personId);
                if (person != null && !lookups.ModeratorIds.Any(x => x.Equals(person.Id)))
                {
                    var @event = new PersonHasBeenInvitedToBbq(bbq.Id, bbq.Date, bbq.Reason);
                    person.Apply(@event);
                    await _personRepository.SaveAsync(person);
                }
            }
        }

        // Método para salvar um churrasco
        public async Task SaveAsync(Bbq entity)
        {
            await _bbqRepository.SaveAsync(entity);
        }

        // Método para criar um novo churrasco
        public async Task<Bbq?> CreateNew(DateTime date, string reason, bool isTrincasPaying)
        {
            var churras = new Bbq();
            churras.Apply(new ThereIsSomeoneElseInTheMood(Guid.NewGuid(), date, reason, isTrincasPaying));
            await this.SaveAsync(churras);

            // Convida moderadores
            var Lookups = await _snapshots.AsQueryable<Lookups>("Lookups").SingleOrDefaultAsync();
            foreach (var personId in Lookups.ModeratorIds)
            {
                Person? person = await _personRepository.GetAsync(personId);
                var @event = new PersonHasBeenInvitedToBbq(churras.Id, churras.Date, churras.Reason);
                person.Apply(@event);

                await _personRepository.SaveAsync(person);
            }

            return churras;
        }
    }
}
