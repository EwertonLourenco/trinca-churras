using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Events;
using Newtonsoft.Json;

namespace Domain.Entities
{

    public class Bbq : AggregateRoot
    {
        public IEnumerable<Person> ConfirmedL { get; set; }
        public IEnumerable<ShoppingList> ShoppingL { get; set; }


        [JsonIgnore]
        public decimal TotalMeat
        {
            get
            {

                if (ShoppingL != null && ShoppingL.Count() > 0)
                {
                    decimal qtTotal = 0;
                    foreach (ShoppingList item in ShoppingL)
                    {
                        foreach (ShoppingItem shoppingItem in item.ShoppingItemL)
                        {
                            if (shoppingItem.FoodType == FoodType.Meat)
                                qtTotal += shoppingItem.QuantityG;
                        }
                    }

                    return qtTotal / 1000;
                }

                return 0;
            }
        }

        [JsonIgnore]
        public decimal TotalVeg
        {
            get
            {
                if (ShoppingL != null && ShoppingL.Count() > 0)
                {
                    decimal qtTotal = 0;
                    foreach (ShoppingList item in ShoppingL)
                    {
                        foreach (ShoppingItem shoppingItem in item.ShoppingItemL)
                        {
                            if (shoppingItem.FoodType == FoodType.Vegetable)
                                qtTotal += shoppingItem.QuantityG;
                        }
                    }

                    return qtTotal / 1000;
                }

                return 0;
            }
        }

        public Bbq()
        {
            ConfirmedL = new List<Person>();
            ShoppingL = new List<ShoppingList>();
        }

        public string Reason { get; set; }
        public BbqStatus Status { get; set; }
        public DateTime Date { get; set; }
        public bool IsTrincasPaying { get; set; }
        public void When(ThereIsSomeoneElseInTheMood @event)
        {
            Id = @event.Id.ToString();
            Date = @event.Date;
            Reason = @event.Reason;
            Status = BbqStatus.New;
        }

        public void When(BbqStatusUpdated @event)
        {
            if (@event.GonnaHappen)
                Status = BbqStatus.PendingConfirmations;
            else
                Status = BbqStatus.ItsNotGonnaHappen;

            if (@event.TrincaWillPay)
                IsTrincasPaying = true;
        }

        public void When(InviteWasDeclined @event)
        {
            //TODO:Deve ser possível rejeitar um convite já aceito antes.
            //Se este for o caso, a quantidade de comida calculada pelo aceite anterior do convite
            //deve ser retirado da lista de compras do churrasco.
            //Se ao rejeitar, o número de pessoas confirmadas no churrasco for menor que sete,
            //o churrasco deverá ter seu status atualizado para “Pendente de confirmações”.

            Person? person = this.ConfirmedL.FirstOrDefault(x => x.Id == @event.PersonId);
            if (person != null)
            {
                ConfirmedL = ConfirmedL.Where(x => x.Id != @event.PersonId);
                ShoppingL = ShoppingL.Where(x => x.PersonID != @event.PersonId);
            }

            if (ConfirmedL.Count() < 7)
                Status = BbqStatus.PendingConfirmations;
        }

        public void When(PersonHasConfirmed @event)
        {
            var person = ConfirmedL.FirstOrDefault(x => x.Id == @event.PersonID);
            if (person == null)
                ConfirmedL = ConfirmedL.Append(new Person { Id = @event.PersonID });

            if (@event.IsVeg)
            {
                ShoppingL = ShoppingL.Append(new ShoppingList()
                {
                    PersonID = @event.PersonID,
                    ShoppingItemL = new List<ShoppingItem>() {
                        new ShoppingItem() {FoodType = FoodType.Vegetable,  QuantityG = 600},
                    }
                });
            }
            else
            {
                ShoppingL = ShoppingL.Append(new ShoppingList()
                {
                    PersonID = @event.PersonID,
                    ShoppingItemL = new List<ShoppingItem>() {
                        new ShoppingItem() {FoodType = FoodType.Meat,  QuantityG = 300},
                        new ShoppingItem() {FoodType = FoodType.Vegetable,  QuantityG = 300}
                    }
                });
            }

            //quando tiver 7 pessoas ele está confirmado.
            if (ConfirmedL.Count() >= 7)
                Status = BbqStatus.Confirmed;
        }

        public object TakeSnapshot()
        {
            return new
            {
                Id,
                Date,
                IsTrincasPaying,
                Status = Status.ToString()
            };
        }
    }
}
