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
/// Object pool for a specific type
/// </summary>
/// <typeparam name="T">Type of the object pool and its instances</typeparam>
public class ObjectPool<T>
{
	#region Variables

	  ////////////////////////////////////////////////////////////////////
	 /////////////////////////      Variables      //////////////////////
	////////////////////////////////////////////////////////////////////

	/// <summary>
	/// List of all instances in the object pool
	/// </summary>
	private List<OPInstance<T>> instances;

	/// <summary>
	/// Dictionary for looking up the OPInstance object for a specific element
	/// </summary>
	private Dictionary<T, OPInstance<T>> instanceLookupDictionary;

	/// <summary>
	/// Func for creating a new instance for the object pool
	/// </summary>
	private Func<T> instanceCreateFunc;
	#endregion

	#region Methods

	  ////////////////////////////////////////////////////////////////////
	 /////////////////////////        Methods      //////////////////////
	////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Constructor of the object pool
	/// </summary>
	/// <param name="instanceCreateFunc">Func for creating new instances for the object pool</param>
	/// <param name="initialPoolSize">Initial object pool size</param>
	public ObjectPool(Func<T> instanceCreateFunc, int initialPoolSize)
	{
		this.instanceCreateFunc = instanceCreateFunc;

		instances = new List<OPInstance<T>>(initialPoolSize);
		instanceLookupDictionary = new Dictionary<T, OPInstance<T>>(initialPoolSize);

		InstantiatePool(initialPoolSize);
	}

	/// <summary>
	/// Method for instantiating the instances of objects for the object pool
	/// </summary>
	/// <param name="poolCapacity">Size of the object pool</param>
	private void InstantiatePool(int poolCapacity)
	{
		for (int i = 0; i < poolCapacity; i++)
			CreateOPInstance();
	}

	/// <summary>
	/// Method for creating a new object pool container instance
	/// </summary>
	/// <returns>New object pool element</returns>
	private OPInstance<T> CreateOPInstance()
	{
		var container = new OPInstance<T>();
		container.Item = instanceCreateFunc();
		instances.Add(container);
		return container;
	}

	/// <summary>
	/// Method for retrieving an instance from the object pool
	/// </summary>
	/// <returns>Instance from the object pool</returns>
	public T RetrieveInstanceFromPool()
	{
		OPInstance<T> container = null;

		foreach (var instance in instances)
		{
			if (instance.Used)
				continue;
			else
				container = instance;
		}

		if (container == null)
		{
			container = CreateOPInstance();
		}

		container.Used = true;
		instanceLookupDictionary.Add(container.Item, container);
		return container.Item;
	}

	/// <summary>
	/// Method for returning an instance to the object pool
	/// </summary>
	/// <param name="item">Item to be returned to the object pool</param>
	public void ReturnInstanceToPool(T item)
	{
		if (instanceLookupDictionary.ContainsKey(item))
		{
			var container = instanceLookupDictionary[item];
			container.Used = false;
			instanceLookupDictionary.Remove(item);
		}
	}
	#endregion
}
