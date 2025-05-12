using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
using UnityEngine.Networking;

namespace DockingFunctions
{
	[KSPAddon(KSPAddon.Startup.MainMenu, false)]
	public class UIAssetsLoader : MonoBehaviour
	{
		internal static AssetBundle AssetBundle;

		// windows
		internal static GameObject windowPrefab;

		public static bool allPrefabsReady = false;

		public IEnumerator LoadBundle(string location)
		{
			while(!Caching.ready)
				yield return null;

			using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(location))
			{
				yield return www.SendWebRequest();

				AssetBundle = DownloadHandlerAssetBundle.GetContent(www);

				LoadBundleAssets();
			}
		}
		
		private void LoadBundleAssets()
		{
			var prefabs = AssetBundle.LoadAllAssets<GameObject>();
			int prefabCounter = 0;

			for(int i = 0; i < prefabs.Length; i++)
			{
				if(prefabs[i].name == "DockingPortRenameDialog")
				{
					windowPrefab = prefabs[i] as GameObject;
					prefabCounter++;
		//			Logger.Log("Successfully loaded control window prefab", Logger.Level.Debug);
				}
			}

			allPrefabsReady = (prefabCounter > 0);
		}

		public void Start()
		{
			if(allPrefabsReady)
				return;

			var assemblyFile = Assembly.GetExecutingAssembly().Location;
			var bundlePath = "file://" + assemblyFile.Replace(new FileInfo(assemblyFile).Name, "").Replace("\\","/") + "../AssetBundles/";

		//	Logger.Log("Loading bundles from BundlePath: " + bundlePath, Logger.Level.Debug);

			Caching.ClearCache();

			StartCoroutine(LoadBundle(bundlePath + "dockingfunctions.ksp"));
		}

		public void OnDestroy()
		{
		}
	}
}

