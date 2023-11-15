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

    interface IReadable {
    }

    /// <summary> Typed value </summary>
    /// <typeparam name="T"> Type of stored value </typeparam>
    struct TV<T> : IReadable {
        /// <summary> Value </summary>
        public T V;
        
        public TV(T v) {
            V = v;
        }

        public override string ToString() => V.Str();
    }

    /// <summary> Typed delegate </summary>
    /// <typeparam name="T"> Type of delegate result </typeparam>
    struct TD<T> : IReadable {
        /// <summary> Delegate value </summary>
        public Func<T> V;

        public TD(Func<T> v) {
            V = v;
        }

        public override string ToString() => V?.Invoke().Str();
    }

    ///// <summary> Typed format </summary>
    //struct TF : IReadable {
    //    /// <summary> A composite format string </summary>
    //    public string F;
    //    /// <summary> An object array that contains zero or more objects to format </summary>
    //    public IReadable[] A;

    //    public TF(string f, params IReadable[] a) {
    //        F = f;
    //        A = a;
    //    }

    //    public override string ToString() => string.Format(F, A);
    //}

    //======-SCRIPT ENDING-======
}