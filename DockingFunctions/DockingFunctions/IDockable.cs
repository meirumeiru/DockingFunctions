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
		Part GetPart();
		Transform GetNodeTransform();
		Vector3 GetDockingOrientation();
		int GetSnapCount();

		DockInfo GetDockInfo();
		void SetDockInfo(DockInfo dockInfo);
	}
}
