using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Events
{
    public class PersonHasConfirmed : IEvent
    {

        public string PersonID { get; set; }
        public string BbqID { get; set; }
        public bool IsVeg { get; set; }
    }
}