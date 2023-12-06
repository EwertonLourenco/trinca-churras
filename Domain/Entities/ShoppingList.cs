using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class ShoppingList
    {

        public string PersonID { get; set; }
        public IEnumerable<ShoppingItem> ShoppingItemL { get; set; }
        public ShoppingList() { }
    }


}