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
using System.Collections;
using System.Collections.Generic;
using System.Text;

using FlexWiki.Formatting;
using FlexWiki.Security;

namespace FlexWiki
{
    /// <summary>
    /// 
    /// </summary>
    [ExposedClass("Array", "Holds an ordered sequence of objects.")]
    public class BELArray : BELObject
    {
        private ArrayList _array = new ArrayList();

        public BELArray()
            : base()
        {
        }

        public BELArray(IEnumerable list)
            : base()
        {
            _array = new ArrayList();
            foreach (object x in list)
                Add(x);
        }

        public ArrayList Array
        {
            get
            {
                return _array;
            }
        }

        [ExposedMethod(ExposedMethodFlags.Default, "Answer the number of objects in the array")]
        public int Count
        {
            get
            {
                return _array.Count;
            }
        }

        [ExposedMethod(ExposedMethodFlags.Default, "Answer true if this object is empty, else false")]
        public bool IsEmpty
        {
            get
            {
                return Count == 0;
            }
        }


        public void Add(object arg)
        {
            _array.Add(BELObject.ConvertToBELObjectIfNeeded(arg));
        }

        [ExposedMethod(ExposedMethodFlags.Default, "Answer this array after the supplied array has been appended on the end.")]
        public BELArray Append(ArrayList array)
        {
            foreach (object each in array)
            {
                _array.Add(BELObject.ConvertToBELObjectIfNeeded(each));
            }
            return this;
        }

        [ExposedMethod(ExposedMethodFlags.NeedContext, "Evalute the given block once for each object in the array; answer a new array containing the result of each block evaluation.")]
        public BELArray Collect(ExecutionContext ctx, Block block)
        {
            BELArray answer = new BELArray();

            foreach (IBELObject each in Array)
            {
                    ArrayList parms = new ArrayList();
                    parms.Add(each);
                    answer.Add(block.Value(ctx, parms));
            }
            return answer;
        }

        [ExposedMethod(ExposedMethodFlags.Default, "Answer the item at the given index")]
        public IBELObject Item(int index)
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException("Index (" + index + ") out of valid range: 0.." + Count.ToString());
            return (IBELObject)(Array[index]);
        }

        [ExposedMethod(ExposedMethodFlags.Default, "Answer a new array that is in reverse order of the original array")]
        public BELArray Reverse()
        {
            ArrayList answer = new ArrayList(Array);
            try
            {
                answer.Reverse();
            }
            catch (InvalidOperationException e)
            {
                throw e.InnerException;
            }
            return new BELArray(answer);
        }

        [ExposedMethod(ExposedMethodFlags.Default, "Sort the array; answer a new array (that is sorted)")]
        public BELArray ReverseSort()
        {
            ArrayList answer = new ArrayList(Array);
            try
            {
                answer.Sort();
                answer.Reverse();
            }
            catch (InvalidOperationException e)
            {
                throw e.InnerException;
            }

            return new BELArray(answer);
        }

        [ExposedMethod(ExposedMethodFlags.NeedContext, "Sort the array; evaluate the block for each object in the array to determine the object used to order that item in the sort")]
        public BELArray ReverseSortBy(ExecutionContext ctx, Block block)
        {
            // This code formerly sorted the list by having the IComparer do the comparison. 
            // This was resulting in lots of superficial comparisons, as the block was being
            // evaluated lots of extra times - for the left and the right item on every comparison.
            // Instead, I changed it to rip through the array, evaluate the block exaclty once
            // for each item, and then do the sort on just the values. 
            List<BelArrayEntry> entries = new List<BelArrayEntry>();

            try
            {
                foreach (IBELObject entry in Array)
                {
                    ArrayList a1 = new ArrayList();
                    a1.Add(entry);
                    IBELObject value = block.Value(ctx, a1);

                    IComparable comparable = value as IComparable;
                    if (comparable == null)
                    {
                        throw new ExecutionException(null, "Can not compare objects of type " + BELType.ExternalTypeNameForType(value.GetType()));
                    }

                    entries.Add(new BelArrayEntry(entry, comparable));
                }

                entries.Sort(new BelArrayEntry.Comparer());
                entries.Reverse();

            }
            catch (InvalidOperationException e)
            {
                throw e.InnerException;
            }

            BELArray results = new BELArray();
            foreach (BelArrayEntry entry in entries)
            {
                results.Add(entry.Entry);
            }
            return results;
        }
        [ExposedMethod(ExposedMethodFlags.NeedContext, "Evaluate the block for each item in the array; answer a new Array that includes on those objects for which the block evaluated to true.")]
        public BELArray Select(ExecutionContext ctx, Block block)
        {
            BELArray answer = new BELArray();

            foreach (IBELObject each in Array)
            {
                ArrayList parms = new ArrayList();

                    parms.Add(each);
                    IBELObject objValue = block.Value(ctx, parms);
                    BELBoolean test = objValue as BELBoolean;
                    if (test == null)
                        throw new ExecutionException(ctx.CurrentLocation, "Select block must evaluate to a boolean.  Got " + BELType.BELTypeForType(objValue.GetType()).ExternalTypeName + " instead.");
                    if (test.Value)
                        answer.Add(each);
            }
            return answer;
        }

