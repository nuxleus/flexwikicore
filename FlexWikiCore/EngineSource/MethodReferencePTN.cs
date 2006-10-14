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
using System.Text;
using System.Collections;

namespace FlexWiki
{
	/// <summary>
	/// 
	/// </summary>
	public class MethodReferencePTN : ExposableParseTreeNode
	{
        private string _MethodName;
        private ArrayList _Args;
        private BlockArgumentsPTN _BlockArguments; 

		public MethodReferencePTN(BELLocation loc, string methodName, ArrayList args, BlockArgumentsPTN blockargs) : base(loc)
		{
			_MethodName = methodName;
			_Args = args;
			_BlockArguments = blockargs;
		}


		private string MethodName
		{
			get
			{
				return _MethodName;
			}
			set
			{
				_MethodName = MethodName;
			}
		}

		private string FullMethodName
		{
			get
			{
				StringBuilder b = new StringBuilder();
				b.Append(MethodName);
				if (_BlockArguments != null && _BlockArguments.QualifiedBlocks != null)
				{
					foreach (QualifiedBlockPTN qb in _BlockArguments.QualifiedBlocks.QualifiedBlocks)
						b.Append(qb.Identifier);
				}
				return b.ToString();
			}
		}

		private IList AllArgs
		{
			get
			{
				ArrayList answer = new ArrayList();
				if (_Args != null)
					answer.AddRange(_Args);
				if (_BlockArguments != null)
				{
					answer.Add(_BlockArguments.Block);
					if (_BlockArguments.QualifiedBlocks != null)
						foreach (QualifiedBlockPTN qb in _BlockArguments.QualifiedBlocks.QualifiedBlocks)
							answer.Add(qb.Block);
				}
				return answer;
			}
		}

		public override System.Collections.IEnumerable Children
		{
			get
			{
				ArrayList answer = new ArrayList();
				if (IsFunction && _Args != null)
					answer.AddRange(_Args);
				if (_BlockArguments != null)
					answer.Add(_BlockArguments);
				return answer;
			}
		}

		public IBELObject  EvaluateWithReceiver(ExecutionContext ctx, IBELObject receiver)
		{
			ArrayList args = new ArrayList();
			foreach (object x in AllArgs)
				args.Add(x);
			IBELObject answer = receiver.ValueOf(FullMethodName, args, ctx);
			if (answer == null)
				throw NoSuchMemberException.ForMemberAndType(Location, FullMethodName, BELType.ExternalTypeNameForType(receiver.GetType()));
			return answer;
		}

		public override IBELObject Expose(ExecutionContext ctx)
		{
			IBELObject answer = null;
			ArrayList args = new ArrayList();
			foreach (object x in AllArgs)
				args.Add(x);
			try
			{
				ctx.PushLocation(Location);
				answer = ctx.FindAndInvoke(FullMethodName, args);
				if (answer == null)
					throw NoSuchMemberException.ForMember(Location, FullMethodName);
			}
			finally
			{
				ctx.PopLocation();
			}

			return answer;
		}

		public override string ToString()
		{
			return (IsFunction ? "Function" : "Property") + " reference: '" + _MethodName + "'";
		}

		public bool IsFunction
		{
			get
			{
				return AllArgs.Count > 0;
			}
		}


	}
}
