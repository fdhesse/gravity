using System;
using System.Collections;
using System.Collections.Generic;

namespace FullInspector {
    /// <summary>
    /// Helper functions that unify IList operations across arrays and "actual" lists.
    /// </summary>
    public static class fiListUtility {
        public static void Add<T>(ref IList list) {
            if (list.GetType().IsArray) {
                T[] arr = (T[])list;
                Array.Resize(ref arr, arr.Length + 1);
                list = arr;
            }
            else {
                list.Add(default(T));
            }
        }

        public static void InsertAt<T>(ref IList list, int index) {
            if (list.GetType().IsArray) {
                var wrappedList = new List<T>((IList<T>)list);
                wrappedList.Insert(index, default(T));
                list = wrappedList.ToArray();
            }
            else {
                list.Insert(index, default(T));
            }
        }

        public static void RemoveAt<T>(ref IList list, int index) {
            if (list.GetType().IsArray) {
                var wrappedList = new List<T>((IList<T>)list);
                wrappedList.RemoveAt(index);
                list = wrappedList.ToArray();
            }
            else {
                list.RemoveAt(index);
            }
        }
    }
}