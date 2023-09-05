using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DockingFunctions
{
	public static class DockingEvents
	{
	public static EventData<IDockable, IDockable> onVesselDocking = new EventData<IDockable, IDockable>("onVesselDocking");
	public static EventData<IDockable, IDockable> onSameVesselDocking = new EventData<IDockable, IDockable>("onSameVesselDocking");

	public static EventData<IDockable, IDockable> onVesselUndocking = new EventData<IDockable, IDockable>("onVesselUndocking");
	public static EventData<IDockable, IDockable> onSameVesselUndocking = new EventData<IDockable, IDockable>("onSameVesselUndocking");

	public static EventData<IDockable, IDockable> onVesselDocked = new EventData<IDockable, IDockable>("onVesselDocked");
	public static EventData<IDockable, IDockable> onSameVesselDocked = new EventData<IDockable, IDockable>("onSameVesselDocked");

	public static EventData<IDockable, IDockable> onVesselUndocked = new EventData<IDockable, IDockable>("onVesselUndocked");
	public static EventData<IDockable, IDockable> onSameVesselUndocked = new EventData<IDockable, IDockable>("OnSameVesselUndocked");

	public static EventData<IDockable, IDockable> onDockingSwitching = new EventData<IDockable, IDockable>("onDockingSwitching");
	public static EventData<IDockable, IDockable> onDockingSwitched = new EventData<IDockable, IDockable>("onDockingSwitched");
	}
}
