using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace VectorGraphicViewer
{
    public partial class Viewer : Form
    {
        static List<GraphicObject> list_gobjects;
        static float max_x = 0;
        static float max_y = 0;
        Graphics graphics;
        const string fileName = "graphicObjectsFile.json";
        private MainMenu mainMenu;
        private MenuItem menuItem_draw;
        private MenuItem menuItem_intersectios;
        public Viewer()
        {
            mainMenu = new MainMenu();
            MenuItem options = mainMenu.MenuItems.Add("&Options");
            menuItem_draw = new MenuItem("&Draw");
            menuItem_draw.Enabled = true;

            menuItem_draw.MenuItems.Add(new MenuItem("&Using graphics", new EventHandler(this.drawGraphics_click)));
            menuItem_draw.MenuItems.Add(new MenuItem("&Using set pixel", new EventHandler(this.drawSetPixel_click)));

            menuItem_intersectios = new MenuItem("&Find intersectios", new EventHandler(this.findIntersectios_click));
            menuItem_intersectios.Enabled = false;

            options.MenuItems.Add(menuItem_draw);
            options.MenuItems.Add(menuItem_intersectios);
            options.MenuItems.Add(new MenuItem("&Export to pdf", new EventHandler(this.exportToPDF_click)));
            this.Menu = mainMenu;
            
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;


            graphics = this.CreateGraphics();
            graphics.ScaleTransform(1.0F, -1.0F);
            graphics.TranslateTransform(this.Width / 2, -this.Height / 2);
          
            list_gobjects = new List<GraphicObject>();
            readFile();

        }

        #region ReadFile
        private void readFile()
        {
            StreamReader sr = new StreamReader(fileName);
            string jsonString = sr.ReadToEnd();
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            List<Dictionary<string, string>> gobjects = serializer.Deserialize<List<Dictionary<string, string>>>(jsonString);
            string type;
            foreach (Dictionary<string, string> gobject in gobjects)
            {

                if (gobject.TryGetValue("type", out type))
                {
                    switch (type)
                    {
                        case "line":
                            {
                                createLine(gobject);
                                break;
                            }
                        case "circle":
                            {
                                createCircle(gobject);
                                break;
                            }
                        case "triangle":
                            {
                                createTriangle(gobject);
                                break;
                            }
                    }
                }
            }
        }

        private void createLine(Dictionary<string, string> gobject)
        {
            string str_a, str_b, str_color, str_lineType;
            gobject.TryGetValue("a", out str_a);
            gobject.TryGetValue("b", out str_b);
            gobject.TryGetValue("color", out str_color);
            gobject.TryGetValue("lineType", out str_lineType);

            GraphicPoint a = new GraphicPoint(float.Parse(str_a.Split(';')[0].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(str_a.Split(';')[1].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture));
            GraphicPoint b = new GraphicPoint(float.Parse(str_b.Split(';')[0].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(str_b.Split(';')[1].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture));


            max_x = Math.Max(max_x, Math.Max(Math.Abs(a.X), Math.Abs(b.X)));
            max_y = Math.Max(max_y, Math.Max(Math.Abs(a.Y), Math.Abs(b.Y)));


            Color color = Color.FromArgb(int.Parse(str_color.Split(';')[0]), int.Parse(str_color.Split(';')[1]), int.Parse(str_color.Split(';')[2]), int.Parse(str_color.Split(';')[3]));
            LineTypes lt;
            Enum.TryParse(str_lineType, true, out lt);
            list_gobjects.Add(new Line(lt, color, a, b));
        }
        private void createCircle(Dictionary<string, string> gobject)
        {
            string str_center, str_radius, str_color, str_lineType, str_fill;
            gobject.TryGetValue("center", out str_center);
            gobject.TryGetValue("radius", out str_radius);
            gobject.TryGetValue("color", out str_color);
            gobject.TryGetValue("lineType", out str_lineType);
            gobject.TryGetValue("filled", out str_fill);
            GraphicPoint center = new GraphicPoint(float.Parse(str_center.Split(';')[0].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(str_center.Split(';')[1].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture));
            float radius = float.Parse(str_radius.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture);
            bool filled = bool.Parse(str_fill);

            max_x = Math.Max(max_x, center.X + radius);
            max_y = Math.Max(max_y, center.Y + radius);


            Color color = Color.FromArgb(int.Parse(str_color.Split(';')[0]), int.Parse(str_color.Split(';')[1]), int.Parse(str_color.Split(';')[2]), int.Parse(str_color.Split(';')[3]));
            LineTypes lt;
            Enum.TryParse(str_lineType, true, out lt);
            list_gobjects.Add(new Circle(lt, color, center, radius, filled));
        }
        private void createTriangle(Dictionary<string, string> gobject)
        {
            string str_a, str_b,str_c, str_color, str_lineType, str_fill;
            gobject.TryGetValue("a", out str_a);
            gobject.TryGetValue("b", out str_b);
            gobject.TryGetValue("c", out str_c);
            gobject.TryGetValue("color", out str_color);
            gobject.TryGetValue("lineType", out str_lineType);
            gobject.TryGetValue("filled", out str_fill);

            GraphicPoint a = new GraphicPoint(float.Parse(str_a.Split(';')[0].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(str_a.Split(';')[1].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture));
            GraphicPoint b = new GraphicPoint(float.Parse(str_b.Split(';')[0].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(str_b.Split(';')[1].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture));
            GraphicPoint c = new GraphicPoint(float.Parse(str_c.Split(';')[0].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(str_c.Split(';')[1].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture));


            max_x = Math.Max(max_x, Math.Max(Math.Abs(a.X), Math.Max(Math.Abs(b.X), Math.Abs(c.X))));
            max_y = Math.Max(max_y, Math.Max(Math.Abs(a.Y), Math.Max(Math.Abs(b.Y), Math.Abs(c.Y))));

            bool filled = bool.Parse(str_fill);
            Color color = Color.FromArgb(int.Parse(str_color.Split(';')[0]), int.Parse(str_color.Split(';')[1]), int.Parse(str_color.Split(';')[2]), int.Parse(str_color.Split(';')[3]));
            LineTypes lt;
            Enum.TryParse(str_lineType, true, out lt);
            list_gobjects.Add(new Triangle(lt, color, a, b,c,filled));
        }

        #endregion

        #region Drawing

        private void drawGraphics_click(object sender, EventArgs e)
        {
            menuItem_draw.Enabled = false;
            menuItem_intersectios.Enabled = true;
            draw(false);
        }

        private void drawSetPixel_click(object sender, EventArgs e)
        {
            menuItem_draw.Enabled = false;
            menuItem_intersectios.Enabled = true;
            draw(true);
        }

        private void draw(bool setPixel)
        {
            float scaling;
            float scalingY = (float)this.Height / (max_y*2);
            float scalingX = (float)this.Width /( max_x*2);
            if (scalingX < scalingY) scaling = scalingX; else scaling = scalingY;

            graphics.DrawLine(new Pen(Color.Red),- this.Width / 2,0, this.Width, 0);
            graphics.DrawLine(new Pen(Color.Red), 0, -this.Height/2, 0,this.Height/2);


            Bitmap bitmap = new Bitmap(this.Width, this.Height);
          
            foreach (GraphicObject gobject in list_gobjects)
            {
                if (setPixel)
                    gobject.draw(bitmap, scaling);
                else
                     gobject.draw(graphics, scaling);
               
            }

            if (setPixel)
                graphics.DrawImage(bitmap, -bitmap.Width/2, -bitmap.Height/2, bitmap.Width, bitmap.Height);



        }
        #endregion

        #region Intersections
        private void findIntersectios_click(object sender, EventArgs e)
        {
                       
            List<GraphicPoint> points = findIntersections();
            foreach (GraphicPoint gpoint in points)
             {
                 gpoint.draw(graphics);

            }

        }
        List<GraphicPoint> findIntersections()
        {
            List<GraphicPoint> points = new List<GraphicPoint>();
            //  list_gobjects.Sort(delegate (GraphicObject o1, GraphicObject o2) { return o1.leftmost_x().CompareTo(o2.leftmost_x()); });
            list_gobjects.Sort((o1, o2) => o1.leftmost_x().CompareTo(o2.leftmost_x()));
            for (int i = 0; i < list_gobjects.Count; i++)
            {
                for (int j = i + 1; j < list_gobjects.Count && (list_gobjects[i].rightmost_x() > list_gobjects[j].leftmost_x()); j++)
                {
                  
                    GraphicObject o1 = list_gobjects[i];
                    GraphicObject o2 = list_gobjects[j];
                    
                   
                    foreach (GraphicPoint p in o1.intesection(o2))
                        {
                            if (o1.contains(p) && o2.contains(p))
                                points.Add(p);
                        }
                    }
            }
            return points;
        }
        #endregion

        #region Export to pdf
        private void exportToPDF_click(object sender, EventArgs e)
        {


            System.IO.FileStream fs = new FileStream("Graphics.pdf", FileMode.Create);
            
            Document document = new Document(PageSize.A4);
           
            PdfWriter writer = PdfWriter.GetInstance(document, fs);
           
            document.Open();

            PdfContentByte cb = writer.DirectContent;
            float scaling;
            float scalingY = (float)document.PageSize.Height / (max_y * 2);
            float scalingX = (float)document.PageSize.Width / (max_x * 2);
            if (scalingX < scalingY) scaling = scalingX; else scaling = scalingY;

            cb.SetColorStroke(new iTextSharp.text.BaseColor(Color.Red));

            cb.MoveTo(0, document.PageSize.Height / 2);

            cb.LineTo(document.PageSize.Width, document.PageSize.Height / 2);

            cb.ClosePathStroke();

            cb.MoveTo(document.PageSize.Width / 2, 0);

            cb.LineTo(document.PageSize.Width/2, document.PageSize.Height);

            cb.ClosePathStroke();

            foreach (GraphicObject gobject in list_gobjects)
            {
                gobject.draw(cb, document.PageSize.Width, document.PageSize.Height, scaling);
                
            }

            document.Close();
            writer.Close();
            fs.Close();
           
        }
        #endregion

    }
}
