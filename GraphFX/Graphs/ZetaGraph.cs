using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Vector3 = SharpDX.Vector3;

namespace GraphFX.Graphs
{
    public class ZetaGraph : Graph
    {
        public bool Animate = true;
        public double Speed = 0.05;
        public double T = 58510.067670379; // time varying input
        public double Sr = 0.5; // zeta input real component
        public int N = 10000; // sum size

        public Color Colour = new Color(1.0f, 1.0f, 1.0f, 1.0f);


        public override void Update(float elapsed)
        {
            if (Animate)
            {
                T += elapsed * Speed;
            }

            Lines.Clear();



            var x = -Sr;
            var y = T;
            var s = new Complex(1.0, 0.0);
            var p0 = new Vector3((float)s.Real, (float)s.Imaginary, 0.0f);
            for (int i = 0; i < N; i++)
            {
                var b = (double)(i + 2);

                //// standard reimann zeta function representation
                s += Complex.Pow(new Complex(b, 0), new Complex(x, y));

                //// expanded version
                //var c = Math.Pow(b, x);
                //var ylogb = y * Math.Log(b);
                //var cos = Math.Cos(ylogb);
                //var sin = Math.Sin(ylogb);
                //var v = new Complex(cos * c, sin * c);
                //s += v;


                var p1 = new Vector3((float)s.Real, (float)s.Imaginary, 0.0f);
                AddLine(p0, p1, Colour);
                p0 = p1;
            }



        }
    }
}
