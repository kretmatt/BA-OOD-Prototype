using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/***********************************************
 * Inspired by and adapted from:
 * Title: Unity Object Pool
 * Author: P., Cardwell-Gardner
 * Date: June 3, 2014 - Jul 8, 2021
 * Availability: https://github.com/thefuntastic/unity-object-pool

	Copyright 2014 Peter Cardwell-Gardner

	Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
	to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
	and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
	OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
	WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
	CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

 * ********************************************/


/// <summary>
/// Class for managing several object pools of elements
/// </summary>
public class PoolManager:MonoBehaviour
{
	#region Variables + Accessors

	  ////////////////////////////////////////////////////////////////////
	 /////////////////////////      Variables      //////////////////////
	////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Instance of the PoolManager class
	/// </summary>
	private static PoolManager instance;

	/// <summary>
	/// Dictionary for looking up an Object Pool depending on the prefab (Mostly for creating new instances of a prefab)
	/// </summary>
	public static Dictionary<GameObject, ObjectPool<GameObject>> prefabLookup = new Dictionary<GameObject, ObjectPool<GameObject>>();

	/// <summary>
	/// Dictionary for looking up an Object Pool depending on the instance (Mostly for removing instances)
	/// </summary>
	public static Dictionary<GameObject, ObjectPool<GameObject>> instanceLookup = new Dictionary<GameObject, ObjectPool<GameObject>>();

	/// <summary>
	/// Accessor method for retrieving the only instance of the PoolManager class inside a scene. Sets it as DontDestroyOnLoad so it doesn't get destroyed when switching scenes
	/// </summary>
	public static PoolManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = FindObjectOfType(typeof(PoolManager)) as PoolManager;

                if (!instance)
                {
					Debug.LogError("There is no PoolManager instance in the entire scene!");
				}
                else
                {
					DontDestroyOnLoad(instance);
                }
			}
			return instance;
		}
		set
		{
			instance = value;
		}
	}

	#endregion

	#region Methods

	////////////////////////////////////////////////////////////////////
	/////////////////////////        Methods      //////////////////////
	////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Method for preparing an object pool for a specific prefab
	/// </summary>
	/// <param name="prefab">Prefab for which a new object pool is created</param>
	/// <param name="size">Amount of instances to be created</param>
	/// <param name="position">Position of the pre-instantiated instances</param>
	public static void PreparePool(GameObject prefab, int size, Vector3 position)
	{
		if (prefabLookup.ContainsKey(prefab))
			return;
		
		var pool = new ObjectPool<GameObject>(() => {
			var go = Instantiate(prefab, position, Quaternion.identity) as GameObject;
			go.SetActive(false);
			return go;
		}, size);
	
		prefabLookup[prefab] = pool;
	}

	/// <summary>
	/// Method for spawning a new instance with an object pool
	/// </summary>
	/// <param name="prefab">Prefab to be instantiated</param>
	/// <param name="position">Position of the new instance</param>
	/// <param name="rotation">Rotation of the new instance</param>
	/// <returns>Instance from an object pool</returns>
	public static GameObject SpawnObject(GameObject prefab, Vector3 position, Quaternion rotation)
	{
		if (!prefabLookup.ContainsKey(prefab))
			PreparePool(prefab, 1, Vector3.zero);

		var opInstance = prefabLookup[prefab].RetrieveInstanceFromPool();
		opInstance.transform.SetPositionAndRotation(position, rotation);
		opInstance.SetActive(true);

		instanceLookup.Add(opInstance, prefabLookup[prefab]);
		
		return opInstance;
	}

	/// <summary>
	/// Method for releasing an instance and returning it back to its object pool
	/// </summary>
	/// <param name="opInstance">Instance to be removed and returned back to its object pool</param>
	public static void ReleaseObject(GameObject opInstance)
	{
		opInstance.SetActive(false);

		if (instanceLookup.ContainsKey(opInstance))
		{
			instanceLookup[opInstance].ReturnInstanceToPool(opInstance);
			instanceLookup.Remove(opInstance);
		}
	}

	#endregion
}
