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

    class JNTimer: InitSubP {
        public JNTimer() : base("Timer", "Shows the elapsed time on \"LCD timer\" when using the command \"ss\".") { }

        protected override SdSubP Init(ushort id) { return new TP(id, this); }

        class TP: SdSubPCmd {
            IMyTextPanel LCD;
            uint start;
            bool s = false;
            ActI MA;

            public TP(ushort id, InitSubP p) : base(id, p) {
                if ((LCD = OS.P.GridTerminalSystem.GetBlockWithName("LCD Timer") as IMyTextPanel) == null) {
                    Terminate("\"LCD Timer\" not found.");
                    return;
                }
                SetCmd(new Dictionary<string, Cmd>
                {
                    { "ss", new Cmd(CmdSS, "Start/stop timer.") }
                });
            }

            void Show() {
                LCD.WriteText(OS.Tick - start + "");
            }

            #region Commands
            string CmdSS(List<string> a) {
                if (s) {
                    LCD.WriteText(NLB.F.TTT(OS.Tick - start) + "");
                    RemAct(ref MA);
                } else {
                    start = OS.Tick;
                    MA = AddAct(Show, 10);
                }
                s = !s;
                return "";
            }
            #endregion Commands
        }
    }
    JNTimer iJNTimer = new JNTimer();

    //======-SCRIPT ENDING-======
}