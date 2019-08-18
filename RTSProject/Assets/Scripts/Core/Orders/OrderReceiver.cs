using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Orders
{
    public class OrderReceiver : MonoBehaviour
    {

        public List<OrderContainer> orderContainerList;

        private void Awake()
        {
            orderContainerList = new List<OrderContainer>();
        }

        private void Start()
        {

        }

        private void Update()
        {
            OrderContainersProcessing();
        }

        private void OrderContainersProcessing()
        {
            foreach (OrderContainer oc in orderContainerList)
            {
                if(oc.GetCurrentState() == OrderContainer.States.Active)
                {
                    oc.order.ProcessMe();
                }

                // TODO : FOR NOW, this will do. But this will definitely change with gameplay requirements and enforced rules.
            }
        }

        public void AddOrderContainerAtAppropriatePosition(OrderContainer oc)
        {
            switch(oc.placement)
            {
                case OrderContainer.PlacementTypes.First:
                    orderContainerList.Insert(0, oc);
                    break;
                /*case OrderContainer.PlacementTypes.Second:
                    break;*/
                case OrderContainer.PlacementTypes.Last:
                    orderContainerList.Add(oc);
                    break;
            }

        }

    }
}