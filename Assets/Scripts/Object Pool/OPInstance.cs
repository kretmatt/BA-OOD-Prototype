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
/// A container class for an instance inside an object pool
/// </summary>
/// <typeparam name="T">Type of the item / object pool</typeparam>
public class OPInstance<T>
{
	/// <summary>
	/// Flag that determines whether the instance is currently in use
	/// </summary>
	public bool Used { get; set; }

	/// <summary>
	/// Actual instance inside the object pool
	/// </summary>
	public T Item { get; set; }
}