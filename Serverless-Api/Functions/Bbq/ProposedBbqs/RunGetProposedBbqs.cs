using System.Net; // Biblioteca para códigos HTTP
using Domain.Entities; // Entidades do domínio
using Domain.Services; // Serviços do domínio
using Microsoft.Azure.Functions.Worker; // Biblioteca para Azure Functions
using Microsoft.Azure.Functions.Worker.Http;

namespace Serverless_Api
{
    public partial class RunGetProposedBbqs
    {
        // Declaração de variáveis e serviços necessários para a função
        private readonly Person _user; // Usuário atual
        private readonly IPersonService _personService; // Serviço de pessoa
        private readonly IBbqService _bbqService; // Serviço de churrasco (barbecue)

        // Construtor da classe
        public RunGetProposedBbqs(IPersonService personService, IBbqService bbqService, Person user)
        {
            _user = user;
            _personService = personService;
            _bbqService = bbqService;
        }

        // Método da função RunGetProposedBbqs que é acionado por uma solicitação HTTP do tipo GET
        [Function(nameof(RunGetProposedBbqs))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "churras")] HttpRequestData req)
        {
            var snapshots = new List<object>(); // Lista para armazenar snapshots

            try
            {
                // Obtém todos os churrascos (barbecues) não rejeitados ou disponíveis para o usuário atual
                snapshots = await _bbqService.GetAllNotRejectedOrAvailable(_user.Id);
            }
            catch (Exception e)
            {
                // Em caso de exceção, retorna uma resposta indicando erro interno (código 500) com a mensagem de erro
                return await req.CreateResponse(System.Net.HttpStatusCode.InternalServerError, new { Message = e.Message, Type = "Error" });
            }

            // Retorna uma resposta indicando sucesso (código 201 - Created) e os snapshots dos churrascos obtidos
            return await req.CreateResponse(HttpStatusCode.Created, snapshots);
        }
    }
}
