using System;
using System.Collections.Generic;
using UnityEngine;

namespace AttachmentAndDockingTools
{
	[KSPAddon(KSPAddon.Startup.Flight, false)]
    public class ShipCoordinator : MonoBehaviour
    {
		public virtual String AddonName { get; set; }

		protected static ShipCoordinator Instance = null;

		protected struct RegisteredShip
		{
			public Vessel vessel;
			public Part followedPart;

			public Vector3 relativePosition;
			public Quaternion relativeRotation;
		};

		private void Awake()
		{
			Instance = this;
			registeredShips = new List<RegisteredShip>();
		}

		protected List<RegisteredShip> registeredShips;

		public static void Register(Vessel vessel, Part followedPart, Vector3 relativePosition, Quaternion relativeRotation)
		{
			Instance.registeredShips.Add(
				new RegisteredShip { vessel = vessel, followedPart = followedPart, relativePosition = relativePosition, relativeRotation = relativeRotation });

// FEHLER, hier die Reihenfolge prüfen und alles umstellen, wen die Hierarchie nicht stimmen würde
		}

		public static void Register(Part part, Part followedPart, bool usePristineCoords, out Vector3 relativePosition, out Quaternion relativeRotation)
		{
			if(usePristineCoords)
			{
				Quaternion rootRotation = part.transform.rotation * Quaternion.Inverse(part.orgRot);
				Vector3 rootPosition = part.transform.position - rootRotation * part.orgPos;

				relativePosition = Quaternion.Inverse(followedPart.transform.rotation) * (rootPosition - followedPart.transform.position);
				relativeRotation = Quaternion.Inverse(followedPart.transform.rotation) * rootRotation;
			}
			else
			{
				relativePosition = Quaternion.Inverse(followedPart.transform.rotation) * (part.vessel.transform.position - followedPart.transform.position);
				relativeRotation = Quaternion.Inverse(followedPart.transform.rotation) * part.vessel.transform.rotation;
			}

			Register(part.vessel, followedPart, relativePosition, relativeRotation);
		}

		public static void Unregister(Vessel vessel)
		{
			for(int i = 0; i < Instance.registeredShips.Count; i++)
			{
				if(Instance.registeredShips[i].vessel == vessel)
					Instance.registeredShips.RemoveAt(i--);
			}
		}

		private void FixedUpdate()
		{
			for(int i = 0; i < registeredShips.Count; i++)
			{
				RegisteredShip r = registeredShips[i];

				if(r.vessel.packed)
				{
					r.vessel.SetRotation(r.followedPart.transform.rotation * r.relativeRotation, true);
					r.vessel.SetPosition(r.followedPart.transform.position + r.followedPart.transform.rotation * r.relativePosition, false);
				}
			}
		}

		private void OnDestroy()
		{
		}
    }
}
