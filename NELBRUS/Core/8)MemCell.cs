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
        public readonly string Name;
        public List<IMemCell> Reg = new List<IMemCell>();

        public MemReg(string name = "Main") {
            Name = name;
        }

        public class MemCell<T>: IMemCell {
            /// <summary> Name </summary>
            public string N { get; }
            /// <summary> Value </summary>
            public T V;
            public string Value => V.Str();

            public MemCell(string name, T v) {
                N = name;
                V = v;
            }

            public static implicit operator T(MemCell<T> v) => v.V;
            //public static implicit operator MemCell<T>(T v) => new MemCell<T>(v);
            public override string ToString() => Value;
        }

        /// <summary> Initilize memory cell </summary>
        /// <typeparam name="T"> Cell type </typeparam>
        /// <param name="name"> Name of cell </param>
        /// <param name="v"> Default value of cell </param>
        protected MemCell<T> GetMC<T>(string name, T v) {
            var c = new MemCell<T>(name, v);
            Reg.Add(c);
            return c;
        }
    }

    interface IMemCell {
        /// <summary> Name </summary>
        string N { get; }
        string Value { get; }
    }

    //======-SCRIPT ENDING-======
}