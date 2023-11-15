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

public partial class Program: MyGridProgram {
    //======-SCRIPT BEGINNING-======

    class JNew: InitSubP {
        public JNew() : base("", new MyVersion(1, 0)) { }

        protected override SdSubP Init(ushort id) => new TP(id, this);

        class TP: SdSubP {

            ActI MA;

            public TP(ushort id, InitSubP p) : base(id, p) {

                MA = AddAct(Main, 1);
            }

            void Main() {

            }
        }
    }
    JNew iJNew = new JNew();

    //======-SCRIPT ENDING-======
}