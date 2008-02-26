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

namespace FlexWiki
{
	/// <summary>
	/// Summary description for BELTimeSpan.
	/// </summary>
	[ExposedClass("TimeSpan", "A period of time (as opposed to a point in time)")]
	public class BELTimeSpan :BELObject, IComparable
	{
		public BELTimeSpan(TimeSpan span)
		{
			_TimeSpan = span;
		}

		TimeSpan _TimeSpan;

		public TimeSpan TimeSpan
		{
			get
			{
				return _TimeSpan;
			}
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Determine whether this object is equal to another object")]
		public override bool Equals(object obj)
		{
			if (!(obj is TimeSpan))
				return false;
			return this.TimeSpan.Equals(obj);
		}

		public override int GetHashCode()
		{
			return this.TimeSpan.GetHashCode ();
		}


		public override string ToString()
		{
			return TimeSpan.ToString();
		}

		public override IOutputSequence ToOutputSequence()
		{
			return new WikiSequence(ToString());
		}

		#region Instance methods for TimeSpan construction.
		[ExposedMethod(ExposedMethodFlags.NeedContext, "Answer an instance of TimeSpan")]
		public static TimeSpan Instance(ExecutionContext ctx, [ExposedParameter(true)] int days, 
			[ExposedParameter(true)] int hours, [ExposedParameter(true)] int minutes, 
			[ExposedParameter(true)] int seconds, [ExposedParameter(true)]int milliseconds)
		{
			if (false == ctx.TopFrame.WasParameterSupplied(1))
				days = 0;
			if (false == ctx.TopFrame.WasParameterSupplied(2))
				hours = 0;
			if (false == ctx.TopFrame.WasParameterSupplied(3))
				minutes = 0;
			if (false == ctx.TopFrame.WasParameterSupplied(4))
				seconds = 0;
			if (false == ctx.TopFrame.WasParameterSupplied(5))
				milliseconds = 0;

			return Instance2(days, hours, minutes, seconds, milliseconds);
		}

		public static TimeSpan Instance2(int days, int hours, int minutes, int seconds, int milliseconds)
		{
			return new TimeSpan(days, hours, minutes, seconds, milliseconds);
		}
		#endregion

		[ExposedMethod(ExposedMethodFlags.Default, "Answer the number of days represented by this TimeSpan")]
		public int Days
		{
			get
			{
				return TimeSpan.Days;
			}
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer the number of hours represented by this TimeSpan")]
		public int Hours
		{
			get
			{
				return TimeSpan.Hours;
			}
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer the number of milliseconds represented by this TimeSpan")]
		public int Milliseconds
		{
			get
			{
				return TimeSpan.Milliseconds;
			}
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer the number of minutes represented by this TimeSpan")]
		public int Minutes
		{
			get
			{
				return TimeSpan.Minutes;
			}
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer the number of seconds represented by this TimeSpan")]
		public int Seconds
		{
			get
			{
				return TimeSpan.Seconds;
			}
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Add the specified TimeSpan to this TimeSpan (and answer a new one)")]
		public TimeSpan Add(TimeSpan span)
		{
			return TimeSpan.Add(span);
		}
		
		[ExposedMethod(ExposedMethodFlags.Default, "Subtract the specified TimeSpan to this TimeSpan (and answer a new one)")]
		public TimeSpan Subtract(TimeSpan span)
		{
			return TimeSpan.Subtract(span);
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Compare this TimeSpan with another TimeSpan and answer -1, 0 or +1 depending on whether is is less than, equal to or greater than the other TimeSpan")]
		public int CompareTo(object obj)
		{
			if (obj is BELTimeSpan)
				return TimeSpan.CompareTo(((BELTimeSpan)obj).TimeSpan);
			if (obj is TimeSpan)
				return TimeSpan.CompareTo(obj);
			throw new ArgumentException("When using CompareTo() to compare time spans, the argument must be a TimeSpan; got " + BELType.ExternalTypeNameForType(obj.GetType()));
		}

	}
}
