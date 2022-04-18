using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Vector2 = SharpDX.Vector2;
using Vector3 = SharpDX.Vector3;

namespace GraphFX.Graphs
{
    public class ZetaGraph : Graph
    {
        public double Sr = 0.5; // zeta input real component
        public double T = 0; //14.134725142; //1000 * Math.PI; // // zeta input imaginary component (time varying)
        public Complex Z = 0; //zeta function result
        public double Tau = 0.0; //updated per frame: approx number of iterations until diverging sequence
        public double Kap = 0.0; //updated per frame: approx number of iterations until reflection point
        public int Steps = 10000; //current number of sum graph steps to plot 

        public Color Colour1 = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        public Color Colour2 = new Color(0.5f, 1.0f, 0.1f, 1.0f);
        public Color Colour3 = new Color(1.0f, 0.0f, 0.0f, 1.0f);

        public Queue<Vector3> ZetaPlotHistory = new Queue<Vector3>(1000);


        public ZetaGraph()
        {
            Inputs["Default"] = new GraphInOut("Zeta Input (scaled and offset)", Vector2.Zero, Vector2.One * -10.0f, Vector2.One * 10.0f);
            Inputs["TrailEnable"] = new GraphInOut("Enable Trail", true);
            Inputs["TrailLength"] = new GraphInOut("Trail Length", 1000, 0, 10000);
            Inputs["Animate"] = new GraphInOut("Animate", true);
            Inputs["Speed"] = new GraphInOut("Speed", 1.0f, 0.0f, 10.0f);
            Inputs["Steps"] = new GraphInOut("Steps", 10000, 10, 100000); // max sum size
            Inputs["StepsTau"] = new GraphInOut("Limit Steps to Tau", true);
            Outputs["ZetaValue"] = new GraphInOut("Zeta Value", Vector2.Zero);
            Outputs["Tau"] = new GraphInOut("Tau Value", 0.0f);
            Outputs["Kap"] = new GraphInOut("Kap Value", 0.0f);
            Outputs["KapApprox"] = new GraphInOut("Approximate Kap Zeta Value", Vector2.Zero);
        }

        public override void Update(float elapsed)
        {
            if (elapsed > 0.02f) elapsed = 0.02f;

            Lines.Clear();

            var zetain = Inputs["Default"].Value;
            var anim = Inputs["Animate"].GetBoolValue();
            var speed = Inputs["Speed"].GetFloatValue();


            Sr = zetain.X * 0.1 + 0.5;// (anim ? 0.5 : 0.5);
            T = (anim ? T + elapsed * speed * zetain.Y : zetain.Y);
            

            UpdateTauKap();


            Steps = Inputs["Steps"].GetIntValue();
            if (Inputs["StepsTau"].GetBoolValue() == true)
            {
                Steps = (int)Tau + 4;
            }


            ZetaValuePlot();
            ZetaSumStepGraph();
            //ZetaSumStepGraphExpanded();
            ZetaSumApprox();
        }


        private void UpdateTauKap()
        {

            ////see Anthony Lander's work on zeta symmetry
            //var piot = Math.PI / T; ////beware of 0 T!
            //var twopiot = piot * 2.0;
            //Tau = 0.5 * ((1.0 / (Math.Exp(piot) - 1.0)) + (1.0 / (1.0 - Math.Exp(-piot)))); ////unfortunate name
            //Kap = 0.5 * ((1.0 / Math.Sqrt(Math.Exp(twopiot) - 1.0)) + (1.0 / Math.Sqrt(1.0 - Math.Exp(-twopiot))));
            Tau = Math.Abs(T) / Math.PI; ////seems equivalent to above? not exactly the same though - is this more accurate? -dexy
            Kap = Math.Sqrt(Tau * 0.5); ////riemann-siegel value
            Kap = Kap - 0.5;//dexyfex adjustment - to match function centerline

            Outputs["Tau"].SetFloatValue((float)Tau);
            Outputs["Kap"].SetFloatValue((float)Kap);
            Outputs["KapApprox"].Draw = false;
        }

