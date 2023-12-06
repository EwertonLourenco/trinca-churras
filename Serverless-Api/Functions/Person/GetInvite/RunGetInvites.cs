using Domain.Entities; // Entidades do domínio
using Domain.Services; // Serviços do domínio
using Microsoft.Azure.Functions.Worker; // Biblioteca para Azure Functions
using Microsoft.Azure.Functions.Worker.Http;

namespace Serverless_Api
{
    public partial class RunGetInvites
    {
        // Declaração de variáveis e serviços necessários para a função
        private readonly Person _user; // Usuário atual
        private readonly IPersonService _personService; // Serviço de pessoa

        // Construtor da classe
        public RunGetInvites(Person user, IPersonService personService)
        {
            _user = user;
            _personService = personService;
        }

        // Método da função RunGetInvites que é acionado por uma solicitação HTTP do tipo GET
        [Function(nameof(RunGetInvites))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "person/invites")] HttpRequestData req)
        {
            Person? person = null; // Inicializa um objeto de pessoa

            try
            {
                // Obtém os convites para o usuário atual com base no seu ID
                person = await _personService.GetInvitesByUserId(_user.Id);
            }
            catch (Exception e)
            {
                // Em caso de exceção, retorna uma resposta indicando erro interno (código 500) com a mensagem de erro
                return await req.CreateResponse(System.Net.HttpStatusCode.InternalServerError, new { Message = e.Message, Type = "Error" });
            }

            // Retorna uma resposta indicando sucesso (código 200 - OK) com o snapshot dos convites obtidos para o usuário atual
            return await req.CreateResponse(System.Net.HttpStatusCode.OK, person.TakeSnapshot());
        }
    }
}
