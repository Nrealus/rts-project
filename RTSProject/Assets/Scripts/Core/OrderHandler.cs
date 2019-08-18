using System;
using UnityEngine;
using Core.Orders;

namespace Core
{
    public class OrderHandler : MonoBehaviour
    {

        public void Init()
        {

        }

        private void Update()
        {

        }

        public void DispatchOrderContainerToReceivers(OrderContainer oc)
        {
            foreach(OrderReceiver p in oc.orderReceivers)
            {
                p.AddOrderContainerAtAppropriatePosition(oc.Copy());
            }
        }

    }
}