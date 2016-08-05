﻿using PdfSharp.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Maps.Rendering
{
    internal interface MGraphics : IDisposable
    {
        XSmoothingMode SmoothingMode { get; set; }
        Graphics Graphics { get; }
        bool SupportsWingdings { get; }

        void ScaleTransform(double scaleXY);
        void ScaleTransform(double scaleX, double scaleY);
        void TranslateTransform(double dx, double dy);
        void RotateTransform(double angle);
        void MultiplyTransform(XMatrix m);

        void IntersectClip(XGraphicsPath path);
        void IntersectClip(RectangleF rect);

        void DrawLine(XPen pen, double x1, double y1, double x2, double y2);
        void DrawLine(XPen pen, PointF pt1, PointF pt2);
        void DrawLines(XPen pen, XPoint[] points);
        void DrawPath(XPen pen, XGraphicsPath path);
        void DrawPath(XSolidBrush brush, XGraphicsPath path);
        void DrawCurve(XPen pen, PointF[] points, double tension = 0.5);
        void DrawClosedCurve(XPen pen, PointF[] points, double tension = 0.5);
        void DrawClosedCurve(XSolidBrush brush, PointF[] points, double tension = 0.5);
        void DrawRectangle(XPen pen, double x, double y, double width, double height);
        void DrawRectangle(XSolidBrush brush, double x, double y, double width, double height);
        void DrawRectangle(XSolidBrush brush, RectangleF rect);
        void DrawEllipse(XPen pen, double x, double y, double width, double height);
        void DrawEllipse(XSolidBrush brush, double x, double y, double width, double height);
        void DrawEllipse(XPen pen, XSolidBrush brush, double x, double y, double width, double height);
        void DrawArc(XPen pen, double x, double y, double width, double height, double startAngle, double sweepAngle);
        void DrawImage(MImage image, double x, double y, double width, double height);
        void DrawImageAlpha(float alpha, MImage image, Rectangle targetRect);

        XSize MeasureString(string text, XFont font);
        void DrawString(string s, XFont font, XSolidBrush brush, double x, double y, XStringFormat format);

        MGraphicsState Save();
        void Restore(MGraphicsState state);
    }
    internal interface MGraphicsState { }
    internal class MImage
    {
        private string path;
        private string url;
        private Image image;
        private XImage ximage;

        public string Url { get { return url; } }
        public XImage XImage
        {
            get
            {
                lock (this)
                {
                    if (ximage == null)
                        ximage = XImage.FromGdiPlusImage(Image);
                    return ximage;
                }
            }
        }
        public Image Image
        {
            get
            {
                lock (this)
                {
                    if (image == null)
                        image = Image.FromFile(path);
                    return image;
                }
            }
        }

        public MImage(string path, string url)
        {
            this.path = path;
            this.url = url;
        }
    }

    internal class MXGraphics : MGraphics
    {
        private XGraphics g;
        public MXGraphics(XGraphics g) { this.g = g; }
        public bool SupportsWingdings { get { return true; } }

        public XSmoothingMode SmoothingMode { get { return g.SmoothingMode; } set { g.SmoothingMode = value; } }
        public Graphics Graphics { get { return g.Graphics; } }

        public void ScaleTransform(double scaleXY) { g.ScaleTransform(scaleXY); }
        public void ScaleTransform(double scaleX, double scaleY) { g.ScaleTransform(scaleX, scaleY); }
        public void TranslateTransform(double dx, double dy) { g.TranslateTransform(dx, dy); }
        public void RotateTransform(double angle) { g.RotateTransform(angle); }
        public void MultiplyTransform(XMatrix m) { g.MultiplyTransform(m); }

        public void IntersectClip(XGraphicsPath path) { g.IntersectClip(path); }
        public void IntersectClip(RectangleF rect) { g.IntersectClip(rect); }

        public void DrawLine(XPen pen, double x1, double y1, double x2, double y2) { g.DrawLine(pen, x1, y1, x2, y2); }
        public void DrawLine(XPen pen, PointF pt1, PointF pt2) { g.DrawLine(pen, pt1, pt2); }
        public void DrawLines(XPen pen, XPoint[] points) { g.DrawLines(pen, points); }
        public void DrawPath(XPen pen, XGraphicsPath path) { g.DrawPath(pen, path); }
        public void DrawPath(XSolidBrush brush, XGraphicsPath path) { g.DrawPath(brush, path); }
        public void DrawCurve(XPen pen, PointF[] points, double tension) { g.DrawCurve(pen, points, tension); }
        public void DrawClosedCurve(XPen pen, PointF[] points, double tension) { g.DrawClosedCurve(pen, points, tension); }
        public void DrawClosedCurve(XSolidBrush brush, PointF[] points, double tension) { g.DrawClosedCurve(brush, points, XFillMode.Alternate, tension); }
        public void DrawRectangle(XPen pen, double x, double y, double width, double height) { g.DrawRectangle(pen, x, y, width, height); }
        public void DrawRectangle(XSolidBrush brush, double x, double y, double width, double height) { g.DrawRectangle(brush, x, y, width, height); }
        public void DrawRectangle(XSolidBrush brush, RectangleF rect) { g.DrawRectangle(brush, rect); }
        public void DrawEllipse(XPen pen, double x, double y, double width, double height) { g.DrawEllipse(pen, x, y, width, height); }
        public void DrawEllipse(XSolidBrush brush, double x, double y, double width, double height) { g.DrawEllipse(brush, x, y, width, height); }
        public void DrawEllipse(XPen pen, XSolidBrush brush, double x, double y, double width, double height) { g.DrawEllipse(pen, brush, x, y, width, height); }
        public void DrawArc(XPen pen, double x, double y, double width, double height, double startAngle, double sweepAngle) { g.DrawArc(pen, x, y, width, height, startAngle, sweepAngle); }
        public void DrawImage(MImage image, double x, double y, double width, double height) { g.DrawImage(image.XImage, x, y, width, height); }

        public void DrawImageAlpha(float alpha, MImage mimage, Rectangle targetRect)
        {
            // Clamp and Quantize
            alpha = Util.Clamp(alpha, 0f, 1f);
            alpha = (float)Math.Round(alpha * 16f) / 16f;
            if (alpha <= 0f)
                return;
            if (alpha >= 1f)
            {
                g.DrawImage(mimage.XImage, targetRect);
                return;
            }

            int key = (int)Math.Round(alpha * 16);

            Image image = mimage.Image;
            XImage ximage;
            int w, h;

            lock (image)
            {
                w = image.Width;
                h = image.Height;

                if (image.Tag == null || !(image.Tag is Dictionary<int, XImage>))
                    image.Tag = new Dictionary<int, XImage>();

                Dictionary<int, XImage> dict = image.Tag as Dictionary<int, XImage>;
                if (dict.ContainsKey(key))
                {
                    ximage = dict[key];
                }
                else
                {
                    // Need to construct a new image (PdfSharp can't alpha-render images)
                    // Memoize these in the image itself, since most requests will be from
                    // a small set

                    Bitmap scratchBitmap = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                    using (var scratchGraphics = Graphics.FromImage(scratchBitmap))
                    {
                        ColorMatrix matrix = new ColorMatrix();
                        matrix.Matrix00 = matrix.Matrix11 = matrix.Matrix22 = 1;
                        matrix.Matrix33 = alpha;

                        ImageAttributes attr = new ImageAttributes();
                        attr.SetColorMatrix(matrix);

                        scratchGraphics.DrawImage(image, new Rectangle(0, 0, w, h), 0, 0, w, h, GraphicsUnit.Pixel, attr);
                    }

                    ximage = XImage.FromGdiPlusImage(scratchBitmap);
                    dict[key] = ximage;
                }
            }

            lock (ximage)
            {
                g.DrawImage(ximage, targetRect);
            }
        }

        public XSize MeasureString(string text, XFont font) { return g.MeasureString(text, font); }
        public void DrawString(string s, XFont font, XSolidBrush brush, double x, double y, XStringFormat format) { g.DrawString(s, font, brush, x, y, format); }

        public MGraphicsState Save() { return new MXGraphicsState(g.Save()); }
        public void Restore(MGraphicsState state) { g.Restore(((MXGraphicsState)state).state); }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    g.Dispose();
                }
                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~MXGraphics() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

        private class MXGraphicsState : MGraphicsState
        {
            public XGraphicsState state;
            public MXGraphicsState(XGraphicsState state) { this.state = state; }
        }

    }

    // TODO: Fix vertical text alignment
    // TODO: Remove unreferenced definitions!

    internal class SVGGraphics : MGraphics
    {
        public const string MediaTypeName = "image/svg+xml";
        private const string NumberFormat = "G6";

        private class Element
        {
            public string name;
            public string content;
            public Dictionary<string, string> attributes = new Dictionary<string, string>();
            public List<Element> children = new List<Element>();

            public Element(string name) { this.name = name; }

            public Element Append(Element child) { children.Add(child); return child; }

            public void Serialize(TextWriter b)
            {
                b.Write("<");
                b.Write(name);
                foreach (KeyValuePair<string, string> entry in attributes)
                {
                    b.Write(" ");
                    b.Write(entry.Key);
                    b.Write("=\"");
                    b.Write(System.Security.SecurityElement.Escape(entry.Value));
                    b.Write("\"");
                }
                if (children.Count == 0 && string.IsNullOrWhiteSpace(content))
                {
                    b.Write("/>");
                    return;
                }
                b.Write(">");

                foreach (var child in children)
                    child.Serialize(b);

                if (!string.IsNullOrWhiteSpace(content))
                    b.Write(System.Security.SecurityElement.Escape(content));

                b.Write("</");
                b.Write(name);
                b.Write(">");
            }

            public bool Has(string name) { return attributes.ContainsKey(name); }
            public string Get(string name) { return attributes[name]; }
            public void Set(string name, string value) { attributes[name] = value; }
            public void Set(string name, double value) { attributes[name] = value.ToString(NumberFormat, CultureInfo.InvariantCulture); }
            public void Set(string name, XColor color) {
                if (color.IsEmpty || color.A == 0)
                    attributes[name] = "None";
                else if (color.A != 1.0)
                    attributes[name] = string.Format("rgba({0},{1},{2},{3:G6})", color.R, color.G, color.B, color.A);
                else
                    attributes[name] = string.Format("rgb({0},{1},{2})", color.R, color.G, color.B);
            }

            public void Apply(XPen pen)
            {
                if (pen == null)
                {
                    Set("stroke", XColor.Empty);
                }
                else
                {
                    Set("stroke", pen.Color);
                    Set("stroke-width", pen.Width);
                }
            }
            public void Apply(XSolidBrush brush)
            {
                Set("fill", brush == null ? XColor.Empty : brush.Color);
            }
            public void Apply(XPen pen, XSolidBrush brush)
            {
                Apply(pen);
                Apply(brush);
            }

            public int NodeCount { get { return 1 + children.Sum(c => c.NodeCount); } }
        }
        
        private class ElementNames
        {
            public const string DEFS = "defs";
            public const string CLIPPATH = "clipPath";

            public const string G = "g";
            public const string PATH = "path";
            public const string LINE = "line";
            public const string POLYLINE = "polyline";
            public const string RECT = "rect";
            public const string ELLIPSE = "ellipse";
            public const string TEXT = "text";
            public const string IMAGE = "image";
        }

        // Builds paths, using relative coordinates to reduce space.
        private class PathBuilder
        {
            private StringBuilder b = new StringBuilder();
            private double lastX = 0;
            private double lastY = 0;
            private bool used = false;

            public override string ToString() { return b.ToString().Trim(); }

            public PathBuilder() { }

            public void MoveTo(double x, double y)
            {
                if (!used)
                {
                    b.Append(string.Format("M {0:G6} {1:G6}", x, y));
                    used = true;
                }
                else
                {
                    b.Append(string.Format(" m {0:G6} {1:G6}", x - lastX, y - lastY));
                }
                lastX = x;
                lastY = y;
            }
            public void LineTo(double x, double y)
            {
                if (!used)
                {
                    b.Append(string.Format("L {0:G6} {1:G6}", x, y));
                    used = true;
                }
                else if (x == lastX)
                {
                    b.Append(string.Format(" v {0:G6}", y - lastY));
                }
                else if (y == lastY)
                {
                    b.Append(string.Format(" h {0:G6}", x - lastX));
                }
                else
                {
                    b.Append(string.Format(" l {0:G6} {1:G6}", x - lastX, y - lastY));
                }
                lastX = x;
                lastY = y;
            }
            public void ArcTo(double rx, double ry, double phi, int arcFlag, int sweepFlag, double x, double y)
            {
                if (!used)
                {
                    b.Append(string.Format("A {0:G6} {1:G6} {2:G6} {3} {4} {5:G6} {6:G6}",
                        rx, ry, phi, arcFlag, sweepFlag, x, y));
                    used = true;
                }
                else
                {
                    b.Append(string.Format(" a {0:G6} {1:G6} {2:G6} {3} {4} {5:G6} {6:G6}",
                        rx, ry, phi, arcFlag, sweepFlag, x - lastX, y - lastY));
                }
                lastX = x;
                lastY = y;
            }
            public void CurveTo(double x1, double y1, double x2, double y2, double x, double y)
            {
                if (!used)
                {
                    b.Append(string.Format("C {0:G6} {1:G6} {2:G6} {3:G6} {4:G6} {5:G6} ",
                        x1, y1, x2, y2, x, y));
                    used = true;
                }
                else
                {
                    b.Append(string.Format(" c {0:G6} {1:G6} {2:G6} {3:G6} {4:G6} {5:G6} ",
                        x1 - lastX, y1 - lastY, x2 - lastX, y2 - lastY, x - lastX, y - lastY));
                }
                lastX = x;
                lastY = y;
            }

            public void Close()
            {
                b.Append(" Z");
            }
        }

        private void Optimize(Element e)
        {
            // Simplify subtrees first
            foreach (var child in e.children)
                Optimize(child);

            // Remove <g>s with no children
            e.children.RemoveAll(child => child.name == ElementNames.G && child.children.Count == 0);

            // Flatten <g> with no properties
            List<Element> c = new List<Element>();
            foreach (var child in e.children)
            {
                if (child.name == ElementNames.G && child.attributes.Count == 0)
                    c.AddRange(child.children);
                else
                    c.Add(child);
            }
            e.children = c;
            
            // If a <g> has only a single child, merge
            if (e.name == ElementNames.G && e.children.Count == 1)
            {
                var child = e.children.First();
                // TODO: Other exclusive elements?
                if (!(e.Has("clip-path") && child.Has("clip-path")))
                {
                    e.name = child.name;
                    e.children = child.children;
                    e.content = child.content;
                    foreach (var entry in child.attributes)
                    {
                        if (e.attributes.ContainsKey(entry.Key))
                            e.attributes[entry.Key] += " " + entry.Value;
                        else
                            e.attributes[entry.Key] = entry.Value;
                    }
                }
            }
        }

        public void Serialize(TextWriter writer)
        {
            Optimize(root);

            writer.WriteLine("<?xml version = \"1.0\" encoding=\"utf-8\"?>");
            writer.Write(string.Format("<svg version=\"1.1\" baseProfile=\"full\" " +
                                "xmlns=\"http://www.w3.org/2000/svg\" " +
                                "xmlns:xlink=\"http://www.w3.org/1999/xlink\" " +
                                "width=\"{0}\" height=\"{1}\">",
                width, height));
            if (defs.children.Count > 0)
                defs.Serialize(writer);
            root.Serialize(writer);
            writer.Write("</svg>");
            writer.Flush();
        }

        private double width;
        private double height;
        private Element root = new Element(ElementNames.G);
        private Element defs = new Element(ElementNames.DEFS);

        private int def_id = 0;
        private Element AddDefinition(Element element)
        {
            element.Set("id", "did" + (++def_id).ToString(CultureInfo.InvariantCulture));
            defs.Append(element);
            return element;
        }

        private Stack<Element> stack = new Stack<Element>();

        private Element Current {  get { return stack.Peek(); } }

        private Element Open(Element element)
        {
            stack.Push(Current.Append(element));
            return element;
        }
        private Element Append(Element element)
        {
            return Current.Append(element);
        }

        public SVGGraphics(double width, double height)
        {
            this.width = width;
            this.height = height;
            stack.Push(root);
        }

        Graphics MGraphics.Graphics { get { return null; } }
        XSmoothingMode MGraphics.SmoothingMode { get; set; }
        public bool SupportsWingdings { get { return false; } }

        #region Drawing

        public void DrawLine(XPen pen, double x1, double y1, double x2, double y2)
        {
            var e = Append(new Element(ElementNames.LINE));
            e.Set("x1", x1);
            e.Set("y1", y1);
            e.Set("x2", x2);
            e.Set("y2", y2);
            e.Apply(pen);
        }

        public void DrawLines(XPen pen, XPoint[] points)
        {
            var e = Append(new Element(ElementNames.PATH));
            var path = new PathBuilder();
            path.MoveTo(points[0].X, points[0].Y);
            for (var i = 0; i < points.Length; ++i)
                    path.LineTo(points[i].X, points[i].Y);
            e.Set("d", path.ToString());
            e.Apply(pen, null);
        }

        public void DrawArc(XPen pen, double x, double y, double width, double height, double startAngle, double sweepAngle)
        {
            // Convert from center to endpoint parameterization
            // https://www.w3.org/TR/SVG/implnote.html#ArcImplementationNotes

            double rx = width / 2;
            double ry = height / 2;
            double cx = x + rx;
            double cy = y + ry;

            // GDI+ uses angles in degrees, clockwise from x axis
            startAngle = -startAngle * Math.PI / 180;
            sweepAngle = -sweepAngle * Math.PI / 180;

            // Since phi is always 0, conversion is simplified
            const double phi = 0;

            double x1 = rx * Math.Cos(startAngle) + cx;
            double y1 = -ry * Math.Sin(startAngle) + cy;
            double x2 = rx * Math.Cos(startAngle + sweepAngle) + cx;
            double y2 = -ry * Math.Sin(startAngle + sweepAngle) + cy;

            int fA = Math.Abs(sweepAngle) > Math.PI ? 1 : 0;
            int fS = sweepAngle < 0 ? 1 : 0;

            var e = Append(new Element(ElementNames.PATH));
            var path = new PathBuilder();
            path.MoveTo(x1, y1);
            path.ArcTo(rx, ry, phi, fA, fS, x2, y2);
            e.Set("d", path.ToString());
            e.Apply(pen, null);
        }

        public void DrawPath(XPen pen, XSolidBrush brush, XGraphicsPath path)
        {
            var e = Append(new Element(ElementNames.PATH));
            e.Set("d", ToSVG(path));
            e.Apply(pen, brush);
        }

        public void DrawCurve(XPen pen, PointF[] points, double tension)
        {
            var e = Append(new Element(ElementNames.PATH));
            e.Set("d", ToSVG(points, tension, false));
            e.Apply(pen, null);
        }

        public void DrawClosedCurve(XPen pen, XSolidBrush brush, PointF[] points, double tension)
        {
            var e = Append(new Element(ElementNames.PATH));
            e.Set("d", ToSVG(points, tension, true));
            e.Apply(pen, brush);
        }

        public void DrawRectangle(XPen pen, XSolidBrush brush, double x, double y, double width, double height)
        {
            var e = Append(new Element(ElementNames.RECT));
            e.Set("x", x);
            e.Set("y", y);
            e.Set("width", width);
            e.Set("height", height);
            e.Apply(pen, brush);
        }

        public void DrawEllipse(XPen pen, XSolidBrush brush, double x, double y, double width, double height)
        {
            var e = Append(new Element(ElementNames.ELLIPSE));
            e.Set("cx", x + width / 2);
            e.Set("cy", y + height / 2);
            e.Set("rx", width / 2);
            e.Set("ry", height / 2);
            e.Apply(pen, brush);
        }
        #endregion

        #region Images
        public void DrawImage(MImage image, double x, double y, double width, double height)
        {
            var e = Append(new Element(ElementNames.IMAGE));
            e.Set("x", x);
            e.Set("y", y);
            e.Set("width", width);
            e.Set("height", height);
            e.Set("xlink:href", image.Url);
        }

        public void DrawImageAlpha(float alpha, MImage image, Rectangle targetRect)
        {
            var e = Append(new Element(ElementNames.IMAGE));
            e.Set("x", targetRect.X);
            e.Set("y", targetRect.Y);
            e.Set("width", targetRect.Width);
            e.Set("height", targetRect.Height);
            e.Set("opacity", alpha);
            e.Set("xlink:href", image.Url);
        }
        #endregion

        #region Clipping
        public void IntersectClip(RectangleF rect)
        {
            var clipPath = AddDefinition(new Element(ElementNames.CLIPPATH));
            var r = clipPath.Append(new Element(ElementNames.RECT));
            r.Set("x", rect.X);
            r.Set("y", rect.Y);
            r.Set("width", rect.Width);
            r.Set("height", rect.Height);

            var e = Open(new Element(ElementNames.G));
            e.Set("clip-path", string.Format("url(#{0})", clipPath.Get("id")));
        }

        public void IntersectClip(XGraphicsPath path)
        {
            var clipPath = AddDefinition(new Element(ElementNames.CLIPPATH));
            var p = clipPath.Append(new Element(ElementNames.PATH));
            p.Set("d", ToSVG(path));

            var e = Open(new Element(ElementNames.G));
            e.Set("clip-path", string.Format("url(#{0})", clipPath.Get("id")));
        }
        #endregion

        #region Text
        private XGraphics scratch;
        public XSize MeasureString(string text, XFont font)
        {
            if (scratch == null) scratch = XGraphics.FromGraphics(Graphics.FromImage(new Bitmap(1, 1)), new XSize(1, 1));
            return scratch.MeasureString(text, font);
        }

        public void DrawString(string s, XFont font, XSolidBrush brush, double x, double y, XStringFormat format)
        {
            var e = Append(new Element(ElementNames.TEXT));
            e.content = s;

            e.Set("font-family", font.Name);
            e.Set("font-size", font.Size);
            if (font.Italic)
                e.Set("font-style", "italic");
            if (font.Bold)
                e.Set("font-weight", "bold");
            if (font.Underline)
                e.Set("text-decoration", "underline");
            else if (font.Strikeout)
                e.Set("text-decoration", "line-through");

            switch (format.Alignment)
            {
                case XStringAlignment.Near: break;
                case XStringAlignment.Center: e.Set("text-anchor", "middle"); break;
                case XStringAlignment.Far: e.Set("text-anchor", "end"); break;
            }

            switch (format.LineAlignment)
            {
                case XLineAlignment.Near: y += font.Size * 0.9; break;
                case XLineAlignment.Center: y += (font.Size * 0.9) / 2; break;
                case XLineAlignment.Far: break;
                case XLineAlignment.BaseLine: break;
            }

            e.Set("x", x);
            e.Set("y", y);
            e.Apply(brush);
        }
        #endregion

        #region Transforms
        public void ScaleTransform(double scaleX, double scaleY)
        {
            var e = Open(new Element(ElementNames.G));
            e.Set("transform", string.Format("scale({0:G6} {1:G6})", scaleX, scaleY));
        }
        public void TranslateTransform(double dx, double dy)
        {
            var e = Open(new Element(ElementNames.G));
            e.Set("transform", string.Format("translate({0:G6} {1:G6})", dx, dy));
        }
        public void RotateTransform(double angle)
        {
            var e = Open(new Element(ElementNames.G));
            e.Set("transform", string.Format("rotate({0:G6})", angle));
        }
        public void MultiplyTransform(XMatrix m)
        {
            // TODO: Verify matrix order
            var e = Open(new Element(ElementNames.G));
            e.Set("transform", string.Format("matrix({0:G6} {1:G6} {2:G6} {3:G6} {4:G6} {5:G6})", 
                m.M11, m.M12, m.M21, m.M22, m.OffsetX, m.OffsetY));
        }
        #endregion

        #region State
        public MGraphicsState Save()
        {
            var state = new State(new Element(ElementNames.G));
            Open(state.element);
            return state;
        }

        public void Restore(MGraphicsState state)
        {
            while (stack.Peek() != ((State)state).element)
                stack.Pop();
            stack.Pop();
        }

        private class State : MGraphicsState
        {
            public Element element;
            public State(Element e) { element = e; }
        }
        #endregion

        #region Relay Methods
        public void DrawLine(XPen pen, PointF pt1, PointF pt2)
        {
            DrawLine(pen, pt1.X, pt1.Y, pt2.X, pt2.Y);
        }
        public void DrawPath(XSolidBrush brush, XGraphicsPath path)
        {
            DrawPath(null, brush, path);
        }
        public void DrawPath(XPen pen, XGraphicsPath path)
        {
            DrawPath(pen, null, path);
        }
        public void DrawRectangle(XSolidBrush brush, RectangleF rect)
        {
            DrawRectangle(null, brush, rect.X, rect.Y, rect.Width, rect.Height);
        }
        public void DrawRectangle(XSolidBrush brush, double x, double y, double width, double height)
        {
            DrawRectangle(null, brush, x, y, width, height);
        }
        public void DrawRectangle(XPen pen, double x, double y, double width, double height)
        {
            DrawRectangle(pen, null, x, y, width, height);
        }
        public void DrawEllipse(XSolidBrush brush, double x, double y, double width, double height)
        {
            DrawEllipse(null, brush, x, y, width, height);
        }
        public void DrawEllipse(XPen pen, double x, double y, double width, double height)
        {
            DrawEllipse(pen, null, x, y, width, height);
        }
        public void DrawClosedCurve(XSolidBrush brush, PointF[] points, double tension)
        {
            DrawClosedCurve(null, brush, points, tension);
        }
        public void DrawClosedCurve(XPen pen, PointF[] points, double tension)
        {
            DrawClosedCurve(pen, null, points, tension);
        }

        public void ScaleTransform(double scaleXY)
        {
            ScaleTransform(scaleXY, scaleXY);
        }
        #endregion

        #region Utilities
        private string ToSVG(XGraphicsPath x)
        {
            var gp = x.Internals.GdiPath.PathData;

            PathBuilder path = new PathBuilder();

            for (int i = 0; i < gp.Points.Length; ++i)
            {
                byte type = gp.Types[i];
                PointF point = gp.Points[i];
                switch (type & 0x7)
                {
                    case 0: path.MoveTo(point.X, point.Y); break;
                    case 1: path.LineTo(point.X, point.Y); break;
                    case 3: throw new ApplicationException("Unsupported path point type: " + type);
                }

                if ((type & 0x20) != 0)
                    throw new ApplicationException("Unsupported path flag type: " + type);

                if ((type & 0x80) != 0)
                    path.Close();
            }

            return path.ToString();
        }

        private string ToSVG(PointF[] points, double tension, bool closed)
        {
            PathBuilder path = new PathBuilder();

            float a = (float)(tension + 1);
            PointF last = PointF.Empty;
            PointF lastd = PointF.Empty;

            Func<int, PointF> deriv = (int i) =>
            {
                if (closed)
                {
                    int j = (i + 1) % points.Length;
                    int k = (i > 0) ? i - 1 : points.Length - 1;
                    return new PointF((points[j].X - points[k].X) / a, (points[j].Y - points[k].Y) / a);
                }

                if (i == 0)
                    return new PointF((points[1].X - points[0].X) / a, (points[1].Y - points[0].Y) / a);
                else if (i == points.Length - 1)
                    return new PointF((points[i].X - points[i - 1].X) / a, (points[i].Y - points[i - 1].Y) / a);
                else
                    return new PointF((points[i + 1].X - points[i - 1].X) / a, (points[i + 1].Y - points[i - 1].Y) / a);
            };
               
            for (int i = 0; i < points.Length; ++i)
            {
                PointF point = points[i];
                PointF pointd = deriv(i);

                if (i == 0)
                {
                    path.MoveTo(point.X, point.Y);
                }
                else
                {
                    path.CurveTo(
                        last.X + lastd.X / 3, last.Y + lastd.Y / 3,
                        point.X - pointd.X / 3, point.Y - pointd.Y / 3,
                        point.X, point.Y);
                }

                last = point;
                lastd = pointd;
            }

            if (closed)
            {
                PointF point = points[0];
                PointF pointd = deriv(0);

                path.CurveTo(
                    last.X + lastd.X / 3, last.Y + lastd.Y / 3, 
                    point.X - pointd.X / 3, point.Y - pointd.Y / 3,
                    point.X, point.Y);
                path.Close();
            }

            return path.ToString();
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~SVGGraphics() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}