        public void ZetaValuePlot()
        {
            var z = Zeta(new Complex(Sr, T));
            var zp = new Vector3((float)z.Real, (float)z.Imaginary, 0.0f); //zeta function output (final result)
            var c = Color.Silver;
            c.A = 127;
            StartLine(Vector3.Zero, c);
            ContinueLine(zp, c);

            Z = z;
            Outputs["ZetaValue"].Value = zp;

            if (Inputs["TrailEnable"].GetBoolValue() == true)
            {
                AddHistory(ZetaPlotHistory, zp);

                var started = false;
                c = Color.SkyBlue;
                c.A = 0;
                float a = 0.0f;
                var incr = 1.0f / ZetaPlotHistory.Count;
                foreach (var hp in ZetaPlotHistory)
                {
                    if (!started) { StartLine(hp, c); started = true; }
                    else ContinueLine(hp, c);
                    a += incr;
                    a = Math.Min(a, 1.0f);
                    c.A = (byte)(a * 255.0f);
                }
            }
        }
        public void ZetaSumStepGraph()
        {
            //plot the progress of the zeta function sum through N steps
            var s = Complex.Zero;
            StartLine(Vector3.Zero, Color.White);
            for (var b = 1.0; b <= Steps; b++)
            {
                var v = Complex.Pow(new Complex(b, 0), new Complex(-Sr, T)); //// standard reimann zeta function representation
                s += v;
                var p = new Vector3((float)s.Real, (float)s.Imaginary, 0.0f); //main step graph output
                //var p = new Vector3((float)v.Real, (float)v.Imaginary, 0.0f); //step delta output
                ContinueLine(p, (b < Kap) ? Colour1 : (b < Tau) ? Colour2 : Colour3);
            }
        }
        public void ZetaSumStepGraphExpanded()
        {
            var subdiv = 1.0;// //experiment with trying to get more "resolution" - zeroes still the same?
            var subdivrt = Math.Sqrt(subdiv);
            var s = Complex.Zero;
            StartLine(Vector3.Zero, Color.White);
            var steps = Steps * subdiv;
            for (var b = subdiv; b <= steps; b += subdiv)
            {
                //// expanded geometric version - useful?
                var mag = Math.Pow(b, -Sr) * subdivrt; //subdiv;//
                var pha = Math.Log(b) * T;
                var cos = mag * Math.Cos(pha);
                var sin = mag * Math.Sin(pha);
                var v = new Complex(cos, sin);
                s += v;
                var p = new Vector3((float)s.Real, (float)s.Imaginary, 0.0f); //main graph output
                //var p = new Vector3((float)v.Real, (float)v.Imaginary, 0.0f); //smooth step delta output
                ContinueLine(p, (b < Kap*subdiv) ? Colour1 : (b < Tau*subdiv) ? Colour2 : Colour3);
            }

        }
        public void ZetaSumApprox()
        {
            //dexytest
            var inp = new Complex(-Sr, T);
            var s = Complex.Zero;
            var maxb = Math.Floor(Kap);
            for (var b = 1.0; b <= maxb; b++)
            {
                var v = Complex.Pow(new Complex(b, 0), inp); //// standard reimann zeta function representation
                s += v;
            }

            var kfrac = Kap - maxb;
            var kfraci = 1.0 - kfrac;
            var vk = Complex.Pow(new Complex(Kap, 0), inp);
            var vb = Complex.Pow(new Complex(maxb, 0), inp);
            var v1 = Complex.Pow(new Complex(maxb + 1.0, 0), inp);
            var v2 = Complex.Pow(new Complex(maxb + 2.0, 0), inp);
            var p = vb.Phase + v1.Phase;
            //var p = vb.Phase + v2.Phase;
            //var p = vb.Phase + vb.Phase * kfraci + v1.Phase * kfrac;
            //var p = vb.Phase + (vb*kfraci+v1*kfrac).Phase /*+ (kfrac * Math.PI) + Math.PI*0.5*/;
            //var p = vb.Phase + (kfrac * v2.Phase + kfraci * v1.Phase);
            var ss = s;
            s = s + v1*kfrac;// + vb*0.1;// *2.0;// (s - sx);
            //s = s + vk*kfrac;
            //s = s - vk*kfraci;

            Outputs["KapApprox"].SetVector2Value(new Vector2((float)s.Real, (float)s.Imaginary));
            Outputs["KapApprox"].Draw = true;


            var s2 = s + vk * 1.0; //Complex.FromPolarCoordinates(1.0, Math.Log(Kap) * T); //vk indicator red line
            StartLine(new Vector3((float)s.Real, (float)s.Imaginary, 0.0f), Color.Red);
            ContinueLine(new Vector3((float)s2.Real, (float)s2.Imaginary, 0.0f), Color.Red);




            StartLine(new Vector3((float)s.Real, (float)s.Imaginary, 0.0f), Color.Blue);
            
            var vi = Complex.FromPolarCoordinates(v1.Magnitude, p - v1.Phase);
            s = s + vi*kfrac;
            ContinueLine(new Vector3((float)s.Real, (float)s.Imaginary, 0.0f), Color.BlueViolet);

            for (var b = maxb; b >= 1.0; b--)
            {
                var v = Complex.Pow(new Complex(b, 0), inp);
                v = Complex.FromPolarCoordinates(v.Magnitude, p - v.Phase);
                s += v;
                ContinueLine(new Vector3((float)s.Real, (float)s.Imaginary, 0.0f), Color.BlueViolet);
            }


            //centerline display
            var zh = Z * 0.5;
            var zp = Z.Phase;
            var cp = zp + Math.PI * 0.5;
            var c0 = Complex.FromPolarCoordinates(/*zh.Magnitude*/3.0, cp);
            var c1 = zh - c0;
            var c2 = zh + c0;
            var c = Color.Silver;
            c.A = 127;
            StartLine(new Vector3((float)c1.Real, (float)c1.Imaginary, 0), c);
            ContinueLine(new Vector3((float)c2.Real, (float)c2.Imaginary, 0), c);
        }


