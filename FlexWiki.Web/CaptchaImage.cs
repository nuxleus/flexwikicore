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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Text;

namespace FlexWiki.Web
{
    public class CaptchaImage : IDisposable
    {
        private const int c_MinDigits = 4;
        private const int c_MaxDigits = 7;

        private string _familyName;
        private int _height;
        private Bitmap _image;
        private Random _random = new Random();
        private string _text;
        private int _width;

        // ====================================================================
        // Initializes a new instance of the CaptchaImage class using the
        // specified text, width and height.
        // ====================================================================
        public CaptchaImage(string s, int width, int height)
        {
            this._text = s;
            this.SetDimensions(width, height);
            this.GenerateImage();
        }

        // ====================================================================
        // Initializes a new instance of the CaptchaImage class using the
        // specified text, width, height and font family.
        // ====================================================================
        public CaptchaImage(string s, int width, int height, string familyName)
        {
            this._text = s;
            this.SetDimensions(width, height);
            this.SetFamilyName(familyName);
            this.GenerateImage();
        }

        // ====================================================================
        // This member overrides Object.Finalize.
        // ====================================================================
        ~CaptchaImage()
        {
            Dispose(false);
        }

        public int Height
        {
            get { return this._height; }
        }
        public Bitmap Image
        {
            get { return this._image; }
        }
        public string Text
        {
            get { return this._text; }
        }
        public int Width
        {
            get { return this._width; }
        }

        // ====================================================================
        // Releases all resources used by this object.
        // ====================================================================
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.Dispose(true);
        }
        public static string GenerateRandomCode()
        {
            Random r = new Random();
            int numDigits = c_MinDigits + r.Next(c_MaxDigits - c_MinDigits);
            return GenerateRandomCode(r, numDigits);
        }

        // ====================================================================
        // Custom Dispose method to clean up unmanaged resources.
        // ====================================================================
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                // Dispose of the bitmap.
                this._image.Dispose();
        }

        // ====================================================================
        // Creates the bitmap image.
        // ====================================================================
        private void GenerateImage()
        {
            // Create a new 32-bit bitmap image.
            Bitmap bitmap = new Bitmap(this._width, this._height, PixelFormat.Format32bppArgb);

            // Create a graphics object for drawing.
            Graphics g = Graphics.FromImage(bitmap);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, this._width, this._height);

            // Fill in the background.
            HatchBrush hatchBrush = new HatchBrush(HatchStyle.SmallConfetti, Color.LightGray, Color.White);
            g.FillRectangle(hatchBrush, rect);

            // Set up the text font.
            SizeF size;
            float fontSize = rect.Height + 1;
            Font font;
            // Adjust the font size until the text fits within the image.
            do
            {
                fontSize--;
                font = new Font(this._familyName, fontSize, FontStyle.Bold);
                size = g.MeasureString(this._text, font);
            } while (size.Width > rect.Width);

            // Set up the text format.
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;

            // Create a path using the text and warp it randomly.
            GraphicsPath path = new GraphicsPath();
            path.AddString(this._text, font.FontFamily, (int)font.Style, font.Size, rect, format);
            float v = 4F;
            PointF[] points = {
                                  new PointF(this._random.Next(rect.Width) / v, this._random.Next(rect.Height) / v),
                                  new PointF(rect.Width - this._random.Next(rect.Width) / v, this._random.Next(rect.Height) / v),
                                  new PointF(this._random.Next(rect.Width) / v, rect.Height - this._random.Next(rect.Height) / v),
                                  new PointF(rect.Width - this._random.Next(rect.Width) / v, rect.Height - this._random.Next(rect.Height) / v)
                              };
            Matrix matrix = new Matrix();
            matrix.Translate(0F, 0F);
            path.Warp(points, rect, matrix, WarpMode.Perspective, 0F);

            // Draw the text.
            hatchBrush = new HatchBrush(HatchStyle.LargeConfetti, Color.LightGray, Color.DarkGray);
            g.FillPath(hatchBrush, path);

            // Add some random noise.
            int m = Math.Max(rect.Width, rect.Height);
            for (int i = 0; i < (int)(rect.Width * rect.Height / 30F); i++)
            {
                int x = this._random.Next(rect.Width);
                int y = this._random.Next(rect.Height);
                int w = this._random.Next(m / 50);
                int h = this._random.Next(m / 50);
                g.FillEllipse(hatchBrush, x, y, w, h);
            }

            // add some lines that cross the characters
            using (Pen pen = new Pen(hatchBrush, 2))
            {
                for (int i = 0; i < 5; ++i)
                {
                    int y1 = this._random.Next(rect.Height);
                    int y2 = this._random.Next(rect.Height);
                    g.DrawLine(pen, 0, y1, rect.Width, y2);
                }
            }

            // Clean up.
            font.Dispose();
            hatchBrush.Dispose();
            g.Dispose();

            // Set the image.
            this._image = bitmap;
        }
        private static string GenerateRandomCode(Random r, int numDigits)
        {
            StringBuilder b = new StringBuilder();
            for (int i = 0; i < numDigits; ++i)
            {
                b.Append(r.Next(10));
            }
            return b.ToString();
        }
        // ====================================================================
        // Sets the image width and height.
        // ====================================================================
        private void SetDimensions(int width, int height)
        {
            // Check the width and height.
            if (width <= 0)
                throw new ArgumentOutOfRangeException("width", width, "Argument out of range, must be greater than zero.");
            if (height <= 0)
                throw new ArgumentOutOfRangeException("height", height, "Argument out of range, must be greater than zero.");
            this._width = width;
            this._height = height;
        }
        // ====================================================================
        // Sets the font used for the image text.
        // ====================================================================
        private void SetFamilyName(string familyName)
        {
            // If the named font is not installed, default to a system font.
            try
            {
                new Font(this._familyName, 12F).Dispose();
                this._familyName = familyName;
            }
            catch (Exception)
            {
                this._familyName = System.Drawing.FontFamily.GenericSerif.Name;
            }
        }

    }
}
