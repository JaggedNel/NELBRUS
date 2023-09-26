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

namespace NELBRUS.Testing {
    public partial class Program: MyGridProgram {
        //======-SCRIPT BEGINNING-======


        #region CoreZone
        // Nelbrus OS v.0.6.0-[14.09.23]

        /// <summary> Operation System NELBRUS instance </summary>
        readonly static NLB OS = new NLB(); // Initializing OS

        #region Common

        /// <summary> Common SE script constructor </summary>
        Program() {
            // Do not put there anything else
            OS.Ready(this
                // Set your custom echo controller here TODO
                );
        }
        /// <summary> Common SE method invoked on game world saving </summary>
        void Save() {
            // Do not put there anything else
            OS.Save();
        }
        /// <summary> Common SE method invoked on triggering programmable block </summary>
        /// <param name="arg"> Trigger argument </param>
        /// <param name="uT"> Type of trigger source </param>
        void Main(string arg, UpdateType uT) {
            // Do not put there anything else
            OS.Main(arg, uT);
        }

        #endregion Common

        #region GlobalProperties

        /// <summary> Global constants </summary>
        class CONST {
            /// <summary> Not available message </summary>
            public const string NA = "N/A";
            /// <summary> Help message </summary>
            public const string mTUH = "Try use /help to fix your problem.";
            /// <summary> Argument exception message </summary>
            public const string mAE = "Argument exception. " + mTUH;
            /// <summary> Subprogram was termited message </summary>
            public const string mSPT = "Subprogram was forcibly closed.";
            /// <summary> Subprogram terminated on starting </summary>
            public const string mSPTS = "Subprogram {0} can not start by cause:\n{1}";
            /// <summary> Subprogram terminated me </summary>
            public const string mSPTP = "Subprogram #{0} '{1}' terminated by cause:\n{2}";

            public const string cmdH = "View commands help.";
            public const string cmdHd = "/help - show available commands;\n/help <command> - show command information.";
            public const string cmdNF = "Command {0} not found. " + mTUH;
            public const string cmdD = "Done (0)";

            /// <summary> Universal converting formats between <see cref="DateTime">DateTime</see> and <see cref="String">String</see> formats </summary>
            public class DT {
                /// <summary> Common date format -> Days.Months.Years </summary>
                public const string D = "dd.MM.yyyy";
                /// <summary> Common time format -> Hours:Minutes:Seconds </summary>
                public const string T = ST + ":ss";
                /// <summary> Short time format -> Hours:Minutes </summary>
                public const string ST = "HH:mm";
                /// <summary> Exact date with time format -> Hours:Minutes:Seconds Days.Months.Years </summary>
                public const string ET = T + " " + D;
                /// <summary> Date with short time format -> Hours:Minutes Days.Months.Years </summary>
                public const string EST = ST + " " + D;
            }
        }

        /// <summary> Action </summary>
        delegate void Act();
        /// <summary> Request without arguments </summary>
        delegate string Req();
        /// <summary> String request with string arguments used for commands </summary>
        /// <returns> Answer of the executed command </returns>
        delegate string ReqA(List<string> a);
        /// <summary> Integer request without arguments </summary>
        delegate int ReqI();


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
                if (value == null)
                    return null;
                var res = $"{value.G}.{value.E}";
                if (value.R.HasValue)
                    res += $".{value.R}";
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



        /// <summary> Basic subprogram class </summary>
        abstract class SubP {

            public string Name { get; protected set; }
            public string Info { get; protected set; }
            public readonly MyVersion V;

            public SubP(string name, MyVersion v = null, string info = CONST.NA) {
                Name = name;
                V = v;
                Info = info;
            }
            public SubP(string name, string info) : this(name, null, info) { }

        }



        /// <summary> Subprogram initilizer class base </summary>
        abstract class InitSubP: SubP {
            public InitSubP(string name, MyVersion v = null, string info = CONST.NA) : base(name, v, info) {
                OS.ISP(this);
            }
            public InitSubP(string name, string info) : this(name, null, info) { }

            /// <summary> Run new subprogram </summary>
            /// <param name="id"> Identificator of new subprogram </param>
            /// <returns> Started subprogram </returns>
            public virtual SdSubP Start(ushort id) { return null; }
        }



