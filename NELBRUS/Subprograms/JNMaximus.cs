using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using VRageMath;

public partial class Program: MyGridProgram {
    //======-SCRIPT BEGINNING-======

    JNMaximus iJNMaximus = new JNMaximus();
    class JNMaximus: InitSubP {
        public JNMaximus() : base("Maximus Event Controller") { }

        public override SdSubP Start(ushort id) { return TP.GetInstance(id, this); }

        class TP: SdSubPCmd {
            static TP Instance;
            enum States {
                Stopped,
                Ready,
                Started,
                Playing,
            }
            enum ChamberTypes {
                Simple,
                Closable,
                Securable
            }

            class LogWriter {
                public IMyTextPanel LCDlog { get; private set; }
                public string Text { get; private set; }

                public LogWriter(string name) {
                    LCDlog = OS.GTS.GetBlockWithName(name) as IMyTextPanel;
                    Reboot();
                }

                public void Write(string message) {
                    Text = $"{message}\n{Text}";
                    LCDlog?.WriteText(Text);
                }
                public void Reboot() {
                    Text = "Log enabled";
                    LCDlog?.WriteText(Text);
                }
            }
            class Chamber {
                public enum States {
                    Undefined,
                    Closed,
                    Closing,
                    Opened,
                }

                public static readonly Color LightCommon = new Color(128, 255, 255);
                public static readonly Color LightWarn = new Color(255, 227, 26);
                public static readonly Color LightAlert = new Color(255, 0, 0);

                public readonly int ZoneNo;
                public readonly int ChamberNo;
                public States State { get; protected set; }
                protected IMyBlockGroup BlockGroup;
                public List<IMySensorBlock> Sensors { get; protected set; }
                protected List<IMyLightingBlock> Lights;
                ActI AAlert, AClose;

                public Chamber(int zone, int chamber) {
                    ZoneNo = zone;
                    ChamberNo = chamber;
                    Sensors = new List<IMySensorBlock>();
                    Lights = new List<IMyLightingBlock>();

                    BlockGroup = OS.GTS.GetBlockGroupWithName($"Зона-{zone}_Отсек-{chamber}");
                    if (BlockGroup == null)
                        throw new Exception($"Не найдена основная группа: зона {zone} отсек {chamber}");
                    BlockGroup.GetBlocksOfType(Sensors);
                    if (Sensors.Count < 3)
                        throw new Exception($"Не найдены или недостаточно сенсоров в зоне {zone} отсеке {chamber} - найдено {Sensors.Count}");
                    OS.GTS.GetBlockGroupWithName($"Зона-{zone}_Отсек-{chamber}_Лампы")?.GetBlocksOfType(Lights);
                    if (!Lights.Any())
                        throw new Exception($"Не найдена группа освещения: зона {zone} отсек {chamber}");

                    State = States.Undefined;
                }

                public virtual void Open() {
                    State = States.Opened;
                    Lights.ForEach(x => { x.Color = LightCommon; x.Enabled = true; });
                }
                public virtual void PrepearClose(uint warnTime, uint alertTime) {
                    State = States.Closing;
                    Instance.Log.Write($"Зона {ZoneNo} отсек {ChamberNo} начинает закрываться.");
                    Lights.ForEach(x => x.Color = LightWarn);
                    AAlert = Instance.AddAct(Alert, 0, warnTime);
                    AClose = Instance.AddAct(Close, 0, warnTime + alertTime);
                }
                protected virtual void Alert() {
                    Lights.ForEach(x => x.Color = LightAlert);
                }
                public virtual void Close() {
                    Lights.ForEach(x => { x.Enabled = false; x.Color = LightCommon; });
                }
                protected virtual void SetClose() {
                    State = States.Closed;
                    Instance.Log.Write($"Зона {ZoneNo} Отсек {ChamberNo} успешно закрыта.");
                    Instance.Seeker.DeclareClosing(this);
                    List<string> ClosedPlayers = new List<string>();
                    Sensors.ForEach(x => {
                        if (!x.LastDetectedEntity.IsEmpty() && !ClosedPlayers.Contains(x.LastDetectedEntity.Name))
                            ClosedPlayers.Add(x.LastDetectedEntity.Name);
                    });
                    if (ClosedPlayers.Any())
                        Instance.Log.Write($"В зоне {ZoneNo} отсеке {ChamberNo} {(ClosedPlayers.Count > 1 ? "заперты игроки" : "заперт игрок")}:\n\t{string.Join("\n\t", ClosedPlayers)}");
                }
            }
            class ChamberClosable: Chamber {
                protected struct CheckableDoor {
                    public IMyDoor Door;
                    private bool problem;
                    public bool Problem {
                        get {
                            return problem;
                        }
                        set {
                            if (!problem && value) {
                                problem = true;
                                Instance.Log.Write($"!Ошибка закрытия двери {Door.CustomName}!");
                            } else if (problem && !value) {
                                problem = false;
                                Instance.Log.Write($"Дверь {Door.CustomName} исправлена.");
                            }
                        }
                    }

