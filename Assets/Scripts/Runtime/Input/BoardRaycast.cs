using Game.Core;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// Pointer → board without colliders (FINDINGS): a ray onto the ground plane, divided by the cell size.
    /// Assumes the board root sits at the world origin, unrotated, as the installer builds it.
    /// </summary>
    public static class BoardRaycast
    {
        /// <summary>Where the ray meets the ground, in cell units (fractions kept). False if it never does.</summary>
        public static bool TryGetCellPoint(Ray ray, out Vector2 cellPoint)
        {
            var ground = new Plane(Vector3.up, Vector3.zero);

            if (!ground.Raycast(ray, out float distance))
            {
                cellPoint = default;
                return false;
            }

            Vector3 hit = ray.GetPoint(distance);
            cellPoint = new Vector2(hit.x / BoardLayout.CellSize, hit.z / BoardLayout.CellSize);

            return true;
        }

        /// <summary>The cell a point in cell units falls into.</summary>
        public static Cell ToCell(Vector2 cellPoint) =>
            new Cell(Mathf.FloorToInt(cellPoint.x), Mathf.FloorToInt(cellPoint.y));
    }
}
