using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Orders
{

    public class MovementOrder : Order
    {

        public List<Vector3> waypoints;

        public MovementOrder(List<OrderModifier> modifiers, List<object> arguments) : base(modifiers, arguments)
        {
            waypoints = new List<Vector3>((List<Vector3>)arguments[0]);
        }

    }

}