                    public CheckableDoor(IMyDoor door) {
                        Door = door;
                        problem = false;
                    }
                }

                protected List<IMyDoor> Doors = new List<IMyDoor>();
                protected List<CheckableDoor> Doors2Close = new List<CheckableDoor>();
                ActI AControlDoors;

                public ChamberClosable(int zone, int chamber) : base(zone, chamber) {
                    OS.GTS.GetBlockGroupWithName($"Зона-{zone}_Отсек-{chamber}_Двери")?.GetBlocksOfType(Doors);
                    if (!Doors.Any())
                        throw new Exception($"Не найдены двери зоны {zone} отсек {chamber}");
                }

                public override void Open() {
                    base.Open();
                    Doors.ForEach(x => { x.Enabled = true; x.CloseDoor(); });
                }
                public override void Close() {
                    base.Close();
                    Doors2Close.Clear();
                    foreach (var i in Doors)
                        if (i.Status == DoorStatus.Closed)
                            i.Enabled = false;
                        else {
                            Doors2Close.Add(new CheckableDoor(i));
                            i.CloseDoor();
                        }
                    AControlDoors = Instance.AddAct(ControlDoors, 60, 59);
                }
                protected void ControlDoors() {
                    CheckableDoor temp;
                    int i = 0;
                    while (i < Doors2Close.Count) {
                        temp = Doors2Close[i];
                        if (temp.Door.Status == DoorStatus.Closing)
                            i++;
                        else if (Doors2Close[i].Door.Status == DoorStatus.Closed) {
                            temp.Door.Enabled = false;
                            temp.Problem = false;
                            Doors2Close.RemoveAt(i);
                        } else if (temp.Door.Status == DoorStatus.Opening || temp.Door.Status == DoorStatus.Open) {
                            if (!temp.Problem)
                                temp.Problem = true;
                            temp.Door.CloseDoor();
                            Doors2Close[i] = temp;
                            i++;
                        }
                    }
                    if (!Doors2Close.Any()) {
                        SetClose();
                        Instance.RemAct(ref AControlDoors);
                    }
                }
            }
            class ChamberSecurable: Chamber {
                protected List<IMyLargeTurretBase> Turrets = new List<IMyLargeTurretBase>();

                public ChamberSecurable(int zone, int chamber) : base(zone, chamber) {
                    OS.GTS.GetBlockGroupWithName($"Зона-{zone}_Отсек-{chamber}_Турели")?.GetBlocksOfType(Turrets);
                    if (!Turrets.Any())
                        throw new Exception($"Не найдены турели зоны {zone} отсек {chamber}");
                }

                public override void Open() {
                    base.Open();
                    Turrets.ForEach(x => x.Enabled = false);
                }
                public override void Close() {
                    base.Close();
                    Turrets.ForEach(x => x.Enabled = true);
                    SetClose();
                }
            }
            class PlayersSeeker {
                public enum PlayersCount {
                    Nobody,
                    Last,
                    Plenty,
                }
                PlayersCount State;
                List<Chamber> Chambers2Search = new List<Chamber>();
                int SearchCounter = 0;
                int FullSearchFreq = 2;
                IMyTextPanel PlayersShower;
                IMyBroadcastListener Listener;
                string ChanelTag;

                public PlayersSeeker() {
                    ChanelTag = "MaximusGame";
                    PlayersShower = OS.GTS.GetBlockWithName("LCDshower") as IMyTextPanel;
                    Listener = OS.P.IGC.RegisterBroadcastListener(ChanelTag);
                }

