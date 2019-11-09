using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text.pdf;

namespace VectorGraphicViewer
{
    class Line : GraphicObject
    {
       
       
        public Line(LineTypes lt, Color c, GraphicPoint a, GraphicPoint b):base(lt, c)
        {
            points = new GraphicPoint[2];
            points[0] = a;points[1] = b;
          
        }
        #region helpers
        public override float leftmost_x()
        {
            return Math.Min(points[0].X, points[1].X);
        }

        public override float rightmost_x()
        {
            return Math.Max(points[0].X, points[1].X);
        }

        internal float X(float y)
        {
            if (Math.Abs(points[1].X - points[0].X) < 0.01)
            {
                return (int)points[0].X;
            }
            float k = (points[1].Y - points[0].Y) / (points[1].X - points[0].X);
            float n = points[0].Y - k * points[0].X;
            return (y - n) / k;
        }

        internal float Y(float x)
        {
            if (Math.Abs(points[1].X - points[0].X) < 0.01)
            {
                return Math.Max(points[0].Y, points[1].Y);
            }
            float k = (points[1].Y - points[0].Y) / (points[1].X - points[0].X);
            float n = points[0].Y - k * points[0].X;
            return k*x+n;
        }
        #endregion
        #region drawing
        public override void draw(Graphics g, float scaling) 
        {
            setScaling(scaling);
            
            g.DrawLine(pen, points[0].X, points[0].Y, points[1].X, points[1].Y);
            
        }
        public override void draw(Bitmap bitmap, float scaling)
        {
            setScaling(scaling);

            if (Math.Abs(points[1].X - points[0].X) < 0.01)
            {
                int y_min = Math.Min((int)points[0].Y, (int)points[1].Y);
                int y_max = Math.Max((int)points[0].Y, (int)points[1].Y);
                while (y_min < y_max)
                {
                   
                    bitmap.SetPixel((int)points[0].X + bitmap.Width / 2, y_min + bitmap.Height / 2, pen.Color);
                    y_min++;
                }
            }
            else
            {
                float k = (points[1].Y - points[0].Y) / (points[1].X - points[0].X);
                float n = points[0].Y - k * points[0].X;

                int x_min = Math.Min((int)points[0].X, (int)points[1].X);
                int x_max = Math.Max((int)points[0].X, (int)points[1].X);
                while (x_min < x_max)
                {
                    int y = (int)(k * x_min + n);
                    if ((Math.Abs(x_min + bitmap.Width / 2) < bitmap.Width) && (Math.Abs(y + bitmap.Height / 2) < bitmap.Height))
                        bitmap.SetPixel(x_min + bitmap.Width / 2, y + bitmap.Height / 2, pen.Color);
                    x_min++;
                }
            }
        }

        public override void draw(PdfContentByte cb,float doc_width,float doc_height,float scaling)
        {
            setScaling(scaling);

            base.draw(cb, doc_width, doc_height, scaling);
            cb.MoveTo(points[0].X+ doc_width/2, points[0].Y + doc_height / 2);

            cb.LineTo(points[1].X + doc_width / 2, points[1].Y + doc_height / 2);

            cb.ClosePathStroke();
        }

        #endregion

        #region intersection
        public override bool contains(GraphicPoint point)
        {
            float k = (points[1].Y - points[0].Y) / (points[1].X - points[0].X);
            float n = points[0].Y - k * points[0].X;

            if ((point.X > leftmost_x()) && point.X < rightmost_x() && Math.Abs(point.Y -(k * point.X + n))<0.01)
                return true;
            else
                return false;
        }

      

        public override List<GraphicPoint> intesection(GraphicObject o)
        {
            float k = (points[1].Y - points[0].Y) / (points[1].X - points[0].X);
            float n = points[0].Y - k * points[0].X;
            return o.intesectionLine(k, n);
        }

        public override List<GraphicPoint> intesectionLine(float k1, float n1)
        {
            List<GraphicPoint> list = new List<GraphicPoint>();
            float k2 = (points[1].Y - points[0].Y) / (points[1].X - points[0].X);
            float n2 = points[0].Y - k2 * points[0].X;

            if (k1 == k2) return list;
            float x = (n2 - n1) / (k1 - k2);
            float y = k1 * x + n1;
            list.Add(new GraphicPoint(x, y));
            return list;
        }

        public override List<GraphicPoint> intesectionCircle(GraphicPoint center, float radius)
        {

            List<GraphicPoint> list = new List<GraphicPoint>();
            float cx = center.X;
            float cy = center.Y;
         

            float k = (points[1].Y - points[0].Y) / (points[1].X - points[0].X);
            float n = points[0].Y - k * points[0].X;

            float A = 1 + k*k;
            float B = -2 * cx + 2 * k * (n - cy);
            float C = cx * cx + (n - cy) * (n - cy) - radius * radius;

            float D = B * B - 4 * A * C;
            if (D<0) return list;

            float x1 = (float)(-B + Math.Sqrt(D)) / (2 * A);
            float y1 = k * x1 + n;


            float x2 = (float)(-B - Math.Sqrt(D)) / (2 * A);
            float y2 = k * x2 + n;

            list.Add(new GraphicPoint(x1, y1));
            list.Add(new GraphicPoint(x2, y2));

            return list;
        }

        #endregion
    }
}
