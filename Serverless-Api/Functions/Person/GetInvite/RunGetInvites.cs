using Domain.Entities; // Entidades do dom�nio
using Domain.Services; // Servi�os do dom�nio
using Microsoft.Azure.Functions.Worker; // Biblioteca para Azure Functions
using Microsoft.Azure.Functions.Worker.Http;

namespace Serverless_Api
{
    public partial class RunGetInvites
    {
        // Declara��o de vari�veis e servi�os necess�rios para a fun��o
        private readonly Person _user; // Usu�rio atual
        private readonly IPersonService _personService; // Servi�o de pessoa

        // Construtor da classe
        public RunGetInvites(Person user, IPersonService personService)
        {
            _user = user;
            _personService = personService;
        }

        // M�todo da fun��o RunGetInvites que � acionado por uma solicita��o HTTP do tipo GET
        [Function(nameof(RunGetInvites))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "person/invites")] HttpRequestData req)
        {
            Person? person = null; // Inicializa um objeto de pessoa

            try
            {
                // Obt�m os convites para o usu�rio atual com base no seu ID
                person = await _personService.GetInvitesByUserId(_user.Id);
            }
            catch (Exception e)
            {
                // Em caso de exce��o, retorna uma resposta indicando erro interno (c�digo 500) com a mensagem de erro
                return await req.CreateResponse(System.Net.HttpStatusCode.InternalServerError, new { Message = e.Message, Type = "Error" });
            }

            // Retorna uma resposta indicando sucesso (c�digo 200 - OK) com o snapshot dos convites obtidos para o usu�rio atual
            return await req.CreateResponse(System.Net.HttpStatusCode.OK, person.TakeSnapshot());
        }
    }
}
