using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class ShoppingItem
    {
        public FoodType FoodType { get; set; }
        public int QuantityG { get; set; }
    }

    public enum FoodType
    {
        Meat = 1,
        Vegetable = 2
    }
}