using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using VRageMath;
using VRage.Game;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Ingame;
using Sandbox.Game.EntityComponents;
using VRage.Game.Components;
using VRage.Collections;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Linq;
using VRage.Game.ModAPI.Ingame.Utilities;
using System.Text.RegularExpressions;
using VRage;

public partial class Program : MyGridProgram
{
    sealed partial class NLB: SdSubPCmd {
        //======-SCRIPT BEGINNING-======

        /// <summary> Standart echo controller </summary>
        public class SEcho: EchoController {
            /// <summary> Fields </summary>
            protected Dictionary<FN, List<IReadable>> F;
            protected Ind OInd;
            protected ActI[] C;
            /// <summary> Message feild type names </summary>
            public enum FN: byte { Base, Msg, T1, T2, T3 };
            string b;

            public SEcho() : base(InitSubP.GetPlug("Standart echo controller")) {
                OInd = new Ind(Ind.UpdTurn,
                    "(._.)",
                    "   ( l: )",
                    "      (.–.)",
                    "         ( :l )",
                    "            (._.)");
                b = OS.Name + " v." + OS.V + CONST.mEB;
                C = new ActI[9];
                F = new Dictionary<FN, List<IReadable>> {
                    { FN.Base, new List<IReadable> { new TD<string>(B) } },
                    { FN.Msg, new List<IReadable>() }
                };
                R = AddAct((Act)Refresh + OInd.Upd, 30);
                DT = NLB.F.TTT(45);
            }

            string B() => string.Format(b, OInd.Get(), OS.GetCountISP(), OS.GetCountRSP());
            public override void Refresh() {
                var t = new StringBuilder();

                foreach (var f in F.Values)
                    for (int i = 0; i < f.Count(); i++)
                        t.Append(f[i] + "\n");

                OS.P.Echo(t.Append("\n\n\n\n\n\n\n").Str());
            }

            /// <summary> Show custom info at echo </summary>
            public override void CShow(string s) {
                CShow(new TV<string>(s));
            }
            /// <summary> Show custom info at echo </summary>
            public void CShow(IReadable s) {
                var l = F[FN.Msg];
                l.Insert(0, s);
                if (l.Count > C.Count())
                    l.RemoveAt(l.Count - 1);
                for (int i = C.Count() - 1; i > 0; i--)
                    C[i] = C[i - 1];
                C[0] = AddAct(RemCM, 0, DT);
                Refresh();
            }
            /// <summary> Remove custom message after the time has elapsed </summary>
            void RemCM() {
                var i = F[FN.Msg].Count;
                if (i > 0) {
                    F[FN.Msg].RemoveAt(i - 1);
                    C[i - 1] = null;
                }
            }
            /// <summary> Remove custom info in echo </summary>
            public override void CClr() {
                for (int i = 0; i < C.Count() && C[i] != null; i++)
                    RemAct(ref C[i]);
                F[FN.Msg].Clear();
            }
        }

        //======-SCRIPT ENDING-======
    }
}