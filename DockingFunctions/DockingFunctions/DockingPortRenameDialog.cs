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
		//	component.title.text = Localizer.Format("#autoLOC_900678");
			component.transform.SetParent(KSP.UI.UIMasterController.Instance.dialogCanvas.transform, worldPositionStays: false);
			component.portName = name;
			component.onAccept = onAccept;
			component.onCancel = onCancel;
			return component;
		}

		protected void Start()
		{
			var dlg = gameObject.GetChild("Dialog");

			nameField = dlg.GetChild("InputField").GetComponent<TMP_InputField>();

			var footer = dlg.GetChild("footer");

			buttonAccept = footer.GetChild("ButtonAccept").GetComponent<Button>();
			buttonCancel = footer.GetChild("ButtonCancel").GetComponent<Button>();

			nameField.text = portName;
		//	nameField.onValueChanged.AddListener(OnNameFieldModified);
		//	nameField.onEndEdit.AddListener(OnNameFieldEndEdit);
		//	nameFieldClickHandler = nameField.GetComponent<PointerClickHandler>();
		//	nameFieldClickHandler.onPointerClick.AddListener(OnNameFieldSelected);
			buttonAccept.onClick.AddListener(OnButtonAccept);
		//	buttonAccept.interactable = Vessel.IsValidVesselName(vesselName);
			buttonCancel.onClick.AddListener(OnButtonCancel);
		}

		public void Terminate()
		{
			Object.Destroy(gameObject);
		}
	/*
		protected void OnNameFieldModified(string newName)
		{
			hasValidName = Vessel.IsValidVesselName(newName);
			if (hasValidName)
			{
				vesselName = newName;
				buttonAccept.interactable = true;
			}
			else
			{
				buttonAccept.interactable = false;
			}
		}
	
		protected void OnNameFieldEndEdit(string s)
		{
			InputLockManager.RemoveControlLock("VesselRenameDialogTextInput");
		}

		protected void OnNameFieldSelected(PointerEventData eventData)
		{
			InputLockManager.SetControlLock(ControlTypes.KEYBOARDINPUT, "VesselRenameDialogTextInput");
		}
	*/
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

