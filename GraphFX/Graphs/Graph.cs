using GraphFX.Rendering;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphFX.Graphs
{
    public abstract class Graph
    {
        public List<GraphLine> Lines = new List<GraphLine>();

        public void AddLine(Vector3 p0, Vector3 p1, Color c)
        {
            Lines.Add(new GraphLine() { Position0 = p0, Position1 = p1, Colour0 = c, Colour1 = c });
        }

        private Vector3 LinePosition = Vector3.Zero;
        private Color LineColour = Color.Black;
        public void StartLine(Vector3 p, Color c)
        {
            LinePosition = p;
            LineColour = c;
        }
        public void ContinueLine(Vector3 p, Color c)
        {
            Lines.Add(new GraphLine() { Position0 = LinePosition, Position1 = p, Colour0 = LineColour, Colour1 = c });
            LinePosition = p;
            LineColour = c;
        }


        public abstract void Update(float elapsed);

    }


    public struct GraphLine
    {
        public Vector3 Position0;
        public Vector3 Position1;
        public Color Colour0;
        public Color Colour1;

        public VertexTypePC Vertex0 { get { return new VertexTypePC() { Position = Position0, Colour = (uint)Colour0.ToRgba() }; } }
        public VertexTypePC Vertex1 { get { return new VertexTypePC() { Position = Position1, Colour = (uint)Colour1.ToRgba() }; } }
    }

}
