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


    /// <summary>OS NELBRUS class.</summary>
    sealed partial class NLB: SdSubPCmd {

        #region Properties

        /// <summary> 
        /// Reference to <see cref="Program">Program</see> class to get access to it`s functions. 
        /// Example: OS.P.Me 
        /// </summary>
        public Program P { get; private set; }
        /// <summary>
        /// Reference to Grid Terminal System of this Program to get access to it`s functions. 
        /// Example: OS.GTS.GetBlockWithName()
        /// </summary>
        public IMyGridTerminalSystem GTS { get; private set; }
        /// <summary> Internal time measurement unit </summary>
        public uint Tick { get; private set; }
        /// <summary> ID for new subprogram to start </summary>
        ushort K;
        /// <summary> Initialised subprograms </summary>
        List<InitSubP> InitSP;
        /// <summary>
        /// Started subprograms.
        /// Mean [id, subprogram].
        /// </summary>
        Dictionary<ushort, SdSubP> SP;
        /// <summary> Started subprograms to close </summary>
        List<ushort> SP2C = new List<ushort>();
        MyIni Memory = new MyIni();
        bool MemOk = false;

        public delegate void DMain(string arg, UpdateType uT);
        public DMain Main;

        /// <summary>Echo controller.</summary>
        public EchoController EchoCtrl { get; private set; }

        #endregion Properties

        public NLB() : base(0, InitSubP.GetPlug("NELBRUS", new MyVersion(0, 6, 0, new DateTime(2023, 09, 15)), "Your OS")) {
            Tick = 0;
            InitSP = new List<InitSubP>();
            SP = new Dictionary<ushort, SdSubP>() { { 0, this } };
            K = 2;
        }

        #region Methods

        /// <summary>
        /// This method used to initialize OS. 
        /// Do not use it for other.
        /// </summary>
        public void Ready(Program p, EchoController EC = null) {
            P = p;
            GTS = P.GridTerminalSystem;
            if (_debug)
                Main = TryMain;
            else
                Main = DoMain;
            SetCmd(new Dictionary<string, Cmd>
            {
                { "start", new Cmd(CmdRun, "Start initialized subprogram by id.", "/start <id> - Start new subprogram, check id by /isp.") },
                { "stop", new Cmd(CmdStop, "Stop runned subprogram by id.", "/stop <id> - Stop subprogram, check id by /sp.") },
                { "sp", new Cmd(CmdSP, "View runned subprograms or run the subprogram command.", "/sp - View runned subprograms;\n/sp <id> - View runned subprogram information;\n/sp <id> <command> [arguments] - Run the subprogram command.") },
                { "isp", new Cmd(CmdISP, "View initilized subprograms information.", "/isp - View initilized subprograms;\n/isp <id> - View initilized subprogram information.") },
                { "clr", new Cmd(CmdClearC, "Clearing the command interface.") },
            });
            EchoCtrl = EC == null ? new EchoController() : EC;
            P.Runtime.UpdateFrequency = UpdateFrequency.Update1;
            SP.Add(1, EchoCtrl);

            if (!(MemOk = Memory.TryParse(P.Storage))) {
                EchoCtrl.CShow("Memory parsing error.");
            } else {
                /// TODO чтение из памяти
                
            }

            // Run all initialized subprograms
            foreach (var i in InitSP)
                RSP(i);
        }
        /// <summary> 
        /// This method used to process save of programmable block.
        /// Do not use it for else.
        /// </summary>
        public void Save() {
            //EchoCtrl.CShow($"Saved at [{F.DT(DateTime.Now)}]");
            //if (!MemOk) 
                return;

            Memory.Clear();
            foreach (var p in SP.Values) {
                foreach (var r in p.Mems) {
                    var t = $"{p.Name}¶{r.Name}";
                    foreach (var c in r.Reg) {
                        Memory.Set(t, c.N, c.Value);
                    }
                }
            }

            OS.P.Storage = Memory.Str();
        }
        void TryMain(string a, UpdateType uT) {
            try {
                DoMain(a, uT);
            } catch (Exception e) {
                OS.P.Me.CustomData = e.Message;
                P.Runtime.UpdateFrequency = UpdateFrequency.None;
            }
        }
        /// <summary> 
        /// This method used to process run of programmable block.
        /// Do not use it for else.
        /// </summary>
        /// <param name="a"> If param is command, then it should start with '/' </param>
        void DoMain(string a, UpdateType uT) {
            switch (uT) {
                case UpdateType.Update1:
                    #region Update1

                    // Close marked subprograms
                    foreach (var i in SP2C)
                        SP.Remove(i);
                    SP2C.Clear();

                    // Process runned subprograms
                    foreach (var p in SP.Values) p.Process();

                    Tick++;

                    #endregion Update1
                    break;
                case UpdateType.Update10:
                    P.Runtime.UpdateFrequency = UpdateFrequency.Update1;
                    break;
                case UpdateType.Update100:
                    goto case UpdateType.Update10;
                default:
                    if (a.StartsWith("/"))
                        EchoCtrl.CShow($"> {Cmd(a.Substring(1), CmdR)}");
                    break;
            }
        }
        /// <summary> Nobody cant stop it :P </summary>
        public override bool MayStop() => false;

        #region SubprogramsManagement

        /// <summary> 
        /// Initialise new subprogram.
        /// Do not use it.
        /// </summary>
        public void ISP(InitSubP p) {
            InitSP.Add(p);
        }
        /// <summary> Run new subprogram </summary>
        /// <returns> New started subprogram or null </returns>
        public SdSubP RSP(InitSubP p) {
            if (SP.Any(a => a.Value.Base == p)) {
                EchoCtrl.CShow(CONST.mSPAS, p.Name);
                return null;
            }
            while (SP.ContainsKey(K)) K++;
            var t = p.Run(K);
            if (t != null)
                if (t.TerminateMsg == null) {
                    SP.Add(K++, t);
                    return t;
                } else
                    EchoCtrl.CShow(CONST.mSPTS, t.Name, t.TerminateMsg);
            return null;
        }
        /// <summary> Stop subprogram </summary>
        /// <returns> True if subprogram successfully stopped </returns>
        public bool SSP(SdSubP p) { 
            if ((TerminateMsg != null || p.MayStop()) && SP.ContainsKey(p.ID) && !SP2C.Contains(p.ID)) {
                if (!string.IsNullOrEmpty(p.TerminateMsg))
                    EchoCtrl.CShow(CONST.mSPTP, p.ID.Str(), p.Name, p.TerminateMsg);
                SP2C.Add(p.ID);
                return true;
            }
            return false;
        }
        public int GetCountISP() => OS.InitSP.Count;
        public int GetCountRSP() => OS.SP.Count;

        #endregion SubprogramsManagement

        #region CommandsManagement

        ///<summary> Run parsed console command </summary>
        /// <param name="r"> Command registry </param>
        /// <param name="n"> Command name </param>
        /// <param name="a"> Console command arguments </param>
        /// <returns> Answer of command executing </returns>
        public static string Cmd(Dictionary<string, Cmd> r, string n, List<string> a)
        {
            Cmd c;
            return r.TryGetValue(n, out c) ? c.C(a) : string.Format(CONST.cmdNF, n);
        }
        /// <summary> Run single line console command </summary>
        /// <param name="s"> Single line console command </param>
        /// <param name="r"> Command registry </param>
        public static string Cmd(string s, Dictionary<string, Cmd> r)
        {
            string n, m;
            List<string> a;
            CP(s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList(), out n, out a);
            return string.IsNullOrEmpty(m = Cmd(r, n, a)) ? string.Format(CONST.cmdD, n) : m;
        }
        /// <summary> Command parse. Returns command name and arguments. </summary>
        /// <param name="s"> Splitted words of command </param>
        /// <param name="n"> Command name </param>
        /// <param name="a"> Command arguments. Arguments marked as "separated words" (in quotation marks) will be considered as a single argument. </param>
        static public void CP(List<string> s, out string n, out List<string> a) {
            a = new List<string>();
            if (s.Count == 0) { n = ""; return; }
            var f = false;
            int c = 0;
            foreach (var w in s) {
                if (f)
                    a[c] += " " + (w.EndsWith("\"") ? w.Remove(w.Length - 1) : w);
                else {
                    a.Add((f = w.StartsWith("\"")) ? w.Substring(1) : w);
                    c++;
                }
            }
            n = a[0];
            a.RemoveAt(0);
        }

        #endregion CommandsManagement

        #region Commands

        string CmdRun(List<string> a) {
            int i;
            if (a.Count() > 0 && int.TryParse(a[0], out i))
                if (InitSP.Count > i && i >= 0)
                    return OS.RSP(InitSP[i]) == null ? $"Attempt to run new subprogram [{InitSP[i].Name}] failed." : $"New subprogram [{InitSP[i].Name}] runned.";
                else
                    return $"Initialized subprogram with ID [{i}] not exist. {CONST.mTUH}";
            else
                return CONST.mAE;
        }
        string CmdStop(List<string> a) {
            ushort i;
            if (a.Count() > 0 && ushort.TryParse(a[0], out i))
                if (SP.ContainsKey(i))
                    return $"Subprogram [{SP[i].Name}] " + (SSP(SP[i]) ? "successfully stopped." : $"can`t be stopped now.");
                else
                    return $"Subprogram with ID [{i}] not exist. {CONST.mTUH}";
            else
                return CONST.mAE;
        }
        string CmdSP(List<string> a) {
            if (a.Count == 0) {
                var r = "Runned subprograms [id - info]: ";
                foreach (var i in SP.Keys)
                    r += $"\n{i} - {F.SPI(SP[i])}";
                return r;
            }
            ushort k;
            if (ushort.TryParse(a[0], out k))
                if (SP.ContainsKey(k))
                    if (a.Count == 1)
                        return $"Runned subprogram information:\n{F.SPI(SP[k], true)}";
                    else
                        return SP[k] is SdSubPCmd ? Cmd((SP[k] as SdSubPCmd).CmdR, a[1], a.GetRange(2, a.Count - 2)) : $"Subprogram {SP[k].Name} does not support commands.";
                else
                    return $"Subprogram with id [{k}] not exist.";
            else
                return CONST.mAE;
        }
        string CmdISP(List<string> a) {
            if (a.Count == 0) {
                var r = "Initialized subprograms:";
                for (int i = 0; i < InitSP.Count; i++)
                    r += $"\n{i} - {F.SPI(InitSP[i])}";
                return r;
            }
            int k;
            return int.TryParse(a[0], out k) ?
                "Initialized subprogram " + (InitSP.Count > k ?
                    $"information:\n{F.SPI(InitSP[k], true)}" :
                    $"with ID [{k}] not exist.") :
                CONST.mAE;
        }
        string CmdClearC(List<string> a) { 
            EchoCtrl.CClr();
            return " ";
        }

        #endregion Commands

        #endregion Methods



        /// #INSERT EchoController
        /// #INSERT SEcho
        /// #INSERT F
    }

    //======-SCRIPT ENDING-======
}