using Gamelogic.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.AI;

namespace Core.Orders
{

    public class MovementOrderArgs : OrderArgs
    {
        public List<Vector3> waypoints;
        public bool hasSpecificLookAtDirection;
        public Vector3 specificLookAtDirection;

        public override OrderArgs Copy()
        {
            return new MovementOrderArgs(waypoints, hasSpecificLookAtDirection, specificLookAtDirection);
        }

        public MovementOrderArgs(List<Vector3> waypoints, bool hasSpecificLookAtDirection, Vector3 specificLookAtDirection)
        {
            this.waypoints = new List<Vector3>(waypoints);
            this.hasSpecificLookAtDirection = hasSpecificLookAtDirection;
            this.specificLookAtDirection = specificLookAtDirection;
        }

    }

}