using System;

namespace RadioController
{
	public partial class ShowGUI : Gtk.Window
	{
		public ShowGUI () : 
				base(Gtk.WindowType.Toplevel)
		{
			this.Build ();
		}
	}
}