        /// <summary> Basic class of running subprogram </summary>
        abstract class SdSubP: SubP {

            /// <summary> System unique identifier </summary>
            public readonly ushort ID;
            /// <summary> Time when subprogram started </summary>
            public readonly DateTime ST;
            /// <summary> Terminate message showed on stop unworkable subprogram. </summary>
            public string TerminateMsg { get; private set; }

            /// <summary> Every tick actions </summary>
            Act EAct;
            /// <summary> 
            /// Actions with frequency registry. 
            /// Mean [tick, [frequency, actions]].
            /// </summary>
            Dictionary<uint, Dictionary<uint, Act>> Acts;
            /// <summary> 
            /// Deferred Actions registry. 
            /// Mean [tick, actions].
            /// </summary>
            Dictionary<uint, Act> DefA;
            /// <summary> Actions directory </summary>
            List<ActI> AD = new List<ActI>(), // General
                A2A = new List<ActI>(),       // To add
                A2D = new List<ActI>();       // To remove
            /// <summary> Actions update needed flag </summary>
            bool UN;

            /// <summary> Action instance </summary>
            public class ActI {
                /// <summary> Action delegate </summary>
                internal protected Act A;
                /// <summary> Start tick </summary>
                internal protected uint ST;
                /// <summary> Frequency </summary>
                internal protected uint F;
                /// <summary> Placement reference </summary>
                internal Dictionary<uint, Act> Ref = null;

                internal ActI(Act a, uint t, uint f) {
                    A = a;
                    ST = t;
                    F = f;
                }
            }

            public SdSubP(ushort id, string name, MyVersion v = null, string info = CONST.NA) : base(name, v, info) {
                ID = id;
                ST = DateTime.Now;
                EAct = delegate { };
                Acts = new Dictionary<uint, Dictionary<uint, Act>>();
                DefA = new Dictionary<uint, Act>();
                TerminateMsg = null;
            }
            public SdSubP(ushort id, string name, string info) : this(id, name, null, info) { }
            /// <summary> Used by NELBRUS in start to run new subprogram </summary>
            public SdSubP(ushort id, SubP p) : this(id, p.Name, p.V, p.Info) { }

            #region ActionsManagent

            /// <summary> 
            /// This method used by OS to process subprogram. 
            /// Do not use it for other. 
            /// </summary>
            public void Process() {
                if (UN)
                    UpdActions();
                var t = OS.Tick;

                // Do deffered actions
                if (DefA.ContainsKey(t)) {
                    DefA[t]();
                    DefA.Remove(t);
                    AD.RemoveAll(i => i.F == 0 && i.ST == t);
                }

                // Do every tick actions
                EAct();

                // Do periodic actions
                if (Acts.ContainsKey(t)) {
                    foreach (var f in Acts[t].Keys) { // Iterate frequencies
                        Acts[t][f](); // Invoke
                                      // Sustain processes
                        var T = t + f;
                        if (Acts.ContainsKey(T))
                            if (Acts[T].ContainsKey(f)) {
                                foreach(var i in AD.Where(i => i.Ref == Acts[t] && i.F == f))
                                    i.Ref = Acts[T];
                                Acts[T][f] += Acts[t][f];
                            } else
                                Acts[T].Add(f, Acts[t][f]);
                        else
                            Acts.Add(T, Acts[t]);
                    }

                    // Clean completed
                    Acts.Remove(t);
                }
            }

