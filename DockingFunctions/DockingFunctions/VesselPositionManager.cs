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

		protected static int idCounter = 0;

		protected class RegisteredVessel
		{
			public int id;

			public Part part;
			public Part followedPart;

			public Vessel vessel;

			public Transform reference;
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
				if(Instance.registeredVessels[i].part.vessel == vessel)
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

		protected static void Calculate(RegisteredVessel r, Transform reference)
		{
			r.vessel = r.part.vessel;
			r.reference = reference;

			r.relativePosition = Quaternion.Inverse(r.reference.rotation) * (r.vessel.transform.position - r.reference.position);
			r.relativeRotation = Quaternion.Inverse(r.reference.rotation) * r.vessel.transform.rotation;
		}

		/*
		 * Description:
		 *     Registers a connection between two parts and returns relativePosition, relativeRotation
		 *     for a later use in ReRegister (e.g. after a load).
		 *     
		 * Remarks:
		 *     It is recommended that followedPart.vessel is dominant over part.vessel.
		*/
		public static int Register(Part part, Part followedPart)
		{
			bool bSwapped = false;

			int i = 0;

			while(i < Instance.registeredVessels.Count)
			{
				if(Instance.registeredVessels[i].part.vessel == part.vessel)
				{
					if(bSwapped)
						return 0;
				
					Part temp = part; part = followedPart; followedPart = temp;
					bSwapped = true;
					i = 0;
				}
				else
					++i;
			}

			int id = ++idCounter;

			Instance.registeredVessels.Add(new RegisteredVessel { id = id, part = part, followedPart = followedPart });

			List<RegisteredVessel> rv = new List<RegisteredVessel>(Instance.registeredVessels);
			Dictionary<Vessel, Vessel> tgt = new Dictionary<Vessel, Vessel>();

			while(rv.Count > 0)
			{
				for(int j = 0; j < rv.Count; j++)
				{
					int k = 0;
					while((k < rv.Count) && (rv[j].followedPart.vessel != rv[k].part.vessel))
						++k;

					if(k >= rv.Count)
					{
						Vessel v;

						if(!tgt.TryGetValue(rv[j].followedPart.vessel, out v))
							v = rv[j].followedPart.vessel;

						Calculate(rv[j], v.transform);
						tgt.Add(rv[j].part.vessel, v);

						rv.RemoveAt(j--);
					}
				}
			}

			return id;
		}

		/*
		 * Description:
		 *     Unregisters a connection between two parts.
		*/
		public static void Unregister(int id)
		{
			for(int i = 0; i < Instance.registeredVessels.Count; i++)
			{
				if(Instance.registeredVessels[i].id == id)
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
						r.vessel.SetRotation(r.reference.rotation * r.relativeRotation, true);
						r.vessel.SetPosition(r.reference.position + r.reference.rotation * r.relativePosition, false);
					}
				}
				catch(Exception)
				{
					if((r.part == null) || (r.followedPart == null) || (r.vessel == null) || (r.reference == null))
						registeredVessels.RemoveAt(i--);
				}
			}
		}

		private void OnDestroy()
		{
		}
    }
}
