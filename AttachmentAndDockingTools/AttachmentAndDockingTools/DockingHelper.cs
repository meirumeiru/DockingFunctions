using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AttachmentAndDockingTools
{
	public static class DockingHelper
	{
		//////////////////////////////
		// DockingEase

		public static void DisableDockingEase(Vessel v)
		{
			Type VesselPrecalculateType = null;

	//		AssemblyLoader.loadedAssemblies.TypeOperation (t => {
	//			if(t.FullName == "VesselPrecalculate") { VesselPrecalculateType = t; } });

			VesselPrecalculateType = typeof(VesselPrecalculate);

			System.Reflection.MethodInfo onDockingComplete;
			onDockingComplete = VesselPrecalculateType.GetMethod("onDockingComplete", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

			Delegate onEvent = Delegate.CreateDelegate(typeof(EventData<GameEvents.FromToAction<Part, Part>>.OnEvent), v.precalc, onDockingComplete);

			GameEvents.onDockingComplete.Remove((EventData<GameEvents.FromToAction<Part, Part>>.OnEvent)onEvent);
		}

		public static void EnableDockingEase(Vessel v)
		{
			Type VesselPrecalculateType = null;

		//	AssemblyLoader.loadedAssemblies.TypeOperation (t => {
		//		if(t.FullName == "VesselPrecalculate") { VesselPrecalculateType = t; } });

			VesselPrecalculateType = typeof(VesselPrecalculate);

			System.Reflection.MethodInfo onDockingComplete;
			onDockingComplete = VesselPrecalculateType.GetMethod("onDockingComplete", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

			Delegate onEvent = Delegate.CreateDelegate(typeof(EventData<GameEvents.FromToAction<Part, Part>>.OnEvent), v.precalc, onDockingComplete);

			GameEvents.onDockingComplete.Add((EventData<GameEvents.FromToAction<Part, Part>>.OnEvent)onEvent);
		}

		public static IEnumerator WaitAndEnableDockingEase(Vessel v)
		{
			for(int i = 0; i < 10; i++)
				yield return new WaitForFixedUpdate();

			EnableDockingEase(v);
		}

		//////////////////////////////
		// Camera Switch

		public static void DisableCameraSwitch()
		{
			Type FlightCameraType = null;

	//		AssemblyLoader.loadedAssemblies.TypeOperation (t => {
	//			if(t.FullName == "FlightCamera") { FlightCameraType = t; } });

			FlightCameraType = typeof(FlightCamera);

			System.Reflection.MethodInfo OnVesselChange;
			OnVesselChange = FlightCameraType.GetMethod("OnVesselChange", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

			Delegate onEvent = Delegate.CreateDelegate(typeof(EventData<Vessel>.OnEvent), FlightCamera.fetch, OnVesselChange);

			GameEvents.onVesselChange.Remove((EventData<Vessel>.OnEvent)onEvent);
		}

		public static void EnableCameraSwitch()
		{
			Type FlightCameraType = null;

	//		AssemblyLoader.loadedAssemblies.TypeOperation (t => {
	//			if(t.FullName == "FlightCamera") { FlightCameraType = t; } });

			FlightCameraType = typeof(FlightCamera);

			System.Reflection.MethodInfo OnVesselChange;
			OnVesselChange = FlightCameraType.GetMethod("OnVesselChange", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

			Delegate onEvent = Delegate.CreateDelegate(typeof(EventData<Vessel>.OnEvent), FlightCamera.fetch, OnVesselChange);

			GameEvents.onVesselChange.Add((EventData<Vessel>.OnEvent)onEvent);
		}

		public static IEnumerator WaitAndEnableCameraSwitch()
		{
			for(int i = 0; i < 10; i++)
				yield return new WaitForFixedUpdate();

			EnableCameraSwitch();
		}

		//////////////////////////////
		// orgPos / orgRot

		public struct orgInfoPart
		{
			public Vector3 orgPos;
			public Quaternion orgRot;

			public orgInfoPart(Part p)
			{
				orgPos = p.orgPos;
				orgRot = p.orgRot;
			}

			public orgInfoPart(Vector3 p_orgPos, Quaternion p_orgRot)
			{
				orgPos = p_orgPos;
				orgRot = p_orgRot;
			}
		}

		public class orgInfo
		{
			public Dictionary<Part, orgInfoPart> data;
		}

		public static orgInfo BuildOrgInfo(Vessel v)
		{
			orgInfo info = new orgInfo();
			info.data = new Dictionary<Part, orgInfoPart>(v.parts.Count);

			foreach(Part p in v.parts)
				info.data.Add(p, new orgInfoPart(p));

			return info;
		}

		public static void _BuildOrgInfo(orgInfo info, Part p)
		{
			info.data.Add(p, new orgInfoPart(p));

			foreach(Part c in p.children)
				_BuildOrgInfo(info, c);
		}

		public static orgInfo BuildOrgInfo(Part p)
		{
			orgInfo info = new orgInfo();
			info.data = new Dictionary<Part, orgInfoPart>();

			_BuildOrgInfo(info, p);

			return info;
		}

		public static void ReRootOrgInfo(orgInfo info, Part newRoot)
		{
			orgInfoPart r;
			info.data.TryGetValue(newRoot, out r);

			Dictionary<Part, orgInfoPart> data = new Dictionary<Part, orgInfoPart>(info.data.Count);

			foreach(KeyValuePair<Part, orgInfoPart> kv in info.data)
			{
				Vector3 orgPos = Quaternion.Inverse(r.orgRot) * (kv.Value.orgPos - r.orgPos);
				Quaternion orgRot = Quaternion.Inverse(r.orgRot) * kv.Value.orgRot;

				data.Add(kv.Key, new orgInfoPart(orgPos, orgRot));
			}

			info.data = data;
		}

		public static void MoveOrgInfo(orgInfo info, Vector3 rootDeltaPos, Quaternion rootDeltaRot)
		{
			Dictionary<Part, orgInfoPart> data = new Dictionary<Part, orgInfoPart>(info.data.Count);

			foreach(KeyValuePair<Part, orgInfoPart> kv in info.data)
			{
				data.Add(kv.Key, new orgInfoPart(
					rootDeltaPos + rootDeltaRot * kv.Value.orgPos,
					rootDeltaRot * kv.Value.orgRot));
			}

			info.data = data;
		}

		public static void ApplyOrgInfo(orgInfo info, Vessel v)
		{
			foreach(Part p in v.parts)
			{
				orgInfoPart r;
				if(!info.data.TryGetValue(p, out r))
					continue; // muss vom anderen Schiff sein -> FEHLER doof, aber, damit's schnell mal läuft

				p.orgPos = r.orgPos;
				p.orgRot = r.orgRot;
			}
		}

		public static void ApplyOrgInfo(orgInfo info, Part p)
		{
			orgInfoPart r;
			info.data.TryGetValue(p, out r);

			p.orgPos = r.orgPos;
			p.orgRot = r.orgRot;

			foreach(Part c in p.children)
				ApplyOrgInfo(info, c);
		}

		public static bool VerifyOrgInfo(orgInfo info, Vessel v)
		{
			bool wrong = false;

			foreach(Part p in v.parts)
			{
				orgInfoPart r;
				info.data.TryGetValue(p, out r);

				if(!(r.orgPos == p.orgPos)
				|| !(r.orgRot == p.orgRot))
					wrong = true;
			}

			return !wrong;
		}

		//////////////////////////////
		// Docking

		public static void DockToVessel(
			Part part, Transform nodeTransform, Vector3 dockingOrientation,
			Part targetPart, Transform targetNodeTransform, Vector3 targetDockingOrientation,
			int snapCount, out Vector3 targetPart_orgPos, out Quaternion targetPart_orgRot)
		{
			// relative rotation part -> nodeTransform
			Quaternion nodeTransform_relRot =
				Quaternion.Inverse(part.transform.rotation)
				* nodeTransform.rotation;
			//	* Quaternion.LookRotation(nodeTransform.forward, part.transform.rotation * dockingOrientation);

			// relative position part -> nodeTransform
			Vector3 nodeTransform_relPos =
				Quaternion.Inverse(part.transform.rotation)
				* (nodeTransform.position - part.transform.position);

			// relative rotation nodeTransform -> targetNodeTransform
			Quaternion nodeToNode = Quaternion.AngleAxis(180f, dockingOrientation);

// FEHLER, supertemp... mal sehen
float dockingAngle =
Vector3.SignedAngle(nodeTransform.rotation * dockingOrientation, targetNodeTransform.rotation * targetDockingOrientation,
	targetNodeTransform.forward);

float snapAngle = 360f / snapCount;

dockingAngle = Mathf.Round(dockingAngle / snapAngle) * snapAngle;

			nodeToNode = nodeToNode * Quaternion.AngleAxis(dockingAngle, Vector3.forward);

			// relative rotation targetNodeTransform -> targetPart
			Quaternion targetPart_relRot =
			//	Quaternion.Inverse(Quaternion.LookRotation(port.nodeTransform.forward, targetPart.transform.rotation * Quaternion.AngleAxis(dockingAngle, targetNodeTransform.forward) * dockingOrientation))
				Quaternion.Inverse(targetNodeTransform.rotation)
				* targetPart.transform.rotation;

			// relative position targetNodeTransform -> targetPart
			Vector3 targetPart_relPos =
			//	Quaternion.Inverse(Quaternion.LookRotation(targetNodeTransform.forward, targetPart.transform.rotation * Quaternion.AngleAxis(dockingAngle, targetNodeTransform.forward) * dockingOrientation))
				Quaternion.Inverse(targetNodeTransform.rotation)
				* (targetPart.transform.position - targetNodeTransform.position);

			// final solution part.orgPos/orgRot -> targetPart.orgPos/orgRot

			Vector3 nodeTransform_orgPos = part.orgPos + part.orgRot * nodeTransform_relPos;
			Quaternion nodeTransform_orgRot = part.orgRot * nodeTransform_relRot;

			nodeTransform_orgRot = nodeTransform_orgRot * nodeToNode;

			targetPart_orgPos = nodeTransform_orgPos + nodeTransform_orgRot * targetPart_relPos;
			targetPart_orgRot = nodeTransform_orgRot * targetPart_relRot;
		}

		// docks a vessel to the targetVessel

// FEHLER, ablösen den Schrott hier
		public static void DockVessels(
			Part part, Transform nodeTransform, Vector3 dockingOrientation,
			Part targetPart, Transform targetNodeTransform, Vector3 targetDockingOrientation,
			int snapCount, out DockedVesselInfo vesselInfo, out DockedVesselInfo targetVesselInfo,
			out Vector3 part_orgPos, out Quaternion part_orgRot)
		{
			Vessel vessel = part.vessel;
			Vessel targetVessel = targetPart.vessel;

			vesselInfo = new DockedVesselInfo();
			vesselInfo.name = vessel.vesselName;
			vesselInfo.vesselType = vessel.vesselType;
			vesselInfo.rootPartUId = vessel.rootPart.flightID;

			targetVesselInfo = new DockedVesselInfo();
			targetVesselInfo.name = targetVessel.vesselName;
			targetVesselInfo.vesselType = targetVessel.vesselType;
			targetVesselInfo.rootPartUId = targetVessel.rootPart.flightID;

			uint data = vessel.persistentId;
			uint data2 = targetVessel.persistentId;

			Vessel oldvessel = vessel;

			GameEvents.onVesselDocking.Fire(data, data2);
			GameEvents.onActiveJointNeedUpdate.Fire(targetVessel);
			GameEvents.onActiveJointNeedUpdate.Fire(vessel);


			DockingHelper.orgInfo orgInfo = DockingHelper.BuildOrgInfo(vessel);

	//		Vector3 part_orgPos; Quaternion part_orgRot;
			DockingHelper.DockToVessel(targetPart, targetNodeTransform, targetDockingOrientation, part, nodeTransform, dockingOrientation, snapCount, out part_orgPos, out part_orgRot);

			vessel.IgnoreGForces(10);

			DockingHelper.DisableDockingEase(targetVessel);

			part.Couple(targetPart);

// FEHLER, hier gehe ich davon aus, dass part.vessel == targetVessel ist... stimmt das? tja, wer weiss... hoffe schon

			DockingHelper.ReRootOrgInfo(orgInfo, part);
			DockingHelper.MoveOrgInfo(orgInfo, part_orgPos, part_orgRot);

			DockingHelper.ApplyOrgInfo(orgInfo, targetVessel);


			GameEvents.onVesselPersistentIdChanged.Fire(data, data2);

			if(oldvessel == FlightGlobals.ActiveVessel)
			{
				FlightGlobals.ForceSetActiveVessel(targetVessel);
				FlightInputHandler.SetNeutralControls();
			}
			else if(targetVessel == FlightGlobals.ActiveVessel)
			{
				targetVessel.MakeActive();
				FlightInputHandler.SetNeutralControls();
			}

			for(int i = 0; i < targetVessel.parts.Count; i++)
			{
				FlightGlobals.PersistentLoadedPartIds.Add(targetVessel.parts[i].persistentId, targetVessel.parts[i]);
				if(targetVessel.parts[i].protoPartSnapshot == null)
					continue;
				FlightGlobals.PersistentUnloadedPartIds.Add(targetVessel.parts[i].protoPartSnapshot.persistentId, targetVessel.parts[i].protoPartSnapshot);
			}

			GameEvents.onVesselWasModified.Fire(targetVessel);
			GameEvents.onDockingComplete.Fire(new GameEvents.FromToAction<Part, Part>(part, targetPart));

			part.StartCoroutine(DockingHelper.WaitAndEnableDockingEase(targetVessel));
		}

		public static void DockVessels(IDockable part, IDockable targetPart)
		{
			DockedVesselInfo vesselInfo, targetVesselInfo;
			Vector3 part_orgPos; Quaternion part_orgRot;

			DockVessels(part.GetPart(), part.GetNodeTransform(), part.GetDockingOrientation(),
				targetPart.GetPart(), targetPart.GetNodeTransform(), targetPart.GetDockingOrientation(),
				Math.Min(part.GetSnapCount(), targetPart.GetSnapCount()),
				out vesselInfo, out targetVesselInfo, out part_orgPos, out part_orgRot);

			DockInfo dockInfo = new DockInfo { part = part, targetPart = targetPart, vesselInfo = vesselInfo, targetVesselInfo = targetVesselInfo,
				orgPos = part_orgPos, orgRot = part_orgRot };

			part.SetDockInfo(dockInfo);
			targetPart.SetDockInfo(dockInfo);
		}

		public static void UndockVessels(Part part, Part targetPart, DockedVesselInfo vesselInfo, DockedVesselInfo targetVesselInfo)
		{
			part.Undock(vesselInfo);
		}

		public static void RedockVessel(Part part, Part targetPart,
			Vector3 part_orgPos, Quaternion part_orgRot,
			Part oldPart, Part oldTargetPart)
		{
		//	GameEvents.onPartDeCoupleComplete.Fire(oldPart); FEHLER, soll ich?

			DockingHelper.orgInfo orgInfo = DockingHelper.BuildOrgInfo(oldPart);

			if(oldPart.attachJoint)
				oldPart.attachJoint.DestroyJoint();
			oldPart.setParent();

		//	GameEvents.onPartCouple.Fire(new GameEvents.FromToAction<Part, Part>(part, targetPart)); FEHLER, soll ich?

			part.SetHierarchyRoot(part);
			part.setParent(targetPart);

			part.CreateAttachJoint(targetPart.attachMode);
			part.ResetJoints();


			DockingHelper.ReRootOrgInfo(orgInfo, part);
			DockingHelper.MoveOrgInfo(orgInfo, part_orgPos, part_orgRot);

			DockingHelper.ApplyOrgInfo(orgInfo, part);

		//	GameEvents.onActiveJointNeedUpdate.Fire(part.vessel); FEHLER, soll ich?

		//	GameEvents.onVesselWasModified.Fire(vessel); FEHLER, soll ich?
		//	GameEvents.onPartCoupleComplete.Fire(new GameEvents.FromToAction<Part, Part>(part, targetPart)); FEHLER, soll ich?
		}
	}
}





