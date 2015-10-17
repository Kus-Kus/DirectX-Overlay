using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Runtime.InteropServices;
using D3D = Microsoft.DirectX.Direct3D;


namespace DirectX_Overlay
{
    public partial class TransparentBorderlessForm : Form
    {

        private Margins marg;
        public PresentParameters presentParams;
        public Texture texture;

        private static D3D.Font font;
        public static D3D.Line drawBoxLine;
        public static D3D.Line drawLine;
        public static D3D.Line drawCircleLine;
        public static D3D.Line drawFilledBoxLine;
        public static D3D.Line drawTriLine;

        //This is used to specify the boundaries of the transparent area
        internal struct Margins
        {
            public int Left, Right, Top, Bottom;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern UInt32 GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_LAYERED = 0x80000;
        public const int WS_EX_TRANSPARENT = 0x20;
        public const int LWA_ALPHA = 0x2;
        public const int LWA_COLORKEY = 0x1;

        Color red = Color.FromArgb(100, 255, 0, 0);
        Color green = Color.FromArgb(100, 0, 255, 0);
        Color blue = Color.FromArgb(100, 0, 0, 255);
        float CenterX = 0.0f;
        float CenterY = 0.0f;

        int CenterrX = 0;
        int CenterrY = 0;

        [DllImport("dwmapi.dll")]
        static extern void DwmExtendFrameIntoClientArea(IntPtr hWnd, ref Margins pMargins);

        private Device device = null;
        public TransparentBorderlessForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetWindowLong(this.Handle, GWL_EXSTYLE,
        (IntPtr)(GetWindowLong(this.Handle, GWL_EXSTYLE) ^ WS_EX_LAYERED ^ WS_EX_TRANSPARENT));

            //Set the Alpha on the Whole Window to 255 (solid)
            SetLayeredWindowAttributes(this.Handle, 0, 255, LWA_ALPHA);

            //Init DirectX
            //This initializes the DirectX device. It needs to be done once.
            //The alpha channel in the backbuffer is critical.
            PresentParameters presentParameters = new PresentParameters();
            presentParameters.Windowed = true;
            presentParameters.SwapEffect = SwapEffect.Discard;
            presentParameters.BackBufferFormat = Format.A8R8G8B8;


            this.device = new Device(0, DeviceType.Hardware, this.Handle,
            CreateFlags.HardwareVertexProcessing, presentParameters);

            drawLine = new D3D.Line(this.device);
            drawBoxLine = new D3D.Line(this.device);
            drawCircleLine = new D3D.Line(this.device);
            drawFilledBoxLine = new D3D.Line(this.device);
            drawTriLine = new D3D.Line(this.device);

            CenterX = (float)this.ClientSize.Width / 2;
            CenterY = (float)this.ClientSize.Height / 2;

            CenterrX = this.ClientSize.Width / 2;
            CenterrY = this.ClientSize.Height / 2;

            font = new D3D.Font(device, new System.Drawing.Font("Fixedsys Regular", 15, FontStyle.Bold));

