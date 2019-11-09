using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text.pdf;

namespace VectorGraphicViewer
{
    class Circle : GraphicObject
    {
       
        private float radius;
        private bool filled;
        private float scaled_radius;
        public Circle(LineTypes lt, Color col,  GraphicPoint c,float r, bool f) :base(lt, col)
        {
            points = new GraphicPoint[1];
            points[0] = c; filled = f; scaled_radius=radius = r;
        }
        #region helpers
        public override float leftmost_x()
        {
            return points[0].X - scaled_radius;
        }

        public override float rightmost_x()
        {
            return points[0].X + scaled_radius;
        }

        public override void setScaling(float scaling)
        {
            base.setScaling(scaling);
            scaled_radius = radius * scaling;
        }

        #endregion
        #region drawing
        public override void draw(Graphics g, float scaling)
        {
            setScaling(scaling);
            if (filled)
            {
                Brush brush = new SolidBrush(pen.Color);
                g.FillEllipse(brush, points[0].X  - scaled_radius, points[0].Y  - scaled_radius, 2 * scaled_radius, 2 * scaled_radius);
            }
            else
            {
                g.DrawEllipse(pen, points[0].X  - scaled_radius, points[0].Y  - scaled_radius, 2 * scaled_radius, 2 * scaled_radius);
            }
        }

        public override void draw(Bitmap bitmap, float scaling)
        {
            setScaling(scaling);
            int center_x= (int)points[0].X;
            int center_y = (int)points[0].Y;
          

            double angle = 0;
            while (angle < 90)
            {

                int x = (int)(scaled_radius * Math.Cos(angle * Math.PI / 180));
                int y = (int)(scaled_radius * Math.Sin(angle * Math.PI / 180));
                bitmap.SetPixel(center_x + x + bitmap.Width / 2, center_y + y + bitmap.Height / 2, pen.Color);
                bitmap.SetPixel(center_x - x + bitmap.Width / 2, center_y + y + bitmap.Height / 2, pen.Color);

                bitmap.SetPixel(center_x + x + bitmap.Width / 2, center_y - y + bitmap.Height / 2, pen.Color);
                bitmap.SetPixel(center_x - x + bitmap.Width / 2, center_y - y + bitmap.Height / 2, pen.Color);

                angle += 0.1;
            }

            if (filled)
            {
                int int_radius = (int)scaled_radius - 1;
                while (int_radius > 0)
                {
                    angle = 0;
                    while (angle < 90)
                    {

                        int x = (int)(int_radius * Math.Cos(angle * Math.PI / 180));
                        int y = (int)(int_radius * Math.Sin(angle * Math.PI / 180));
                        bitmap.SetPixel(center_x + x + bitmap.Width / 2, center_y + y + bitmap.Height / 2, pen.Color);
                        bitmap.SetPixel(center_x - x + bitmap.Width / 2, center_y + y + bitmap.Height / 2, pen.Color);

                        bitmap.SetPixel(center_x + x + bitmap.Width / 2, center_y - y + bitmap.Height / 2, pen.Color);
                        bitmap.SetPixel(center_x - x + bitmap.Width / 2, center_y - y + bitmap.Height / 2, pen.Color);

                        angle += 0.1;
                    }

                    int_radius--;
                }
            }
        }

        public override void draw(PdfContentByte cb, float doc_width, float doc_height, float scaling)
        {
            setScaling(scaling);
            base.draw(cb, doc_width, doc_height, scaling);

            cb.Circle(points[0].X + doc_width / 2, points[0].Y + doc_height / 2, scaled_radius);
            if (filled)
            {
                cb.ClosePathFillStroke();

            }
            else
            {
                cb.ClosePathStroke();
            }


        }
        public override List<GraphicPoint> intesection(GraphicObject o)
        {
            return o.intesectionCircle(points[0], scaled_radius);
        }
        #endregion
        #region intersection
        public override bool contains(GraphicPoint point)
        {
            if (Math.Abs(Math.Pow(points[0].X - point.X, 2) + Math.Pow(points[0].Y - point.Y, 2) - Math.Pow(scaled_radius, 2)) < 0.1)
                return true;
            else
                return false;
        }

        public override List<GraphicPoint> intesectionLine(float k, float n)
        {
            List<GraphicPoint> list = new List<GraphicPoint>();
            float cx = points[0].X;
            float cy = points[0].Y;

            float A = 1 + k * k;
            float B = -2 * cx + 2 * k * (n - cy);
            float C = cx * cx + (n - cy) * (n - cy) - scaled_radius * scaled_radius;

            float D = B * B - 4 * A * C;
            if (D < 0) return list;

            float x1 = (float)(-B + Math.Sqrt(D)) / (2 * A);
            float y1 = k * x1 + n;


            float x2 = (float)(-B - Math.Sqrt(D)) / (2 * A);
            float y2 = k * x2 + n;

            list.Add(new GraphicPoint(x1, y1));
            list.Add(new GraphicPoint(x2, y2));

            return list;
        }

        public override List<GraphicPoint> intesectionCircle(GraphicPoint center1, float radius1)
        {
            GraphicPoint center = points[0];
            float dx = center.X - center1.X;
            float dy = center.Y - center1.Y;
            double dist = Math.Sqrt(dx * dx + dy * dy);

            List<GraphicPoint> list = new List<GraphicPoint>();
           
            if ((dist > scaled_radius + radius1) || (dist < Math.Abs(scaled_radius - radius1)) || ((dist == 0) && (scaled_radius == radius1)))
            {
                return list;
            }

            double a = (scaled_radius * scaled_radius - radius1 * radius1 + dist * dist) / (2 * dist);
            double h = Math.Sqrt(scaled_radius * scaled_radius - a * a);

               
            double cx2 = center.X + a * (center1.X - center.X) / dist;
            double cy2 = center.Y + a * (center1.Y - center.Y) / dist;

          
            list.Add( new GraphicPoint(
                    (float)(cx2 + h * (center1.Y - center.Y) / dist),
                    (float)(cy2 - h * (center1.X - center.X) / dist)));
            list.Add(new GraphicPoint(
                   (float)(cx2 - h * (center1.Y - center.Y) / dist),
                   (float)(cy2 + h * (center1.X - center.X) / dist)));

            return list;  

        }
        #endregion

    }
}
