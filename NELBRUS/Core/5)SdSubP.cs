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

public partial class Program : MyGridProgram {
    //======-SCRIPT BEGINNING-======

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
            public Act A;
            /// <summary> Start tick </summary>
            public uint ST;
            /// <summary> Frequency </summary>
            public uint F;
            /// <summary> Placement reference </summary>
            public Dictionary<uint, Act> Ref = null;

            public ActI(Act a, uint t, uint f) { 
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
            if (UN) UpdActions();
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
            foreach(var i in A2D) {
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

    //======-SCRIPT ENDING-======
}