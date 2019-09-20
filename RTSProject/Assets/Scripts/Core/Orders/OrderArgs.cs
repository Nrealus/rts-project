using Gamelogic.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.AI;

namespace Core.Orders
{

    public class OrderArgs
    {
        public virtual OrderArgs Copy()
        {
            return new OrderArgs();
        }

        public OrderArgs()
        {
        }

    }

}