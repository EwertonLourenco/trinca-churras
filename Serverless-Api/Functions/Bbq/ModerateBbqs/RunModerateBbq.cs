using Domain.Entities; // Entidades do dom�nio
using Domain.Services; // Servi�os do dom�nio
using Microsoft.Azure.Functions.Worker; // Biblioteca para Azure Functions
using Microsoft.Azure.Functions.Worker.Http;

namespace Serverless_Api
{
    public partial class RunModerateBbq
    {
        // Declara��o de vari�veis e servi�os necess�rios para a fun��o
        private readonly IPersonService _personService; // Servi�o de pessoa
        private readonly IBbqService _bbqService; // Servi�o de churrasco (barbecue)

        // Construtor da classe
        public RunModerateBbq(IBbqService bbqService, IPersonService personService)
        {
            _bbqService = bbqService;
            _personService = personService;
        }

        // M�todo da fun��o RunModerateBbq que � acionado por uma solicita��o HTTP do tipo PUT
        [Function(nameof(RunModerateBbq))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "put", Route = "churras/{id}/moderar")] HttpRequestData req, string id)
        {
            try
            {
                // Recebe e desserializa o corpo da requisi��o HTTP em um objeto ModerateBbqRequest
                var moderationRequest = await req.Body<ModerateBbqRequest>();

                // Modera o churrasco (barbecue) com base nos dados recebidos
                Bbq? bbq = await _bbqService.Moderate(id, moderationRequest.GonnaHappen, moderationRequest.TrincaWillPay);

                // Retorna uma resposta indicando sucesso (c�digo 200 - OK) e o snapshot do churrasco modificado
                return await req.CreateResponse(System.Net.HttpStatusCode.OK, bbq.TakeSnapshot());
            }
            catch (Exception e)
            {
                // Em caso de exce��o, retorna uma resposta indicando erro interno (c�digo 500) com a mensagem de erro
                return await req.CreateResponse(System.Net.HttpStatusCode.InternalServerError, new { Message = e.Message, Type = "Error" });
            }
        }
    }
}
