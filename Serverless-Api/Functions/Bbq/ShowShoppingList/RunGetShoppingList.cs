using Domain.Entities; // Entidades do domínio
using Microsoft.Azure.Functions.Worker.Http; // Biblioteca para lidar com solicitações HTTP
using Microsoft.Azure.Functions.Worker; // Biblioteca para Azure Functions
using Domain.Services; // Serviços do domínio

namespace Serverless_Api
{
    public partial class RunGetShoppingList
    {
        // Declaração de variáveis e serviços necessários para a função
        private readonly Person _user; // Usuário atual
        private readonly IPersonService _personService; // Serviço de pessoa
        private readonly IBbqService _bbqService; // Serviço de churrasco (barbecue)

        // Construtor da classe
        public RunGetShoppingList(Person user, IPersonService personService, IBbqService bbqService)
        {
            _user = user;
            _bbqService = bbqService;
            _personService = personService;
        }

        // Método da função RunGetShoppingList que é acionado por uma solicitação HTTP do tipo GET
        [Function(nameof(RunGetShoppingList))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "churras/{id}/shoppinglist")] HttpRequestData req, string id)
        {
            Bbq? bbq = null; // Inicializa um objeto de churrasco

            try
            {
                // Verifica se o ID do churrasco não está vazio
                if (string.IsNullOrEmpty(id))
                    throw new ArgumentNullException("Bbq id is null.");

                // Obtém a lista de compras do churrasco pelo ID para o usuário atual
                bbq = await _bbqService.GetShoppingListByBbqId(_user.Id, id);
            }
            catch (Exception e)
            {
                // Em caso de exceção, retorna uma resposta indicando erro interno (código 500) com a mensagem de erro
                return await req.CreateResponse(System.Net.HttpStatusCode.InternalServerError, new { Message = e.Message, Type = "Error" });
            }

            // Retorna uma resposta indicando sucesso (código 200 - OK) com a lista de compras e totais de carne e vegetais
            return await req.CreateResponse(System.Net.HttpStatusCode.OK,
                new
                {
                    ShoppingList = bbq.ShoppingL,
                    TotalMeat = bbq.TotalMeat.ToString() + "KG",
                    TotalVeg = bbq.TotalVeg.ToString() + "KG"
                });
        }
    }
}
