using System.Net; // Biblioteca para c�digos HTTP
using Domain.Entities; // Entidades do dom�nio
using Domain.Services; // Servi�os do dom�nio
using Microsoft.Azure.Functions.Worker; // Biblioteca para Azure Functions
using Microsoft.Azure.Functions.Worker.Http;

namespace Serverless_Api
{
    public partial class RunGetProposedBbqs
    {
        // Declara��o de vari�veis e servi�os necess�rios para a fun��o
        private readonly Person _user; // Usu�rio atual
        private readonly IPersonService _personService; // Servi�o de pessoa
        private readonly IBbqService _bbqService; // Servi�o de churrasco (barbecue)

        // Construtor da classe
        public RunGetProposedBbqs(IPersonService personService, IBbqService bbqService, Person user)
        {
            _user = user;
            _personService = personService;
            _bbqService = bbqService;
        }

        // M�todo da fun��o RunGetProposedBbqs que � acionado por uma solicita��o HTTP do tipo GET
        [Function(nameof(RunGetProposedBbqs))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "churras")] HttpRequestData req)
        {
            var snapshots = new List<object>(); // Lista para armazenar snapshots

            try
            {
                // Obt�m todos os churrascos (barbecues) n�o rejeitados ou dispon�veis para o usu�rio atual
                snapshots = await _bbqService.GetAllNotRejectedOrAvailable(_user.Id);
            }
            catch (Exception e)
            {
                // Em caso de exce��o, retorna uma resposta indicando erro interno (c�digo 500) com a mensagem de erro
                return await req.CreateResponse(System.Net.HttpStatusCode.InternalServerError, new { Message = e.Message, Type = "Error" });
            }

            // Retorna uma resposta indicando sucesso (c�digo 201 - Created) e os snapshots dos churrascos obtidos
            return await req.CreateResponse(HttpStatusCode.Created, snapshots);
        }
    }
}
