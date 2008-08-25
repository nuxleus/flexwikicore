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
using System.IO;
using System.Web;
using System.Web.UI;
using System.Drawing.Imaging;
using System.Reflection;
using System.Drawing;

namespace FlexWiki.Web
{
  public class Thumbnailer : BaseHttpHandler
  {
    #region Enumerations
    /// <summary>
    /// An internal enumeration defining the thumbnail sizes.
    /// </summary>
    internal enum ThumbnailSizeType
    {
      Small = 72,
      Medium = 144,
      Large = 288
    }
    #endregion Enumerations

    #region Fields
    // Declare and define global constants
    private const string IMG_PARAM = "img"; // image parameter
    private const string SIZE_PARAM = "size"; // size parameter
    // Partial path to embedded default images
    private const string DEFAULT_THUMBNAIL = ".im.notfound";

    // Declare and define default values to class member variables
    private string _mimeText = "image/gif";
    private ImageFormat _formatType = ImageFormat.Gif;
    private ThumbnailSizeType _sizeType = ThumbnailSizeType.Small;
    #endregion Fields

    #region Methods
    /// <summary>
    /// Determines if the img parameter is a valid image.
    /// </summary>
    /// <param name="fileName">File name from the img parameter.</param>
    /// <returns>
    ///   <c>true</c> if valid image, otherwise <c>false</c>
    /// </returns>
    private bool IsValidImage (string fileName)
    {
      string ext = Path.GetExtension(fileName).ToLower();
      bool isValid = false;
      switch (ext)
      {
        case ".jpg":
        case ".jpeg":
          isValid = true;
          this._mimeText = "image/jpeg";
          this._formatType = ImageFormat.Jpeg;
          break;
        case ".gif":
          isValid = true;
          this._mimeText = "image/gif";
          //this._formatType = ImageFormat.Gif;
          this._formatType = ImageFormat.Jpeg;
          break;
        case ".png":
          isValid = true;
          this._mimeText = "image/png";
          //this._formatType = ImageFormat.Png;
          this._formatType = ImageFormat.Jpeg;
          break;   
        case ".bmp":
          isValid = true;
          this._mimeText = "image/bmp";
          //this._formatType = ImageFormat.Png;
          this._formatType = ImageFormat.Jpeg;
          break;                  
        default:
          isValid = false;
          break;
      }
      return isValid;
    }

    /// <summary>
    /// Sets the size of the thumbnail base on the size parameter.
    /// </summary>
    /// <param name="size">The size parameter.</param>
    private void SetSize (string size)
    {
      int sizeVal;
      if (!Int32.TryParse(size.Trim(), System.Globalization.NumberStyles.Integer, null, out sizeVal))
        sizeVal = (int)ThumbnailSizeType.Small;

      try
      {
        this._sizeType = (ThumbnailSizeType)sizeVal;
      }
      catch
      {
        this._sizeType = ThumbnailSizeType.Small;
      }
    }

    /// <summary>
    /// This method generates the actual thumbnail.
    /// </summary>
    /// <param name="src"></param>
    /// <returns>Thumbnail image</returns>
    private System.Drawing.Image CreateThumbnail (System.Drawing.Image src)
    {
      int maxSize = (int)this._sizeType;

      int w = src.Width;
      int h = src.Height;

      if (w > maxSize)
      {
        h = (h * maxSize) / w;
        w = maxSize;
      }

      if (h > maxSize)
      {
        w = (w * maxSize) / h;
        h = maxSize;
      }

      // The third parameter is required and is of type delegate.  Rather then create a method that
      // does nothing, .NET 2.0 allows for anonymous delegate (similar to anonymous functions in other languages).
      return src.GetThumbnailImage(w, h, delegate() { return false; }, IntPtr.Zero);
    }

    /// <summary>
    /// Get default image.
    /// </summary>
    /// <remarks>
    /// This method is only invoked when there is a problem with the parameters.
    /// </remarks>
    /// <param name="context"></param>
    private void GetDefaultImage (HttpContext context)
    {
      Assembly a = Assembly.GetAssembly(this.GetType());
      Stream imgStream = null;
      Bitmap bmp = null;
      string file = string.Format("{0}{1}{2}", DEFAULT_THUMBNAIL, (int)this._sizeType, ".gif");

      imgStream = a.GetManifestResourceStream(a.GetName().Name + file);
      if (imgStream != null)
      {
        bmp = (Bitmap.FromStream(imgStream) as Bitmap);
        bmp.Save(context.Response.OutputStream, this._formatType);

        imgStream.Close();
        bmp.Dispose();
      }
    }
    #endregion Methods

    #region BaseHttpHandler Overrides
    /// <summary>
    /// Gets a value indicating whether this handler requires users to be authenticated.
    /// </summary>
    /// <value>
    ///    <c>true</c> if authentication is required
    ///    otherwise, <c>false</c>.
    /// </value>
    public override bool RequiresAuthentication
    {
      get { return false; }
    }

    /// <summary>
    /// Gets the MIME Type.
    /// </summary>
    public override string ContentMimeType
    {
      get { return this._mimeText; }
    }

    /// <summary>
    /// Main interface for reacting to the Thumbnailer request.
    /// </summary>
    /// <param name="context"></param>
    protected override void HandleRequest (HttpContext context)
    {
      if (string.IsNullOrEmpty(context.Request.QueryString[SIZE_PARAM]))
        this._sizeType = ThumbnailSizeType.Small;
      else
        this.SetSize(context.Request.QueryString[SIZE_PARAM]);

      if ((string.IsNullOrEmpty(context.Request.QueryString[IMG_PARAM])) ||
       (!this.IsValidImage(context.Request.QueryString[IMG_PARAM])))
      {
        this.GetDefaultImage(context);
      }
      else
      {
        string file = context.Request.QueryString[IMG_PARAM].Trim().ToLower().Replace("\\", "/");
        if (file.IndexOf("/") != 0)
          file = "/" + file;

        if (!File.Exists(context.Server.MapPath("~" + file)))
          this.GetDefaultImage(context);
        else
        {
          using (System.Drawing.Image im = System.Drawing.Image.FromFile(context.Server.MapPath("~" + file)))
          using (System.Drawing.Image tn = this.CreateThumbnail(im))
          {
            tn.Save(context.Response.OutputStream, this._formatType);
          }
        }
      }
    }   

    /// <summary>
    /// This is required by the base class; however, always returns true because if there is a problem, 
    /// a default image will be return from the handler.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public override bool ValidateParameters (HttpContext context)
    {
      return true;
    }
    #endregion BaseHttpHandler Overrides
  }
}
