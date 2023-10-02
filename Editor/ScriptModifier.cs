using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

public class ScriptModifier: EditorWindow
{
	struct Toggle
	{
		public string name;
		public bool state;
		public bool canToggle;
		public Toggle (string name, bool state, bool canToggle = true)
		{ this.name = name; this.state = state; this.canToggle = canToggle; }
	}

	[MenuItem("Component/Scripts/New Script %&n")]
	public static void ShowWindow()
	{
		GetWindow(typeof(ScriptModifier));
	}

	private bool autoFocus = true;

	private string scriptName = "NewScript";

	// MonoBehaviour booleans
	Toggle[] monoBehaviourBools = {
		new Toggle("Awake()", false),
		new Toggle("OnEnable()", false),
		new Toggle("Start()", true),
		new Toggle("FixedUpdate()", false),
		new Toggle("Update()", true),
		new Toggle("LateUpdate()", false),
		new Toggle("OnDisable()", false)
	};

	// State
	Toggle noneSelect = new Toggle("None", false);

	// Dropdown menu
	public enum UnityClassType
	{
		MonoBehaviour,
		ScriptableObject,
		EditorWindow,
		Editor,
		Singleton,
		Component,
		Custom
	}
	public UnityClassType selectedClassOption;
	public System.Action CurrentAction = null;

	// Scriptable object
	string fileName = string.Empty;
	string menuName = string.Empty;

	// Singleton
	Toggle derivedSingleton = new Toggle("Derive from Singleton", false);
	Toggle dontDestroyOnLoad = new Toggle("DontDestroyOnLoad()", false);
	string singletonContent = string.Empty;

	// Custom
	string customClassName = string.Empty;
	Toggle emptyClass = new Toggle("Empty Class", true);

	// Main
	string contentTemplate;

	private void OnGUI()
	{
		GUILayout.Label("Custom Script Creator", EditorStyles.boldLabel);

		EditorGUI.BeginChangeCheck();

		GUI.SetNextControlName(scriptName);

		scriptName = EditorGUILayout.TextField("Script Name", scriptName);

		if (autoFocus)
		{
			EditorGUI.FocusTextInControl("NewScript");
			autoFocus = false; // Disable autofocus after the first frame
		}

		GUILayout.Space(10);

		// Drop Down menu
		selectedClassOption = (UnityClassType) EditorGUILayout.EnumPopup("Unity Class Type", selectedClassOption);

		SelectUnityClassType(selectedClassOption);

		GUILayout.Space(10);

		if (CurrentAction == null)
		{
			CurrentAction = SelectMonoBehaviour;
		}

		CurrentAction();

		GUILayout.Space(10);

		GUI.enabled = true;

		if (GUILayout.Button("Create Script") || Event.current.keyCode == KeyCode.Return)
		{
			EditorGUI.EndChangeCheck();
			CreateNewScript();
		}

		if (Event.current.keyCode == KeyCode.Escape)
		{
			Close();
		}

	}

	private static void CreateScriptsFolder()
	{
		string parentFolderPath = "Assets";

		if (!AssetDatabase.IsValidFolder(parentFolderPath))
		{
			Debug.LogError("The parent folder 'Assets' does not exist.");
			return;
		}

		string folderGuid = AssetDatabase.CreateFolder(parentFolderPath, "Scripts");
		string createdFolderPath = AssetDatabase.GUIDToAssetPath(folderGuid);

		if (!string.IsNullOrEmpty(createdFolderPath))
		{
			Debug.Log("The 'Assets/Scripts' folder has been created.");
		}
		else
		{
			Debug.LogError("Failed to create the 'Assets/Scripts' folder.");
		}
	}

	void DeriveFromSingleton()
	{
		if (derivedSingleton.state && selectedClassOption == UnityClassType.Singleton)
		{
			string scriptsFolderPath = "Assets/Scripts";

			if (!AssetDatabase.IsValidFolder(scriptsFolderPath))
			{
				CreateScriptsFolder();
			}

			CreateSingletonScript();
		}
	}
	
	void EmptyClass()
	{
		if (emptyClass.state)
		{
			customClassName = string.Empty;
		} 
	}

	void NoneSelect()
	{
		if (noneSelect.state)
		{
			for (int i = 0; i < monoBehaviourBools.Length; i++ )
			{
				monoBehaviourBools[i].canToggle = false;
			}
		} else
		{
			for (int i = 0; i < monoBehaviourBools.Length; i++)
			{
				monoBehaviourBools[i].canToggle = true;
			}
		}
	}

