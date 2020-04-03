using GraphFX.Rendering.Utils;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Device = SharpDX.Direct3D11.Device;

namespace GraphFX.Rendering.Shaders
{

    public struct LineShaderVSSceneVars
    {
        public Matrix ViewProj;
        public Vector4 CameraPos;
    }

    public class LineShader : Shader, IDisposable
    {
        bool disposed = false;

        VertexShader vs;
        PixelShader ps;
        InputLayout layout;

        GpuVarsBuffer<LineShaderVSSceneVars> VSSceneVars;

        GpuCBuffer<VertexTypePC> vertices;


        public LineShader(Device device)
        {
            byte[] vsbytes = File.ReadAllBytes("Shaders\\LineVS.cso");
            byte[] psbytes = File.ReadAllBytes("Shaders\\LinePS.cso");


            vs = new VertexShader(device, vsbytes);
            ps = new PixelShader(device, psbytes);

            VSSceneVars = new GpuVarsBuffer<LineShaderVSSceneVars>(device);

            layout = VertexTypePC.GetLayout(device, vsbytes);


            vertices = new GpuCBuffer<VertexTypePC>(device, 1000); //should be more than needed....
        }


        public override void SetShader(DeviceContext context)
        {
            context.VertexShader.Set(vs);
            //context.InputAssembler.SetVertexBuffers(0, null);
            context.InputAssembler.SetIndexBuffer(null, Format.Unknown, 0);
            context.PixelShader.Set(ps);
        }

        public override bool SetInputLayout(DeviceContext context)
        {
            context.InputAssembler.InputLayout = layout;
            return true;
        }

        public override void SetSceneVars(DeviceContext context, Camera camera)
        {
            VSSceneVars.Vars.ViewProj = Matrix.Transpose(camera.ViewProjMatrix);
            VSSceneVars.Vars.CameraPos = new Vector4(camera.Position, 0.0f);
            VSSceneVars.Update(context);
            VSSceneVars.SetVSCBuffer(context, 0);
        }


        public void RenderLines(DeviceContext context, List<VertexTypePC> verts, Camera camera)
        {
            SetShader(context);
            SetInputLayout(context);
            SetSceneVars(context, camera);

            int drawn = 0;
            int linecount = verts.Count / 2;
            int maxcount = vertices.StructCount / 2;
            while (drawn < linecount)
            {
                vertices.Clear();

                int offset = drawn * 2;
                int bcount = Math.Min(linecount - drawn, maxcount);
                for (int i = 0; i < bcount; i++)
                {
                    int t = offset + (i * 2);
                    vertices.Add(verts[t + 0]);
                    vertices.Add(verts[t + 1]);
                }
                drawn += bcount;

                vertices.Update(context);
                vertices.SetVSResource(context, 0);

                context.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;
                context.Draw(vertices.CurrentCount, 0);
            }
        }


        public override void UnbindResources(DeviceContext context)
        {
            context.VertexShader.SetConstantBuffer(0, null);
            context.VertexShader.SetShaderResource(0, null);
            context.VertexShader.Set(null);
            context.PixelShader.Set(null);
        }



        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            VSSceneVars.Dispose();
            vertices.Dispose();
            layout.Dispose();
            ps.Dispose();
            vs.Dispose();
        }


    }
}
