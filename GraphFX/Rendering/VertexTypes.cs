using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Device = SharpDX.Direct3D11.Device;

namespace GraphFX.Rendering
{

    public struct VertexTypePC
    {
        public Vector3 Position;
        public uint Colour;

        public static InputLayout GetLayout(Device device, byte[] vsbytes)
        {
            return new InputLayout(device, vsbytes, new[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                new InputElement("COLOR", 0, Format.R8G8B8A8_UNorm, 16, 0),
            });
        }
    }

}
