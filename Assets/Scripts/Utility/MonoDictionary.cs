using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

/// <summary>
/// A Dictionary that can be serialized by Unity. This class works well with a special custom drawer
/// implemented in order to nicely edit the dictionnary in the inspector.
/// </summary>
[System.Serializable]
public class MonoDictionary<K,V> : IDictionary<K,V>, ISerializationCallbackReceiver
{
	[SerializeField]
	private List<K> m_Keys = new List<K>();

	[SerializeField]
	private List<V> m_Values = new List<V>();

	// use an internal dictionnary and do not directly inherit from one, because otherwise the Add() function throw an exception
	// when it is used inside the OnAfterDeserialize() function. I found this solution on the net.
	private Dictionary<K, V> m_Dictionary = new Dictionary<K, V>();

	// The dictionary keys are only used by the custom drawer to display the duplicates at the end of the list
	// However, we cannot remove this serialized field when building for the Device Target, otherwise the game will crash during serialization
	[SerializeField]
	[HideInInspector]
	private List<bool> m_IsKeyDuplicated = null;

    public MonoDictionary()
    {
    }

    public MonoDictionary(MonoDictionary<K, V> other)
    {
        m_Keys = new List<K>(other.Keys);
        m_Values = new List<V>(other.Values);
		m_Dictionary = new Dictionary<K, V>(other.m_Dictionary);
	}

	// Warning the dictionnary can be smaller than the list when the same key appears in the key list.
	// And this condition appears normally when you want to increase the list key in the Inspector.
	// So we first take all the unique keys from the dictionary (which place unique key at the top of the list)
	// then fill the rest with default values.
	// If we don't do that, it's impossible to increase the list in the inspector.
	// Actually the best way is to do nothing to not disturb the two serialized list
	public void OnBeforeSerialize()
	{
//		// get the current number of keys
//		int keyCount = (m_Keys != null) ? m_Keys.Count : this.Count;
//		// create two new list of the good size
//		List<K> newKeys = new List<K>(keyCount);
//		List<V> newValues = new List<V>(keyCount);
//		// extract all the kvp from the dictionnary and put them in list
//		foreach (var kvp in this as Dictionary<K,V>)
//		{
//			newKeys.Add(kvp.Key);
//			newValues.Add(kvp.Value);
//		}
//
//		// also duplicate the last kvp if needed to preserve the size of the list
//		if (m_Keys != null)
//		{
//			for (int i = newKeys.Count; i < m_Keys.Count; ++i)
//			{
//				newKeys.Add(default(K));
//				newValues.Add(default(V));
//			}
//		}
//
//		// set the new arrays
//		m_Keys = newKeys;
//		m_Values = newValues;
	}

	// warning the dictionary size may be smaller than the list size, because the keys are unique
	// in the dictionnary, and not necessarly in the list of keys.
	public void OnAfterDeserialize()
	{
		// clear the dictionary
		this.Clear();
		// get the number of kvp to add, and add them
		int nbKeyValue = ((m_Keys != null) && (m_Values != null)) ? Math.Min(m_Keys.Count, m_Values.Count) : 0;

		#if UNITY_EDITOR
		if (nbKeyValue > 0)
			m_IsKeyDuplicated = new List<bool>(nbKeyValue);
		#else
		// when serializing on target, just clear this list that we don't need anymore.
		if (m_IsKeyDuplicated != null)
		{
			m_IsKeyDuplicated.Clear();
			m_IsKeyDuplicated = null;
		}
		#endif

		for (int i = 0; i < nbKeyValue ; ++i)
		{
			bool isKeyDuplicated = (m_Keys[i] == null) || this.ContainsKey(m_Keys[i]);

			#if UNITY_EDITOR
			m_IsKeyDuplicated.Add(isKeyDuplicated);
			#endif
				
			if (!isKeyDuplicated)
				this.Add(m_Keys[i], m_Values[i]);
		}
	}

	public ICollection<K> Keys
	{
		get	{ return ((IDictionary<K, V>)m_Dictionary).Keys; }
	}

	public ICollection<V> Values
	{
		get	{ return ((IDictionary<K, V>)m_Dictionary).Values; }
	}

	public int Count
	{
		get { return ((IDictionary<K, V>)m_Dictionary).Count; }
	}

	public bool IsReadOnly
	{
		get	{ return ((IDictionary<K, V>)m_Dictionary).IsReadOnly; }
	}

	public V this[K key]
	{
		get { return ((IDictionary<K, V>)m_Dictionary)[key]; }
		set	{ ((IDictionary<K, V>)m_Dictionary)[key] = value; }
	}

	public void Add(K key, V value)
	{
		((IDictionary<K, V>)m_Dictionary).Add(key, value);
	}

	public bool ContainsKey(K key)
	{
		return ((IDictionary<K, V>)m_Dictionary).ContainsKey(key);
	}

	public bool Remove(K key)
	{
		return ((IDictionary<K, V>)m_Dictionary).Remove(key);
	}

	public bool TryGetValue(K key, out V value)
	{
		return ((IDictionary<K, V>)m_Dictionary).TryGetValue(key, out value);
	}

	public void Add(KeyValuePair<K, V> item)
	{
		((IDictionary<K, V>)m_Dictionary).Add(item);
	}

	public void Clear()
	{
		((IDictionary<K, V>)m_Dictionary).Clear();
	}

	public bool Contains(KeyValuePair<K, V> item)
	{
		return ((IDictionary<K, V>)m_Dictionary).Contains(item);
	}

	public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
	{
		((IDictionary<K, V>)m_Dictionary).CopyTo(array, arrayIndex);
	}

	public bool Remove(KeyValuePair<K, V> item)
	{
		return ((IDictionary<K, V>)m_Dictionary).Remove(item);
	}

	public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
	{
		return ((IDictionary<K, V>)m_Dictionary).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IDictionary<K, V>)m_Dictionary).GetEnumerator();
	}
}

// We can declare some common dictionary here
[System.Serializable]
public class StringStringDictionary : MonoDictionary<string, string> {}

[System.Serializable]
public class StringIntDictionary : MonoDictionary<string, int> {}

[System.Serializable]
public class StringParticleSystemDictionary : MonoDictionary<string, ParticleSystem> { }

[System.Serializable]
public class StringGameObjectDictionary : MonoDictionary<string, GameObject> {}