        [ExposedMethod(ExposedMethodFlags.Default, "Answer a new array that is a copy of this array, but trimmed to have no more than the given number of objects.")]
        public BELArray Snip(int maxCount)
        {
            BELArray answer = new BELArray();
            foreach (IBELObject each in Array)
            {
                answer.Add(each);
                if (answer.Count >= maxCount)
                    return answer;
            }
            return answer;
        }

        [ExposedMethod(ExposedMethodFlags.Default, "Sort the array; answer a new array (that is sorted)")]
        public BELArray Sort()
        {
            ArrayList answer = new ArrayList(Array);
            try
            {
                answer.Sort();
            }
            catch (InvalidOperationException e)
            {
                throw e.InnerException;
            }

            return new BELArray(answer);
        }

        [ExposedMethod(ExposedMethodFlags.NeedContext, "Sort the array; evaluate the block for each object in the array to determine the object used to order that item in the sort")]
        public BELArray SortBy(ExecutionContext ctx, Block block)
        {
            // This code formerly sorted the list by having the IComparer do the comparison. 
            // This was resulting in lots of superficial comparisons, as the block was being
            // evaluated lots of extra times - for the left and the right item on every comparison.
            // Instead, I changed it to rip through the array, evaluate the block exaclty once
            // for each item, and then do the sort on just the values. 
            List<BelArrayEntry> entries = new List<BelArrayEntry>();

            try
            {
                foreach (IBELObject entry in Array)
                {
                    ArrayList a1 = new ArrayList();
                    a1.Add(entry);
                    IBELObject value = block.Value(ctx, a1);

                    IComparable comparable = value as IComparable;
                    if (comparable == null)
                    {
                        throw new ExecutionException(null, "Can not compare objects of type " + BELType.ExternalTypeNameForType(value.GetType()));
                    }

                    entries.Add(new BelArrayEntry(entry, comparable));
                }

                entries.Sort(new BelArrayEntry.Comparer());

            }
            catch (InvalidOperationException e)
            {
                throw e.InnerException;
            }

            BELArray results = new BELArray();
            foreach (BelArrayEntry entry in entries)
            {
                results.Add(entry.Entry); 
            }
            return results; 
        }

        [ExposedMethod("ToOneString", ExposedMethodFlags.Default, "Answer a single string that is the concatenation of all of the objects in the array converted to strings")]
        public string ToOneString
        {
            get
            {
                StringBuilder b = new StringBuilder();
                foreach (IBELObject each in _array)
                    b.Append(each.ToString());
                return b.ToString();
            }
        }

        public override IOutputSequence ToOutputSequence()
        {
            CompositeWikiSequence seq = new CompositeWikiSequence();
            foreach (IWikiSequenceProducer each in _array)
                seq.Add(each.ToOutputSequence());
            return seq;
        }

        [ExposedMethod(ExposedMethodFlags.NeedContext, "Convert this object to a Presentation")]
        public IPresentation ToPresentation(ExecutionContext ctx)
        {
            return ToOutputSequence().ToPresentation(ctx.Presenter);
        }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            foreach (object each in _array)
                b.Append(each.ToString());
            return b.ToString();
        }

        [ExposedMethod(ExposedMethodFlags.Default, "Answer the unique elements in this array")]
        public ArrayList Unique()
        {
            Hashtable hashTable = new Hashtable();
            ArrayList uniqueElements = new ArrayList();
            foreach (object element in Array)
            {
                try
                {
                    string key = element.ToString();
                    hashTable.Add(key, element);
                    uniqueElements.Add(element);
                }
                catch (ArgumentException)
                {
                    // This is expected when a non-unique element is added for the second or subsequent times.
                }
            }
            return uniqueElements;
        }

    }
}
