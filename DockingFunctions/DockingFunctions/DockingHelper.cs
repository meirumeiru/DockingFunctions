using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace DockingFunctions
{
	public static class DockingHelper
	{
		//////////////////////////////
		// DockingEase

		public static void DisableDockingEase(Vessel v)
		{
			Type VesselPrecalculateType = null;

		//	AssemblyLoader.loadedAssemblies.TypeOperation (t => {
		//		if(t.FullName == "VesselPrecalculate") { VesselPrecalculateType = t; } });

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

		public static void SuspendDockingEase(Vessel v, int frames)
		{
			DisableDockingEase(v);
			FlightDriver.fetch.StartCoroutine(WaitAndEnableDockingEase(v, frames));
		}

		public static IEnumerator WaitAndEnableDockingEase(Vessel v, int frames)
		{
			while(frames-- > 0)
				yield return new WaitForFixedUpdate();

			EnableDockingEase(v);
		}

		//////////////////////////////
		// Camera Switch

		private static Part targetPart;
		private static Vector3 position1;
		private static Vector3 position2;

		public static void SaveCameraPosition(Part part)
		{
			targetPart = (FlightCamera.fetch.targetMode == FlightCamera.TargetMode.Part) ? FlightCamera.fetch.partTarget : null;

			position1 = part.transform.InverseTransformPoint(FlightCamera.fetch.GetPivot().position);
			position2 = part.transform.InverseTransformPoint(FlightCamera.fetch.GetCameraTransform().position);
		}

		public static void RestoreCameraPosition(Part part)
		{
			if(targetPart)
				FlightCamera.fetch.SetTargetPart(targetPart);

			FlightCamera.fetch.GetPivot().position = part.transform.TransformPoint(position1);
			FlightCamera.fetch.SetCamCoordsFromPosition(part.transform.TransformPoint(position2));
			FlightCamera.fetch.GetCameraTransform().position = part.transform.TransformPoint(position2);
		}

		public static void DisableCameraSwitch()
		{
			Type FlightCameraType = null;

		//	AssemblyLoader.loadedAssemblies.TypeOperation (t => {
		//		if(t.FullName == "FlightCamera") { FlightCameraType = t; } });

			FlightCameraType = typeof(FlightCamera);

			System.Reflection.MethodInfo OnVesselChange;
			OnVesselChange = FlightCameraType.GetMethod("OnVesselChange", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

			Delegate onEvent = Delegate.CreateDelegate(typeof(EventData<Vessel>.OnEvent), FlightCamera.fetch, OnVesselChange);

			GameEvents.onVesselChange.Remove((EventData<Vessel>.OnEvent)onEvent);
		}

		public static void EnableCameraSwitch()
		{
			Type FlightCameraType = null;

		//	AssemblyLoader.loadedAssemblies.TypeOperation (t => {
		//		if(t.FullName == "FlightCamera") { FlightCameraType = t; } });

			FlightCameraType = typeof(FlightCamera);

			System.Reflection.MethodInfo OnVesselChange;
			OnVesselChange = FlightCameraType.GetMethod("OnVesselChange", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

			Delegate onEvent = Delegate.CreateDelegate(typeof(EventData<Vessel>.OnEvent), FlightCamera.fetch, OnVesselChange);

			GameEvents.onVesselChange.Add((EventData<Vessel>.OnEvent)onEvent);
		}

		public static void SuspendCameraSwitch(int frames)
		{
			DisableCameraSwitch();
			FlightDriver.fetch.StartCoroutine(WaitAndEnableCameraSwitch(frames));
		}

		public static IEnumerator WaitAndEnableCameraSwitch(int frames)
		{
			while(frames-- > 0)
				yield return new WaitForFixedUpdate();

			EnableCameraSwitch();
		}

		//////////////////////////////
		// position / rotation

		public struct posInfoPart
		{
			public Vector3 position;
			public Quaternion rotation;

			public posInfoPart(Part p)
			{
				position = p.transform.position;
				rotation = p.transform.rotation;
			}

			public posInfoPart(Vector3 p_position, Quaternion p_rotation)
			{
				position = p_position;
				rotation = p_rotation;
			}

			public void Apply(Part p)
			{
				p.transform.position = position;
				p.transform.rotation = rotation;
			}
		}

		public class posInfo
		{
			public Dictionary<Part, posInfoPart> data;
		}

		public static posInfo BuildPosInfo(Vessel v)
		{
			posInfo info = new posInfo();
			info.data = new Dictionary<Part, posInfoPart>(v.parts.Count);

			foreach(Part p in v.parts)
				info.data.Add(p, new posInfoPart(p));

			return info;
		}

		public static void ApplyPosInfo(posInfo info, Vessel v)
		{
			foreach(Part p in v.parts)
			{
				posInfoPart r;
				if(!info.data.TryGetValue(p, out r))
					continue;

				r.Apply(p);
			}
		}

		// helper functions for re-docking (create the same result as if there was a real docking)

		private static void SetRotation(List<Part> parts, Quaternion rotation)
		{
			for(int i = 0; i < parts.Count; i++)
			{
				Part part = parts[i];
				part.partTransform.rotation = rotation * part.orgRot;
			}
		}

		private static void SetPosition(List<Part> parts, QuaternionD rotation, Vector3 position)
		{
			for(int i = 0; i < parts.Count; i++)
			{
				Part part = parts[i];
				part.partTransform.position = position + rotation * part.orgPos;
			}
		}

		//////////////////////////////
		// parts

		private static void FindChildParts(Part part, List<Part> parts)
		{
			parts.AddRange(part.children);

			foreach(Part child in part.children)
				FindChildParts(child, parts);
		}

		private static List<Part> FindParts(Part part)
		{
			List<Part> parts = new List<Part>();

			parts.Add(part);
			FindChildParts(part, parts);

			return parts;
		}

		// helper functions for re-docking (create the same result as if there was a real docking)

		public static void Couple(Part part, Part tgtPart)
		{
			part.SetHierarchyRoot(part);
			part.setParent(tgtPart);
			part.CreateAttachJoint(tgtPart.attachMode);
			part.ResetJoints();
		}

		public static void SetVessel(Part part)
		{
			part.vessel.parts.Add(part);

			for(int i = 0; i < part.children.Count; i++)
				SetVessel(part.children[i]);
		}

		//////////////////////////////
		// Connections

		public static Dictionary<Part, DockInfo> FindAllDockInfo(Vessel v)
		{
			Dictionary<Part, DockInfo> data = new Dictionary<Part, DockInfo>();

			foreach(Part p in v.Parts)
			{
				IDockable dockable = p.FindModuleImplementing<IDockable>();

				if(dockable != null)
				{
					DockInfo dockInfo = dockable.GetDockInfo();

					if((dockInfo != null) && (dockInfo.part.GetPart() == p))
						data.Add(p, dockInfo);
				}
			}

			return data;
		}

		//////////////////////////////
		// Latching

		private static void CalculateLatchingValues(
			Part part, Transform nodeTransform, Vector3 dockingOrientation,
			Transform targetNodeTransform, Vector3 targetDockingOrientation,
			int snapCount, out Vector3 part_position, out Quaternion part_rotation)
		{
			// relative rotation targetNodeTransform -> nodeTransform
			Quaternion nodeToNode =
				Quaternion.AngleAxis(180f, targetDockingOrientation)
				* Quaternion.AngleAxis(Vector3.SignedAngle(targetDockingOrientation, dockingOrientation, Vector3.forward), Vector3.forward);

			// find docking angle (from current values, not org-values) -> used for decision which docking angle ("snap angle") to choose
			float dockingAngle =
			Vector3.SignedAngle(targetNodeTransform.rotation * targetDockingOrientation, nodeTransform.rotation * dockingOrientation,
				nodeTransform.forward);

			// -> at correct docking with 0 degrees, the DockingOrientation of both ports point into the same direction

			float snapAngle = 360f / snapCount;

			dockingAngle = Mathf.Round(dockingAngle / snapAngle) * snapAngle;

			nodeToNode = nodeToNode * Quaternion.AngleAxis(dockingAngle, Vector3.forward);

			// relative rotation nodeTransform -> part
			Quaternion part_relRot =
				Quaternion.Inverse(nodeTransform.rotation)
				* part.transform.rotation;

			// relative position nodeTransform -> part
			Vector3 part_relPos =
				Quaternion.Inverse(nodeTransform.rotation)
				* (part.transform.position - nodeTransform.position);

			// final solution targetPart.position/rotation -> part.position/rotation

			Vector3 nodeTransform_position = targetNodeTransform.position;
			Quaternion nodeTransform_rotation = targetNodeTransform.rotation * nodeToNode;

			part_position = nodeTransform_position + nodeTransform_rotation * part_relPos;
			part_rotation = nodeTransform_rotation * part_relRot;
		}

		public static void CalculateLatchingPositionAndRotation(IDockable part, IDockable targetPart, out Vector3 partPosition, out Quaternion partRotation)
		{
			CalculateLatchingValues(
				part.GetPart(), part.GetNodeTransform(), part.GetDockingOrientation(),
				targetPart.GetNodeTransform(), targetPart.GetDockingOrientation(),
				Math.Min(part.GetSnapCount(), targetPart.GetSnapCount()),
				out partPosition, out partRotation);
		}

		//////////////////////////////
		// Docking

		// docks a vessel to the targetVessel
		private static void ExecuteDockVessels(
			Part part, Transform nodeTransform, Vector3 dockingOrientation,
			Part targetPart, Transform targetNodeTransform, Vector3 targetDockingOrientation,
			int snapCount, out DockedVesselInfo vesselInfo, out DockedVesselInfo targetVesselInfo)
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

			// disable DockingEase for a while
			SuspendDockingEase(targetVessel, 10);

			// save current positions
			posInfo posInfo = BuildPosInfo(vessel);
			posInfo targetPosInfo = BuildPosInfo(targetVessel);

			// set positions for docking (required, for orgPos/orgRot calculation that is done internally)
			targetVessel.SetRotation(targetVessel.transform.rotation);

			// calculate docking orientation
			Vector3 partPos; Quaternion partRot;

			CalculateLatchingValues(part, nodeTransform, dockingOrientation, targetNodeTransform, targetDockingOrientation, snapCount, out partPos, out partRot);

			vessel.SetRotation(partRot * Quaternion.Inverse(part.orgRot));
			vessel.SetPosition(partPos - vessel.transform.rotation * part.orgPos);

			vessel.IgnoreGForces(10);

			// couple parts
			part.Couple(targetPart);

			// restore positions
			ApplyPosInfo(posInfo, targetVessel);
			ApplyPosInfo(targetPosInfo, targetVessel);

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

			part.fuelLookupTargets.Add(targetPart);
			targetPart.fuelLookupTargets.Add(part);
			GameEvents.onPartFuelLookupStateChange.Fire(new GameEvents.HostedFromToAction<bool, Part>(true, targetPart, part));
		}

		private static void ExecuteDockSameVessel(
			Part part, Transform nodeTransform, Vector3 dockingOrientation,
			Part targetPart, Transform targetNodeTransform, Vector3 targetDockingOrientation,
			int snapCount, out ConfigurableJoint sameVesselJoint)
		{
		//	GameEvents.onSameVesselDock.Fire(new GameEvents.FromToAction<ModuleDockingNode, ModuleDockingNode>(this, node));

			posInfoPart posInfoTargetPart = new posInfoPart(targetPart);
			posInfoPart posInfoPart = new posInfoPart(part);

			// set to orgPos/orgRot
			targetPart.partTransform.rotation = targetPart.vessel.transform.rotation * targetPart.orgRot;
			targetPart.partTransform.position = targetPart.vessel.transform.position + ((QuaternionD)targetPart.vessel.transform.rotation) * targetPart.orgPos;

			// calculate docking orientation
			Vector3 partPos; Quaternion partRot;

			CalculateLatchingValues(part, nodeTransform, dockingOrientation, targetNodeTransform, targetDockingOrientation, snapCount, out partPos, out partRot);

			part.partTransform.rotation = partRot;
			part.partTransform.position = partPos;

			// create joint
			ConfigurableJoint cfj = part.gameObject.AddComponent<ConfigurableJoint>();

			cfj.connectedBody = targetPart.GetComponent<Rigidbody>();

			float stackNodeFactor = 2f;
			float srfNodeFactor = 0.8f;

			float breakingForceModifier = 1f;
			float breakingTorqueModifier = 1f;

			float attachNodeSize = 1f;

			float linearForce = Mathf.Min(part.breakingForce, targetPart.breakingForce) *
				breakingForceModifier *
				(attachNodeSize + 1f) * (part.attachMode == AttachModes.SRF_ATTACH ? srfNodeFactor : stackNodeFactor);

			float torqueForce = Mathf.Min(part.breakingTorque, targetPart.breakingTorque) *
				breakingTorqueModifier *
				(attachNodeSize + 1f) * (part.attachMode == AttachModes.SRF_ATTACH ? srfNodeFactor : stackNodeFactor);

			cfj.xMotion = cfj.yMotion = cfj.zMotion = ConfigurableJointMotion.Limited;
			cfj.angularXMotion = cfj.angularYMotion = cfj.angularZMotion = ConfigurableJointMotion.Limited;

			SoftJointLimit angularLimit = default(SoftJointLimit);
			angularLimit.bounciness = 0f;

			SoftJointLimitSpring angularLimitSpring = default(SoftJointLimitSpring);
			angularLimitSpring.spring = 0f;
			angularLimitSpring.damper = 0f;

			cfj.highAngularXLimit = angularLimit;
			cfj.lowAngularXLimit = angularLimit;
			cfj.angularYLimit = angularLimit;
			cfj.angularZLimit = angularLimit;
			cfj.angularXLimitSpring = angularLimitSpring;
			cfj.angularYZLimitSpring = angularLimitSpring;

			SoftJointLimit linearJointLimit = default(SoftJointLimit);
			linearJointLimit.limit = 1f;
			linearJointLimit.bounciness = 0f;

			SoftJointLimitSpring linearJointLimitSpring = default(SoftJointLimitSpring);
			linearJointLimitSpring.damper = 0f;
			linearJointLimitSpring.spring = 0f;

			cfj.linearLimit = linearJointLimit;
			cfj.linearLimitSpring = linearJointLimitSpring;

			JointDrive angularDrive = new JointDrive { maximumForce = PhysicsGlobals.JointForce, positionSpring = 60000f, positionDamper = 0f };
			cfj.angularXDrive = cfj.angularYZDrive = angularDrive; 

			JointDrive linearDrive = new JointDrive { maximumForce = PhysicsGlobals.JointForce, positionSpring = PhysicsGlobals.JointForce, positionDamper = 0f };
			cfj.xDrive = cfj.yDrive = cfj.zDrive = linearDrive;

			cfj.breakForce = linearForce;
			cfj.breakTorque = torqueForce;

			posInfoTargetPart.Apply(targetPart);
			posInfoPart.Apply(part);

			GameEvents.onVesselWasModified.Fire(targetPart.vessel);
		//	GameEvents.onDockingComplete.Fire(new GameEvents.FromToAction<Part, Part>(part, targetPart));

			// option -> if we would do this here, we woulnd't have to do it in ExecuteReDockVessels
			// -> but, KSP doesn't do it when docking to sameVessel

		//	part.fuelLookupTargets.Add(targetPart);
		//	targetPart.fuelLookupTargets.Add(part);
		//	GameEvents.onPartFuelLookupStateChange.Fire(new GameEvents.HostedFromToAction<bool, Part>(true, targetPart, part));

			sameVesselJoint = cfj;
		}

		// re-docks a vessel to the targetVessel after having been docked via the parts as same vessel docking
		private static void ExecuteReDockVessels(
			Part part, Transform nodeTransform, Vector3 dockingOrientation,
			Part targetPart, Transform targetNodeTransform, Vector3 targetDockingOrientation, int snapCount,
			List<Part> virtualVesselParts)
		{
			Vessel vessel = part.vessel;

		//	GameEvents.onVesselDocking.Fire(vessel.persistentId, vessel.persistentId);
			GameEvents.onActiveJointNeedUpdate.Fire(vessel);

			// save current positions
			posInfo posInfo = BuildPosInfo(vessel);
	
			// set positions for docking (required, for orgPos/orgRot calculation that is done internally)
			vessel.SetRotation(vessel.transform.rotation);

			// calculate docking orientation
			Vector3 partPos; Quaternion partRot;

			CalculateLatchingValues(part, nodeTransform, dockingOrientation, targetNodeTransform, targetDockingOrientation, snapCount, out partPos, out partRot);

			foreach(Part p in virtualVesselParts)
				p.UpdateOrgPosAndRot(part);

			SetRotation(virtualVesselParts, partRot);
			SetPosition(virtualVesselParts, partRot, partPos);

			vessel.IgnoreGForces(10);

			foreach(Part p in virtualVesselParts)
				p.UpdateOrgPosAndRot(vessel.rootPart);

			// couple parts
			Couple(part, targetPart);

			// fix the order of the parts in the vessel
			foreach(Part p in virtualVesselParts)
				p.vessel.parts.Remove(p);

			SetVessel(part);

			// restore positions
			ApplyPosInfo(posInfo, vessel);

			FlightInputHandler.SetNeutralControls();

			for(int i = 0; i < vessel.parts.Count; i++)
				vessel.parts[i].SetCollisionIgnores();

			GameEvents.onVesselWasModified.Fire(vessel);
		//	GameEvents.onDockingComplete.Fire(new GameEvents.FromToAction<Part, Part>(part, targetPart));

			part.fuelLookupTargets.Add(targetPart);
			targetPart.fuelLookupTargets.Add(part);
			GameEvents.onPartFuelLookupStateChange.Fire(new GameEvents.HostedFromToAction<bool, Part>(true, targetPart, part));
		}

		// undocks a vessel

		private static IEnumerator ExecuteUndockVessels(Part part, DockedVesselInfo vesselInfo)
		{
			yield return new WaitForEndOfFrame();

			if(vesselInfo != null)
				part.Undock(vesselInfo);
			else
				part.decouple();
		}

		public static void DockVessels(IDockable part, IDockable targetPart)
		{
			if(part.GetPart().vessel != targetPart.GetPart().vessel)
			{
				DockingEvents.onVesselDocking.Fire(part, targetPart);

				DockedVesselInfo vesselInfo, targetVesselInfo;

				ExecuteDockVessels(part.GetPart(), part.GetNodeTransform(), part.GetDockingOrientation(),
					targetPart.GetPart(), targetPart.GetNodeTransform(), targetPart.GetDockingOrientation(),
					Math.Min(part.GetSnapCount(), targetPart.GetSnapCount()),
					out vesselInfo, out targetVesselInfo);

				DockInfo dockInfo = new DockInfo { part = part, targetPart = targetPart, vesselInfo = vesselInfo, targetVesselInfo = targetVesselInfo,
					isSameVesselJoint = false, sameVesselJoint = null };

				part.SetDockInfo(dockInfo);
				targetPart.SetDockInfo(dockInfo);

				DockingEvents.onVesselDocked.Fire(part, targetPart);
			}
			else
			{
				DockingEvents.onSameVesselDocking.Fire(part, targetPart);

				ConfigurableJoint _sameVesselJoint = null;

				ExecuteDockSameVessel(part.GetPart(), part.GetNodeTransform(), part.GetDockingOrientation(),
					targetPart.GetPart(), targetPart.GetNodeTransform(), targetPart.GetDockingOrientation(),
					Math.Min(part.GetSnapCount(), targetPart.GetSnapCount()),
					out _sameVesselJoint);

				DockInfo dockInfo = new DockInfo { part = part, targetPart = targetPart, vesselInfo = null, targetVesselInfo = null,
					isSameVesselJoint = true, sameVesselJoint = _sameVesselJoint };

				part.SetDockInfo(dockInfo);
				targetPart.SetDockInfo(dockInfo);

				DockingEvents.onSameVesselDocked.Fire(part, targetPart);
			}
		}

		private static void RedockVessel(DockInfo targetDockInfo, DockInfo dockInfo, List<Part> virtualVesselParts)
		{
			IDockable part, targetPart;

			if(virtualVesselParts.Contains(targetDockInfo.part.GetPart()))
			{ part = targetDockInfo.part; targetPart = targetDockInfo.targetPart; }
			else
			{ part = targetDockInfo.targetPart; targetPart = targetDockInfo.part; }

			DockingEvents.onDockingSwitching.Fire(targetDockInfo.part, part);
			DockingEvents.onDockingSwitching.Fire(targetDockInfo.targetPart, targetPart);

			ExecuteReDockVessels(part.GetPart(), part.GetNodeTransform(), part.GetDockingOrientation(),
							targetPart.GetPart(), targetPart.GetNodeTransform(), targetPart.GetDockingOrientation(),
							Math.Min(part.GetSnapCount(), targetPart.GetSnapCount()),
							virtualVesselParts);

			DockInfo dockInfoNew = new DockInfo { part = part, targetPart = targetPart, vesselInfo = null, targetVesselInfo = null,
				isSameVesselJoint = false, sameVesselJoint = null };

			if(dockInfo != null) // the only valid dockInfo data come from dockInfo (may be null if it was pre-attached / targetDockInfo is a sameVesselJoint and doesn't have this data)
			{
				dockInfoNew.vesselInfo = dockInfo.vesselInfo;
				dockInfoNew.targetVesselInfo = dockInfo.targetVesselInfo;
			}

			targetDockInfo.part.SetDockInfo(dockInfoNew);
			targetDockInfo.targetPart.SetDockInfo(dockInfoNew);

			DockingEvents.onDockingSwitched.Fire(targetDockInfo.part, part);
			DockingEvents.onDockingSwitched.Fire(targetDockInfo.targetPart, targetPart);
		}

		public static void UndockVessels(IDockable part, IDockable targetPart)
		{
			if(part.GetPart().vessel != targetPart.GetPart().vessel)
				return;

			IDockable vesselPart = part;

			if(targetPart.GetPart().parent == part.GetPart())
			{ IDockable temp = part; part = targetPart; targetPart = temp; }

			Dictionary<Part, DockInfo> data = FindAllDockInfo(part.GetPart().vessel);

			DockInfo dockInfo;
			if((data.TryGetValue(part.GetPart(), out dockInfo) || data.TryGetValue(targetPart.GetPart(), out dockInfo))
				&& dockInfo.isSameVesselJoint)
			{
				DockingEvents.onSameVesselUndocking.Fire(part, targetPart);

				if(dockInfo.sameVesselJoint)
					UnityEngine.Object.Destroy(dockInfo.sameVesselJoint);

				part.SetDockInfo(null);
				targetPart.SetDockInfo(null);

				DockingEvents.onSameVesselUndocked.Fire(part, targetPart);
			}
			else
			{
				List<Part> virtualVesselParts = FindParts(part.GetPart());

				DockInfo targetDockInfo = null;

				foreach(KeyValuePair<Part, DockInfo> kv in data)
				{
					if((kv.Value != dockInfo)
					&& (virtualVesselParts.Contains(kv.Key) != virtualVesselParts.Contains(kv.Value.targetPart.GetPart())))
					{
						targetDockInfo = kv.Value;
						break;
					}
				}

				if(targetDockInfo != null)
				{
					if(!targetDockInfo.isSameVesselJoint)
						Logger.Log("found secondary dockInfo without isSameVesselJoint flag", Logger.Level.Error);

					DockingEvents.onSameVesselUndocking.Fire(part, targetPart);

					if((bool)part.GetPart().attachJoint)
						part.GetPart().attachJoint.DestroyJoint();

					AttachNode attachNode = part.GetPart().FindAttachNodeByPart(part.GetPart().parent);
					if(attachNode != null)
					{
						attachNode.attachedPart = null;

						int i = 0;
						for(int count = part.GetPart().Modules.Count; i < count; i++)
						{
							if(part.GetPart().Modules[i] is IActivateOnDecouple activateOnDecouple)
								activateOnDecouple.DecoupleAction(attachNode.id, weDecouple: true);
						}
					}

					if(targetDockInfo.sameVesselJoint)
						UnityEngine.Object.Destroy(targetDockInfo.sameVesselJoint);

					part.GetPart().setParent();

					part.SetDockInfo(null);
					targetPart.SetDockInfo(null);

					RedockVessel(targetDockInfo, dockInfo, virtualVesselParts);

					DockingEvents.onSameVesselUndocked.Fire(part, targetPart);
				}
				else
				{
					bool SAS = part.GetPart().vessel.ActionGroups[KSPActionGroup.SAS];
					bool RCS = part.GetPart().vessel.ActionGroups[KSPActionGroup.RCS];

					DockingEvents.onVesselUndocking.Fire(part, targetPart);

					part.GetPart().StartCoroutine(ExecuteUndockVessels(part.GetPart(), (dockInfo == null) ? null : dockInfo.vesselInfo));


					part.SetDockInfo(null);
					targetPart.SetDockInfo(null);

					// copy sas and rcs state
					part.GetPart().vessel.ActionGroups.SetGroup(KSPActionGroup.SAS, SAS);
					part.GetPart().vessel.ActionGroups.SetGroup(KSPActionGroup.RCS, RCS);

					targetPart.GetPart().vessel.ActionGroups.SetGroup(KSPActionGroup.SAS, SAS);
					targetPart.GetPart().vessel.ActionGroups.SetGroup(KSPActionGroup.RCS, RCS);

					DockingEvents.onVesselUndocked.Fire(part, targetPart);

				//	part.fuelLookupTargets.Remove(targetPart);
				//	targetPart.fuelLookupTargets.Remove(part);
				//	GameEvents.onPartFuelLookupStateChange.Fire(new GameEvents.HostedFromToAction<bool, Part>(true, part, targetPart));
				}

				// set focus to correct vessel
				if(FlightGlobals.ActiveVessel != vesselPart.GetPart().vessel)
				{
					FlightGlobals.ForceSetActiveVessel(vesselPart.GetPart().vessel);
					FlightInputHandler.SetNeutralControls();
				}
			}

			// option -> set other camera ?
		}

		// call this function to rebuild the docking state information after loading
		public static void OnLoad(IDockable part, DockedVesselInfo vesselInfo, IDockable targetPart, DockedVesselInfo targetVesselInfo)
		{
			// checks (just to be sure)
			if(part.GetPart().parent != targetPart.GetPart()) // -> sameVesselJoint
			{
				if(targetPart.GetPart().parent == part.GetPart())
				{
					Logger.Log("wrong configuration detected", Logger.Level.Error);
					return; // this should not happen
				}

				if(part.GetDockInfo() != null)
				{
					Logger.Log("sameVesselJoint already built", Logger.Level.Error);
					return; // this should not happen
				}
			}

			DockInfo dockInfo = new DockInfo { part = part, targetPart = targetPart, vesselInfo = vesselInfo, targetVesselInfo = targetVesselInfo,
				isSameVesselJoint = (part.GetPart().parent != targetPart.GetPart()), sameVesselJoint = null };

			if(dockInfo.isSameVesselJoint)
			{
				ConfigurableJoint _sameVesselJoint = null;

				ExecuteDockSameVessel(part.GetPart(), part.GetNodeTransform(), part.GetDockingOrientation(),
					targetPart.GetPart(), targetPart.GetNodeTransform(), targetPart.GetDockingOrientation(),
					Math.Min(part.GetSnapCount(), targetPart.GetSnapCount()),
					out _sameVesselJoint);

				dockInfo.sameVesselJoint = _sameVesselJoint;
			}

			part.SetDockInfo(dockInfo);
			targetPart.SetDockInfo(dockInfo);
		}
	}
}