	void CreateTextField(ref string dataName, string label, bool disable = false)
	{
		GUI.enabled = !disable;

		dataName = EditorGUILayout.TextField(label, dataName);
	}

	void CreateToggle(ref Toggle toggle, System.Action YourMethod = null, float width = 100)
	{
		GUILayout.BeginHorizontal();

		GUILayout.Label(toggle.name, GUILayout.Width(width));

		GUI.enabled = toggle.canToggle;
		toggle.state = GUILayout.Toggle(toggle.state, string.Empty);
		GUI.enabled = true;

		GUILayout.EndHorizontal();

		if (YourMethod != null)
		{
			YourMethod();
		}
	}

	void SelectMonoBehaviour()
	{
		CreateToggle(ref noneSelect, NoneSelect);

		GUILayout.Space(10);

		for (int i = 0; i < monoBehaviourBools.Length; i++)
		{
			CreateToggle(ref monoBehaviourBools[i]);
		}
	}

	void SelectScriptableObject()
	{
		CreateTextField(ref fileName, "File Name");
		CreateTextField(ref menuName, "Menu Name");
	}

	void SelectEditorWindow()
	{

	}

	void SelectEditor()
	{

	}

	void SelectComponent()
	{

	}

	void SelectCustom()
	{
		CreateToggle(ref emptyClass, EmptyClass);
		CreateTextField(ref customClassName, "Base Class", emptyClass.state);
	}

	void SelectSingleton()
	{
		CreateToggle(ref derivedSingleton, null, 130);
		CreateToggle(ref dontDestroyOnLoad, null, 130);
	}

	void SelectUnityClassType(UnityClassType selectedClass)
	{
		void SubMethod(System.Action newAction)
		{
			CurrentAction = newAction;
			UpdateMainTemplate(selectedClass);
		}

		switch (selectedClass)
		{
			case UnityClassType.MonoBehaviour:
				SubMethod(SelectMonoBehaviour);
				break;
			case UnityClassType.ScriptableObject:
				SubMethod(SelectScriptableObject);
				break;
			case UnityClassType.EditorWindow:
				SubMethod(SelectEditorWindow);
				break;
			case UnityClassType.Editor:
				SubMethod(SelectEditor);
				break;
			case UnityClassType.Singleton:
				SubMethod(SelectSingleton);
				break;
			case UnityClassType.Component:
				SubMethod(SelectComponent);
				break;
			case UnityClassType.Custom:
				SubMethod(SelectCustom);
				break;
		}
	}

	void CreateSingletonScript()
	{
		

		string selectedFolder = "Assets/Scripts"; // Change this to your desired folder path
		string scriptName = "Singleton"; // Change this to your desired script name

		string singletonScriptPath = System.IO.Path.Combine(selectedFolder, scriptName + ".cs");

		if (File.Exists(singletonScriptPath)) { return; }

		// Create the script file with a default template
		System.IO.File.WriteAllText(singletonScriptPath, singletonContent);
		/*AssetDatabase.Refresh();

		// Select the newly created script in the Unity Editor
		Object createdScript = AssetDatabase.LoadAssetAtPath(singletonScriptPath, typeof(Object));
		Selection.activeObject = createdScript;
		EditorGUIUtility.PingObject(createdScript);*/

		Close();
	}

	bool FindScript(string scriptName)
	{
		string[] scriptFiles = System.IO.Directory.GetFiles("Assets", "*.cs", System.IO.SearchOption.AllDirectories);

		foreach (string scriptFile in scriptFiles)
		{
			string fileName = System.IO.Path.GetFileNameWithoutExtension(scriptFile);
			if (fileName == scriptName)
			{
				return true;
			}
		}

		return false;
	}

