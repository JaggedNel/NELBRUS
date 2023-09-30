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

public partial class Program : MyGridProgram
{
    sealed partial class NLB: SdSubPCmd {
        //======-SCRIPT BEGINNING-======

        /// <summary> Help functions </summary>
        public abstract class F {

            /// <summary> Turn <see cref="DateTime">DateTime</see> into string date with common format </summary>
            /// <param name="dt"> Date </param>
            public static string D(DateTime dt) => dt.ToString(CONST.DT.D);
            /// <summary> Turn <see cref="DateTime">DateTime</see> into string time with common format </summary>
            /// <param name="dT"> Time </param>
            public static string T(DateTime dt) => dt.ToString(CONST.DT.T);
            /// <summary> Turn <see cref="DateTime">DateTime</see> into string date with time in common format </summary>
            public static string DT(DateTime dt) => dt.ToString(CONST.DT.ET);
            /// <summary> Turn <see cref="DateTime">DateTime</see> into string date with short time in common format </summary>
            public static string DTS(DateTime dt) => dt.ToString(CONST.DT.EST);

            /// <summary> Time to ticks </summary>
            /// <param name="s"> Seconds </param>
            /// <param name="m"> Minutes </param>
            /// <param name="h"> Hours </param>
            public static uint TTT(byte s, byte m = 0, byte h = 0) => (uint)(s + m * 60 + h * 3600) * 60;
            /// <summary> Ticks to Time </summary>
            /// <param name="t"> Time in ticks </param>
            public static TimeSpan TTT(uint t) => new TimeSpan(t * 10000000);

            /// <summary> Get subprogram information </summary>
            /// <param name="p"> Subprogram </param>
            /// <param name="i"> Get advanced information </param>
            public static string SPI(SubP p, bool i = false) {
                var r = $"[{p.Name}]" + p.V == null ? $" v.{p.V}" : "";
                return i ? 
                    r + $"\n{p.Info}{(p is SdSubP ? $"\nWas launched at [{DT((p as SdSubP).ST)}].\nCommands support: {p is SdSubPCmd}." : "")}" : 
                    r;
            }
        }

        //======-SCRIPT ENDING-======
    }

}