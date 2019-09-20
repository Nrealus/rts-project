using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.AI;

namespace Core.Orders
{
    public class OrderReceiver : MonoBehaviour
    {

        public List<OrderContainer> orderContainerList;
        public OrderContainer currentOrderContainer;

        //public Intelligence intel;

        private void Awake()
        {
            orderContainerList = new List<OrderContainer>();
        }

        private void Start()
        {
            //intel = GetComponent<Intelligence>();
        }

        private void Update()
        {
            OrderContainersProcessing();
        }

        private OrderContainer previousOc;
        private void OrderContainersProcessing()
        {
            bool b = false;
            foreach (OrderContainer oc in orderContainerList)
            {
                if (oc.GetCurrentStateForOrderReceiver(this) == OrderContainer.OrderStates.Active)
                {
                    b = true;
                    if (oc != previousOc)
                    {
                        previousOc = oc;
                        currentOrderContainer = oc;
                        //intel.SetCurrentDecisionMaker(intel.GetDecisionMakerFromOrderType(oc.orderType));
                    }
                    break;
                }
            }
            if (!b)
            {
                currentOrderContainer = null;
                //intel.SetCurrentDecisionMaker(null);
            }
        }

        public void AddOrderContainerAtAppropriatePosition(OrderContainer oc)
        {
            switch(oc.placement)
            {
                case OrderPlacementTypes.First:
                    orderContainerList.Insert(0, oc);
                    break;
                /*case OrderPlacementTypes.Second:
                    break;*/
                case OrderPlacementTypes.Last:
                    orderContainerList.Add(oc);
                    break;
            }

        }

    }
}