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
        public Dictionary<string, GraphInOut> Inputs = new Dictionary<string, GraphInOut>();
        public Dictionary<string, GraphInOut> Outputs = new Dictionary<string, GraphInOut>();
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


    public class GraphInOut
    {
        public string Name;
        public GraphInOutType Type;
        public Vector3 Value;
        public Vector3 Min;//for slider input
        public Vector3 Max;//for slider input
        public bool Draw;//toggle output display

        public GraphInOut(string name, bool v)
        {
            Name = name;
            Type = GraphInOutType.Boolean;
            SetBoolValue(v);
        }
        public GraphInOut(string name, float v, float min = 0.0f, float max = 0.0f)
        {
            Name = name;
            Type = GraphInOutType.Scalar;
            Value = new Vector3(v);
            Min = new Vector3(min);
            Max = new Vector3(max);
        }
        public GraphInOut(string name, Vector2 v, Vector2 min = default(Vector2), Vector2 max = default(Vector2))
        {
            Name = name;
            Type= GraphInOutType.Vector2;
            Value = new Vector3(v, 0);
            Min = new Vector3(min, 0);
            Max = new Vector3(max, 0);
            Draw = true;
        }
        public GraphInOut(string name, Vector3 v, Vector3 min = default(Vector3), Vector3 max = default(Vector3))
        {
            Name = name;
            Type = GraphInOutType.Vector3;
            Value = v;
            Min = min;
            Max = max;
        }
        public GraphInOut(string name, int v, int min = 0, int max = 0)
        {
            Name = name;
            Type = GraphInOutType.Integer;
            SetIntValue(v);
            Min = new Vector3(min);
            Max = new Vector3(max);
        }

        public bool GetBoolValue()
        {
            return Value.X != 0.0f;
        }
        public void SetBoolValue(bool v)
        {
            Value.X = v ? 1.0f : 0.0f;
        }
        public int GetIntValue()
        {
            return (int)Value.X;//todo: improve this..?
        }
        public void SetIntValue(int v)
        {
            Value.X = (float)v;//todo: improve this..?
        }
        public float GetFloatValue()
        {
            return Value.X;
        }
        public void SetFloatValue(float v)
        {
            Value.X = v;
        }
        public Vector2 GetVector2Value()
        {
            return new Vector2(Value.X, Value.Y);
        }
        public void SetVector2Value(Vector2 v)
        {
            Value.X = v.X;
            Value.Y = v.Y;
        }
        public Vector3 GetVector3Value()
        {
            return Value;
        }
        public void SetVector3Value(Vector3 v)
        {
            Value = v;
        }
    }

    public enum GraphInOutType
    {
        Boolean = 0,
        Scalar = 1,
        Vector2 = 2,
        Vector3 = 3,
        Vector4 = 4,//future?
        Integer = 5,
    }


}
