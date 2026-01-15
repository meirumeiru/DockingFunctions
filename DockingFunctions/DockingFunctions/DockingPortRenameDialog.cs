using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace DockingFunctions
{
	public class DockingPortRenameDialog : MonoBehaviour
	{
		private TMP_InputField nameField;

		private Button buttonAccept;
		private Button buttonCancel;

		private string portName = string.Empty;

		private Callback<string> onAccept;
		private Callback onCancel;

		public static DockingPortRenameDialog Spawn(string name, Callback<string> onAccept, Callback onCancel)
		{
			DockingPortRenameDialog component = Object.Instantiate(UIAssetsLoader.windowPrefab).AddComponent<DockingPortRenameDialog>();
			component.transform.GetChild(0).gameObject.AddComponent<DragPanel>();
			component.transform.SetParent(KSP.UI.UIMasterController.Instance.dialogCanvas.transform, worldPositionStays: false);
			component.portName = name;
			component.onAccept = onAccept;
			component.onCancel = onCancel;
			return component;
		}

		private const ControlTypes MyLocks =
			ControlTypes.ALL_SHIP_CONTROLS | ControlTypes.EVA_INPUT
			| ControlTypes.ACTIONS_ALL     | ControlTypes.GROUPS_ALL
			| ControlTypes.THROTTLE        | ControlTypes.TIMEWARP
			| ControlTypes.MISC            // Stage locking (mod-L)
			| ControlTypes.MAP_TOGGLE      // M
			| ControlTypes.STAGING         // Space
			| ControlTypes.CAMERACONTROLS  // Backspace
			| ControlTypes.UI_DIALOGS;     // Navball toggle

		protected void Start()
		{
			var dlg = gameObject.GetChild("Dialog");

			nameField = dlg.GetChild("InputField").GetComponent<TMP_InputField>();

			nameField.onSelect.AddListener(v => { InputLockManager.SetControlLock(MyLocks, "DockingFunctionsControlLock"); });
			nameField.onDeselect.AddListener(v => { InputLockManager.RemoveControlLock("DockingFunctionsControlLock"); });

			nameField.text = portName;

			var footer = dlg.GetChild("footer");

			buttonAccept = footer.GetChild("ButtonAccept").GetComponent<Button>();
			buttonCancel = footer.GetChild("ButtonCancel").GetComponent<Button>();

			buttonAccept.onClick.AddListener(OnButtonAccept);
			buttonCancel.onClick.AddListener(OnButtonCancel);
		}

		public void Terminate()
		{
			Object.Destroy(gameObject);
		}

		protected void OnButtonAccept()
		{
			onAccept(nameField.text);
			Terminate();
		}

		protected void OnButtonCancel()
		{
			onCancel();
			Terminate();
		}

		protected void Update()
		{
	/*		if(Input.GetKeyDown(KeyCode.Return) && !nameField.isFocused && hasValidName)
			{
				OnButtonAccept();
			}
			if(Input.GetKeyDown(KeyCode.Escape) && !nameField.isFocused)
			{
				OnButtonCancel();
			}
	*/	}
	}
}

