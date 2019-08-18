using Gamelogic.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Core.Orders
{

    public class OrderContainer
    {
        public List<OrderModifier> modifiers;

        public List<OrderReceiver> orderReceivers;

        public List<object> arguments;

        public float targetTime;

        public enum States { Pending, WaitingUntilDelay, Paused, Active, Cancelled }
        private StateMachine<States> stateMachine;

        public enum PlacementTypes { First, Second, Last }
        public PlacementTypes placement;

        public System.Type orderType;
        public Order order;

        public OrderContainer Copy()
        {
            OrderContainer oc = new OrderContainer(orderType, modifiers, orderReceivers, arguments, targetTime);
            return oc;
        }

        public OrderContainer(System.Type orderType, List<OrderModifier> modifiers, List<OrderReceiver> orderReceivers, List<object> arguments, float targetTime)
        {

            this.modifiers = new List<OrderModifier>(modifiers);
            this.orderReceivers = new List<OrderReceiver>(orderReceivers);
            this.arguments = new List<object>(arguments);
            this.targetTime = targetTime;

            stateMachine = new StateMachine<States>();
            stateMachine.AddState(States.Pending);
            stateMachine.AddState(States.WaitingUntilDelay, null, WaitingDelayUpdate);
            stateMachine.AddState(States.Active, InstantiateOrder);
            stateMachine.AddState(States.Cancelled, DestroyOrder);

            StartAsPending();
        }

        public void StartAsPending()
        {
            stateMachine.CurrentState = States.Pending;
        }

        public void FromPendingToCountdown()
        {
            stateMachine.CurrentState = States.WaitingUntilDelay;
        }

        public void SetAsPaused()
        {

        }

        private void WaitingDelayUpdate()
        {
            if (Time.time <= targetTime) //  TODO : this should and will be obviously changed
            {
                stateMachine.CurrentState = States.Active;
            }
        }

        private void DestroyOrder()
        {
            order = null;
        }

        private void InstantiateOrder()
        {
            //order = new Order(modifiers, /*targetedReceivers,*/ arguments);

            var constructorInfo = orderType.GetConstructor(new[] { typeof(List<OrderModifier>), typeof(List<OrderReceiver>) });
            if (constructorInfo != null)
            {
                object[] parameters = new object[] { modifiers, orderReceivers };
                order = (Order)constructorInfo.Invoke(parameters);
            }

            //order = ((T)new Order(modifiers, arguments));
            //order.Init();
        }

        public States GetCurrentState()
        {
            return stateMachine.CurrentState;
        }

    }
}
