using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

namespace PrefabCreator {
	public class AddNewPrefabWindow : EditorWindow
	{
		public static readonly List<string> imageExtensions = new List<string> { ".png", ".psd", ".tif", ".tiff" };
		public static readonly List<string> meshExtensions = new List<string> { ".fbx" };

		public PrefabFolderContent content = null;
		private DirectoryInfo lastSelectedDirectory;
		private string warnings;

		private string selectedDirectory = "";
		private string filesToSelect = "";
		private Vector2 scrollAmount = Vector2.zero;

		private bool overwriteExistingFiles = false;
		private bool deleteCollisionMesh = false;

		[MenuItem("Project/Asset Tools/Add New Prefab")]
		static void Init()
		{
			// Get existing open window or if none, make a new one:
			AddNewPrefabWindow window = GetWindow<AddNewPrefabWindow>("Add Prefab");
			window.Show();
		}

		#region OnGUI
		void OnGUI()
		{
			scrollAmount = EditorGUILayout.BeginScrollView(scrollAmount);
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Add a new prefab from folder", EditorStyles.boldLabel);
			EditorGUILayout.Space();

			if (GUILayout.Button("Select Folder"))
			{
				OpenFolder();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();

			selectedDirectory = EditorGUILayout.TextField("Selected Folder", selectedDirectory);

			EditorGUILayout.Space();
			filesToSelect = EditorGUILayout.TextField("Select files with name", filesToSelect);
			EditorGUILayout.Space();

			if (GUILayout.Button("Retrieve Files from Folder"))
			{
				RetrieveFiles();
			}

			if (content == null || content.directory == null)
			{
				EditorGUILayout.EndScrollView();
				return;
			}

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Selected prefab", EditorStyles.boldLabel);
			EditorGUILayout.Space();

			
			content.prefabName = EditorGUILayout.TextField("Prefab Name", content.prefabName);
			content.prefabType = (PrefabCreator.PrefabType)EditorGUILayout.EnumPopup("Prefab Type", content.prefabType);
			
			if(content.prefabType == PrefabCreator.PrefabType.CUSTOM)
            {
				content.customPrefabTag = EditorGUILayout.TextField("Custom Prefab Tag", content.customPrefabTag);
				content.customPrefabPath = EditorGUILayout.TextField("Custom Prefab Path", content.customPrefabPath);
            }

			if (NewPrefabCreator.PrefabNameExists(content.prefabName, content.prefabType))
			{
				EditorGUILayout.HelpBox("A prefab with this name and type already exists. To overwrite it, select the 'overwrite existing files'-box below.", MessageType.Warning);
			}

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Files in folder", EditorStyles.boldLabel);
			EditorGUILayout.Space();

			GUI.enabled = false;
			EditorGUILayout.TextField("Mesh", content.fileMesh != null ? content.fileMesh.Name : "None");
			EditorGUILayout.TextField("Collision Mesh", content.fileCollisionMesh != null ? content.fileCollisionMesh.Name : "None");
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Material Textures", EditorStyles.boldLabel);
			foreach (FolderContentMaterial mat in content.materials)
			{
				GUI.enabled = true;
				EditorGUILayout.LabelField("      " + mat.materialName, EditorStyles.boldLabel);
                mat.materialName = EditorGUILayout.TextField("Material Name", mat.materialName);
				
				if (NewPrefabCreator.MaterialNameExists(mat.materialName, content.prefabType))
				{
					EditorGUILayout.HelpBox("A material with this name and type already exists. To overwrite it, select the 'overwrite existing files'-box below.", MessageType.Warning);
				}
				GUI.enabled = false;
				EditorGUILayout.TextField("Albedo", mat.fileAlbedo != null ? mat.fileAlbedo.Name : "None");
				EditorGUILayout.TextField("Metallic", mat.fileMetallic != null ? mat.fileMetallic.Name : "None");
				EditorGUILayout.TextField("Normal", mat.fileNormal != null ? mat.fileNormal.Name : "None");
				EditorGUILayout.TextField("AO", mat.fileAO != null ? mat.fileAO.Name : "None");
				EditorGUILayout.TextField("Mask", mat.fileMask != null ? mat.fileMask.Name : "None");
				EditorGUILayout.Space();
			}
			GUI.enabled = true;

			overwriteExistingFiles = EditorGUILayout.ToggleLeft("Overwrite Existing Files", overwriteExistingFiles);
			deleteCollisionMesh = EditorGUILayout.ToggleLeft("Delete collision mesh after use", deleteCollisionMesh);
			if (!string.IsNullOrEmpty(warnings))
			{
				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Remarks", EditorStyles.boldLabel);
				EditorGUILayout.Space();

				EditorGUILayout.HelpBox(warnings, MessageType.Warning);
			}

			EditorGUILayout.Space();
			if (content.prefabType != PrefabCreator.PrefabType.NONE && !(content.prefabType == PrefabCreator.PrefabType.CUSTOM && string.IsNullOrEmpty(content.customPrefabPath)))
			{
				if (GUILayout.Button("Create prefab"))
				{
					NewPrefabCreator.CreateNewPrefab(content, false, overwriteExistingFiles);
					if (deleteCollisionMesh)
						NewPrefabCreator.DeleteCollisionMesh(content);
				}
				EditorGUILayout.Space();

				GUI.color = Color.red;

				if (GUILayout.Button("Add as raw assets"))
				{
					NewPrefabCreator.CreateNewPrefab(content, true, overwriteExistingFiles);
				}

				GUI.color = Color.white;
			}

			EditorGUILayout.Space();
			EditorGUILayout.Space();

			#region Ping Buttons
			foreach (FolderContentMaterial mat in content.materials)
			{
				if(!(string.IsNullOrEmpty(mat.assetPathMaterial)))
					EditorGUILayout.LabelField("     " + mat.materialName, EditorStyles.boldLabel);
				if (!string.IsNullOrEmpty(mat.assetPathAlbedo))
					if (GUILayout.Button("Ping Texture Albedo"))
						EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(mat.assetPathAlbedo));

				if (!string.IsNullOrEmpty(mat.assetPathMetallic))
					if (GUILayout.Button("Ping Texture Metallic"))
						EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(mat.assetPathMetallic));

				if (!string.IsNullOrEmpty(mat.assetPathNormal))
					if (GUILayout.Button("Ping Texture Normal"))
						EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(mat.assetPathNormal));

				if (!string.IsNullOrEmpty(mat.assetPathAO))
					if (GUILayout.Button("Ping Texture AO"))
						EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(mat.assetPathAO));

				if (!string.IsNullOrEmpty(mat.assetPathMask))
					if (GUILayout.Button("Ping Texture Mask"))
						EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(mat.assetPathMask));


				if (!string.IsNullOrEmpty(mat.assetPathMaterial))
					if (GUILayout.Button("Ping Material"))
						EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(mat.assetPathMaterial));
				EditorGUILayout.Space();
			}
			EditorGUILayout.Space();
			if (!string.IsNullOrEmpty(content.assetPathMesh))
				if (GUILayout.Button("Ping Mesh"))
					EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(content.assetPathMesh));

			if (!string.IsNullOrEmpty(content.assetPathCollisionMesh))
				if (GUILayout.Button("Ping Collision Mesh"))
					EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(content.assetPathCollisionMesh));

			if (!string.IsNullOrEmpty(content.assetPathPrefab))
				if (GUILayout.Button("Ping Prefab"))
					EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(content.assetPathPrefab));
			#endregion
			EditorGUILayout.EndScrollView();

		}
		#endregion

		#region open folder
		private void OpenFolder()
		{
			//Last selected path is saved and used as default starting path
			string startLocation = "";
			if(lastSelectedDirectory != null)
			{
				startLocation = lastSelectedDirectory.Parent.FullName;
			}

			selectedDirectory = EditorUtility.OpenFolderPanel("Select folder containing asset files", startLocation, "");

			//if no directory is selected, end the function
			if(string.IsNullOrEmpty(selectedDirectory))
			{
				Debug.Log("[" + this + "] No folder selected");
				warnings += "No folder Selected\n";
				return;
			}

			lastSelectedDirectory = new DirectoryInfo(selectedDirectory);	
		}
		#endregion

		#region Sort files
		private void RetrieveFiles()
		{
			//reset any previous values to prepare for OpenFolder
			content = new PrefabFolderContent();
			warnings = "";

			//enter directory information into folderContent
			content.directory = new DirectoryInfo(selectedDirectory);
			content.prefabName = content.directory.Name;
			FileInfo[] files = content.directory.GetFiles();

			//enter information from each file in folder into folderContent
			foreach (FileInfo file in files)
			{
				if (IsToBeImported(file))
				{
					SortFile(file);
				}
			}
		}

		private void SortFile(FileInfo file)
		{
			if(IsImage(file)) 
				SortImage(file);
			else if(IsMesh(file)) 
				SortMesh(file);
			else
			{
				Debug.LogError("[" + this + "] Unrecognised file type. Make sure this file follows existing conventions or move it out of the selected folder. [" + file.Name + "]");
				warnings += "Unrecognised file type. Make sure this file follows existing conventions or move it out of the selected folder. [" + file.Name + "]\n";
			}
		}

		private void SortImage(FileInfo file)
		{
			content.AddTexture(ref content, file,ref warnings);
		}

		private void SortMesh(FileInfo file)
		{
			string tag = PrefabFolderContent.GetTag(file);
			switch (tag)
			{
				case "Collision" :
					if (content.fileCollisionMesh == null)
						content.fileCollisionMesh = file;
					else
					{
						Debug.LogError("[" + this + "] Attempting to assign multiple collision meshes. Make sure there are no ambiguous files. [" + content.fileCollisionMesh.Name + " and " + file.Name + "]");
						warnings += "Attempting to assign multiple collision meshes. Make sure there are no ambiguous files. [" + content.fileCollisionMesh.Name + " and " + file.Name + "]\n";
					}
					break;
				default:
					if (content.fileMesh == null)
                    {
						content.fileMesh = file;

						//updating prefab name to the name of the given mesh
						content.prefabName = PrefabFolderContent.GetFileNameWithoutExtension(file);
					}
					else
					{
						Debug.LogError("[" + this + "] Attempting to assign multiple meshes. Make sure there are no ambiguous files. [" + content.fileMesh.Name + " and " + file.Name + "]");
						warnings += "Attempting to assign multiple meshes. Make sure there are no ambiguous files. [" + content.fileMesh.Name + " and " + file.Name + "]\n";
					}
					break;
			}

		}

		private bool IsImage(FileInfo file)
		{
			string extension = file.Extension;
			return imageExtensions.Contains(extension.ToLower());
		}

		private bool IsMesh(FileInfo file)
		{
			string extension = file.Extension;
			return meshExtensions.Contains(extension.ToLower());
		}

		//Check if this file is a file to be imported
		private bool IsToBeImported(FileInfo file)
		{
			return file.Name.Contains(filesToSelect);
		}
		#endregion
	}
}