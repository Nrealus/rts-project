using Gamelogic.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Orders
{

    public class Order
    {

        protected OrderModifier[] modifiers;

        //protected List<OrderReceiver> targetedReceivers;

        public virtual void ProcessMe()
        {

        }
        
        /*public virtual void Init()
        {

        }*/

        public Order(List<OrderModifier> modifiers, /*List<OrderReceiver> targetedReceivers,*/ List<object> arguments)
        {
            modifiers.CopyTo(this.modifiers);
            //this.targetedReceivers = new List<OrderReceiver>(targetedReceivers);
        }

    }

}