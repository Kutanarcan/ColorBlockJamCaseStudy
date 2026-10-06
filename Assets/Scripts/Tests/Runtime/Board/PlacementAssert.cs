using System.Collections.Generic;
using Game.Runtime;
using UnityEngine;

namespace Game.Tests.Runtime
{
    /// <summary>Test-only: finds a placement by position and yaw, within float tolerance.</summary>
    internal static class PlacementAssert
    {
        public static bool Has(IReadOnlyList<Placement> placements, Vector3 position, float yaw)
        {
            for (int i = 0; i < placements.Count; i++)
            {
                if (Is(placements[i], position, yaw))
                    return true;
            }

            return false;
        }

        public static bool Is(Placement placement, Vector3 position, float yaw) =>
            placement.Position == position && Mathf.Approximately(placement.Yaw, yaw);
    }
}
