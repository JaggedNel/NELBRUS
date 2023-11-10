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

    /// <summary> Runnig subprogram with console commands support </summary>
    class SdSubPCmd: SdSubP {

        /// <summary> Command registry </summary>
        public Dictionary<string, Cmd> CmdR { get; private set; }

        //public SdSubPCmd(ushort id, string name, MyVersion v = null, string info = CONST.NA) : base(id, name, v, info) {
        //    CmdR = new Dictionary<string, Cmd> { { "help", new Cmd(CmdHelp, CONST.cmdH, CONST.cmdHd) } };
        //}
        //public SdSubPCmd(ushort id, string name, string info) : this(id, name, null, info) { }


        /// <summary>Used by NELBRUS in start method to run new subprogram.</summary>
        public SdSubPCmd(ushort id, InitSubP p) : base(id, p) {
            CmdR = new Dictionary<string, Cmd> { { "help", new Cmd(CmdHelp, CONST.cmdH, CONST.cmdHd) } };
        }

        #region CommandsManagement

        /// <summary> Set new console command </summary>
        /// <param name="n"> Command name </param>
        /// <param name="c"> Method of command </param>
        public void SetCmd(string n, Cmd c) { CmdR.Add(n, c); }
        /// <summary> Set collection of new console commands.</summary>
        /// <param name="c"> Collection </param>
        public void SetCmd(Dictionary<string, Cmd> c) { foreach (var i in c) CmdR.Add(i.Key, i.Value); }

        #endregion CommandsManagement

        #region Default commands

        string CmdHelp(List<string> a) {
            var r = new StringBuilder();
            if (a.Count() == 0) {
                r.Append("Available commands:");
                foreach (var i in CmdR)
                    r.Append($"\n[{i.Key}] - {i.Value.H}");
            } else
                return CmdR.ContainsKey(a[0]) ? $"[{a[0]}] - {CmdR[a[0]].H}\nDetails:\n{CmdR[a[0]].D}" : $"Command [{a[0]}] not found. {CONST.mTUH}";
            return r.Str();
        }

        #endregion Default commands
    }

    //======-SCRIPT ENDING-======
}