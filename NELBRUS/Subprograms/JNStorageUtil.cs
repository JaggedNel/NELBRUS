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
using static System.Net.Mime.MediaTypeNames;

public partial class Program: MyGridProgram {
    //======-SCRIPT BEGINNING-======

    class StorageUtil: InitSubP {
        public StorageUtil() : base("StorageUtil", new MyVersion(1, 0)) { }

        protected override SdSubP Init(ushort id) => new TP(id, this);

        class TP: SdSubP {
            IMyTextSurface TextPanel;
            ActI MA;
            string LastStorage;
            StringBuilder content = new StringBuilder();

            public TP(ushort id, InitSubP p) : base(id, p) {
                TextPanel = OS.P.Me.GetSurface(0);
                TextPanel.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
                MA = AddAct(Main, 30);
            }

            public override void Init() {
                OS.Save();
                LastStorage = OS.P.Storage.Replace(((char)13).ToString(), "");
                TextPanel.WriteText(OS.P.Storage);
            }
            static int check = 0;
            void Main() {
                content.Clear();
                TextPanel.ReadText(content);
                var t1 = content.Str();
                string t = t1;
                var t2 = TextPanel.GetText();
                if (string.IsNullOrWhiteSpace(t1))
                    t = t2;
                t = t.Replace(((char)13).ToString(), "");

                //OS.P.Me.CustomData = t;

                //if (check++ >= 100)
                //    throw new Exception("check " + check + "\n" + TextPanel.GetText());
                if (t != LastStorage) {
                    List<string> ch;
                    OS.P.Storage = t;
                    OS.P.Me.CustomData = $"t1: {t1}\n" +
                        $"t2: {t2}\n\n" +
                        $"{t}\n{Compare(t, LastStorage, out ch)}\n"
                        + OS.P.Me.CustomData;
                    throw new Exception("Restart program block");
                }
                else
                    if (OS.P.Storage != t)
                        TextPanel.WriteText(OS.P.Storage = LastStorage = t);
            }
            string Compare(string s1, string s2, out List<string> changes) {
                changes = new List<string>();
                string res = "";
                string look = s1;
                string compar = s2;
                if (s1.Length != s2.Length)
                    res += $"Length {s1.Length}/{s2.Length}\n";
                if (s1.Length < s2.Length) {
                    look = s2;
                    compar = s1;
                }
                for (int i = 0; i < look.Length; i++) {
                    char c = look[i];
                    if (!compar.Contains(c))
                        changes.Add("Не содержит" + (ushort)c + " " + String.Format(@"\x{0:x4}", (ushort)c) + "\n");
                }
                res += "S1: " + String.Join(" ", s1.Select(c => (ushort)c + " " + String.Format(@"\x{0:x4}", (ushort)c))) + "\n";
                res += "S2: " + String.Join(" ", s2.Select(c => (ushort)c + " " + String.Format(@"\x{0:x4}", (ushort)c))) + "\n";
                res += "Ch: " + String.Join(" ", changes);
                return res;
            }
        }
    }
    StorageUtil iStorageUtil = new StorageUtil();

    //======-SCRIPT ENDING-======
}
/*

Program() {
        Me.CustomData = "";
        string temp = string.IsNullOrWhiteSpace(Storage) ? "clear" : Storage;
        Storage = ""; temp += "\n\n" + (string.IsNullOrWhiteSpace(Storage) ? "clear" : Storage);
        Echo(temp);
}
void Main(string arg, UpdateType uT) {}

*/