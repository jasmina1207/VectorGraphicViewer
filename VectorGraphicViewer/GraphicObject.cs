using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using iTextSharp.text.pdf;

namespace VectorGraphicViewer
{
    enum LineTypes {Solid, Dot, Dash,DashDot }
    class GraphicPoint
    {
        float x;
        float y;
        float scaling;
        public GraphicPoint(float cx, float cy)
        { x = cx;y = cy;
          scaling = 1;
        }
      
        public float X
        {
            get { return x*scaling; }
        }
       

        public float Y
        {
            get { return y* scaling; }
        }

       public float Scaling
        {
            get { return  scaling; }
            set { scaling = value; }
        }

        public static implicit operator Point(GraphicPoint gp)
        {
            return new Point((int)gp.X, (int)gp.Y);
        }

        public void draw(Graphics g)
        { 
            g.DrawLine(new Pen(Color.DarkBlue), X - 2, Y - 2, X + 2, Y + 2);
            g.DrawLine(new Pen(Color.DarkBlue), X - 2, Y + 2, X + 2, Y - 2);
        }
    }
    abstract class GraphicObject
    {
       protected LineTypes lineType;
       protected Color color;
       protected Pen pen;
       protected GraphicPoint[] points;

       public GraphicObject(LineTypes lt, Color c) {
            lineType = lt;color = c;
            pen = new Pen(c);
            
            DashStyle ds;
            Enum.TryParse(lt.ToString(), true, out ds);
            pen.DashStyle = ds;
        }

        #region helpers
        public virtual void setScaling(float scaling)
        {
            if (points != null)
            {
                foreach (GraphicPoint gp in points)
                    gp.Scaling = scaling;
            }
        }
        public abstract float leftmost_x();
        public abstract float rightmost_x();
        #endregion
        #region drawing
        public abstract void draw(Graphics g, float scaling);
        public abstract void draw(Bitmap bitmap, float scaling);
        public virtual void draw(PdfContentByte cb, float doc_width, float doc_height, float scaling)
        {
  
            cb.SetColorStroke(new iTextSharp.text.BaseColor(color));
            cb.SetColorFill(new iTextSharp.text.BaseColor(color));
            float width = 1.5f;
            cb.SetLineWidth(width);
            switch (lineType)
            {
                case LineTypes.Dash:
                    cb.SetLineDash(new float[] { width * 3, width }, 0);
                    break;
                case LineTypes.Dot:
                    cb.SetLineDash(new float[] { width }, 0);
                    break;
                case LineTypes.DashDot:
                    cb.SetLineDash(new float[] { width * 3, width, width }, 0);
                    break;
                case LineTypes.Solid:
                default:
                    cb.SetLineDash(new float[] { }, 0);
                    break;
            }
        }

        #endregion
        #region intersection
        public abstract bool contains(GraphicPoint point);

        public abstract List<GraphicPoint> intesection(GraphicObject o);

        public abstract List<GraphicPoint> intesectionLine(float k, float n);

        public abstract List<GraphicPoint> intesectionCircle(GraphicPoint center,float radius);
        #endregion
    }
}
