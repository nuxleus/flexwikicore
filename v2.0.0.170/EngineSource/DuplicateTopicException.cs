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
	/// Summary description for DuplicateTopicException.
	/// </summary>
	public class DuplicateTopicException : ApplicationException
	{
		public DuplicateTopicException() : base()
		{
		}
		public DuplicateTopicException(string message) : base(message)
		{
		}

		TopicRevision _Topic;

		public TopicRevision Topic
		{
			get
			{
				return _Topic;
			}
			set
			{
				_Topic = value;
			}
		}

		public static DuplicateTopicException ForTopic(TopicRevision tn)
		{
			DuplicateTopicException answer = new DuplicateTopicException("Duplicate topic: " + tn.ToString());
			answer.Topic = tn;
			return answer;
		}
		
	}
}
