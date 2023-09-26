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
    //======-SCRIPT BEGINNING-======

    /// <summary> 
    /// Represents the version number of an item. 
    /// v.3.1-[15.09.2023] 
    /// </summary>
    class MyVersion {
        /// <summary> Version Generation (Major) </summary>
        public readonly byte G;
        /// <summary> Version Edition (Minor) </summary>
        public readonly byte E;
        /// <summary> Version Revision </summary>
        public readonly byte? R;
        /// <summary> Version Date </summary>
        public readonly DateTime? D;

        /// <param name="d"> Version date </param>
        public MyVersion(DateTime d) : this(0, 0, d) { }
        /// <param name="M"> Major </param>
        /// <param name="m"> Minor </param>
        /// <param name="d"> Version date </param>
        public MyVersion(byte M, byte m, DateTime d) : this(M, m) { D = d; }
        /// <param name="M"> Generation </param>
        /// <param name="m"> Edition </param>
        /// <param name="r"> Revision </param>
        /// <param name="d"> Version date </param>
        public MyVersion(byte M, byte m, byte? r = null, DateTime? d = null) { G = M; E = m; R = r; D = d; }

        public static implicit operator string(MyVersion value) {
            if (value == null) return null;
            var res = $"{value.G}.{value.E}";
            if (value.R.HasValue) res += $".{value.R}";
            return value.D.HasValue ? res + $"-[{NLB.F.D(value.D.Value)}]" : res;
        }
        public static implicit operator MyVersion(string version) {
            int i;
            DateTime d;
            DateTime? D = null; 
            if ((i = version.IndexOf("-")) > 0 && i < version.Count() - 1) {
                D = DateTime.TryParseExact(version.Substring(i + 1), $"[{CONST.DT.D}]", null, System.Globalization.DateTimeStyles.None, out d) ? d : D;
                version = version.Remove(i);
            }
            var w = Array.ConvertAll(version.Split('.'), Byte.Parse);

            switch (w.Count()) {
                case 2:
                    return new MyVersion(w[0], w[1], null, D);
                case 3:
                    return new MyVersion(w[0], w[1], w[2], D);
                default:
                    return null;
            }
        }
    }

    //======-SCRIPT ENDING-======
}