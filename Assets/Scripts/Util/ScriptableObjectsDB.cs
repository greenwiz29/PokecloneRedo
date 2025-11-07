using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScriptableObjectsDB<T> where T : ScriptableObject
{
	static Dictionary<string, T> objects;

	public static void Init()
	{
		objects = new Dictionary<string, T>();

		var baseArr = Resources.LoadAll<T>("").ToList();
		foreach (var obj in baseArr)
		{
			if (objects.ContainsKey(obj.name))
			{
				Debug.LogError($"There are multiple pokemon with the name {obj.name}");
				continue;
			}
			objects.Add(obj.name, obj);
		}
	}

	public static T GetObjectByName(string name)
	{
		if (!objects.ContainsKey(name))
		{
			Debug.LogError($"Object with name {name} not found in the database");
			return null;
		}
		return objects[name];
	}

	// Source - https://stackoverflow.com/a
	// Posted by StriplingWarrior, modified by community. See post 'Timeline' for change history
	// Retrieved 2025-11-07, License - CC BY-SA 2.5

	public static IEnumerable<TValue> RandomValues<TKey, TValue>(IDictionary<TKey, TValue> dict)
	{
		List<TValue> values = Enumerable.ToList(dict.Values);
		int size = dict.Count;
		while (true)
		{
			yield return values[Random.Range(0, size)];
		}
	}

	public static T GetRandomObject()
	{
		return RandomValues(objects).Take(1).FirstOrDefault();
	}
}
