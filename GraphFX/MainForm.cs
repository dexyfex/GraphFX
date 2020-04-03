using GraphFX.Graphs;
using GraphFX.Maths;
using GraphFX.Rendering;
using GraphFX.Rendering.DirectX;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Color = SharpDX.Color;

namespace GraphFX
{
    public partial class MainForm : Form, DXForm
    {
        public Form Form { get { return this; } } //for DXForm/DXManager use
        
        public Renderer Renderer = null;
        public object RenderSyncRoot { get { return Renderer.RenderSyncRoot; } }

        volatile bool running = false;
        volatile bool pauserendering = false;

        Stopwatch frametimer = new Stopwatch();
        Camera camera;

        bool Is2DView = true;
        int View2DDragX = 0;
        int View2DDragY = 0;

        bool MouseLButtonDown = false;
        bool MouseRButtonDown = false;
        int MouseX;
        int MouseY;
        System.Drawing.Point MouseDownPoint;
        System.Drawing.Point MouseLastPoint;


        bool initedOk = false;



        //bool toolsPanelResizing = false;
        //int toolsPanelResizeStartX = 0;
        //int toolsPanelResizeStartLeft = 0;
        //int toolsPanelResizeStartRight = 0;


        bool enableGrid = true;
        float gridSize = 1.0f;
        int gridCount = 40;
        List<VertexTypePC> gridVerts = new List<VertexTypePC>();
        object gridSyncRoot = new object();


        Graph Graph = new ZetaGraph();





        public MainForm()
        {
            InitializeComponent();

            Renderer = new Renderer(this);
            camera = Renderer.camera;

            initedOk = Renderer.Init();

        }


        public void InitScene(Device device)
        {
            int width = ClientSize.Width;
            int height = ClientSize.Height;

            try
            {
                Renderer.DeviceCreated(device, width, height);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading shaders!\n" + ex.ToString());
                return;
            }


            camera.Is2DView = true;
            camera.FollowPosition = Vector3.Zero;
            camera.TargetDistance = 2.0f;
            camera.CurrentDistance = 2.0f;
            camera.TargetRotation.Y = 0.0f;// 0.2f;
            camera.CurrentRotation.Y = 0.0f;// 0.2f;
            camera.TargetRotation.X = 0.0f;// 0.5f * (float)Math.PI;
            camera.CurrentRotation.X = 0.0f;// 0.5f * (float)Math.PI;


            //new Thread(new ThreadStart(ContentThread)).Start();

            frametimer.Start();
        }
        public void CleanupScene()
        {

            Renderer.DeviceDestroyed();

            int count = 0;
            while (running && (count < 5000)) //wait for the content thread to exit gracefully
            {
                Thread.Sleep(1);
                count++;
            }
        }
        public void RenderScene(DeviceContext context)
        {
            float elapsed = (float)frametimer.Elapsed.TotalSeconds;
            frametimer.Restart();

            if (pauserendering) return;

            if (!Monitor.TryEnter(Renderer.RenderSyncRoot, 50))
            { return; } //couldn't get a lock, try again next time

            UpdateControlInputs(elapsed);

            Renderer.Update(elapsed, MouseLastPoint.X, MouseLastPoint.Y);
            Renderer.BeginRender(context);

            RenderGrid(context);
            RenderGraph(context, elapsed);

            Renderer.RenderGeometry();


            Renderer.EndRender();

            Monitor.Exit(Renderer.RenderSyncRoot);

        }
        public void BuffersResized(int w, int h)
        {
            Renderer.BuffersResized(w, h);
        }
        public bool ConfirmQuit()
        {
            return true;
        }

        private void Init()
        {
            //called from MainForm_Load

            if (!initedOk)
            {
                Close();
                return;
            }


            MouseWheel += MainForm_MouseWheel;




            UpdateGridVerts();
            //GridSizeComboBox.SelectedIndex = 1;
            //GridCountComboBox.SelectedIndex = 1;



            Renderer.Start();
        }



        private void UpdateControlInputs(float elapsed)
        {
            if (elapsed > 0.1f) elapsed = 0.1f;

            float moveSpeed = 50.0f;


            Vector3 movevec = Vector3.Zero;

            if (Is2DView)
            {
                movevec *= elapsed * moveSpeed * Math.Min(camera.OrthographicTargetSize * 0.01f, 50.0f);
                float viewscale = 1.0f / camera.Height;
                float fdx = View2DDragX * viewscale;
                float fdy = View2DDragY * viewscale;
                movevec.X -= fdx * camera.OrthographicSize;
                movevec.Y += fdy * camera.OrthographicSize;
            }
            else
            {
                movevec *= elapsed * moveSpeed * Math.Min(camera.TargetDistance, 20.0f);
            }


            Vector3 movewvec = camera.ViewInvQuaternion.Multiply(movevec);
            camera.FollowPosition += movewvec;

            View2DDragX = 0;
            View2DDragY = 0;

        }

