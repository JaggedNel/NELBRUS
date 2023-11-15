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
using VRage;

public partial class Program : MyGridProgram
{
    //======-SCRIPT BEGINNING-======

    /// <summary>
    /// Memory register
    /// </summary>
    class MemReg {
        /// <summary> Global memory </summary>
        public static List<MyIni> Regs = new List<MyIni>();

        readonly SdSubP Base;
        public readonly string N;
        /// <summary> Register </summary>
        List<IMemCell> Reg = new List<IMemCell>();
        /// <summary> Local memory </summary>
        public readonly MyIni Mem;
        /// <summary> Reset required </summary>
        public bool RR;

        /// <param name="p"> Subprogram register holder </param>
        /// <param name="n"> Name of register </param>
        public MemReg(SdSubP p, string n = "Main") {
            if (p.Regs.Any(r => r.N == n))
                OS.Error(new ArgumentException($"Register '{n}' already exists in program '{p.Name}'"));
            Base = p;
            N = n;
            p.Regs.Add(this);

            Mem = Regs.Find(r => r.ContainsSection(n) && r.GetSectionComment(n) == p.Name);
            if (!(RR = Mem != default(MyIni))) {
                Mem = new MyIni();
                Mem.AddSection(n);
                Mem.SetSectionComment(n, p.Name);
                RR = OS.GM.ContainsSection(n) && OS.GM.GetSectionComment(n) == p.Name;
                Regs.Add(Mem);
            }
        }

        public class MemCell<T>: IMemCell {
            /// <summary> Buffer </summary>
            static T B;

            MemReg Reg;
            public readonly MyIniKey Key;

            /// <summary> Cell name </summary>
            public string N { get; }
            T v;
            /// <summary> Cell value </summary>
            public T V {
                get { return v; }
                set {
                    if (!value.Equals(v)) {
                        v = value;
                        Reg.Mem.Set(Key, Disp);
                        OS.ToSave();
                    }
                }
            }
            /// <summary> Displayed value </summary>
            public string Disp => v.Str();

            /// <remarks> Do not use it </remarks>
            public MemCell(MemReg reg, string n, T V) {
                Reg = reg;
                N = n;
                Key = new MyIniKey(reg.N, n);
                v = V;
                Reg.Mem.Set(Key, Disp);
            }

            const string S = "System.";
            delegate t conv<t>(string v);
            static void b<t>(string v, conv<t> c) {
                MemCell<t>.B = c(v);
            }
            /// <summary> Rewrite cell value from string </summary>
            /// <param name="V"> Value </param>
            public void Reset(string V) {
                var t = v.GetType().Str();
                switch (t) {
                    case S + "Boolean":
                        b(V, Convert.ToBoolean);
                        break;
                    case S + "Byte":
                        b(V, Convert.ToByte);
                        break;
                    case S + "Char":
                        b(V, Convert.ToChar);
                        break;
                    case S + "Decimal":
                        b(V, Convert.ToDecimal);
                        break;
                    case S + "Double":
                        b(V, Convert.ToDouble);
                        break;
                    case S + "Int16":
                        b(V, Convert.ToInt16);
                        break;
                    case S + "Int32":
                        b(V, Convert.ToInt32);
                        break;
                    case S + "Int64":
                        b(V, Convert.ToInt64);
                        break;
                    case S + "SByte":
                        b(V, Convert.ToSByte);
                        break;
                    case S + "Single":
                        b(V, Convert.ToSingle);
                        break;
                    case S + "String":
                        MemCell<string>.B = V;
                        break;
                    case S + "UInt16":
                        b(V, Convert.ToUInt16);
                        break;
                    case S + "UInt32":
                        b(V, Convert.ToUInt32);
                        break;
                    case S + "UInt64":
                        b(V, Convert.ToUInt64);
                        break;
                    default:
                        OS.Error(new Exception($"Wrong parsing type: '{t}' of value: '{V}'"));
                        break;
                }
                v = B;
            }

            public static implicit operator T(MemCell<T> v) => v.V;
            //public static implicit operator MemCell<T>(T v) => new MemCell<T>(v);
            public override string ToString() => Disp;
        }

        /// <summary> Rewrite register values from memory space </summary>
        /// <remarks> Do not use it </remarks>
        public void Reset() {
            var K = new List<MyIniKey>();
            Mem.GetKeys(K);
            foreach (var k in K) {
                var t = Reg.FirstOrDefault(c => k.Name == c.N);
                if (t != null) {
                    t.Reset(OS.GM.Get(k).Str());
                }
            }
        }

        /// <summary> Initilize memory cell </summary>
        /// <typeparam name="T"> Cell type </typeparam>
        /// <param name="n"> Name of cell </param>
        /// <param name="v"> Default value of cell </param>
        protected MemCell<T> GetMC<T>(string n, T v) {
            if (Reg.Any(c => c.N == n))
                OS.Error(new ArgumentException($"Register '{N}' in program '{Base.Name}' already contains parameter '{n}'"));
            var C = new MemCell<T>(this, n, v);
            Reg.Add(C);
            return C;
        }
    }

    interface IMemCell {
        /// <summary> Cell name </summary>
        string N { get; }
        /// <summary> Displayed value </summary>
        string Disp { get; }

        /// <summary> Reset value by current <see cref="MyIni">MyIni</see> </summary>
        /// <param name="V"></param>
        void Reset(string V);
    }

    //======-SCRIPT ENDING-======
}