            /// <summary> Process to add and remove custom actions </summary>
            void UpdActions() {
                uint t;
                Dictionary<uint, Act> d;

                // Add new
                foreach (var i in A2A) {
                    t = i.ST;
                    d = null;
                    if (i.F == 0)
                        if ((d = DefA).ContainsKey(t))
                            d[t] += i.A;
                        else
                            d.Add(t, i.A);
                    else if (i.F == 1)
                        EAct += i.A;
                    else if (Acts.ContainsKey(t))
                        if ((d = Acts[t]).ContainsKey(i.F))
                            d[i.F] += i.A;
                        else
                            d.Add(i.F, i.A);
                    else
                        Acts.Add(t, d = new Dictionary<uint, Act>() { { i.F, i.A } });

                    i.Ref = d;
                    AD.Add(i);
                }
                A2A.Clear();

                // Remove
                foreach (var i in A2D) {
                    t = i.ST;
                    if (i.F == 0) {
                        if ((DefA[i.ST] -= i.A) == null)
                            DefA.Remove(i.ST);
                    } else if (i.F == 1)
                        EAct -= i.A;
                    else if ((i.Ref[i.F] -= i.A) == null) {
                        i.Ref.Remove(i.F);
                        if (i.Ref.Count == 0)
                            Acts.Remove(Acts.First(p => p.Value == i.Ref).Key);
                    }

                    i.A = null;
                    i.Ref = null;
                    AD.Remove(i);
                }
                A2D.Clear();

                UN = false;
            }
            /// <summary> Remove all actions </summary>
            public void ClearActs() {
                for (var j = 0; j < AD.Count;) {
                    var t = AD[j++];
                    RemAct(ref t);
                }
            }
            /// <summary> Register new triggerable action with span </summary>
            /// <param name="act"> Action delegate </param>
            /// <param name="f"> Frequency. if equal to 0 it will be invoked once. </param>
            /// <param name="s"> Time span </param>
            public ActI AddAct(Act act, uint f = 10, uint s = 0) {
                var i = new ActI(act, OS.Tick + 1 + s, f);
                A2A.Add(i);
                UN = true;
                return i;
            }
            /// <summary> Remove action triggered by the frequency </summary>
            /// <param name="i"> Action instance </param>
            public void RemAct(ref ActI i) {
                if (UN |= (!A2A.Remove(i) && !A2D.Contains(i)))
                    A2D.Add(i);
                i = null;
            }

            #endregion ActionsManagent

            /// <summary> Stop started subprogram </summary>
            public virtual void Stop() { OS.SSP(this); }
            /// <summary> 
            /// Returns true to let OS stop this subprogram.
            /// WARNING: Do not forget stop child subprograms there.
            /// </summary>
            public virtual bool MayStop() => true;
            /// <summary> Stop subprogram immediately </summary>
            /// <param name="msg"> Message about termination reason </param>
            public void Terminate(string msg = "") {
                TerminateMsg = string.IsNullOrEmpty(msg) ? CONST.mSPT : msg;
                Stop();
            }
        }



        /// <summary> Runnig subprogram with console commands support </summary>
        class SdSubPCmd: SdSubP {

            /// <summary> Command registry </summary>
            public Dictionary<string, Cmd> CmdR { get; private set; }

            public SdSubPCmd(ushort id, string name, MyVersion v = null, string info = CONST.NA) : base(id, name, v, info) {
                CmdR = new Dictionary<string, Cmd> { { "help", new Cmd(CmdHelp, CONST.cmdH, CONST.cmdHd) } };
            }
            public SdSubPCmd(ushort id, string name, string info) : this(id, name, null, info) { }
            /// <summary>Used by NELBRUS in start method to run new subprogram.</summary>
            public SdSubPCmd(ushort id, SubP p) : this(id, p.Name, p.V, p.Info) { }

            #region CommandsManagement

            /// <summary> Set new console command </summary>
            /// <param name="n"> Command name </param>
            /// <param name="c"> Method of command </param>
            public void SetCmd(string n, Cmd c) { CmdR.Add(n, c); }
            /// <summary> Set collection of new console commands.</summary>
            /// <param name="c"> Collection </param>
            public void SetCmd(Dictionary<string, Cmd> c) { foreach (var i in c) { CmdR.Add(i.Key, i.Value); } }

            #endregion CommandsManagement

            #region Default commands

            string CmdHelp(List<string> a) {
                var r = new StringBuilder();
                if (a.Count() == 0) {
                    r.Append("Available commands:");
                    foreach (var i in CmdR)
                        r.Append($"\n{NLB.F.Brckt(i.Key)} - {i.Value.H}");
                } else
                    return CmdR.ContainsKey(a[0]) ? $"{NLB.F.Brckt(a[0])} - {CmdR[a[0]].H}\nDetails:\n{CmdR[a[0]].D}" : $"Command {NLB.F.Brckt(a[0])} not found. {CONST.mTUH}";
                return r.ToString();
            }