        public Complex Zeta(Complex s, int n = 10000)
        {
            var d = s.Real;
            var d1 = s.Imaginary;
            var d2 = 0.0;
            var d3 = 0.0;
            var d4 = 0.0;
            var d5 = 0.0;
            var dd1 = d1;
            for (var l1 = 1; l1 <= n; l1++)
            { // calculate Dirichlet Eta series partial sum of l iterations
                var d6 = 2 * l1 - 1;
                var d7 = 2 * l1;
                var d8 = (dd1 * Math.Log(d6)) % 6.2831853071795862;
                var d9 = (dd1 * Math.Log(d7)) % 6.2831853071795862;
                var d10 = -d;
                var d11 = Math.Pow(d6, d10);
                var d12 = Math.Pow(d7, d10);
                d2 += d11 * Math.Cos(d8) - d12 * Math.Cos(d9);
                d3 += d12 * Math.Sin(d9) - d11 * Math.Sin(d8);
                d4 += Math.Pow(d6, -0.5) * Math.Cos(d8) - Math.Pow(d7, -0.5) * Math.Cos(d9);
                var d14 = Math.Pow(d7, -0.5) * Math.Sin(d9);
                var d15 = Math.Pow(d6, -0.5);
                var d16 = Math.Sin(d8);
                d5 += d14 - d15 * d16;
            }
            // calculate Zeta from Eta value
            var d17 = 1.0 - Math.Pow(2, 1.0 - d) * Math.Cos(d1 * Math.Log(2));
            var d18 = Math.Pow(2, 1.0 - d) * Math.Sin(d1 * Math.Log(2));
            var d19 = (d17 * d2 + d18 * d3) / (Math.Pow(d17, 2) + Math.Pow(d18, 2));
            var d20 = (d17 * d3 - d18 * d2) / (Math.Pow(d17, 2) + Math.Pow(d18, 2));
            return new Complex(d19, -d20);//who's upsidedown here?
        }




        private void AddHistory(Queue<Vector3> h, Vector3 v)
        {
            var max = Inputs["TrailLength"].GetIntValue();
            while (h.Count >= max)
            {
                h.Dequeue();
            }
            h.Enqueue(v);
        }


    }
}