                List<string> FullSearch(bool includingClosed) {
                    var DetectedPlayers = new List<string>();
                    foreach (var x in Instance.Chambers)
                        foreach (var y in x) {
                            if (includingClosed || y.State == Chamber.States.Opened)
                                y.Sensors.ForEach(z => {
                                    if (!z.LastDetectedEntity.IsEmpty() && !DetectedPlayers.Contains(z.LastDetectedEntity.Name))
                                        DetectedPlayers.Add(z.LastDetectedEntity.Name);
                                });
                        }
                    return DetectedPlayers;
                }
                PlayersCount FastSearch(out string player) {
                    player = null;
                    var TempChambers = new List<Chamber>();
                    foreach (var i in Chambers2Search)
                        TempChambers.Add(i);

                    int SensorNo = 0;
                    int ChamberNo;
                    while (TempChambers.Any()) {
                        ChamberNo = 0;
                        while (ChamberNo < TempChambers.Count()) {
                            if (!TempChambers[ChamberNo].Sensors[SensorNo].LastDetectedEntity.IsEmpty())
                                if (player == null)
                                    player = TempChambers[ChamberNo].Sensors[SensorNo].LastDetectedEntity.Name;
                                else if (TempChambers[ChamberNo].Sensors[SensorNo].LastDetectedEntity.Name != player)
                                    return PlayersCount.Plenty;
                            if (SensorNo == TempChambers[ChamberNo].Sensors.Count() - 1)
                                TempChambers.Remove(TempChambers[ChamberNo]);
                            else
                                ChamberNo++;
                        }
                        SensorNo++;
                    }

                    if (player == null)
                        return PlayersCount.Nobody;
                    else
                        return PlayersCount.Last;
                }
                public void Reboot() {
                    if (Instance.State == States.Stopped)
                        return;
                    if (Instance.Chambers == null)
                        throw new Exception("Ошибка инициализации списка отсеков в системе слежения.");
                    State = PlayersCount.Nobody;
                    Chambers2Search.Clear();
                    for (int i = 0; i < Instance.Chambers.Count(); i++)
                        for (int j = 0; j < Instance.Chambers[i].Count(); j++)
                            if (Instance.Chambers[i][j].State != Chamber.States.Closed)
                                Chambers2Search.Add(Instance.Chambers[i][j]);
                }
                public void ManageSearch() {
                    if (SearchCounter++ % FullSearchFreq == 0)
                        SearchPlayers();
                    else
                        CheckPlayers();
                }
                public string SearchPlayers(bool includingClosed = false) {
                    var list = FullSearch(includingClosed);
                    string res;
                    if (list.Count == 0) {
                        res = "На поле никого не осталось!";
                        if (State != PlayersCount.Nobody) {
                            Instance.Log.Write(res);
                            State = PlayersCount.Nobody;
                        }
                    } else if (list.Count == 1) {
                        res = $"Остался последний игрок:\n\t{list[0]}";
                        if (State != PlayersCount.Last) {
                            Instance.Log.Write(res);
                            State = PlayersCount.Last;
                        }
                    } else {
                        res = $"На поле игроки:\n\t{string.Join("\n\t", list)}";
                        if (State != PlayersCount.Plenty) {
                            Instance.Log.Write(res);
                            State = PlayersCount.Plenty;
                        }
                    }
                    UpdatePlayersList(res);
                    return res;
                }
                public string CheckPlayers() {
                    string player;
                    string res = null;
                    var currentState = FastSearch(out player);
                    switch (currentState) {
                        case PlayersCount.Nobody:
                            if (State != PlayersCount.Nobody) {
                                State = PlayersCount.Nobody;
                                res = "На поле никого не осталось!";
                                UpdatePlayersList(res);
                                Instance.Log.Write(res);
                            }
                            break;
                        case PlayersCount.Last:
                            if (State != PlayersCount.Last) {
                                State = PlayersCount.Last;
                                res = $"Остался последний игрок:\n\t{player}";
                                UpdatePlayersList(res);
                                Instance.Log.Write(res);
                            }
                            break;
                        case PlayersCount.Plenty:
                            if (State != PlayersCount.Plenty) {
                                State = PlayersCount.Plenty;
                                res = "На поле множество игроков.";
                                Instance.Log.Write(res);
                            }
                            break;
                        default:
                            throw new Exception("Неожиданное состояние количества игроков.");
                    }
                    return res;
                }
                public void DeclareClosing(Chamber chamber) {
                    Chambers2Search.Remove(chamber);
                }
                void UpdatePlayersList(string list) {
                    PlayersShower?.WriteText(list);
                    OS.P.IGC.SendBroadcastMessage(ChanelTag, list);
                }
            }

            States State;
            List<IMyAssembler> RespawnPoints = new List<IMyAssembler>();
            List<IMyGravityGenerator> GravityGeneratorsBase = new List<IMyGravityGenerator>();
            List<IMyGravityGenerator> GravityGeneratorsReverse = new List<IMyGravityGenerator>();
            List<IMyDoor> Doors = new List<IMyDoor>();