            #endregion Default commands
        }



        #endregion GlobalProperties



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


            /// <summary>Echo controller.</summary>
            public EchoController EchoCtrl { get; private set; }

            #endregion Properties

            public NLB() : base(0, "NELBRUS", new MyVersion(0, 6, 0, new DateTime(2023, 09, 15)), "Your OS") {
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
                // Run all initialized subprograms
                foreach (var i in InitSP)
                    RSP(i);
            }
            /// <summary> 
            /// This method used to process save of programmable block.
            /// Do not use it for else.
            /// </summary>
            public void Save() {
                EchoCtrl.CShow($"Saved at {F.Brckt(F.DT(DateTime.Now))}");
            }
            /// <summary> 
            /// This method used to process run of programmable block.
            /// Do not use it for else.
            /// </summary>
            /// <param name="a"> If param is command, then it should start with '/' </param>
            public void Main(string a, UpdateType uT) {
                switch (uT) {
                    case UpdateType.Update1:
                        #region Update1

                        // Close marked subprograms
                        foreach (var i in SP2C)
                            SP.Remove(i);
                        SP2C.Clear();

                        // Process runned subprograms
                        foreach (var p in SP.Values)
                            p.Process();

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
            public override bool MayStop() { return false; }

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
                while (SP.ContainsKey(K))
                    K++;
                var t = p.Start(K);
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
                        EchoCtrl.CShow(CONST.mSPTP, p.ID, p.Name, p.TerminateMsg);
                    SP2C.Add(p.ID);
                    return true;
                }
                return false;
            }
            public int GetCountISP() { return OS.InitSP.Count; }
            public int GetCountRSP() { return OS.SP.Count; }

            #endregion SubprogramsManagement

            #region CommandsManagement