	private void CreateNewScript()
	{
		DeriveFromSingleton();

		// Get the currently selected folder in the Unity Editor
		string selectedFolder = "Assets";
        Object selectedObject = Selection.activeObject;
		if (selectedObject != null)
		{
			string selectedPath = AssetDatabase.GetAssetPath(selectedObject.GetInstanceID());
			if (!string.IsNullOrEmpty(selectedPath))
			{
				selectedFolder = selectedPath;
				if (!System.IO.Directory.Exists(selectedPath))
				{
					selectedFolder = System.IO.Path.GetDirectoryName(selectedPath);
				}
			}
		}

		scriptName = scriptName.Replace(" ", "");

		string scriptPath = System.IO.Path.Combine(selectedFolder, scriptName + ".cs");

		if (FindScript(scriptName)) { Debug.LogWarning("This script is already created. Be sure you named it correctly!"); return; }

		// Create the script file with a default template
		System.IO.File.WriteAllText(scriptPath, contentTemplate);
		AssetDatabase.Refresh();

		// Select the newly created script in the Unity Editor
		Object createdScript = AssetDatabase.LoadAssetAtPath(scriptPath, typeof(Object));
		Selection.activeObject = createdScript;
		EditorGUIUtility.PingObject(createdScript);

		Close();
	}

	string MethodTemplate(string methodName, bool giveSpace = true, string returnType = "void")
	{
		if (giveSpace)
		{
			return "\n\t" + returnType + " " + methodName + "\n\t{\n\n\t}\n";
		} else
		{
			return "\n\t" + returnType + " " + methodName + "\n\t{\n\n\t}";
		}
	}

