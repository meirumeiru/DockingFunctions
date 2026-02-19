using System;
using System.Collections.Generic;
using UnityEngine;

namespace DockingFunctions
{
	[KSPAddon(KSPAddon.Startup.Flight, false)]
    public class VesselPositionManager : MonoBehaviour
    {
		public virtual String AddonName { get; set; }

		protected static VesselPositionManager Instance = null;

		protected struct RegisteredVessel
		{
			public Vessel vessel;
			public Part followedPart;

			public Vector3 relativePosition;
			public Quaternion relativeRotation;
		};

		public void Awake()
		{
			Instance = this;
			registeredVessels = new List<RegisteredVessel>();
		}

		protected List<RegisteredVessel> registeredVessels;

		public static bool IsFollowing(Vessel vessel)
		{
			for(int i = 0; i < Instance.registeredVessels.Count; i++)
			{
				if(Instance.registeredVessels[i].vessel == vessel)
					return true;
			}

			return false;
		}

		public static bool IsFollowed(Vessel vessel)
		{
			for(int i = 0; i < Instance.registeredVessels.Count; i++)
			{
				if(Instance.registeredVessels[i].followedPart.vessel == vessel)
					return true;
			}

			return false;
		}

		/*
		 * Description:
		 *     Inserts a connection manually (e.g. a previously registered connection after a load).
		*/
		public static void Insert(Vessel vessel, Part followedPart, Vector3 relativePosition, Quaternion relativeRotation)
		{
			int i = 0;
			
			while((i < Instance.registeredVessels.Count)
			   && (Instance.registeredVessels[i].followedPart.vessel != vessel))
				++i;

			Instance.registeredVessels.Insert(i,
				new RegisteredVessel { vessel = vessel, followedPart = followedPart, relativePosition = relativePosition, relativeRotation = relativeRotation });
		}

		/*
		 * Description:
		 *     Registers a connection between two parts and returns relativePosition, relativeRotation
		 *     for a later use in ReRegister (e.g. after a load).
		 *     
		 * Remarks:
		 *     It is recommended that followedPart.vessel is dominant over part.vessel.
		*/
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

			Insert(part.vessel, followedPart, relativePosition, relativeRotation);
		}

		/*
		 * Description:
		 *     Unregisters a connection between two parts.
		*/
		public static void Unregister(Vessel vessel)
		{
			for(int i = 0; i < Instance.registeredVessels.Count; i++)
			{
				if(Instance.registeredVessels[i].vessel == vessel)
					Instance.registeredVessels.RemoveAt(i--);
			}
		}

		private void FixedUpdate()
		{
			for(int i = 0; i < registeredVessels.Count; i++)
			{
				RegisteredVessel r = registeredVessels[i];

				try
				{
					if(r.vessel.packed)
					{
						r.vessel.SetRotation(r.followedPart.transform.rotation * r.relativeRotation, true);
						r.vessel.SetPosition(r.followedPart.transform.position + r.followedPart.transform.rotation * r.relativePosition, false);
					}
				}
				catch(Exception)
				{
					if((r.vessel == null) || (r.followedPart == null))
						registeredVessels.RemoveAt(i--);
				}
			}
		}

		private void OnDestroy()
		{
		}
    }
}