            readonly int[] ChambersCount = new int[] { 8, 8, 1, 1 };
            readonly ChamberTypes[] ChambersType = new ChamberTypes[] { ChamberTypes.Closable, ChamberTypes.Closable, ChamberTypes.Securable, ChamberTypes.Simple };
            readonly uint[] TimeWarn = new uint[] { NLB.F.TTT(50, 0), NLB.F.TTT(50, 0), NLB.F.TTT(50, 0), NLB.F.TTT(0, 15) }; // Сколько горит жёлтым
            readonly uint[] TimeAlert = new uint[] { NLB.F.TTT(10), NLB.F.TTT(10), NLB.F.TTT(10), NLB.F.TTT(10) }; // Сколько горит красным
            readonly uint TimeCloseBreak = NLB.F.TTT(0, 1); //Перерыв между закрытием отсеков
            readonly uint TimeOpenThird = NLB.F.TTT(0, 2); // Время до открытия отсека третьей зоны
            readonly uint TimeRespawnsOff = NLB.F.TTT(0, 2); // Через сколько отключатся респавны
            readonly uint TimeFirstClose = NLB.F.TTT(30, 2); // Время до первого выбора отсека для закрытия
            readonly uint TimeFirstGravityChanging = NLB.F.TTT(0, 1); // Время до первой смены гравитации после выбора отсека на закрытие
            readonly uint TimeChangeGravityFreq = NLB.F.TTT(0, 1); // Частота смены гравитации

            readonly uint[] ClosingChambersCount = new uint[] { 2, 2, 1, 1 }; // Количество одновременно закрываемых отсеков


            static Random rng = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            int[][] CloseQueue;
            int CurZone;
            int CurChamber;

            Chamber[][] Chambers;
            LogWriter Log;
            PlayersSeeker Seeker;
            ActI ACheckRespawns, APlay, ACheckPlayers;
            ChamberClosable ThirdZoneChamber;
            ActI AOpenThird;
            bool GravityDirectionNormal = true;
            ActI AChangeGravity;

            TP(ushort id, SubP p) : base(id, p) {
                State = States.Stopped;

                OS.GTS.GetBlockGroupWithName("Точки возрождения")?.GetBlocksOfType(RespawnPoints, x => x.GetType().Name == "MySurvivalKit");
                if (!RespawnPoints.Any())
                    throw new Exception("Не найдены точки возрождения.");
                OS.GTS.GetBlockGroupWithName("Гравитационные генераторы (down)")?.GetBlocksOfType(GravityGeneratorsBase);
                if (!GravityGeneratorsBase.Any())
                    throw new Exception("Не найдены генераторы гравитации (down).");
                OS.GTS.GetBlockGroupWithName("Гравитационные генераторы (up)")?.GetBlocksOfType(GravityGeneratorsReverse);
                if (!GravityGeneratorsReverse.Any())
                    throw new Exception("Не найдены генераторы гравитации (up).");
                OS.GTS.GetBlockGroupWithName("Двери")?.GetBlocksOfType(Doors);
                if (!Doors.Any())
                    throw new Exception("Не найдена общая группа дверей.");

                Log = new LogWriter("LCDlog");
                Chambers = new Chamber[ChambersCount.Length][];
                for (int i = 0; i < ChambersCount.Length; i++) {
                    Chambers[i] = new Chamber[ChambersCount[i]];
                    for (int j = 0; j < ChambersCount[i]; j++) {
                        switch (ChambersType[i]) {
                            case ChamberTypes.Closable:
                                Chambers[i][j] = new ChamberClosable(i + 1, j + 1);
                                break;
                            case ChamberTypes.Securable:
                                Chambers[i][j] = new ChamberSecurable(i + 1, j + 1);
                                break;
                            case ChamberTypes.Simple:
                                Chambers[i][j] = new Chamber(i + 1, j + 1);
                                break;
                            default:
                                throw new Exception("Неожиданный тип отсека.");
                        }
                    }
                }
                ThirdZoneChamber = new ChamberClosable(3, 1);

                Seeker = new PlayersSeeker();
                ACheckRespawns = AddAct(CheckRespawns, NLB.F.TTT(3));

                SetCmd(new Dictionary<string, Cmd>
                {
                    { "Play", new Cmd(CmdStartEvent, "Начать событие.")},
                    { "Stop", new Cmd(CmdStopEvent, "Остановить событие.")},
                    { "Reboot", new Cmd(CmdReboot, "Произвести перенастройку систем. Рекомендуется перед каждым запуском.")},
                    { "Search", new Cmd(CmdSearch, "Поиск игроков на карте.")},
                    { "Check", new Cmd(CmdCheck, "Проверить количество игроков.")},
                });
            }
            public static TP GetInstance(ushort id, SubP p) {
                return Instance ?? (Instance = new TP(id, p));
            }

