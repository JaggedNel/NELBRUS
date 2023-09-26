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

    /// <summary> Command </summary>
    struct Cmd {

        /// <summary> Command request </summary>
        public ReqA C;
        /// <summary> Help info about command </summary>
        public string H;
        /// <summary> Details about command </summary>
        public string D;

        /// <param name="c"> Command request </param>
        /// <param name="h"> Help info </param>
        /// <param name="d"> Details </param>
        public Cmd(ReqA c, string h = CONST.NA, string d = CONST.NA) { C = c; H = h; D = d; }

    }

    //======-SCRIPT ENDING-======
}