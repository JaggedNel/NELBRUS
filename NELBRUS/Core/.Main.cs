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
using System.CodeDom;

// Ingame compiler version: 2.9.0.63208
// Ingame language version: C# 6
// #error version

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
        OS.Ready(this, 
            new NLB.SEcho() // Set your custom echo controller here
            );
    }
    /// <summary> Common SE method invoked on game world saving </summary>
    void Save() => OS.Save();
    /// <summary> Common SE method invoked on triggering programmable block </summary>
    /// <param name="arg"> Trigger argument </param>
    /// <param name="uT"> Type of trigger source </param>
    void Main(string arg, UpdateType uT) => OS.Main(arg, uT);

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
        /// <summary> Subprogram already started </summary>
        public const string mSPAS = "Subprogram '{0}' already started.";
        /// <summary> Subprogram terminated on starting </summary>
        public const string mSPTS = "Subprogram '{0}' can not start by cause:\n{1}";
        /// <summary> Subprogram terminated me </summary>
        public const string mSPTP = "Subprogram #{0} '{1}' terminated by cause:\n{2}";

        /// <summary> Base info for echo </summary>
        public const string mEB = "\nIs worked {0}\nInitialized subprograms: {1}\nRunned subprograms: {2}";

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

    /// #INSERT 1)Cmd
    /// #INSERT 2)MyVersion
    /// #INSERT 3)SubP
    /// #INSERT 4)InitSubP
    /// #INSERT 5)SdSubP
    /// #INSERT 6)SdSubPCmd
    /// #INSERT 7)TypedValue
    /// #INSERT 8)MemCell

    #endregion GlobalProperties

    /// #INSERT NLB

    /// #INSERT Ind

    #endregion CoreZone

    /// #ADDITIONS

}

/// <summary> Methods of code reduction </summary>
static class E {
    /// <summary> <see cref="object.ToString"/> </summary>
    public static string Str(this object o) => o.ToString();
    public static void AddRange<T>(this List<T> l, params T[] v) => l.AddRange(v);

    //======-SCRIPT ENDING-======
}