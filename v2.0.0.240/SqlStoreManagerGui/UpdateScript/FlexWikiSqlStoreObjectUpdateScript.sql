/*License Statement
 Copyright (c) Microsoft Corporation.  All rights reserved.

The use and distribution terms for this software are covered by the 
Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
which can be found in the file CPL.TXT at the root of this distribution.
By using this software in any fashion, you are agreeing to be bound by 
the terms of this license.

You must not remove this notice, or any other, from this software.
*/

/*        ******************************************************
If you are 
      1. logged on as the same user that was used to create the FlexWikiSqlStore
      2.  and the Sql Server is on the local computer, 
      3. in the default instance, 
you can double-click this file and it will correctly 
update your FlexWikiSqlStore with the new Stored Procedures.

If not all of the above 3 conditions are met you must open the sql script in your
implementations Sql Editor (SQL 2000 Query Analyzer or SQL 2005 Management Studio)
and connect to the correct instance on the server containing the FlexWikiSqlStore.
Then all that is required is to execute the update to input the new Stored Procedures.

Do not re-run the database creation tool if you have an existing Sql Store with topics
that you do not want to lose.
*********************************************************************  */

use FlexWikiSqlStore
GO
/****** Object:  Stored Procedure dbo.WriteTopicLock    Script Date: 2/13/2008 10:51:25 AM ******/
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[WriteTopicLock]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[WriteTopicLock]
GO

/****** Object:  Stored Procedure dbo.WriteTopicUnlock    Script Date: 2/13/2008 10:51:25 AM ******/
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[WriteTopicUnlock]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[WriteTopicUnlock]
GO


SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

/****** Object:  Stored Procedure dbo.WriteTopicLock    Script Date: 2/13/2008 10:54:25 AM ******/

CREATE     PROCEDURE Dbo.WriteTopicLock
(
	@namespace NVARCHAR(255),
	@topicName NVARCHAR(255)
)
AS
	SET NOCOUNT ON

	DECLARE @topicId INTEGER
	DECLARE @namespaceId INTEGER

	SET @topicId = null
	SET @namespaceId = null

	SELECT @namespaceId = namespaceTable.NamespaceId
	FROM dbo.Namespace namespaceTable 
	WHERE namespaceTable.[Name] = @namespace

	SELECT @topicId = topicTable.TopicId
	FROM dbo.Namespace namespaceTable JOIN 
			dbo.Topic topicTable ON namespaceTable.NamespaceId = topicTable.NamespaceId
	WHERE namespaceTable.[Name] = @namespace
			AND topicTable.[Name] = @topicName


	IF @topicId IS NULL 
	BEGIN
		RAISERROR('Specified namespace: %s does not contain the topic: %s.', 16, 1, @namespace, @topicName)
	END
	
	UPDATE dbo.Topic
	SET Writable = 'FALSE'
	WHERE TopicId = @topicId

GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

/****** Object:  Stored Procedure dbo.WriteTopicUnlock    Script Date: 2/13/2008 10:54:25 AM ******/

CREATE     PROCEDURE Dbo.WriteTopicUnlock
(
	@namespace NVARCHAR(255),
	@topicName NVARCHAR(255)
)
AS
	SET NOCOUNT ON

	DECLARE @topicId INTEGER
	DECLARE @namespaceId INTEGER

	SET @topicId = null
	SET @namespaceId = null

	SELECT @namespaceId = namespaceTable.NamespaceId
	FROM dbo.Namespace namespaceTable 
	WHERE namespaceTable.[Name] = @namespace

	SELECT @topicId = topicTable.TopicId
	FROM dbo.Namespace namespaceTable JOIN 
			dbo.Topic topicTable ON namespaceTable.NamespaceId = topicTable.NamespaceId
	WHERE namespaceTable.[Name] = @namespace
			AND topicTable.[Name] = @topicName


	IF @topicId IS NULL 
	BEGIN
		RAISERROR('Specified namespace: %s does not contain the topic: %s.', 16, 1, @namespace, @topicName)
	END
	
	UPDATE dbo.Topic
	SET Writable = 'TRUE'
	WHERE TopicId = @topicId

GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO