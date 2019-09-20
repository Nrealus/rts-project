using Gamelogic.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Core.Orders
{
    public enum OrderPlacementTypes { First, Second, Last }
    public enum OrderTypes { Null, Movement, Stop, Fire };

    public class OrderContainer
    {

        public OrderTypes orderType;
        public OrderPlacementTypes placement;
        public float targetTime;

        public enum OrderStates { WaitingUntilStartTime, Pending, Paused, Active, FinishedButWaitingForOthersToFinish }
        private Dictionary<OrderReceiver, OrderStates> orderStateForReceiver;
        public Dictionary<OrderReceiver, OrderStates>.KeyCollection orderReceivers { get { return orderStateForReceiver.Keys; } }

        public OrderArgs orderArguments;

        public List<OrderModifier> modifiers;

        /*public OrderContainer Copy()
        {
            OrderContainer oc = new OrderContainer(orderType, placement, modifiers, orderReceivers, orderData, targetTime);
            return oc;
        }*/

        public OrderContainer(OrderTypes orderType, OrderPlacementTypes placement, float targetTime, List<OrderReceiver> orderReceivers, OrderArgs orderArguments, List<OrderModifier> modifiers)
        {

            this.orderType = orderType;
            this.placement = placement;
            this.targetTime = targetTime;

            orderStateForReceiver = new Dictionary<OrderReceiver, OrderStates>();
            foreach (OrderReceiver or in orderReceivers)
            {
                orderStateForReceiver.Add(or, OrderStates.WaitingUntilStartTime);
            }

            this.orderArguments = orderArguments;//.Copy();
            this.modifiers = modifiers;// new List<OrderModifier>(modifiers);
            //this.orderReceivers = new List<OrderReceiver>(orderReceivers);

            ActivateOrderForReceivers(); // Testing purposes
        }

        public void SetAsPaused()
        {

        }

        private void ActivateOrderForReceivers()
        {
            foreach (OrderReceiver or in orderReceivers)
            {

                // THERE WILL PROBABLY BE CONDITIONS COMING FROM MODIFIERS
                // for example, checking if all receivers have finished their current orders, if the modifier wants to wait until all receivers are "ready" so they start simultaneously
                orderStateForReceiver[or] = OrderStates.Active;
            }
        }

        private void CheckTargetTimePassed()
        {
            if (true/*Time.time >= targetTime*/) //  TODO : Not Time.time of course but something else
            {
                foreach (OrderReceiver or in orderReceivers)
                {
                    orderStateForReceiver[or] = OrderStates.Pending;
                }
            }
        }

        public OrderStates GetCurrentStateForOrderReceiver(OrderReceiver or)
        {
            return orderStateForReceiver[or];
        }

    }

}
