namespace Geta.Klarna.Checkout.Models
{
    public class Gui
    {
        public Gui(bool disableAutoFocus)
        {
            DisableAutoFocus = disableAutoFocus;
        }
        public bool DisableAutoFocus { get; private set; }
    }
}