	void UpdateMainTemplate(UnityClassType selectedClassOption)
	{
		string newTemplate = string.Empty;

		switch (selectedClassOption)
		{
			case UnityClassType.MonoBehaviour:

				newTemplate = string.Empty;

				newTemplate += $"using System.Collections;\nusing System.Collections.Generic;\nusing UnityEngine;\n\npublic class {scriptName} : {selectedClassOption}\n{{";

				if (!noneSelect.state)
				{
					for (int i = 0; i < monoBehaviourBools.Length; i++)
					{
						if (monoBehaviourBools[i].state)
						{
							if (i == monoBehaviourBools.Length - 1)
							{
								newTemplate += MethodTemplate(monoBehaviourBools[i].name, false); ;
							}
							else
							{
								newTemplate += MethodTemplate(monoBehaviourBools[i].name); ;
							}
						}
					}
				} else
				{
					newTemplate += "\n\n";
				}

				break;

			case UnityClassType.ScriptableObject:
				newTemplate = string.Empty;

				newTemplate += "using UnityEngine;\n\n[CreateAssetMenu(";

				if (!fileName.Equals(""))
				{
					newTemplate += $"fileName = \"{fileName}\", ";
				}

				if (!menuName.Equals(""))
				{
					newTemplate += $"menuName = \"{menuName}\")]";
				} else
				{
					newTemplate += $"menuName = \"{scriptName}\")]";
				}

				newTemplate += $"\npublic class {scriptName} : {selectedClassOption}\n{{\n\n";

				break;

			case UnityClassType.EditorWindow:

				newTemplate = string.Empty;

				newTemplate += $"using UnityEditor;\nusing UnityEngine;\n\npublic class {scriptName} : {selectedClassOption}\n{{\n\n";

				break;

			case UnityClassType.Editor:

				newTemplate = string.Empty;

				newTemplate += $"using UnityEditor;\nusing UnityEngine;\n\npublic class {scriptName} : {selectedClassOption}\n{{\n\n";

				break;

			case UnityClassType.Singleton:

				newTemplate = string.Empty;
				singletonContent = string.Empty;

				if (derivedSingleton.state)
				{
					newTemplate += $"using System.Collections;\r\nusing System.Collections.Generic;" +
						$"\r\nusing UnityEngine;\r\n\r\npublic class {scriptName} : Singleton<{scriptName}>\r\n{{\r\n\t\r\n"; 

					if (dontDestroyOnLoad.state)
					{
						singletonContent = $"using System.Collections;" +
							$"\r\nusing System.Collections.Generic;\r\nusing UnityEngine;" +
							$"\r\n\r\npublic class Singleton<T> : MonoBehaviour where T : Component" +
							$"\r\n{{\r\n\tprivate static T instance;\r\n\r\n\tpublic static T Instance" +
							$"\r\n\t{{\r\n\t\tget\r\n\t\t{{\r\n\t\t\tif (instance == null)\r\n\t\t\t{{\r\n\t\t\t\tinstance = FindObjectOfType<T>();" +
							$"\r\n\r\n\t\t\t\tif (instance == null)\r\n\t\t\t\t{{\r\n\t\t\t\t\tGameObject singletonObj = new GameObject();" +
							$"\r\n\t\t\t\t\tsingletonObj.name = typeof(T).Name;\r\n\t\t\t\t\tinstance = singletonObj.AddComponent<T>();" +
							$"\r\n\t\t\t\t}}\r\n\t\t\t}}\r\n\r\n\t\t\treturn instance;\r\n\t\t}}\r\n\t}}\r\n\r\n\tprivate void Awake()" +
							$"\r\n\t{{\r\n\t\tif (instance != null && instance != this)\r\n\t\t{{\r\n\t\t\tDestroy(gameObject);" +
							$"\r\n\t\t\treturn;\r\n\t\t}}\r\n\r\n\t\tinstance = this as T;\r\n\t\tDontDestroyOnLoad(gameObject);\r\n\t}}\r\n}}";
					} else
					{
						singletonContent = $"using System.Collections;" +
							$"\r\nusing System.Collections.Generic;\r\nusing UnityEngine;" +
							$"\r\n\r\npublic class Singleton <T> : MonoBehaviour where T : Component\r\n{{\r\n\t" +
							$"private static T instance;\r\n\r\n    public static T Instance\r\n\t{{\r\n\t\tget\r\n\t\t{{\r\n\t\t\t" +
							$"if (instance == null)\r\n\t\t\t{{\r\n\t\t\t\tinstance = FindObjectOfType<T>();\r\n\r\n\t\t\t\t" +
							$"if (instance == null)\r\n\t\t\t\t{{\r\n\t\t\t\t\tGameObject singletonObj = new GameObject();" +
							$"\r\n\t\t\t\t\tsingletonObj.name = typeof(T).Name;\r\n\t\t\t\t\tinstance = singletonObj.AddComponent<T>();" +
							$"\r\n\t\t\t\t}}\r\n\t\t\t}}\r\n\r\n\t\t\treturn instance;\r\n\t\t}}\r\n\t}}\r\n\r\n\tprivate void Awake()" +
							$"\r\n\t{{\r\n\t\tif (instance != null)\r\n\t\t{{\r\n\t\t\tDestroy(gameObject);\t\r\n\t\t}}\r\n\t\t" +
							$"else\r\n\t\t{{\r\n\t\t\tinstance = GetComponent<T>();\r\n\t\t}}\r\n\t}}\r\n}}";
					}

				} else
				{
					newTemplate += $"using UnityEditor;{Line()}using UnityEngine;{Line(2)}public class {scriptName} : {UnityClassType.MonoBehaviour}{Line()}{{{Line()}";

					newTemplate += $"{Tab()}public static {scriptName} Instance {{ get; private set; }}{Line(2)}";
					newTemplate += $"{Tab()}private void Awake(){Line()}{Tab()}{{{Line()}{Tab(2)}if (Instance == null){Line()}{Tab(2)}{{{Line()}{Tab(3)}Instance = this;{Line()}";
					if (dontDestroyOnLoad.state)
					{
						newTemplate += $"{Tab(3)}DontDestroyOnLoad(gameObject);{Line()}";
					}
					newTemplate += $"{Tab(2)}}}{Line()}{Tab(2)}else if (Instance != this){Line()}{Tab(2)}{{{Line()}{Tab(3)}Destroy(gameObject);{Line()}{Tab(2)}}}{Line()}{Tab()}}}{Line()}";
				}

				break;

			case UnityClassType.Component: 

				newTemplate = string.Empty;

				newTemplate += $"\nusing UnityEngine;\n\npublic class {scriptName} : {selectedClassOption}\n{{\n\n";

				break;

			case UnityClassType.Custom:

				newTemplate = string.Empty;

				if (customClassName.Equals(""))
				{
					newTemplate += $"using System.Collections;\nusing System.Collections.Generic;\nusing UnityEngine;\n\npublic class {scriptName}\n{{\n\n";

				} else
				{
					newTemplate += $"using System.Collections;\nusing System.Collections.Generic;\nusing UnityEngine;\n\npublic class {scriptName} : {customClassName}\n{{\n\n";
				}

				break;

			default: break;
		}

		contentTemplate = newTemplate + "}";
	}

	string Tab(int count = 1)
	{
		string tab = string.Empty;

		for (int i = 0; i < count; i++)
		{
			tab += "\t";
		}

		return tab;
	}

	string Line(int count = 1)
	{
		string newLine = string.Empty;

		for (int i = 0; i < count; i++)
		{
			newLine += "\n";
		}

		return newLine;
	}
}
