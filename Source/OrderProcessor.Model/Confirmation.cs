﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessor.Model
{
    public class Confirmation  
    {
        public int OrderId { get; set; }

        public Guid AgentId { get; set; }

        public string OrderStatus { get; set; }
    }
}
