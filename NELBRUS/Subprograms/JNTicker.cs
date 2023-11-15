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

    class JNTicker: InitSubP {
        // Used for initialisation of subprogram
        public JNTicker() : base("Ticker", "First subprogram for NELBRUS system. This subprogram takes LCD with name \"Ticker\" and show current tick on it.") {
            Mem = new LocMem();
        }

        protected override SdSubP Init(ushort id) => new TP(id, this);

        class LocMem : MemReg {
            public LocMem() {
                 LCDname = GetMC("LCDame", "LCD Ticker");
            }

            public MemCell<string> LCDname;
        }

        class TP /* This Program */ : SdSubPCmd {
            IMyTextPanel LCD;
            ActI MA; // Show current tick on text panel
            LocMem mem;

            public TP(ushort id, InitSubP p) : base(id, p) {
                mem = p.Mem as LocMem;
                
                SetCmd(new Dictionary<string, Cmd>
                {
                    { "pause", new Cmd(CmdPause, "Pause show current tick.") },
                    { "play", new Cmd(CmdPlay, "Continue show current tick.") },
                    { "cLCDn", new Cmd(CmdChangeLCDname, "Change LCD name.") },
                });
            }

            void Init() {
                if ((LCD = OS.P.GridTerminalSystem.GetBlockWithName(mem.LCDname) as IMyTextPanel) == null)
                    OS.ECtrl.CShow(LCDnotFound());
                else
                    MA = AddAct(Show, 20);
            }

            string LCDnotFound() {
                return $"'LCD' [{mem.LCDname}] not found.";
            }

            void Show() {
                LCD.WriteText(OS.Tick.Str());
            }
            void MePause() { RemAct(ref MA); }
            void MePlay() { if (MA == null) MA = AddAct(Show, 20); }

            #region Commands
            string CmdPause(List<string> a) { MePause(); return "[ || ]"; }
            string CmdPlay(List<string> a) { if (LCD != null) MePlay(); else return LCDnotFound(); return "[ > ]"; }
            string CmdChangeLCDname(List<string> a) { mem.LCDname.V = a[0]; return ""; }
            #endregion Commands
        }
    }
    JNTicker iJNTicker = new JNTicker();

    //======-SCRIPT ENDING-======
}