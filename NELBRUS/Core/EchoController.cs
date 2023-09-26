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
using System.Security.Cryptography;

public partial class Program : MyGridProgram
{
    sealed partial class NLB: SdSubPCmd {
        //======-SCRIPT BEGINNING-======

        /// <summary> Basic echo controller </summary>
        public class EchoController : SdSubP
        {
            /// <summary> Duration of message </summary>
            public uint DT;
            /// <summary> Refresh action </summary>
            protected ActI R;

            public EchoController() : base(1, "ECHO") {
                R = AddAct(Refresh, 100);
            }
            public EchoController(string n, MyVersion v = null, string i = CONST.NA) : base(1, n, v, i) { }

            /// <summary>
            /// Echo controller works with NELBRUS. 
            /// Do not stop it.
            /// </summary>
            public override bool MayStop() => false;
            /// <summary> Refresh information at echo </summary>
            public virtual void Refresh() => OS.P.Echo("OS NELBRUS is working. Echo unconfigured.");
            /// <summary> Show custom info at echo </summary>
            public virtual void CShow(string s) => OS.P.Echo(s);
            /// <summary>
            /// Show custom info at echo
            /// </summary>
            /// <param name="s"> A composite format string </param>
            /// <param name="p"> An object array that contains zero or more objects to format </param>
            public void CShow(string s, params object[] p) => CShow(String.Format(s, p));
            /// <summary> Remove custom info in echo </summary>
            public virtual void CClr() => Refresh();
        }

        //======-SCRIPT ENDING-======
    }


}