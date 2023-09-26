﻿using System;
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

public partial class Program: MyGridProgram {
    //======-SCRIPT BEGINNING-======

    JNew iJNew = new JNew();
    class JNew: InitSubP {
        public JNew() : base("", new MyVersion(1, 0)) { }

        public override SdSubP Start(ushort id) { return new TP(id, this); } // return OS.CSP<TP>() ? null : new TP(id, this); 

        class TP: SdSubP {

            ActI MA;

            public TP(ushort id, SubP p) : base(id, p) {

                MA = AddAct(Main, 1);
            }

            void Main() {

            }
        }
    }

    //======-SCRIPT ENDING-======
}