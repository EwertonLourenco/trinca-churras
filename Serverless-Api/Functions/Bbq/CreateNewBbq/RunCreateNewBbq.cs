using System.Net;
using CrossCutting; // Biblioteca para funcionalidades auxiliares
using Domain.Entities; // Entidades do domínio
using Microsoft.Azure.Functions.Worker; // Biblioteca para Azure Functions
using Microsoft.Azure.Functions.Worker.Http;
using Domain.Services; // Serviços do domínio

namespace Serverless_Api
{
    public partial class RunCreateNewBbq
    {
        // Declaração de variáveis e serviços necessários para a função
        private readonly Person _user; // Usuário atual
        private readonly SnapshotStore _snapshots; // Armazenamento de snapshots
        private readonly IPersonService _personService; // Serviço de pessoa
        private readonly IBbqService _bbqService; // Serviço de churrasco (barbecue)

        // Construtor da classe
        public RunCreateNewBbq(IPersonService personService, IBbqService bbqService, SnapshotStore snapshots, Person user)
        {
            _user = user;
            _snapshots = snapshots;
            _personService = personService;
            _bbqService = bbqService;
        }

        // Método da função RunCreateNewBbq que é acionado por uma solicitação HTTP
        [Function(nameof(RunCreateNewBbq))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "churras")] HttpRequestData req)
        {
            try
            {
                // Recebe e desserializa o corpo da requisição HTTP em um objeto NewBbqRequest
                var input = await req.Body<NewBbqRequest>();
                if (input == null)
                    return await req.CreateResponse(HttpStatusCode.BadRequest, "input is required."); // Retorna um erro se o corpo da requisição estiver vazio

                // Cria um novo churrasco (barbecue) com base nos dados recebidos
                Bbq? bbq = await _bbqService.CreateNew(input.Date, input.Reason, input.IsTrincasPaying);

                // Gera um snapshot do churrasco criado
                var churrasSnapshot = bbq.TakeSnapshot();

                // Retorna uma resposta indicando sucesso (código 201 - Created) e o snapshot do churrasco
                return await req.CreateResponse(HttpStatusCode.Created, churrasSnapshot);
            }
            catch (Exception e)
            {
                // Em caso de exceção, retorna uma resposta indicando erro interno (código 500) com a mensagem de erro
                return await req.CreateResponse(System.Net.HttpStatusCode.InternalServerError, new { Message = e.Message, Type = "Error" });
            }
        }
    }
}
