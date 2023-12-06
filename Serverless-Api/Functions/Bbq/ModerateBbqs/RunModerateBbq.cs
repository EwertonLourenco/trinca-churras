using Domain.Entities; // Entidades do domínio
using Domain.Services; // Serviços do domínio
using Microsoft.Azure.Functions.Worker; // Biblioteca para Azure Functions
using Microsoft.Azure.Functions.Worker.Http;

namespace Serverless_Api
{
    public partial class RunModerateBbq
    {
        // Declaração de variáveis e serviços necessários para a função
        private readonly IPersonService _personService; // Serviço de pessoa
        private readonly IBbqService _bbqService; // Serviço de churrasco (barbecue)

        // Construtor da classe
        public RunModerateBbq(IBbqService bbqService, IPersonService personService)
        {
            _bbqService = bbqService;
            _personService = personService;
        }

        // Método da função RunModerateBbq que é acionado por uma solicitação HTTP do tipo PUT
        [Function(nameof(RunModerateBbq))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "put", Route = "churras/{id}/moderar")] HttpRequestData req, string id)
        {
            try
            {
                // Recebe e desserializa o corpo da requisição HTTP em um objeto ModerateBbqRequest
                var moderationRequest = await req.Body<ModerateBbqRequest>();

                // Modera o churrasco (barbecue) com base nos dados recebidos
                Bbq? bbq = await _bbqService.Moderate(id, moderationRequest.GonnaHappen, moderationRequest.TrincaWillPay);

                // Retorna uma resposta indicando sucesso (código 200 - OK) e o snapshot do churrasco modificado
                return await req.CreateResponse(System.Net.HttpStatusCode.OK, bbq.TakeSnapshot());
            }
            catch (Exception e)
            {
                // Em caso de exceção, retorna uma resposta indicando erro interno (código 500) com a mensagem de erro
                return await req.CreateResponse(System.Net.HttpStatusCode.InternalServerError, new { Message = e.Message, Type = "Error" });
            }
        }
    }
}
