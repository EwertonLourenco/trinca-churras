﻿using Domain.Entities; // Entidades do domínio
using Microsoft.Azure.Functions.Worker; // Biblioteca para Azure Functions
using Microsoft.Azure.Functions.Worker.Http;
using Domain.Services; // Serviços do domínio

namespace Serverless_Api
{
    public partial class RunAcceptInvite
    {
        // Declaração de variáveis e serviços necessários para a função
        private readonly Person _user; // Usuário atual
        private readonly IPersonService _personService; // Serviço de pessoa
        private readonly IBbqService _bbqService; // Serviço de churrasco (barbecue)

        // Construtor da classe
        public RunAcceptInvite(IPersonService personService, IBbqService bbqService, Person user)
        {
            _user = user;
            _personService = personService;
            _bbqService = bbqService;
        }

        // Método da função RunAcceptInvite que é acionado por uma solicitação HTTP do tipo PUT
        [Function(nameof(RunAcceptInvite))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "put", Route = "person/invites/{inviteId}/accept")] HttpRequestData req, string inviteId)
        {
            var answer = await req.Body<InviteAnswer>(); // Obtém a resposta do convite do corpo da requisição
            Person? person = null; // Inicializa um objeto de pessoa

            try
            {
                // Verifica se o ID do convite não está vazio
                if (string.IsNullOrEmpty(inviteId))
                    throw new ArgumentNullException("InviteId is null");

                // Aceita o convite para o usuário atual com base na resposta (se é veg ou não)
                person = await _personService.AcceptInvite(_user.Id, inviteId, answer.IsVeg);
            }
            catch (Exception e)
            {
                // Em caso de exceção, retorna uma resposta indicando erro interno (código 500) com a mensagem de erro
                return await req.CreateResponse(System.Net.HttpStatusCode.InternalServerError, new { Message = e.Message, Type = "Error" });
            }

            // Retorna uma resposta indicando sucesso (código 200 - OK) com o snapshot da pessoa atualizada
            return await req.CreateResponse(System.Net.HttpStatusCode.OK, person.TakeSnapshot());
        }
    }
}