            Thread dx = new Thread(new ThreadStart(this.dxThread));
            dx.IsBackground = true;
            dx.Start();


        }
        private void dxThread()
        {
            while (true)
            {
                // Place your update logic here
                device.Clear(ClearFlags.Target, Color.FromArgb(0, 0, 0, 0), 1.0f, 0);
                device.RenderState.ZBufferEnable = false;
                device.RenderState.Lighting = false;
                device.RenderState.CullMode = Cull.None;
                device.Transform.Projection = Matrix.OrthoOffCenterLH(0, this.Width, this.Height, 0, 0, 1);

                // Place your drawing logic here
                device.BeginScene();

                //DrawLine(CenterX + 15, CenterY + 15, CenterX + 3, CenterY + 3, 3, Color.FromArgb(100, 0, 104, 204));
                //DrawLine(CenterX - 15, CenterY + 15, CenterX - 3, CenterY + 3, 3, Color.FromArgb(100, 0, 104, 204));
                //DrawLine(CenterX + 15, CenterY - 15, CenterX + 3, CenterY - 3, 3, Color.FromArgb(100, 0, 104, 204));
                //DrawLine(CenterX - 15, CenterY - 15, CenterX - 3, CenterY - 3, 3, Color.FromArgb(100, 0, 104, 204));
                //DrawPoint(CenterX - 1, CenterY - 1, Color.Blue);

                DrawCircle(CenterrX, CenterrY, 10, 50, Color.Blue);
                //DrawBox(CenterrX, CenterrY, 120, 120, Color.Peru);
                //DrawLine(CenterrX, CenterrY, 500, 500, 2, red);
                //DrawBox(CenterrX, CenterrY, 200, 200, Color.Peru);
                //DrawTriangle(CenterrX, CenterrY, 250, 500, Color.Azure);
                //DrawTriangle(CenterrX - 20, CenterrY + 12, 290, 540, Color.Azure);
                
                // Draws 3D Cube
                DrawBox(CenterrX, CenterrY, 200, 200, Color.Azure);
                DrawBox(CenterrX + 100, CenterrY - 100, 200, 200, Color.Azure);
                DrawLine(CenterrX, CenterrY, CenterrX + 100, CenterrY - 100, 2, Color.Azure);
                DrawLine(CenterrX + 200, CenterrY, CenterrX + 300, CenterrY - 100, 2, Color.Azure);
                DrawLine(CenterrX + 200, CenterrY + 200, CenterrX + 300, CenterrY + 100, 2, Color.Azure);
                DrawLine(CenterrX, CenterrY + 200, CenterrX + 100, CenterrY + 100, 2, Color.Azure);

                device.EndScene();
                device.Present();
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            marg.Left = 0;
            marg.Top = 0;
            marg.Right = this.Width;
            marg.Bottom = this.Height;

            DwmExtendFrameIntoClientArea(this.Handle, ref marg);
        }

        // Method for Drawing text on screen
        public static void DrawFont(string text, Point position, Color color)
        {
            font.DrawText(null, text, new Point(position.X, position.Y), Color.Black);
            font.DrawText(null, text, position, color);
        }

        // Method for Drawing Lines
        public static void DrawLine(float x1, float y1, float x2, float y2, float w, Color Color)
        {
            drawLine.Width = w;
            drawLine.Antialias = false;
            drawLine.GlLines = true;

            Vector2[] vertices = 
            {
                new Vector2(x1, y1),
                new Vector2(x2, y2)
            };

            drawLine.Begin();
            drawLine.Draw(vertices, Color.ToArgb());           
            drawLine.End();
        }

        // Method for drawing Filled Boxes
        public static void DrawFilledBox(float x, float y, float w, float h, Color Color)
        {
            Vector2[] vLine = new Vector2[2];

            drawFilledBoxLine.Width = w;
            drawFilledBoxLine.GlLines = true;
            drawFilledBoxLine.Antialias = false;

            vLine[0].X = x + w / 2;
            vLine[0].Y = y;
            vLine[1].X = x + w / 2;
            vLine[1].Y = y + h;

            drawFilledBoxLine.Begin();
            drawFilledBoxLine.Draw(vLine, Color.ToArgb());
            drawFilledBoxLine.End();

            /*              Example             */
            /* DrawFilledBox(x , y, w, h, color); */
        }

        // Method for Drawing Crosshair Boxes
        public static void DrawCrosshairBox(float x, float y, float w, float h, float px, Color color)
        {
            DrawFilledBox(x, y + h, w, px, color);
            DrawFilledBox(x - px, y, px, h, color);
            DrawFilledBox(x, y - px, w, px, color);
            DrawFilledBox(x + w, y, px, h, color);

            /*              Example             */
            /* DrawCrosshairBox(x , y, w, h, px, color); */
        }

        // Method for drawing 2D Boxes
        public static void DrawBox(float x, float y, float w, float h, Color color)
        {
            Vector2[] vertices = 
            {
                new Vector2(x, y),
                new Vector2(x + w, y),
                new Vector2(x + w, y + h),
                new Vector2(x, y + h),
                new Vector2(x, y)
            };
            drawBoxLine.Begin();
            drawBoxLine.Draw(vertices, color);
            drawBoxLine.End();

            /*              Example             */
            /* DrawBox(x , y, w, h, color); */
        }
      
        // Method for Drawing Not perfect Circle
        public static void DrawCircle(float x, float y, float radius, Color color)
        {
            float PI = 3.14159265f;
            double t = 0; ;

            for (t = 0.0; t <= PI * 2; t += 0.1)
            {
                x = (float)(x - radius * Math.Cos(t));
                y = (float)(y - radius * Math.Sin(t));
                DrawPoint(x, y, color);
            }

            /*              Example              */
            /* DrawCircle(x Axis, y Axis, radius, Color); */
        }

        // Method for drawing a point on the screen
        public static void DrawPoint(float x, float y, Color color)
        {
            DrawFilledBox(x, y, 1, 1, color);

            /*           Example            */
            /* DrawPoint(x Axis, y Axis, Color */
        }

        // Method for drawing Perfect Circle
        private void DrawCircle(int X, int Y, int radius, int numSides, Color color)
        {
            Vector2[] Line = new Vector2[100];

            float Step = (float)(Math.PI * 2.0 / numSides);
            int Count = 0;
            for (float a = 0; a < Math.PI * 2.0; a += Step)
            {
                float X1 = (float)(radius * Math.Cos(a) + X);
                float Y1 = (float)(radius * Math.Sin(a) + Y);
                float X2 = (float)(radius * Math.Cos(a + Step) + X);
                float Y2 = (float)(radius * Math.Sin(a + Step) + Y);

                Line[Count].X = X1;
                Line[Count].Y = Y1;
                Line[Count + 1].X = X2;
                Line[Count + 1].Y = Y2;
                Count += 2;
            }
            drawCircleLine.Begin();
            drawCircleLine.Draw(Line, color);
            drawCircleLine.End();

            /*                    Example                         */
            /* DrawCircle(x Axis, y Axis, radius, numOfSides, color); */
        }

        // Method for drawing out a triangle
        public static void DrawTriangle(int x, int y, int w, int h, Color c)
        {

            Vector2[] vertices =
            {
                new Vector2(x, y),
                new Vector2(x + w, y),
                new Vector2(x + (w / 2), y - h),
                new Vector2(x, y)
            };
            drawBoxLine.Begin();
            drawBoxLine.Draw(vertices, c);
            drawBoxLine.End();

            /*                    Example                         */
            /* Triangle(x Axis, y Axis, base length, height, color); */
        }
    }
}