            ///<summary> Run parsed console command </summary>
            /// <param name="r"> Command registry </param>
            /// <param name="n"> Command name </param>
            /// <param name="a"> Console command arguments </param>
            /// <returns> Answer of command executing </returns>
            public static string Cmd(Dictionary<string, Cmd> r, string n, List<string> a) {
                Cmd c; // Tip: the ad can not be embedded in C# 6.0
                return r.TryGetValue(n, out c) ? c.C(a) : CONST.cmdNF;
            }
            /// <summary> Run single line console command </summary>
            /// <param name="s"> Single line console command </param>
            /// <param name="r"> Command registry </param>
            public static string Cmd(string s, Dictionary<string, Cmd> r) {
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
                    if (InitSP.Count > i)
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




            /// <summary> Basic echo controller </summary>
            public class EchoController: SdSubP {
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



            /// <summary> Standart echo controller </summary>
            public class SEcho: EchoController {
                protected Dictionary<FieldNames, List<object>> Fields;
                protected Ind OInd;
                ActI[] C = new ActI[9];
                /// <summary> Message feild types </summary>
                public enum FieldNames: byte { Base, Msg, T1, T2, T3 };

                public SEcho() : base("Standart echo controller") {
                    OInd = new Ind(Ind.UpdTurn,
                        "(._.)",
                        "   ( l: )",
                        "      (.–.)",
                        "         ( :l )",
                        "            (._.)");
                    Fields = new Dictionary<FieldNames, List<object>> {
                    { FieldNames.Base, new List<object> { new List<object> {
                        $"OS NELBRUS v.{(string)OS.V}\nIs worked ",  (Req)OInd.Get, "\nInitialized subprograms: ", (ReqI)OS.GetCountISP, "\nRunned subprograms: ", (ReqI)OS.GetCountRSP } }
                    },
                    { FieldNames.Msg, new List<object>() } // Custom information
                };
                    R = AddAct((Act)Refresh + OInd.Upd, 30);
                    DT = F.TTT(45);
                }

                public override void Refresh() {
                    var t = new StringBuilder();
                    foreach (var f in Fields.Values) {
                        for (int i = 0; i < f.Count(); i++) {
                            if (f[i] is List<object>)
                                t.Append(Get(f[i] as List<object>));
                            else
                                t.Append(GetObj(f[i]));
                            t.Append("\n");
                        }
                    }
                    t.Append("\n\n\n\n\n\n\n");
                    OS.P.Echo(t.ToString());
                }
                StringBuilder Get(List<object> line) {
                    var s = new StringBuilder();
                    foreach (var l in line)
                        s.Append(GetObj(l));
                    return s;
                }
                string GetObj(object o) {
                    return o is Req ? ((Req)o)() :
                        o is ReqI ? ((ReqI)o)().ToString() :
                        o.ToString();
                }

                /// <summary> Show custom info at echo </summary>
                public override void CShow(string s) {
                    var l = Fields[FieldNames.Msg];
                    l.Insert(0, s);
                    if (l.Count > C.Count())
                        l.RemoveAt(l.Count - 1);
                    for (int i = C.Count() - 1; i > 0; i--)
                        C[i] = C[i - 1];
                    C[0] = AddAct(RemCM, 0, DT);
                    Refresh();
                }
                /// <summary> Remove custom message after the time has elapsed </summary>
                void RemCM() {
                    var i = Fields[FieldNames.Msg].Count;
                    if (i > 0) {
                        Fields[FieldNames.Msg].RemoveAt(i - 1);
                        C[i - 1] = null;
                    }
                }
                /// <summary> Remove custom info in echo </summary>
                public override void CClr() {
                    for (int i = 0; i < C.Count() && C[i] != null; i++) {
                        RemAct(ref C[i]);
                    }
                    Fields[FieldNames.Msg].Clear();
                }
            }



            /// <summary> Help functions </summary>
            public abstract class F {

                /// <summary>Returns text with chosen symbols on edges.</summary>
                /// <param name="t">Editable text.</param>
                /// <param name="b">Used brackets.</param>
                public static string Brckt(string t, char? b = '[') {
                    switch (b) {
                        case '[':
                            return $"[{t}]";
                        case '{':
                            return $"{{{t}}}";
                        case '<':
                            return $"<{t}>";
                        case '(':
                            return $"({t})";
                        case null:
                            return t;
                        default:
                            return $"{b}{t}{b}";
                    }
                }

                /// <summary> Turn <see cref="DateTime">DateTime</see> into string date with common format </summary>
                /// <param name="dt"> Date </param>
                public static string D(DateTime dt) { return dt.ToString(CONST.DT.D); }
                /// <summary> Turn <see cref="DateTime">DateTime</see> into string time with common format </summary>
                /// <param name="dT"> Time </param>
                public static string T(DateTime dt) { return dt.ToString(CONST.DT.T); }
                /// <summary> Turn <see cref="DateTime">DateTime</see> into string date with time in common format </summary>
                public static string DT(DateTime dt) { return dt.ToString(CONST.DT.ET); }
                /// <summary> Turn <see cref="DateTime">DateTime</see> into string date with short time in common format </summary>
                public static string DTS(DateTime dt) { return dt.ToString(CONST.DT.EST); }

                /// <summary> Time to ticks </summary>
                /// <param name="s"> Seconds </param>
                /// <param name="m"> Minutes </param>
                /// <param name="h"> Hours </param>
                public static uint TTT(byte s, byte m = 0, byte h = 0) {
                    return (uint)(s + m * 60 + h * 3600) * 60;
                }
                /// <summary> Ticks to Time </summary>
                /// <param name="t"> Time in ticks </param>
                public static TimeSpan TTT(uint t) {
                    return new TimeSpan(t * 10000000);
                }

                /// <summary>Get subprogram information.</summary>
                /// <param name="p">Subprogram.</param>
                /// <param name="i">Get advanced information.</param>
                public static string SPI(SubP p, bool i = false) {
                    string r = $"[{p.Name}]" + p.V == null ? $" v.{p.V}" : "";
                    return i ?
                        r + $"\n{p.Info}{(p is SdSubP ? $"\nWas launched at [{DT((p as SdSubP).ST)}].\nCommands support: {p is SdSubPCmd}." : "")}" :
                        r;
                }
            }


        }




        /// <summary> Indicator automat </summary>
        class Ind {
            /// <summary> Current variant </summary>
            int CV;
            /// <summary> Indicator variants </summary>
            string[] Vrnts;
            /// <summary> Direction iterator </summary>
            int i;

            /// <summary> Method to update indicator </summary>
            /// <param name="cv"> Current variant </param>
            /// <param name="v"> Indicator variants </param>
            /// <param name="i"> Direction iterator </param>
            public delegate void UpdMethod(ref int cv, string[] v, ref int i);
            UpdMethod UM;

            /// <param name="um"> Method to update indicator </param>
            /// <param name="v"> Variants </param>
            public Ind(UpdMethod um, params string[] v) {
                CV = 0;
                UM = um;
                Vrnts = v;
                i = 1;
            }

            /// <summary> Update the indicator </summary>
            public void Upd() {
                UM(ref CV, Vrnts, ref i);
            }
            /// <summary> Get current indicator </summary>
            public string Get() {
                return Vrnts[CV];
            }

            #region UpdateMethods

            /// <summary> Update in order then reverse </summary>
            public static void UpdTurn(ref int cv, string[] v, ref int i) {
                cv += i;
                if (cv >= v.Length - 1 || cv <= 0)
                    i *= -1;
            }
            /// <summary> Update in order then repeat </summary>
            public static void UpdRepeat(ref int cv, string[] v, ref int i) {
                cv = cv >= v.Length - 1 ? 0 : cv + i;
            }

            #endregion UpdateMethods
        }



        #endregion CoreZone




        JNDalnoboy iJNDalnoboy = new JNDalnoboy();
        /// <summary>
        /// On-board system for smart rover Dalnoboy.
        /// Steam Workshop: https://steamcommunity.com/sharedfiles/filedetails/?id=2190992795
        /// </summary>
        class JNDalnoboy: InitSubP {
            public JNDalnoboy() : base("DALNOBOY on-board computer", new MyVersion(1, 0)) { }

            public override SdSubP Start(ushort id) { return new TP(id, this); }

            public class TP: SdSubPCmd {
                IMyShipController Controller;
                IMyMotorStator RotorSusp, RotorSolar;
                IMyMotorAdvancedStator HingeNeck, HingeSolar;
                List<IMyMotorAdvancedStator> HingesSolar = new List<IMyMotorAdvancedStator>();
                List<IMyShipController> Controllers = new List<IMyShipController>();
                List<IMyMotorAdvancedStator> Hinges = new List<IMyMotorAdvancedStator>();
                List<IMyMotorSuspension> Wheels = new List<IMyMotorSuspension>();
                float whangle, Tangle;
                bool Solar = true;

                ActI MA, GC, TS;

                public TP(ushort id, SubP p) : base(id, p) {
                    OS.GTS.GetBlocksOfType(Controllers, x => x.CanControlShip);
                    RotorSusp = OS.GTS.GetBlockWithName("Suspension Rotor") as IMyMotorStator;
                    HingeNeck = OS.GTS.GetBlockWithName("Neck Hinge") as IMyMotorAdvancedStator;
                    RotorSolar = OS.GTS.GetBlockWithName("Solar Rotor") as IMyMotorStator;
                    HingeSolar = OS.GTS.GetBlockWithName("Solar Hinge") as IMyMotorAdvancedStator;
                    OS.GTS.GetBlocksOfType(HingesSolar, x => x.CustomName.StartsWith("Solar Hinge") && x.CustomName != "Solar Hinge");
                    OS.GTS.GetBlocksOfType(Hinges, x => x.CustomName.StartsWith("Suspension"));
                    OS.GTS.GetBlocksOfType(Wheels);
                    SetCmd("tsp", new Cmd(CmdTurnSolar, "Turn solar panels"));

                    if (Controllers.Count == 0 || RotorSusp == null || HingeNeck == null)
                        Terminate("Dalnoboy blocks not found.");
                    else {
                        if (HingeSolar == null || RotorSolar == null)
                            Solar = false;
                        GC = AddAct(GetController, 20);
                        MA = AddAct(Control, 5, 1);
                    }
                }

                void Control() {

                    var TargetVecLoc = CustVectorTransform(Controller.GetTotalGravity(), HingeNeck.WorldMatrix.GetOrientation());
                    var Roll = Math.Atan2(-TargetVecLoc.X, TargetVecLoc.Z);
                    var Pitch = Math.Atan2(TargetVecLoc.Y, TargetVecLoc.Z);
                    HingeNeck.TargetVelocityRad = Turn(-(float)Roll, HingeNeck.Angle);

                    DampSuspRot(RotorSusp);
                    foreach (var h in Hinges)
                        DampSuspHinge(h, Roll, Pitch);
                }
                void GetController() {
                    if (!(Controller ?? (Controller = Controllers[0])).IsUnderControl || !Controller.CanControlShip)
                        for (int i = 1; i < Controllers.Count; i++)
                            if (Controllers[i].IsUnderControl && Controllers[i].CanControlShip) {
                                Controller = Controllers[i];
                                break;
                            }

                    if (Controller.GetShipSpeed() >= 12 && Controller.RollIndicator == 0)
                        if (Controller.GetShipSpeed() >= 40)
                            whangle = .17f;
                        else
                            whangle = .26f;
                    else
                        whangle = .38f;
                    if (Tangle != whangle) {
                        Tangle = whangle;
                        foreach (IMyMotorSuspension w in Wheels)
                            w.MaxSteerAngle = Tangle;
                    }
                }
                void DampSuspRot(IMyMotorStator r) {
                    r.TargetVelocityRPM = -r.Angle * 180 / 3.14f;
                    r.Torque = Math.Abs(r.Angle / r.UpperLimitRad * 120000);
                }
                void DampSuspHinge(IMyMotorAdvancedStator h, double r, double p) {
                    var T =
                    30000
                    + h.Angle / Math.Abs(h.LowerLimitRad) * 30000
                    + (float)(2 * p / Math.PI) * Math.Sign(Vector3D.Dot(h.GetPosition() - HingeNeck.GetPosition(), HingeNeck.WorldMatrix.Up)) * 20000
                    + (float)(2 * r / Math.PI) * Math.Sign(Vector3D.Dot(h.GetPosition() - HingeNeck.GetPosition(), HingeNeck.WorldMatrix.Forward)) * 20000
                    ;
                    byte upLegs = (byte)Convert.ToSingle(Controller.MoveIndicator.Y < 0 && ((Vector3D.Dot(h.GetPosition() - HingeNeck.GetPosition(), HingeNeck.WorldMatrix.Up) < 0 && Controller.MoveIndicator.Z <= 0) || (Vector3D.Dot(h.GetPosition() - HingeNeck.GetPosition(), HingeNeck.WorldMatrix.Up) > 0 && Controller.MoveIndicator.Z > 0)));
                    h.Torque = Convert.ToSingle(T > 0) * T + upLegs * 100000;
                    h.TargetVelocityRPM = upLegs * 80 - 40;
                }
                /// <summary>Transfer of coordinates of Vec to Orientation coordinate system.</summary>
                Vector3D CustVectorTransform(Vector3D Vec, MatrixD Orientation) {
                    // standart
                    //return new Vector3D(Vec.Dot(Orientation.Right), Vec.Dot(Orientation.Up), Vec.Dot(Orientation.Forward));
                    return new Vector3D(Vec.Dot(Orientation.Backward), Vec.Dot(Orientation.Up), Vec.Dot(Orientation.Right));
                }

                float Turn(float DesiredAngle, float CurrentAngle) {
                    float Turn = DesiredAngle - CurrentAngle;
                    Turn = Normalize(Turn);
                    return Turn;
                }
                float Normalize(float Angle) {
                    if (Angle < -Math.PI)
                        Angle += 2 * (float)Math.PI;
                    else if (Angle > Math.PI)
                        Angle -= 2 * (float)Math.PI;
                    return Angle;
                }

                public void TurnSolar() {
                    if (RotorSolar.Angle < .3 || RotorSolar.Angle > 2 * Math.PI - .45) {
                        HingeSolar.TargetVelocityRad *= -1;
                        RotorSolar.TargetVelocityRad = 0;
                        RemAct(ref TS);
                    } else
                        RotorSolar.TargetVelocityRad = 1;
                }

                string CmdTurnSolar(List<string> a) {
                    if (!Solar)
                        return "Dalnoboy solar panels not available!";
                    foreach (var i in HingesSolar)
                        i.TargetVelocityRad *= -1;
                    TS = AddAct(TurnSolar, 30);
                    return "";
                }
            }
        }



        //======-SCRIPT ENDING-======
    }
}