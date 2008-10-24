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
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Net;

namespace FlexWiki.Web
{
  public abstract class BaseHttpHandler : IHttpHandler
  {
    #region Fields
    #endregion Fields

    #region Properties
    /// <summary>
    /// Gets a value indicating whether this handler requires users to be authenticated.
    /// </summary>
    /// <value>
    ///    <c>true</c> if authentication is required
    ///    otherwise, <c>false</c>.
    /// </value>
    public abstract bool RequiresAuthentication { get; }

    /// <summary>
    /// Gets the MIME Type.
    /// </summary>
    public abstract string ContentMimeType { get; }
    public bool IsReusable
    {
      get { return true; }
    }
    #endregion Properties

    #region Methods
    /// <summary>
    /// Processs the incoming HTTP request.
    /// </summary>
    /// <param name="context">Context.</param>
    public void ProcessRequest (HttpContext context)
    {
      this.SetResponseCachePolicy(context.Response.Cache);

      if (!this.ValidateParameters(context))
      {
        this.RespondInternalError(context);
        return;
      }

      if (this.RequiresAuthentication && !context.User.Identity.IsAuthenticated)
      {
        this.RespondForbidden(context);
        return;
      }

      context.Response.ContentType = this.ContentMimeType;
      this.HandleRequest(context);
    }

    /// <summary>
    /// Handles the request.  This is where you put your business logic.
    /// </summary>
    /// <remarks>
    /// <p>This method should result in a call to one (or more) of the following methods:</p>
    /// <p><code>context.Response.BinaryWrite();</code></p>
    /// <p><code>context.Response.Write();</code></p>
    /// <p><code>context.Response.WriteFile();</code></p>
    /// <p>
    /// <code>
    /// someStream.Save(context.Response.OutputStream);
    /// </code>
    /// </p>
    /// <p>etc...</p>
    /// <p>
    /// If you want a download box to show up with a pre-populated filename, add this call here 
    /// (supplying a real filename).
    /// </p>
    /// <p>
    /// </p>
    /// <code>Response.AddHeader("Content-Disposition", "attachment; filename=\"" + Filename + "\"");</code>
    /// </p>
    /// </remarks>
    /// <param name="context">Context.</param>
    protected abstract void HandleRequest (HttpContext context);

    /// <summary>
    /// Validates the parameters.  Inheriting classes must implement this and return 
    /// true if the parameters are valid, otherwise false.
    /// </summary>
    /// <param name="context">Context.</param>
    /// <returns><c>true</c> if the parameters are valid; otherwise, <c>false</c></returns>
    public abstract bool ValidateParameters (HttpContext context);

    /// <summary>
    /// Sets the cache policy.  Unless a handler overrides
    /// this method, handlers will not allow a response to be cached.
    /// </summary>
    /// <param name="cache">Cache.</param>
    public void SetResponseCachePolicy (HttpCachePolicy cache)
    {
      cache.SetCacheability(HttpCacheability.NoCache);
      cache.SetNoStore();
      cache.SetExpires(DateTime.MinValue);
    }

    /// <summary>
    /// Helper method used to Respond to the request that an error occurred in 
    /// processing the request.
    /// </summary>
    /// <param name="context">Context.</param>
    protected void RespondInternalError (HttpContext context)
    {
      context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
      context.Response.End();
    }

    /// <summary>
    /// Helper method used to Respond to the request that the request in attempting 
    /// to access a resource that the user does not have access to.
    /// </summary>
    /// <param name="context">Context.</param>
    protected void RespondForbidden (HttpContext context)
    {
      context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
      context.Response.End();
    }

    /// <summary>
    /// Helper method used to Respond to the request that the file was not found.
    /// </summary>
    /// <param name="context">Context.</param>
    protected void RespondFileNotFound (HttpContext context)
    {
      context.Response.StatusCode = (int)HttpStatusCode.NotFound;
      context.Response.End();
    }
    #endregion Methods

    #region Constructors
    public BaseHttpHandler () { }
    #endregion Constructors
  }
}
