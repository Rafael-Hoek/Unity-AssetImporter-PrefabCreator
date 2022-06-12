using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text.RegularExpressions;

namespace PrefabCreator {
	public static class NewPrefabCreator 
	{
		//Asset folder locations
		private static readonly string texturesAssetFolder = "Assets/Project/Textures/";
		private static readonly string meshesAssetFolder = "Assets/Project/Meshes/";
		private static readonly string materialsAssetFolder = "Assets/Project/Materials/";
		private static readonly string prefabsFolder = "Assets/Project/Prefabs/";

        #region existing name checkers
		public static bool PrefabNameExists(string name, PrefabCreator.PrefabType type)
        {
			if (type == PrefabCreator.PrefabType.NONE)
				return false;
			string path = prefabsFolder + PrefabPaths.List[type] + name + ".prefab";
			Type assetTypeAtPath = AssetDatabase.GetMainAssetTypeAtPath(path);
			return assetTypeAtPath != null;
        }

		public static bool MaterialNameExists(string name, PrefabCreator.PrefabType type)
		{
			if (type == PrefabCreator.PrefabType.NONE)
				return false;
			string path = materialsAssetFolder + PrefabPaths.List[type] + name + ".mat";
			Type assetTypeAtPath = AssetDatabase.GetMainAssetTypeAtPath(path);
			return assetTypeAtPath != null;
		}
		#endregion