        private void UpdateMousePosition(MouseEventArgs e)
        {
            MouseX = e.X;
            MouseY = e.Y;
            MouseLastPoint = e.Location;
        }

        private void RotateCam(int dx, int dy)
        {
            if (Is2DView == false)
            {
                camera.MouseRotate(dx, dy);
            }
            else
            {
                //need to move the camera XY with mouse in 2D view mode...
                View2DDragX += dx;
                View2DDragY += dy;
            }
        }

        private void MoveCameraToView(Vector3 pos, float rad)
        {
            //move the camera to a default place where the given sphere is fully visible.

            rad = Math.Max(0.01f, rad);

            camera.FollowPosition = pos;
            camera.TargetDistance = rad * 1.6f;
            camera.CurrentDistance = rad * 1.6f;

            camera.UpdateProj = true;

        }



        private void UpdateGridVerts()
        {
            lock (gridSyncRoot)
            {
                gridVerts.Clear();

                float s = gridSize * gridCount * 0.5f;
                uint cblack = (uint)Color.Black.ToRgba();
                uint cgray = (uint)Color.Gray.ToRgba();
                uint cred = (uint)Color.Red.ToRgba();
                uint cgrn = (uint)Color.Green.ToRgba();
                int interval = 10;

                for (int i = 0; i <= gridCount; i++)
                {
                    float o = (gridSize * i) - s;
                    if ((i % interval) != 0)
                    {
                        gridVerts.Add(new VertexTypePC() { Position = new Vector3(o, -s, 0), Colour = cgray });
                        gridVerts.Add(new VertexTypePC() { Position = new Vector3(o, s, 0), Colour = cgray });
                        gridVerts.Add(new VertexTypePC() { Position = new Vector3(-s, o, 0), Colour = cgray });
                        gridVerts.Add(new VertexTypePC() { Position = new Vector3(s, o, 0), Colour = cgray });
                    }
                }
                for (int i = 0; i <= gridCount; i++) //draw main lines last, so they are on top
                {
                    float o = (gridSize * i) - s;
                    if ((i % interval) == 0)
                    {
                        var cx = (o == 0) ? cred : cblack;
                        var cy = (o == 0) ? cgrn : cblack;
                        gridVerts.Add(new VertexTypePC() { Position = new Vector3(o, -s, 0), Colour = cy });
                        gridVerts.Add(new VertexTypePC() { Position = new Vector3(o, s, 0), Colour = cy });
                        gridVerts.Add(new VertexTypePC() { Position = new Vector3(-s, o, 0), Colour = cx });
                        gridVerts.Add(new VertexTypePC() { Position = new Vector3(s, o, 0), Colour = cx });
                    }
                }

            }
        }

        private void RenderGrid(DeviceContext context)
        {
            if (!enableGrid) return;

            lock (gridSyncRoot)
            {
                if (gridVerts.Count > 0)
                {
                    Renderer.RenderLines(gridVerts);
                }
            }
        }

        private void RenderGraph(DeviceContext context, float elapsed)
        {
            if (Graph == null) return;

            Graph.Update(elapsed);

            foreach (var line in Graph.Lines)
            {
                Renderer.LineVerts.Add(line.Vertex0);
                Renderer.LineVerts.Add(line.Vertex1);
            }

        }




        private void MainForm_Load(object sender, EventArgs e)
        {
            Init();
        }

        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left: MouseLButtonDown = true; break;
                case MouseButtons.Right: MouseRButtonDown = true; break;
            }

            //if (!ToolsPanelShowButton.Focused)
            //{
            //    ToolsPanelShowButton.Focus(); //make sure no textboxes etc are focused!
            //}

            MouseDownPoint = e.Location;
            MouseLastPoint = MouseDownPoint;

            if (MouseLButtonDown)
            {
            }

            if (MouseRButtonDown)
            {
                //SelectMousedItem();
            }

            MouseX = e.X; //to stop jumps happening on mousedown, sometimes the last MouseMove event was somewhere else... (eg after clicked a menu)
            MouseY = e.Y;
        }

        private void MainForm_MouseUp(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left: MouseLButtonDown = false; break;
                case MouseButtons.Right: MouseRButtonDown = false; break;
            }

            if (e.Button == MouseButtons.Left)
            {
            }
        }

        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            int dx = e.X - MouseX;
            int dy = e.Y - MouseY;

            //if (MouseInvert)
            //{
            //    dy = -dy;
            //}

            if (MouseLButtonDown)
            {
                RotateCam(dx, dy);
            }
            if (MouseRButtonDown)
            {
            }

            UpdateMousePosition(e);



        }

        private void MainForm_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta != 0)
            {
                camera.MouseZoom(e.Delta);
            }
        }
    }
}
