using GraphFX.Maths;
using GraphFX.Rendering.DirectX;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphFX.Rendering
{
    public class Renderer
    {
        private DXForm Form;

        public DXManager DXMan { get; } = new DXManager();
        public Device Device { get; private set; }

        public object RenderSyncRoot { get; } = new object();

        public ShaderManager shaders;

        public Camera camera;

        private double currentRealTime = 0;
        private float currentElapsedTime = 0;
        private int framecount = 0;
        private float fcelapsed = 0.0f;
        private int fps = 0;

        private DeviceContext context;




        //public List<MapBox> WhiteBoxes = new List<MapBox>();
        //public List<MapSphere> WhiteSpheres = new List<MapSphere>();
        public List<VertexTypePC> LineVerts = new List<VertexTypePC>();
        public List<VertexTypePC> TriVerts = new List<VertexTypePC>();





        public Renderer(DXForm form)
        {
            Form = form;

            camera = new Camera(10.0f, 0.005f, 1.0f);
        }


        public bool Init()
        {
            return DXMan.Init(Form, false);
        }

        public void Start()
        {
            DXMan.Start();
        }

        public void DeviceCreated(Device device, int width, int height)
        {
            Device = device;

            shaders = new ShaderManager(device, DXMan);
            shaders.OnWindowResize(width, height); //init the buffers

            camera.OnWindowResize(width, height); //init the projection stuff
        }

        public void DeviceDestroyed()
        {
            shaders.Dispose();

            Device = null;
        }

        public void BuffersResized(int width, int height)
        {
            lock (RenderSyncRoot)
            {
                camera.OnWindowResize(width, height);
                shaders.OnWindowResize(width, height);
            }
        }

        public void ReloadShaders()
        {
            if (shaders != null)
            {
                shaders.Dispose();
            }
            shaders = new ShaderManager(Device, DXMan);
        }


        public void Update(float elapsed, int mouseX, int mouseY)
        {
            framecount++;
            fcelapsed += elapsed;
            if (fcelapsed >= 0.5f)
            {
                fps = framecount * 2;
                framecount = 0;
                fcelapsed -= 0.5f;
            }
            if (elapsed > 0.1f) elapsed = 0.1f;
            currentRealTime += elapsed;
            currentElapsedTime = elapsed;





            camera.SetMousePosition(mouseX, mouseY);

            camera.Update(elapsed);
        }


        public void BeginRender(DeviceContext ctx)
        {
            context = ctx;


            shaders.BeginFrame(context, currentRealTime, currentElapsedTime);





            LineVerts.Clear();
            TriVerts.Clear();
            //WhiteBoxes.Clear();
            //WhiteSpheres.Clear();
        }




        public void EndRender()
        {
        }






        public void SetCameraMode(string modestr)
        {
            lock (RenderSyncRoot)
            {
                switch (modestr)
                {
                    case "Perspective":
                        camera.IsOrthographic = false;
                        camera.Is2DView = false;
                        break;
                    case "Orthographic":
                        camera.IsOrthographic = true;
                        camera.Is2DView = false;
                        break;
                    case "2D":
                        camera.IsOrthographic = true;
                        camera.Is2DView = true;
                        break;
                }
                camera.UpdateProj = true;
            }
        }


        public string GetStatusText()
        {
            return string.Format("Fps: {0}", fps);
        }












        public void RenderArrowLines3D(Vector3 pos, Vector3 dir, Vector3 up, Quaternion ori, float len, float rad, uint colour)
        {
            Vector3 ax = Vector3.Cross(dir, up);
            Vector3 sx = ax * rad;
            Vector3 sy = up * rad;
            Vector3 sz = dir * len;
            VertexTypePC[] c = new VertexTypePC[8];
            Vector3 d0 = -sx - sy;
            Vector3 d1 = -sx + sy;
            Vector3 d2 = +sx - sy;
            Vector3 d3 = +sx + sy;
            c[0].Position = d0;
            c[1].Position = d1;
            c[2].Position = d2;
            c[3].Position = d3;
            c[4].Position = d0 + sz;
            c[5].Position = d1 + sz;
            c[6].Position = d2 + sz;
            c[7].Position = d3 + sz;
            for (int i = 0; i < 8; i++)
            {
                c[i].Colour = colour;
                c[i].Position = pos + ori.Multiply(c[i].Position);
            }

            LineVerts.Add(c[0]);
            LineVerts.Add(c[1]);
            LineVerts.Add(c[1]);
            LineVerts.Add(c[3]);
            LineVerts.Add(c[3]);
            LineVerts.Add(c[2]);
            LineVerts.Add(c[2]);
            LineVerts.Add(c[0]);
            LineVerts.Add(c[4]);
            LineVerts.Add(c[5]);
            LineVerts.Add(c[5]);
            LineVerts.Add(c[7]);
            LineVerts.Add(c[7]);
            LineVerts.Add(c[6]);
            LineVerts.Add(c[6]);
            LineVerts.Add(c[4]);
            LineVerts.Add(c[0]);
            LineVerts.Add(c[4]);
            LineVerts.Add(c[1]);
            LineVerts.Add(c[5]);
            LineVerts.Add(c[2]);
            LineVerts.Add(c[6]);
            LineVerts.Add(c[3]);
            LineVerts.Add(c[7]);

            c[0].Position = pos + ori.Multiply(dir * (len + rad * 5.0f));
            c[4].Position += ori.Multiply(d0);
            c[5].Position += ori.Multiply(d1);
            c[6].Position += ori.Multiply(d2);
            c[7].Position += ori.Multiply(d3);
            LineVerts.Add(c[4]);
            LineVerts.Add(c[5]);
            LineVerts.Add(c[5]);
            LineVerts.Add(c[7]);
            LineVerts.Add(c[7]);
            LineVerts.Add(c[6]);
            LineVerts.Add(c[6]);
            LineVerts.Add(c[4]);
            LineVerts.Add(c[0]);
            LineVerts.Add(c[4]);
            LineVerts.Add(c[0]);
            LineVerts.Add(c[5]);
            LineVerts.Add(c[0]);
            LineVerts.Add(c[6]);
            LineVerts.Add(c[0]);
            LineVerts.Add(c[7]);


        }

        public void RenderCircle2D(Vector3 position, float radius, uint col)
        {
            const int Reso = 36;
            const float MaxDeg = 360f;
            const float DegToRad = 0.0174533f;
            const float Ang = DegToRad * MaxDeg / Reso;

            var dir = Vector3.UnitZ;// .Normalize(position - camera.Position);
            var up = Vector3.Normalize(dir.GetPerpVec());
            var axis = Vector3.Cross(dir, up);
            var c = new VertexTypePC[Reso];

            for (var i = 0; i < Reso; i++)
            {
                var rDir = Quaternion.RotationAxis(dir, i * Ang).Multiply(axis);
                c[i].Position = position + (rDir * radius);
                c[i].Colour = col;
            }

            for (var i = 0; i < c.Length; i++)
            {
                LineVerts.Add(c[i]);
                LineVerts.Add(c[(i + 1) % c.Length]);
            }
        }
        public void RenderCircle3D(Vector3 position, float radius, uint col)
        {
            const int Reso = 36;
            const float MaxDeg = 360f;
            const float DegToRad = 0.0174533f;
            const float Ang = DegToRad * MaxDeg / Reso;

            var dir = Vector3.Normalize(position - camera.Position);
            var up = Vector3.Normalize(dir.GetPerpVec());
            var axis = Vector3.Cross(dir, up);
            var c = new VertexTypePC[Reso];

            for (var i = 0; i < Reso; i++)
            {
                var rDir = Quaternion.RotationAxis(dir, i * Ang).Multiply(axis);
                c[i].Position = position + (rDir * radius);
                c[i].Colour = col;
            }

            for (var i = 0; i < c.Length; i++)
            {
                LineVerts.Add(c[i]);
                LineVerts.Add(c[(i + 1) % c.Length]);
            }
        }

        public void RenderCubeLines(Vector3 p1, Vector3 p2, Vector3 a2, Vector3 a3, uint col)
        {
            VertexTypePC v = new VertexTypePC();
            v.Colour = col;
            var c1 = p1 - a2 - a3;
            var c2 = p1 - a2 + a3;
            var c3 = p1 + a2 + a3;
            var c4 = p1 + a2 - a3;
            var c5 = p2 - a2 - a3;
            var c6 = p2 - a2 + a3;
            var c7 = p2 + a2 + a3;
            var c8 = p2 + a2 - a3;
            v.Position = c1; LineVerts.Add(v);
            v.Position = c2; LineVerts.Add(v); LineVerts.Add(v);
            v.Position = c3; LineVerts.Add(v); LineVerts.Add(v);
            v.Position = c4; LineVerts.Add(v); LineVerts.Add(v);
            v.Position = c1; LineVerts.Add(v); LineVerts.Add(v);
            v.Position = c5; LineVerts.Add(v);
            v.Position = c2; LineVerts.Add(v);
            v.Position = c6; LineVerts.Add(v);
            v.Position = c3; LineVerts.Add(v);
            v.Position = c7; LineVerts.Add(v);
            v.Position = c4; LineVerts.Add(v);
            v.Position = c8; LineVerts.Add(v);
            v.Position = c5; LineVerts.Add(v);
            v.Position = c6; LineVerts.Add(v); LineVerts.Add(v);
            v.Position = c7; LineVerts.Add(v); LineVerts.Add(v);
            v.Position = c8; LineVerts.Add(v); LineVerts.Add(v);
            v.Position = c5; LineVerts.Add(v);
        }




        public void RenderGeometry(DepthStencilMode dsmode = DepthStencilMode.DisableAll)
        {
            shaders.SetRasterizerMode(context, RasterizerMode.Solid);
            shaders.SetDepthStencilMode(context, dsmode);
            shaders.SetDefaultBlendState(context);

            if (LineVerts.Count > 0)
            {
                shaders.Lines.RenderLines(context, LineVerts, camera);
            }
            if (TriVerts.Count > 0)
            {
                //shaders.Lines.RenderTriangles(context, TriVerts, camera);
            }



        }

        public void RenderLines(List<VertexTypePC> linelist, DepthStencilMode dsmode = DepthStencilMode.DisableAll)
        {
            shaders.SetRasterizerMode(context, RasterizerMode.Solid);
            shaders.SetDepthStencilMode(context, dsmode);
            shaders.SetDefaultBlendState(context);
            shaders.Lines.RenderLines(context, linelist, camera);
        }





    }
}
