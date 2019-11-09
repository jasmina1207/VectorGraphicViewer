using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text.pdf;

namespace VectorGraphicViewer
{
    class Triangle : GraphicObject
    {
        
        private bool filled;

        public Triangle(LineTypes lt, Color col, GraphicPoint a, GraphicPoint b, GraphicPoint c, bool f) :base(lt, col)
        {
            points = new GraphicPoint[3];
            points[0] = a; points[1] = b; points[2] = c; filled = f;
        }
        
        #region helpers
        public override float leftmost_x()
        {
            return Math.Min(points[0].X, Math.Min(points[1].X, points[1].X));
        }

        public override float rightmost_x()
        {
            return Math.Max(points[0].X, Math.Max(points[1].X, points[1].X));
        }

        public Line[] asLines()
        {
            Line[] lines = new Line[] { new Line(lineType, color, points[0], points[1]), new Line(lineType, color, points[1], points[2]), new Line(lineType, color, points[0], points[2]) };
            return lines;
        }
        #endregion
        #region drawing
        public override void draw(Graphics g, float scaling)
        {
            setScaling(scaling);
            if (filled)
            {
                Brush brush = new SolidBrush(pen.Color);
                g.FillPolygon(brush, new Point[] { points[0], points[1], points[2] });
            }
            else
            {
                g.DrawPolygon(pen, new Point[] { points[0], points[1], points[2] });
            }
            
        }
        public override void draw(Bitmap bitmap, float scaling)
        {
            
            if (!filled)
            {
                foreach (Line line in asLines())
                {
                    line.draw(bitmap, scaling);
                }
            }

           else
            {
                setScaling(scaling);
                List<GraphicPoint> list_points = points.ToList();
                list_points.Sort(delegate (GraphicPoint p1, GraphicPoint p2) { return -p1.Y.CompareTo(p2.Y); });

                GraphicPoint a = list_points[0];
                GraphicPoint b = list_points[1];
                GraphicPoint c = list_points[2];

                Line ab = new Line(lineType, color, a, b);
                Line bc = new Line(lineType, color, b, c);
                Line ac = new Line(lineType, color, a, c);


                Line leftEdge;
                Line rightEdge;

                if (ab.leftmost_x() < ac.leftmost_x())
                {
                    leftEdge = ab; rightEdge = ac;
                }
                else { leftEdge = ac; rightEdge = ab; }
             
                Line bottomEdge = bc;

                int startY = (int)a.Y, endY = (int)c.Y;
                for (int y = startY; y >= endY; y--)
                {
                    int startX, endX = (int)rightEdge.X(y);
                    
                    if (y < leftEdge.Y((int)leftmost_x()))
                        startX = (int)bottomEdge.X(y);
                    else
                        startX = (int)leftEdge.X(y);

                    for (int x = startX; x <= endX; x++)
                    {
                        if ((Math.Abs(x + bitmap.Width / 2)< bitmap.Width) &&(Math.Abs(y + bitmap.Height / 2)<bitmap.Height))
                            bitmap.SetPixel(x + bitmap.Width / 2, y + bitmap.Height / 2, pen.Color);
                    }
                }
 
            }
        }
        public override void draw(PdfContentByte cb, float doc_width, float doc_height, float scaling)
        {
            setScaling(scaling);
            base.draw(cb, doc_width, doc_height, scaling);
           
            cb.MoveTo(points[0].X + doc_width / 2, points[0].Y + doc_height / 2);
            cb.LineTo(points[1].X + doc_width / 2, points[1].Y + doc_height / 2);
            cb.LineTo(points[2].X + doc_width / 2, points[2].Y + doc_height / 2);
            cb.LineTo(points[0].X + doc_width / 2, points[0].Y + doc_height / 2);
            if (filled)
            {
                cb.ClosePathFillStroke();
            }
            else
            {
                cb.ClosePathStroke();
            }
        }

        #endregion
        #region intersection
        public override bool contains(GraphicPoint point)
        {
            foreach (Line l in asLines())
                if (l.contains(point)) return true;
            return false;
        }

        public override List<GraphicPoint> intesection(GraphicObject o)
        {
            List<GraphicPoint> points = new List<GraphicPoint>();
            foreach (Line line in asLines())
            {
                points.AddRange(line.intesection(o));
            }
            return points;
        }

        public override List<GraphicPoint> intesectionLine(float k, float n)
        {
            List<GraphicPoint> points = new List<GraphicPoint>();
            foreach (Line line in asLines())
            {
                points.AddRange(line.intesectionLine(k,n));
            }
            return points;
        }

        public override List<GraphicPoint> intesectionCircle(GraphicPoint center, float radius)
        {
            List<GraphicPoint> points = new List<GraphicPoint>();
            foreach (Line line in asLines())
            {
                points.AddRange(line.intesectionCircle(center, radius));
            }
            return points;
        }
        #endregion
    }
}
