using System;
using Game.Core;

namespace Game.Runtime
{
    /// <summary>
    /// One frozen block's look as the level goes on (D102, D104): each exit that wears the ice down shows the lower
    /// count with a shake; at zero the count melts and the block wears its own color again. Reads the logic only when
    /// refreshed, and shows nothing when nothing changed.
    /// </summary>
    public sealed class IceLook : IModifierLook
    {
        private readonly IIceView view;
        private readonly Ice ice;
        private readonly Action thaw;
        private int shown;

        public IceLook(IIceView view, Ice ice, Action thaw)
        {
            this.view = view;
            this.ice = ice;
            this.thaw = thaw;
            shown = ice.Durability.Remaining;
        }

        public void Refresh()
        {
            int remaining = ice.Durability.Remaining;

            if (remaining == shown)
                return;

            shown = remaining;

            if (ice.Durability.IsDepleted)
            {
                view.Melt();
                thaw();
            }
            else
            {
                view.Bump(remaining);
            }
        }
    }
}
