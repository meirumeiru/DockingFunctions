using System;
using UnityEngine;

namespace AttachmentAndDockingTools
{
	public class DockInfo
	{
		public IDockable part;
		public IDockable targetPart;

		public Vector3 orgPos;
		public Quaternion orgRot;

		public DockedVesselInfo vesselInfo;
		public DockedVesselInfo targetVesselInfo;
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
