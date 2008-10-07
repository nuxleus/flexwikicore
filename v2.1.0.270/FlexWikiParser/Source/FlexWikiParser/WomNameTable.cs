#region License Statement
// Copyright (c) Microsoft Corporation.  All rights reserved.
//
// The use and distribution terms for this software are covered by the 
// Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
// which can be found in the file CPL.TXT at the root of this distribution.
// By using this software in any fashion, you are agreeing to be bound by 
// the terms of this license.
//
// You must not remove this notice, or any other, from this software.
#endregion

using System;
using System.Xml;

// Note:
// We use FNV1a hashing algorithm: http://www.isthe.com/chongo/tech/comp/fnv/index.html
// This algorithm is designed to work with 2^n table sizes.
// The algorithm has been released into the public domain. 

// Code below is optimized for fast execution. 
// This is why you may find some code duplications. 
// It is done by purpose to avoid additional overhead connected with function calls.

// This name table is thread safe. Reading from the table is done without locking.
// Writing uses lock statement.

namespace FlexWiki
{
    /// <summary>
    /// Thread safe name table.
    /// </summary>
    public class WomNameTable : XmlNameTable
    {
        // Fields

        // Buckets for hash entries. Size of the array is 2*n.
        // First n elements are used as a pointers to entries.
        // Second n elements are used to represent entries.
        private Entry[] _buckets = new Entry[64];

        // Number of entries in the name table.
        private int _count;

        // Implementation of singleton pattern.
        private static WomNameTable s_instance = new WomNameTable();

        // Properties

        /// <summary>
        /// Gets glbal instance of the name table.
        /// </summary>
        public static WomNameTable Instance
        {
            get { return s_instance; }
        }

        // Methods

        /// <summary>
        /// Atomizes the specified string and adds it to the <c>WomNameTable</c>. 
        /// </summary>
        /// <param name="array">The string to add.</param>
        /// <returns>The atomized string or the existing string if it already exists in the name table.</returns>
        public override string Add(string array)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (array.Length == 0)
            {
                return String.Empty;
            }

            // Use FNV1a algorithm to calculate hash code
            int hashCode = 0;
            for (int i = 0; i < array.Length; i++)
            {
                hashCode ^= array[i];
                hashCode += (hashCode << 1) + (hashCode << 4) + (hashCode << 7) + (hashCode << 8) + (hashCode << 24);
            }

            // Store reference to the m_buckets to guarantee thread safe reading.
            Entry[] buckets = _buckets;
            int length = array.Length;
            int mask = (buckets.Length / 2) - 1;
            int index = buckets[hashCode & mask].next;
            while (index != 0)
            {
                Entry entry = buckets[index];
                if (hashCode == entry.hashCode && length == entry.length && array == entry.atom)
                {
                    return entry.atom;
                }
                index = entry.next;
            }

            // Value not found. Add new atom.
            return AddAtom(array, hashCode);
        }

        /// <summary>
        /// Atomizes the specified string and adds it to the <c>WomNameTable</c>. 
        /// </summary>
        /// <param name="array">The character array containing the string to add.</param>
        /// <param name="offset">The zero-based index into the array specifying the first character of the string.</param>
        /// <param name="length">The number of characters in the string.</param>
        /// <returns>The atomized string or the existing string if one already exists in the name table. 
        /// If length is zero, String.Empty is returned.</returns>
        public override string Add(char[] array, int offset, int length)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (length == 0)
            {
                return String.Empty;
            }

            // Use FNV1a algorithm to calculate hash code
            int hashCode = 0;
            for (int i = offset; i < offset + length; i++)
            {
                hashCode ^= array[i];
                hashCode += (hashCode << 1) + (hashCode << 4) + (hashCode << 7) + (hashCode << 8) + (hashCode << 24);
            }

            // Store reference to the m_buckets to guarantee thread safe reading.
            Entry[] buckets = _buckets;
            int mask = (buckets.Length / 2) - 1;
            int index = buckets[hashCode & mask].next;
            while (index != 0)
            {
                Entry entry = buckets[index];
                if (hashCode == entry.hashCode && length == entry.length)
                {
                    string atom = entry.atom;
                    for (int i = length; --i >= 0; )
                    {
                        if (atom[i] != array[offset + i])
                        {
                            goto TryNext;
                        }
                    }
                    return atom;
                }
            TryNext:
                index = entry.next;
            }

