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

        /// <summary> Reference to <see cref="Program">Program</see> class to get access to it`s functions </summary>
        public Program P { get; private set; }
        /// <summary> Reference to Grid Terminal System of this Program to get access to it`s functions </summary>
        /// <example> OS.GTS.GetBlockWithName() </example>
        public IMyGridTerminalSystem GTS => P.GridTerminalSystem;
        /// <summary> Reference to currently runned programmable block </summary>
        public IMyProgrammableBlock Me => P.Me;
        /// <summary> Internal time measurement unit </summary>
        public uint Tick { get; private set; }
        /// <summary> ID for new subprogram to start </summary>
        ushort K;
        /// <summary> Initialised subprograms </summary>
        List<InitSubP> InitSP;
        /// <summary> Started subprograms </summary>
        /// <remarks> Mean [id, subprogram] </remarks>
        Dictionary<ushort, SdSubP> SP;
        /// <summary> Started subprograms to close </summary>
        List<ushort> SP2C = new List<ushort>();
        /// <summary> Global memory space </summary>
        public readonly MyIni GM = new MyIni();
        /// <summary> Is memory have been readed </summary>
        bool MOk = false;
        /// <summary> Memory rewrite needed </summary>
        bool SN = false;

        /// <summary> Main action kind </summary>
        public delegate void DMain(string arg, UpdateType uT);
        /// <summary> Main action </summary>
        public DMain Main { get; private set; }
        /// <summary> Saving action </summary>
        ActI SA;

        /// <summary> Echo controller </summary>
        public EchoController ECtrl { get; private set; }

        #endregion Properties

        public NLB() : base(0, InitSubP.GetPlug("NELBRUS", new MyVersion(0, 6, 0, new DateTime(2023, 09, 15)), "Your OS")) {
            Tick = 0;
            InitSP = new List<InitSubP>();
            SP = new Dictionary<ushort, SdSubP>() { { 0, this } };
            K = 2;
        }

        #region Methods

        /// <summary> This method used to initialize OS </summary>
        /// <remarks> Do not use it for other </remarks>
        /// <example> OS.Ready(this, new NLB.SEcho()); </example>
        public void Ready(Program p, EchoController EC = null) {
            P = p;
            if (_d)
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
            ECtrl = EC == null ? new EchoController() : EC;
            P.Runtime.UpdateFrequency = UpdateFrequency.Update1;
            SP.Add(1, ECtrl);

            if (!(MOk = GM.TryParse(P.Storage)))
                ECtrl.CShow("Memory parsing error.");
            //else {
            //    /// TODO чтение из памяти

            //}

            // Run all initialized subprograms
            foreach (var i in InitSP)
                RSP(i);

            if (MOk)
                SA = AddAct(TrySave, 60);
        }

        /// <summary> Mark saving needed </summary>
        public void ToSave() {
            SN = true;
        }
        /// <summary> Execute saving if needed </summary>
        public void TrySave() {
            if (SN) 
                Save();
        }
        /// <summary> This method used to process save event of programmable block </summary>
        /// <remarks> Do not use it </remarks>
        public void Save() {
            SN = false;
            if (!MOk)
                return;

            // Build subprogram memory dump
            var t = new StringBuilder();
            foreach (var p in SP.Values) {
                foreach (var r in p.Regs) {
                    t.AppendLine(r.Mem.Str());
                }
            }

            OS.P.Storage = t.Str();
        }
        /// <summary> This Method used to process run of programmable block with handling errors </summary>
        /// <remarks> Do not use it </remarks>
        /// <param name="a"> Run argument. If argument is command, then it should start with '/' </param>
        /// <param name="uT"> Type of trigger source </param>
        void TryMain(string a, UpdateType uT) {
            try {
                DoMain(a, uT);
            } catch (Exception e) {
                Error(e);
            }
        }
        /// <summary> This method used to process run of programmable block </summary>
        /// <remarks> Do not use it </remarks>
        /// <param name="a"> Run argument. If argument is command, then it should start with '/'. </param>
        /// <param name="uT"> Type of trigger source </param>
        void DoMain(string a, UpdateType uT) {
            switch (uT) {
                case UpdateType.Update1:
                    // Close marked subprograms
                    foreach (var i in SP2C)
                        SP.Remove(i);
                    SP2C.Clear();

                    // Process runned subprograms
                    foreach (var p in SP.Values) p.Process();

                    Tick++;
                    break;
                case UpdateType.Update10:
                case UpdateType.Update100:
                    P.Runtime.UpdateFrequency = UpdateFrequency.Update1;
                    break;
                default:
                    // Processing command
                    if (a.StartsWith("/"))
                        ECtrl.CShow($"> {Cmd(a.Substring(1), CmdR)}");
                    break;
            }
        }
        /// <summary> Log error and stop program block </summary>
        /// <param name="e"> Handled error </param>
        public void Error(Exception e) {
            var t = $"ERROR {F.NowDT} - {e.GetType()}:  {e.Message}";
            P.Echo(t);
            Log($"{t}\n({e.Source} - {e.InnerException?.Message})\nTrace:\n{e.StackTrace}");
            throw new Exception("NELBRUS handled exception! Check CustomData for info.");
        }
        /// <summary> Nobody cant stop it :P </summary>
        public override bool MayStop() => false;

        #region Subprograms management

        /// <summary> Initialise new subprogram </summary>
        /// <remarks> Do not use it </remarks>
        public void ISP(InitSubP p) {
            InitSP.Add(p);
        }
        /// <summary> Run new subprogram </summary>
        /// <returns> New started subprogram or null </returns>
        public SdSubP RSP(InitSubP p) {
            // Cancel if already started
            if (SP.Any(a => a.Value.Base == p)) {
                ECtrl.CShow(CONST.mSPAS, p.Name);
                return null;
            }
            // Getting unic key
            while (SP.ContainsKey(K)) K++;
            // Try start
            var t = p.Run(K);
            if (t != null) {
                if (t.TMsg == null) {
                    // Start completed
                    SP.Add(K++, t);
                    // Reading memory
                    foreach (var R in t.Regs.Where(r => r.RR))
                        R.Reset();
                    // Initilize
                    if (t.Init())
                        return t;
                    else
                        SP.Remove(--K);
                }
                ECtrl.CShow(CONST.mSPTS, t.Name, t.TMsg);
            }
            return null;
        }
        /// <summary> Stop subprogram </summary>
        /// <returns> True if subprogram successfully stopped </returns>
        public bool SSP(SdSubP p) { 
            if ((TMsg != null || p.MayStop()) && SP.ContainsKey(p.ID) && !SP2C.Contains(p.ID)) {
                if (!string.IsNullOrEmpty(p.TMsg))
                    ECtrl.CShow(CONST.mSPTP, p.ID.Str(), p.Name, p.TMsg);
                SP2C.Add(p.ID);
                return true;
            }
            return false;
        }
        /// <returns> Count of initilized subprograms </returns>
        public int GetCountISP() => OS.InitSP.Count;
        /// <returns> Count of started subprograms </returns>
        public int GetCountRSP() => OS.SP.Count;

        #endregion Subprograms management

        #region Commands management

        /// <summary> Run parsed console command </summary>
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
        /// <returns> Answer of command executing </returns>
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

        #endregion Commands management

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
            ECtrl.CClr();
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