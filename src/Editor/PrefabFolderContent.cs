	using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace PrefabCreator
{
    public class PrefabFolderContent
	{
		public const string none = "None";
        private static Regex extensionCatcher = new Regex(@"(.+?)(\.[^\.]+$|$)");

		public DirectoryInfo directory = null;

		public string prefabName = none;
		public PrefabCreator.PrefabType prefabType = PrefabCreator.PrefabType.NONE;

		public string customPrefabTag = "";
		public string customPrefabPath = "";

		//List of textures and their resulting material
		public List<FolderContentMaterial> materials = new List<FolderContentMaterial>();

		// In windows
		public FileInfo fileMesh = null;
		public FileInfo fileCollisionMesh = null;

		// In Unity
		public string assetPathMesh = null;
		public string assetPathCollisionMesh = null;
		public string assetPathPrefab = null;

		public void AddTexture(ref PrefabFolderContent content, FileInfo file, ref string warnings)
        {
			string tag = PrefabFolderContent.GetTag(file);
			string name = "";
			if (!string.IsNullOrEmpty(tag))
				name = PrefabFolderContent.SplitTagAndName(file.Name)[1].Value;
			else
				name = PrefabFolderContent.GetFileNameWithoutExtension(file);
			FolderContentMaterial mat = content.GetMaterial(name);

			switch (tag)
			{
				case "M":
					mat.fileMetallic = file;
					break;
				case "N":
					mat.fileNormal = file;
					break;
				case "AO":
					mat.fileAO = file;
					break;
				case "Mask":
					mat.fileMask = file;
					break;
				case "":
					if (mat.fileAlbedo == null)
						mat.fileAlbedo = file;
					else
					{
						Debug.LogError("[" + this + "] Attempting to assign multiple Albedo textures. Make sure there are no ambiguous files. [" + mat.fileAlbedo.Name + " and " + file.Name + "]");
						warnings += "Attempting to assign multiple Albedo textures. Make sure there are no ambiguous files. [" + mat.fileAlbedo.Name + " and " + file.Name + "]\n";
					}
					break;
				default:
					Debug.LogWarning("[" + this + "] Found texture with unknown tag [" + file.Name + "]");
					warnings += "Found texture with unknown tag[" + file.Name + "]\n";
					break;
			}
		}

		public FolderContentMaterial GetMaterial(string name)
        {
			foreach(FolderContentMaterial mat in materials)
            {
				if(mat.materialName.Equals(name))
                {
					return mat;
                }
            }
			FolderContentMaterial newMat = new FolderContentMaterial(name);
			materials.Add(newMat);
			return newMat;
        }

		public static string GetFileNameWithoutExtension(FileInfo file)
        {
			return extensionCatcher.Match(file.Name).Groups[1].Value;
        }

		public static string GetFileExtension(FileInfo file)
		{
			return extensionCatcher.Match(file.Name).Groups[2].Value;
		}
		public static string GetTag(FileInfo file)
		{
			if (file == null)
				return null;
			Regex regex = new Regex("_([^_]*)[.]");
			Match match = regex.Match(file.Name);
			string result = match.Groups[1].ToString();
			return result;
		}
		public static GroupCollection SplitTagAndName(string title)
		{
			return Regex.Match(title, "(.*)_([a-zA-Z]*)").Groups;
		}
	}

	public class FolderContentMaterial
    {
		public string materialName = "";

		public FileInfo fileAlbedo = null;
		public FileInfo fileMetallic = null;
		public FileInfo fileNormal = null;
		public FileInfo fileAO = null;
		public FileInfo fileMask = null;

		public string assetPathAlbedo = null;
		public string assetPathMetallic = null;
		public string assetPathNormal = null;
		public string assetPathAO = null;
		public string assetPathMask = null;

		public string assetPathMaterial = null;

		public FolderContentMaterial(string name)
        {
			materialName = name;
        }
	}
}
