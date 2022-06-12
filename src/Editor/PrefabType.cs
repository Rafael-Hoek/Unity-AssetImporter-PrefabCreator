using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PrefabCreator {
	public enum PrefabType
	{
		NONE = 0,
		CUSTOM = 1,
		Structure = 2,
		Prop = 3,
		Test = 4,
	}

	public static class PrefabPaths
	{
		//specific PrefabType-locations
		public static readonly Dictionary<PrefabCreator.PrefabType, string> List = new Dictionary<PrefabType, string>()
		{
			{PrefabCreator.PrefabType.NONE,  ""},
			{PrefabCreator.PrefabType.CUSTOM,  ""},
			{PrefabCreator.PrefabType.Structure,  "Environments/Structures/"},
			{PrefabCreator.PrefabType.Prop,       "Environments/Props/"},
			{PrefabCreator.PrefabType.Test,       "Environments/Props/TestFolder/"},
		};
	}
}

