using System.Collections.Generic;
using Geta.Klarna.Checkout.Models;

namespace Geta.Klarna.Checkout.Extensions
{
    internal static class GuiExtensions
    {
        internal static Dictionary<string, object> ToDictionary(this Gui gui)
        {
            var guiData = new Dictionary<string, object>();

            if (gui.DisableAutoFocus)
            {
                guiData.Add("options", new[] { "disable_autofocus" });
            }

            return guiData;
        }
    }
}