		#region Starter function
		public static void CreateNewPrefab(PrefabFolderContent content, bool isRawAssets = false, bool overwriteAssets = false)
		{
			// Check Name conflicts (use AssetDatabase)

			// Add Mesh to folder
			content.assetPathMesh = AddFileToFolderAndImport(content.fileMesh, meshesAssetFolder, content.prefabType, content.prefabName, overwriteAssets);
			content.assetPathCollisionMesh = AddFileToFolderAndImport(content.fileCollisionMesh, meshesAssetFolder, content.prefabType, content.prefabName, overwriteAssets);

			// Add textures to folder
			foreach (FolderContentMaterial mat in content.materials)
			{
				mat.assetPathAlbedo = AddFileToFolderAndImport(mat.fileAlbedo, texturesAssetFolder, content.prefabType, mat.materialName, overwriteAssets);
				mat.assetPathMetallic = AddFileToFolderAndImport(mat.fileMetallic, texturesAssetFolder, content.prefabType, mat.materialName, overwriteAssets);
				mat.assetPathNormal = AddFileToFolderAndImport(mat.fileNormal, texturesAssetFolder, content.prefabType, mat.materialName, overwriteAssets);
				mat.assetPathAO = AddFileToFolderAndImport(mat.fileAO, texturesAssetFolder, content.prefabType, mat.materialName, overwriteAssets);
				mat.assetPathMask = AddFileToFolderAndImport(mat.fileMask, texturesAssetFolder, content.prefabType, mat.materialName, overwriteAssets);

				// Create material
				mat.assetPathMaterial = GenerateMaterial(mat, content.prefabType, overwriteAssets);
			}

			// create Resources folder for part
			// Add new PartInfo to folder
			if (!isRawAssets)
			{
				content.assetPathPrefab = GeneratePrefab(content, overwriteAssets);
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
		#endregion

		#region import file
		public static string AddFileToFolderAndImport(FileInfo file, string assetFolder, PrefabCreator.PrefabType type, string prefabName, bool overwrite = false)
		{
			if (file == null)
				return "";

			string assetPath = ConstructAssetPath(file, assetFolder, type, prefabName);

			//add absolute path to current (relative) path
			string fullAssetPath = UnityDirectoriesUtil.GetProjectRoot(true) + assetPath;
			string partialAssetPath = UnityDirectoriesUtil.GetProjectRoot(true) + assetFolder;
			if (type != PrefabCreator.PrefabType.NONE)
					partialAssetPath += PrefabPaths.List[type];

			System.IO.Directory.CreateDirectory(partialAssetPath);

			CopyToPath(file, fullAssetPath, overwrite);

            AssetDatabase.ImportAsset(assetPath);
			AssetDatabase.SetLabels(AssetDatabase.LoadMainAssetAtPath(assetPath),new string[] { type.ToString() });
			return assetPath;
		}

		public static void CopyToPath(FileInfo file, string targetPath, bool overwrite)
		{
			Debug.Log("Copying " + file.Name + " to " + targetPath);

			File.Copy(file.FullName, targetPath, overwrite);
		}

		private static string ConstructAssetPath(FileInfo existingFile, string assetFolder, PrefabCreator.PrefabType assetType, string name)
        {
			string assetPath = assetFolder;

			if (assetType != PrefabCreator.PrefabType.NONE)
			{
				assetPath += PrefabPaths.List[assetType];
			}

			//construct new file name based on given prefab name, tag of given file and extension of given file
			assetPath += name;
			if (!string.IsNullOrEmpty(PrefabFolderContent.GetTag(existingFile)))
				assetPath = assetPath + "_" + PrefabFolderContent.GetTag(existingFile);
			assetPath += PrefabFolderContent.GetFileExtension(existingFile);

			return assetPath;
		}
		#endregion

		#region generate material
		public static string GenerateMaterial(FolderContentMaterial mat, PrefabCreator.PrefabType prefabType, bool overwriteExisting = false)
		{
			if(!overwriteExisting && MaterialNameExists(mat.materialName, prefabType))
            {
				Debug.LogError("Material already exists: " + mat.materialName);
				return null;
            }
			Shader shader = Shader.Find("Standard");
			Material newMaterial = new Material(shader);

			Texture texAlbedo = AssetDatabase.LoadAssetAtPath<Texture>(mat.assetPathAlbedo);
			Texture texMetallic = AssetDatabase.LoadAssetAtPath<Texture>(mat.assetPathMetallic);
			Texture texNormal = AssetDatabase.LoadAssetAtPath<Texture>(mat.assetPathNormal);
			Texture texAO = AssetDatabase.LoadAssetAtPath<Texture>(mat.assetPathAO);
			Texture texMask = AssetDatabase.LoadAssetAtPath<Texture>(mat.assetPathMask);

			newMaterial.SetTexture("_MainTex", texAlbedo);
			newMaterial.SetTexture("_MetallicGlossMap", texMetallic);
			newMaterial.SetTexture("_BumpMap", texNormal);
			newMaterial.SetTexture("_ColorMask", texMask);

			if (texAO != null)
			{
				newMaterial.SetTexture("_OcclusionMap", texAO);
			}
			else
			{
				// If there is no AO map the AO is in the green channel of the metal map
				newMaterial.SetTexture("_OcclusionMap", texMetallic);
			}

			// prevent everything from glowing if there is no mask texture
			if (texMask == null)
			{
				newMaterial.SetColor("_EmissionColor", Color.clear);
			}

			string assetPath = materialsAssetFolder + PrefabPaths.List[prefabType];
			System.IO.Directory.CreateDirectory(UnityDirectoriesUtil.GetProjectRoot(true) + assetPath);

			assetPath += mat.materialName + ".mat";

			AssetDatabase.CreateAsset(newMaterial, assetPath);
			AssetDatabase.SetLabels(AssetDatabase.LoadMainAssetAtPath(assetPath), new string[] { prefabType.ToString() });

			return assetPath;
		}
		#endregion

		#region create prefab
		public static string GeneratePrefab(PrefabFolderContent content, bool overwriteExisting = false)
		{
			if (!overwriteExisting && PrefabNameExists(content.prefabName, content.prefabType))
			{
				Debug.LogError("Material already exists: " + content.prefabName);
				return null;
			}

			//create the asset path
			string path = prefabsFolder + PrefabPaths.List[content.prefabType] + "/";

			//create a directory at the given path (this only does something if there is no directory present at the given path)
			System.IO.Directory.CreateDirectory(UnityDirectoriesUtil.GetProjectRoot(true) + path);

			//load previously created mesh as GameObject
			GameObject meshPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(content.assetPathMesh);
			GameObject prefabInstance = PrefabUtility.InstantiatePrefab(meshPrefab) as GameObject;

			// Load previously created material (as long as a material exists
			Material baseMaterial = null;
			if(content.materials.Count > 0)
				baseMaterial = AssetDatabase.LoadAssetAtPath<Material>(content.materials[0].assetPathMaterial);

			// Assign correct name to prefab
			prefabInstance.name = content.prefabName;

			//assign material to prefab instance, and on all child-meshrenderers
			MeshRenderer renderer = prefabInstance.transform.GetComponent<MeshRenderer>();
			if (renderer != null)
            {
				if (content.materials.Count == 1)
				{
					
					renderer.material = baseMaterial;
				}
			}
				

			foreach (Transform child in prefabInstance.GetComponentsInChildren<Transform>())
			{
				//if the object has a mesh renderer, add the material to it
				renderer = child.GetComponent<MeshRenderer>();
				if (renderer != null)
					//check if child is supposed to be mesh collider
					if (PrefabFolderContent.SplitTagAndName(child.name)[2].Value.Equals("MeshCollider"))
					{
						MeshCollider meshCollider = child.gameObject.AddComponent<MeshCollider>();
						meshCollider.sharedMesh = child.GetComponent<MeshFilter>().sharedMesh;
						GameObject.DestroyImmediate(child.GetComponent<MeshRenderer>());
						GameObject.DestroyImmediate(child.GetComponent<MeshFilter>());
					} else
                    {
						if (content.materials.Count == 1)
							renderer.material = baseMaterial;
					}
			}

			#region create collision mesh
			GameObject collisionMeshInstance = null;
			if (!string.IsNullOrEmpty(content.assetPathCollisionMesh))
			{
				//add collision mesh to scene
				GameObject collisionMesh = AssetDatabase.LoadAssetAtPath<GameObject>(content.assetPathCollisionMesh);
				collisionMeshInstance = PrefabUtility.InstantiatePrefab(collisionMesh) as GameObject;

				//set collision instance as a child of prefab instance
				collisionMeshInstance.transform.SetParent(prefabInstance.transform);
				PrefabUtility.UnpackPrefabInstance(collisionMeshInstance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

				//create list of gameObjects that will eventually store all colliders
				List<GameObject> colliderContainers = new List<GameObject>();

				//if the collision mesh instance has a mesh renderer, remove mesh renderer from collision mesh instance and add box collider
				MeshRenderer collisionInstanceMeshRenderer = collisionMeshInstance.GetComponent<MeshRenderer>();
				if (collisionInstanceMeshRenderer != null)
				{
					AddCollider(collisionMeshInstance.transform, colliderContainers);
				}

                //from every child of the collision mesh instance, if they have a mesh renderer remove mesh renderer from and add box collider
                Transform[] collisionMeshObjects = collisionMeshInstance.GetComponentsInChildren<Transform>();
                for (int i = collisionMeshObjects.Length - 1; i >= 0 ; i--)
				{
                    Transform child = collisionMeshObjects[i];
                    collisionInstanceMeshRenderer = child.GetComponent<MeshRenderer>();
					if (collisionInstanceMeshRenderer != null)
					{
						AddCollider(child, colliderContainers);
					}
				}

			}
			#endregion

			// Save modified prefab as variant 
			string assetPath = path + content.prefabName + ".prefab";
			PrefabUtility.UnpackPrefabInstance(prefabInstance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
			PrefabUtility.SaveAsPrefabAsset(prefabInstance, assetPath);

			// Destroy prefab instance and collision instance
			GameObject.DestroyImmediate(prefabInstance);
			prefabInstance = null;
			if (collisionMeshInstance != null)
			{
				GameObject.DestroyImmediate(collisionMeshInstance);
				collisionMeshInstance = null;
			}

			// Return path to our new prefab
			AssetDatabase.SetLabels(AssetDatabase.LoadMainAssetAtPath(assetPath), new string[] { content.prefabType.ToString() });
			return assetPath;
		}

		private static void AddCollider(Transform collisionMeshObject, List<GameObject> colliderContainers)
		{
			//get the tag from the collision mesh object, which decides the type of collider that will be generated around the object
			string tag = PrefabFolderContent.SplitTagAndName(collisionMeshObject.name)[2].Value;
			Collider collider;

			//assign a new collider component to the collision mesh object and cache the created collider
			switch (tag)
			{
				case "BoxCollider":
					collider = collisionMeshObject.gameObject.AddComponent<BoxCollider>();
					break;
				case "SphereCollider":
					collider = collisionMeshObject.gameObject.AddComponent<SphereCollider>();
					break;
				case "CapsuleCollider":
					collider = collisionMeshObject.gameObject.AddComponent<CapsuleCollider>();
					break;
				case "WheelCollider":
					collider = collisionMeshObject.gameObject.AddComponent<WheelCollider>();
					break;
				default:
					collider = collisionMeshObject.gameObject.AddComponent<BoxCollider>();
					break;
			}

			//sort the generated collider into a collision container
			//a collider container is an aggregation of different colliders. Any colliders with the same transform can be put in the same collider container so that there are less game objects cluttering the scene

			//copy the created collider component
			UnityEditorInternal.ComponentUtility.CopyComponent(collider);

			//check if the list of existing collider containers contains a collider object with the same transform as the collision object that is to be collapsed
			foreach (GameObject colliderContainer in colliderContainers)
            {
				if(colliderContainer.transform.parent.gameObject.Equals(collisionMeshObject.parent.gameObject) && colliderContainer.transform.rotation == collisionMeshObject.rotation)
                {
					//paste the new collider under the existing collider container
					UnityEditorInternal.ComponentUtility.PasteComponentAsNew(colliderContainer);
					//set generated collider to null. This is purely so that we can later check if it has been aggregated or not (collider is only set to null if it has been aggregated into an existing collider container)
					collider = null;
                }
            }
			//check if collider is null. Earlier we set the collider to null only after it has been aggregated, so now if it isn't null, we know that it hasn't been aggregated into an existing collider container
			if(collider != null)
            {
				//create a new collider container game object and set the rotation and parent equal to that of the currently created collision mesh object
				GameObject colliderObject = new GameObject();
				colliderObject.name = "Collider";
				colliderObject.transform.rotation = collisionMeshObject.rotation;
				colliderObject.transform.SetParent(collisionMeshObject.parent);

				//aggregate the collider into the collider container
				UnityEditorInternal.ComponentUtility.PasteComponentAsNew(colliderObject);

				//add the collider container to the list of collider containers
				colliderContainers.Add(colliderObject);
			}

			//destroy the collision mesh object after it has served its purpose (allowing us to generate a collider around it, and then aggregating that collider into a container)
			GameObject.DestroyImmediate(collisionMeshObject.gameObject);
		}
		#endregion

		#region delete collision mesh
		public static void DeleteCollisionMesh(PrefabFolderContent content)
        {
			AssetDatabase.DeleteAsset(content.assetPathCollisionMesh);
			content.assetPathCollisionMesh = null;
        }
		#endregion
	}
}
