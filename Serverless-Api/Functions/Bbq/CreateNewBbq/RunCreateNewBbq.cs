using System.Net;
using CrossCutting; // Biblioteca para funcionalidades auxiliares
using Domain.Entities; // Entidades do dom�nio
using Microsoft.Azure.Functions.Worker; // Biblioteca para Azure Functions
using Microsoft.Azure.Functions.Worker.Http;
using Domain.Services; // Servi�os do dom�nio

namespace Serverless_Api
{
    public partial class RunCreateNewBbq
    {
        // Declara��o de vari�veis e servi�os necess�rios para a fun��o
        private readonly Person _user; // Usu�rio atual
        private readonly SnapshotStore _snapshots; // Armazenamento de snapshots
        private readonly IPersonService _personService; // Servi�o de pessoa
        private readonly IBbqService _bbqService; // Servi�o de churrasco (barbecue)

        // Construtor da classe
        public RunCreateNewBbq(IPersonService personService, IBbqService bbqService, SnapshotStore snapshots, Person user)
        {
            _user = user;
            _snapshots = snapshots;
            _personService = personService;
            _bbqService = bbqService;
        }

        // M�todo da fun��o RunCreateNewBbq que � acionado por uma solicita��o HTTP
        [Function(nameof(RunCreateNewBbq))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "churras")] HttpRequestData req)
        {
            try
            {
                // Recebe e desserializa o corpo da requisi��o HTTP em um objeto NewBbqRequest
                var input = await req.Body<NewBbqRequest>();
                if (input == null)
                    return await req.CreateResponse(HttpStatusCode.BadRequest, "input is required."); // Retorna um erro se o corpo da requisi��o estiver vazio

                // Cria um novo churrasco (barbecue) com base nos dados recebidos
                Bbq? bbq = await _bbqService.CreateNew(input.Date, input.Reason, input.IsTrincasPaying);

                // Gera um snapshot do churrasco criado
                var churrasSnapshot = bbq.TakeSnapshot();

                // Retorna uma resposta indicando sucesso (c�digo 201 - Created) e o snapshot do churrasco
                return await req.CreateResponse(HttpStatusCode.Created, churrasSnapshot);
            }
            catch (Exception e)
            {
                // Em caso de exce��o, retorna uma resposta indicando erro interno (c�digo 500) com a mensagem de erro
                return await req.CreateResponse(System.Net.HttpStatusCode.InternalServerError, new { Message = e.Message, Type = "Error" });
            }
        }
    }
}
