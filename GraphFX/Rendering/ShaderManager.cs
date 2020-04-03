using GraphFX.Rendering.DirectX;
using GraphFX.Rendering.Shaders;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphFX.Rendering
{
    public class ShaderManager
    {
        DXManager DXMan;
        Device Device;

        RasterizerState rsSolid;
        RasterizerState rsWireframe;
        RasterizerState rsSolidDblSided;
        RasterizerState rsWireframeDblSided;
        BlendState bsDefault;
        BlendState bsAlpha;
        BlendState bsAdd;
        DepthStencilState dsEnabled;
        DepthStencilState dsDisableAll;
        DepthStencilState dsDisableComp;
        DepthStencilState dsDisableWrite;
        DepthStencilState dsDisableWriteRev;



        public LineShader Lines { get; set; }


        int Width;
        int Height;

        bool disposed = false;

        public double CurrentRealTime = 0;
        public float CurrentElapsedTime = 0;




        public ShaderManager(Device device, DXManager dxman)
        {
            Device = device;
            DXMan = dxman;

            Lines = new LineShader(device);


            RasterizerStateDescription rsd = new RasterizerStateDescription()
            {
                CullMode = CullMode.Back,
                DepthBias = 0,
                DepthBiasClamp = 0.0f,
                FillMode = FillMode.Solid,
                IsAntialiasedLineEnabled = true,
                IsDepthClipEnabled = true,
                IsFrontCounterClockwise = true,
                IsMultisampleEnabled = true,
                IsScissorEnabled = false,
                SlopeScaledDepthBias = 0.0f
            };
            rsSolid = new RasterizerState(device, rsd);
            rsd.FillMode = FillMode.Wireframe;
            rsWireframe = new RasterizerState(device, rsd);
            rsd.CullMode = CullMode.None;
            rsWireframeDblSided = new RasterizerState(device, rsd);
            rsd.FillMode = FillMode.Solid;
            rsSolidDblSided = new RasterizerState(device, rsd);


            BlendStateDescription bsd = new BlendStateDescription()
            {
                AlphaToCoverageEnable = false,//true,
                IndependentBlendEnable = false,
            };
            bsd.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
            bsd.RenderTarget[0].BlendOperation = BlendOperation.Add;
            bsd.RenderTarget[0].DestinationAlphaBlend = BlendOption.One;
            bsd.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
            bsd.RenderTarget[0].IsBlendEnabled = true;
            bsd.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            bsd.RenderTarget[0].SourceAlphaBlend = BlendOption.Zero;
            bsd.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
            bsd.RenderTarget[1] = bsd.RenderTarget[0];
            bsd.RenderTarget[2] = bsd.RenderTarget[0];
            bsd.RenderTarget[3] = bsd.RenderTarget[0];
            bsDefault = new BlendState(device, bsd);

            bsd.AlphaToCoverageEnable = true;
            bsAlpha = new BlendState(device, bsd);

            bsd.AlphaToCoverageEnable = false;
            bsd.RenderTarget[0].DestinationBlend = BlendOption.One;
            bsAdd = new BlendState(device, bsd);

            DepthStencilStateDescription dsd = new DepthStencilStateDescription()
            {
                BackFace = new DepthStencilOperationDescription()
                {
                    Comparison = Comparison.GreaterEqual,
                    DepthFailOperation = StencilOperation.Zero,
                    FailOperation = StencilOperation.Zero,
                    PassOperation = StencilOperation.Zero,
                },
                DepthComparison = Comparison.GreaterEqual,
                DepthWriteMask = DepthWriteMask.All,
                FrontFace = new DepthStencilOperationDescription()
                {
                    Comparison = Comparison.GreaterEqual,
                    DepthFailOperation = StencilOperation.Zero,
                    FailOperation = StencilOperation.Zero,
                    PassOperation = StencilOperation.Zero
                },
                IsDepthEnabled = true,
                IsStencilEnabled = false,
                StencilReadMask = 0,
                StencilWriteMask = 0
            };
            dsEnabled = new DepthStencilState(device, dsd);
            dsd.DepthWriteMask = DepthWriteMask.Zero;
            dsDisableWrite = new DepthStencilState(device, dsd);
            dsd.DepthComparison = Comparison.LessEqual;
            dsDisableWriteRev = new DepthStencilState(device, dsd);
            dsd.DepthComparison = Comparison.Always;
            dsDisableComp = new DepthStencilState(device, dsd);
            dsd.IsDepthEnabled = false;
            dsDisableAll = new DepthStencilState(device, dsd);
        }



        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            dsEnabled.Dispose();
            dsDisableWriteRev.Dispose();
            dsDisableWrite.Dispose();
            dsDisableComp.Dispose();
            dsDisableAll.Dispose();
            bsDefault.Dispose();
            bsAlpha.Dispose();
            bsAdd.Dispose();
            rsSolid.Dispose();
            rsWireframe.Dispose();

            Lines.Dispose();

        }



        public void BeginFrame(DeviceContext context, double currentRealTime, float elapsedTime)
        {
            CurrentRealTime = currentRealTime;
            CurrentElapsedTime = elapsedTime;


            DXMan.ClearRenderTarget(context);
            DXMan.SetDefaultRenderTarget(context);

            SetRasterizerMode(context, RasterizerMode.SolidDblSided);
            SetDepthStencilMode(context, DepthStencilMode.DisableAll);
            SetDefaultBlendState(context);
        }











        public void SetRasterizerMode(DeviceContext context, RasterizerMode mode)
        {
            switch (mode)
            {
                default:
                case RasterizerMode.Solid:
                    context.Rasterizer.State = rsSolid;
                    break;
                case RasterizerMode.Wireframe:
                    context.Rasterizer.State = rsWireframe;
                    break;
                case RasterizerMode.SolidDblSided:
                    context.Rasterizer.State = rsSolidDblSided;
                    break;
                case RasterizerMode.WireframeDblSided:
                    context.Rasterizer.State = rsWireframeDblSided;
                    break;
            }
        }
        public void SetDepthStencilMode(DeviceContext context, DepthStencilMode mode)
        {
            switch (mode)
            {
                default:
                case DepthStencilMode.Enabled:
                    context.OutputMerger.DepthStencilState = dsEnabled;
                    break;
                case DepthStencilMode.DisableWrite:
                    context.OutputMerger.DepthStencilState = dsDisableWrite;
                    break;
                case DepthStencilMode.DisableComp:
                    context.OutputMerger.DepthStencilState = dsDisableComp;
                    break;
                case DepthStencilMode.DisableAll:
                    context.OutputMerger.DepthStencilState = dsDisableAll;
                    break;
            }
        }
        public void SetDefaultBlendState(DeviceContext context)
        {
            context.OutputMerger.BlendState = bsDefault;
        }
        public void SetAlphaBlendState(DeviceContext context)
        {
            context.OutputMerger.BlendState = bsAlpha;
        }

        public void OnWindowResize(int w, int h)
        {
            Width = w;
            Height = h;
        }
    }

    public enum RasterizerMode
    {
        Solid = 1,
        Wireframe = 2,
        SolidDblSided = 3,
        WireframeDblSided = 4,
    }
    public enum DepthStencilMode
    {
        Enabled = 1,
        DisableWrite = 2,
        DisableComp = 3,
        DisableAll = 4,
    }
}
