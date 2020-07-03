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
        public double Speed = 0.15;
        public double Sr = 0.5; // zeta input real component
        public double T = 10000 * Math.PI; //14.134725142;// // zeta input imaginary component (time varying)
        public int N = 10001; // max sum size

        public Color Colour1 = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        public Color Colour2 = new Color(0.5f, 1.0f, 0.1f, 1.0f);
        public Color Colour3 = new Color(1.0f, 0.0f, 0.0f, 1.0f);


        public override void Update(float elapsed)
        {
            T -= Animate ? elapsed * Speed : 0;
            

            Lines.Clear();




            ////see Anthony Lander's work on zeta symmetry
            //var piot = Math.PI / T; ////beware of 0 T!
            //var twopiot = piot * 2.0;
            //var tau = 0.5 * ((1.0 / (Math.Exp(piot) - 1.0)) + (1.0 / (1.0 - Math.Exp(-piot)))); ////unfortunate name
            //var kap = 0.5 * ((1.0 / Math.Sqrt(Math.Exp(twopiot) - 1.0)) + (1.0 / Math.Sqrt(1.0 - Math.Exp(-twopiot))));
            var tau = T / Math.PI; ////seems equivalent to above? not exactly the same though - is this more accurate?
            var kap = Math.Sqrt(tau * 0.5); ////dexyguess


            var x = -Sr;
            var s = Complex.Zero;
            StartLine(Vector3.Zero, Color.White);
            for (var b = 1.0; b <= N; b++)
            {
                //// standard reimann zeta function representation
                var v = Complex.Pow(new Complex(b, 0), new Complex(x, T));
                s += v;

                //// expanded version
                //var c = Math.Pow(b, x);
                //var ylogb = y * Math.Log(b);
                //var cos = c * Math.Cos(ylogb);
                //var sin = c * Math.Sin(ylogb);
                //var v = new Complex(cos, sin);
                //s += v;

                var colour = (b < kap) ? Colour1 : (b < tau) ? Colour2 : Colour3;

                var p = new Vector3((float)s.Real, (float)s.Imaginary, 0.0f); //main step graph output
                //var p = new Vector3((float)v.Real, (float)v.Imaginary, 0.0f); //step delta output
                ContinueLine(p, colour);
            }

            //s = Complex.Zero;
            //StartLine(Vector3.Zero, Color.White);
            //for (var b = 1.0; b <= N; b++)
            //{
            //    //// standard reimann zeta function representation, negative side
            //    var v = Complex.Pow(new Complex(b, 0), new Complex(x, -T));
            //    s += v;
            //    var colour = (b < kap) ? Colour1 : (b < tau) ? Colour2 : Colour3;
            //    var p = new Vector3((float)s.Real, (float)s.Imaginary, 0.0f); //main step graph output
            //    //var p = new Vector3((float)v.Real, (float)v.Imaginary, 0.0f); //step delta output
            //    ContinueLine(p, colour);
            //}



            var subdiv = 0.5;
            var subdivrt = Math.Sqrt(subdiv);

            s = Complex.Zero;
            StartLine(Vector3.Zero, Color.White);
            for (int i = 1; i <= N; i++)
            {
                var b = i * subdiv;

                var c = Math.Pow(b, x);
                var logb = Math.Log(b);
                var ylogb = T * logb;
                var cos = c * Math.Cos(ylogb);
                var sin = c * Math.Sin(ylogb);
                var v = new Complex(cos, sin) * subdivrt;

                //var dcos = sin / logb;
                //var dsin = cos / logb;
                //var v = new Complex(dcos, dsin);
                s += v;

                var colour = (b < kap) ? Colour1 : (b < tau) ? Colour2 : Colour3;

                var p = new Vector3((float)s.Real, (float)s.Imaginary, 0.0f); //main graph output
                //var p = new Vector3((float)v.Real, (float)v.Imaginary, 0.0f); //smooth step delta output
                //ContinueLine(p, colour);
            }



        }
    }
}