            // Value not found. Add new atom.
            return AddAtom(new string(array, offset, length), hashCode);
        }

        /// <summary>
        /// Gets the atomized string with the specified value. 
        /// </summary>
        /// <param name="array">The name to find.</param>
        /// <returns>The atomized string object or a null reference if the string has not already been atomized.</returns>
        public override string Get(string array)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (array.Length == 0)
            {
                return String.Empty;
            }

            // Use FNV1a algorithm to calculate hash code
            int hashCode = 0;
            for (int i = 0; i < array.Length; i++)
            {
                hashCode ^= array[i];
                hashCode += (hashCode << 1) + (hashCode << 4) + (hashCode << 7) + (hashCode << 8) + (hashCode << 24);
            }

            // Store reference to the m_buckets to guarantee thread safe reading.
            Entry[] buckets = _buckets;
            int length = array.Length;
            int mask = (buckets.Length / 2) - 1;
            int index = buckets[hashCode & mask].next;
            while (index != 0)
            {
                Entry entry = buckets[index];
                if (hashCode == entry.hashCode && length == entry.length && array == entry.atom)
                {
                    return entry.atom;
                }
                index = entry.next;
            }
            return null;
        }

        /// <summary>
        /// Gets the atomized string containing the same characters as the specified range of characters in the given array. 
        /// </summary>
        /// <param name="array">The character array containing the name to find.</param>
        /// <param name="offset">The zero-based index into the array specifying the first character of the name.</param>
        /// <param name="length">The number of characters in the name.</param>
        /// <returns>The atomized string or a null reference if the string has not already been atomized. 
        /// If length is zero, String.Empty is returned.</returns>
        public override string Get(char[] array, int offset, int length)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (length == 0)
            {
                return String.Empty;
            }

            // Use FNV1a algorithm to calculate hash code
            int hashCode = 0;
            for (int i = offset; i < offset + length; i++)
            {
                hashCode ^= array[i];
                hashCode += (hashCode << 1) + (hashCode << 4) + (hashCode << 7) + (hashCode << 8) + (hashCode << 24);
            }

            // Store reference to the m_buckets to guarantee thread safe reading.
            Entry[] buckets = _buckets;
            int mask = (buckets.Length / 2) - 1;
            int index = buckets[hashCode & mask].next;
            while (index != 0)
            {
                Entry entry = buckets[index];
                if (hashCode == entry.hashCode && length == entry.length)
                {
                    string atom = entry.atom;
                    for (int i = length; --i >= 0; )
                    {
                        if (atom[i] != array[offset + i])
                        {
                            goto TryNext;
                        }
                    }
                    return atom;
                }
            TryNext:
                index = entry.next;
            }
            return null;
        }

        /// <summary>
        /// Creates a new atomized string and adds it to the name table.
        /// </summary>
        /// <param name="value">New atomized string.</param>
        /// <param name="hashCode">hash code of the value string.</param>
        /// <returns>The atomized string or the existing string if one already exists in the name table.</returns>
        private string AddAtom(string value, int hashCode)
        {
            lock (this)
            {
                // Try to get the name again after we locked
                int valueLength = value.Length;
                int capacity = _buckets.Length / 2;
                int mask = capacity - 1;
                int bucket = hashCode & mask;
                int index = _buckets[bucket].next;
                while (index != 0)
                {
                    Entry entry = _buckets[index];
                    if (hashCode == entry.hashCode && valueLength == entry.length && value == entry.atom)
                    {
                        return entry.atom;
                    }
                    index = entry.next;
                }

                // Name was not found. Need to add it.
                int newIndex = capacity + _count;
                _buckets[newIndex] = new Entry(value, hashCode, _buckets[bucket].next);
                _buckets[bucket].next = newIndex;
                _count++;
                if (_count == capacity)
                {
                    GrowBuckets();
                }
                return value;
            }
        }

        /// <summary>
        /// Double size of the m_buckets array.
        /// </summary>
        private void GrowBuckets()
        {
            // Create a new double size array.
            int capacity = _buckets.Length / 2;
            int newCapacity = capacity * 2;
            Entry[] newBuckets = new Entry[newCapacity * 2];

            // Copy all entries from the old array to the new one.
            int mask = capacity - 1;
            for (int i = 0; i < capacity; i++)
            {
                Entry entry = _buckets[capacity + i];
                int bucket = entry.hashCode & mask;
                int index = newCapacity + i;
                newBuckets[index] = entry;
                newBuckets[index].next = newBuckets[bucket].next;
                newBuckets[bucket].next = index;
            }

            // Now we will start to use the new array.
            _buckets = newBuckets;
        }

        private struct Entry
        {
            // Hash code of the atom in the entry.
            internal int hashCode;

            // Reference to atomized string.
            internal string atom;

            // Length of the atom in the entry.
            internal int length;

            // Reference to next entry used in the name table if names have the same hash code.
            internal int next;

            public Entry(string atom, int hashCode, int next)
            {
                this.atom = atom;
                this.length = atom.Length;
                this.hashCode = hashCode;
                this.next = next;
            }
        }
    }
}
