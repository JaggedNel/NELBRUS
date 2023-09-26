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
            protected Dictionary<FieldNames, List<object>> Fields;
            protected Ind OInd;
            ActI[] C = new ActI[9];
            /// <summary> Message feild types </summary>
            public enum FieldNames : byte { Base, Msg, T1, T2, T3 };

            public SEcho() : base("Standart echo controller") {
                OInd = new Ind(Ind.UpdTurn,
                    "(._.)",
                    "   ( l: )",
                    "      (.–.)",
                    "         ( :l )",
                    "            (._.)");
                Fields = new Dictionary<FieldNames, List<object>> {
                    { FieldNames.Base, new List<object> { new List<object> {
                        $"OS NELBRUS v.{(string)OS.V}\nIs worked ",  (Req)OInd.Get, "\nInitialized subprograms: ", (ReqI)OS.GetCountISP, "\nRunned subprograms: ", (ReqI)OS.GetCountRSP } }
                    },
                    { FieldNames.Msg, new List<object>() } // Custom information
                };
                R = AddAct((Act)Refresh + OInd.Upd, 30);
                DT = F.TTT(45);
            }

            public override void Refresh() {
                var t = new StringBuilder();
                foreach (var f in Fields.Values) {
                    for (int i = 0; i < f.Count(); i++) {
                        if (f[i] is List<object>)
                            t.Append(Get(f[i] as List<object>));
                        else
                            t.Append(GetObj(f[i]));
                        t.Append("\n");
                    }
                }
                t.Append("\n\n\n\n\n\n\n");
                OS.P.Echo(t.ToString());
            }
            StringBuilder Get(List<object> line) {
                var s = new StringBuilder();
                foreach (var l in line)
                    s.Append(GetObj(l));
                return s;
            }
            string GetObj(object o) {
                return o is Req ? ((Req)o)() :
                    o is ReqI ? ((ReqI)o)().ToString() :
                    o.ToString();
            }

            /// <summary> Show custom info at echo </summary>
            public override void CShow(string s) {
                var l = Fields[FieldNames.Msg];
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
                var i = Fields[FieldNames.Msg].Count;
                if (i > 0) {
                    Fields[FieldNames.Msg].RemoveAt(i - 1);
                    C[i - 1] = null;
                }
            }
            /// <summary> Remove custom info in echo </summary>
            public override void CClr() {
                for (int i = 0; i < C.Count() && C[i] != null; i++) {
                    RemAct(ref C[i]);
                }
                Fields[FieldNames.Msg].Clear();
            }
        }

        //======-SCRIPT ENDING-======
    }
}