            void CheckRespawns() {
                if (State == States.Started)
                    RespawnPoints.ForEach(x => x.Enabled = true);
                else
                    RespawnPoints.ForEach(x => x.Enabled = false);
            }
            void StartEvent() {
                State = States.Started;
                CurZone = 0;
                CurChamber = 0;
                CloseQueue = new int[ChambersCount.Length][];
                for (int i = 0; i < ChambersCount.Length; i++) {
                    CloseQueue[i] = new int[ChambersCount[i]];
                    for (int j = 0; j < ChambersCount[i]; j++)
                        CloseQueue[i][j] = j;
                    Shuffle(CloseQueue[i]);
                }
                RespawnPoints.ForEach(x => x.Enabled = true);

                AOpenThird = AddAct(ThirdZoneChamber.Open, 0, TimeOpenThird);
                APlay = AddAct(CloseNewChamber, 0, TimeFirstClose);
                ACheckPlayers = AddAct(StartChecking, 0, TimeRespawnsOff);
                AChangeGravity = AddAct(FirstGravityChange, 0, TimeFirstClose + TimeFirstGravityChanging);
            }
            public static void Shuffle<T>(IList<T> list) {
                int n = list.Count;
                while (n > 1) {
                    n--;
                    int k = rng.Next(n + 1);
                    T value = list[k];
                    list[k] = list[n];
                    list[n] = value;
                }
            }
            void CloseNewChamber() {
                bool c = true;
                var temp = TimeWarn[CurZone] + TimeAlert[CurZone];
                for (int i = 0; i < ClosingChambersCount[CurZone]; i++) {
                    Chambers[CurZone][CloseQueue[CurZone][CurChamber]].PrepearClose(TimeWarn[CurZone], TimeAlert[CurZone]);
                    if (++CurChamber >= ChambersCount[CurZone]) {
                        CurChamber = 0;
                        if (++CurZone >= ChambersCount.Length) {
                            CurZone = 0;
                            c = false;
                        }
                        break;
                    }
                }
                if (c)
                    APlay = AddAct(CloseNewChamber, 0, temp + TimeCloseBreak);
            }
            void FirstGravityChange() {
                AChangeGravity = AddAct(ChangeGravity, TimeChangeGravityFreq, 0);
            }
            void ChangeGravity() {
                GravityDirectionNormal = !GravityDirectionNormal;
                Instance.GravityGeneratorsBase.ForEach(x => x.Enabled = GravityDirectionNormal);
                Instance.GravityGeneratorsReverse.ForEach(x => x.Enabled = !GravityDirectionNormal);
            }
            void StartChecking() {
                ACheckPlayers = AddAct(Seeker.ManageSearch, NLB.F.TTT(5), 29);
                RespawnPoints.ForEach(x => x.Enabled = false);
                State = States.Playing;
            }
            void Reboot() {
                ClearActs();
                State = States.Ready;
                ACheckRespawns = AddAct(CheckRespawns, NLB.F.TTT(3), 1);
                foreach (var x in Chambers)
                    foreach (var y in x)
                        y.Open();
                ThirdZoneChamber.Close();
                Doors.ForEach(x => x.CloseDoor());
                RespawnPoints.ForEach(x => x.Enabled = false);
                GravityGeneratorsBase.ForEach(x => x.Enabled = true);
                GravityGeneratorsReverse.ForEach(x => x.Enabled = false);

                Log.Reboot();
                Seeker.Reboot();
            }

            #region commands
            string CmdStartEvent(List<string> _) {
                if (State == States.Ready) {
                    StartEvent();
                    return "Событие запущено!";
                } else if (State == States.Started || State == States.Playing)
                    return "Игра уже идёт!";
                else if (State == States.Stopped) {
                    Reboot();
                    StartEvent();
                    return "Событие запущено с перезагрузкой!";
                } else
                    throw new Exception("Неожиданное состояние игры при запуске.");
            }
            string CmdStopEvent(List<string> _) {
                if (State == States.Playing || State == States.Started) {
                    ClearActs();
                    ACheckRespawns = AddAct(CheckRespawns, NLB.F.TTT(3));
                    return "Событие остановлено!";
                } else
                    return "Событие не запущено.";
            }
            string CmdReboot(List<string> _) {
                Reboot();
                return "Настройки перезагружены!";
            }
            string CmdSearch(List<string> _) {
                return Seeker.SearchPlayers();
            }
            string CmdCheck(List<string> _) {
                return Seeker.CheckPlayers();
            }
            #endregion commands
        }
    }

    //======-SCRIPT ENDING-======
}

