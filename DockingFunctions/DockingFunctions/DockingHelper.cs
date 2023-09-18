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

		private static Vector3 position1;
		private static Vector3 position2;

		public static void SaveCameraPosition(Part part)
		{
			position1 = part.transform.InverseTransformPoint(FlightCamera.fetch.GetPivot().position);
			position2 = part.transform.InverseTransformPoint(FlightCamera.fetch.GetCameraTransform().position);
		}

		public static void RestoreCameraPosition(Part part)
		{
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

				p.transform.position = r.position;
				p.transform.rotation = r.rotation;
			}
		}

		//////////////////////////////
		// parts

		private static void FindChildParts(Part part, List<Part> childParts)
		{
			childParts.AddRange(part.children);

			foreach(Part child in part.children)
				FindChildParts(child, childParts);
		}

		private static List<Part> FindChildParts(Part part)
		{
			List<Part> childParts = new List<Part>();

			FindChildParts(part, childParts);

			return childParts;
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
		// Docking

		private static void CalculateDockingValues(
			Part targetPart, Transform targetNodeTransform, Vector3 targetDockingOrientation,
			Part part, Transform nodeTransform, Vector3 dockingOrientation,
			int snapCount, out Vector3 part_orgPos, out Quaternion part_orgRot)
		{
			// relative rotation targetPart -> targetNodeTransform
			Quaternion targetNodeTransform_relRot =
				Quaternion.Inverse(targetPart.transform.rotation)
				* targetNodeTransform.rotation;

			// relative position targetPart -> targetNodeTransform
			Vector3 targetNodeTransform_relPos =
				Quaternion.Inverse(targetPart.transform.rotation)
				* (targetNodeTransform.position - targetPart.transform.position);

			// relative rotation targetNodeTransform -> nodeTransform
			Quaternion nodeToNode =
				Quaternion.AngleAxis(180f, targetDockingOrientation)
				* Quaternion.AngleAxis(Vector3.SignedAngle(targetDockingOrientation, dockingOrientation, Vector3.forward), Vector3.forward);

// FEHLER, supertemp... mal sehen -> ich nutze die aktuell Position um zu erkennen, ob ich noch für ein snap eine Rotation machen muss -> heisst (ganz wichtig!!!) ich nutze AKTUELLE Daten um ABZUSCHÄTZEN, welche Rotation ich THEORETISCH noch machen muss -> das unterscheidet sich grundlegend von ALLEM ANDEREN HIER DRIN!!!!! GAAAAAAAANZ WICHTIG ZU VERSTEHEN!!!!! KAPPIERT???
			float dockingAngle =
			Vector3.SignedAngle(targetNodeTransform.rotation * targetDockingOrientation, nodeTransform.rotation * dockingOrientation,
				nodeTransform.forward);

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

			// final solution targetPart.orgPos/orgRot -> part.orgPos/orgRot

			Vector3 targetNodeTransform_orgPos = targetPart.orgPos + targetPart.orgRot * targetNodeTransform_relPos;
			Quaternion targetNodeTransform_orgRot = targetPart.orgRot * targetNodeTransform_relRot;

			Vector3 nodeTransform_orgPos = targetNodeTransform_orgPos;
			Quaternion nodeTransform_orgRot = targetNodeTransform_orgRot * nodeToNode;

			part_orgPos = nodeTransform_orgPos + nodeTransform_orgRot * part_relPos;
			part_orgRot = nodeTransform_orgRot * part_relRot;
		}
/* unused, maybe useful to restore same vessel dockings after load (which is currently still missing)
 
		private static void CorrectAttachJoint(Part part)
		{
			// FEHLER, nur mal so eine Idee -> zudem: geht nur, wenn der Joint nur 1 gross ist... sonst nicht -> aber bei DockingNodes gehen wir davon aus... einfach mal... ok?

// der Joint wurde gebaut mit den gegebenen Positionen... Frage: stimmen die? kommt's zusammen mit KJR zu Problemen? weil ein Joint überbelastet?

// FEHLER, eigentlich ist das Problem, dass ich die Teils nach attach-Node verbinden müsste

			Part linkPart = part.parent;

part.attachJoint.joints[0].autoConfigureConnectedAnchor = false;

Vector3 anchor = part.orgPos + part.orgRot * part.attachJoint.joints[0].anchor;

Vector3 connectedAnchor = Quaternion.Inverse(linkPart.orgRot) * (anchor - linkPart.orgPos);

	//		newJoint.connectedAnchor = Quaternion.Inverse(linkPart.orgRot) * (part.orgPos - linkPart.orgPos);
	//		newJoint.SetTargetRotationLocal((Quaternion.Inverse(part.transform.rotation) * linkPart.transform.rotation * (Quaternion.Inverse(linkPart.orgRot) * part.orgRot)).normalized, Quaternion.identity);

			part.attachJoint.joints[0].connectedAnchor = connectedAnchor;

part.attachJoint.joints[0].SetTargetRotationLocal((Quaternion.Inverse(part.transform.rotation) * linkPart.transform.rotation *
(Quaternion.Inverse(linkPart.orgRot) * part.orgRot)).normalized, Quaternion.identity);
		}
*/
		// docks a vessel to the targetVessel
		private static void ExecuteDockVessels(
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

			// disable DockingEase for a while
			SuspendDockingEase(targetVessel, 10);

			// save current positions
			posInfo posInfo = BuildPosInfo(vessel);
			posInfo targetPosInfo = BuildPosInfo(targetVessel);

			// calculate docking orientation
			CalculateDockingValues(targetPart, targetNodeTransform, targetDockingOrientation, part, nodeTransform, dockingOrientation, snapCount, out part_orgPos, out part_orgRot);

			// set positions for docking (required, for orgPos/orgRot calculation that is done internally)
			targetVessel.SetRotation(targetVessel.transform.rotation);
			vessel.SetRotation(Quaternion.FromToRotation(nodeTransform.forward, -targetNodeTransform.forward) * vessel.transform.rotation);
			vessel.SetPosition(vessel.transform.position - (nodeTransform.position - targetNodeTransform.position), usePristineCoords: true);
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

		//	part.fuelLookupTargets.Add(targetPart);
		//	targetPart.fuelLookupTargets.Add(part);
		//	GameEvents.onPartFuelLookupStateChange.Fire(new GameEvents.HostedFromToAction<bool, Part>(true, targetPart, part));
		}

		private static void ExecuteDockSameVessel(
			Part part, Transform nodeTransform, Vector3 dockingOrientation,
			Part targetPart, Transform targetNodeTransform, Vector3 targetDockingOrientation,
			int snapCount,
			out Vector3 part_orgPos, out Quaternion part_orgRot, out ConfigurableJoint sameVesselJoint)
		{
		//	GameEvents.onSameVesselDock.Fire(new GameEvents.FromToAction<ModuleDockingNode, ModuleDockingNode>(this, node));

			// calculate docking orientation
			CalculateDockingValues(targetPart, targetNodeTransform, targetDockingOrientation, part, nodeTransform, dockingOrientation, snapCount, out part_orgPos, out part_orgRot);

			// create joint
			ConfigurableJoint cfj = part.gameObject.AddComponent<ConfigurableJoint>();

			cfj.connectedBody = targetPart.GetComponent<Rigidbody>();

			cfj.autoConfigureConnectedAnchor = false;
			cfj.anchor = part.transform.InverseTransformPoint(nodeTransform.position);
			cfj.connectedAnchor = targetPart.transform.InverseTransformPoint(targetNodeTransform.position);

			cfj.SetTargetRotationLocal((Quaternion.Inverse(part.transform.rotation) * targetPart.transform.rotation *
				(Quaternion.Inverse(targetPart.orgRot) * part.orgRot)).normalized, Quaternion.identity);

			float stackNodeFactor = 2f;
			float srfNodeFactor = 0.8f;

			float breakingForceModifier = 1f;
			float breakingTorqueModifier = 1f;

			float attachNodeSize = 1f;

			float linearForce = Mathf.Min(part.breakingForce, targetPart.breakingForce) *
				breakingForceModifier *
				(attachNodeSize + 1f) * (part.attachMode == AttachModes.SRF_ATTACH ? srfNodeFactor : stackNodeFactor)
				/ part.attachJoint.joints.Count;

			float torqueForce = Mathf.Min(part.breakingTorque, targetPart.breakingTorque) *
				breakingTorqueModifier *
				(attachNodeSize + 1f) * (part.attachMode == AttachModes.SRF_ATTACH ? srfNodeFactor : stackNodeFactor)
				/ part.attachJoint.joints.Count;


			float extraLinearForce = PhysicsGlobals.JointForce;
			float extraLinearSpring = PhysicsGlobals.JointForce;
			float extraLinearDamper = 0f;

			float extraAngularForce = PhysicsGlobals.JointForce;
			float extraAngularSpring = 60000f;
			float extraAngularDamper = 0f;

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

			JointDrive angularDrive = new JointDrive { maximumForce = extraAngularForce, positionSpring = extraAngularSpring, positionDamper = extraAngularDamper };
			cfj.angularXDrive = cfj.angularYZDrive = angularDrive; 

			JointDrive linearDrive = new JointDrive { maximumForce = extraLinearForce, positionSpring = extraLinearSpring, positionDamper = extraLinearDamper };
			cfj.xDrive = cfj.yDrive = cfj.zDrive = linearDrive;

			cfj.breakForce = linearForce;
			cfj.breakTorque = torqueForce;

		//	CorrectAttachJoint(part);

		//	GameEvents.onVesselWasModified.Fire(targetVessel);
		//	GameEvents.onDockingComplete.Fire(new GameEvents.FromToAction<Part, Part>(part, targetPart));

			sameVesselJoint = cfj;
		}

		public static void DockVessels(IDockable part, IDockable targetPart)
		{
			if(part.GetPart().vessel != targetPart.GetPart().vessel)
			{
				DockingEvents.onVesselDocking.Fire(part, targetPart);

				DockedVesselInfo vesselInfo, targetVesselInfo;
				Vector3 part_orgPos; Quaternion part_orgRot;

				ExecuteDockVessels(part.GetPart(), part.GetNodeTransform(), part.GetDockingOrientation(),
					targetPart.GetPart(), targetPart.GetNodeTransform(), targetPart.GetDockingOrientation(),
					Math.Min(part.GetSnapCount(), targetPart.GetSnapCount()),
					out vesselInfo, out targetVesselInfo, out part_orgPos, out part_orgRot);

				DockInfo dockInfo = new DockInfo { part = part, targetPart = targetPart, vesselInfo = vesselInfo, targetVesselInfo = targetVesselInfo,
					isSameVesselJoint = false, sameVesselJoint = null };

				part.SetDockInfo(dockInfo);
				targetPart.SetDockInfo(dockInfo);

				DockingEvents.onVesselDocked.Fire(part, targetPart);
			}
			else
			{
				DockingEvents.onSameVesselDocking.Fire(part, targetPart);

				Vector3 part_orgPos; Quaternion part_orgRot;
				ConfigurableJoint _sameVesselJoint = null;

				ExecuteDockSameVessel(part.GetPart(), part.GetNodeTransform(), part.GetDockingOrientation(),
					targetPart.GetPart(), targetPart.GetNodeTransform(), targetPart.GetDockingOrientation(),
					Math.Min(part.GetSnapCount(), targetPart.GetSnapCount()),
					out part_orgPos, out part_orgRot, out _sameVesselJoint);

				DockInfo dockInfo = new DockInfo { part = part, targetPart = targetPart, vesselInfo = null, targetVesselInfo = null,
					isSameVesselJoint = true, sameVesselJoint = _sameVesselJoint };

				part.SetDockInfo(dockInfo);
				targetPart.SetDockInfo(dockInfo);

				DockingEvents.onSameVesselDocked.Fire(part, targetPart);
			}
		}

		private static void RedockVessel(DockInfo di, Vessel vessel)
		{
			if(di.sameVesselJoint)
				UnityEngine.Object.Destroy(di.sameVesselJoint);

			di.isSameVesselJoint = false;
			di.sameVesselJoint = null;

			DockedVesselInfo vesselInfo, targetVesselInfo;
			Vector3 part_orgPos; Quaternion part_orgRot;

			DockInfo dockInfo;

			if(di.targetPart.GetPart().vessel == vessel)
			{
				ExecuteDockVessels(di.part.GetPart(), di.part.GetNodeTransform(), di.part.GetDockingOrientation(),
					di.targetPart.GetPart(), di.targetPart.GetNodeTransform(), di.targetPart.GetDockingOrientation(),
					Math.Min(di.part.GetSnapCount(), di.targetPart.GetSnapCount()),
					out vesselInfo, out targetVesselInfo, out part_orgPos, out part_orgRot);

				dockInfo = new DockInfo { part = di.part, targetPart = di.targetPart, vesselInfo = vesselInfo, targetVesselInfo = targetVesselInfo,
					isSameVesselJoint = false, sameVesselJoint = null };
			}
			else
			{
				ExecuteDockVessels(di.targetPart.GetPart(), di.targetPart.GetNodeTransform(), di.targetPart.GetDockingOrientation(),
					di.part.GetPart(), di.part.GetNodeTransform(), di.part.GetDockingOrientation(),
					Math.Min(di.targetPart.GetSnapCount(), di.part.GetSnapCount()),
					out vesselInfo, out targetVesselInfo, out part_orgPos, out part_orgRot);

				dockInfo = new DockInfo { part = di.targetPart, targetPart = di.part, vesselInfo = vesselInfo, targetVesselInfo = targetVesselInfo,
					isSameVesselJoint = false, sameVesselJoint = null };
			}

			di.part.SetDockInfo(dockInfo);
			di.targetPart.SetDockInfo(dockInfo);
		}

		public static void UndockVessels(IDockable part, IDockable targetPart)
		{
			if(part.GetPart().vessel != targetPart.GetPart().vessel)
				return;

			if(targetPart.GetPart().parent == part.GetPart())
			{ IDockable temp = part; part = targetPart; targetPart = temp; }

			Dictionary<Part, DockInfo> data = FindAllDockInfo(part.GetPart().vessel);

			DockInfo r;
			data.TryGetValue(part.GetPart(), out r);

		//	DockInfo r2;
		//	data.TryGetValue(targetPart.GetPart(), out r2);

			if((r != null) && r.isSameVesselJoint)
			{
				DockingEvents.onSameVesselUndocking.Fire(part, targetPart);

				if(r.sameVesselJoint)
					UnityEngine.Object.Destroy(r.sameVesselJoint);

				part.SetDockInfo(null);
				targetPart.SetDockInfo(null);

				DockingEvents.onSameVesselUndocked.Fire(part, targetPart);
			}
			else
			{
				List<Part> children = FindChildParts(part.GetPart());

				DockInfo di = null;

				foreach(KeyValuePair<Part, DockInfo> kv in data)
				{
					if(children.Contains(kv.Key) != children.Contains(kv.Value.targetPart.GetPart()))
					{
						di = kv.Value;
						break;
					}
				}

				if(di != null)
				{
					DockingEvents.onDockingSwitching.Fire(di.part, part);
					DockingEvents.onDockingSwitching.Fire(di.targetPart, targetPart);

					DockingEvents.onSameVesselUndocking.Fire(part, targetPart);

					Vessel oldVessel = part.GetPart().vessel;

					if((r != null) && (r.vesselInfo != null))
						part.GetPart().Undock(r.vesselInfo);
					else
						part.GetPart().decouple();

					part.SetDockInfo(null);
					targetPart.SetDockInfo(null);

					RedockVessel(di, oldVessel);

					DockingEvents.onDockingSwitched.Fire(di.part, part);
					DockingEvents.onDockingSwitched.Fire(di.targetPart, targetPart);

					DockingEvents.onSameVesselUndocked.Fire(part, targetPart);
				}
				else
				{
					DockingEvents.onVesselUndocking.Fire(part, targetPart);

					if((r != null) && (r.vesselInfo != null))
						part.GetPart().Undock(r.vesselInfo);
					else
						part.GetPart().decouple();

					part.SetDockInfo(null);
					targetPart.SetDockInfo(null);

					DockingEvents.onVesselUndocked.Fire(part, targetPart);

				//	part.fuelLookupTargets.Remove(targetPart);
				//	targetPart.fuelLookupTargets.Remove(part);
				//	GameEvents.onPartFuelLookupStateChange.Fire(new GameEvents.HostedFromToAction<bool, Part>(true, part, targetPart));
				}
			}

// FEHELR, beim undock noch die korrekte Kamera setzen und so weiter und so fort... und, was heisst hier schon "target"? ... na ja...
		}

// FEHLER, mit isSameVesselJoint noch laden lernen
		public static void OnLoad(IDockable part1, DockedVesselInfo vesselInfo1, IDockable part2, DockedVesselInfo vesselInfo2) 
		{
			DockInfo dockInfo = new DockInfo { part = part1, targetPart = part2, vesselInfo = vesselInfo1, targetVesselInfo = vesselInfo2,
				isSameVesselJoint = false, sameVesselJoint = null };

			part1.SetDockInfo(dockInfo);
			part2.SetDockInfo(dockInfo);
		}
	}
}

