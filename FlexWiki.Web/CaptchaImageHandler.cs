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
using System.Configuration;
using System.Drawing.Imaging; 
using System.Web;

namespace FlexWiki.Web
{
    public class CaptchaImageHandler : IHttpHandler
    {
        public bool IsReusable 
        { 
            get { return true; } 
        }


        private string CaptchaKey
        {
            get 
            {
                FlexWikiWebApplication application = FlexWikiWebApplication; 

                if (application == null)
                {
                    return null; 
                }

                return application.ApplicationConfiguration.CaptchaKey; 
            }
        }

        private FlexWikiWebApplication FlexWikiWebApplication
        {
            get
            {
                Federation federation = HttpContext.Current.Application[Constants.FederationCacheKey] as Federation;

                if (federation == null)
                {
                    return null;
                }

                return federation.Application as FlexWikiWebApplication;
            }
        }

        public void ProcessRequest(HttpContext ctx) 
        {
            string encryptedCode = ctx.Request.PathInfo;

            if (string.IsNullOrEmpty(encryptedCode))
            {
                FlexWikiWebApplication.LogWarning(this.GetType().FullName, "Missing path after CaptchaImage.ashx"); 
                throw new ApplicationException(
                    "Missing the path after CaptchaImage.ashx. CAPTCHA URLs are expected to be in the format CaptchaImage.ashx/0123456789ABCDEF0123456789ABCDEF"
                );
            }

            if (!encryptedCode.StartsWith("/"))
            {
                FlexWikiWebApplication.LogWarning(this.GetType().FullName, 
                    "Path after CaptchaImage.ashx does not start with a slash: " + encryptedCode); 
            }

            encryptedCode = encryptedCode.Substring(1); 

            // decrypt the code from the path to figure out what image to display
            if (CaptchaKey == null)
            {
                throw new ApplicationException("CaptchaKey is not properly configured in FlexWiki configuration file.");
            }

            string code;
            try 
            {
                code = Security.Decrypt(encryptedCode, CaptchaKey);
            }
            catch (Exception x) 
            {
                throw new ApplicationException("Failed to decrypt captcha code", x);
            }

            using (CaptchaImage ci = new CaptchaImage(code, 150, 37, "Lucida Console")) 
            {
                ctx.Response.ContentType = "image/jpeg";
                ci.Image.Save(ctx.Response.OutputStream, ImageFormat.Jpeg);
            }
        }
    }

}