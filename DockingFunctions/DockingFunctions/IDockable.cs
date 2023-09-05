using System;
using UnityEngine;

namespace DockingFunctions
{
	public class DockInfo
	{
		public IDockable part;
		public IDockable targetPart;

		public DockedVesselInfo vesselInfo;
		public DockedVesselInfo targetVesselInfo;

		public bool isSameVesselJoint;
		public ConfigurableJoint sameVesselJoint;
	};

	public interface IDockable
	{
		// used by the system to perform docking

		Part GetPart();
		Transform GetNodeTransform();
		Vector3 GetDockingOrientation();
		int GetSnapCount();

		DockInfo GetDockInfo();
		void SetDockInfo(DockInfo dockInfo);

		// available for other mods to get information

		bool IsDocked();
		IDockable GetOtherDockable();
	}
}
