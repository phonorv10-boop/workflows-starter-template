// ROUTER_v44 CHANGELOG (surviving behaviour only). STATUS: SOURCE CHANGED / NOT COMPILED / NOT RUNTIME-TESTED.
// v44: LiteDbDir is built from NinjaTrader.Core.Globals.UserDataDir (sounds\LOGS\ROUTER\LITEDB) instead of a hardcoded C:\Users\nanup path.
//      Still a fixed path: prasan.config is not read for it (v28 lock unchanged). OpenDb adds a guard: if the resolved database file does not
//      exist but the old hardcoded one does (and they differ), it logs STARTUP_ERROR | LITEDB_PATH_MOVED and throws, so ROUTER never starts
//      on an empty store by accident. On the original machine the resolved path equals the old one.
//      Also: the owner-path ROUTER_UNLOADED line (ShutdownTail) now carries shellId, state, reachedDataLoaded and Role=OWNER, like the REJECTED line. Logging only.
// ROUTER_v43 CHANGELOG (surviving behaviour only). STATUS: SOURCE CHANGED / NOT COMPILED / NOT RUNTIME-TESTED.
// v43: logging only. Each indicator object gets a shellId at creation, before DataLoaded. The unload lines (ROUTER_UNLOADED, Terminated)
//       now name which object unloaded: shellId, instanceId (blank if DataLoaded never ran), state, whether it reached DataLoaded,
//       and the current owner. No logic change.
// ROUTER_v42 CHANGELOG (surviving behaviour only). STATUS: SOURCE CHANGED / NOT COMPILED / NOT RUNTIME-TESTED.
// v42: diagnostic only. InsertLive logs LIVE_OI_VARIANT when a live row matches an earlier live row on time, price, volume, bid and ask
//       but has a different OpenInterest (the case that the OI-inclusive live key lets through, e.g. SeqNum 6363/6364 on 2026OCT19).
//       The row is handled exactly as in v41. No dedupe, store, bus or counter change. Logs capped at 100 per session.
// ROUTER_v41 CHANGELOG (surviving behaviour only). STATUS: SOURCE CHANGED / NOT COMPILED / NOT RUNTIME-TESTED.
// v41: historical rewrite is append-only. No DeleteMany and no SeqN renumber on attach. A native trade is inserted only when no active
//       stored row (LIVE or HIST) has the same exact key (EventTime.Ticks, tick-grid price, volume; OI excluded), counted per key so that
//       genuine repeated prints are kept. Natives covered by the LIVE tolerance pairing (PlanPhaseCarry) are never inserted. Sequence numbers
//       continue above the store maximum. Unpaired stored rows are kept and counted (stored_unpaired). Live path unchanged. The short-read
//       guard was removed because it protected deletes, which no longer happen.
// ROUTER_v40 CHANGELOG (surviving behaviour only). STATUS: SOURCE ONLY / NOT COMPILED / NOT RUNTIME-TESTED.
// v40: NO time-based retention. Tick and minute rows are never deleted or pruned for age.
//      Removed: TickRetentionDays, MinuteRetentionDays, PruneEnabled, PruneStore (and its PRUNE stage and the call at roll).
//      Replay window (BusRebuild), LITEDB window counter (FinalizeDayState) and the diagnostic walk (ReclassifyStoreNeutral) cover every stored day.
//      Log placement (which folder a tick log lives in, ExportPast, RelocateLogs) is not retention and is unchanged.
// ROUTER_v39 CHANGELOG (surviving behaviour only). Build identity: RouterBuildId.Id and BannerFull.
// v39: (1) RouterBuildId.Id names this build (was the v32 string). (2) AppendMainSilent (used by LogEvent) has the same null-logPath guard as AppendMainLocked.
//      (3) ASSUMPTIONS block matches the code: NEUTRAL (v35: never reclassified, never inherited), OI (REC41: Volume field only, no Price fallback), numbered 1-8.
//      (4) Removed no-op code: the RECLASSIFY_NEUTRAL stage (ReclassifyNeutralHistToday only rebuilt counters, and CLASS_REBUILD_COUNTERS rebuilds them next) and the
//      empty transaction in ReclassifyStoreNeutral (its CLASS_INVARIANT diagnostics are kept). (5) Removed the unreachable TIMER_0900 roll branch in DashboardTick (v37: FIRST_TICK only).
//      (6) Stale comments corrected (pruning, orphan add-in comments, NEUTRAL inheritance). No change to stored data, classification, bus rows or the live path.
// v38: (1) log lines written before the first rotation go to the calendar-day MAIN file (no longer lost; was ArgumentNullException on logPath).
//      (2) stale REC28 comment corrected: NEUTRAL is not inherited (v35). (3) empty no-op loops removed from the retention walk and REC08 pass.
// ROUTER_v37 CHANGELOG (surviving behaviour only).
// v37: no clock roll at 09:00. The session day changes only on the first live tick of the new trading day (OnMarketDataBody -> RollAsyncOrSkip FIRST_TICK),
//      so the dashboard counters reset when live ticks arrive. The TIMER_0900 branch was removed in v39 (it was unreachable); nothing else in the roll path changed.
// ROUTER_v36 / v35 CHANGELOG (surviving behaviour only).
// v36: silent failures reported. Log / LogEvent / Debug write failures are counted and printed to the Output window (LOG_WRITE_FAIL, first 20).
//      Checkpoint flush at session roll (CHECKPOINT_ROLL_FLUSH_FAIL), post-insert repair (POST_INSERT_REPAIR_FAIL) and session-stop writes are now logged.
//      Temp-file cleanup catches stay silent by design: the write failure that caused them is already logged.
// v35: NEUTRAL is never given a side. A tick with no valid classification stays NEUTRAL. Removed: live inheritance (ProcessLive),
//      history inheritance (HIST_CLASSIFY), the retention-walk NEUTRAL reclassify (first-row price rule and carry-forward), and the
//      REC08 carry-forward (its method was deleted in v39). NEUTRAL rows are published to the bus as NEUTRAL and do not enter CumDelta as a side.
//      Previous-TICK price reference is unchanged and still used where a row has no stored direction (BackfillDirectionToday, ClassifyTick).
// v34: VECTOR bus replay is a chronological merge of MINUTE and TICK rows by EventTime, per trading day, ascending days.
//      Tie-break at equal EventTime: TICK before MINUTE; within a type, EventTime then Id. Replaces minute-then-tick block order.
//      A bus slot with a different layout is NOT replaced and this instance does NOT publish. Startup logs STARTUP_ERROR | LAYOUT_MISMATCH and auto-stops.
//      Rebuild publish failures (ticks and minutes) are counted in busPublishFails and the first 20 are logged.
// v33: OI fix: an unknown OI (zero field) no longer overwrites a carried valid OI after the seed is used up (ProcessLive OI handler). Minute publish failures are now counted in busPublishFails. No other logic change.
// v32: dead-code and comment cleanup of v31 only: eight unused private members removed, changelog condensed. No logic change.
// v31: ReloadDayState rebuilds the key sets and reconciles the key quarantine under ONE dbGate hold (order dbGate -> keyLock, as InsertLive), so a startup retry that runs attach with liveReady already true cannot race a live insert.
// v30: KEYSET recovery after a stored live insert: one retry of the key-set add, verified with Contains; if the identity is still absent it is held in a fixed 64-entry quarantine and InsertLive treats it as already stored.
//      Quarantine full = live inserts fail closed (Ins.Failed, nothing written) until the key sets are rebuilt from the store. Reconciled at RebuildKeysFromStore / ReloadDayState, cleared at the session roll.
//      InsertCore reports whether every post-insert step completed: Ins.Inserted, or Ins.InsertedPartial (row stored, a post-insert step failed). Both are handled as a stored row.
// v29: live path: each post-insert step (tail, key set, checkpoint, counters, bus) is isolated (LIVE_POST_INSERT_PARTIAL); no transaction, no rollback. EnqueueLive queue ownership is decided by QueueCharge under lock(rec), so liveQueued is released exactly once.
// v28: the database opens ONLY at the fixed RouterConfig path (litedb.dir / litedb.path in prasan.config are not read); test mode removed; StartLiveWriter publishes the queue and writer only after the thread has started.
// v26: diagnostic ledger: counters on every OnMarketData callback and accepted-tick outcome; a LEDGER line per TICK_FLOW window.
// v25: FLOOR line on the dashboard and floor= in TICK_FLOW (liveFloorDropped); LIVE_DB_NULL_DROP written to MAIN, first 50 per run.
// v24: MISSED and LIVE are recounted from the stored rows of the session day on every attach, before the non-trading write guard (read-only).
// v23: a live tick dropped because the database handle is null is counted in liveDroppedCount and logged.
// v22: the early floor drop in ProcessLive counts liveReceived, so every tick that reaches ProcessLive is counted once.
// v21: the history-rewrite floor (liveFloorTicks) is set inside lock (dbGate) after the rewrite commit; InsertLive re-checks it under dbGate (FloorDrop, BOUNDARY_DROP | path=INSERT_LOCKED). The early ProcessLive check stays as a fast exit.
// v18: ProcessLive drops live rows at or before liveFloorTicks, so ticks buffered before the floor was set cannot duplicate history (BOUNDARY_DROP | path=PROCESS_LIVE).
// v17: BusRebuild publishes minute rows with Phase "MINUTE" (bus only).
// v16: OI carry-forward: the last valid OI is seeded from the store on attach and roll (OI_SEED) and replaced by the first live valid OI (OI_LIVE_RESUMED).
// v15: PREV_SEED: the history classifier starts from the previous stored close. PREV_CLOSE_DATE: the previous-close lookup matches the calendar day.
// v13: PAST_DAY_FROZEN: NATIVE_HISTORY does not rewrite a session day that is before today.
// v11: PHASE_MATCH: native history vs LIVE matching (PlanPhaseCarry) uses a 2-second tolerance (PhaseMatchToleranceMs), same price and volume, one-to-one, nearest in time. DIAG_PAIRING writes ROUTER_DIAG_NATIVE_<day>.csv and ROUTER_DIAG_LIVE_<day>.csv at each attach.
// v10: prev-day prevClose from stored days; ACCOUNT_UNRESOLVED verdict; STARTUP_ABORTED log on owner loss; holiday override + HOLIDAY_SPARSE warning; EOD_DEDUPE attribution.
// v09: post-close quiet mode, FIRST_TRADE_NO_PREV_DIAG, QUOTE_DIAG label.
// v06: RollSession zeroes the per-day dashboard lot counters (MISSED, LIVE, VECTOR today); the retention-window totals and the VECTOR bus are not reset at roll.
// v05: native 1-Tick history plus NinjaTrader live ticks, one insert path, VECTOR bus PRASAN_BUS_V3. Phase values: LIVE (live tick) and HIST (historical or recovered tick).
#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Indicators;
#endregion

// ========================================================================================
// DO NOT REMOVE THIS BLOCK. Build identity: BannerFull and BannerDate (below) are shown on the dashboard FILENAME line
// and in the _LOADED log line. The file name is the version label.
//
// PURPOSE     : Native 1-Tick history + NIN live -> one insert path
// =========================================================================================
// =========================================================================================
// ASSUMPTIONS (stated, not enforced in code). If one stops holding, the named behaviour is wrong; the code does not detect it.
//  1. NEUTRAL (v35). A tick with no valid classification stays NEUTRAL, in the store and on the bus. It is never given a side, never inherited from an
//     earlier row and never reclassified later (live, native history and the retention walk all keep the stored direction). NEUTRAL rows do not enter
//     CumDelta as a side. It occurs when no previous close is available (FIRST_TRADE_NO_PREV), when the price equals its reference (previous close, previous
//     tick or exact quote mid), or when the quote sizes do not establish a side. Native history rows carry no quotes: price vs previous tick only.
//  2. The PC clock and time zone are IST. Every session time (09:00 roll, 09:15 open, 15:40 close, 15:45 EOD, 15:35 final heal) is compared
//     against DateTime.Now / e.Time as local wall-clock time, and the cut-offs are fixed constants, not read from the exchange calendar.
//  3. Trading days = Monday-Friday minus the NT8 Trading Hours template holidays (read once at DataLoaded). Muhurat / special sessions
//     and exchange schedule changes are not known to the code.
//  4. Instrument is a NIFTY future whose Last tick Volume is raw contracts and whose LotSize (indicator property) matches the exchange lot.
//     Lots are volume / LotSize with integer division, so a volume that is not a multiple of LotSize is truncated per use.
//  5. A NIN row is identified by (EventTime ticks, Price, Volume, OI). Two genuine NIN prints with the same four values are stored once.
//     Cross-phase matching (live vs native HIST) uses NoOiKey (EventTime ticks, Price, Volume), not the OI-inclusive key.
//  6. The DailyVolume event is the exchange day total; the OpenInterest event carries OI in e.Volume ONLY (REC41: no Price fallback). A zero or missing
//     Volume field is UNKNOWN (-1) and never overwrites a carried valid OI. The field is logged (OI_FIELD_DIAG / OI_FIELD_SOURCE) but not confirmed.
//  7. The BidQty / AskQty units: confirmed as raw contracts against one pre-market sample (bidQty=650, askQty=780 on 2026OCT07).
//     Live-session sizes are not separately verified. The comparison tradeVol >= askQty / tradeVol >= bidQty assumes both sides are
//     raw contracts.
//  8. Dashboard price direction is last tick price vs first tick price of the session day, over native HIST and LIVE rows together, by EventTime.
// =========================================================================================
// =========================================================================================

namespace NinjaTrader.NinjaScript.Indicators.PRASAN
{
    // ---------- LiteDB record types ----------
    public class MinuteRecord
    {
        public long Id { get; set; }
        public DateTime EventTime { get; set; }
        public DateTime Day { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public long Volume { get; set; }
        public long OpenInterest { get; set; }
    }

    public class TickRecord
    {
        public long Id { get; set; }
        public DateTime EventTime { get; set; }
        public DateTime Day { get; set; }
        public double Price { get; set; }
        public long Volume { get; set; }
        public long OpenInterest { get; set; }
        public string Phase { get; set; }
        public double? BidPrice { get; set; }    // quote used at classification, null = none (never fabricated)
        public double? AskPrice { get; set; }
        public string Direction { get; set; }    // "BUY" | "SELL" | "NEUTRAL" | null (legacy)
        public long DayVolume { get; set; }      // exchange DayVolume captured with a NIN tick, -1 / 0 = unknown
        // STD01: SeqNum lives in LiteDB and the checkpoint only. It is NOT a bus field and NOT a CSV column.
        public string SeqNum { get; set; }       // "OCT04-001" (R1)
        public long SeqN { get; set; }           // numeric part of SeqNum, 0 = legacy row without a SeqNum
        public bool Superseded { get; set; }
        // v26-DIAG: in-memory only. [BsonIgnore] is NOT VERIFIED for the referenced LiteDB version (gate S4).
        [LiteDB.BsonIgnore] public bool Accepted { get; set; }     // set by LedgerAccept
        [LiteDB.BsonIgnore] public bool Recorded { get; set; }     // a pipeline terminal has been recorded
        [LiteDB.BsonIgnore] public int  TerminalCode { get; set; } // P_* code of the recorded terminal
        [LiteDB.BsonIgnore] public int  Stage { get; set; }        // 0 none, 1 tickCol.Insert entered, 2 Insert returned
        // ROUTER_v29: in-memory only (same BsonIgnore caveat as above). Queue ownership, changed only under lock(record):
        // 0 never queued, 1 counted in liveQueued (queued, or TryAdd outcome not yet known), 3 taken by the writer,
        // 2 released (liveQueued decremented), 4 revoked by the producer after TryAdd threw (writer must skip it).
        [LiteDB.BsonIgnore] public int  QueueCharge { get; set; }
    }

    // no marker is treated as incomplete and refetched (once per attach, existing historyRetried guard).


    // ---------- VECTOR bus (Model A, WRITE SIDE ONLY) ----------
    public struct RouterTick
    {
        public DateTime EventTime;
        public double   Price;
        public long     Volume;
        public long     OpenInterest; // -1 if unknown
        public string   Direction;    // "BUY" | "SELL" | "NEUTRAL" | "UNKNOWN"
        public string   Phase;        // "LIVE" | "HIST"
    }

    // BUS CONTRACT (unchanged; any reader, e.g. PLOTTER, copies this layout exactly):
    //   slot name  : "PRASAN_ROUTER_BUS"   (AppDomain.CurrentDomain.GetData)
    //   value      : object[9]
    //     [0] string         layout tag "PRASAN_BUS_V3"
    //     [1] object         lock gate: lock(bus[1]) around every read/write of [2]..[7]
    //     [2] List<DateTime> EventTime
    //     [3] List<double>   Price
    //     [4] List<long>     Volume
    //     [5] List<long>     OpenInterest (-1 if unknown)
    //     [6] List<string>   Direction ("BUY"|"SELL"|"NEUTRAL"|"UNKNOWN")
    //     [7] List<string>   Phase ("LIVE"|"HIST")
    //     [8] long[4]        counters: [0]=published [1]=publishedLive [2]=plotterConsumed [3]=replayDone
    // ROUTER writes c[0], c[1], c[3]. c[2] belongs to PLOTTER: ROUTER only zeroes it at ATTACH (bus clear),
    // never reads it, never touches it on a rebuild.
    public static class VECTOR
    {
        private const string SlotName = "PRASAN_ROUTER_BUS";
        private const string Layout   = "PRASAN_BUS_V3";
        private static readonly object initGate = new object();
        private static volatile object[] bus;
        private static string slotState = "NOT_OPENED";
        private static volatile bool layoutMismatch = false;
        private static string mismatchDetail = "";

        private static object[] Bus()
        {
            object[] b = bus;
            if (b != null) return b;
            lock (initGate)
            {
                if (bus != null) return bus;
                object[] existing = AppDomain.CurrentDomain.GetData(SlotName) as object[];
                if (existing != null && existing.Length == 9 && (existing[0] as string) == Layout)
                {
                    long[] oc = existing[8] as long[];
                    if (oc == null || oc.Length != 4)
                    {
                        long[] nc = new long[4];
                        if (oc != null) Array.Copy(oc, nc, Math.Min(oc.Length, 3));
                        lock (existing[1]) { existing[8] = nc; }
                    }
                    bus = existing;
                    slotState = "REUSED";
                }
                else if (existing != null)
                {
                    // v34: a slot with a different layout belongs to another build. It is not replaced (its readers would lose their list),
                    // and this instance does not publish (Publish returns false). START logs STARTUP_ERROR and auto-stops the instance.
                    layoutMismatch = true;
                    mismatchDetail = "slotLayout=" + (existing.Length > 0 ? (existing[0] as string ?? "null") : "empty")
                        + " | slotLength=" + existing.Length + " | expected=" + Layout + " | expectedLength=9";
                    bus = NewBus();
                    slotState = "LAYOUT_MISMATCH";
                }
                else
                {
                    bus = NewBus();
                    AppDomain.CurrentDomain.SetData(SlotName, bus);
                    slotState = "CREATED";
                }
                return bus;
            }
        }

        private static object[] NewBus()
        {
            return new object[]
            {
                Layout, new object(),
                new List<DateTime>(), new List<double>(), new List<long>(), new List<long>(),
                new List<string>(), new List<string>(),
                new long[4]
            };
        }

        // Opens the slot (creates it if missing) and returns CREATED | REUSED | LAYOUT_MISMATCH.
        public static string Init() { Bus(); return slotState; }

        // v34: true when the shared slot exists with an incompatible layout. Publish is refused while this is set.
        public static bool LayoutMismatch { get { return layoutMismatch; } }
        public static string MismatchDetail { get { return mismatchDetail; } }

        // Counter reads take the same lock gate as the writers (Publish / Clear), so a read never races a c[] update.
        public static long PublishedCount     { get { object[] b = Bus(); lock (b[1]) { return ((long[])b[8])[0]; } } }
        public static long PublishedLiveCount { get { object[] b = Bus(); lock (b[1]) { return ((long[])b[8])[1]; } } }

        // Append one row to the six lists under the lock gate; c[0]++ and, for LIVE rows, c[1]++.
        public static bool Publish(RouterTick t)
        {
            if (layoutMismatch) return false;   // v34: never publish to a private bus that PLOTTER cannot read
            try
            {
                object[] b = Bus();
                lock (b[1])
                {
                    ((List<DateTime>)b[2]).Add(t.EventTime);
                    ((List<double>)b[3]).Add(t.Price);
                    ((List<long>)b[4]).Add(t.Volume);
                    ((List<long>)b[5]).Add(t.OpenInterest);
                    ((List<string>)b[6]).Add(t.Direction);
                    ((List<string>)b[7]).Add(t.Phase);
                    long[] c = (long[])b[8];
                    c[0]++;
                    if (t.Phase == "LIVE") c[1]++;
                }
                return true;
            }
            catch { return false; }
        }

        // Clears the six lists, c[0], c[1] and c[3] (replayDone back to 0).
        // atAttach=true  : also zeroes c[2] (a fresh attach starts the read side over).
        public static void Clear(bool atAttach)
        {
            object[] b = Bus();
            lock (b[1])
            {
                ((List<DateTime>)b[2]).Clear();
                ((List<double>)b[3]).Clear();
                ((List<long>)b[4]).Clear();
                ((List<long>)b[5]).Clear();
                ((List<string>)b[6]).Clear();
                ((List<string>)b[7]).Clear();
                long[] c = (long[])b[8];
                c[0] = 0; c[1] = 0; c[3] = 0;
                if (atAttach) c[2] = 0;
            }
        }

        // c[3]: 0 = replay / rebuild not finished, 1 = finished (bus safe to read from index 0).
        public static void SetReplayDone(bool done)
        {
            object[] b = Bus();
            lock (b[1]) { ((long[])b[8])[3] = done ? 1 : 0; }
        }
    }

    // ---------- Config ----------
    // prasan.config reader: key=value pairs, read once on first access. Missing file/key -> the default.
    internal static class Cfg
    {
        private static readonly Dictionary<string, string> values = Load();

        private static Dictionary<string, string> Load()
        {
            var d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                string path = Path.Combine(NinjaTrader.Core.Globals.UserDataDir, "PRASAN_DATA", "CONFIG", "prasan.config");
                if (!File.Exists(path)) return d;
                foreach (string raw in File.ReadAllLines(path))
                {
                    string line = raw.Trim();
                    if (line.Length == 0 || line.StartsWith("#") || line.StartsWith(";")) continue;
                    int eq = line.IndexOf('=');
                    if (eq <= 0) continue;
                    string key = line.Substring(0, eq).Trim();
                    string val = line.Substring(eq + 1).Trim();
                    if (key.Length > 0) d[key] = val;
                }
            }
            catch { }
            return d;
        }

        public static string Read(string key, string fallback)
        {
            string v;
            if (!values.TryGetValue(key, out v)) return fallback;
            if (string.IsNullOrWhiteSpace(v)) return fallback;
            return v;
        }
    }

    // BUILD-ID (diagnostic only). A compile-time constant: no code path reads it and nothing executes it.
    // Its only use is to identify this exact compiled candidate by searching NinjaTrader.Custom.dll for the string.
    internal static class RouterBuildId
    {
        public const string Id = "ROUTER_V44";
    }

    internal static class RouterConfig
    {
        public static readonly string RootDir   = Path.Combine(NinjaTrader.Core.Globals.UserDataDir, "PRASAN_DATA");
        public static readonly string ConfigDir = Path.Combine(RootDir, "CONFIG");


        // Marker folder. STD01 keeps exactly TWO persistent markers here (R20):
        //   ROUTER_CHECKPOINT_<day>.marker   and   ROUTER_LIFECYCLE.marker
        public static readonly string RouterDataDir = Path.Combine(ConfigDir, "ROUTER");

        // ROUTER_v28 DB LOCK: the store is opened ONLY at this fixed path. prasan.config (litedb.dir / litedb.path) is not read,
        // so no config value can send the database anywhere else.
        // v44: built from the NinjaTrader user-data folder, not a literal user name. Fixed relative layout, no config override.
        public static readonly string LiteDbDir  = Path.Combine(NinjaTrader.Core.Globals.UserDataDir, "sounds", "LOGS", "ROUTER", "LITEDB");
        // v44: the pre-v44 hardcoded location, used only by the OpenDb guard (LITEDB_PATH_MOVED).
        public const string LegacyLiteDbPath = @"C:\Users\nanup\Documents\NinjaTrader 8\sounds\LOGS\ROUTER\LITEDB\ROUTER.litedb";
        public static readonly string LiteDbPath = Path.Combine(LiteDbDir, "ROUTER.litedb");

        // LogDir: MAIN / DEBUG logs, the current day's and the 5th day's tick logs (R19).
        // PastLogDir (LogDir\LOGS): the previous 4 days' tick logs and the minute logs older than 5 days (R19).
        public static readonly string LogDir     = Cfg.Read("log.path", Path.Combine(NinjaTrader.Core.Globals.UserDataDir, "sounds", "LOGS", "ROUTER"));
        public static readonly string PastLogDir = Path.Combine(LogDir, "LOGS");
        public static readonly string EodDir     = Path.Combine(LogDir, "CSV_FILES");

        // R20: checkpoint filename uses the same day format as the EOD files, e.g. ROUTER_CHECKPOINT_2026OCT04.marker
        public static string CheckpointPath(DateTime day)
        {
            return Path.Combine(RouterDataDir, "ROUTER_CHECKPOINT_" + day.ToString("yyyyMMMdd", CultureInfo.InvariantCulture).ToUpperInvariant() + ".marker");
        }

        // Lifecycle ledger (crash ledger for unpaired STOP reconciliation, R20/R24).
        // One line: START|instanceId|startLocalIso
        public static readonly string LifecycleMarkerPath = Path.Combine(RouterDataDir, "ROUTER_LIFECYCLE.marker");
    }

    // ============================================================================
    // ROUTER indicator
    // ============================================================================
    public class ROUTER : Indicator
    {
        // Frozen banner text shown in the Properties panel.
        // Editable ONLY in source. Never edited from the UI.
        private const string BannerFull =
            "ROUTER_v44";
        private const string BannerDate = "2026OCT21";   // STD62: build date, frozen (v14: corrected from 2026OCT14)

        // ---------- user parameters ----------
        // audit3 (#29): the banner used to carry [NinjaScriptProperty] (+ ReadOnly/Display), which turned it into a generated but
        // permanently-ignored script parameter in the indicator's Properties grid. It is display text only and is not editable
        // from the UI, so it is a plain field now - the banner text itself is unchanged.
        public string BannerDisplay
        {
            get { return BannerFull; }
            set { /* intentionally ignored - banner is frozen, read-only display only */ }
        }

        [NinjaScriptProperty]
        [Browsable(false)]
        [Display(Name = "Symbol", Description = "Kept for wrapper compatibility.", Order = 4, GroupName = "1. Data")]
        public string InstrumentSymbol { get; set; }

        [NinjaScriptProperty]
        [Browsable(false)]
        [Range(1, 100000)]
        [Display(Name = "Lot Size", Description = "Contract lot size used to convert volume to lots.", Order = 5, GroupName = "1. Data")]
        public int LotSize { get; set; }

        // ---------- constants ----------
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        private const string TsFmt = "yyyy-MM-dd HH:mm:ss.fff";
        private static readonly TimeSpan SessionStartTod = new TimeSpan(9, 0, 0);    // session day rolls at 09:00
        private static readonly TimeSpan MarketOpenTod   = new TimeSpan(9, 15, 0);   // R26: market open, never test at 09:00
        private static readonly TimeSpan MarketCloseTod  = new TimeSpan(15, 40, 0);  // NSE close
        private static readonly TimeSpan EodWriteTod     = new TimeSpan(15, 45, 0);  // EOD CSV + RECON verdict (R14)
        // (REC31: detection window removed)   // R26 startup mode detection - used ONLY for R26 ...
        // v40: no time-based retention. Rows are never deleted or pruned for age (TickRetentionDays, MinuteRetentionDays, PruneEnabled removed).
        // Log placement is not retention and keeps its own day counts:
        private const int LogCatchUpDays   = 4;   // ExportPast: past trading days whose tick log is written if missing (placement only)
        private const int MinuteLogPastDays = 5;  // ExportPast: minute logs for days before this are written to LOGS (placement only)

        private const int LivePendingMaxRows  = 200000;
        private const int CpFlushWatermark    = 20000;   // cpBuf size that forces an early CpFlush(false) instead of dropping K lines
        private const int StartupRetryMax = 3;   // STD34 (M3): max startup retries before auto-stop
        private static readonly int[] StartupRetryBackoffSec = { 10, 30, 60 };   // STD34 (M3): backoff in seconds
        private const long SeqMaxSane = 1000000000L;   // STD36 (N2): ceiling for checkpoint MaxSeq; one billion is far beyond any real session
        private const int LiveExportMinMs     = 5000;
        private const int TickFlowIntervalSec = 30;

        // ---------- LiteDB ----------
        private LiteDatabase db;
        private ILiteCollection<MinuteRecord> minuteCol;
        private ILiteCollection<TickRecord>   tickCol;
        // =====================================================================
        // LOCK HIERARCHY (audit3 #30: the full list, not a partial one). Do not invert, and do not add a lock
        // without placing it here first. Every path in this file acquires in this order only:
        //
        //     > maintLock   (RunStartup, TryRoll, ShutdownTail; taken before exportLock and dbGate)
        //     > liveGate   (closed across a roll / replacement / drain; NEVER held while anything slow runs)
        //       > exportLock   (every write of today's tick log; also the .tmp name)
        //         > dbGate   (LiteDB access; released before any file write)
        //           > tailLock   (the append-only tick-log tail; dbGate -> tailLock only)
        //           > keyLock    (content-key sets; never held while acquiring dbGate)
        //             > cpLock     (checkpoint buffer; dbGate -> cpLock, and liveGate -> ... -> cpLock)
        //                 > dashLock    (dashboard counters; a leaf - dashLock is never held while taking anything else)
        //                   > seqLock    (SeqNum counter; a leaf)
        //
        // LEAF LOCKS - they hold nothing else, so they may be taken anywhere, but never inside one of the locks above
        // while doing slow I/O: logLock, liveWriterLock, detectLock, reloginLock, apxGate, apxQuoteGate, apxQuotePendGate,
        // Gate (the process-wide owner slot), initGate (inside VECTOR) and the bus gate (b[1], inside VECTOR).
        // apxGate is a leaf in the lock order but its critical sections are kept SHORT (an invoke only, never a wait) -
        // contention at that point, and the gate must exclude a
        // concurrent login / subscribe from re-entering the shutdown.
        // =====================================================================
        private readonly object dbGate = new object();

        // ---------- logging ----------
        private readonly object logLock = new object();
        private string logPath;
        private string debugPath;
        private volatile bool mainRotatePending = false;   // R18: rotate MAIN / DEBUG on the first tick of the new trading day

        // ---------- ownership / lifecycle ----------
        private static readonly object Gate = new object();
        private static ROUTER _owner;
        public static ROUTER Owner { get { lock (Gate) { return _owner; } } }
        private volatile bool ownerReady = false;
        private volatile bool startupOk = false;
        private volatile int startupNonFatalFailures = 0;   // STD28 (audit): count of non-fatal Stage failures during startup
        private volatile bool terminated = false;
        private int startupRetryCount = 0;   // STD34 (M3): number of startup retries attempted
        private volatile string lastStageName = "";   // written by Stage(); read by the startup failure log and the AUTOSTOP bundle
        private DateTime startupRetryNextUtc = DateTime.MinValue;   // STD34 (M3): next scheduled retry time
        private volatile bool autoStopped = false;   // STD34 (M3): auto-stop triggered
        private string autoStopReason = null;   // STD34 (M3): reason for auto-stop
        private string instanceId;
        private readonly string shellId = Guid.NewGuid().ToString("N").Substring(0, 8);   // v43: identity of this indicator object, from creation
        private string OwnerIdText() { try { var o = Owner; return (o != null && o.instanceId != null) ? o.instanceId : "none"; } catch { return "unknown"; } }
        private string duplicateWarning = null;
        private DateTime sessionStartLocal = DateTime.MinValue;
        private DateTime attachEventTime   = DateTime.MinValue;   // ORIGINAL ATTACH EventTime (R8 / R25 / R26 cutoff)

        public override string ToString() { return "ROUTER"; }
        public override string DisplayName { get { return "ROUTER"; } }

        // ---------- session ----------
        private long currentSessionDayTicks = DateTime.MinValue.Ticks;
        // Volatile DateTime via Interlocked for atomic reads/writes with memory barriers
        private DateTime currentSessionDay
        {
            get => new DateTime(Interlocked.Read(ref currentSessionDayTicks));
            set => Interlocked.Exchange(ref currentSessionDayTicks, value.Ticks);
        }
        private readonly HashSet<DateTime> holidaySet = new HashSet<DateTime>();

        // ---------- SeqNum (R1-R3) ----------
        private readonly object seqLock = new object();
        private long seqCounter = 0;                   // highest SeqNum number allocated for the current session day

        // ---------- content keys (R4, R10): source-scoped, one mechanism ----------
        private readonly object keyLock = new object();
        private readonly HashSet<string> keyNin = new HashSet<string>();     // LIVE keys + plain KeyOf of HIST rows (handover)
        private readonly HashSet<string> keyHist = new HashSet<string>();    // HIST keys with occurrence suffix (HistKeyN)
        private readonly HashSet<string> keyHistPlain = new HashSet<string>(); // REC11 diagnostic: plain KeyOf of current-day HIST rows (match phase only)
        // ROUTER_v30: identities of stored live rows whose key-set add failed (even after one retry). Fixed array, guarded by keyLock: storing into it allocates nothing.
        private const int KeyQuarMax = 64;
        private readonly string[] keyQuar = new string[KeyQuarMax];
        private int keyQuarCount = 0;            // guarded by keyLock
        private bool keyQuarOverflow = false;    // guarded by keyLock; true = an identity could not be held: live inserts fail closed until a rebuild from the store
        private long keyQuarHits = 0;            // diagnostic
        private bool KeyQuarantinedLocked(string key) { for (int i = 0; i < keyQuarCount; i++) if (keyQuar[i] == key) return true; return false; }
        // Caller holds keyLock. true = the identity is held. false = no room: overflow latched.
        private bool QuarantineKeyLocked(string key)
        {
            if (KeyQuarantinedLocked(key)) return true;
            if (keyQuarCount < KeyQuarMax) { keyQuar[keyQuarCount++] = key; return true; }
            keyQuarOverflow = true;
            return false;
        }
        private void ClearKeyQuarantineLocked() { Array.Clear(keyQuar, 0, KeyQuarMax); keyQuarCount = 0; keyQuarOverflow = false; }
        // Caller holds keyLock, immediately after keyNin was rebuilt from the store. The rebuilt set is the authority: a held identity that is
        // now in keyNin is established as present; one that is not has no active stored row, so it must not block a genuine tick.
        private void ReconcileKeyQuarantineLocked(string why)
        {
            if (keyQuarCount == 0 && !keyQuarOverflow) return;
            int present = 0, absent = 0;
            for (int i = 0; i < keyQuarCount; i++) { if (keyNin.Contains(keyQuar[i])) present++; else absent++; }
            bool wasOverflow = keyQuarOverflow;
            ClearKeyQuarantineLocked();
            Log("KEYSET_QUARANTINE_RECONCILED | why=" + why + " | presentInRebuiltSet=" + present + " | noActiveStoredRow=" + absent + " | overflowWas=" + wasOverflow);
        }

        // ---------- checkpoint (R9-R11) ----------
        private readonly object cpLock = new object();
        private readonly List<string> cpBuf = new List<string>();
        private DateTime cpFlushFailLastUtc = DateTime.MinValue;   // guarded by cpLock: CpFlush failure log/alert throttle (1 per minute)
        // audit3 (#16): a failing flush must not be retried on EVERY append while the insert path holds dbGate + cpLock.
        // cpRetryNotBeforeUtc backs the retries off; cpBufHardCap bounds the buffer and cpDirty hands the file to the next full
        // rewrite (CpRewrite rebuilds it from the stored rows, so nothing is lost that LiteDB does not already hold).
        private DateTime cpRetryNotBeforeUtc = DateTime.MinValue;
        private bool cpDirty = false;
        private const int CpFailBackoffSec = 5;
        private const int CpBufHardCap = 600000;
        private string cpPath = null;

        // ---------- live gate: ONE insert path, arrival order preserved ----------
        // and are flushed in arrival order through the single insert path. Nothing is silently discarded.
        private readonly object liveGate = new object();
        private volatile bool liveReady = false;
        private readonly List<TickRecord> livePending = new List<TickRecord>();
        private long liveReceivedSeq = 0;    // ROUTER_v02 (RECON A): live ticks accepted into the pipeline (queued or pending)
        private long liveProcessedSeq = 0;   // ROUTER_v02 (RECON A): live ticks the writer or the drain has finished with
        private long liveRecvUtcTicks = 0;   // ROUTER_v02 (RECON A): UTC ticks when the last live tick was accepted
        private long ntBarReadUtcTicks = 0;  // ROUTER_v02 (RECON A): UTC ticks when the NT daily bar was last read
        private long procStallSeen = -1;     // ROUTER_v02 (RECON A): processed count at the last change (dashLock)
        private DateTime procStallSinceUtc = DateTime.MinValue;   // ROUTER_v02 (RECON A): since when the lag has stood (dashLock)
        private long liveDroppedCount = 0;
        private long liveFailedSeq = 0;      // ROUTER_v02 (fix 2): live ticks whose store operation failed (insert failed or exception)
        private long liveDroppedBase = 0;    // ROUTER_v02 (fix 1): liveDroppedCount at the start of this session (roll or attach)
        private long liveFailedBase = 0;     // ROUTER_v02 (fix 2): liveFailedSeq at the start of this session (roll or attach)
        private readonly object maintLock = new object();

        // ---------- STD06 live writer (item 9): ONE thread runs ProcessLive; OnMarketData only builds and enqueues ----------
        private volatile System.Collections.Concurrent.BlockingCollection<TickRecord> liveQueue = null;
        private Thread liveWriter = null;
        private readonly object liveWriterLock = new object();
        private long liveQueued = 0;                       // ticks enqueued and not yet finished by the writer
        private const int LiveWriterFlushMs = 15000;       // bounded wait for the queue to empty (roll / reconnect refetch / detach)

        // ---------- R26 startup mode ----------
        private const int ModeNone = 0, ModeWaitOpen = 1, ModeLiveNin = 3, ModeLateLogin = 4, ModeSkipped = 5;   // REC31: ModeDetect removed; values unchanged
        private volatile int modeState = ModeNone;

// ---------- live side-channel state ----------
        private long   cachedOi  = -1;
        // REC41 Gate C: OI diagnostics. Counters and a status line only. No store write, no bus publish, no fill.
        private long oiEvtCount = 0, oiValidCount = 0, oiZeroFieldCount = 0, oiUnknownCount = 0;
        private long oiLastValidEventTicks = 0;   // exchange EventTime ticks of the last valid OI event; 0 = none this session
        private bool oiStaleLogged = false;       // one-shot per session
        private volatile bool oiSeeded = false;   // ROUTER_v16: cachedOi holds a carried value from the store until the first live valid OI
        private const int OiStaleMinutes = 15;    // owner decision Q1
        private volatile bool histTransitionSeen = false;   // ROUTER_STD81_RECOVER: set at State.Transition
        private const int HistRecoverWaitSec = 120;          // ROUTER_STD81_RECOVER
        private readonly List<TickRecord> histRecover = new List<TickRecord>();   // today's native ticks, captured in OnBarUpdate (Historical + Transition)
        private DateTime histRecoverDay = DateTime.MinValue;                      // the session day that capture belongs to
        private int    dayVolSourceActive  = 0;   // STD60: 1=LIVE_DAILY_VOLUME, 2=STORE_MAX_DAY_VOL, 3=MINUTE_DAY_TOTAL, 0=none
        // STD73: DAY_VOL (and therefore RECON) is taken from the NT daily bar only. When the bar is not available, DAY_VOL is unknown (-1),
        // not a fallback value. true = pinned (STD73). false = the STD72 fallback chain, unchanged.
        private const bool DayVolPinToNtBar = true;
        private long   cachedDayVolume = -1;   // STD10: exchange DailyVolume from OnMarketData; -1 = unknown (reset at RollSession)
        private long   cachedDayVolumeDayTicks = 0;   // STD64: the session-day ticks cachedDayVolume belongs to; 0 = none (long: DateTime.Ticks does not fit an int)
        private long   cachedNtBarVolume = -1;   // STD66: NT daily bar volume (Data Box), raw; -1 = unknown or bar is not the session day's
        private double prevLivePrice = double.NaN;   // STD07-AUDIT (item 30): lastDayVolChangeLocal removed - it was write-only
        private DateTime prevLivePriceDay = DateTime.MinValue;   // STD27 (M14): the session day prevLivePrice belongs to

        // ---------- dashboard timer ----------
        private System.Timers.Timer dashboardTimer;
        private readonly object dashTimerGate = new object();
        private volatile bool dashTimerStopped = false;
        // audit3 (#14): one overall unload budget for the UI thread, plus the three smaller bounds used inside the tail.
        private const int ShutdownTailBudgetMs  = 8000;
        private const int ShutdownDashGateWaitMs = 1000;
        private const int ShutdownWriterJoinMs = 1500;
        private readonly Dictionary<string, DateTime> dirtyNextTry = new Dictionary<string, DateTime>();   // ROUTER_v02: per-day 60 s retry throttle
        private readonly Dictionary<string, string> dirtyLastState = new Dictionary<string, string>();     // ROUTER_v02: per-day state, logged on change only
        private DateTime dirtyScanNextUtc = DateTime.MinValue;                                              // ROUTER_v02: directory scan at most once a minute
        private volatile bool shutdownIncomplete = false;   // set only on the tail thread, read by the UI thread after the bounded wait

        // Written by OnMarketData(Bid) / OnMarketData(Ask) only. Read by ProcessLive. Never persisted on their own.
        private readonly object quoteLock = new object();   // leaf lock: liveBid / liveAsk. Nothing is taken inside it.
        private double liveBid = double.NaN;                 // guarded by quoteLock
        private double liveAsk = double.NaN;                 // guarded by quoteLock
        private long liveBidQty = -1;                        // REC08: standing bid size (raw contracts), guarded by quoteLock; -1 = none
        private long liveAskQty = -1;                        // REC08: standing ask size (raw contracts), guarded by quoteLock; -1 = none
        // hub Closed/Shutdown raises pending; the next Connected clears it and schedules ONE reconnect fill (R5/R25).
        // STD07-AUDIT (item 30): fetchTimedOut removed - it was write-only (the FETCH_TIMEOUT log line is the real signal)


        // =====================================================================
        // lifecycle
        // =====================================================================
        protected override void OnStateChange()
        {
            if (State == State.Transition) histTransitionSeen = true;   // ROUTER_STD81_RECOVER
            if (State == State.SetDefaults)
            {
                Name = "ROUTER";
                Description = "NinjaTrader live ticks + native historical recovery -> one insert path -> LiteDB + VECTOR (Model A write side).";
                Calculate = Calculate.OnBarClose;   // STD07-AUDIT (item 33): no OnBarUpdate consumer - OnEachTick only cost callbacks. Live data flows through OnMarketData; markers are drawn from the poll timer via TriggerCustomEvent.
                IsOverlay = true;
                DisplayInDataBox = false;
                PaintPriceMarkers = false;
                InstrumentSymbol = "NIFTY-1";
                LotSize = 65;
            }
            else if (State == State.Configure)
            {
                AddDataSeries(BarsPeriodType.Day, 1);   // STD66: NT daily bar = the SOT (index 1)
                AddDataSeries(BarsPeriodType.Tick, 1);  // ROUTER_STD81_RECOVER: native 1-Tick series (index 2)
            }
            else if (State == State.DataLoaded)
            {
                instanceId = Guid.NewGuid().ToString("N").Substring(0, 8);
                attachEventTime = DateTime.Now;   // ORIGINAL ATTACH EventTime
                startupNonFatalFailures = 0;   // STD28 (audit): reset non-fatal failure counter
                eodRetryNotBeforeUtc = DateTime.MinValue;   // STD29 (M1): reset EOD backoff
                Directory.CreateDirectory(RouterConfig.LogDir);

                // Holidays first: SessionDay() needs them (Trading Hours template full holidays).
                try
                {
                    holidaySet.Clear();
                    foreach (KeyValuePair<DateTime, string> hol in Bars.TradingHours.Holidays)
                        holidaySet.Add(hol.Key.Date);
                }
                catch (Exception hex) { holidaySet.Clear(); Debug("HOLIDAY_LIST_FAIL | " + Ex(hex)); }

                // ROUTER_v10 (F4): extra holidays from prasan.config, e.g. holidays.extra=2026-10-02,2026-11-10
                foreach (string tok in Cfg.Read("holidays.extra", "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    DateTime hd;
                    if (DateTime.TryParseExact(tok.Trim(), "yyyy-MM-dd", Inv, DateTimeStyles.None, out hd)) holidaySet.Add(hd.Date);
                    else Log("HOLIDAY_EXTRA_BAD | token=" + tok.Trim());
                }

                // ROUTER_v10 (F4): warn when weekday holidays for this year look too few. Threshold: holidays.minPerYear (default 12).
                int weekdayHolidays = 0;
                foreach (DateTime h in holidaySet)
                    if (h.Year == DateTime.Now.Year && h.DayOfWeek != DayOfWeek.Saturday && h.DayOfWeek != DayOfWeek.Sunday) weekdayHolidays++;
                int minPerYear;
                if (!int.TryParse(Cfg.Read("holidays.minPerYear", "12"), NumberStyles.Integer, Inv, out minPerYear)) minPerYear = 12;
                if (weekdayHolidays < minPerYear)
                    Log("HOLIDAY_SPARSE | year=" + DateTime.Now.Year + " | weekdayHolidays=" + weekdayHolidays + " | min=" + minPerYear
                        + " | add NSE dates to prasan.config as holidays.extra");

                currentSessionDay = SessionToday();
                histRecoverDay = currentSessionDay.Date;   // ROUTER_STD81_RECOVER: day whose native ticks OnBarUpdate captures
                RotateLogs(currentSessionDay);
                Debug("STATE | DataLoaded | instanceId=" + instanceId);
                try
                {
                    Log("HOLIDAY_LIST | template=" + Bars.TradingHours.Name + " | count=" + holidaySet.Count
                        + " | sessionDayIsHoliday=" + holidaySet.Contains(currentSessionDay.Date));
                }
                catch { }
                Log("======== " + BannerFull + "_LOADED ======== InstanceId=" + instanceId);   // REC31: identity follows the build banner
                Log("LOT_SIZE_CHECK | configured=" + LotSize + " | instrument=" + (Instrument != null ? Instrument.FullName : "NULL"));
                Log("ROUTER_SESSION_START | sessionDay=" + currentSessionDay.ToString("yyyy-MM-dd", Inv));

                bool isOwner;
                lock (Gate)
                {
                    if (_owner != null && _owner != this) isOwner = false;
                    else { _owner = this; isOwner = true; }
                }
                if (!isOwner)
                {
                    string ownerId = "unknown";
                    try { var o = Owner; if (o != null) ownerId = o.instanceId ?? "unknown"; } catch { }
                    Log("ROUTER attach rejected | new instanceId=" + instanceId + " | owner instanceId=" + ownerId);
                    LogEvent("DUPLICATE_ATTACH_REJECTED", "SECONDARY");
                    Print("ERROR: ROUTER ALREADY RUNNING WITH INSTANCEID " + ownerId);
                    duplicateWarning = "ROUTER ALREADY RUNNING WITH INSTANCEID " + ownerId;
                    return;
                }

                LogEvent("ROUTER_STARTED", "OWNER");
                try
                {
                    ReconcileUnpairedStop();     // R24: unpaired STOP from a crashed run, BEFORE the new START
                    WriteSessionStart();         // R20: START line -> MAIN
                    WriteLifecycleMarker();      // crash ledger, written only after the START line landed
                    Log("ROUTER_STARTED_INFO | instrument=" + (Instrument != null ? Instrument.FullName : "NULL"));
                    Log("ROUTER_PATHS | config=" + RouterConfig.ConfigDir
                        + " | routerData=" + RouterConfig.RouterDataDir + " | liteDb=" + RouterConfig.LiteDbPath
                        + " | logDir=" + RouterConfig.LogDir + " | pastLogDir=" + RouterConfig.PastLogDir);
                    Log("CHECKPOINT_PATH | path=" + RouterConfig.CheckpointPath(currentSessionDay));

                    ownerReady = true;
                }
                catch (Exception oex)
                {
                    // STD07-AUDIT (audit2 #20): the owner slot was claimed above; if init throws before ownerReady=true, release it so a
                    // later attach can succeed (Terminated's !ownerReady branch also releases it defensively). Rethrow so NT8 sees the failure.
                    lock (Gate) { if (_owner == this) _owner = null; }
                    Log("ROUTER_OWNER_INIT_FAIL | releasing owner slot | " + Ex(oex));
                    throw;
                }
                InitModeAtAttach();          // R26 mode state at attach (non-trading day -> skipped)

                dashboardTimer = new System.Timers.Timer(1000);
                dashboardTimer.Elapsed += (s, ev) =>
                {
                    lock (dashTimerGate)
                    {
                        if (dashTimerStopped) return;
                        try { DashboardTick(); } catch (Exception dex) { Debug("DASH_TICK_EX | " + Ex(dex)); }
                    }
                };
                dashboardTimer.AutoReset = true;
                dashboardTimer.Enabled = true;

                StartLiveWriter();   // STD06 (item 9): live inserts run here; nothing is enqueued until startup opens the gate
                Task.Run(() => RunStartup());
            }
            else if (State == State.Terminated)
            {
                Debug("STATE | Terminated | instanceId=" + (instanceId ?? "none") + " | shellId=" + shellId + " | state=" + State);
                terminated = true;
                // audit3 (#14): set the stop flag FIRST (a volatile, so an in-flight tick sees it without the gate), then take
                // the gate only if it is free quickly. The old code took dashTimerGate unconditionally and DashboardTick holds
                // it across a FULL tick-log rewrite, so an unload could sit behind a multi-second export before it even started.
                dashTimerStopped = true;
                bool gotDash = false;
                try { gotDash = System.Threading.Monitor.TryEnter(dashTimerGate, ShutdownDashGateWaitMs); }
                catch { }
                try
                {
                    if (dashboardTimer != null) { try { dashboardTimer.Stop(); } catch { } }
                    if (gotDash) { try { if (dashboardTimer != null) { dashboardTimer.Dispose(); dashboardTimer = null; } } catch { } }
                }
                finally { if (gotDash) { try { System.Threading.Monitor.Exit(dashTimerGate); } catch { } } }
                if (!gotDash) Debug("SHUTDOWN_DASH_TIMER_WAIT_TIMEOUT | stopped without disposing the timer; the export/close tail continues");
                Debug("DASH_TIMER_STOPPED | instanceId=" + instanceId);

                if (!ownerReady)
                {
                    // STD07-AUDIT (audit2 #20): a DataLoaded that threw AFTER claiming the static owner slot but BEFORE ownerReady=true
                    // reaches this branch. Without releasing _owner here, the dead instance owns the slot forever and every later attach
                    // is rejected as a duplicate until NinjaTrader restarts. Release it defensively (idempotent with the owner-init catch).
                    lock (Gate) { if (_owner == this) _owner = null; }
                    Log("======== ROUTER_UNLOADED ======== InstanceId=" + (instanceId ?? "none") + " | shellId=" + shellId
                        + " | state=" + State + " | reachedDataLoaded=" + (instanceId != null) + " | owner=" + OwnerIdText()
                        + " | Role=REJECTED | reason=not_owner_at_unload");
                    return;
                }
                ownerReady = false;
                // audit3 (#14): the ENTIRE slow tail (writer drain, session stop, final checkpoint flush, final export, quote
                // budget. Before, the UI thread itself ran the writer join (15 s) + maintLock wait (5 s) + tail wait (20 s) +
                // SAPI (2.5 s) - each bounded, the sum not (up to ~40 s of frozen UI). The UI thread now waits at most
                // ShutdownTailBudgetMs and gives up with SHUTDOWN_TAIL_TIMEOUT; the tail keeps finishing in the background.
                var tailSw = Stopwatch.StartNew();
                Task shutdownTail;
                try { shutdownTail = Task.Run(ShutdownTail); }
                catch (Exception sex) { shutdownIncomplete = true; Debug("SHUTDOWN_TAIL_START_EX | " + Ex(sex)); shutdownTail = null; }
                if (shutdownTail != null)
                {
                    // STD17 (C2): the tail now bounds its OWN exportLock wait at ShutdownTailBudgetMs (ShutdownTail), so the UI budget
                    // is that budget plus the same budget again - otherwise the UI always gave up at exactly the moment the bounded
                    // export finished. Still one bounded wait, and far under the ~40 s of frozen UI audit3 (#14) removed.
                    int uiBudgetMs = ShutdownTailBudgetMs + ShutdownTailBudgetMs;
                    try { if (!shutdownTail.Wait(uiBudgetMs)) { shutdownIncomplete = true; Debug("SHUTDOWN_TAIL_TIMEOUT | budgetMs=" + uiBudgetMs + " | export/close still running in the background"); } }
                    catch (Exception wex) { shutdownIncomplete = true; Debug("SHUTDOWN_TAIL_WAIT_EX | " + Ex(wex)); }
                }
                tailSw.Stop();
                Debug("SHUTDOWN_UI_WAIT_DONE | elapsedMs=" + tailSw.ElapsedMilliseconds + " | complete=" + (shutdownTail != null && shutdownTail.IsCompleted));
            }
        }

        // audit3 (#14): the whole shutdown tail, off the NT8 UI thread, each step individually bounded. Runs exactly once.
        // ownerReady is already false and terminated is already true, so no new live insert can start here.
        // ROUTER_v02 (closure): the single dirty-marker location and writer. Marker = ROUTER_DIRTY_<yyyyMMMdd>.marker under RouterDataDir.
        private string DirtyMarkerPath(DateTime day) { return Path.Combine(RouterConfig.RouterDataDir, "ROUTER_DIRTY_" + DayTag(day) + ".marker"); }

        private bool WriteDirtyMarker(DateTime day, string why)
        {
            try
            {
                Directory.CreateDirectory(RouterConfig.RouterDataDir);
                File.WriteAllText(DirtyMarkerPath(day), "dirty|" + why + "|" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", Inv));
                return true;
            }
            catch (Exception ex) { Debug("DIRTY_MARKER_WRITE_FAIL | day=" + DayTag(day) + " | " + Ex(ex)); return false; }
        }

        // ROUTER_v02 (fix C): every outstanding ROUTER_DIRTY_<day>.marker is recovered, not only today's. Scanned at most once a minute.
        private void RecoverDirtyMarkers()
        {
            if (DateTime.UtcNow < dirtyScanNextUtc) return;
            dirtyScanNextUtc = DateTime.UtcNow.AddSeconds(60);
            string[] files;
            try
            {
                if (!Directory.Exists(RouterConfig.RouterDataDir)) return;
                files = Directory.GetFiles(RouterConfig.RouterDataDir, "ROUTER_DIRTY_*.marker");
            }
            catch (Exception ex) { Debug("DIRTY_SCAN_FAIL | " + Ex(ex)); return; }
            foreach (string f in files)
            {
                string tag = Path.GetFileNameWithoutExtension(f).Substring("ROUTER_DIRTY_".Length);
                DateTime day;
                if (!DateTime.TryParseExact(tag, "yyyyMMMdd", Inv, DateTimeStyles.None, out day))
                {
                    string prevBad; if (!dirtyLastState.TryGetValue(tag, out prevBad) || prevBad != "BAD_NAME")
                    { dirtyLastState[tag] = "BAD_NAME"; Log("DIRTY_MARKER_BAD_NAME | file=" + Path.GetFileName(f)); }
                    continue;
                }
                DateTime nextTry;
                if (dirtyNextTry.TryGetValue(tag, out nextTry) && DateTime.UtcNow < nextTry) continue;
                dirtyNextTry[tag] = DateTime.UtcNow.AddSeconds(60);
                string state = RecoverOneDay(day, f);
                string last;
                if (!dirtyLastState.TryGetValue(tag, out last) || last != state)
                {
                    dirtyLastState[tag] = state;
                    Log("DIRTY_MARKER_" + state + " | day=" + tag);
                }
            }
        }

        // ROUTER_v02: returns the state for logging. The marker is deleted only when the checkpoint file AND the tick-log file were both written.
        private string RecoverOneDay(DateTime day, string markerPath)
        {
            try
            {
                bool written = false;
                lock (dbGate)
                {
                    if (db == null || tickCol == null) return "KEPT_STORE_CLOSED";
                    List<TickRecord> active = StoreTicksOrdered(day);   // active rows: the same set ATTACH_END and the tick log use
                    if (active.Count == 0)
                    {
                        // An empty day has nothing to rebuild or export. It is cleared as such, never reported as a recovery.
                        File.Delete(markerPath);
                        return "EMPTY_DAY_NOTHING_TO_RECOVER";
                    }
                    int ni, nh; long ms;
                    written = CpWriteFile(day, active, "DIRTY_MARKER", out ni, out nh, out ms);
                    if (written && day.Date == currentSessionDay.Date)
                    {
                        // The file now holds every stored row. Buffered K lines are already covered: drop them, never append them twice.
                        lock (cpLock) { cpPath = RouterConfig.CheckpointPath(day); cpBuf.Clear(); cpDirty = false; cpRetryNotBeforeUtc = DateTime.MinValue; }
                    }
                }
                if (!written) return "KEPT_CHECKPOINT_FAILED";
                string home = LogHomeFor(TradingDaysBetween(day, currentSessionDay));
                bool exported;
                lock (exportLock)
                {
                    exported = ExportTickLog(day, home);
                    // The current day's steady-state tail must not append onto a file rewritten here: force its next export to a full rewrite.
                    if (exported && day.Date == currentSessionDay.Date) TailMarkDirty();
                }
                if (!exported) return "KEPT_EXPORT_FAILED";
                File.Delete(markerPath);
                return "CLEARED";
            }
            catch (Exception ex) { Debug("DIRTY_MARKER_EX | " + Ex(ex)); return "KEPT_EXCEPTION"; }
        }

        // ROUTER_v02 (fix A): the shutdown task writes the marker first, and deletes it only after every required step has succeeded.
        private void ShutdownTail()
        {
            var sw = Stopwatch.StartNew();
            bool gotMaint = false;
            bool tailComplete = false;
            DateTime markerDay = currentSessionDay.Date;
            bool preExisting = File.Exists(DirtyMarkerPath(markerDay));   // an older marker is recovery evidence: this tail must not remove it
            bool markerOk = WriteDirtyMarker(markerDay, "SHUTDOWN_IN_PROGRESS");   // before any shutdown work: a termination from here on leaves it behind
            try
            {
                bool writerDrained = StopLiveWriter(ShutdownWriterJoinMs);   // STD06: drain queued ticks into LiteDB before the checkpoint flush, final export and DB close
                WriteSessionStop();   // R23: exactly one STOP line -> MAIN
                if (markerOk) ClearLifecycleMarker();   // ROUTER_v02 (decision 4): no dirty marker = keep the lifecycle evidence
                bool cpConfirmed = false;
                if (!writerDrained)
                {
                    // STD59: the writer is still alive; its last inserts are in flight. Do not flush or export against a live writer.
                    shutdownIncomplete = true;
                    Log("SHUTDOWN_FLUSH_SKIPPED | reason=writer_still_active | writerJoinMs=" + ShutdownWriterJoinMs);
                }
                else
                {
                    try
                    {
                        bool repaired = CpRepairDirty(false);
                        bool flushed = CpFlush(true);
                        cpConfirmed = repaired && flushed;
                    }
                    catch (Exception cex) { cpConfirmed = false; Debug("CHECKPOINT_FINAL_FLUSH_FAIL | " + Ex(cex)); }
                    if (!cpConfirmed) { shutdownIncomplete = true; Debug("CHECKPOINT_NOT_CONFIRMED | at=shutdown"); }
                }

                // STD17 (H9): blocking lock. Monitor.Enter so the Exit below releases it exactly once.
                try { System.Threading.Monitor.Enter(maintLock); gotMaint = true; }
                catch { }

                bool exportOk;
                if (db == null) exportOk = true;                              // no store was ever opened: nothing to export
                else if (!startupOk || !writerDrained) exportOk = false;      // export not performed: the marker must stay
                else
                {
                    bool gotExport = false;
                    exportOk = false;
                    try
                    {
                        gotExport = System.Threading.Monitor.TryEnter(exportLock, ShutdownTailBudgetMs);
                        if (!gotExport) { shutdownIncomplete = true; Debug("FINAL_EXPORT_SKIPPED | export_lock_busy"); }
                        else
                        {
                            bool hasRows;
                            lock (dbGate) { hasRows = tickCol != null && tickCol.Find(x => x.Day == markerDay).Any(); }
                            exportOk = !hasRows || ExportTodayLogs(true);   // an empty day has nothing to export
                        }
                    }
                    catch (Exception fex) { exportOk = false; shutdownIncomplete = true; Debug("FINAL_EXPORT_FAIL | " + Ex(fex)); }
                    finally { if (gotExport) { try { System.Threading.Monitor.Exit(exportLock); } catch { } } }
                    if (!exportOk) shutdownIncomplete = true;
                }

                bool disposeOk = false;
                lock (dbGate)
                {
                    bool disposed = false;
                    try { if (db != null) { db.Dispose(); disposed = true; } }
                    catch (Exception ex) { shutdownIncomplete = true; Debug("LITEDB_DISPOSE | EX=" + Ex(ex)); }
                    if (disposed) { db = null; tickCol = null; minuteCol = null; disposeOk = true; }
                    else if (db != null) Debug("LITEDB_DISPOSE_KEPT | dispose failed, handle retained");
                    else disposeOk = true;   // store was already closed
                }
                if (gotMaint) { try { System.Threading.Monitor.Exit(maintLock); } catch { } gotMaint = false; }
                lock (Gate) { if (_owner == this) _owner = null; }
                Log("======== ROUTER_UNLOADED ======== InstanceId=" + instanceId + " | shellId=" + shellId + " | state=" + State
                    + " | reachedDataLoaded=" + (instanceId != null) + " | Role=OWNER");
                LogEvent("ROUTER_STOPPED", "OWNER");
                // Every required step has positively succeeded. Only this line lets the marker go.
                tailComplete = writerDrained && cpConfirmed && exportOk && disposeOk;
            }
            catch (Exception ex) { shutdownIncomplete = true; tailComplete = false; Debug("SHUTDOWN_TAIL_EX | " + Ex(ex)); }
            finally
            {
                if (gotMaint) { try { System.Threading.Monitor.Exit(maintLock); } catch { } }   // audit5 (#6): never leave maintLock held
                if (tailComplete && !preExisting)
                {
                    try { File.Delete(DirtyMarkerPath(markerDay)); Debug("DIRTY_MARKER_CLEARED_AT_SHUTDOWN | day=" + DayTag(markerDay)); }
                    catch (Exception dex) { Debug("DIRTY_MARKER_CLEAR_FAIL | " + Ex(dex)); }
                }
                sw.Stop(); Debug("SHUTDOWN_TAIL_DONE | elapsedMs=" + sw.ElapsedMilliseconds + " | complete=" + tailComplete);
            }
        }


        private bool OwnerGone() { return terminated || PRASAN.ROUTER.Owner != this; }   // STD07-AUDIT (item 7): a terminated instance is gone even before the owner slot flips

        // =====================================================================
        // logging (R18: MAIN / DEBUG rotate on the first tick of the new trading day)
        // =====================================================================
        private static string DayTag(DateTime d) { return d.ToString("yyyyMMMdd", Inv).ToUpperInvariant(); }

        private void RotateLogs(DateTime day)
        {
            lock (logLock)
            {
                // REC22: log files are named by the CALENDAR date of the write, not the session day. A holiday or weekend
                // attach therefore goes into that day's own log, never into the last trading day's file.
                logPath   = Path.Combine(RouterConfig.LogDir, "ROUTER_MAIN_"  + DayTag(DateTime.Now.Date) + ".log");
                debugPath = Path.Combine(RouterConfig.LogDir, "ROUTER_DEBUG_" + DayTag(DateTime.Now.Date) + ".log");
                mainRotatePending = false;
            }
        }

        // STD01: Log samples its timestamp INSIDE logLock and Prints INSIDE logLock.
        // LogEvent builds its line OUTSIDE the lock and never Prints. Both original timings are preserved exactly.
        private void AppendMainLocked(string msg)
        {
            lock (logLock)
            {
                Directory.CreateDirectory(RouterConfig.LogDir);
                if (logPath == null) logPath = Path.Combine(RouterConfig.LogDir, "ROUTER_MAIN_" + DayTag(DateTime.Now.Date) + ".log");   // v38: first write before RotateLogs
                string line = "EventTime=" + DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff", Inv).ToUpperInvariant() + " | " + msg;
                File.AppendAllText(logPath, line + Environment.NewLine);
                Print(line);
            }
        }

        private void AppendMainSilent(string line)
        {
            lock (logLock)
            {
                Directory.CreateDirectory(RouterConfig.LogDir);
                if (logPath == null) logPath = Path.Combine(RouterConfig.LogDir, "ROUTER_MAIN_" + DayTag(DateTime.Now.Date) + ".log");   // v39: same guard as AppendMainLocked
                File.AppendAllText(logPath, line + Environment.NewLine);
            }
        }

        private void Log(string msg)
        {
            try { AppendMainLocked(msg); }
            catch (Exception lex) { NoteLogFailure("MAIN", lex); }
        }

        private void LogEvent(string eventName, string role)
        {
            try
            {
                string ts = DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff", Inv).ToUpperInvariant();
                string line = "EventTime=" + ts + " | Event=" + eventName
                            + " | InstanceId=" + (instanceId ?? "unknown") + " | Role=" + role;
                AppendMainSilent(line);
            }
            catch (Exception lex2) { NoteLogFailure("MAIN_EVENT", lex2); }
        }

        private static string Ex(Exception ex)
        {
            return ex.GetType().Name + " | " + ex.Message;
        }

        private static int debugWriteCount = 0;
        private void Debug(string msg)
        {
            try
            {
                lock (logLock)
                {
                    Directory.CreateDirectory(RouterConfig.LogDir);
                    string p = debugPath ?? Path.Combine(RouterConfig.LogDir, "ROUTER_DEBUG_" + DayTag(DateTime.Now) + ".log");
                    if (Interlocked.Increment(ref debugWriteCount) % 200 == 0)
                    {
                        FileInfo fi = new FileInfo(p);
                        if (fi.Exists && fi.Length >= 5L * 1024 * 1024)
                        {
                            string old = p + ".old";
                            if (File.Exists(old)) File.Delete(old);
                            File.Move(p, old);
                        }
                    }
                    File.AppendAllText(p, "[" + DateTime.Now.ToString("HH:mm:ss.fff", Inv) + "] " + msg + Environment.NewLine);
                }
            }
            catch (Exception dex) { NoteLogFailure("DEBUG", dex); }
        }

        // v36: a failed log write is counted and printed to the NinjaTrader Output window (first 20 only).
        // It is never written to the log file that just failed.
        private static long logFailCount = 0;
        private void NoteLogFailure(string where, Exception ex)
        {
            long n = Interlocked.Increment(ref logFailCount);
            if (n > 20) return;
            try { Print("ROUTER LOG_WRITE_FAIL | where=" + where + " | nth=" + n + " | " + ex.GetType().Name + ": " + ex.Message); }
            catch (Exception) { }   // last resort: the Output window itself failed
        }

        private void LogBoth(string msg) { Log(msg); Debug(msg); }

        // =====================================================================
        // lifecycle ledger (R20, R23, R24) - START / STOP lines go to MAIN only (never in the tick log)
        // =====================================================================
        private void WriteSessionStart()
        {
            sessionStartLocal = DateTime.Now;
            string line = "ROUTER STARTED at " + sessionStartLocal.ToString(TsFmt, Inv)
                + " | InstanceId=" + instanceId + " | Role=OWNER";
            Log(line);
        }

        private static string UptimeText(DateTime start, DateTime stop)
        {
            if (start == DateTime.MinValue) return "unknown";
            TimeSpan up = stop - start;
            if (up < TimeSpan.Zero) up = TimeSpan.Zero;
            return string.Format(Inv, "{0:00}:{1:00}:{2:00}", (long)up.TotalHours, up.Minutes, up.Seconds);
        }

        private void WriteSessionStop()
        {
            DateTime stop = DateTime.Now;
            string line = "ROUTER STOPPED at " + stop.ToString(TsFmt, Inv)
                + " | InstanceId=" + instanceId + " | Role=OWNER | Uptime=" + UptimeText(sessionStartLocal, stop);
            Log(line);
        }


        private void WriteLifecycleMarker()
        {
            try
            {
                Directory.CreateDirectory(RouterConfig.RouterDataDir);
                File.WriteAllText(RouterConfig.LifecycleMarkerPath,
                    "START|" + instanceId + "|" + sessionStartLocal.ToString(TsFmt, Inv));
            }
            catch (Exception ex) { Debug("LIFECYCLE_MARKER_WRITE_FAIL | " + Ex(ex)); }
        }

        private void ClearLifecycleMarker()
        {
            try { if (File.Exists(RouterConfig.LifecycleMarkerPath)) File.Delete(RouterConfig.LifecycleMarkerPath); }
            catch (Exception ex) { Debug("LIFECYCLE_MARKER_CLEAR_FAIL | " + Ex(ex)); }
        }

        // R24: a surviving marker means the prior run never wrote its STOP line. Write the synthetic STOP into MAIN.
        private void ReconcileUnpairedStop()
        {
            bool keepLifecycle = true;   // ROUTER_v02 (decision 4): cleared only after the dirty marker is safely written
            try
            {
                string path = RouterConfig.LifecycleMarkerPath;
                if (!File.Exists(path)) return;
                string raw = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(raw)) return;
                string[] parts = raw.Split('|');
                string priorInstance = parts.Length > 1 ? parts[1] : "unknown";
                DateTime priorStart  = DateTime.MinValue;
                // new marker: START|instance|startIso. A stale marker written by an older build has one extra field before the time.
                string priorStartText = parts.Length > 3 ? parts[3] : (parts.Length > 2 ? parts[2] : null);
                if (priorStartText != null) DateTime.TryParse(priorStartText, Inv, DateTimeStyles.None, out priorStart);
                // ROUTER_v02 (fix B): an unpaired lifecycle marker means a crash. Its session day gets a dirty marker so recovery runs.
                DateTime crashDay = priorStart != DateTime.MinValue ? SessionDay(priorStart) : SessionToday();
                keepLifecycle = !WriteDirtyMarker(crashDay, "UNPAIRED_LIFECYCLE");
                DateTime now = DateTime.Now;
                string uptime = UptimeText(priorStart, now);
                Log("ROUTER STOPPED at " + now.ToString(TsFmt, Inv)
                    + " | InstanceId=" + priorInstance + " | Role=OWNER | Uptime=" + uptime + " | Reason=PRIOR_SESSION_UNPAIRED");
                Log("ROUTER_STOPPED_UNPAIRED | priorInstance=" + priorInstance
                    + " | priorStart=" + (priorStart == DateTime.MinValue ? "unknown" : priorStart.ToString(TsFmt, Inv))
                    + " | uptime=" + uptime);
                LogEvent("ROUTER_STOPPED_UNPAIRED", "OWNER");
            }
            catch (Exception ex) { Debug("LIFECYCLE_RECONCILE_FAIL | " + Ex(ex)); }
            finally { if (!keepLifecycle) ClearLifecycleMarker(); }
        }

        // =====================================================================
        // session day (09:00 roll, weekends + holidays hold the last trading day) - R12
        // =====================================================================
        private bool IsTradingDate(DateTime d)
        {
            d = d.Date;
            return d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday && !holidaySet.Contains(d);
        }

        private DateTime PrevTradingDate(DateTime d)
        {
            d = d.Date.AddDays(-1);
            int cap = 30;
            while (!IsTradingDate(d) && cap-- > 0) d = d.AddDays(-1);
            return d;
        }

        private DateTime SessionDay(DateTime t)
        {
            DateTime d = t.Date;
            if (t.TimeOfDay < SessionStartTod) d = d.AddDays(-1);
            while (!IsTradingDate(d)) d = d.AddDays(-1);
            return d;
        }

        private DateTime SessionToday() { return SessionDay(DateTime.Now); }

        // STD43: single decision point for "is NIN supplying the session right now". True only inside trading hours AND when a NIN Last tick
        // arrived within the last 30 s (same value as SapiAlerts.NinSilentSec, so the SAPI alert and the fill policy agree).

        // R12 held day. Primary source = LiteDB ticks table + HOLIDAY_LIST; fallback = the day's checkpoint marker.
        // Only used when the clock is OUTSIDE an active trading session day (weekend, holiday, or before 09:00);
        // inside one, the session day is simply today's and nothing is held.
        private DateTime ResolveHeldDay()
        {
            DateTime now = DateTime.Now;
            DateTime cand = SessionDay(now);
            bool holding = !IsTradingDate(now.Date) || now.TimeOfDay < SessionStartTod;
            if (!holding) return cand;
            try
            {
                DateTime d = cand;
                for (int i = 0; i < 14; i++)
                {
                    bool has;
                    lock (dbGate) { has = tickCol != null && tickCol.Exists(x => x.Day == d); }
                    if (has) { Log("HELD_DAY | day=" + d.ToString("yyyy-MM-dd", Inv) + " | source=LITEDB_TICKS+HOLIDAY_LIST"); return d; }
                    d = PrevTradingDate(d);
                }
            }
            catch (Exception ex) { Debug("HELD_DAY_LITEDB_FAIL | " + Ex(ex)); }
            try
            {
                DateTime best = DateTime.MinValue;
                if (Directory.Exists(RouterConfig.RouterDataDir))
                    foreach (string f in Directory.GetFiles(RouterConfig.RouterDataDir, "ROUTER_CHECKPOINT_*.marker"))
                    {
                        string stem = Path.GetFileNameWithoutExtension(f).Substring("ROUTER_CHECKPOINT_".Length);
                        DateTime cd;
                        if (DateTime.TryParseExact(stem, "yyyyMMMdd", Inv, DateTimeStyles.None, out cd)
                            && cd <= cand && cd > best && IsTradingDate(cd)) best = cd;
                    }
                if (best != DateTime.MinValue)
                {
                    Log("HELD_DAY | day=" + best.ToString("yyyy-MM-dd", Inv) + " | source=CHECKPOINT_MARKER");
                    return best;
                }
            }
            catch (Exception ex) { Debug("HELD_DAY_CHECKPOINT_FAIL | " + Ex(ex)); }
            Log("HELD_DAY | day=" + cand.ToString("yyyy-MM-dd", Inv) + " | source=CALENDAR");
            return cand;
        }

        // =====================================================================
        // SeqNum (R1-R3). Format MMMdd-NNN, 3-digit minimum, continues past 999 without truncation.
        // =====================================================================
        private static string SeqText(DateTime day, long n)
        {
            return day.ToString("MMMdd", Inv).ToUpperInvariant() + "-" + n.ToString("000", Inv);
        }

        // Allocates the next SeqNum number. Called ONLY when a row is inserted as a genuinely new row (R3).
        private long AllocSeqN()
        {
            lock (seqLock) { seqCounter++; return seqCounter; }
        }

        // Raises the counter to at least n (restart recovery / re-seed). Never lowers it.
        private void RaiseSeqTo(long n)
        {
            lock (seqLock) { if (n > seqCounter) seqCounter = n; }
        }

        private void ResetSeqForDay()
        {
            lock (seqLock) { seqCounter = 0; }
        }

        // =====================================================================
        // content key (R4 / R10): (EventTime, Price, Volume, OI). Source defines the scope, not the tuple.
        // =====================================================================
        // v41: identity for the historical insert gate. OI is excluded (same as the cross-phase match today).
        private string ExactKeyOf(long ticks, double price, long volume)
        {
            return ticks.ToString(Inv) + "|" + SnapToTick(price).ToString("R", Inv) + "|" + volume.ToString(Inv);
        }

        private static string KeyOf(DateTime eventTime, double price, long volume, long oi)
        {
            return eventTime.Ticks.ToString(Inv) + "|" + price.ToString("R", Inv) + "|" + volume.ToString(Inv) + "|" + oi.ToString(Inv);
        }

        private static string KeyOf(TickRecord r) { return KeyOf(r.EventTime, r.Price, r.Volume, r.OpenInterest); }

        // occurrence number: 1 = the plain key (old stored days and checkpoints stay valid), n>=2 = key + "#n". NIN keys are unchanged.

        // Rebuild source-scoped key sets from stored rows with the same occurrence rule (order of rows does not matter: membership only).
        private static void AddStoredKeys(IEnumerable<TickRecord> rows, HashSet<string> nin, HashSet<string> hist)
        {
            var occ = new Dictionary<string, int>();
            foreach (TickRecord r in rows)
            {
                string b = KeyOf(r);
                if (IsLiveTick(r)) { nin.Add(b); continue; }
                int n; occ.TryGetValue(b, out n); n++; occ[b] = n;
                hist.Add(HistKeyN(b, n));
                nin.Add(b);   // handover: a live tick identical to a recovered row is the same trade
            }
        }

        // Cross-phase match key for the HIST recovery: time + price + volume, no OpenInterest.
        private static string NoOiKey(TickRecord r) { return r.EventTime.Ticks.ToString(Inv) + "|" + PvoNoOi(r.Price, r.Volume); }

        // Occurrence-suffixed key for recovered (HIST) rows: identical trades (same time, price, volume, OI) are each kept once.
        private static string HistKeyN(string b, int n) { return b + "#" + n.ToString(Inv); }

        private static bool IsLiveTick(TickRecord r) { return r.Phase == "LIVE"; }   // tests Phase only; no source label
        private static bool IsNativeTape(TickRecord r) { return r.Phase == "LIVE" || r.Phase == "HIST"; }   // NT-sourced rows: live + recovered

        // =====================================================================
        // checkpoint (R9 / R10 / R11): ONE file per trading day, holds SeqNum state AND content-key state.
        //   H|<day>|<utc>                                     header, one per rewrite
        //   S|<maxSeqN>|<utc>                                  SeqNum state, the LAST S line wins
        // and the session roll REWRITE the file from the final stored state (compaction). The checkpoint never prunes.
        // =====================================================================
        private sealed class CpState { public bool Exists; public long MaxSeq; public int Nin; public int Hist; public int Lines; }

        private void CpOpenDay(DateTime day)
        {
            lock (cpLock) { cpPath = RouterConfig.CheckpointPath(day); cpBuf.Clear(); }
        }

        private CpState CpLoad(DateTime day)
        {
            var st = new CpState();
            try
            {
                string p = RouterConfig.CheckpointPath(day);
                if (!File.Exists(p)) return st;
                st.Exists = true;
                foreach (string line in File.ReadLines(p))
                {
                    st.Lines++;
                    if (line.Length < 2) continue;
                    string[] f = line.Split('|');
                    long n;
                    if (f[0] == "K" && f.Length >= 7)
                    {
                        if (long.TryParse(f[2], NumberStyles.Integer, Inv, out n) && n > 0 && n <= SeqMaxSane && n > st.MaxSeq) st.MaxSeq = n;
                        if (f[1] == "NIN") st.Nin++; else if (f[1] == "HIST") st.Hist++;
                    }
                    else if (f[0] == "S" && f.Length >= 2 && long.TryParse(f[1], NumberStyles.Integer, Inv, out n) && n > 0 && n <= SeqMaxSane && n > st.MaxSeq) st.MaxSeq = n;
                }
            }
            catch (Exception ex) { Debug("CHECKPOINT_LOAD_FAIL | " + Ex(ex)); }
            return st;
        }

        // Buffer one K line for an accepted insert (called from the single insert path).
        private void CpAppendKey(bool nin, long seqN, string key)
        {
            lock (cpLock)
            {
                if (cpPath == null) return;
                // audit3 (#16): hard cap. The disk is not taking the appends and the buffer would otherwise grow without
                // bound on the insert path. Give up on incremental K lines instead and let the next CpRewrite rebuild the
                if (cpBuf.Count >= CpBufHardCap)
                {
                    if (!cpDirty) { cpDirty = true; Debug("CHECKPOINT_BUFFER_FULL | n=" + cpBuf.Count + " | checkpoint marked dirty; rewritten from the store at the next opportunity"); }
                    return;
                }
                if (cpBuf.Count >= CpFlushWatermark) CpFlush(false);   // flush, never drop the K line (cpLock is re-entrant; order dbGate -> cpLock)
                cpBuf.Add("K|" + (nin ? "NIN" : "HIST") + "|" + seqN.ToString(Inv) + "|" + key);
            }
        }

        // Append the buffered K lines plus the current S line. force=true writes an S line even with nothing buffered.
        // audit3 (#16): the whole batch goes out in ONE AppendAllText, so a partial write can no longer duplicate or reorder K
        // lines on a retry; a failure backs off CpFailBackoffSec and keeps the buffer for the next attempt.
        // ROUTER_v02: true only when the buffered lines reached the checkpoint file, or nothing was pending.
        // A forced flush returns false whenever the checkpoint is not confirmed (no path, still dirty, or the append failed).
        private bool CpFlush(bool force)
        {
            DateTime cpDay = currentSessionDay;   // STD36 (N1): snapshot to avoid reading under cpLock
            bool result = false;
            lock (cpLock)
            {
                if (cpPath == null) result = !force;
                else if (cpDirty) result = !force;   // a full rewrite is pending: a forced flush cannot confirm the checkpoint
                else if (cpBuf.Count == 0 && !force) result = true;
                else if (!force && DateTime.UtcNow < cpRetryNotBeforeUtc) result = true;   // backoff: not a failure of this call
                else
                {
                    try
                    {
                        Directory.CreateDirectory(RouterConfig.RouterDataDir);
                        var sb = new System.Text.StringBuilder();
                        if (!File.Exists(cpPath))
                            sb.Append("H|").Append(cpDay.ToString("yyyy-MM-dd", Inv)).Append('|').Append(DateTime.UtcNow.ToString("o", Inv)).Append(Environment.NewLine);
                        for (int i = 0; i < cpBuf.Count; i++) sb.Append(cpBuf[i]).Append(Environment.NewLine);
                        long maxSeq; lock (seqLock) { maxSeq = seqCounter; }
                        sb.Append("S|").Append(maxSeq.ToString(Inv)).Append('|').Append(DateTime.UtcNow.ToString("o", Inv)).Append(Environment.NewLine);
                        File.AppendAllText(cpPath, sb.ToString(), new System.Text.UTF8Encoding(false));
                        cpBuf.Clear();
                        cpRetryNotBeforeUtc = DateTime.MinValue;
                        result = true;
                    }
                    catch (Exception ex)
                    {
                        result = false;
                        cpRetryNotBeforeUtc = DateTime.UtcNow.AddSeconds(CpFailBackoffSec);   // audit3 (#16): do not retry on every append
                        if ((DateTime.UtcNow - cpFlushFailLastUtc).TotalSeconds >= 60)
                        {
                            cpFlushFailLastUtc = DateTime.UtcNow;
                            Debug("CHECKPOINT_FLUSH_FAIL | buffered=" + cpBuf.Count + " | retryBackoffSec=" + CpFailBackoffSec + " | " + Ex(ex));
                        }
                    }
                }
            }
            return result;
        }


        // audit5 (#8): once cpDirty is set (buffer hard cap) K lines are no longer buffered and CpFlush(false) returns early, so the checkpoint
        // stayed incomplete until the next roll / replacement. This rewrites it from the stored rows. dbGate is held across read + rewrite so
        // no insert slips between them (order maintLock > dbGate > cpLock is kept).
        // STD18 (H8): the maintLock wait is BOUNDED. The single-argument Monitor.TryEnter blocked INDEFINITELY, and this runs on the 1 Hz timer
        // thread which already holds dashTimerGate - so a startup / refetch / heal holding maintLock froze the whole dashboard and guaranteed
        // OnStateChange(Terminated)'s TryEnter(dashTimerGate, 1000) failed, leaving the timer undisposed. cpDirty is sticky, so skipping a tick
        // just retries on the next one.
        // STD18 (M2): cpRetryNotBeforeUtc - the field CpFlush already uses - also gates the rebuild, so a permanently failing checkpoint path
        // does not re-read the whole day and rewrite the whole file under dbGate once per second.
        // ROUTER_v02: true when the checkpoint is not dirty after the call. false = still dirty, or the repair was blocked.
        private bool CpRepairDirty(bool takeMaint)
        {
            bool dirty; lock (cpLock) { dirty = cpDirty && cpPath != null; }
            if (!dirty) return true;
            bool inBackoff; lock (cpLock) { inBackoff = DateTime.UtcNow < cpRetryNotBeforeUtc; }
            if (inBackoff) return false;
            bool result = false;
            bool gotMaintRepair = false;
            try
            {
                if (takeMaint) { if (!System.Threading.Monitor.TryEnter(maintLock, 100)) return false; gotMaintRepair = true; }
                lock (dbGate)
                {
                    if (db == null || tickCol == null) result = false;
                    else
                    {
                        DateTime day = currentSessionDay.Date;
                        List<TickRecord> rows = tickCol.Find(x => x.Day == day).Where(x => !x.Superseded).OrderBy(x => x.EventTime).ThenBy(x => x.Id).ToList();
                        CpRewrite(day, rows, "DIRTY_REPAIR");
                        bool stillDirty; lock (cpLock) { stillDirty = cpDirty; }
                        if (stillDirty)
                        {
                            cpRetryNotBeforeUtc = DateTime.UtcNow.AddSeconds(30);
                            Debug("CHECKPOINT_REPAIR_BACKOFF | inSec=30");
                            result = false;
                        }
                        else result = true;
                    }
                }
            }
            catch (Exception ex) { result = false; Debug("CHECKPOINT_DIRTY_REPAIR_FAIL | " + Ex(ex)); }
            finally { if (gotMaintRepair) { try { System.Threading.Monitor.Exit(maintLock); } catch { } } }
            return result;
        }


        // replacement, session roll). The rows are the authoritative store rows; keys are source-scoped.
        // ROUTER_v02 (closure): writes one day's checkpoint file. It touches no global checkpoint state, so it is safe for any day.
        // Returns true only after the file was replaced or moved into place. Failures are logged and reported as false.
        private bool CpWriteFile(DateTime day, List<TickRecord> rows, string why, out int nNinOut, out int nHistOut, out long maxSeqOut)
        {
            long ms = 0; int ni = 0, nh = 0; bool ok = false;
            lock (cpLock)
            {
                try
                {
                    Directory.CreateDirectory(RouterConfig.RouterDataDir);
                    string path = RouterConfig.CheckpointPath(day);
                    string tmp = path + ".tmp";
                    var cpHistOcc = new Dictionary<string, int>();   // occurrence numbers for the K line key of HIST rows
                    using (var sw = new StreamWriter(tmp, false, new System.Text.UTF8Encoding(false)))
                    {
                        sw.WriteLine("H|" + day.ToString("yyyy-MM-dd", Inv) + "|" + DateTime.UtcNow.ToString("o", Inv));
                        foreach (TickRecord r in rows)
                        {
                            bool nin = IsLiveTick(r);
                            if (nin) ni++; else nh++;
                            if (r.SeqN > ms) ms = r.SeqN;
                            string ck = KeyOf(r);
                            if (!nin) { int occN; cpHistOcc.TryGetValue(ck, out occN); occN++; cpHistOcc[ck] = occN; ck = HistKeyN(ck, occN); }
                            sw.WriteLine("K|" + (nin ? "NIN" : "HIST") + "|" + r.SeqN.ToString(Inv) + "|" + ck);
                        }
                        // STD14: S| for the CURRENT day stays max(store maxSeq, seqCounter). For a PAST day it is that day's own max only.
                        if (day.Date == currentSessionDay.Date)
                        {
                            long counter; lock (seqLock) { counter = seqCounter; }
                            if (counter > ms) ms = counter;
                        }
                        sw.WriteLine("S|" + ms.ToString(Inv) + "|" + DateTime.UtcNow.ToString("o", Inv));
                    }
                    if (File.Exists(path)) File.Replace(tmp, path, null); else File.Move(tmp, path);
                    ok = true;
                }
                catch (Exception ex) { ok = false; Log("CHECKPOINT_REWRITE_FAIL | why=" + why + " | " + Ex(ex)); }
            }
            nNinOut = ni; nHistOut = nh; maxSeqOut = ms;
            return ok;
        }

        // ROUTER_v02: returns true only when the checkpoint file was written. Global checkpoint state changes only on success.
        private bool CpRewrite(DateTime day, List<TickRecord> rows, string why)
        {
            int ni, nh; long ms; bool ok;
            lock (cpLock)
            {
                ok = CpWriteFile(day, rows, why, out ni, out nh, out ms);
                if (ok)
                {
                    cpPath = RouterConfig.CheckpointPath(day);
                    cpBuf.Clear();
                    // audit3 (#16): a successful full rewrite also clears a pending "checkpoint dirty" mark and any flush backoff
                    cpDirty = false;
                    cpRetryNotBeforeUtc = DateTime.MinValue;
                }
            }
            if (ok) Log("CHECKPOINT_REWRITE | why=" + why + " | day=" + DayTag(day) + " | nin=" + ni + " | hist=" + nh
                + " | maxSeq=" + (ms > 0 ? SeqText(day, ms) : "none"));
            return ok;
        }


        // =====================================================================
        // =====================================================================

        // =====================================================================
        // =====================================================================
        // audit3 (#6): STICKY "the server kicked us" flag. Shutdown (session terminated / another user) sets it and only a
        // successful Connected clears it. Before, every re-arm passed `false`, so after the first retry a kick decayed to the
        // plain 10/30/60 s table and the 60 -> 600 s ping-pong protection was lost after one attempt.

        // plain drop: 10, 30, 60, 120, 300, 600 s. Server Shutdown (kicked by another session): 60, 120, 240, 480, 600 s.

        // Called when attach could not log in / reach the hub: arm the gap refetch and the first retry.


        // Dashboard tick (1 Hz, timer thread): never blocks; the login runs on a pool thread, one at a time.

        // audit3 (#21): best-effort teardown of an instance that is being discarded. Closes the SignalR connection if the
        // add-in exposes ShutdownConnection, then Dispose() if it is IDisposable. Never throws.

        // NO POPUP: the add-in registers hub.On("Shutdown", ...) whose body is MessageBox.Show(...) then Shutdown event then StopAsync.
        // Remove that handler on the add-in's private _hubConnection and register ours. Idempotent (Remove clears ours too). Never throws.

        // Our replacement for the add-in's Shutdown handler: same flag/log path as before, then stop the terminated connection. No dialog.
        // Shared by all three ShutdownConnection paths: CancellationToken-aware reflection args, invoke, bounded 5 s wait.
        // Callers keep their own catch; all three log Ex(ex) only (Type | Message, never ex.ToString()), so the added frame cannot alter log text.
        // STD20 (H5): catches and logs the fault instead of letting it escape. task.Wait() throws AggregateException and
        // NT8 was restarted. Kept STATIC on purpose: Log() is an instance method, and making this instance would cascade CS0120
        // instead - Gate is a leaf, so apxGate -> Gate introduces no inversion, and Owner is null-safe during an unload fault.


        // =====================================================================

        // Dashboard-timer driver: retry an unconfirmed subscribe at most every 30 s, off the timer thread.

        // "NIFTY-1" -> "NIFTY", "NIFTY" -> "NIFTY". audit3 (#12): the add-in may report the same contract under a different spelling than
        // the ticker ROUTER subscribed; matching on the root keeps classification alive instead of silently falling back.

        // AS-OF lookup over an ordered snapshot (pointer p only moves forward). Same-calendar-day quotes only.

        // PROPOSAL, NOT IMPLEMENTED: an as-of quote lookup with a maximum quote age (QuoteMaxAgeSec) and a bounded
        // scan (QuoteAsOfScan). Neither exists in this build. The live path uses the last received quote with no age check.

        // Persisted (LiteDB QUOTES) + in-memory quotes of one calendar day, ordered, consecutive duplicates removed.
        // audit3 (#3): the persisted store holds only CHANGES and a persisted row therefore has no re-confirmation time.
        // FlushQuotes runs every second, so nearly every in-memory quote already has a persisted twin; the dedup kept the
        // FIRST copy (the persisted one, Seen unset) and threw the in-memory Seen away, which made the STD07 stale-quote rule
        // copy REPLACES its persisted twin (its Seen is always the later one) and only then are consecutive duplicates dropped.

        // =====================================================================
        // Part 2 state
        // =====================================================================

        private readonly object dashLock = new object();
        private long storeRows, storeVolNin;
        private long storeVolBuy = 0;      // STD62: classified BUY lots (raw)
        private long storeVolSell = 0;     // STD62: classified SELL lots (raw)
        private long storeVolNeutral = 0;  // STD62: classified NEUTRAL lots (raw)
        private long storeVolUnknown = 0;  // STD62: classified UNKNOWN lots (raw)
        private long storeMaxDayVol = -1;                                 // max stored NIN DayVolume (R14 rebuild source)
        private long dayVolRaw = -1;                                      // DAY_VOL shown on the dashboard, raw, -1 = unknown
        // STD45: dashboard-only, guarded by dashLock. Last tick time of the day (the @ time on DAY_VOL and STORE).
        private DateTime lastTickT = DateTime.MinValue;

        private long liveReceived, liveInserted, liveDup, livePublished, busPublishFails;
        // ROUTER_v08 (D10): duplicate drops split by whether exchange DayVolume moved past the last kept row.
        private long liveDupDvSame = 0, liveDupDvMoved = 0, liveDupDvUnknown = 0;
        private long liveDupDvMovedRaw = 0;   // ROUTER_v10 (F5): raw volume of dropped duplicates where DayVolume moved
        private long lastKeptDayVolume = -1;     // DayVolume of the last inserted live row; -1 = none this day
        private int  dupMovedLogN = 0;           // caps DUP_DV_MOVED lines per attach
        private DateTime eodWrittenDay = DateTime.MinValue;
        private DateTime eodRetryNotBeforeUtc = DateTime.MinValue;   // STD29 (M1): backoff after a failed EOD write
        private DateTime lastLiveExportUtc = DateTime.MinValue;
        // STD45: dashboard-only. Timestamp of the last RefreshDayVol cycle, used to stamp the DAY_VOL, STORE and
        // RECON lines with the time their numbers refer to.
        private long dashRefreshedAtTicks = 0;
        private int liveExportBusy = 0;

        // ---------- STD06 today's tick log tail (item 1): append-only in steady state ----------
        private readonly object exportLock = new object();   // serialises every write of today's tick log (full rewrite, append, roll export)
        private readonly object tailLock = new object();     // guards tailPending / tailDirty / tailHas / tailTime (lock order: dbGate -> tailLock)
        private readonly List<TickRecord> tailPending = new List<TickRecord>();   // live rows stored since the last export, in file order
        private bool tailDirty = true;                       // true = next export is a full rewrite from LiteDB
        private DateTime tailDay = DateTime.MinValue;
        private string tailPath = null;
        private bool tailHas = false;
        private DateTime tailTime = DateTime.MinValue;       // EventTime of the last row in the file or in tailPending (the watermark)
        private double tailPrevPrice = double.NaN;           // running export state after the last row written (exportLock only)
        private long tailPrevOi = -1, tailCum = 0;
        private bool tailClosedWritten = false;
        private const int TailPendingMax = 200000;
        private DateTime tickFlowLastUtc = DateTime.MinValue;
        private long tickFlowLastRecv, tickFlowLastIns, tickFlowLastDup;
        private long tickFlowLastNeut = 0;   // REC09: per-window baseline for lNeut
        // suppress a whole burst of genuine NIN trades whenever the lot size repeated. A match now DECREMENTS the entry it

        private long lastNinTickTicks = 0;           // UTC ticks of the last NIN Last tick received


        private bool seqContinuityFail = false;
        private string lastLiveSide = null;                         // REC27: last stored BUY/SELL, diagnostic only (NEUTRAL is never inherited since v35)
        private double firstTradePrevCloseForNewDay = double.NaN;   // REC27: snapped prevClose for the day's first live tick      // set by ReloadDayState, spoken by FinalizeDayState

        // ROUTER_v21: FloorDrop = row at or before the history boundary, rejected under dbGate (counted by ProcessLive).
        // ROUTER_v30: Inserted = LiteDB insert returned AND every post-insert step completed. InsertedPartial = LiteDB insert returned (the row is stored)
        // but a post-insert step failed or the key set needed quarantine. Both mean "a row is stored"; InsertedPartial is never an insert failure.
        private enum Ins { Inserted, DroppedDup, Failed, FloorDrop, InsertedPartial }

        // Per-day insert context: SeqNum counter + source-scoped key sets. The current day uses the live fields.
        private sealed class DayCtx
        {
            public DateTime Day;
            public bool IsCurrent;
            public long Counter;                 // used only when !IsCurrent
            public HashSet<string> Nin;
            public HashSet<string> Hist;         // recovered NT history rows: occurrence-suffixed keys (HistKeyN)
        }

        private DayCtx CurrentCtx()
        {
            return new DayCtx { Day = currentSessionDay.Date, IsCurrent = true, Nin = keyNin, Hist = keyHist };
        }

        private long AllocFor(DayCtx c) { if (c.IsCurrent) return AllocSeqN(); c.Counter++; return c.Counter; }
        private long CurrentSeq() { lock (seqLock) { return seqCounter; } }

        // =====================================================================
        // helpers
        // =====================================================================
        private long LotsOf(long rawVol) { return rawVol / (LotSize > 0 ? LotSize : 1); }
        private static string FmtPrice(double p) { return p.ToString("0.00", Inv); }

        // audit3 (#9): OI-FREE twin key (second | price | volume). NIN OI is a cached side-channel value updated by the
        // The STORED key (KeyOf) is deliberately unchanged - stored days and checkpoints depend on it - so this is a
        // matching-only key and OI is demoted to a tie-breaker.
        private static string PvoNoOi(double price, long vol) { return price.ToString("R", Inv) + "|" + vol.ToString(Inv); }

        // STD49: 4-argument form kept for DirOrClassify (CSV / replay). It forwards with unknown sizes, so the mid-spread tie-breaker never fires here.
        private static string ClassifyTick(double price, double bid, double ask, double prev)
        {
            return ClassifyTick(price, bid, ask, prev, -1, -1, 0);
        }

        // DIAG (REC06_DIAG): which ClassifyTick branch fired on this thread. Diagnostic only, never read by logic.
        [ThreadStatic] private static string classifyBranch;
        private static void DiagCount(Dictionary<string, int> m, string key)
        {
            string k = key ?? "NA"; int c; m.TryGetValue(k, out c); m[k] = c + 1;
        }
        private static string DiagTally(Dictionary<string, int> m)
        {
            return string.Join(",", m.Keys.OrderBy(k => k, StringComparer.Ordinal).Select(k => k + ":" + m[k]));
        }

        private static string ClassifyTick(double price, double bid, double ask, double prev, long bidQty, long askQty, long tradeVol)
        {
            bool usable = !double.IsNaN(bid) && bid > 0 && !double.IsNaN(ask) && ask > 0 && bid <= ask;
            if (usable)
            {
                if (bid < ask)
                {
                    if (price >= ask) { classifyBranch = "ASK"; return "BUY"; }
                    if (price <= bid) { classifyBranch = "BID"; return "SELL"; }
                    // STD49: strictly inside the spread. Sizes are raw contracts, same unit as tradeVol (SB-0). A size <= 0 means unknown
                    // (-1 = no size, 0 = empty side), so the comparison is skipped; without that guard tradeVol(0) >= -1 would fire on the 4-argument path.
                    double mid = (bid + ask) / 2.0;
                    if (askQty > 0 && price > mid && tradeVol >= askQty) { classifyBranch = "MIDSIZE_ASK"; return "BUY"; }
                    if (bidQty > 0 && price < mid && tradeVol >= bidQty) { classifyBranch = "MIDSIZE_BID"; return "SELL"; }
                    classifyBranch = price == mid ? "MID_EXACT" : "INSIDE_NOSIZE";
                    return "NEUTRAL";   // exact mid stays NEUTRAL
                }
                // locked book (bid == ask): a trade AT the locked price is NEUTRAL, not BUY
                classifyBranch = "LOCKED";
                return "NEUTRAL";
            }
            if (double.IsNaN(prev)) { classifyBranch = "NOPREV"; return "NEUTRAL"; }
            classifyBranch = price > prev ? "PREV_UP" : price < prev ? "PREV_DOWN" : "PREV_EQ";
            return price > prev ? "BUY" : price < prev ? "SELL" : "NEUTRAL";
        }

        private static double QuoteOrNaN(double? q) { return q.HasValue ? q.Value : double.NaN; }
        private static string DirOrClassify(TickRecord r, double prev)
        {
            return r.Direction ?? ClassifyTick(r.Price, QuoteOrNaN(r.BidPrice), QuoteOrNaN(r.AskPrice), prev);
        }

        private static string ModeName(int m)
        {
            switch (m)
            {
                case ModeWaitOpen:  return "WAIT_OPEN";
                case ModeLiveNin:   return "LIVE_NIN";
                case ModeLateLogin: return "LATE_LOGIN";
                case ModeSkipped:   return "SKIPPED_NON_TRADING_DAY";
                default:            return "NONE";
            }
        }

        private DateTime TradingDayBack(DateTime day, int n)
        {
            DateTime d = day.Date;
            for (int i = 0; i < n; i++) d = PrevTradingDate(d);
            return d;
        }

        // trading days in (older, newer]
        private int TradingDaysBetween(DateTime older, DateTime newer)
        {
            int n = 0; DateTime d = older.Date;
            while (d < newer.Date && n < 1000) { d = d.AddDays(1); if (IsTradingDate(d)) n++; }
            return n;
        }

        private bool HasTicks(DateTime day) { lock (dbGate) { return tickCol != null && tickCol.Exists(x => x.Day == day); } }


        // STD07-AUDIT (item 10): marker day|firstSeq|lastSeq|complete. Called only after a SUCCESSFUL fetch whose window covered
        // the day. firstSeq/lastSeq are the stored rows' SeqN bounds at write time (0 when the day is empty). Upsert by Day.

        // STD39: ACTIVE rows only (Superseded == false). Filtered in memory: rows written before STD39 have no Superseded
        // field, and a LiteDB-side comparison would not treat a missing field as false.
        private List<TickRecord> StoreTicksOrdered(DateTime day)
        {
            lock (dbGate) { return tickCol.Find(x => x.Day == day).Where(x => !x.Superseded).OrderBy(x => x.EventTime).ThenBy(x => x.Id).ToList(); }
        }

        // STD39: raw view including Superseded rows. Used ONLY to recover the SeqNum counter.
        private List<TickRecord> StoreTicksAll(DateTime day)
        {
            lock (dbGate) { return tickCol.Find(x => x.Day == day).ToList(); }
        }

        // twin with the flag. Nothing is deleted.

        // STD39: the one-trade-one-row invariant. Two ACTIVE rows with one SeqNum is a bug, and is logged the same day.
        private void CheckSeqInvariant(DateTime day, List<TickRecord> activeRows, string why)
        {
            try
            {
                var seen = new HashSet<long>();
                int shared = 0; long firstShared = 0;
                foreach (TickRecord r in activeRows)
                {
                    if (r.SeqN <= 0) continue;
                    if (!seen.Add(r.SeqN)) { shared++; if (firstShared == 0) firstShared = r.SeqN; }
                }
                if (shared > 0)
                    Log("INVARIANT_FAIL | why=" + why + " | day=" + DayTag(day) + " | activeRowsSharingSeqNum=" + shared + " | first=" + SeqText(day, firstShared));
                else
                    Debug("INVARIANT_OK | why=" + why + " | day=" + DayTag(day) + " | activeRows=" + activeRows.Count);
            }
            catch (Exception ex) { Debug("INVARIANT_CHECK_EX | " + Ex(ex)); }
        }

        private bool HoldingDay()
        {
            DateTime now = DateTime.Now;
            return !IsTradingDate(now.Date) || now.TimeOfDay < SessionStartTod;
        }

        // =====================================================================
        // stages
        // =====================================================================
        private void Stage(string name, Action work, bool fatal)
        {
            lastStageName = name;
            var sw = Stopwatch.StartNew();
            LogBoth("STAGE_BEGIN | " + name);
            try
{
                work();
                sw.Stop();
                Log("STAGE_OK | " + name + " | elapsedMs=" + sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                sw.Stop();
                Log("STAGE_FAIL | " + name + " | fatal=" + fatal + " | elapsedMs=" + sw.ElapsedMilliseconds + " | " + Ex(ex));
                Debug("STAGE_FAIL | " + name + " | fatal=" + fatal + " | " + ex.ToString());
                if (fatal) throw;
                startupNonFatalFailures++;   // STD28 (audit): track non-fatal failures
            }
        }

        private void OpenDb()
        {
            try
            {
                lock (dbGate)
                {
                    // v44 LITEDB_PATH_MOVED guard: the store location is now derived from UserDataDir. If that resolved file is absent but the
                    // old hardcoded file exists elsewhere, history is there and this path is wrong. Stop instead of creating an empty database.
                    try
                    {
                        string resolvedFull = Path.GetFullPath(RouterConfig.LiteDbPath);
                        string legacyFull = Path.GetFullPath(RouterConfig.LegacyLiteDbPath);
                        if (!string.Equals(resolvedFull, legacyFull, StringComparison.OrdinalIgnoreCase)
                            && !File.Exists(resolvedFull) && File.Exists(legacyFull))
                        {
                            Log("STARTUP_ERROR | LITEDB_PATH_MOVED | resolved=" + resolvedFull + " (missing) | legacy=" + legacyFull + " (exists) | not opening an empty store");
                            throw new InvalidOperationException("LITEDB_PATH_MOVED: resolved store path is missing but the legacy store exists");
                        }
                    }
                    catch (InvalidOperationException) { throw; }
                    catch (Exception gex) { Debug("LITEDB_PATH_GUARD_EX | " + Ex(gex)); }
                    foreach (string d in new[] { RouterConfig.RootDir, RouterConfig.LiteDbDir, RouterConfig.LogDir, RouterConfig.PastLogDir, RouterConfig.RouterDataDir })
                        Directory.CreateDirectory(d);
                    // STD40 (A1): OpenDb is idempotent. A startup retry re-enters this stage; a handle left open by the failed attempt is disposed first.
                    lock (dbGate) { try { if (db != null) { db.Dispose(); db = null; } } catch { } }
                    minuteCol = null; tickCol = null;
                    db = new LiteDatabase("Filename=" + RouterConfig.LiteDbPath + ";Connection=direct");
                    minuteCol = db.GetCollection<MinuteRecord>("minute");
                    tickCol   = db.GetCollection<TickRecord>("ticks");
                    minuteCol.EnsureIndex(x => x.Day);
                    // composite for the Day + EventTime range/order pattern used by
                    minuteCol.EnsureIndex(x => new { x.Day, x.EventTime });
                    tickCol.EnsureIndex(x => x.Day);
                    // composite for the Day + EventTime range/order pattern used by
                    tickCol.EnsureIndex(x => new { x.Day, x.EventTime });
                    Log("LITEDB_OPEN | path=" + RouterConfig.LiteDbPath + " | minute=" + minuteCol.Count() + " | ticks=" + tickCol.Count());
                }
            }
            catch (Exception ex)
            {
                Log("LITEDB_OPEN_FAIL | " + Ex(ex));
                throw;
            }
        }

        // ROUTER_STD81_RECOVER: recovers today's NinjaTrader native 1-Tick history (09:15:00 -> attach) into LiteDB with Phase "HIST".
        // Runs once from RunStartup. Skipped if the store already has any row for today. Dedupe: KeyOf plus an occurrence
        // counter, so identical trades (same time, price, volume, OI) are each kept once. InsertCore(bulk: true): no bus publish,
        // no counter bump, no checkpoint line. The single insert path (InsertCore) is still used.
        // REC15: READ-ONLY integrity check. Compares stored HIST rows with the NinjaTrader candidates for the same window, per identity key.
        // Writes nothing to the store. Runs at recovery, before the insert loop, so the store is still in its pre-recovery state.
        private static void IncCount(Dictionary<string, int> m, string k) { int c; m.TryGetValue(k, out c); m[k] = c + 1; }
        private bool OnTickGrid(double p)
        {
            double tick = (Instrument != null && Instrument.MasterInstrument != null) ? Instrument.MasterInstrument.TickSize : 0.0;
            if (!(tick > 0)) tick = 0.05;
            double q = p / tick;
            return Math.Abs(q - Math.Round(q)) < 1e-6;
        }
        private void DiagStoreIntegrity(DateTime day, List<TickRecord> cands)
        {
            try
            {
                List<TickRecord> stored;
                lock (dbGate) { stored = tickCol == null ? new List<TickRecord>() : tickCol.Find(x => x.Day == day && !x.Superseded).ToList(); }
                var storedHist = new Dictionary<string, int>();
                var storedLive = new HashSet<string>();
                int liveOffGrid = 0, liveRows = 0;
                foreach (TickRecord r in stored)
                {
                    string k = KeyOf(r);
                    if (r.Phase == "HIST") IncCount(storedHist, k);
                    else if (r.Phase == "LIVE") { storedLive.Add(k); liveRows++; if (!OnTickGrid(r.Price)) liveOffGrid++; }
                }
                var candCount = new Dictionary<string, int>();
                foreach (TickRecord r in cands) IncCount(candCount, KeyOf(r));

                int excessRows = 0, excessKeys = 0, missingRows = 0, missingKeys = 0, mixedKeys = 0;
                var excessList = new List<string>();
                foreach (KeyValuePair<string, int> kv in storedHist)
                {
                    int c; candCount.TryGetValue(kv.Key, out c);
                    if (kv.Value > c)
                    {
                        excessRows += kv.Value - c; excessKeys++;
                        if (excessList.Count < 20) excessList.Add(kv.Key + " | stored=" + kv.Value + " | candidates=" + c);
                    }
                    if (storedLive.Contains(kv.Key)) mixedKeys++;
                }
                foreach (KeyValuePair<string, int> kv in candCount)
                {
                    int sh; storedHist.TryGetValue(kv.Key, out sh);
                    if (kv.Value > sh) { missingRows += kv.Value - sh; missingKeys++; }
                }
                Log("STORE_INTEGRITY | day=" + DayTag(day) + " | storedHist=" + storedHist.Values.Sum() + " | storedLive=" + liveRows
                    + " | candidates=" + cands.Count + " | excessRows=" + excessRows + " | excessKeys=" + excessKeys
                    + " | missingRows=" + missingRows + " | missingKeys=" + missingKeys
                    + " | mixedPhaseKeys=" + mixedKeys + " | liveOffGrid=" + liveOffGrid);
                foreach (string line in excessList) Log("STORE_INTEGRITY_EXCESS | " + line);
            }
            catch (Exception ex) { Log("STORE_INTEGRITY_FAIL | " + Ex(ex)); }
        }

        // REC17: ONE procedure for every attach (late login, missed attach, shutdown, normal start). Replaces RecoverTodayFromNativeSeries.
        // 1. Read NinjaTrader's native 1-Tick history from 09:15 to the attach moment (the only backfill source).
        // 2. Validate BEFORE touching the store: if the read has fewer trades than the stored HIST rows for the same window, retain and log HISTORY_SHORT.
        // 3. Rewrite only the window [09:15, last history row] in one LiteDB transaction. Live rows after the boundary are preserved.
        // 4. Sequence numbers are positional: the window is re-numbered 1..H in NinjaTrader order; preserved live rows follow at H+1...
        // 5. Live capture starts STRICTLY after the last history row; live ticks at or before it are dropped as BOUNDARY_DROP.
        // The checkpoint is rewritten by the ATTACH_END CpRewrite that follows this stage. Store and checkpoint are not rolled back together.
        private long liveFloorTicks = -1;   // REC24: -1 = not set. One field, one read, one write (atomic).
        private long liveDbNullLogN = 0;   // ROUTER_v25: counts LIVE_DB_NULL_DROP MAIN lines; the first 50 are logged, the drop itself is always counted
        private long liveFloorDropped = 0;  // ROUTER_v18: live rows dropped at or before the floor on the insert path (covered by history)
        // ===== v26-DIAG: accounting ledger. Counters only. ledgerLock is a leaf: no I/O, no logging, no other lock inside. =====
        private const int X_NULL = 0, X_GUARD = 1, X_QUOTE_BID = 2, X_QUOTE_ASK = 3, X_DAYVOL = 4, X_OI = 5,
            X_OTHER = 6, X_TIMEMIN = 7, X_VOLNEG = 8, X_FLOOR_INGEST = 9, X_EXC = 10, X_COUNT = 11;
        private const int P_QUEUE_FULL = 0, P_QUEUE_GONE = 1, P_ROLL_DROP = 2, P_FLOOR_EARLY = 3, P_DB_NULL = 4,
            P_DAY_MISMATCH = 5, P_INSERT_RETURNED = 6, P_DUP = 7, P_FLOOR_LOCKED = 8, P_FAILED_CLEAN = 9,
            P_FAILED_UNCERTAIN = 10, P_HANDOFF_FAILED = 11, P_PEND_OVER = 12, P_COUNT = 13;
        private readonly object ledgerLock = new object();
        private readonly long[] ledgerX = new long[X_COUNT];
        private readonly long[] ledgerP = new long[P_COUNT];
        private long ledgerE, ledgerA, ledgerO, ledgerContractViolation, ledgerDupTerminal, ledgerPostInsertExc, ledgerPostTerminalExc;
        private long ledgerCallbacksEntered;   // Interlocked; diagnostic, APPROX (not read under ledgerLock)

        // Ingress resolution: one call per callback. 'resolved' is the callback's own local (one thread).
        private void LedgerIngress(ref bool resolved, int code)
        {
            lock (ledgerLock)
            {
                if (resolved) { ledgerContractViolation++; return; }
                resolved = true;
                ledgerE++;
                ledgerX[code]++;
            }
        }

        // Accept: E, A and O each +1 in one ledgerLock region, together with resolved = true.
        // ASSUMPTION (not guaranteed by the lock): the transition runs to completion. The lock serialises concurrent readers and
        // writers; it does not make the sequence atomic against thread abort or out-of-memory. Setting resolved first means a
        // mid-transition interruption cannot produce a second (X_EXC) resolution; it can leave E/A/O unbalanced, which I1/I2 would show.
        private void LedgerAccept(ref bool resolved, TickRecord r)
        {
            lock (ledgerLock)
            {
                if (resolved) { ledgerContractViolation++; return; }
                resolved = true;
                ledgerE++; ledgerA++; ledgerO++;
                r.Accepted = true;
            }
        }

        // Producer catch only: records P_HANDOFF_FAILED if and only if no terminal exists yet. Never counts a duplicate.
        private void HandOffFailed(TickRecord r)
        {
            lock (ledgerLock)
            {
                if (!r.Accepted || r.Recorded) return;
                r.Recorded = true;
                r.TerminalCode = P_HANDOFF_FAILED;
                ledgerP[P_HANDOFF_FAILED]++;
                ledgerO--;
            }
        }

        // Pipeline terminal for an accepted tick. Lowers O at most once. Returns true only if this call recorded it.
        private bool PipelineTerminal(TickRecord r, int code)
        {
            lock (ledgerLock)
            {
                if (!r.Accepted) { ledgerContractViolation++; return false; }
                if (r.Recorded) { ledgerDupTerminal++; return false; }
                r.Recorded = true;
                r.TerminalCode = code;
                ledgerP[code]++;
                ledgerO--;
                return true;
            }
        }

        // Exception in ProcessLive: classified only by what is already established on the tick.
        private void LedgerProcessException(TickRecord r, int stage)
        {
            lock (ledgerLock)
            {
                if (!r.Accepted) { ledgerContractViolation++; return; }
                if (r.Recorded)
                {
                    if (r.TerminalCode == P_INSERT_RETURNED) ledgerPostInsertExc++;   // row confirmed inserted; bookkeeping threw
                    else ledgerPostTerminalExc++;
                    return;
                }
                int code;
                if (stage == 0) code = P_FAILED_CLEAN;            // no insert entered: no row written
                else if (stage == 1) code = P_FAILED_UNCERTAIN;   // insert entered, did not return: row may exist
                else { ledgerContractViolation++; return; }       // stage 2 without a record: not classified
                r.Recorded = true;
                r.TerminalCode = code;
                ledgerP[code]++;
                ledgerO--;
            }
        }

        private void LedgerContractViolation()
        {
            lock (ledgerLock) { ledgerContractViolation++; }
        }

        // One LEDGER line per TICK_FLOW window that reaches the snapshot call. It is NOT emitted for windows skipped by the
        // TickFlowIntervalSec gate, the first (baseline) call, or POST_CLOSE_QUIET windows. Snapshot under ledgerLock; written outside it.
        // I1 = E - sumX - A and I2 = A - sumP - O are exact for this snapshot (only ledger counters are included).
        private void LedgerLine()
        {
            long e, a, o, cv, dup, pie, pte;
            long[] x = new long[X_COUNT], p = new long[P_COUNT];
            lock (ledgerLock)
            {
                e = ledgerE; a = ledgerA; o = ledgerO; cv = ledgerContractViolation;
                dup = ledgerDupTerminal; pie = ledgerPostInsertExc; pte = ledgerPostTerminalExc;
                Array.Copy(ledgerX, x, X_COUNT);
                Array.Copy(ledgerP, p, P_COUNT);
            }
            long cb = Interlocked.Read(ref ledgerCallbacksEntered);
            long sx = 0, sp = 0;
            for (int i = 0; i < X_COUNT; i++) sx += x[i];
            for (int i = 0; i < P_COUNT; i++) sp += p[i];
            Log("LEDGER | snap=LOCKED_COUNTERS | I1=" + (e - sx - a) + " | I2=" + (a - sp - o)
                + " | E=" + e + " | sumX=" + sx + " | A=" + a + " | sumP=" + sp + " | O=" + o
                + " | contractViolation=" + cv + " | dupTerminal=" + dup + " | postInsertExc=" + pie
                + " | postTerminalExc=" + pte + " | callbacksEntered(APPROX)=" + cb
                + " | X=" + string.Join(",", x) + " | P=" + string.Join(",", p));
        }
        // ===== end v26-DIAG ledger =====
        private int oiZeroLogN = 0;   // REC20 diagnostic counter

        private void RewriteTodayFromNativeSeries()
        {
            DateTime day = currentSessionDay.Date;
            DateTime windowStart = day.Add(MarketOpenTod);
            DateTime cutoff = attachEventTime;
            RecomputeClassTotals(day);   // REC49: baseline from the store before any rewrite, so a skipped or rolled-back rewrite still reports store truth
            // ROUTER_v13 (PAST_DAY_FROZEN): a session day before today is closed history. Before 09:00 the session day is still
            // yesterday, so every restart used to rewrite yesterday with a new pairing (14 Oct: 3,117 / 7,024 / 7,024 covered on
            // three restarts of the same data). Past days are read only; the stored rows are kept.
            if (day < DateTime.Now.Date)
            {
                Log("NATIVE_HISTORY | skipped | reason=past_day_frozen | day=" + DayTag(day) + " | today=" + DayTag(DateTime.Now.Date) + " | store kept");
                return;
            }
            try
            {
                var sw = Stopwatch.StartNew();
                while ((!histTransitionSeen || HistCapturedCount() == 0) && sw.Elapsed.TotalSeconds < HistRecoverWaitSec && !OwnerGone())
                    Thread.Sleep(200);
                if (HistCapturedCount() == 0) { Log("NATIVE_HISTORY | skipped | reason=no_bars_captured | transition=" + histTransitionSeen); return; }
                if (!histTransitionSeen) Log("NATIVE_HISTORY | partial | reason=transition_not_seen");

                List<TickRecord> hist;
                lock (histRecover)
                {
                    // REC20: copy only. The capture is cleared after a successful commit, so a retained rewrite keeps it.
                    hist = new List<TickRecord>(histRecover);
                }
                // NinjaTrader order: stable sort by EventTime keeps capture order for equal timestamps.
                hist = hist.Where(x => x.EventTime >= windowStart && x.EventTime <= cutoff).OrderBy(x => x.EventTime).ToList();
                if (hist.Count == 0) { Log("NATIVE_HISTORY | empty | day=" + DayTag(day) + " | retained"); return; }

                DateTime boundary = hist[hist.Count - 1].EventTime;   // lastHistoryRow.EventTime
                // REC20: the live floor is NOT set here. It is set only after the rewrite commits (see below).
                Log("NATIVE_HISTORY | read | day=" + DayTag(day) + " | trades=" + hist.Count
                    + " | first=" + hist[0].EventTime.ToString(TsFmt, Inv) + " | boundary=" + boundary.ToString(TsFmt, Inv));

                DiagStoreIntegrity(day, hist);   // read-only

                // Direction for history rows: unchanged rule (price vs previous, REC08 inherit). No quotes exist retroactively.
                // v15 (PREV_SEED): the first history trade is compared with the previous close, as the live path does.
                // The looked-up value is used here because SEED_LIVE_SIDE (ComputeFirstSideForNewDay) runs after this stage.
                double prevHist = double.NaN;
                {
                    DateTime seedFrom;
                    double seedClose = PrevStoredClose(day, out seedFrom);
                    if (!double.IsNaN(seedClose)) prevHist = SnapToTick(seedClose);
                }
                var brHist = new Dictionary<string, int>();
                foreach (TickRecord cr in hist)
                {
                    // v35: NEUTRAL stays NEUTRAL (no carry-forward). Price reference is the previous TICK.
                    string d = ClassifyTick(cr.Price, double.NaN, double.NaN, prevHist, -1, -1, cr.Volume);
                    DiagCount(brHist, classifyBranch);
                    cr.Direction = d;
                    prevHist = cr.Price;
                }
                Log("HIST_CLASSIFY | day=" + DayTag(day) + " | total=" + hist.Count + " | branches=" + DiagTally(brHist));

                lock (dbGate)
                {
                    if (db == null || tickCol == null) { Log("NATIVE_HISTORY | skipped | reason=db_closed"); return; }

                    // REC49 classification rules. A row captured from the live feed is LIVE and is never deleted or relabelled by a rewrite.
                    // HIST rows are derived from NinjaTrader history and are rebuilt on every attach. A native trade is covered when a stored
                    // LIVE row has the same identity (time to the millisecond, tick-grid price, volume). Covered trades are not inserted again.
                    DateTime wsLocal = windowStart, bLocal = boundary;   // locals for the LiteDB expressions
                    List<TickRecord> windowRows = CaptureWindowRows(day, wsLocal, bLocal);
                    List<TickRecord> storedLive = windowRows.Where(x => x.Phase == "LIVE").ToList();
                    long storedLiveRaw = 0;
                    foreach (TickRecord lr in storedLive) storedLiveRaw += lr.Volume;

                    bool[] covered = PlanPhaseCarry(hist, storedLive);
                    DumpPairing(day, hist, storedLive, covered);   // ROUTER_v11 (DIAG_PAIRING): read-only, outside PHASE_MATCH
                    // v41 append-only gate. A native trade is inserted only when no active stored row (LIVE or HIST) has the same exact key
                    // (EventTime.Ticks, tick-grid price, volume; OI excluded). Counts are per key, so two genuine identical native prints
                    // need two stored rows. Natives covered by the LIVE tolerance pairing (PlanPhaseCarry) are never inserted again.
                    var presentCount = new Dictionary<string, int>();
                    foreach (TickRecord pr in windowRows)
                    {
                        string pk = ExactKeyOf(pr.EventTime.Ticks, pr.Price, pr.Volume);
                        int pc; presentCount.TryGetValue(pk, out pc); presentCount[pk] = pc + 1;
                    }
                    var addList = new List<TickRecord>();
                    long coveredRaw = 0, addRawPlanned = 0, alreadyStoredRaw = 0;
                    int liveCoveredN = 0, alreadyStoredN = 0, tolCoveredOnlyN = 0;
                    for (int ci = 0; ci < hist.Count; ci++)
                    {
                        TickRecord nr = hist[ci];
                        string nk = ExactKeyOf(nr.EventTime.Ticks, nr.Price, nr.Volume);
                        int have; presentCount.TryGetValue(nk, out have);
                        bool exactAvail = have > 0;
                        if (exactAvail) presentCount[nk] = have - 1;
                        if (covered[ci]) { liveCoveredN++; coveredRaw += nr.Volume; }
                        if (covered[ci] || exactAvail)
                        {
                            alreadyStoredN++; alreadyStoredRaw += nr.Volume;
                            if (!exactAvail) tolCoveredOnlyN++;   // covered by LIVE tolerance pairing only (informational)
                        }
                        else { addList.Add(nr); addRawPlanned += nr.Volume; }
                    }
                    long storedUnpaired = 0;
                    foreach (int leftover in presentCount.Values) storedUnpaired += leftover;   // stored rows with no native twin: kept, counted
                    int liveUnmatchedN = storedLive.Count - liveCoveredN;
                    long liveUnmatchedRaw = storedLiveRaw - coveredRaw;

                    List<long> plannedOi; string abortReason, abortDetail; int validStored, carriedN, unknownN;
                    bool planOk = PlanOiCarry(day, addList, windowRows, out plannedOi, out abortReason, out abortDetail, out validStored, out carriedN, out unknownN);
                    if (!planOk)
                    {
                        Log("NATIVE_HISTORY | OI_ABORT | day=" + DayTag(day) + " | reason=" + abortReason + " | " + abortDetail + " | retained");
                        return;
                    }
                    // REC49 audit field: LIVE rows of the day that lie outside the window (kept as they are, logged separately).
                    List<TickRecord> liveOutRows = tickCol.Find(x => x.Day == day && !x.Superseded && x.Phase == "LIVE" && !(x.EventTime >= wsLocal && x.EventTime <= bLocal)).ToList();
                    long liveOutRaw = 0;
                    foreach (TickRecord lo in liveOutRows) liveOutRaw += lo.Volume;
                    Log("NATIVE_HISTORY | PHASE_LEDGER | day=" + DayTag(day) + " | history=" + hist.Count
                        + " | live_stored=" + storedLive.Count + " | live_stored_lots=" + LotsOf(storedLiveRaw)
                        + " | covered_by_live=" + liveCoveredN + " | already_stored=" + alreadyStoredN + " | hist_to_add=" + addList.Count + " | hist_to_add_lots=" + LotsOf(addRawPlanned)
                        + " | live_unmatched=" + liveUnmatchedN + " | live_unmatched_lots=" + LotsOf(liveUnmatchedRaw)
                        + " | oi_carried=" + carriedN
                        + " | live_outside_window=" + liveOutRows.Count + " | live_outside_window_lots=" + LotsOf(liveOutRaw));

                    DayCtx ctx = CurrentCtx();
                    List<TickRecord> liveBeforeRows = tickCol.Find(x => x.Day == day && !x.Superseded && x.Phase == "LIVE").ToList();
                    int liveBefore = liveBeforeRows.Count;
                    long liveBeforeRaw = 0;
                    foreach (TickRecord lr in liveBeforeRows) liveBeforeRaw += lr.Volume;

                    int insertedN = 0;
                    long addedRaw = 0;
                    db.BeginTrans();
                    try
                    {
                        // v41 append-only: no stored row is deleted, relabelled or renumbered. New SeqNums continue above the store maximum.
                        long storeMaxSeq = 0;
                        foreach (TickRecord x in StoreTicksAll(day)) if (x.SeqN > storeMaxSeq) storeMaxSeq = x.SeqN;
                        RaiseSeqTo(storeMaxSeq);
                        for (int pi = 0; pi < addList.Count; pi++)
                        {
                            TickRecord h = addList[pi];
                            var r = new TickRecord
                            {
                                EventTime = h.EventTime, Day = h.Day, Price = h.Price, Volume = h.Volume,
                                OpenInterest = plannedOi[pi], Phase = "HIST", DayVolume = h.DayVolume,
                                Direction = h.Direction, BidPrice = h.BidPrice, AskPrice = h.AskPrice
                            };
                            InsertCore(ctx, r, true);
                            insertedN++; addedRaw += r.Volume;
                        }
                        // Invariant: the LIVE rows of the day are exactly the rows that existed before the rewrite.
                        List<TickRecord> liveAfterRows = tickCol.Find(x => x.Day == day && !x.Superseded && x.Phase == "LIVE").ToList();
                        long liveAfterRaw = 0;
                        foreach (TickRecord lr in liveAfterRows) liveAfterRaw += lr.Volume;
                        if (liveAfterRows.Count != liveBefore || liveAfterRaw != liveBeforeRaw)
                            throw new InvalidOperationException("LIVE_INVARIANT_FAILED | before=" + liveBefore + "/" + liveBeforeRaw + " | after=" + liveAfterRows.Count + "/" + liveAfterRaw);
                        db.Commit();
                    }
                    catch (Exception ex)
                    {
                        db.Rollback();
                        Log("NATIVE_HISTORY | ROLLED_BACK | " + Ex(ex));
                        RebuildKeysFromStore(day);
                        long mx = 0;
                        foreach (TickRecord x in StoreTicksAll(day)) if (x.SeqN > mx) mx = x.SeqN;
                        ResetSeqForDay(); RaiseSeqTo(mx);
                        return;
                    }

                    Interlocked.Exchange(ref liveFloorTicks, boundary.Ticks);
                    lock (histRecover) { histRecover.Clear(); }

                    // Totals come from the store, so they are the same after any number of restarts.
                    long liveStoreRaw = 0, derivedStoreRaw = 0;
                    foreach (TickRecord row in tickCol.Find(x => x.Day == day && !x.Superseded).ToList())
                    {
                        if (row.Phase == "LIVE") liveStoreRaw += row.Volume;
                        else derivedStoreRaw += row.Volume;
                    }
                    lock (totalsLock) { lateLoginRaw = derivedStoreRaw; liveRaw = liveStoreRaw; }
                    // REC49 account identity: store window volume = native window volume + unmatched LIVE volume.
                    // A non-zero unresolved count means a LIVE row has no native twin: it may be a true extra trade or a duplicate. Not auto-resolved.
                    long winStoreRaw = 0;
                    foreach (TickRecord wr in tickCol.Find(x => x.Day == day && !x.Superseded && x.EventTime >= wsLocal && x.EventTime <= bLocal).ToList())
                        winStoreRaw += wr.Volume;
                    long winNativeRaw = 0;
                    foreach (TickRecord nat in hist) winNativeRaw += nat.Volume;
                    long winDiff = winStoreRaw - winNativeRaw;
                    // ROUTER_v10 (F2): OK only when volumes reconcile AND no LIVE row is unmatched.
                    bool accountMatch = winDiff == liveUnmatchedRaw;
                    string accountVerdict = !accountMatch ? "ACCOUNT_MISMATCH" : (liveUnmatchedN > 0 ? "ACCOUNT_UNRESOLVED" : "ACCOUNT_OK");
                    Log("NATIVE_HISTORY | " + accountVerdict + " | day=" + DayTag(day)
                        + " | store_window_raw=" + winStoreRaw + " | native_window_raw=" + winNativeRaw + " | diff_raw=" + winDiff
                        + " | live_unmatched_raw=" + liveUnmatchedRaw + " | unresolved_live_unmatched=" + liveUnmatchedN);

                    RebuildKeysFromStore(day);
                    RebuildCounters(day);
                    Log("NATIVE_HISTORY | REWRITTEN | day=" + DayTag(day) + " | history=" + hist.Count + " | append_only=true | already_stored=" + alreadyStoredN + " | stored_unpaired=" + storedUnpaired + " | phase_covered_not_exact=" + tolCoveredOnlyN
                        + " | hist_added=" + insertedN + " | hist_added_lots=" + LotsOf(addedRaw)
                        + " | live_kept=" + liveBefore + " | live_kept_lots=" + LotsOf(liveBeforeRaw)
                        + " | store_live_lots=" + LotsOf(liveStoreRaw) + " | store_latelogin_lots=" + LotsOf(derivedStoreRaw)
                        + " | boundary=" + boundary.ToString(TsFmt, Inv) + " | checkpoint=ATTACH_END");
                }
            }
            catch (Exception ex) { Log("NATIVE_HISTORY | FAIL | " + Ex(ex)); }
        }

        // REC42: read-only. Every active row of the day inside the window, any phase (same set DeleteMany removes).
        // REC49: LIVE and LATELOGIN totals are derived from the store for the session day, never from rewrite arithmetic alone.
        private void RecomputeClassTotals(DateTime day)
        {
            long liveS = 0, derS = 0;
            lock (dbGate)
            {
                if (tickCol == null) return;
                foreach (TickRecord row in tickCol.Find(x => x.Day == day && !x.Superseded).ToList())
                {
                    if (row.Phase == "LIVE") liveS += row.Volume;
                    else derS += row.Volume;
                }
            }
            lock (totalsLock) { lateLoginRaw = derS; liveRaw = liveS; }
        }

        private List<TickRecord> CaptureWindowRows(DateTime day, DateTime ws, DateTime b)
        {
            return tickCol.Find(x => x.Day == day && !x.Superseded && x.EventTime >= ws && x.EventTime <= b)
                          .OrderBy(x => x.EventTime).ThenBy(x => x.Id).ToList();
        }

        // REC44: pure plan, no database access. planned[i] = OI for the i-th captured (native) row.
        // Stored OI is matched to native rows by trade identity (time to the millisecond, tick-grid price, volume), not by SeqNum:
        // one missing live row shifts every later sequence position, which is what aborted 12 Oct (OCT12-3770).
        // Duplicate identities are consumed in stored order. Never aborts on a mismatch: unmatched stored OI is counted and dropped
        // (native history is the authority for the window). Where both carry OI and they differ, the native value is kept.
        private bool PlanOiCarry(DateTime day, List<TickRecord> captured, List<TickRecord> stored,
            out List<long> planned, out string reason, out string detail, out int validStored, out int carried, out int unknown)
        {
            planned = new List<long>(); reason = null; detail = ""; validStored = 0; carried = 0; unknown = 0;
            int orphans = 0, conflicts = 0;
            var byKey = new Dictionary<string, Queue<TickRecord>>();
            foreach (TickRecord s in stored)
            {
                if (s.OpenInterest != -1) validStored++;
                if (s.OpenInterest == -1) continue;
                string k = IdentityKey(s);
                Queue<TickRecord> q;
                if (!byKey.TryGetValue(k, out q)) { q = new Queue<TickRecord>(); byKey[k] = q; }
                q.Enqueue(s);
            }
            for (int i = 0; i < captured.Count; i++)
            {
                TickRecord c = captured[i];
                Queue<TickRecord> q;
                if (byKey.TryGetValue(IdentityKey(c), out q) && q.Count > 0)
                {
                    TickRecord p = q.Dequeue();
                    if (c.OpenInterest != -1 && c.OpenInterest != p.OpenInterest) conflicts++;
                    planned.Add(c.OpenInterest != -1 ? c.OpenInterest : p.OpenInterest);
                    carried++;
                }
                else
                {
                    planned.Add(c.OpenInterest);
                    if (c.OpenInterest == -1) unknown++;
                }
            }
            foreach (Queue<TickRecord> q in byKey.Values) orphans += q.Count;
            detail = "orphans=" + orphans + " | conflicts=" + conflicts;
            if (orphans > 0 || conflicts > 0) Log("NATIVE_HISTORY | OI_MATCH_NOTE | day=" + DayTag(day) + " | " + detail);
            return true;
        }

        // REC48: pure plan, no database access. carriedLive[i] = true when native row i matches a stored LIVE row by identity.
        // Duplicate identities are consumed in stored order, the same rule as PlanOiCarry.
        // =========================================================================================
        // DO NOT CHANGE THE LINES BELOW (PhaseMatchToleranceMs and PlanPhaseCarry) WITHOUT OWNER SIGN-OFF.
        // Reverting to an exact-millisecond match, removing the one-to-one rule, or dropping the tolerance
        // makes NinjaTrader history duplicate LIVE trades again on every restart (14 Oct, 2026OCT15 build notes).
        // =========================================================================================
        // ROUTER_v11 (PHASE_MATCH): a native history trade is covered by a stored LIVE row when price (tick grid) and volume are equal
        // and the two EventTimes are within PhaseMatchToleranceMs. Each LIVE row covers at most one history trade, nearest in time first.
        // Replaces the exact-millisecond match: NinjaTrader history returns the same trades 1-2 s later on some reads, so the exact
        // match missed them and every restart stored them a second time (14 Oct: 4,143 history rows added on top of the live rows).
        private const long PhaseMatchToleranceMs = 2000;
        private bool[] PlanPhaseCarry(List<TickRecord> captured, List<TickRecord> stored)
        {
            var liveByPv = new Dictionary<string, List<int>>();   // price|volume -> indexes into liveMs
            var liveMs = new List<long>();                        // LIVE EventTime, ms since epoch
            foreach (TickRecord s0 in stored)
            {
                if (s0.Phase != "LIVE") continue;
                string k = SnapToTick(s0.Price).ToString("R", Inv) + "|" + s0.Volume.ToString(Inv);
                List<int> lst;
                if (!liveByPv.TryGetValue(k, out lst)) { lst = new List<int>(); liveByPv[k] = lst; }
                lst.Add(liveMs.Count);
                liveMs.Add(s0.EventTime.Ticks / 10000);
            }
            var used = new bool[liveMs.Count];
            var carried = new bool[captured.Count];
            for (int i = 0; i < captured.Count; i++)
            {
                TickRecord c = captured[i];
                string k = SnapToTick(c.Price).ToString("R", Inv) + "|" + c.Volume.ToString(Inv);
                List<int> lst;
                if (!liveByPv.TryGetValue(k, out lst)) continue;
                long cMs = c.EventTime.Ticks / 10000;
                int best = -1; long bestDiff = long.MaxValue;
                foreach (int li in lst)
                {
                    if (used[li]) continue;
                    long d = Math.Abs(liveMs[li] - cMs);
                    if (d <= PhaseMatchToleranceMs && d < bestDiff) { best = li; bestDiff = d; }
                }
                if (best >= 0) { used[best] = true; carried[i] = true; }
            }
            return carried;
        }
        // END OF PROTECTED BLOCK (PHASE_MATCH). See banner above. Owner sign-off needed to edit.

        // ROUTER_v11 (DIAG_PAIRING): read-only. At each attach writes the native history list (with the covered flag from the
        // pairing) and the LIVE rows of the same window, so unpaired LIVE volume can be found offline. Reads covered[] only.
        // Does not change the pairing, the store, any counter or any log line other than DIAG_PAIRING.
        private void DumpPairing(DateTime day, List<TickRecord> hist, List<TickRecord> live, bool[] covered)
        {
            try
            {
                var nb = new System.Text.StringBuilder();
                nb.AppendLine("EventTime,Price,Volume,OpenInterest,Covered");
                int coveredN = 0;
                for (int i = 0; i < hist.Count; i++)
                {
                    TickRecord h = hist[i];
                    bool cv = i < covered.Length && covered[i];
                    if (cv) coveredN++;
                    nb.AppendLine(h.EventTime.ToString(TsFmt, Inv) + "," + h.Price.ToString("R", Inv) + "," + h.Volume.ToString(Inv) + ","
                                  + h.OpenInterest.ToString(Inv) + "," + (cv ? "1" : "0"));
                }
                string nPath = Path.Combine(RouterConfig.LogDir, "ROUTER_DIAG_NATIVE_" + DayTag(day) + ".csv");
                File.WriteAllText(nPath, nb.ToString(), new System.Text.UTF8Encoding(false));

                var lb = new System.Text.StringBuilder();
                lb.AppendLine("EventTime,Price,Volume,OpenInterest,DayVolume");
                foreach (TickRecord l in live)
                    lb.AppendLine(l.EventTime.ToString(TsFmt, Inv) + "," + l.Price.ToString("R", Inv) + "," + l.Volume.ToString(Inv) + ","
                                  + l.OpenInterest.ToString(Inv) + "," + l.DayVolume.ToString(Inv));
                string lPath = Path.Combine(RouterConfig.LogDir, "ROUTER_DIAG_LIVE_" + DayTag(day) + ".csv");
                File.WriteAllText(lPath, lb.ToString(), new System.Text.UTF8Encoding(false));

                Log("DIAG_PAIRING | day=" + DayTag(day) + " | native=" + hist.Count + " | covered=" + coveredN
                    + " | live=" + live.Count + " | files=" + nPath + " ; " + lPath);
            }
            catch (Exception ex) { Log("DIAG_PAIRING_FAIL | " + Ex(ex)); }
        }

        private string IdentityKey(TickRecord r)
        {
            return new DateTime(r.EventTime.Ticks / 10000 * 10000).Ticks.ToString(Inv) + "|" + SnapToTick(r.Price).ToString("R", Inv) + "|" + r.Volume.ToString(Inv);
        }

        private void RebuildKeysFromStore(DateTime day)
        {
            List<TickRecord> rows = StoreTicksOrdered(day);
            lock (keyLock)
            {
                keyNin.Clear(); keyHist.Clear(); keyHistPlain.Clear();
                AddStoredKeys(rows, keyNin, keyHist);
                foreach (TickRecord r in rows) if (!IsLiveTick(r)) keyHistPlain.Add(KeyOf(r));
                ReconcileKeyQuarantineLocked("REBUILD_FROM_STORE");   // ROUTER_v30
            }
        }

        // REC19: permanent holiday/weekend guard. On a non-trading date the session day stays on the last trading day (SessionDay),
        // so every write stage would otherwise rewrite that old day. Writes are blocked on non-trading dates unless the
        // test-only switch is set in the config (litedb.allow.nontrading.writes=true). Keep that switch off on the live database.
        // REC28: one SEED_RESULT line per attach. Logs stored values only; nothing is inferred from SNAP lines.
        private void LogSeedResult(DateTime day)
        {
            string prevTxt = double.IsNaN(firstTradePrevCloseForNewDay) ? "unavailable" : firstTradePrevCloseForNewDay.ToString("R", Inv);
            TickRecord first = null;
            lock (dbGate)
            {
                if (tickCol != null)
                    first = tickCol.Find(x => x.Day == day && !x.Superseded)
                                   .OrderBy(x => x.EventTime).ThenBy(x => x.SeqN).ThenBy(x => x.Id).FirstOrDefault();
            }
            string firstTxt = "unavailable", storedFirstRowCandidateTxt = "unavailable";
            if (first != null)
            {
                firstTxt = first.EventTime.ToString(TsFmt, Inv) + "|SeqN=" + first.SeqN + "|p=" + first.Price.ToString("R", Inv)
                         + "|Dir=" + (first.Direction ?? "null");
                if (!double.IsNaN(firstTradePrevCloseForNewDay))
                {
                    bool tie;
                    string c = FirstSideFromPrice(first.Price, firstTradePrevCloseForNewDay, out tie);
                    storedFirstRowCandidateTxt = c ?? "tie";
                }
            }
            Log("SEED_RESULT | day=" + DayTag(day) + " | prevClose=" + prevTxt + " | firstRow=" + firstTxt
                + " | storedFirstRowCandidate=" + storedFirstRowCandidateTxt + " | lastLiveSide=" + (lastLiveSide ?? "unavailable"));
        }

        // ===== REC27: NEUTRAL = 0 =====
        private bool PricesEqualOnGrid(double a, double b, out double snapA, out double snapB)
        {
            double tick = (Instrument != null && Instrument.MasterInstrument != null) ? Instrument.MasterInstrument.TickSize : 0.0;
            if (!(tick > 0)) tick = 0.05;
            snapA = SnapToTick(a);
            snapB = SnapToTick(b);
            return Math.Abs(snapA - snapB) <= tick / 2.0;
        }

        private string FirstSideFromPrice(double price, double prevClose, out bool isTie)
        {
            double sa, sb;
            isTie = PricesEqualOnGrid(price, prevClose, out sa, out sb);
            if (isTie) return null;
            return sa > sb ? "BUY" : "SELL";
        }

        private string LastLiveSideFromStore()
        {
            DateTime today = currentSessionDay.Date;   // local copy: LiteDB expressions use locals, not .Date on the document field
            lock (dbGate)
            {
                if (tickCol == null) return null;
                TickRecord r = tickCol.Find(x => x.Day == today && !x.Superseded && x.Phase == "LIVE"
                                                 && (x.Direction == "BUY" || x.Direction == "SELL"))
                                      .OrderByDescending(x => x.EventTime).ThenByDescending(x => x.SeqN).ThenByDescending(x => x.Id)
                                      .FirstOrDefault();
                if (r != null) return r.Direction;
                r = tickCol.Find(x => x.Day == today && !x.Superseded && x.Phase == "HIST"
                                    && (x.Direction == "BUY" || x.Direction == "SELL"))
                           .OrderByDescending(x => x.EventTime).ThenByDescending(x => x.SeqN).ThenByDescending(x => x.Id)
                           .FirstOrDefault();
                if (r != null) return r.Direction;
                return null;   // HIST51: legacy REC direction fallback retired
            }
        }

        // ROUTER_v10 (F1): previous day = latest stored day before 'day'. Does not depend on the holiday list (D17).
        private DateTime PrevStoredDay(DateTime day)
        {
            DateTime d = day.Date;
            DateTime best = DateTime.MinValue;
            lock (dbGate)
            {
                if (tickCol == null) return DateTime.MinValue;
                foreach (DateTime x in tickCol.Query().Where(r => r.Day < d).Select(r => r.Day).ToEnumerable())
                    if (x.Date > best) best = x.Date;
            }
            return best;
        }

        // ROUTER_v10 (F1): last active price of the latest stored day before 'day', or NaN.
        private double PrevStoredClose(DateTime day, out DateTime fromDay)
        {
            fromDay = PrevStoredDay(day);
            if (fromDay == DateTime.MinValue) return double.NaN;
            DateTime f = fromDay;
            lock (dbGate)
            {
                if (tickCol == null) return double.NaN;
                // v15 (PREV_CLOSE_DATE): match the calendar day, not x.Day == midnight. Stored Day values carry a time part,
                // so the exact comparison found no rows for the prior day (FIRST_TRADE_NO_PREV rowsExact=0 on 14 and 15 Oct).
                TickRecord pc = tickCol.Find(x => x.Day >= f && x.Day < f.AddDays(1) && !x.Superseded)
                                       .OrderByDescending(x => x.EventTime).ThenByDescending(x => x.SeqN).ThenByDescending(x => x.Id)
                                       .FirstOrDefault();
                return pc != null ? pc.Price : double.NaN;
            }
        }

        // Void. Sets the companion prevClose for the day's first live tick and clears lastLiveSide.
        private void ComputeFirstSideForNewDay(DateTime day)
        {
            DateTime prevFrom;
            double prevClose = PrevStoredClose(day, out prevFrom);
            firstTradePrevCloseForNewDay = double.IsNaN(prevClose) ? double.NaN : SnapToTick(prevClose);
            lastLiveSide = null;
        }

        // All-days walk (no time window since v40), read-only since v35/v39: stored directions are preserved. Logs FIRST_TRADE_NO_PREV and CLASS_INVARIANT diagnostics per day.
        private void ReclassifyStoreNeutral()
        {
            DateTime today = currentSessionDay.Date;

            var days = new List<DateTime>();
            lock (dbGate)
            {
                if (tickCol == null) return;
                days = tickCol.Query().Select(x => x.Day).ToEnumerable()
                              .Select(d => d.Date).Where(d => d <= today)
                              .Distinct().OrderBy(d => d).ToList();
            }

            int daysWalked = 0, totalUpdated = 0, noPrevDays = 0;
            long totalNeutralLeft = 0, totalUnknownLeft = 0;

            foreach (DateTime day in days)
            {
                DateTime prevDay;
                double prevClose = PrevStoredClose(day, out prevDay);
                if (!double.IsNaN(prevClose)) prevClose = SnapToTick(prevClose);
                bool havePrev = !double.IsNaN(prevClose);
                if (!havePrev)
                {
                    noPrevDays++;
                    Log("FIRST_TRADE_NO_PREV | day=" + DayTag(day) + " | latestStoredBefore="
                        + (prevDay == DateTime.MinValue ? "none" : DayTag(prevDay)));
                    if (prevDay != DateTime.MinValue)
                    {
                        // ROUTER_v09 (D1, diagnostic only): exact Day match vs same-date match for the last day tried.
                        int rowsExact = 0, rowsSameDate = 0;
                        lock (dbGate)
                        {
                            if (tickCol != null)
                            {
                                DateTime lk = prevDay.Date;
                                rowsExact = tickCol.Find(x => x.Day == prevDay).Count();
                                rowsSameDate = tickCol.Find(x => x.Day >= lk && x.Day < lk.AddDays(1)).Count();
                            }
                        }
                        Log("FIRST_TRADE_NO_PREV_DIAG | day=" + DayTag(day) + " | lastLookup=" + DayTag(prevDay)
                            + " | rowsExact=" + rowsExact + " | rowsSameDate=" + rowsSameDate);
                    }
                }


                List<TickRecord> rows;
                lock (dbGate)
                {
                    if (tickCol == null) return;
                    rows = tickCol.Find(x => x.Day == day && !x.Superseded)
                                  .OrderBy(x => x.EventTime).ThenBy(x => x.SeqN).ThenBy(x => x.Id).ToList();
                }
                if (rows.Count == 0) continue;

                int updated = 0;   // always 0: the walk no longer writes (v35)

                int neutLeft, unkLeft;
                lock (dbGate)
                {
                    neutLeft = tickCol == null ? 0 : tickCol.Find(x => x.Day == day && !x.Superseded && x.Direction == "NEUTRAL").Count();
                    unkLeft = tickCol == null ? 0 : tickCol.Find(x => x.Day == day && !x.Superseded && (x.Direction == null || x.Direction == "UNKNOWN")).Count();
                }
                Log("CLASS_INVARIANT | day=" + DayTag(day) + " | neutralLeft=" + neutLeft + " | unknownLeft=" + unkLeft);
                totalNeutralLeft += neutLeft; totalUnknownLeft += unkLeft;
                totalUpdated += updated; daysWalked++;
            }
            Log("CLASS_INVARIANT_ALL | days=" + daysWalked + " | neutralLeft=" + totalNeutralLeft + " | unknownLeft=" + totalUnknownLeft
                + " | noPrevDays=" + noPrevDays + " | updated=" + totalUpdated);
        }

        // REC25: one-off SeqNum renumber. Active rows get 1..N, superseded rows get N+1..M, each in the order
        // EventTime, then current SeqN, then Id. Changes only SeqN and SeqNum. Rewrites the day's checkpoint through CpRewrite.
        private void RenumberStoreSeq(string gate)
        {
            var days = new List<DateTime>();
            lock (dbGate)
            {
                if (tickCol == null) return;
                if (gate == "ALL") days = tickCol.Query().Select(x => x.Day).ToEnumerable()
                                                  .Select(d => d.Date).Distinct().OrderBy(d => d).ToList();
                else
                {
                    DateTime one;
                    if (DateTime.TryParseExact(gate, "yyyy-MM-dd", Inv, DateTimeStyles.None, out one)) days.Add(one.Date);
                }
            }

            long totalRows = 0; int dayCount = 0;
            foreach (DateTime day in days)
            {
                long activeN = 0, supN = 0, maxSeq = 0;
                bool done = false;
                lock (dbGate)
                {
                    if (tickCol == null) return;
                    List<TickRecord> all = tickCol.Find(x => x.Day == day).ToList();
                    if (all.Count == 0)
                    {
                        Log("RENUMBER_MISSING_DAY | day=" + day.ToString("yyyyMMdd", Inv));
                        continue;
                    }
                    var act = all.Where(x => !x.Superseded).OrderBy(x => x.EventTime).ThenBy(x => x.SeqN).ThenBy(x => x.Id).ToList();
                    var sup = all.Where(x => x.Superseded).OrderBy(x => x.EventTime).ThenBy(x => x.SeqN).ThenBy(x => x.Id).ToList();
                    db.BeginTrans();
                    try
                    {
                        long n = 0;
                        foreach (TickRecord r in act) { n++; r.SeqN = n; r.SeqNum = SeqText(day, n); tickCol.Update(r); }
                        foreach (TickRecord r in sup) { n++; r.SeqN = n; r.SeqNum = SeqText(day, n); tickCol.Update(r); }
                        db.Commit();
                        activeN = act.Count; supN = sup.Count; maxSeq = n; done = true;
                    }
                    catch (Exception ex)
                    {
                        db.Rollback();
                        Log("RENUMBER_ROLLED_BACK | day=" + day.ToString("yyyyMMdd", Inv) + " | " + Ex(ex));
                        continue;
                    }
                }
                if (!done) continue;

                // Deliberate deviation: set exactly, not raised. RaiseSeqTo cannot lower the counter.
                if (day == currentSessionDay.Date) lock (seqLock) { seqCounter = maxSeq; }
                CpRewrite(day, StoreTicksAll(day), "RENUMBER");   // REC26: all rows, so S| = M matches the store
                Log("RENUMBER | day=" + day.ToString("yyyyMMdd", Inv) + " | active=" + activeN + " | superseded=" + supN + " | maxSeq=" + maxSeq);
                totalRows += activeN + supN; dayCount++;
            }
            if (gate == "ALL") Log("RENUMBERED_ALL | days=" + dayCount + " | totalRows=" + totalRows);
        }

        private bool WritesAllowed(string stage)
        {
            if (IsTradingDate(DateTime.Now.Date)) return true;
            string flag = Cfg.Read("litedb.allow.nontrading.writes", "false");
            if (string.Equals(flag, "true", StringComparison.OrdinalIgnoreCase))
            {
                Log("NON_TRADING_WRITES_ALLOWED | TEST_ONLY | stage=" + stage + " | today=" + DateTime.Now.ToString("yyyy-MM-dd", Inv));
                return true;
            }
            Log("WRITE_BLOCKED | stage=" + stage + " | reason=non_trading_date | today=" + DateTime.Now.ToString("yyyy-MM-dd", Inv)
                + " | sessionDay=" + DayTag(currentSessionDay));
            return false;
        }

        // ROUTER_v10 (F3): every early return on owner loss is named in the log.
        private bool AbortIfOwnerGone(string where)
        {
            if (!OwnerGone()) return false;
            Log("STARTUP_ABORTED | stage=" + where + " | reason=owner_gone | terminated=" + terminated);
            return true;
        }

        private void RunStartup()
        {
            Interlocked.Exchange(ref liveDroppedBase, Interlocked.Read(ref liveDroppedCount));   // ROUTER_v02 (fix 1): attach starts a session baseline
            Interlocked.Exchange(ref liveFailedBase, Interlocked.Read(ref liveFailedSeq));       // ROUTER_v02 (fix 2)
            var swTotal = Stopwatch.StartNew();
            lock (maintLock)
            {
                try
                {
                    if (AbortIfOwnerGone("START")) return;
                    Stage("START", () =>
                    {
                        Log("START | instanceId=" + instanceId);
                        string busSlot = VECTOR.Init();
                        Log("VECTOR_BUS | slot=" + busSlot + " | published=" + VECTOR.PublishedCount);
                        if (VECTOR.LayoutMismatch)
                        {
                            Log("STARTUP_ERROR | LAYOUT_MISMATCH | " + VECTOR.MismatchDetail
                                + " | this instance will not publish; PLOTTER cannot receive its rows. Align the bus owners and restart.");
                            throw new InvalidOperationException("LAYOUT_MISMATCH: shared VECTOR bus slot has an incompatible layout");
                        }
                    }, true);
                    Stage("LITEDB_OPEN", () => { if (!OwnerGone()) OpenDb(); }, true);
                    Stage("SESSION_DAY_RESTORE", () => { if (!OwnerGone()) RestoreSessionDay(); }, true);
                    bool writesOn = WritesAllowed("STARTUP_WRITES");
                    // ROUTER_v24: read-only recount of MISSED and LIVE on every attach, before the write guard. RecomputeClassTotals only reads
                    // the store and assigns the two totals (no add, no write), so it is safe on non-trading dates and idempotent.
                    Stage("RECOUNT_LOTS", () => { if (!OwnerGone()) RecomputeClassTotals(currentSessionDay.Date); }, false);
                    Stage("NATIVE_HISTORY",     () => { if (!OwnerGone() && writesOn) RewriteTodayFromNativeSeries(); }, false);
                    Stage("BACKFILL_DIRECTION", () => { if (!OwnerGone() && writesOn) BackfillDirectionToday(); },       false);
                    Stage("RECLASSIFY_STORE", () => { if (!OwnerGone() && writesOn) ReclassifyStoreNeutral(); }, false);   // REC27
                    Stage("CLASS_REBUILD_COUNTERS", () => { if (!OwnerGone() && writesOn) RebuildCounters(currentSessionDay.Date); }, false);   // REC27
                    Stage("SEED_LIVE_SIDE", () =>
                    {
                        if (OwnerGone()) return;
                        ComputeFirstSideForNewDay(currentSessionDay.Date);
                        lastLiveSide = LastLiveSideFromStore();
    LogSeedResult(currentSessionDay.Date);   // REC28
                        Log("BUMPSTORE_REC_EXCLUDED | counters=BUY,SELL,NEUTRAL,UNKNOWN exclude Phase=REC");
                    }, false);   // REC27: seed after the walk and the rebuild, before the live gate opens
                    // REC25: renumber pass. Placed after RECLASSIFY_NEUTRAL and before MODE_DECISION (see deviation note).
                    string renumGate = Cfg.Read("litedb.renumber.day", "");
                    if (!string.IsNullOrWhiteSpace(renumGate))
                    {
                        Stage("RENUMBER_SEQ", () =>
                        {
                            if (OwnerGone()) return;
                            if (renumGate == "ALL")
                            {
                                if (WritesAllowed("RENUMBER_ALL")) RenumberStoreSeq("ALL");
                                return;
                            }
                            DateTime specific;
                            if (DateTime.TryParseExact(renumGate, "yyyy-MM-dd", Inv, DateTimeStyles.None, out specific))
                            {
                                RenumberStoreSeq(specific.ToString("yyyy-MM-dd", Inv));
                                return;
                            }
                            Log("RENUMBER_SKIP | value=" + renumGate);
                        }, false);
                    }
                    Stage("MODE_DECISION",      () => { if (!OwnerGone()) WaitModeDecision(); }, false);
                    Stage("PAST_EXPORT",        () => { if (!OwnerGone() && writesOn) ExportPast(); },                    false);
                    Stage("COUNTERS_REBUILD",   () => { if (!OwnerGone()) FinalizeDayState(); }, true);
                    Stage("REPLAY",             () => { if (!OwnerGone()) BusRebuild(true, false); }, true);
                    Stage("LOG_EXPORT",         () => { if (!OwnerGone() && writesOn) ExportTodayLogs(false); },          false);
                    if (AbortIfOwnerGone(lastStageName)) return;

                    // drain the startup / detection buffer in arrival order through the single insert path, THEN go live.
                    DrainAndGoLive();
                    VECTOR.SetReplayDone(true);   // c[3]: written by ROUTER at startup completion
                    startupOk = true;
                    swTotal.Stop();
                    // REC36: diagnostic only. Same counters the panel reads; lots = raw / LotSize (LotsOf). No state changes.
                    long dtToday; lock (dashLock) { dtToday = storeVolNin; }
                    long dtWin, dtVecTotal, dtVecToday;
                    lock (totalsLock) { dtWin = litedbWindowRaw; dtVecTotal = vecTotalRaw; dtVecToday = vecTodayRaw; }
                    Log("DASH_TOTALS | litedbToday=" + LotsOf(dtToday) + " | litedbWindow=" + LotsOf(dtWin)
                        + " | vectorToday=" + LotsOf(dtVecToday) + " | vectorWindow=" + LotsOf(dtVecTotal));
                    Log("STARTUP_COMPLETE | elapsedMs=" + swTotal.ElapsedMilliseconds + " | busPublished=" + VECTOR.PublishedCount
                        + " | mode=" + ModeName(modeState)
                        + " | nonFatalFailures=" + startupNonFatalFailures);
                    // REC30: holiday confirmation, once per attach. Source: NinjaTrader Nse trading-hours template.
                    try
                    {
                        int yearCount = 0;
                        foreach (KeyValuePair<DateTime, string> hol in Bars.TradingHours.Holidays.OrderBy(h => h.Key))
                        {
                            if (hol.Key.Year != DateTime.Now.Year) continue;
                            yearCount++;
                            Log("HOLIDAY_DATE | " + hol.Key.ToString("yyyy-MM-dd ddd", Inv) + " | " + hol.Value);
                        }
                        Log("HOLIDAY_LIST_DUMP | template=" + Bars.TradingHours.Name + " | year=" + DateTime.Now.Year
                            + " | countThisYear=" + yearCount + " | totalInTemplate=" + Bars.TradingHours.Holidays.Count);
                    }
                    catch (Exception hdx) { Log("HOLIDAY_LIST_DUMP_FAIL | " + Ex(hdx)); }
                }
                catch (Exception ex)
                {
                    swTotal.Stop();
                    Log("STARTUP_FAILED | elapsedMs=" + swTotal.ElapsedMilliseconds + " | stage=" + lastStageName
                        + " | retry=" + startupRetryCount + "/" + StartupRetryMax
                        + " | " + Ex(ex));
                    Debug("STARTUP_FAILED | " + ex.ToString());

                    if (VECTOR.LayoutMismatch)
                    {
                        // v34: not retryable. The other bus owner must be aligned, then this instance restarted.
                        TriggerAutoStop("VECTOR_LAYOUT_MISMATCH", lastStageName, ex);
                    }
                    else if (startupRetryCount < StartupRetryMax)
                    {
                        int backoff = StartupRetryBackoffSec[Math.Min(startupRetryCount, StartupRetryBackoffSec.Length - 1)];
                        startupRetryCount++;
                        startupRetryNextUtc = DateTime.UtcNow.AddSeconds(backoff);
                        Log("STARTUP_RETRY_SCHEDULED | attempt=" + startupRetryCount + " | inSec=" + backoff);
                    }
                    else
                    {
                        TriggerAutoStop("STARTUP_RETRY_EXHAUSTED", lastStageName, ex);
                    }
                    // replayDone stays 0: the read side keeps waiting for the next successful attach.
                }
            }
}

        private void TriggerAutoStop(string reason, string stage, Exception ex)
        {
            if (autoStopped) return;
            autoStopped = true;
            autoStopReason = reason;
            Log("AUTO_STOP | reason=" + reason + " | stage=" + stage
                + " | ex=" + (ex == null ? "none" : ex.GetType().Name + ": " + ex.Message));

            try
            {
                string ts = DateTime.Now.ToString("yyyyMMMdd_HHmmss", Inv).ToUpperInvariant();
                string bundlePath = Path.Combine(RouterConfig.LogDir, "ROUTER_AUTOSTOP_" + ts + ".log");
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("ROUTER AUTOSTOP DEBUG BUNDLE");
                sb.AppendLine("reason       : " + reason);
                sb.AppendLine("stage        : " + stage);
                sb.AppendLine("lastStage    : " + lastStageName);
                sb.AppendLine("instanceId   : " + instanceId);
                sb.AppendLine("time         : " + DateTime.Now.ToString("yyyyMMMdd HH:mm:ss.fff", Inv).ToUpperInvariant());
                sb.AppendLine("sessionDay   : " + currentSessionDay.ToString("yyyy-MM-dd", Inv));
                sb.AppendLine();
                sb.AppendLine("--- exception ---");
                sb.AppendLine(ex == null ? "(none)" : ex.ToString());
                sb.AppendLine();
                sb.AppendLine("--- counters ---");
                sb.AppendLine("liveReceived     : " + Interlocked.Read(ref liveReceived));
                sb.AppendLine("liveWritten      : " + Interlocked.Read(ref liveInserted));
                sb.AppendLine("livePublished    : " + Interlocked.Read(ref livePublished));
                sb.AppendLine("liveDroppedCount : " + Interlocked.Read(ref liveDroppedCount));
                sb.AppendLine("startupOk        : " + startupOk);
                sb.AppendLine("ownerReady       : " + ownerReady);
                sb.AppendLine("terminated       : " + terminated);
                sb.AppendLine();
                sb.AppendLine("--- main log tail (last 200 lines) ---");
                try
                {
                    if (logPath != null && File.Exists(logPath))
                    {
                        string[] all = File.ReadAllLines(logPath);
                        int start = Math.Max(0, all.Length - 200);
                        for (int i = start; i < all.Length; i++) sb.AppendLine(all[i]);
                    }
                    else sb.AppendLine("(main log not found)");
                }
                catch (Exception lex) { sb.AppendLine("(main log read failed: " + lex.Message + ")"); }

                Directory.CreateDirectory(RouterConfig.LogDir);
                File.WriteAllText(bundlePath, sb.ToString());
                Log("AUTO_STOP_BUNDLE | path=" + bundlePath);
            }
            catch (Exception bex)
            {
                try { Debug("AUTOSTOP_BUNDLE_FAIL | " + bex.GetType().Name + " | " + bex.Message); } catch { }
            }

            // Item E ledger: close the session so the START/STOP pair stays balanced.
            try { WriteSessionStop(); } catch (Exception sex) { NoteLogFailure("SESSION_STOP", sex); }
            try { ClearLifecycleMarker(); } catch (Exception lmx) { NoteLogFailure("LIFECYCLE_MARKER", lmx); }
        }

        private void RunStartupRetryIfNeeded()
        {
            if (autoStopped || terminated || !ownerReady) return;
            if (startupOk) return;
            if (startupRetryNextUtc == DateTime.MinValue || DateTime.UtcNow < startupRetryNextUtc) return;
            startupRetryNextUtc = DateTime.MinValue;
            Log("STARTUP_RETRY_BEGIN | attempt=" + startupRetryCount);
            Task.Run(() => RunStartup());
        }

        // R12 held day, R2 SeqNum recovery, R4/R10 key sets, R14 counters from LiteDB.
        private void RestoreSessionDay()
        {
            DateTime held = ResolveHeldDay();
            if (held.Date != currentSessionDay.Date)
            {
                Log("SESSION_DAY_HELD | from=" + currentSessionDay.ToString("yyyy-MM-dd", Inv) + " | to=" + held.ToString("yyyy-MM-dd", Inv));
                currentSessionDay = held.Date;
                RotateLogs(currentSessionDay);   // STD28 (audit): repoint MAIN/DEBUG to the held day
            }
            ReloadDayState("ATTACH");
        }

        // Rebuild SeqNum counter, key sets and checkpoint path for currentSessionDay from LiteDB + checkpoint.
        private void ReloadDayState(string why)
        {
            DateTime day = currentSessionDay.Date;
            CpOpenDay(day);
            CpState cp = CpLoad(day);
            List<TickRecord> rows = StoreTicksOrdered(day);

            // legacy rows (no SeqNum) of this day get one, in time order, so the store and checkpoint agree
            long maxStore = 0;
            foreach (TickRecord r in StoreTicksAll(day)) if (r.SeqN > maxStore) maxStore = r.SeqN;   // STD39: superseded rows keep their SeqNum
            ResetSeqForDay();
            RaiseSeqTo(Math.Max(cp.MaxSeq, maxStore));
            int legacy = 0;
            lock (dbGate)
            {
                foreach (TickRecord r in rows)
                {
                    if (r.SeqN > 0) continue;
                    long n = AllocSeqN();
                    r.SeqN = n; r.SeqNum = SeqText(day, n);
                    tickCol.Update(r);
                    legacy++;
                }
            }
            // ROUTER_v31 attach fix: snapshot and key rebuild under ONE dbGate hold (order dbGate -> keyLock, same as InsertLive), so a live insert
            // cannot land between the store read and the quarantine reconcile (a startup retry can run this with liveReady already true).
            lock (dbGate)
            {
                List<TickRecord> keyRows = StoreTicksOrdered(day);   // dbGate is re-entrant
                lock (keyLock)
                {
                    keyNin.Clear(); keyHist.Clear(); keyHistPlain.Clear();
                    AddStoredKeys(keyRows, keyNin, keyHist);   // HIST keys carry the occurrence number
                    foreach (TickRecord r in keyRows) if (!IsLiveTick(r)) keyHistPlain.Add(KeyOf(r));   // REC11: plain HIST keys for match phase
                    ReconcileKeyQuarantineLocked("RELOAD_DAY_STATE");   // ROUTER_v30
                }
            }
            TailMarkDirty();   // STD06 (item 1): legacy SeqNum updates / a reload mean today's file must be rewritten once
            RebuildCounters(day);   // STD07-AUDIT (item 30): empty lock (dashLock) { } removed
            long cur = CurrentSeq();
            seqContinuityFail = cp.Exists && cp.MaxSeq > maxStore;   // checkpoint is AHEAD of the store: stored rows are missing. (cp behind store = normal, un-flushed K lines.)
            Log((cp.Exists ? "CHECKPOINT_LOADED" : "CHECKPOINT_ABSENT") + " | why=" + why + " | day=" + DayTag(day) + " | cpMaxSeq=" + cp.MaxSeq
                + " | storeMaxSeq=" + maxStore + " | rows=" + rows.Count);
            Log("SEQNUM_RECOVERED | day=" + DayTag(day) + " | highest=" + (cur > 0 ? SeqText(day, cur) : "none")
                + " | next=" + SeqText(day, cur + 1) + " | legacyAssigned=" + legacy);
            if (cp.Exists && maxStore > 0 && cp.MaxSeq != maxStore)
                Log("SEQNUM_CONTINUITY_NOTE | cpMaxSeq=" + cp.MaxSeq + " | storeMaxSeq=" + maxStore + " | counter=" + cur + " (highest of both used)");
        }

        // STD01: ONE definition of the dashboard counter reset. Caller holds dashLock.
        private void ResetDashLocked()
        {
            storeRows = 0; storeVolNin = 0;
            storeVolBuy = 0; storeVolSell = 0; storeVolNeutral = 0; storeVolUnknown = 0;   // STD62
            storeMaxDayVol = -1;
            lastTickT = DateTime.MinValue;   // STD45
        }

        // R14: DAY_VOL source, STORE_VOL, direction volumes and OI rebuilt from LiteDB for the whole day.
        // ROUTER_v16 (OI carry): NinjaTrader sends OI only on change, so a fresh attach starts with cachedOi = -1 and every
        // live tick is stored with OI N/A until the first OI event. Seed cachedOi from the last valid OI already in the store:
        // today's rows first, else the previous trading day. The first live valid OI replaces the seed (OI_LIVE_RESUMED).
        private void SeedOiFromStore(DateTime day)
        {
            try
            {
                long oi = -1; DateTime at = DateTime.MinValue; string src = null;
                DateTime d = day.Date;
                lock (dbGate)
                {
                    if (tickCol == null) return;
                    for (int i = 0; i < 2 && oi < 0; i++)
                    {
                        TickRecord last = tickCol.Find(x => x.Day == d && x.OpenInterest != -1 && !x.Superseded)
                                                 .OrderByDescending(x => x.EventTime).ThenByDescending(x => x.Id).FirstOrDefault();
                        if (last != null) { oi = last.OpenInterest; at = last.EventTime; src = (i == 0) ? "SAME_DAY_STORE" : "PREV_SESSION_STORE"; }
                        else d = PrevTradingDate(d).Date;
                    }
                }
                if (oi >= 0)
                {
                    Interlocked.Exchange(ref cachedOi, oi);
                    oiSeeded = true;
                    Log("OI_SEED | source=" + src + " | oi=" + oi + " | asOf=" + at.ToString(TsFmt, Inv));
                }
                else Log("OI_SEED | source=NONE | oi=-1");
            }
            catch (Exception ex) { Debug("OI_SEED_FAIL | " + Ex(ex)); }
        }

        private void RebuildCounters(DateTime day)
        {
            List<TickRecord> rows = StoreTicksOrdered(day);
            lock (dashLock)
            {
                ResetDashLocked();
                foreach (TickRecord r in rows) BumpStoreLocked(r);
            }
        }

        // REC05 UNKNOWN fix: classify stored active rows of today whose Direction is null (HIST rows stored unclassified by an
        // earlier build, or any row that reached the store without a side). Uses the stored quotes when present (DirOrClassify);
        // HIST rows have no quotes and fall back to price-vs-previous. Counters rebuilt only when something changed.
        // (v39: the REC08 NEUTRAL-reclassify pass was removed; it only rebuilt counters.)
        private void BackfillDirectionToday()
        {
            DateTime day = currentSessionDay.Date;
            int rowN = 0, fixedN = 0, alreadyN = 0;
            lock (dbGate)
            {
                if (tickCol == null) return;
                List<TickRecord> rows = tickCol.Find(x => x.Day == day).Where(x => !x.Superseded)
                                               .OrderBy(x => x.EventTime).ThenBy(x => x.Id).ToList();
                rowN = rows.Count;
                if (rowN == 0) return;
                int nullDir = rows.Count(x => x.Direction == null);   // DIAG (REC06_DIAG)
                int unknownDir = rows.Count(x => x.Direction == "UNKNOWN");
                int histRows = rows.Count(x => x.Phase == "HIST");
                int liveRows = rows.Count(x => x.Phase == "LIVE");
                Log("DIR_BACKFILL_PRE | day=" + DayTag(day) + " | total=" + rows.Count
                    + " | nullDir=" + nullDir + " | unknownDir=" + unknownDir
                    + " | hist=" + histRows + " | live=" + liveRows);
                double prev = double.NaN;
                var brBackfill = new Dictionary<string, int>();   // DIAG
                foreach (TickRecord r in rows)
                {
                    if (r.Direction == "BUY" || r.Direction == "SELL" || r.Direction == "NEUTRAL") alreadyN++;
                    else { r.Direction = DirOrClassify(r, prev); tickCol.Update(r); fixedN++; DiagCount(brBackfill, classifyBranch); }
                    prev = r.Price;
                }
                int nBuy = rows.Count(x => x.Direction == "BUY");   // DIAG
                int nSell = rows.Count(x => x.Direction == "SELL");
                int nNeut = rows.Count(x => x.Direction == "NEUTRAL");
                Log("DIR_BACKFILL_POST | buy=" + nBuy + " | sell=" + nSell + " | neutral=" + nNeut + " | branches=" + DiagTally(brBackfill));
            }
            if (fixedN > 0) RebuildCounters(day);
            Log("DIRECTION_BACKFILL | day=" + DayTag(day) + " | rows=" + rowN + " | updated=" + fixedN + " | alreadySet=" + alreadyN);
        }

        private void BumpStore(TickRecord r) { lock (dashLock) { BumpStoreLocked(r); } }

        private void BumpStoreLocked(TickRecord r)
        {
            storeRows++;
            if (IsNativeTape(r)) storeVolNin += r.Volume;
            // STD62: tally the already-stored Direction into exactly one bucket (null / empty / unrecognised = UNKNOWN). Classifies nothing.
            string dir = r.Direction;
            if (r.Phase == "LIVE" || r.Phase == "HIST")   // HIST51: LIVE and HIST move the panel's direction counters
            {
                if (dir == "BUY")           storeVolBuy     += r.Volume;
                else if (dir == "SELL")     storeVolSell    += r.Volume;
                else if (dir == "NEUTRAL")  storeVolNeutral += r.Volume;
                else                        storeVolUnknown += r.Volume;
            }
            if (r.DayVolume > storeMaxDayVol) storeMaxDayVol = r.DayVolume;
            // STD45: dashboard-only (last tick time). Reads the row, writes nothing to the store.
            if (r.EventTime > lastTickT) lastTickT = r.EventTime;
        }

        private void ZeroCounters()
        {
            lock (dashLock) { ResetDashLocked(); }
            Interlocked.Exchange(ref dayVolRaw, -1);   // STD07-AUDIT: also written by RefreshDayVol (timer) and read by the render thread
        }

        // end of attach: checkpoint = final stored state (R11 / R9), counters from LiteDB (R14).
        private void FinalizeDayState()
        {
            DateTime day = currentSessionDay.Date;
            RebuildCounters(day);   // REC21: in-memory and read-only, so the dashboard shows the session on any attach
            SeedOiFromStore(day);   // ROUTER_v16: carry the last valid OI into this attach
            // REC34: LITEDB TILLNOW = native (LIVE + HIST) active volume over the retained window. Once per attach.
            DateTime winTo = currentSessionDay.Date;   // v40: no lower bound, every stored day
            long winRaw;
            lock (dbGate)
            {
                winRaw = tickCol == null ? 0 :
                    tickCol.Query().Where(x => x.Day <= winTo && !x.Superseded && (x.Phase == "LIVE" || x.Phase == "HIST"))
                           .Select(x => x.Volume).ToEnumerable().Sum();
            }
            lock (totalsLock) { litedbWindowRaw = winRaw; }
            Log("LITEDB_WINDOW | fromDay=ALL_STORED | toDay=" + DayTag(winTo) + " | raw=" + winRaw + " | lots=" + LotsOf(winRaw));
            if (WritesAllowed("ATTACH_END_CHECKPOINT")) CpRewrite(day, StoreTicksOrdered(day), "ATTACH_END");
            CheckSeqInvariant(day, StoreTicksOrdered(day), "ATTACH_END");   // STD39
            if (seqContinuityFail) { Log("SEQNUM_CONTINUITY_FAILURE | day=" + DayTag(day) + " | checkpoint was ahead of the stored rows at attach"); }
            else { Log("SEQNUM_CONTINUITY_CONFIRMED | day=" + DayTag(day) + " | highest=" + (CurrentSeq() > 0 ? SeqText(day, CurrentSeq()) : "none")); }
        }

        // =====================================================================
        // R26 startup mode (decision only: never touches an EventTime, never becomes a fill interval)
        // =====================================================================
        private void InitModeAtAttach()
        {
            DateTime now = attachEventTime;
            if (!IsTradingDate(now.Date))
            {
                modeState = ModeSkipped;
                Log("MODE_SKIPPED | non-trading day | held=" + currentSessionDay.ToString("yyyy-MM-dd", Inv));
                return;
            }
            if (now.TimeOfDay < MarketOpenTod)
            {
                modeState = ModeWaitOpen;
                Log("MODE_WAIT_OPEN | attach=" + now.ToString(TsFmt, Inv) + " | mode becomes LIVE_NIN at the first Last tick at or after 09:15");
                return;
            }
            modeState = ModeLateLogin;   // REC31: attached at or after the open on a trading date; no promotion
            Log("MODE_SET | mode=LATE_LOGIN | attach=" + now.ToString(TsFmt, Inv));
        }

        private void WaitModeDecision()
        {
            if (modeState == ModeWaitOpen || modeState == ModeSkipped)
            {
                Log("MODE_WAIT_NOT_BLOCKING_STARTUP | state=" + ModeName(modeState));
                return;
            }
            Log("MODE_DECISION_READY | state=" + ModeName(modeState));
        }

        // =====================================================================
        // =====================================================================
        // ONE insert path for an accepted row (R3, R4, R10, R15).
        //   InsertCore : SeqNum (always new) -> LiteDB -> key set -> checkpoint -> counters -> VECTOR.
        // =====================================================================
        // REC14: snap a live Last price to the instrument's tick grid. Reads Instrument.MasterInstrument.TickSize; 0.05 only if that is unavailable.
        // Removes the float32 round-trip artifact (e.g. 22614.599609375 -> 22614.6). Logs the first 10 raw/snapped pairs.
        private int snapLogN = 0;
        // ROUTER_v02 classify (approved Finding 1): quote snap. Same rounding as SnapToTick, but it does not write the
        // SNAP diagnostic, so the trade SNAP log keeps its first ten trade snaps.
        // Rounding must stay identical to SnapToTick. Change both together.
        private double SnapQuoteToTick(double px)
        {
            double tick = (Instrument != null && Instrument.MasterInstrument != null) ? Instrument.MasterInstrument.TickSize : 0.0;
            if (!(tick > 0)) tick = 0.05;
            int dec = (int)Math.Ceiling(-Math.Log10(tick));
            dec = Math.Max(0, Math.Min(15, dec));
            double snapped = Math.Round(Math.Round(px / tick, 0, MidpointRounding.AwayFromZero) * tick, dec, MidpointRounding.AwayFromZero);
            return snapped;
        }

        // Rounding must stay identical to SnapQuoteToTick. Change both together.
        private double SnapToTick(double px)
        {
            double tick = (Instrument != null && Instrument.MasterInstrument != null) ? Instrument.MasterInstrument.TickSize : 0.0;
            if (!(tick > 0)) tick = 0.05;
            int dec = (int)Math.Ceiling(-Math.Log10(tick));
            dec = Math.Max(0, Math.Min(15, dec));
            double snapped = Math.Round(Math.Round(px / tick, 0, MidpointRounding.AwayFromZero) * tick, dec, MidpointRounding.AwayFromZero);
            if (snapLogN < 10)
            {
                snapLogN++;
                Log("SNAP | n=" + snapLogN + " | raw=" + px.ToString("R", Inv) + " | snapped=" + snapped.ToString("R", Inv) + " | tick=" + tick.ToString("R", Inv));
            }
            return snapped;
        }

        // REC10 diagnostic: first DupSampleMax dropped duplicates, incoming tick fields only. Logs only; no dedupe change.
        private readonly List<string> dupSamples = new List<string>();
        private const int DupSampleMax = 100;
        private void DiagDupSample(TickRecord r, string matchPhase)
        {
            if (dupSamples.Count >= DupSampleMax) return;
            dupSamples.Add("n=" + (dupSamples.Count + 1) + "|t=" + r.EventTime.ToString("HH:mm:ss.fff", Inv)
                + "|p=" + r.Price.ToString("R", Inv) + "|v=" + r.Volume + "|o=" + r.OpenInterest + "|matchPhase=" + matchPhase);
            Log("DUP_SAMPLE (per attach, cap 100) | n=" + dupSamples.Count
                + " | t=" + r.EventTime.ToString("HH:mm:ss.fff", Inv)
                + " | p=" + r.Price.ToString("R", Inv)
                + " | v=" + r.Volume
                + " | o=" + r.OpenInterest
                + " | matchPhase=" + matchPhase);
        }

        // v42 diagnostic (logs only). Session-scoped map: key without OI -> first OI seen. Called under keyLock.
        private readonly Dictionary<string, string> liveOiSeen = new Dictionary<string, string>();
        private long liveOiVariantCount = 0;
        private int liveOiVariantLogN = 0;
        private const int LiveOiVariantLogMax = 100;
        private const int LiveOiSeenMax = 200000;
        private static string QuoteText(double? q) { return q.HasValue ? q.Value.ToString("R", Inv) : "N/A"; }
        private void DiagLiveOiVariant(TickRecord r)
        {
            try
            {
                string nk = r.EventTime.Ticks.ToString(Inv) + "|" + r.Price.ToString("R", Inv) + "|" + r.Volume.ToString(Inv)
                          + "|" + QuoteText(r.BidPrice) + "|" + QuoteText(r.AskPrice);
                string oi = r.OpenInterest.ToString(Inv);
                string prevOi;
                if (liveOiSeen.TryGetValue(nk, out prevOi))
                {
                    if (prevOi != oi)
                    {
                        liveOiVariantCount++;
                        if (liveOiVariantLogN < LiveOiVariantLogMax)
                        {
                            liveOiVariantLogN++;
                            Log("LIVE_OI_VARIANT (diagnostic, cap 100) | n=" + liveOiVariantCount
                                + " | t=" + r.EventTime.ToString(TsFmt, Inv) + " | p=" + r.Price.ToString("R", Inv)
                                + " | v=" + r.Volume + " | bid=" + QuoteText(r.BidPrice) + " | ask=" + QuoteText(r.AskPrice)
                                + " | oi_seen=" + prevOi + " | oi_now=" + oi + " | action=unchanged");
                        }
                    }
                }
                else if (liveOiSeen.Count < LiveOiSeenMax) liveOiSeen[nk] = oi;
            }
            catch (Exception dex) { Debug("LIVE_OI_VARIANT_DIAG_FAIL | " + Ex(dex)); }
        }

        private Ins InsertLive(TickRecord r)
        {
            string key = KeyOf(r);
            bool complete = true;
            lock (dbGate)
            {
                if (db == null || tickCol == null) return Ins.Failed;
                // ROUTER_v21: floor re-check under dbGate. The rewrite sets liveFloorTicks inside dbGate after its commit, so this read is
                // ordered against that write. The unlocked check in ProcessLive is only a fast exit and can read a stale floor.
                long floorLocked = Interlocked.Read(ref liveFloorTicks);
                if (floorLocked >= 0 && r.EventTime.Ticks <= floorLocked) return Ins.FloorDrop;
                DayCtx ctx = CurrentCtx();
                bool quarantined = false, quarOverflow = false;
                lock (keyLock)
                {
                    if (ctx.Nin.Contains(key))
                    {
                        // REC11: phase of the row that owns this key. HIST wins when both exist.
                        string matchPhase = keyHistPlain.Contains(key) ? "HIST" : "LIVE";
                        DiagDupSample(r, matchPhase);
                        return Ins.DroppedDup;
                    }
                    // v42 diagnostic: logs only. Does not change the decision below.
                    DiagLiveOiVariant(r);
                    // ROUTER_v30: an identity whose stored row failed its key-set add is held in the quarantine. It is already stored: a duplicate.
                    quarantined = keyQuarCount > 0 && KeyQuarantinedLocked(key);
                    quarOverflow = keyQuarOverflow;
                }   // same content already stored (LIVE or recovered HIST)
                if (quarantined)
                {
                    long qh = Interlocked.Increment(ref keyQuarHits);
                    if (qh <= 50) Log("KEYSET_QUARANTINE_HIT | n=" + qh + " | t=" + r.EventTime.ToString(TsFmt, Inv) + " | p=" + r.Price.ToString("R", Inv) + " | v=" + r.Volume + " | treated as already stored");
                    return Ins.DroppedDup;
                }
                // ROUTER_v30: the quarantine overflowed, so a stored identity may be missing from both sets. Nothing is written until the key sets are rebuilt from the store.
                if (quarOverflow) return Ins.Failed;
                complete = InsertCore(ctx, r, false, null, true);   // InsertLive is the only live caller
            }
            return complete ? Ins.Inserted : Ins.InsertedPartial;
        }

        // Caller holds dbGate. bulk=true (replacement / history): no live publish, no counter bump, no checkpoint K line.
        // ROUTER_v30: returns true when every post-insert step completed (always true on the bulk path, where exceptions propagate). false = the row is stored but a live post-insert step failed.
        private bool InsertCore(DayCtx ctx, TickRecord r, bool bulk, string keyOverride = null, bool recordInserted = false)
        {
            long seqN = AllocFor(ctx);                   // R3: every stored row is a genuinely new insert and allocates
            string seqText = SeqText(ctx.Day, seqN);
            r.SeqN = seqN; r.SeqNum = seqText;
            r.Stage = 1;
            tickCol.Insert(r);
            r.Stage = 2;
            if (recordInserted) PipelineTerminal(r, P_INSERT_RETURNED);   // LiteDB Insert returned normally ONLY. Does not imply the steps below completed.
            // ROUTER_v29: the row is stored from here on. On the live path (bulk=false) each downstream step is isolated, so one failing
            // step cannot skip the others (in particular the key-set update) and cannot turn a stored row into a "failed insert".
            // Bulk / history path (bulk=true): unchanged, exceptions propagate exactly as in v28. No transaction, no rollback.
            string failedSteps = null;
            RunPostInsertStep(bulk, "TAIL", r, ref failedSteps, () => TailNote(ctx, r, bulk), TailMarkDirty);   // STD06 (item 1): today's file tail (caller holds dbGate)
            bool nin = IsLiveTick(r);
            string key = keyOverride ?? KeyOf(r);
            Action addKey = () =>
            {
                lock (keyLock)
                {
                    if (nin) ctx.Nin.Add(key);
                    else { ctx.Hist.Add(key); ctx.Nin.Add(KeyOf(r)); if (ctx.IsCurrent) keyHistPlain.Add(KeyOf(r)); }   // HIST: occurrence key in Hist, plain key in Nin for the handover; REC11: plain key tagged HIST
                }
            };
            if (bulk) addKey();   // bulk / history: exactly as v28, exceptions propagate
            else EnsureLiveKeyRecorded(addKey, ctx, nin ? key : KeyOf(r), r, ref failedSteps);
            if (bulk) return true;
            RunPostInsertStep(false, "CHECKPOINT", r, ref failedSteps, () => CpAppendKey(nin, seqN, key), () => { lock (cpLock) { cpDirty = true; } });   // dirty = rewritten from the store by CpRepairDirty
            RunPostInsertStep(false, "COUNTERS", r, ref failedSteps, () => BumpStore(r), null);
            RunPostInsertStep(false, "PUBLISH", r, ref failedSteps, () => PublishRow(r), null);
            if (failedSteps != null) PostInsertPartial(r, failedSteps);
            return failedSteps == null;
        }

        // ROUTER_v30: live path only. Runs the key-set add. On failure: one retry, then Contains on the NIN key set. Only a successful Contains
        // counts as repaired. Otherwise the identity is held in the quarantine (InsertLive then refuses a second insert of it); if the quarantine
        // has no room, keyQuarOverflow is latched and InsertLive fails closed. The step is then reported as failed in failedSteps.
        private void EnsureLiveKeyRecorded(Action addKey, DayCtx ctx, string ninKey, TickRecord r, ref string failedSteps)
        {
            string firstErr = null;
            try { addKey(); return; }
            catch (Exception ex) { firstErr = ex.GetType().Name + ":" + ex.Message; }
            bool present = false;
            try { addKey(); lock (keyLock) { present = ctx.Nin.Contains(ninKey); } } catch (Exception) { present = false; }
            if (present)
            {
                Log("KEYSET_RETRY_OK | seq=" + r.SeqNum + " | t=" + r.EventTime.ToString(TsFmt, Inv) + " | first=" + firstErr + " | identity verified present in the key set");
                return;
            }
            bool held = false;
            try { lock (keyLock) { held = QuarantineKeyLocked(ninKey); } } catch (Exception) { held = false; }
            failedSteps = (failedSteps == null ? "" : failedSteps + ";") + "KEYSET:" + firstErr + ":" + (held ? "QUARANTINED_FAIL_CLOSED_FOR_IDENTITY" : "QUARANTINE_FULL_FAIL_CLOSED_FOR_ALL");
        }

        // ROUTER_v29. bulk=true: run the step as in v28 (exceptions propagate). bulk=false (live): catch, remember the step, run the repair hook.
        private void RunPostInsertStep(bool bulk, string step, TickRecord r, ref string failedSteps, Action act, Action repair)
        {
            if (bulk) { act(); return; }
            try { act(); }
            catch (Exception ex)
            {
                failedSteps = (failedSteps == null ? "" : failedSteps + ";") + step + ":" + ex.GetType().Name + ":" + ex.Message;
                if (repair != null) { try { repair(); } catch (Exception rex) { Debug("POST_INSERT_REPAIR_FAIL | " + Ex(rex)); } }
            }
        }

        private long livePostInsertPartial = 0;   // ROUTER_v29: stored live rows whose later processing failed (not failed inserts)
        private void PostInsertPartial(TickRecord r, string steps)
        {
            long n = Interlocked.Increment(ref livePostInsertPartial);
            LedgerProcessException(r, r.Stage);   // once per tick: ledger postInsertExc (the P_INSERT_RETURNED terminal is already recorded)
            if (n <= 50)
                Log("LIVE_POST_INSERT_PARTIAL | stored=true | n=" + n + " | seq=" + r.SeqNum + " | t=" + r.EventTime.ToString(TsFmt, Inv)
                    + " | p=" + r.Price.ToString("R", Inv) + " | v=" + r.Volume + " | steps=" + steps
                    + " | tailAndCheckpoint=marked for rewrite from store | keySetCountersBus=rebuilt from store at next attach or roll");
        }

        // Consume one claim from a cover count map. Guarded by lock (pixCover) at every caller.


        private RouterTick ToBus(TickRecord r, string direction)
        {
            return new RouterTick
            {
                EventTime = r.EventTime, Price = r.Price, Volume = r.Volume, OpenInterest = r.OpenInterest,
                Direction = direction ?? "UNKNOWN",
                Phase = r.Phase == "LIVE" ? "LIVE" : "HIST"            // HIST51: bus carries stored Phase (LIVE or HIST)
            };
        }

        // v34: every failed rebuild publish is counted; the first 20 are logged with time and price.
        private void LogRebuildPublishFail(string kind, DateTime t, double px)
        {
            long n = Interlocked.Increment(ref busPublishFails);
            if (n <= 20) Log("BUS_PUBLISH_FAIL | kind=" + kind + " | nth=" + n + " | t=" + t.ToString("yyyyMMdd HH:mm:ss.fff", Inv) + " | price=" + FmtPrice(px));
        }

        private void PublishRow(TickRecord r)
        {
            if (VECTOR.Publish(ToBus(r, r.Direction)))
            {
                Interlocked.Increment(ref livePublished);
                if (IsNativeTape(r))   // REC34: eligible tick volume = native rows only (Phase LIVE)
                {
                    lock (totalsLock)
                    {
                        vecTotalRaw += r.Volume;
                        if (r.Day.Date == currentSessionDay.Date) vecTodayRaw += r.Volume;
                    }
                }
            }
            else
            {
                long n = Interlocked.Increment(ref busPublishFails);
                Log("BUS_PUBLISH_FAIL | nth=" + n + " | t=" + r.EventTime.ToString("HH:mm:ss.fff", Inv) + " | price=" + FmtPrice(r.Price));
            }
        }

        // =====================================================================
        // v34: Clear (c[2] untouched when not at attach) -> per trading day, ascending: MINUTE and TICK rows merged by EventTime (TICK first on ties).
        // =====================================================================
        private void BusRebuild(bool atAttach, bool setDone)
        {
            var sw = Stopwatch.StartNew();
            // replacement and after every reconnect refetch - which spiked memory and held dbGate for the whole rebuild.
            // The rows are now streamed ONE DAY at a time and dbGate is held only while a day's batch is being read.
            // v34: publish order is chronological across the replay (see the merge loop below), not minute-block then tick-block.
            List<DateTime> days, minDays, tickDays;
            lock (totalsLock) { vecTotalRaw = 0; vecTodayRaw = 0; }   // REC34: counters follow the bus clear below
            // v40: the replay covers every stored day (no time window). MinValue is kept only so the REPLAY_WINDOW log line still prints.
            DateTime tickFrom = DateTime.MinValue;
            DateTime minFrom  = DateTime.MinValue;
            lock (dbGate)
            {
                if (db == null) return;
                // STD11 (SB.3): raw stored Day values, no .Date. Query().Select(x => x.Day) keeps the key only per document
                // (the projection runs inside the lazy reader), so the memory profile is unchanged from audit3 (#17).
                var minSet = new SortedSet<DateTime>();
                var tickSet = new SortedSet<DateTime>();
                foreach (DateTime d in minuteCol.Query().Select(x => x.Day).ToEnumerable()) minSet.Add(d);
                foreach (DateTime d in tickCol.Query().Select(x => x.Day).ToEnumerable()) tickSet.Add(d);
                var daySet = new SortedSet<DateTime>(minSet);
                daySet.UnionWith(tickSet);
                minDays = minSet.ToList();
                tickDays = tickSet.ToList();
                days = daySet.ToList();
            }
            Log("REPLAY_DAYS | count=" + days.Count
                + " | first=" + (days.Count > 0 ? days[0].ToString("o") : "none")
                + " | kind=" + (days.Count > 0 ? days[0].Kind.ToString() : "none"));   // STD11 (SB.5)
            Log("REPLAY_WINDOW | tickDays=" + tickDays.Count + " | tickFrom=" + tickFrom.ToString("yyyy-MM-dd", Inv)
                + " | minuteDays=" + minDays.Count + " | minuteFrom=" + minFrom.ToString("yyyy-MM-dd", Inv)
                + " | range=" + (days.Count > 0 ? days[0].ToString("yyyy-MM-dd", Inv) + ".." + days[days.Count - 1].ToString("yyyy-MM-dd", Inv) : "none"));   // STD40 (A5)
            VECTOR.Clear(atAttach);

            long minCount = 0, tickCount = 0, live = 0;
            int minuteDaysFound = 0, tickDaysFound = 0;   // STD11 (SB.5)
            // v34: one ascending pass over trading days. Within a day, MINUTE and TICK rows are merged by EventTime, so the bus is
            // in time order across the whole replay. Tie-break at equal EventTime: TICK before MINUTE; then Id. Each day is read
            // under dbGate and published after the lock is released.
            foreach (DateTime d in days)
            {
                List<MinuteRecord> mins = new List<MinuteRecord>();
                List<TickRecord> ticks = new List<TickRecord>();
                bool hasMin = minDays.Contains(d), hasTick = tickDays.Contains(d);
                lock (dbGate)
                {
                    if (db == null) break;
                    if (hasMin && minuteCol != null) mins = minuteCol.Find(x => x.Day == d).OrderBy(x => x.EventTime).ThenBy(x => x.Id).ToList();
                    if (hasTick && tickCol != null) ticks = tickCol.Find(x => x.Day == d).Where(x => !x.Superseded).OrderBy(x => x.EventTime).ThenBy(x => x.Id).ToList();
                }
                if (mins.Count > 0) minuteDaysFound++;   // STD11 (SB.5)
                if (ticks.Count > 0) tickDaysFound++;

                int mi = 0, ti = 0;
                double prev = double.NaN;   // price-vs-previous never crosses a day boundary
                while (mi < mins.Count || ti < ticks.Count)
                {
                    bool takeTick = ti < ticks.Count && (mi >= mins.Count || ticks[ti].EventTime <= mins[mi].EventTime);
                    if (takeTick)
                    {
                        TickRecord t = ticks[ti++];
                        string dir = DirOrClassify(t, prev);
                        prev = t.Price;
                        if (IsLiveTick(t)) live++;
                        if (VECTOR.Publish(ToBus(t, dir)))
                        {
                            if (IsNativeTape(t))   // REC34: eligible tick volume only
                            {
                                lock (totalsLock)
                                {
                                    vecTotalRaw += t.Volume;
                                    if (t.Day.Date == currentSessionDay.Date) vecTodayRaw += t.Volume;
                                }
                            }
                        }
                        else LogRebuildPublishFail("TICK", t.EventTime, t.Price);
                        tickCount++;
                    }
                    else
                    {
                        MinuteRecord m = mins[mi++];
                        // v17: bus-only tag. LiteDB and c[1] unaffected.
                        if (!VECTOR.Publish(new RouterTick { EventTime = m.EventTime, Price = m.Close, Volume = m.Volume, OpenInterest = m.OpenInterest, Direction = "NEUTRAL", Phase = "MINUTE" }))
                            LogRebuildPublishFail("MINUTE", m.EventTime, m.Close);
                        minCount++;
                    }
                }
            }
            Log("REPLAY_QUERY | days=" + days.Count + " | tickDaysWithRows=" + tickDaysFound + " | minuteDaysWithRows=" + minuteDaysFound
                + " | rowsPublished=" + (minCount + tickCount));   // STD11 (SB.5)
            if (setDone) VECTOR.SetReplayDone(true);
            Log((atAttach ? "REPLAY" : "BUS_REBUILD") + " | minute=" + minCount + " | ticks=" + tickCount + " | days=" + days.Count + " | nin=" + live
                + " | published=" + VECTOR.PublishedCount + " | elapsedMs=" + sw.ElapsedMilliseconds);
        }

        // =====================================================================
        // fetch orchestration
        // =====================================================================

        // asked for the day now, and in that case increments the day's
        // attempt count under covAttemptLock. Returns false when the day
        // already has rows (STD52) or when its attempt count has reached
        // CoverageAttemptMax (STD58).
        //
        // why is used only in log text, so the caller can name itself.


        // =====================================================================
        //   held day (weekend / holiday / before 09:00)  -> fill 09:15 -> 15:41
        //   query failed OR no LIVE rows from 09:15      -> Case A  09:15 -> attach
        //   earliest > 09:15:30 AND latest < attach-30 s -> Case E  09:15 -> attach (one window covers both gaps)
        //   earliest > 09:15:30                          -> Case D  09:15 -> earliest
        //   latest < attach-30 s (downtime gap)          -> Case C  latest+1 s -> attach
        // Every non-held window ends at min(attach, 15:41); an empty window is not owed. Each window runs at most once.
        // =====================================================================

        // One streaming pass (no list is built, dbGate held only while reading): count / earliest / latest of the active LIVE rows of the day from 09:15.
        // Returns false when the store cannot be read; the caller treats that as Case A.

        // STD78: NIN volume (raw units) with EventTime within sec seconds of t.

        // STD76: true when some value in the sorted list lies within sec seconds of t.

        // R25 hub reconnect: last stored EventTime -> the RECONNECT event's own EventTime. R26 detection does not apply.
        // sits at (roughly) "now", so the outage was never backfilled at all. The window therefore starts at
        // min(last stored EventTime, the local time of the last hub drop), floored at 09:15 - the replacement is idempotent,
        // so a wider window costs only the fetch.


        // =====================================================================
        // run only when RECON is positive (lots). The STD13 mid-session heal (20 s Missing timer, 5 min cooldown, same-residual guard) is deleted.
        // =====================================================================

        // Process the buffered live ticks in ARRIVAL ORDER through the single insert path, then open the gate.
        private void DrainAndGoLive()
        {
            int total = 0;
            while (true)
            {
                List<TickRecord> batch;
                lock (liveGate)
                {
                    if (livePending.Count == 0) { liveReady = true; break; }
                    batch = new List<TickRecord>(livePending);
                    livePending.Clear();
                }
                foreach (TickRecord rec in batch)
                {
                    // The gate is closed (liveReady=false) and liveGate is NOT held: a roll here never touches livePending,
                    // so ticks that arrive meanwhile queue behind this batch and are drained by the next loop pass.
                    if (db != null && rec.Day.Date > currentSessionDay.Date && !TryRoll(rec.Day.Date, "FIRST_TICK", rec.EventTime))
                    {
                        // STD07-AUDIT: the roll failed, so this tick cannot be inserted (ProcessLive would refuse a foreign day).
                        // It was already lost - now it is counted and named instead of vanishing behind TryRoll's generic LIVE_FAIL.
                        PipelineTerminal(rec, P_ROLL_DROP);
                        Interlocked.Increment(ref liveDroppedCount);
                        Log("LIVE_ROLL_DROP | t=" + rec.EventTime.ToString(TsFmt, Inv) + " | tickDay=" + rec.Day.ToString("yyyy-MM-dd", Inv)
                            + " | session=" + currentSessionDay.ToString("yyyy-MM-dd", Inv) + " | reason=roll_failed");
                        continue;
                    }
                    ProcessLiveCounted(rec); total++;
                }
            }
            if (total > 0) Log("LIVE_PENDING_FLUSHED | rows=" + total + " | dropped=" + Interlocked.Read(ref liveDroppedCount));
        }

        // =====================================================================
        // live NIN path
        // =====================================================================
        // ROUTER_STD81_RECOVER: native 1-Tick capture, same pattern as NATIVE_TICK_DUMP. Historical + Transition only.
        // Keeps today's ticks from 09:15 in histRecover; RecoverTodayFromNativeSeries drains it. No barsAgo indexing anywhere.
        protected override void OnBarUpdate()
        {
            if (BarsInProgress != 2) return;   // index 2 = the 1-Tick series added in State.Configure
            if (State != State.Historical && State != State.Transition) return;
            DateTime day = histRecoverDay;
            if (day == DateTime.MinValue || CurrentBars == null || CurrentBars.Length < 3 || CurrentBars[2] < 0) return;
            DateTime t = Times[2][0];
            if (t.Date != day || t < day.Add(MarketOpenTod)) return;
            var rec = new TickRecord
            {
                EventTime = t,
                Day = SessionDay(t),
                Price = Closes[2][0],
                Volume = (long)Volumes[2][0],
                OpenInterest = -1,   // unknown: the codebase's unknown OI is -1, never 0
                Phase = "HIST",
                DayVolume = -1
            };
            lock (histRecover) { histRecover.Add(rec); }
        }

        private int HistCapturedCount() { lock (histRecover) { return histRecover.Count; } }

        // REC41 Gate C helpers. Read or write only in-memory counters and the log.
        private string OiCountsText()
        {
            long last = Interlocked.Read(ref oiLastValidEventTicks);
            string lastS = last > 0 ? new DateTime(last).ToString("HH:mm:ss", Inv) : "NONE";
            return "events since attach " + Interlocked.Read(ref oiEvtCount) + " | valid " + Interlocked.Read(ref oiValidCount)
                + " | zero-field " + Interlocked.Read(ref oiZeroFieldCount) + " | unknown " + Interlocked.Read(ref oiUnknownCount)
                + " | last valid " + lastS + " | " + (oiStaleLogged ? "STALE" : "OK");
        }

        // Warns once per session when no valid OI has arrived for OiStaleMinutes of exchange time (since the last valid OI, or since 09:15).
        // ROUTER_v07 (D9b): staleness is measured from the LATEST of 09:15, the attach time, and the last valid OI
        // seen by this attach. A late attach therefore cannot warn before it has had OiStaleMinutes to see OI.
        private void CheckOiFeedStale(DateTime t)
        {
            if (oiStaleLogged) return;
            DateTime refT = currentSessionDay.Date.Add(MarketOpenTod);
            long last = Interlocked.Read(ref oiLastValidEventTicks);
            if (last > 0 && new DateTime(last) > refT) refT = new DateTime(last);
            if (attachEventTime != DateTime.MinValue && attachEventTime > refT) refT = attachEventTime;
            if (t < refT) return;
            if ((t - refT).TotalMinutes < OiStaleMinutes) return;
            oiStaleLogged = true;
            Log("OI_FEED_STALE | minutes=" + OiStaleMinutes + " | sinceRef=" + refT.ToString("HH:mm:ss", Inv) + " | " + OiCountsText());
        }

        protected override void OnMarketData(MarketDataEventArgs e)
        {
            Interlocked.Increment(ref ledgerCallbacksEntered);   // diagnostic, APPROX
            bool resolved = false;
            try { OnMarketDataBody(e, ref resolved); }
            catch (Exception) { if (!resolved) LedgerIngress(ref resolved, X_EXC); throw; }
        }

        private void OnMarketDataBody(MarketDataEventArgs e, ref bool resolved)
        {
            // QDIAG: quote events are counted before the guard, so feed events and guard discards can be told apart. Counts only.
            bool qdQ = e != null && (e.MarketDataType == MarketDataType.Bid || e.MarketDataType == MarketDataType.Ask);
            bool qdIsBid = qdQ && e.MarketDataType == MarketDataType.Bid;
            if (qdQ) { lock (quoteLock) { if (qdIsBid) qdBidSeen++; else qdAskSeen++; } }
            if (e == null || !ownerReady || terminated || autoStopped)
            {
                if (qdQ) { lock (quoteLock) { if (qdIsBid) qdBidGuardDrop++; else qdAskGuardDrop++; } }
                LedgerIngress(ref resolved, e == null ? X_NULL : X_GUARD);
                return;
            }

            if (e.MarketDataType == MarketDataType.Bid) { LedgerIngress(ref resolved, X_QUOTE_BID); lock (quoteLock) { qdBidPassed++; liveBid = SnapQuoteToTick(e.Price); liveBidQty = (long)e.Volume; qdLastBidRaw = e.Price; QdTally(true, e.Price); } return; }   // record only, never classify here
            if (e.MarketDataType == MarketDataType.Ask) { LedgerIngress(ref resolved, X_QUOTE_ASK); lock (quoteLock) { qdAskPassed++; liveAsk = SnapQuoteToTick(e.Price); liveAskQty = (long)e.Volume; qdLastAskRaw = e.Price; QdTally(false, e.Price); } return; }
            if (e.MarketDataType == MarketDataType.DailyVolume)
            {
                LedgerIngress(ref resolved, X_DAYVOL);
                if (e.Volume > 0)
                {
                    Interlocked.Exchange(ref cachedDayVolume, (long)e.Volume);
                    Interlocked.Exchange(ref cachedDayVolumeDayTicks, currentSessionDay.Date.Ticks);   // STD64
                }
                return;
            }
            if (e.MarketDataType == MarketDataType.OpenInterest)
            {
                LedgerIngress(ref resolved, X_OI);
                // REC41: the Price-as-OI fallback is REMOVED. OI is read from the Volume field only.
                // A zero or missing field is UNKNOWN (-1): never a price, never a stored 0. Counters only.
                long oiVol = (long)e.Volume;
                long oiVal = oiVol > 0 ? oiVol : -1L;
                Interlocked.Increment(ref oiEvtCount);
                if (oiVol == 0)
                {
                    Interlocked.Increment(ref oiZeroFieldCount);
                    if (oiZeroLogN < 10) { oiZeroLogN++; Log("OI_ZERO_SEEN | n=" + oiZeroLogN + " | volField=0 | priceField=" + (long)e.Price + " | resultingOi=-1 | t=" + e.Time.ToString(TsFmt, Inv)); }
                }
                if (oiVal > 0)
                {
                    Interlocked.Increment(ref oiValidCount);
                    Interlocked.Exchange(ref oiLastValidEventTicks, e.Time.Ticks);
                }
                else Interlocked.Increment(ref oiUnknownCount);
                if (oiVal > 0) Interlocked.Exchange(ref cachedOi, oiVal);   // ROUTER_v16 / v33: unknown (-1) never overwrites a carried valid OI
                if (oiVal > 0 && oiSeeded) { oiSeeded = false; Log("OI_LIVE_RESUMED | oi=" + oiVal + " | t=" + e.Time.ToString(TsFmt, Inv)); }
                return;
            }
            if (e.MarketDataType != MarketDataType.Last) { LedgerIngress(ref resolved, X_OTHER); return; }
            if (e.Time == DateTime.MinValue) { LedgerIngress(ref resolved, X_TIMEMIN); return; }

            // R26 / audit3 (#23): detection is triggered ONLY by a Last tick now. It used to start on ANY market event, so a
            // Bid or Ask tick at 09:15 opened the 3 s window and a quiet first second produced a false "Late login detected".
            // A Last tick inside the window is what actually proves a live feed.
            // REC31: mode from the clock and the first Last tick only. No timer, no detection window.
            if (modeState == ModeSkipped && IsTradingDate(e.Time.Date))
            {
                modeState = e.Time.TimeOfDay < MarketOpenTod ? ModeWaitOpen : ModeLiveNin;
                Log("MODE_SET | mode=" + ModeName(modeState) + " | first Last tick " + e.Time.ToString(TsFmt, Inv));
            }
            else if (modeState == ModeWaitOpen && IsTradingDate(e.Time.Date) && e.Time.TimeOfDay >= MarketOpenTod)
            {
                modeState = ModeLiveNin;
                Log("MODE_SET | mode=LIVE_NIN | first Last tick " + e.Time.ToString(TsFmt, Inv));
            }

            if (e.Volume < 0) { LedgerIngress(ref resolved, X_VOLNEG); return; }   // R15: volume-0 rows ARE stored

            // REC24: historical-to-live boundary, tested before SnapToTick so a dropped tick never uses a SNAP log slot.
            long ft = Interlocked.Read(ref liveFloorTicks);
            if (ft >= 0 && e.Time.Ticks <= ft)
            {
                LedgerIngress(ref resolved, X_FLOOR_INGEST);
                Log("BOUNDARY_DROP | t=" + e.Time.ToString("HH:mm:ss.fff", Inv) + " | p=" + e.Price.ToString("R", Inv) + " | v=" + (long)e.Volume);
                return;
            }
            CheckOiFeedStale(e.Time);              // REC41 Gate C: warning only, never a fill
            double livePx = SnapToTick(e.Price);   // REC14
            Interlocked.Exchange(ref lastNinTickTicks, DateTime.UtcNow.Ticks);   // SAPI feed-state only (never data)

            var rec = new TickRecord
            {
                EventTime = e.Time,                       // the tick's own EventTime: never shifted, rounded or replaced
                Day = SessionDay(e.Time),
                Price = livePx,   // REC14: tick-grid price, not the float32 artifact
                Volume = (long)e.Volume,
                OpenInterest = Interlocked.Read(ref cachedOi),
                Phase = "LIVE",                           // NinjaTrader live tick
                DayVolume = Interlocked.Read(ref cachedDayVolume)
            };

            LedgerAccept(ref resolved, rec);
            bool roll = false;
            bool attempted = false;   // true once a carrier (livePending or EnqueueLive) is about to be entered
            try
            {
            lock (liveGate)
            {
                if (!liveReady)
                {
                    // startup / detection / replacement / roll: buffered, processed later in arrival order. Never silently discarded.
                    if (livePending.Count < LivePendingMaxRows)
                    {
                        attempted = true;
                        livePending.Add(rec); Interlocked.Increment(ref liveReceivedSeq); Interlocked.Exchange(ref liveRecvUtcTicks, DateTime.UtcNow.Ticks);
                    }
                    else { PipelineTerminal(rec, P_PEND_OVER); Interlocked.Increment(ref liveDroppedCount); Debug("LIVE_PENDING_OVERFLOW | t=" + rec.EventTime.ToString(TsFmt, Inv)); }
                    return;
                }
                if (db != null && rec.Day.Date > currentSessionDay.Date)
                {
                    // Session roll: close the gate and queue this tick FIRST (livePending is empty while liveReady is true), then leave liveGate.
                    liveReady = false;
                    attempted = true;
                    livePending.Add(rec); Interlocked.Increment(ref liveReceivedSeq); Interlocked.Exchange(ref liveRecvUtcTicks, DateTime.UtcNow.Ticks);
                    roll = true;
                }
                else { attempted = true; EnqueueLive(rec); }   // STD06 (item 9): was ProcessLive(rec) on the data thread
            }
            }
            catch (Exception)
            {
                // Not attempted: no carrier holds the tick, so it is recorded once here.
                // Attempted: ownership may have transferred; nothing is recorded here, the tick stays in O as visible residue.
                if (!attempted) HandOffFailed(rec);   // no-op if a terminal was already recorded (e.g. P_PEND_OVER before a failing Debug)
                throw;
            }
            if (!roll) return;

            // STD07-AUDIT (item 8): the roll + drain run on a pool thread via RollAsyncOrSkip (defined below TryRoll, single-flight
            // guarded) - the NT8 data thread returns immediately; the triggering tick is already buffered in livePending.
            RollAsyncOrSkip(rec.Day.Date, "FIRST_TICK", rec.EventTime);
        }

        // holds it) with liveGate NOT held and liveReady=false, so live ticks keep buffering. Returns false (and logs) if the roll threw.
        private bool TryRoll(DateTime newDay, string why, DateTime eventLocal)
        {
            try
            {
                // failure, which makes DrainAndGoLive count the triggering tick as LIVE_ROLL_DROP instead of inserting it against
                // the old session day. DrainAndGoLive is deliberately NOT called from here: RollAsyncOrSkip calls it right after this
                // returns, and DrainAndGoLive itself calls TryRoll, so calling it here would recurse.
                if (!WaitLiveWriter("ROLL_" + why))
                {
                    Log("FILL_ABORT_LIVE_WRITER | site=ROLL_" + why + " | waitedMs=" + LiveWriterFlushMs);
                    return false;
                }
                Stopwatch waitSw = Stopwatch.StartNew();
                lock (maintLock)
                {
                    waitSw.Stop();
                    if (waitSw.ElapsedMilliseconds > 1000)
                        Log("ROLL_MAINT_WAIT | waitMs=" + waitSw.ElapsedMilliseconds + " | why=" + why + " | to=" + newDay.ToString("yyyy-MM-dd", Inv));
                    RollSession(newDay, why, eventLocal);
                }
                return true;
            }
            catch (Exception ex)
            {
                Log("LIVE_FAIL | " + Ex(ex));
                Debug("LIVE_FAIL | " + ex.ToString());
                return false;
            }
        }

        private int rollInFlight = 0;   // STD07-AUDIT (item 8): single-flight guard - one roll+drain task at a time
        // audit4 (#3): a request that arrives while a roll task is in flight used to be dropped (ROLL_ASYNC_SKIP) after its caller had ALREADY
        // closed the gate (liveReady=false) and buffered the tick. If the in-flight task had opened the gate just before the skip, nothing
        // drained again and both retrigger paths (first tick / 09:00 timer) require liveReady==true, so ticks piled up in livePending until
        // the cap. A skipped request is now REMEMBERED (rollRerun + its arguments) and the finishing task runs once more. Interlocked only:
        // no new lock, so the lock hierarchy is unchanged.
private sealed class RollReq { public DateTime Day; public DateTime Ev; public string Why; }

    private RollReq rollRerunReq = null;

        // STD07-AUDIT (item 8): roll + drain off the caller's thread. The caller has already closed the gate (liveReady=false) and
        // buffered the triggering tick, so new ticks keep queueing in livePending / the writer queue and nothing is lost. If a
        // second trigger arrives while a roll task is running it is queued as a rerun (audit4 #3): the running task finishes its
        // roll + drain, releases the single-flight flag, and then runs once more for the queued request.
        private void RollAsyncOrSkip(DateTime newDay, string why, DateTime eventLocal)
        {
            if (Interlocked.CompareExchange(ref rollInFlight, 1, 0) != 0)
            {
                // record the request FIRST, then re-test the flag: if the running task released it in between, we take over;
                // otherwise the task is guaranteed to see rollRerun=1 after it releases (it checks AFTER the release).
                Interlocked.Exchange(ref rollRerunReq, new RollReq { Day = newDay, Ev = eventLocal, Why = why });
                Log("ROLL_ASYNC_SKIP | already in flight - rerun queued | day=" + newDay.ToString("yyyy-MM-dd", Inv) + " | why=" + why);
                if (Interlocked.CompareExchange(ref rollInFlight, 1, 0) != 0) return;
                Interlocked.Exchange(ref rollRerunReq, null);   // we own the flag now and run this request ourselves
            }
            Task.Run(() =>
            {
                DateTime day = newDay, ev = eventLocal; string w = why;
                while (true)
                {
                    try
                    {
                        // STD18 (M1, follow-up): the drain is named explicitly on the aborted-roll path instead of relying on a bare
                        // trailing call. Still exactly one DrainAndGoLive() per path, and still OUTSIDE TryRoll, so the
                        // DrainAndGoLive -> TryRoll edge cannot recurse.
                        if (!TryRoll(day, w, ev))
                        {
                            Log("FILL_ABORT_LIVE_WRITER | site=ROLL_ASYNC | waitedMs=" + LiveWriterFlushMs);
                            try { DrainAndGoLive(); } catch (Exception dex) { Log("ROLL_DRAIN_EX | " + Ex(dex)); lock (liveGate) { liveReady = true; } }
                        }
                        else DrainAndGoLive();
                    }
                    catch (Exception ex) { Log("ROLL_ASYNC_EX | " + Ex(ex)); }
Interlocked.Exchange(ref rollInFlight, 0);
                    RollReq req = Interlocked.Exchange(ref rollRerunReq, null);
                    if (req == null) return;
                    if (Interlocked.CompareExchange(ref rollInFlight, 1, 0) != 0) return;
                    day = req.Day; ev = req.Ev; w = req.Why ?? w;
                    Log("ROLL_ASYNC_RERUN | day=" + day.ToString("yyyy-MM-dd", Inv) + " | why=" + w);
                }
            });
        }

        // STD06 (item 9): called under liveGate with liveReady=true. Never blocks; a full or closed queue counts into liveDroppedCount.
        private void EnqueueLive(TickRecord rec)
        {
            var q = liveQueue;
            if (q == null)
            {
                // STD27 (M16): no writer. If this is a writer crash (not shutdown),
                // restart it. If the restart fails or we are terminated, drop.
                if (!terminated)
                {
                    try { StartLiveWriter(); q = liveQueue; }
                    catch (Exception) { PipelineTerminal(rec, P_QUEUE_GONE); throw; }   // writer could not start: this tick has no carrier
                }
                if (q == null)
                {
                    PipelineTerminal(rec, P_QUEUE_GONE);
                    Interlocked.Increment(ref liveDroppedCount);
                    Debug("LIVE_QUEUE_GONE | terminated=" + terminated + " | t=" + rec.EventTime.ToString(TsFmt, Inv));
                    return;
                }
            }
            // ROUTER_v29: the tick carries a queue charge. liveQueued is released exactly once per tick through ReleaseQueueCharge, by the
            // writer when it finishes the tick, or by this producer when it settles the tick itself. State changes happen under lock(rec).
            lock (rec) { rec.QueueCharge = 1; }
            Interlocked.Increment(ref liveQueued);
            bool ok = false;
            Exception addEx = null;
            try { ok = q.TryAdd(rec); }
            catch (Exception tex) { addEx = tex; }
            if (ok) { Interlocked.Increment(ref liveReceivedSeq); Interlocked.Exchange(ref liveRecvUtcTicks, DateTime.UtcNow.Ticks); return; }
            if (addEx == null)
            {
                // TryAdd returned false: the item was not added (queue full).
                ReleaseQueueCharge(rec);
                PipelineTerminal(rec, P_QUEUE_FULL);
                Interlocked.Increment(ref liveDroppedCount);
                Debug("LIVE_QUEUE_OVERFLOW | t=" + rec.EventTime.ToString(TsFmt, Inv));
                return;
            }
            // TryAdd threw. Ownership is decided by the per-record state, not by assumption:
            //   state 1 (writer has not taken the tick): the producer revokes it. The writer will skip it, so it is never processed;
            //     liveQueued is released here and the tick is recorded as dropped (P_HANDOFF_FAILED).
            //   state 3 or 2 (writer already took or finished it): the tick was queued and the writer owns it; nothing is released here.
            bool revoked = false;
            lock (rec) { if (rec.QueueCharge == 1) { rec.QueueCharge = 4; revoked = true; } }
            if (revoked)
            {
                Interlocked.Decrement(ref liveQueued);
                HandOffFailed(rec);
                Interlocked.Increment(ref liveDroppedCount);
                Debug("LIVE_QUEUE_TRYADD_EXC | outcome=REVOKED_NOT_PROCESSED | t=" + rec.EventTime.ToString(TsFmt, Inv) + " | p=" + rec.Price.ToString("R", Inv)
                    + " | v=" + rec.Volume + " | exc=" + Ex(addEx));
            }
            else
            {
                Interlocked.Increment(ref liveReceivedSeq); Interlocked.Exchange(ref liveRecvUtcTicks, DateTime.UtcNow.Ticks);
                Debug("LIVE_QUEUE_TRYADD_EXC | outcome=ALREADY_TAKEN_BY_WRITER | t=" + rec.EventTime.ToString(TsFmt, Inv) + " | exc=" + Ex(addEx));
            }
        }

        // ROUTER_v29: releases the tick's queue charge exactly once (state 1 or 3 -> 2). A tick that was never queued (0), already released (2)
        // or revoked by the producer (4) changes nothing, so liveQueued cannot be decremented twice for one tick.
        private void ReleaseQueueCharge(TickRecord r)
        {
            lock (r) { if (r.QueueCharge != 1 && r.QueueCharge != 3) return; r.QueueCharge = 2; }
            Interlocked.Decrement(ref liveQueued);
        }

        private void StartLiveWriter()
        {
            lock (liveWriterLock)
            {
                if (liveWriter != null) return;
                var q = new System.Collections.Concurrent.BlockingCollection<TickRecord>(LivePendingMaxRows);
                var t = new Thread(() => LiveWriterLoop(q)) { IsBackground = true, Name = "ROUTER_LIVE_WRITER" };
                // ROUTER_v28: start the thread BEFORE publishing the queue and writer fields. If Start() throws, nothing is
                // published, so EnqueueLive never sees a queue that has no consumer and the next call can try again.
                t.Start();
                Interlocked.Exchange(ref liveQueued, 0);   // STD40 (A2): a dead writer's unconsumed items must not keep WaitLiveWriter waiting on the new queue
                liveQueue = q;
                liveWriter = t;
            }
        }

        private void LiveWriterLoop(System.Collections.Concurrent.BlockingCollection<TickRecord> q)
        {
            try
            {
                foreach (TickRecord rec in q.GetConsumingEnumerable())
                {
                    try { ProcessLiveCounted(rec); }
                    catch (Exception ex) { Log("LIVE_WRITER_EX | " + Ex(ex)); }   // ProcessLive already catches; defensive only
                    finally { ReleaseQueueCharge(rec); }   // ROUTER_v29: exactly-once release
                }
            }
            catch (Exception ex)
            {
                Debug("LIVE_WRITER_LOOP_EX | " + ex.ToString());
                // STD17 (C1): the writer thread is dead. Clear both fields so EnqueueLive's q == null fallback re-engages and
                // StartLiveWriter can build a fresh queue - otherwise the queue fills to LivePendingMaxRows with no consumer.
                // CompleteAdding is guarded: if it threw, liveQueue would keep its stale value and the fix below would never run.
                // ROUTER_v01 (fix 4): drain items the dead writer never processed, so liveQueued returns to zero.
                try
                {
                    while (q.TryTake(out TickRecord leftover))
                    {
                        try { ProcessLiveCounted(leftover); }
                        catch (Exception lex) { Log("LIVE_WRITER_LEFTOVER_EX | " + Ex(lex)); }
                        finally { ReleaseQueueCharge(leftover); }   // ROUTER_v29: exactly-once release
                    }
                }
                catch (Exception dex) { Debug("LIVE_WRITER_DRAIN_EX | " + Ex(dex)); }
                try { q.CompleteAdding(); } catch { }
                lock (liveWriterLock) { liveQueue = null; liveWriter = null; }
            }
        }

        // Waits (bounded) until every enqueued tick has been processed. Callers hold neither liveGate nor dbGate.
        private bool WaitLiveWriter(string why)
        {
            if (liveQueue == null) return true;
            Stopwatch sw = Stopwatch.StartNew();
            while (Interlocked.Read(ref liveQueued) > 0)
            {
                if (sw.ElapsedMilliseconds > LiveWriterFlushMs)
                {
                    Log("LIVE_WRITER_FLUSH_TIMEOUT | why=" + why + " | queued=" + Interlocked.Read(ref liveQueued));
                    return false;
                }
                Thread.Sleep(2);
            }
            return true;
        }

        // Detach: no more enqueues (ownerReady / terminated are already set); drain what is queued, then stop the thread.
        // audit3 (#14): the join is bounded by the caller's budget, so the unload tail stays inside ShutdownTailBudgetMs.
        private bool StopLiveWriter(int joinMs)
        {
            System.Collections.Concurrent.BlockingCollection<TickRecord> q; Thread t;
            lock (liveWriterLock) { q = liveQueue; t = liveWriter; }
            if (q == null) return true;
            try { q.CompleteAdding(); } catch { }
            bool joined = true;
            if (t != null && !t.Join(joinMs))
            {
                joined = false;
                Log("LIVE_WRITER_STOP_TIMEOUT | joinedMs=" + joinMs + " | queued=" + Interlocked.Read(ref liveQueued));
            }
            lock (liveWriterLock) { liveQueue = null; liveWriter = null; }
            return joined;
        }

        // Caller serialises (liveGate held, or the drain loop while the gate is closed). No roll here: OnMarketData / DrainAndGoLive roll first.
        // STD06: runs on the single writer thread for live ticks (or on the drain loop while the gate is closed and the queue is empty).
        // DIAG (REC06_DIAG): cumulative live NEUTRAL count and live branch tally. Diagnostic only.
        private long liveNeutralCount = 0;
        // QDIAG: quote reception, validation and trade-time outcome counters. Diagnostic only, never read by logic.
        private long qdBidSeen, qdAskSeen, qdBidGuardDrop, qdAskGuardDrop, qdBidPassed, qdAskPassed;   // seen/guardDrop/passed: quote events
        private long qdBidValid, qdBidNaN, qdBidPosInf, qdBidNegInf, qdBidNonPos;                       // per-event bid validation (guarded by quoteLock)
        private long qdAskValid, qdAskNaN, qdAskPosInf, qdAskNegInf, qdAskNonPos;                       // per-event ask validation (guarded by quoteLock)
        private long qdCrossEvents, qdCrossEpisodes; private bool qdCrossPrev;                          // guarded by quoteLock
        private double qdLastBidRaw = double.NaN, qdLastAskRaw = double.NaN;                             // guarded by quoteLock
        private long qdTTotal, qdTNoBid, qdTNoAsk, qdTNonFinite, qdTNonPos, qdTCrossed, qdTOk, qdTInfUsed;  // trade-time outcome, exclusive by first failing check
        private long qdTBidNaN, qdTBidPosInf, qdTBidNegInf, qdTAskNaN, qdTAskPosInf, qdTAskNegInf;      // trade-time per-side non-finite, independent of the exclusive outcome
        private long qdNeutIn, qdNeutQuoted, qdNeutNoInherit;                                           // NEUTRAL trace counters (qdNeutQuoted has its own cap)
        // QDIAG: called under quoteLock after the assignment. Buckets: NaN, +Infinity, -Infinity, finite <= 0, valid (finite > 0).
        // crossedEvents = number of quote events after which the stored pair has bid > ask. It repeats for every event while one crossed pair stays stored.
        // crossedEpisodes = number of transitions from not-crossed to crossed. This is the count of distinct crossed-book episodes.
        private void QdTally(bool isBid, double px)
        {
            if (isBid)
            {
                if (double.IsNaN(px)) qdBidNaN++;
                else if (double.IsPositiveInfinity(px)) qdBidPosInf++;
                else if (double.IsNegativeInfinity(px)) qdBidNegInf++;
                else if (px <= 0) qdBidNonPos++;
                else qdBidValid++;
            }
            else
            {
                if (double.IsNaN(px)) qdAskNaN++;
                else if (double.IsPositiveInfinity(px)) qdAskPosInf++;
                else if (double.IsNegativeInfinity(px)) qdAskNegInf++;
                else if (px <= 0) qdAskNonPos++;
                else qdAskValid++;
            }
            bool crossed = !double.IsNaN(liveBid) && !double.IsNaN(liveAsk) && liveBid > liveAsk;
            if (crossed) { qdCrossEvents++; if (!qdCrossPrev) qdCrossEpisodes++; }
            qdCrossPrev = crossed;
        }
        // REC34 lot totals (raw contracts). LITEDB: native rows in the retained window + live inserts. VECTOR: native tick rows published.
        private long litedbWindowRaw = 0;
        private long lateLoginRaw = 0;   // REC45: native trades rewritten at the last late-login attach (raw contracts)
        private long liveRaw = 0;        // REC45: live trades stored since the last late-login rewrite (raw contracts)
        private long vecTotalRaw = 0;
        private long vecTodayRaw = 0;
        private readonly object totalsLock = new object();   // REC35: one lock for the three lot totals (writers and the dashboard snapshot)
        private readonly Dictionary<string, int> liveBranchTally = new Dictionary<string, int>();
        private readonly object liveBranchLock = new object();

        // ROUTER_v02 (RECON A): every tick that reaches ProcessLive is counted once it has finished, duplicates and day-filtered rows included.
        // ROUTER_v02 (fix 2): a failed store operation is counted as a failure, never as a successful insert.
        private void FailLive(string msg)
        {
            Interlocked.Increment(ref liveFailedSeq);
            Log(msg);
        }

        private void ProcessLiveCounted(TickRecord rec)
        {
            // ROUTER_v29: take ownership under lock(rec). A tick the producer revoked after a TryAdd exception is skipped: the producer has already
            // released liveQueued and recorded the terminal, and it is not counted as processed.
            bool revoked = false;
            lock (rec) { if (rec.QueueCharge == 4) revoked = true; else if (rec.QueueCharge == 1) rec.QueueCharge = 3; }
            if (revoked) { Debug("LIVE_QUEUE_REVOKED_SKIP | t=" + rec.EventTime.ToString(TsFmt, Inv) + " | p=" + rec.Price.ToString("R", Inv) + " | v=" + rec.Volume); return; }
            try { ProcessLive(rec); }
            finally { Interlocked.Increment(ref liveProcessedSeq); }
        }

        // ROUTER_v08 (D10), read only: is this tick's key already stored today? Used only to keep duplicates out of
        // the classifier counts and QDIAG lines. The real insert decision is still made by InsertLive.
        private bool IsKnownLiveDuplicate(TickRecord r)
        {
            if (db == null || tickCol == null) return false;
            DayCtx ctx = CurrentCtx();
            lock (keyLock) { return ctx.Nin.Contains(KeyOf(r)); }
        }

        private void ProcessLive(TickRecord rec)
        {
            try
            {
                long floorNow = Interlocked.Read(ref liveFloorTicks);   // ROUTER_v18: floor guard on every live insert path
                if (floorNow >= 0 && rec.EventTime.Ticks <= floorNow)
                {
                    Interlocked.Increment(ref liveReceived);   // ROUTER_v22: received counts every tick that reaches ProcessLive, early drops included
                    PipelineTerminal(rec, P_FLOOR_EARLY);
                    Interlocked.Increment(ref liveFloorDropped);
                    Log("BOUNDARY_DROP | path=PROCESS_LIVE | t=" + rec.EventTime.ToString(TsFmt, Inv) + " | p=" + rec.Price.ToString("R", Inv) + " | v=" + rec.Volume);
                    return;
                }
                if (db == null)
                {
                    // ROUTER_v23: a tick with no open database is lost; count it so the DROPPED line and the tick flow show it.
                    PipelineTerminal(rec, P_DB_NULL);
                    Interlocked.Increment(ref liveDroppedCount);
                    if (!terminated && Interlocked.Increment(ref liveDbNullLogN) <= 50) Log("LIVE_DB_NULL_DROP | t=" + rec.EventTime.ToString(TsFmt, Inv) + " | p=" + rec.Price.ToString("R", Inv) + " | v=" + rec.Volume);
                    return;
                }
                if (rec.Day.Date != currentSessionDay.Date)
                {
                    PipelineTerminal(rec, P_DAY_MISMATCH);
                    string tag = rec.Day.Date > currentSessionDay.Date ? "LIVE_ROLL_MISSING" : "LIVE_STALE_DAY";
                    Log(tag + " | tickDay=" + rec.Day.ToString("yyyy-MM-dd", Inv) + " | session=" + currentSessionDay.ToString("yyyy-MM-dd", Inv) + " | not inserted");
                    Interlocked.Increment(ref liveDroppedCount);
                    return;   // defensive: a tick of another day is never written into this day's store
                }
                if (mainRotatePending)   // R18: MAIN / DEBUG rotate on the first tick of the new trading day
                {
                    RotateLogs(currentSessionDay);
                    Log("LOG_ROTATED | day=" + DayTag(currentSessionDay) + " | on=first tick");
                }

                Interlocked.Increment(ref liveReceived);

                double bid, ask; long bq, aq;
                lock (quoteLock) { bid = liveBid; ask = liveAsk; bq = liveBidQty; aq = liveAskQty; }
                bool hit = !double.IsNaN(bid) && !double.IsNaN(ask) && bid > 0 && ask > 0 && bid <= ask;
                // QDIAG (read only): trade-time outcome, exclusive by first failing check. The hit expression above is unchanged.
                Interlocked.Increment(ref qdTTotal);
                if (double.IsNaN(bid)) Interlocked.Increment(ref qdTNoBid);
                else if (double.IsNaN(ask)) Interlocked.Increment(ref qdTNoAsk);
                else if (double.IsInfinity(bid) || double.IsInfinity(ask)) Interlocked.Increment(ref qdTNonFinite);
                else if (bid <= 0 || ask <= 0) Interlocked.Increment(ref qdTNonPos);
                else if (bid > ask) Interlocked.Increment(ref qdTCrossed);
                else Interlocked.Increment(ref qdTOk);   // finite, positive, uncrossed
                if (hit && (double.IsInfinity(bid) || double.IsInfinity(ask))) Interlocked.Increment(ref qdTInfUsed);   // a non-finite quote passed the unchanged hit test
                // QDIAG (read only): per-side non-finite counts, independent of the exclusive outcome above.
                if (double.IsNaN(bid)) Interlocked.Increment(ref qdTBidNaN);
                else if (double.IsPositiveInfinity(bid)) Interlocked.Increment(ref qdTBidPosInf);
                else if (double.IsNegativeInfinity(bid)) Interlocked.Increment(ref qdTBidNegInf);
                if (double.IsNaN(ask)) Interlocked.Increment(ref qdTAskNaN);
                else if (double.IsPositiveInfinity(ask)) Interlocked.Increment(ref qdTAskPosInf);
                else if (double.IsNegativeInfinity(ask)) Interlocked.Increment(ref qdTAskNegInf);
                rec.BidPrice = hit ? (double?)bid : null;
                rec.AskPrice = hit ? (double?)ask : null;
                double prevForClassify = (prevLivePriceDay.Date == rec.Day.Date) ? prevLivePrice : double.NaN;
                rec.Direction = ClassifyTick(rec.Price, hit ? bid : double.NaN, hit ? ask : double.NaN, prevForClassify,
                                             hit ? bq : -1, hit ? aq : -1, rec.Volume);
                if (rec.Direction == "NEUTRAL") Interlocked.Increment(ref liveNeutralCount);   // DIAG
                string qdIncoming = rec.Direction; string qdBranch = classifyBranch;   // QDIAG: read only
                bool knownDup = IsKnownLiveDuplicate(rec);
                if (!knownDup) lock (liveBranchLock) { DiagCount(liveBranchTally, classifyBranch); }           // DIAG

                // REC28: the first-side candidate is computed here, but lastLiveSide is NOT assigned until the insert succeeds.
                string firstCandidate = null;
                if (lastLiveSide == null && !double.IsNaN(firstTradePrevCloseForNewDay))
                {
                    bool tieLive;
                    firstCandidate = FirstSideFromPrice(rec.Price, firstTradePrevCloseForNewDay, out tieLive);   // null on a tie
                }
                // v35: NEUTRAL is never given a side. candidateSide is for the QDIAG log only and is never written to rec.Direction.
                string inheritSide = lastLiveSide ?? firstCandidate;
                if (qdIncoming == "NEUTRAL" && !knownDup)   // QDIAG: log the first 20 incoming NEUTRAL, and the first 300 quoted NEUTRAL on an independent counter
                {
                    long qn = Interlocked.Increment(ref qdNeutIn);
                    long qq = hit ? Interlocked.Increment(ref qdNeutQuoted) : Interlocked.Read(ref qdNeutQuoted);
                    if (qn <= 20 || (hit && qq <= 300))
                        Log("QDIAG_NEUTRAL | t=" + rec.EventTime.ToString(TsFmt, Inv) + " | p=" + rec.Price.ToString("R", Inv)
                            + " | hit=" + hit + " | bid=" + bid.ToString("R", Inv) + " | ask=" + ask.ToString("R", Inv)
                            + " | branch=" + qdBranch + " | lastLiveSide=" + (lastLiveSide ?? "null") + " | firstCandidate=" + (firstCandidate ?? "null")
                            + " | candidateSide=" + (inheritSide ?? "null") + " | result=" + rec.Direction);
                }
                if (rec.Direction == "NEUTRAL" && !knownDup) Interlocked.Increment(ref qdNeutNoInherit);   // QDIAG: NEUTRAL after inheritance

                Ins res = InsertLive(rec);
                // ROUTER_v30: Inserted and InsertedPartial both mean the row is stored. Everything below advances state that follows the STORED row
                // (counts of stored rows and lots, classification reference); none of it depends on tail, checkpoint, dashboard counters or bus publication.
                if (res == Ins.Inserted || res == Ins.InsertedPartial)
                {
                    Interlocked.Increment(ref liveInserted);
                    lock (totalsLock) { litedbWindowRaw += rec.Volume; liveRaw += rec.Volume; }   // REC34/REC35: native live row joins the retained window; REC45: LIVE_LOTS
                    // REC29: the classification reference advances only on a real insert, like lastLiveSide.
                    prevLivePrice = rec.Price;
                    if (rec.DayVolume > 0) lastKeptDayVolume = rec.DayVolume;   // ROUTER_v08 (D10)
                    prevLivePriceDay = rec.Day;
                    // REC28: state changes only on a real insert. DroppedDup and Failed never reach these lines.
                    if (lastLiveSide == null && firstCandidate != null) lastLiveSide = firstCandidate;
                    if (rec.Direction == "BUY" || rec.Direction == "SELL") lastLiveSide = rec.Direction;
                }
                else if (res == Ins.DroppedDup)
                {
                    PipelineTerminal(rec, P_DUP);
                    Interlocked.Increment(ref liveDup);        // no SeqNum on a dropped duplicate, no state change
                    // ROUTER_v08 (D10): moved = exchange DayVolume advanced past the last kept row, so the key may have dropped a real trade.
                    if (rec.DayVolume <= 0 || lastKeptDayVolume < 0) Interlocked.Increment(ref liveDupDvUnknown);
                    else if (rec.DayVolume > lastKeptDayVolume)
                    {
                        Interlocked.Increment(ref liveDupDvMoved);
                        Interlocked.Add(ref liveDupDvMovedRaw, rec.Volume);
                        if (dupMovedLogN < 50)
                        {
                            dupMovedLogN++;
                            Log("DUP_DV_MOVED | n=" + dupMovedLogN + " | t=" + rec.EventTime.ToString(TsFmt, Inv) + " | p=" + rec.Price.ToString("R", Inv)
                                + " | v=" + rec.Volume + " | o=" + rec.OpenInterest + " | dv=" + rec.DayVolume + " | lastKeptDv=" + lastKeptDayVolume);
                        }
                    }
                    else Interlocked.Increment(ref liveDupDvSame);
                }
                else if (res == Ins.FloorDrop)
                {
                    // ROUTER_v21: counted and logged here, once per rejected row (InsertLive only returns the result).
                    PipelineTerminal(rec, P_FLOOR_LOCKED);
                    Interlocked.Increment(ref liveFloorDropped);
                    Log("BOUNDARY_DROP | path=INSERT_LOCKED | t=" + rec.EventTime.ToString(TsFmt, Inv) + " | p=" + rec.Price.ToString("R", Inv) + " | v=" + rec.Volume);
                }
                else
                {
                    // Ins.Failed is returned only before tickCol.Insert (db or tickCol null, or ROUTER_v30 quarantine overflow). Any other value is a contract violation.
                    if (res == Ins.Failed) PipelineTerminal(rec, P_FAILED_CLEAN); else LedgerContractViolation();
                    FailLive("LIVE_INSERT_FAILED | t=" + rec.EventTime.ToString(TsFmt, Inv) + " | p=" + rec.Price.ToString("R", Inv) + " | v=" + rec.Volume);   // REC27: no state change
                }
            }
            catch (Exception ex)
            {
                LedgerProcessException(rec, rec.Stage);   // P_INSERT_RETURNED is recorded in InsertCore; this only classifies the exception
                if (rec.Stage == 2)
                {
                    // ROUTER_v29: Insert already returned, the row is in LiteDB. This is not a failed insert; it is not added to liveFailedSeq.
                    Interlocked.Increment(ref livePostInsertPartial);
                    Log("LIVE_POST_INSERT_EXC | stored=true | seq=" + rec.SeqNum + " | " + Ex(ex));
                }
                else
                {
                Interlocked.Increment(ref liveFailedSeq);
                Log("LIVE_FAIL | " + Ex(ex));
                }
                Debug("LIVE_FAIL | " + ex.ToString());
            }
        }

        // STD64: the SOT survives a same-session-day re-attach. The live
        // exchange day total is reset ONLY when the session day actually
        // changes (a real roll). A re-attach on the same day keeps the
        // number the exchange has already published, because the exchange
        // has not changed it.
        private void ResetOrPreserveDayVolume(DateTime sessionDay)
        {
            long stamp = Interlocked.Read(ref cachedDayVolumeDayTicks);
            if (stamp == sessionDay.Date.Ticks)
            {
                // Same session day: preserve. The SOT is still valid.
                return;
            }
            // Different day (or no stamp at all): reset. A new session
            // starts with no live day total until the event fires.
            Interlocked.Exchange(ref cachedDayVolume, -1);
            Interlocked.Exchange(ref cachedDayVolumeDayTicks, 0);
        }

        // 09:00 session roll. Caller has set liveReady=false under liveGate, released it, and calls this with liveGate NOT held;
        // DrainAndGoLive() follows. RollSession never touches livePending. SeqNum counter resets, no counter marker is written.
        private void RollSession(DateTime newDay, string why, DateTime eventLocal)
        {
            DateTime prev = currentSessionDay.Date;
            if (newDay.Date <= prev) return;
            Log("SESSION_ROLLOVER | from=" + prev.ToString("yyyy-MM-dd", Inv) + " | to=" + newDay.ToString("yyyy-MM-dd", Inv) + " | why=" + why);
            try { CpFlush(true); }
            catch (Exception rfx)
            {
                try { Print("ROUTER CHECKPOINT_ROLL_FLUSH_FAIL | " + Ex(rfx)); } catch (Exception) { }   // Output window: visible even if the main log fails
                Log("CHECKPOINT_ROLL_FLUSH_FAIL | " + Ex(rfx));
            }
            currentSessionDay = newDay.Date;
            CpOpenDay(newDay.Date);
            ResetSeqForDay();
            lock (keyLock) { keyNin.Clear(); keyHist.Clear(); keyHistPlain.Clear(); ClearKeyQuarantineLocked(); }   // ROUTER_v30: ProcessLive drops other-day ticks, so a held identity of the old day can no longer reach InsertLive
            TailMarkDirty();   // STD06 (item 1): new day, new file
            prevLivePrice = double.NaN;
            prevLivePriceDay = DateTime.MinValue;
            lastKeptDayVolume = -1; dupMovedLogN = 0;   // ROUTER_v08 (D10)
            ResetOrPreserveDayVolume(currentSessionDay);   // STD64
            Log("OI_DAY_SUMMARY | " + OiCountsText());   // REC41 Gate C: counts of the session that just ended
            Interlocked.Exchange(ref cachedOi, -1);
            Interlocked.Exchange(ref oiEvtCount, 0); Interlocked.Exchange(ref oiValidCount, 0);
            Interlocked.Exchange(ref oiZeroFieldCount, 0); Interlocked.Exchange(ref oiUnknownCount, 0);
            Interlocked.Exchange(ref oiLastValidEventTicks, 0); oiStaleLogged = false;
            oiSeeded = false;
            SeedOiFromStore(newDay.Date);   // ROUTER_v16: new session starts from the last valid OI until the first live OI
            ZeroCounters();
            lock (totalsLock) { lateLoginRaw = 0; liveRaw = 0; vecTodayRaw = 0; }   // v06: per-day lot counters follow the roll (window totals and the bus are left alone)
            Interlocked.Exchange(ref liveDroppedBase, Interlocked.Read(ref liveDroppedCount));   // ROUTER_v02 (fix 1): per-session baseline
            Interlocked.Exchange(ref liveFailedBase, Interlocked.Read(ref liveFailedSeq));       // ROUTER_v02 (fix 2)
            CpRewrite(newDay.Date, new List<TickRecord>(), "SESSION_ROLL");
            ComputeFirstSideForNewDay(newDay.Date);   // REC27: companion prevClose for the new day; clears lastLiveSide
            mainRotatePending = true;   // rotated on the first tick (R18)
            Task.Run(() =>
            {
                try
                {
                    lock (exportLock) { ExportTickLog(prev, RouterConfig.LogDir); }   // STD06: same lock as today's export (shared .tmp name)
                    RelocateLogs();   // v40: no prune step
                }
                catch (Exception ex) { Debug("ROLL_FOLLOWUP_FAIL | " + Ex(ex)); }
            });
        }

        // v40: PruneStore removed. No LiteDB row is deleted for age. (DeleteMany in the same-day replacement path is not retention and is unchanged.)

        // =====================================================================
        // exports (STD01: line 1 = MARKET OPENED built at export, line 2 = header, tick rows, last line = MARKET CLOSED at last row once the day is over; same for the minute log) and log placement (R19)
        // =====================================================================
        private const string ExportHeader =
            "EventTime | SeqNum | Phase | Type | Method | Price | Bid | Ask | Volume | Lots | DayVolume | OpenInterest | OIDrift | Classification | Delta | CumDelta";

        private static string ExportRow(DateTime t, string seqNum, string type, double price, long volume, long oi, string cls, long lots, ref long cumDelta,
                                        string phase, string bidStr, string askStr, bool emitType, string oiDriftStr, string dayVolStr)
        {
            long delta = cls == "BUY" ? lots : cls == "SELL" ? -lots : 0;
            cumDelta += delta;
            string deltaStr = delta > 0 ? "+" + delta.ToString(Inv) : delta.ToString(Inv);
            return "EventTime=" + t.ToString("yyyyMMMdd HH:mm:ss.fff", Inv).ToUpperInvariant()
                + " | SeqNum=" + (string.IsNullOrEmpty(seqNum) ? "N/A" : seqNum)
                + " | Phase=" + phase
                + (emitType ? " | Type=" + type : "")
                + " | Method=CAN"
                + " | Price=" + price.ToString("F2", Inv)
                + " | Bid=" + bidStr
                + " | Ask=" + askStr
                + " | Volume=" + volume.ToString(Inv)
                + " | Lots=" + lots.ToString(Inv)
                + " | DayVolume=" + dayVolStr
                + " | OpenInterest=" + (oi < 0 ? "N/A" : oi.ToString(Inv))
                + " | OIDrift=" + oiDriftStr
                + " | Classification=" + cls
                + " | Delta=" + deltaStr
                + " | CumDelta=" + cumDelta.ToString(Inv);
        }

        // R19: tick logs - current day and the 5th day in LogDir, previous 4 days in LOGS. Minute logs only for days older than 5.
        // idx = trading days back from the session day. One function, one place to change.
        private static string LogHomeFor(int idx)
        {
            if (idx <= 0) return RouterConfig.LogDir;
            if (idx >= 1 && idx <= 4) return RouterConfig.PastLogDir;
            if (idx == 5) return RouterConfig.LogDir;
            return RouterConfig.PastLogDir;
        }

        // Shared by the tick log and the minute log: first line = MARKET OPENED (09:15 of the day), last line = MARKET CLOSED (last row's time),
        // written only once the day is over. InstanceId = the instance exporting now.
        // STD01: one builder for both lines. Text is byte-identical to the two originals it replaces.
        // ROUTER_v02 (decision 1): the last-trade time is labelled as the last trade, never as the official close.
        private string ClosedLine(DateTime lastTrade)
        {
            return "MARKET CLOSED | LAST_TRADE_AT " + lastTrade.ToString("yyyyMMMdd HH:mm:ss.fff", Inv).ToUpperInvariant()
                + " | InstanceId=" + (instanceId ?? "N/A");
        }

        private string MarketLine(string label, DateTime t)
        {
            return "MARKET " + label + " AT " + t.ToString("yyyyMMMdd HH:mm:ss.fff", Inv).ToUpperInvariant()
                + " | InstanceId=" + (instanceId ?? "N/A");
        }

        // ROUTER_v02 (decision 1): MARKET CLOSED waits for the natural lifecycle (session roll), never a timer.
        private bool DayIsOver(DateTime day)
        {
            // The day has closed on its own once the session has moved past it (roll). No clock cutoff is used.
            return day.Date < currentSessionDay.Date;
        }

        // ROUTER_v02: one verdict for the dashboard and the EOD log. Balanced only when every check passes and the raw difference is zero.
        private static string ReconState(long dv, long storeVol, long recv, long proc, long dropSession, long failSession,
                                         long barSnap, long lastRecvSnap, double stalledSec)
        {
            if (dv <= 0) return "n/a";
            if (storeVol == 0) return "AWAITING TICKS";
            if (dropSession > 0) return "DROPPED " + dropSession + " TICKS";
            if (failSession > 0) return "STORE FAILED " + failSession + " TICKS";
            long lag = recv - proc;
            if (lag > 0 && stalledSec >= 10) return "WRITER STALLED";
            if (lag > 0) return "WRITER BEHIND " + lag + " TICKS";
            if (barSnap == 0) return "BAR TIME UNAVAILABLE";
            if (barSnap < lastRecvSnap) return "BAR STALE";
            return (dv - storeVol) == 0 ? "Balanced" : "MISMATCH";
        }

        // STD06 (item 1): ONE row writer for the full rewrite and the append, so appended rows are byte-identical to rewritten rows.
        private void WriteTickRowLine(TextWriter sw, TickRecord r, ref double prevPrice, ref long prevOi, ref long cum)
        {
            string cls = DirOrClassify(r, prevPrice);
            bool nin = IsLiveTick(r);
            string bs = r.BidPrice.HasValue ? r.BidPrice.Value.ToString("F2", Inv) : "N/A";
            string asks = r.AskPrice.HasValue ? r.AskPrice.Value.ToString("F2", Inv) : "N/A";
            // First tick row of the file has no previous row: OI change is zero by definition, not unknown.
            string drift = (r.OpenInterest >= 0) ? (prevOi >= 0 ? LotsOf(r.OpenInterest - prevOi) : 0L).ToString(Inv) : "N/A";
            string dayVol = (nin && r.DayVolume >= 0) ? r.DayVolume.ToString(Inv) : "N/A";
            sw.WriteLine(ExportRow(r.EventTime, r.SeqNum, "TICK", r.Price, r.Volume, r.OpenInterest, cls, LotsOf(r.Volume), ref cum,
                                   nin ? "LIVE" : "HIST", bs, asks, false, drift, dayVol));   // REC46: export shows HIST for recovered rows; storage and counters unchanged
            prevPrice = r.Price; prevOi = r.OpenInterest;
        }

        // STD06 (item 1): called by InsertCore (caller holds dbGate) for every stored row of any day.
        private void TailNote(DayCtx ctx, TickRecord r, bool bulk)
        {
            if (!ctx.IsCurrent) return;                       // a past-day replacement never touches today's file
            lock (tailLock)
            {
                if (tailDirty) return;                        // the next export is a full rewrite from LiteDB and includes this row
                if (bulk || (tailHas && r.EventTime < tailTime)) { tailDirty = true; tailPending.Clear(); return; }   // out of order / replacement
                tailPending.Add(r);
                tailTime = r.EventTime; tailHas = true;
                if (tailPending.Count > TailPendingMax) { tailDirty = true; tailPending.Clear(); }
            }
        }

        private void TailMarkDirty() { lock (tailLock) { tailDirty = true; tailPending.Clear(); } }

        // STD06 (item 1): today's tick log. Steady state = append the rows stored since the last export. Anything else = one full rewrite
        // from LiteDB (same output as before). Returns false when nothing was written because of a failure or an empty day.
        private bool ExportTodayTickLog()
        {
            lock (exportLock)
            {
                DateTime day = currentSessionDay.Date;
                string dir = RouterConfig.LogDir;
                string path = Path.Combine(dir, "ROUTER_TICK_" + DayTag(day) + ".log");
                bool over = DayIsOver(day);
                bool exists = File.Exists(path);
                List<TickRecord> add = null;
                bool full;
                lock (tailLock)
                {
                    bool clean = !tailDirty && tailDay == day && tailPath == path && exists;
                    if (clean && over == tailClosedWritten && tailPending.Count == 0) return true;   // nothing new since the last export
                    if (clean && !over && !tailClosedWritten) { add = new List<TickRecord>(tailPending); tailPending.Clear(); full = false; }
                    else full = true;
                }

                if (!full)
                {
                    try
                    {
                        double pp = tailPrevPrice; long po = tailPrevOi, cm = tailCum;
                        using (var fs = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read))
                        using (var sw = new StreamWriter(fs, new System.Text.UTF8Encoding(false)))
                        {
                            foreach (TickRecord r in add) WriteTickRowLine(sw, r, ref pp, ref po, ref cm);
                        }
                        tailPrevPrice = pp; tailPrevOi = po; tailCum = cm;
                        return true;
                    }
                    catch (IOException ioex)
                    {
                        TailMarkDirty();   // the file may hold a partial append: the next export rewrites it
                        Debug("TICKLOG_APPEND_FAILED | file=" + Path.GetFileName(path) + " | " + ioex.Message);
                        return false;
                    }
                    catch (Exception) { TailMarkDirty(); throw; }
                }

                List<TickRecord> rows;
                lock (dbGate)   // snapshot + tail reset are atomic with respect to InsertCore (it holds dbGate)
                {
                    if (tickCol == null) return false;
                    rows = StoreTicksOrdered(day);
                    lock (tailLock)
                    {
                        tailPending.Clear(); tailDirty = (rows.Count == 0); tailDay = day; tailPath = path;
                        tailHas = rows.Count > 0; tailTime = rows.Count > 0 ? rows[rows.Count - 1].EventTime : DateTime.MinValue;
                    }
                }
                if (rows.Count == 0) return false;
                Directory.CreateDirectory(dir);
                string tmp = path + ".tmp";
                try
                {
                    double prevPrice = double.NaN; long prevOi = -1, cum = 0;
                    using (var sw = new StreamWriter(tmp, false, new System.Text.UTF8Encoding(false)))
                    {
                        sw.WriteLine(MarketLine("OPENED", day.Date.Add(MarketOpenTod)));
                        sw.WriteLine(ExportHeader);
                        foreach (TickRecord r in rows) WriteTickRowLine(sw, r, ref prevPrice, ref prevOi, ref cum);
                        if (over) sw.WriteLine(ClosedLine(rows[rows.Count - 1].EventTime));
                    }
                    if (File.Exists(path)) File.Replace(tmp, path, null); else File.Move(tmp, path);
                    tailPrevPrice = prevPrice; tailPrevOi = prevOi; tailCum = cum; tailClosedWritten = over;
                    return true;
                }
                catch (IOException ioex)
                {
                    try { if (File.Exists(tmp)) File.Delete(tmp); } catch { }
                    TailMarkDirty();
                    Debug("TICKLOG_WRITE_FAILED | file=" + Path.GetFileName(path) + " | " + ioex.Message);
                    return false;
                }
                catch (Exception) { TailMarkDirty(); throw; }
            }
        }

        private bool ExportTickLog(DateTime day, string dir)
        {
            List<TickRecord> rows = StoreTicksOrdered(day);
            if (rows.Count == 0) return false;
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, "ROUTER_TICK_" + DayTag(day) + ".log");
            string tmp = path + ".tmp";
            try
            {
                double prevPrice = double.NaN; long prevOi = -1, cum = 0;
                using (var sw = new StreamWriter(tmp, false, new System.Text.UTF8Encoding(false)))
                {
                    sw.WriteLine(MarketLine("OPENED", day.Date.Add(MarketOpenTod)));
                    sw.WriteLine(ExportHeader);
                    foreach (TickRecord r in rows) WriteTickRowLine(sw, r, ref prevPrice, ref prevOi, ref cum);   // STD06: shared row writer
                    if (DayIsOver(day)) sw.WriteLine(ClosedLine(rows[rows.Count - 1].EventTime));
                }
                if (File.Exists(path)) File.Replace(tmp, path, null); else File.Move(tmp, path);
                return true;
            }
            catch (IOException ioex)
            {
                try { if (File.Exists(tmp)) File.Delete(tmp); } catch { }
                Debug("TICKLOG_WRITE_FAILED | file=" + Path.GetFileName(path) + " | " + ioex.Message);
                return false;
            }
        }

        // ROUTER_v02 (decision 3): true only when the minute log was written. A day with no minute rows has nothing to lose: true.
        // ROUTER_v02 (fix 3): true only when the minute log was written. Any exception returns false, so the caller keeps the rows.
        // A day with no minute rows has nothing to lose: true.
        private bool ExportMinuteLog(DateTime day, string dir)
        {
            string tmp = null;
            try
            {
                List<MinuteRecord> rows;
                lock (dbGate) { rows = minuteCol != null ? minuteCol.Find(x => x.Day == day).OrderBy(x => x.EventTime).ToList() : new List<MinuteRecord>(); }
                if (rows.Count == 0) return true;
                Directory.CreateDirectory(dir);
                string path = Path.Combine(dir, "ROUTER_MIN_" + DayTag(day) + ".log");
                tmp = path + ".tmp";   // STD07-AUDIT (item 24): same tmp + replace pattern as the tick log
                long cum = 0;
                using (var sw = new StreamWriter(tmp, false, new System.Text.UTF8Encoding(false)))
                {
                    sw.WriteLine(MarketLine("OPENED", day.Date.Add(MarketOpenTod)));
                    sw.WriteLine(ExportHeader);
                    foreach (MinuteRecord r in rows)
                    {
                        // STD07-AUDIT (item 26): a minute bar has no tick direction. Delta/CumDelta stay 0 for MINUTE rows.
                        string cls = "NEUTRAL";
                        // STD07-AUDIT (item 25): DayVolume is NIN-only; minute rows carry no exchange day volume.
                        sw.WriteLine(ExportRow(r.EventTime, "N/A", "MINUTE", r.Close, r.Volume, r.OpenInterest, cls, LotsOf(r.Volume), ref cum, "HIST", "N/A", "N/A", true, "N/A", "N/A"));
                    }
                    if (DayIsOver(day)) sw.WriteLine(ClosedLine(rows[rows.Count - 1].EventTime));
                }
                if (File.Exists(path)) File.Replace(tmp, path, null); else File.Move(tmp, path);
                tmp = null;
                return true;
            }
            catch (Exception ex)
            {
                try { if (tmp != null && File.Exists(tmp)) File.Delete(tmp); } catch { }
                Debug("MINLOG_WRITE_FAILED | day=" + DayTag(day) + " | " + Ex(ex));
                return false;
            }
        }

        // Session day's tick log, rewritten from LiteDB (LogDir). Called at attach end, every 5 s, at roll and at detach.
        // STD06: every 5 s the call now APPENDS the newly stored rows; the full rewrite happens only when the tail is dirty (see ExportTodayTickLog).
        // ROUTER_v02: true only when the day's tick log is confirmed written, or nothing new was due.
        private bool ExportTodayLogs(bool final)
        {
            if (db == null) return true;   // no store was ever opened: nothing to export
            return ExportTodayTickLog();   // STD06 (item 1): append-only in steady state, full rewrite when dirty
        }


        // Required past exports at attach (R19 + "PAST EXPORTS AND EMPTY DAYS"): generate from LiteDB; an empty day is retried
        private void ExportPast()
        {
            Directory.CreateDirectory(RouterConfig.PastLogDir);
            for (int i = 1; i <= LogCatchUpDays; i++)
            {
                DateTime day = TradingDayBack(currentSessionDay, i);
                string f = "ROUTER_TICK_" + DayTag(day) + ".log";
                if (File.Exists(Path.Combine(RouterConfig.LogDir, f)) || File.Exists(Path.Combine(RouterConfig.PastLogDir, f))) continue;
                if (!HasTicks(day)) { Log("FETCH_SKIPPED | day=" + DayTag(day) + " | reason=no_rows | source=NT_ONLY"); continue; }
                if (ExportTickLog(day, LogHomeFor(i))) Log("PAST_TICKLOG_WRITTEN | file=" + f);
            }
            DateTime minuteBefore = TradingDayBack(currentSessionDay, MinuteLogPastDays);
            List<DateTime> mdays;
            lock (dbGate) { mdays = minuteCol.Query().Where(x => x.Day < minuteBefore).Select(x => x.Day).ToList().Select(d => d.Date).Distinct().OrderBy(x => x).ToList(); }
            int written = 0;
            foreach (DateTime d in mdays)
            {
                string f = "ROUTER_MIN_" + DayTag(d) + ".log";
                if (File.Exists(Path.Combine(RouterConfig.PastLogDir, f))) continue;
                ExportMinuteLog(d, RouterConfig.PastLogDir); written++;
            }
            if (written > 0) Log("PAST_MINLOG_WRITTEN | files=" + written);
            RelocateLogs();
        }

        private static bool TryParseLogDay(string name, string prefix, out DateTime day)
        {
            day = DateTime.MinValue;
            if (!name.EndsWith(".log", StringComparison.OrdinalIgnoreCase) || name.Length <= prefix.Length + 4) return false;
            string tag = name.Substring(prefix.Length, name.Length - prefix.Length - 4);
            return DateTime.TryParseExact(tag, "yyyyMMMdd", Inv, DateTimeStyles.None, out day);
        }

        // Moves existing tick / minute logs to where R19 says they belong. Never overwrites, never deletes.
        private void RelocateLogs()
        {
            try
            {
                int moved = 0;
                foreach (string dir in new[] { RouterConfig.LogDir, RouterConfig.PastLogDir })
                {
                    if (!Directory.Exists(dir)) continue;
                    foreach (string f in Directory.GetFiles(dir, "ROUTER_TICK_*.log", SearchOption.TopDirectoryOnly))
                    {
                        DateTime d; string name = Path.GetFileName(f);
                        if (!TryParseLogDay(name, "ROUTER_TICK_", out d) || d.Date > currentSessionDay.Date) continue;
                        string want = LogHomeFor(d.Date == currentSessionDay.Date ? 0 : TradingDaysBetween(d, currentSessionDay));
                        if (string.Equals(Path.GetFullPath(want).TrimEnd('\\'), Path.GetFullPath(dir).TrimEnd('\\'), StringComparison.OrdinalIgnoreCase)) continue;
                        string dest = Path.Combine(want, name);
                        if (File.Exists(dest)) continue;
                        try { Directory.CreateDirectory(want); File.Move(f, dest); moved++; Log("LOG_RELOCATE | kind=tick | file=" + name + " | to=" + (want == RouterConfig.LogDir ? "LogDir" : "LOGS")); }
                        catch (Exception ex) { Log("LOG_RELOCATE_FAILED | file=" + name + " | " + ex.Message); }
                    }
                }
                if (Directory.Exists(RouterConfig.LogDir))
                    foreach (string f in Directory.GetFiles(RouterConfig.LogDir, "ROUTER_MIN_*.log", SearchOption.TopDirectoryOnly))
                    {
                        string name = Path.GetFileName(f), dest = Path.Combine(RouterConfig.PastLogDir, name);   // minute logs only in LOGS (older than 5 days)
                        if (File.Exists(dest)) continue;
                        try { Directory.CreateDirectory(RouterConfig.PastLogDir); File.Move(f, dest); moved++; Log("LOG_RELOCATE | kind=minute | file=" + name + " | to=LOGS"); }
                        catch (Exception ex) { Log("LOG_RELOCATE_FAILED | file=" + name + " | " + ex.Message); }
                    }
                if (moved > 0) Log("LOG_RELOCATE_DONE | moved=" + moved);
            }
            catch (Exception ex) { Log("LOG_RELOCATE_EX | " + ex.Message); }
        }

        // =====================================================================
        // RECON (R13 / R14) and the 15:45 EOD report. RECON = DAY_VOL - STORE_VOL, exactly 0 to PASS. No tolerance band.
        // =====================================================================
        private void RefreshDayVol()
        {
            RefreshDayVolCore();   // STD45: body unchanged (renamed below); the stamp must run on every exit path, and the body has early returns
            Interlocked.Exchange(ref dashRefreshedAtTicks, DateTime.Now.Ticks);
        }

        // REC21: last DAY_VOL of a session, kept in the router data folder. Written only during a live session; read only for that session day.
        private static readonly string LastDayVolPath = Path.Combine(RouterConfig.RouterDataDir, "ROUTER_LAST_DAYVOL.txt");
        private void SaveLastDayVol(DateTime sessionDay, long raw)
        {
            try
            {
                Directory.CreateDirectory(RouterConfig.RouterDataDir);
                string tmp = LastDayVolPath + ".tmp";
                File.WriteAllText(tmp, sessionDay.ToString("yyyy-MM-dd", Inv) + "|" + raw.ToString(Inv));
                if (File.Exists(LastDayVolPath)) File.Replace(tmp, LastDayVolPath, null);
                else File.Move(tmp, LastDayVolPath);
            }
            catch (Exception ex) { Log("DAYVOL_SAVE_FAIL | " + Ex(ex)); }
        }
        private long ReadLastDayVol(DateTime sessionDay)
        {
            try
            {
                if (!File.Exists(LastDayVolPath)) return -1;
                string[] parts = File.ReadAllText(LastDayVolPath).Trim().Split('|');
                if (parts.Length != 2 || parts[0] != sessionDay.ToString("yyyy-MM-dd", Inv)) return -1;
                long v;
                return long.TryParse(parts[1], out v) && v > 0 ? v : -1;
            }
            catch (Exception ex) { Log("DAYVOL_READ_FAIL | " + Ex(ex)); return -1; }
        }

        private void RefreshDayVolCore()
        {
            long ntBar = Interlocked.Read(ref cachedNtBarVolume);   // STD66
            if (ntBar > 0)
            {
                Interlocked.Exchange(ref dayVolRaw, ntBar);
                if (currentSessionDay.Date == DateTime.Now.Date && IsTradingDate(DateTime.Now.Date)) SaveLastDayVol(currentSessionDay, ntBar);   // REC21
                LogDayVolSource(4, "NT_DAILY_BAR", ntBar);   // STD66: new tag 4, tags 1/2/3 unchanged
                return;
            }
            // REC21: session is a past day (weekend, holiday, or before 09:00 on a trading day): show that session's saved DAY_VOL.
            // Monday 09:00 the session becomes Monday, which has no saved value, so DAY_VOL resets to n/a until Monday's bar.
            if (currentSessionDay.Date != DateTime.Now.Date || !IsTradingDate(DateTime.Now.Date))
            {
                long savedDv = ReadLastDayVol(currentSessionDay);
                if (savedDv > 0)
                {
                    Interlocked.Exchange(ref dayVolRaw, savedDv);
                    LogDayVolSource(5, "SAVED_SESSION", savedDv);   // REC21
                    return;
                }
            }
            if (DayVolPinToNtBar)   // STD73: no fallback while pinned; unknown shows as n/a
            {
                Interlocked.Exchange(ref dayVolRaw, -1);
                Interlocked.Exchange(ref dayVolSourceActive, 0);
                return;
            }
            long cached = Interlocked.Read(ref cachedDayVolume);
            if (cached > 0)
            {
                Interlocked.Exchange(ref dayVolRaw, cached);
                LogDayVolSource(1, "LIVE_DAILY_VOLUME", cached);   // STD60
                return;
            }
            long fallback; lock (dashLock) { fallback = storeMaxDayVol; }
            if (fallback > 0)
            {
                Interlocked.Exchange(ref dayVolRaw, fallback);
                LogDayVolSource(2, "STORE_MAX_DAY_VOL", fallback);   // STD60
                return;
            }
            Interlocked.Exchange(ref dayVolRaw, -1);
            Interlocked.Exchange(ref dayVolSourceActive, 0);   // STD60: none
        }

        // STD60: log DAY_VOL_SOURCE only when the active source changes
        // for this session day. dayVolSourceLogged stays 0 (it is kept
        // for compatibility but no longer gates the line); the change
        // detector is dayVolSourceActive.
        private void LogDayVolSource(int tag, string name, long value)
        {
            if (Interlocked.Exchange(ref dayVolSourceActive, tag) == tag) return;
            Log("DAY_VOL_SOURCE | source=" + name + " | value=" + value);
        }

        // STD66: runs on the chart thread (via TriggerCustomEvent). Caches
        // the daily bar's volume ONLY when that bar is the current session
        // day; otherwise caches -1 so RefreshDayVolCore falls through to
        // the next source. This stops a stale yesterday bar showing after
        // the 09:00 roll or on a quiet start. Never throws.
        private void ReadNtBarVolume()
        {
            try
            {
                if (BarsArray == null || BarsArray.Length < 2) { Interlocked.Exchange(ref cachedNtBarVolume, -1L); return; }   // REC38: same stale-value defect as the catch below
                Bars daily = BarsArray[1];
                if (daily == null || daily.Count < 1) { Interlocked.Exchange(ref cachedNtBarVolume, -1L); return; }
                int idx = daily.Count - 1;
                long v = (long)daily.GetVolume(idx);
                bool sameDay = daily.GetTime(idx).Date == currentSessionDay.Date;
                Interlocked.Exchange(ref cachedNtBarVolume, (sameDay && v > 0) ? v : -1L);
                Interlocked.Exchange(ref ntBarReadUtcTicks, DateTime.UtcNow.Ticks);
            }
            catch (Exception ex)
            {
                // REC38 (FIX 9): a failed read must not leave the previous bar volume eligible for selection in RefreshDayVolCore.
                Interlocked.Exchange(ref cachedNtBarVolume, -1L);
                Debug("NTBAR_READ_EX | " + Ex(ex));
            }
        }

        private static string ReconWord(long recon) { return recon == 0 ? "ACCEPT" : recon > 0 ? "Missing" : "Surplus"; }

        private void CheckEod()
        {
            if (!ownerReady || !startupOk) return;
            DateTime now = DateTime.Now;
            if (!IsTradingDate(now.Date) || now.TimeOfDay < EodWriteTod) return;
            DateTime day = SessionDay(now);
            if (eodWrittenDay == day || currentSessionDay.Date != day) return;
            if (DateTime.UtcNow < eodRetryNotBeforeUtc) return;   // STD29 (M1): backoff after a failed write
            // STD07-AUDIT (item 18): a restart after 15:45 must not rewrite (and .bak-spam) an EOD file that is already on disk,
            // and the flag is set only AFTER a success, so a failed write is retried on the next 1 Hz tick.
            string eodPath = EodCsvPath(day);
            if (File.Exists(eodPath)) { eodWrittenDay = day; Log("EOD_EXISTS_SKIP | day=" + DayTag(day) + " | path=" + eodPath); return; }
            try
            {
                if (WriteEod(day)) { eodWrittenDay = day; eodRetryNotBeforeUtc = DateTime.MinValue; }
                else { eodRetryNotBeforeUtc = DateTime.UtcNow.AddSeconds(30); }
            }
            catch (Exception ex)
            {
                Log("EOD_FAIL | " + Ex(ex));
                eodRetryNotBeforeUtc = DateTime.UtcNow.AddSeconds(30);
            }
        }

        // STD19 (H4): returns false when it bailed without producing a CSV, so CheckEod does not latch eodWrittenDay and the day is retried.
        private bool WriteEod(DateTime day)
        {
            RebuildCounters(day);
            RefreshDayVol();
            long dv = Interlocked.Read(ref dayVolRaw), sv;   // STD07-AUDIT
            long eodRecv, eodProc, eodDrop, eodFail, eodBar, eodLastRecv;
            lock (dashLock)
            {
                sv = storeVolNin;
                eodRecv = Interlocked.Read(ref liveReceivedSeq); eodProc = Interlocked.Read(ref liveProcessedSeq);
                eodDrop = Interlocked.Read(ref liveDroppedCount) - Interlocked.Read(ref liveDroppedBase);
                eodFail = Interlocked.Read(ref liveFailedSeq) - Interlocked.Read(ref liveFailedBase);
                eodBar = Interlocked.Read(ref ntBarReadUtcTicks); eodLastRecv = Interlocked.Read(ref liveRecvUtcTicks);
            }
            string eodState = ReconState(dv, sv, eodRecv, eodProc, eodDrop, eodFail, eodBar, eodLastRecv, 0);   // same verdict as the dashboard
            // ROUTER_v10 (F5): dedupe attribution. Counters are cumulative since indicator load, not per day.
            Log("EOD_DEDUPE | day=" + DayTag(day)
                + " | dup_total=" + Interlocked.Read(ref liveDup)
                + " | dv_same=" + Interlocked.Read(ref liveDupDvSame)
                + " | dv_moved=" + Interlocked.Read(ref liveDupDvMoved)
                + " | dv_moved_raw=" + Interlocked.Read(ref liveDupDvMovedRaw)
                + " | dv_unknown=" + Interlocked.Read(ref liveDupDvUnknown)
                + " | store_gap_raw=" + (dv < 0 ? "n/a" : (dv - sv).ToString(Inv)));
            DateTime eodEvt = day.Date.Add(EodWriteTod);   // live-only: an EOD verdict computed long after 15:45 is not spoken
            if (dv < 0) { Log("EOD_RECON | day=" + DayTag(day) + " | DAY_VOL=n/a | STORE_VOL=" + sv + " | verdict=FAIL (DAY_VOL unknown)"); }
            else
            {
                long reconRaw = dv - sv;                                          // diagnostic log only
                long reconLots = reconRaw / (LotSize > 0 ? LotSize : 1);          // RECON is LOT-BASED
                Log("EOD_RECON | day=" + DayTag(day) + " | DAY_VOL=" + dv + " | STORE_VOL=" + sv + " | RECON_RAW=" + reconRaw + " | RECON_LOTS=" + reconLots + " | " + ReconWord(reconLots)
                    + " | verdict=" + (eodState == "Balanced" ? "PASS" : eodState));
                if (reconLots != 0)
                {
                }
            }

            List<TickRecord> rows = StoreTicksOrdered(day);
            if (rows.Count == 0) { Log("EOD_CSV_SKIPPED | day=" + DayTag(day) + " | no rows"); return false; }
            Directory.CreateDirectory(RouterConfig.EodDir);
            string path = EodCsvPath(day);
            // audit3 (#27): row lots are Volume / LotSize TRUNCATED, so whenever a quantity is not a whole number of lots the
            // per-row sum cannot equal the dashboard aggregate. Name those rows instead of leaving the two numbers to disagree.
            int partialLotRows = LotSize > 0 ? rows.Count(x => x.Volume % LotSize != 0) : 0;
            if (partialLotRows > 0)
                Log("EOD_LOT_ROUNDING | day=" + DayTag(day) + " | rowsWithNonLotMultipleVolume=" + partialLotRows
                    + " | note=per-row Lots is truncated, so the row-lot sum can differ from the dashboard aggregate");
            // STD13 (change 2): the CSV is written even when RECON is not 0; the gap is named in the log (voice was spoken above).
            if (eodState != "Balanced")   // same verdict as EOD_RECON
                Log("EOD_CSV_RESIDUAL_GAP | day=" + DayTag(day) + " | DAY_VOL=" + (dv < 0 ? "n/a" : dv.ToString(Inv)) + " | STORE_VOL=" + sv
                    + " | RECON_LOTS=" + (dv < 0 ? "n/a" : ((dv - sv) / (LotSize > 0 ? LotSize : 1)).ToString(Inv)) + " | RECON_RAW=" + (dv < 0 ? "n/a" : (dv - sv).ToString(Inv))
                    + " | note=CSV written with a known volume gap");
            // STD13 (change 2): one row per tick, same values as the tick log (the same row writer). REC06: the EOD CSV is comma separated (RFC 4180); the .log files stay pipe separated.
            string tmp = path + ".tmp";
            double prevPrice = double.NaN; long prevOi = -1, cum = 0;
            try
            {
                using (var sw = new StreamWriter(tmp, false, new System.Text.UTF8Encoding(false)))
                {
                    sw.WriteLine("EventTime,SeqNum,Phase,Type,Method,Price,Bid,Ask,Volume,Lots,DayVolume,OpenInterest,OIDrift,Classification,Delta,CumDelta");
                    var lineBuf = new StringWriter(Inv);
                    foreach (TickRecord r in rows)
                    {
                        lineBuf.GetStringBuilder().Length = 0;
                        WriteTickRowLine(lineBuf, r, ref prevPrice, ref prevOi, ref cum);
                        sw.WriteLine(CsvFromTickRow(lineBuf.ToString().TrimEnd('\r', '\n')));
                    }
                }
                if (File.Exists(path)) File.Replace(tmp, path, null); else File.Move(tmp, path);
                Log("EOD_CSV_WRITTEN | path=" + path + " | ticks=" + rows.Count);
                return true;
            }
            catch (IOException ioex)
            {
                try { if (File.Exists(tmp)) File.Delete(tmp); } catch { }
                Log("EOD_CSV_REPLACE_FAILED | path=" + path + " | " + ioex.Message);
                return false;
            }
        }

        // STD13 (change 2): yyyyMMdd numeric file name (CSV interchange rule). Folder = the existing CSV_FILES.
        private static string EodCsvPath(DateTime day) { return Path.Combine(RouterConfig.EodDir, "ROUTER_TICK_" + day.ToString("yyyyMMdd", Inv) + ".csv"); }

        // REC06: "Key=Value | Key=Value | ..." tick row -> "Value,Value,..." for the EOD CSV only.
        // Drops every Key=, inserts TICK as column 4 (after Phase; tick rows carry no Type=), and RFC 4180-quotes any field
        // that contains a comma, double quote, CR or LF (inner quotes doubled). No field carries these today; the rule is for safety.
        private static string CsvFromTickRow(string keyValueRow)
        {
            string[] parts = keyValueRow.Split(new[] { " | " }, StringSplitOptions.None);
            var v = new List<string>(parts.Length + 1);
            for (int i = 0; i < parts.Length; i++)
            {
                int eq = parts[i].IndexOf('=');
                string f = eq >= 0 ? parts[i].Substring(eq + 1) : parts[i];
                if (f.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0) f = "\"" + f.Replace("\"", "\"\"") + "\"";
                v.Add(f);
            }
            v.Insert(3, "TICK");
            return string.Join(",", v);
        }

        // =====================================================================
        // dashboard timer (1 Hz)
        // =====================================================================
        private void DashboardTick()
        {
            if (!ownerReady || terminated) return;
            RunStartupRetryIfNeeded();   // STD34 (M3): scheduled startup retry
            if (mainRotatePending && !terminated)
            {
                RotateLogs(currentSessionDay);
                Log("LOG_ROTATED | day=" + DayTag(currentSessionDay) + " | on=1Hz-timer");
            }
            CpFlush(false);
            if (startupOk) CpRepairDirty(true);   // audit5 (#8)
            if (startupOk) RecoverDirtyMarkers();   // ROUTER_v02 (fix C): every outstanding marker, any day
            RefreshDayVol();

            if (startupOk)
            {
                // v37/v39: no clock roll here. The session day changes only on the first live tick of the new trading day (OnMarketDataBody -> RollAsyncOrSkip FIRST_TICK).

                if (db != null && (DateTime.UtcNow - lastLiveExportUtc).TotalMilliseconds >= LiveExportMinMs   // L9 deferred: a wall-clock guard. A Stopwatch is the correct fix (monotonic across NTP steps). Left as-is for a dedicated pass.
                    && Interlocked.CompareExchange(ref liveExportBusy, 1, 0) == 0)
                {
                    // audit3 (#15): the export runs on a POOL THREAD. It used to run inline here while the timer callback held
                    // check, the SAPI transitions, the EOD write and the subscribe retry by the length of the rewrite.
                    // liveExportBusy already prevents overlap, so only the dispatch changes; nothing runs after the launch.
                    lastLiveExportUtc = DateTime.UtcNow;
                    Task.Run(() =>
                    {
                        // STD17 (C2): re-check terminated INSIDE exportLock. A 5 s export that starts after the shutdown tail has
                        // begun is what used to park that tail on exportLock forever. exportLock is re-entrant, so ExportTodayLogs
                        // re-taking it here is the same lock count as before.
                        try
                        {
                            lock (exportLock)
                            {
                                if (terminated) { Debug("LIVE_EXPORT_SKIPPED | terminated"); return; }
                                ExportTodayLogs(false);
                            }
                        }
                        catch (Exception ex) { Debug("LIVE_EXPORT_EX | " + Ex(ex)); }
                        finally { Interlocked.Exchange(ref liveExportBusy, 0); }
                    });
                }
                CheckEod();
                // STD66: the NT daily bar is read on the chart thread only.
                try { TriggerCustomEvent(o => ReadNtBarVolume(), null); }
                catch (Exception tvx) { Debug("NTBAR_QUEUE_EX | " + Ex(tvx)); }
                EmitTickFlow();
            }
            try
            {
                if (ChartControl != null) ChartControl.Dispatcher.InvokeAsync(() => { try { ChartControl.InvalidateVisual(); } catch { } });
            }
            catch { }
        }

        private bool postCloseQuietLogged = false;   // ROUTER_v09 (D15)

        private void EmitTickFlow()
        {
            DateTime nowUtc = DateTime.UtcNow;
            long recv = Interlocked.Read(ref liveReceived), ins = Interlocked.Read(ref liveInserted), dup = Interlocked.Read(ref liveDup);
            if (tickFlowLastUtc == DateTime.MinValue)
            {
                tickFlowLastUtc = nowUtc; tickFlowLastRecv = recv; tickFlowLastIns = ins; tickFlowLastDup = dup;
                tickFlowLastNeut = Interlocked.Read(ref liveNeutralCount);   // REC09: baseline so the first window is per-window
                lock (liveBranchLock) { liveBranchTally.Clear(); }
                return;
            }
            double sec = (nowUtc - tickFlowLastUtc).TotalSeconds;
            if (sec < TickFlowIntervalSec) return;
            // ROUTER_v09 (D15): after the tape close, with no new live ticks, pause the periodic diagnostics.
            // Resumes automatically as soon as a live tick arrives. Logs the pause once.
            if (recv != tickFlowLastRecv) postCloseQuietLogged = false;
            if (DateTime.Now.TimeOfDay >= MarketCloseTod && recv == tickFlowLastRecv)
            {
                tickFlowLastUtc = nowUtc; tickFlowLastRecv = recv; tickFlowLastIns = ins; tickFlowLastDup = dup;
                if (!postCloseQuietLogged) { postCloseQuietLogged = true; Log("POST_CLOSE_QUIET | no new live ticks after " + MarketCloseTod + " | periodic diagnostics paused"); }
                return;
            }
            try { LedgerLine(); } catch (Exception lex) { Debug("LEDGER_LINE_EX | " + Ex(lex)); }
            long neut = Interlocked.Read(ref liveNeutralCount);   // REC09: per-window
            string brTxt;
            lock (liveBranchLock) { brTxt = DiagTally(liveBranchTally); liveBranchTally.Clear(); }   // REC09: atomic snapshot-and-clear
            Log("TICK_FLOW | win=" + (int)Math.Round(sec) + "s | recv=+" + (recv - tickFlowLastRecv) + " | inserted=+" + (ins - tickFlowLastIns)
                + " | dup=+" + (dup - tickFlowLastDup) + " | dropped=" + Interlocked.Read(ref liveDroppedCount) + " | floor=" + Interlocked.Read(ref liveFloorDropped) + " | pub=" + VECTOR.PublishedCount
                + " | wq=" + Interlocked.Read(ref liveQueued)   // STD06: ticks queued for the writer
                + " | dupDv(same/moved/unk)=" + Interlocked.Read(ref liveDupDvSame) + "/" + Interlocked.Read(ref liveDupDvMoved) + "/" + Interlocked.Read(ref liveDupDvUnknown)
                + " | dv=" + Interlocked.Read(ref cachedDayVolume)
                + " | bar=" + Interlocked.Read(ref cachedNtBarVolume)
                + " | lNeut=+" + (neut - tickFlowLastNeut)   // DIAG, per window
                + " | lBr=" + brTxt);
            Log("NEUTRAL_DIAG | classifier=" + Interlocked.Read(ref liveNeutralCount) + " | residual=" + Interlocked.Read(ref qdNeutNoInherit));
            string qdTxt;
            lock (quoteLock)
            {
                qdTxt = "seen B=" + qdBidSeen + " A=" + qdAskSeen
                    + " | guardDrop B=" + qdBidGuardDrop + " A=" + qdAskGuardDrop
                    + " | passed B=" + qdBidPassed + " A=" + qdAskPassed
                    + " | bid valid=" + qdBidValid + " nan=" + qdBidNaN + " +inf=" + qdBidPosInf + " -inf=" + qdBidNegInf + " nonPos=" + qdBidNonPos
                    + " | ask valid=" + qdAskValid + " nan=" + qdAskNaN + " +inf=" + qdAskPosInf + " -inf=" + qdAskNegInf + " nonPos=" + qdAskNonPos
                    + " | crossedEvents(per-event, repeats)=" + qdCrossEvents + " crossedEpisodes(distinct)=" + qdCrossEpisodes
                    + " | lastBidRaw=" + (double.IsNaN(qdLastBidRaw) ? "NA" : qdLastBidRaw.ToString("R", Inv))
                    + " | lastAskRaw=" + (double.IsNaN(qdLastAskRaw) ? "NA" : qdLastAskRaw.ToString("R", Inv));
            }
            Log("QUOTE_DIAG | " + qdTxt
                + " | trade total=" + Interlocked.Read(ref qdTTotal) + " ok=" + Interlocked.Read(ref qdTOk)
                + " noBid=" + Interlocked.Read(ref qdTNoBid) + " noAsk=" + Interlocked.Read(ref qdTNoAsk)
                + " nonFinite=" + Interlocked.Read(ref qdTNonFinite) + " nonPos=" + Interlocked.Read(ref qdTNonPos)
                + " crossed=" + Interlocked.Read(ref qdTCrossed) + " infUsed=" + Interlocked.Read(ref qdTInfUsed)
                + " | bidNonFinite nan=" + Interlocked.Read(ref qdTBidNaN) + " +inf=" + Interlocked.Read(ref qdTBidPosInf) + " -inf=" + Interlocked.Read(ref qdTBidNegInf)
                + " | askNonFinite nan=" + Interlocked.Read(ref qdTAskNaN) + " +inf=" + Interlocked.Read(ref qdTAskPosInf) + " -inf=" + Interlocked.Read(ref qdTAskNegInf)
                + " | neutIn=" + Interlocked.Read(ref qdNeutIn) + " neutQuoted=" + Interlocked.Read(ref qdNeutQuoted) + " neutOut=" + Interlocked.Read(ref qdNeutNoInherit));
            long dgNin;
            lock (dashLock) { dgNin = storeVolNin; }
            long dgNt = Interlocked.Read(ref cachedNtBarVolume), dgDv = Interlocked.Read(ref dayVolRaw);
            Log("NT_NIN | ntBar=" + dgNt + " | nin=" + dgNin + " | dayVolRaw=" + dgDv + " | ntMinusStore=" + (dgNt - dgNin));

            // REC10 diagnostic: read-only store view. No counting or dedupe logic changed.
            DateTime dgDay = currentSessionDay.Date;
            List<TickRecord> dgActive = new List<TickRecord>(), dgLive = new List<TickRecord>();
            lock (dbGate)
            {
                if (tickCol != null)   // no early return: the window reset at the end of EmitTickFlow must still run
                {
                    dgActive = tickCol.Find(x => x.Day == dgDay && !x.Superseded).ToList();
                    dgLive = dgActive.Where(x => x.Phase == "LIVE").OrderBy(x => x.EventTime).ThenBy(x => x.Id).ToList();
                }
            }
            TickRecord dgFirst = dgLive.Count > 0 ? dgLive[0] : null;
            TickRecord dgLast = dgLive.Count > 0 ? dgLive[dgLive.Count - 1] : null;
            long dgLiveVol = dgLive.Sum(x => x.Volume);
            Log("NT_STORE_DRIFT | bar=" + dgNt + " | nin=" + dgNin + " | delta=" + (dgNt - dgNin)
                + " | lastTick=" + (dgLast != null ? dgLast.EventTime.ToString(TsFmt, Inv) : "none")
                + " | storeNinRows=" + dgLive.Count + " | activeStoreRows=" + dgActive.Count);
            Log("STORE_SNAPSHOT | rows=" + dgLive.Count
                + " | first=" + (dgFirst != null ? dgFirst.EventTime.ToString("HH:mm:ss.fff", Inv) : "none")
                + " | last=" + (dgLast != null ? dgLast.EventTime.ToString("HH:mm:ss.fff", Inv) : "none")
                + " | volSum=" + dgLiveVol
                + " | volAvg=" + (dgLive.Count > 0 ? (dgLiveVol / dgLive.Count) : 0));
            tickFlowLastUtc = nowUtc; tickFlowLastRecv = recv; tickFlowLastIns = ins; tickFlowLastDup = dup;
            tickFlowLastNeut = neut;   // REC09
        }

        // =====================================================================
        // rendering (dashboard layout: see BuildDashboardText)
        // =====================================================================

        private string BuildDashboardText()
        {
            string Rule = new string('─', 60);
            Func<string, string> L = s => s.PadRight(8) + " : ";
            var sb = new System.Text.StringBuilder();
            if (autoStopped) sb.AppendLine(L("AUTOSTOP") + (autoStopReason ?? "unknown"));
            long vNin, dv;
            long recvSnap, procSnap, dropSnap, failSnap, barSnap, lastRecvSnap;
            double stalledSec;
            DateTime tickT;
            long vBuy, vSell, vNeut, vUnk;
            lock (dashLock)
            {
                vNin = storeVolNin; tickT = lastTickT;
                vBuy = storeVolBuy; vSell = storeVolSell;
                vNeut = storeVolNeutral; vUnk = storeVolUnknown;
                // ROUTER_v02 (RECON A): counters and store are read together under dashLock, the same lock BumpStore holds.
                dv = Interlocked.Read(ref dayVolRaw);
                recvSnap = Interlocked.Read(ref liveReceivedSeq);
                procSnap = Interlocked.Read(ref liveProcessedSeq);
                dropSnap = Interlocked.Read(ref liveDroppedCount);
                failSnap = Interlocked.Read(ref liveFailedSeq);
                barSnap = Interlocked.Read(ref ntBarReadUtcTicks);
                lastRecvSnap = Interlocked.Read(ref liveRecvUtcTicks);
                if (procSnap != procStallSeen || recvSnap == procSnap) { procStallSeen = procSnap; procStallSinceUtc = DateTime.UtcNow; }
                stalledSec = (DateTime.UtcNow - procStallSinceUtc).TotalSeconds;
            }
            int ls = LotSize > 0 ? LotSize : 1;
            long storeVol = vNin;

            DateTime nowLocal = DateTime.Now;
            bool pastClose = currentSessionDay.Date.Add(MarketCloseTod) < nowLocal;
            string dayState = pastClose ? "market_closed" : (HoldingDay() ? "held" : "LIVE");
            string atTick = tickT > DateTime.MinValue ? " @ " + tickT.ToString("HH:mm:ss", Inv) : "";
            long refreshedTicks = Interlocked.Read(ref dashRefreshedAtTicks);
            string atRefresh = refreshedTicks > 0 ? " @ " + new DateTime(refreshedTicks).ToString("HH:mm:ss", Inv) : "";

            long classVol = vBuy + vSell + vNeut + vUnk;
            string buyPct  = classVol > 0 ? (100.0 * vBuy  / classVol).ToString("0.0", Inv) : "0.0";
            string sellPct = classVol > 0 ? (100.0 * vSell / classVol).ToString("0.0", Inv) : "0.0";
            string neutPct = classVol > 0 ? (100.0 * vNeut / classVol).ToString("0.0", Inv) : "0.0";
            string unkPct  = classVol > 0 ? (100.0 * vUnk  / classVol).ToString("0.0", Inv) : "0.0";

            string strength = vBuy > vSell ? "BUYERS" : vSell > vBuy ? "SELLERS" : "NEUTRAL";

            sb.AppendLine(Rule);
            sb.AppendLine(L("FILENAME") + BannerFull + " coded on " + BannerDate);
            sb.AppendLine(L("DAY") + DayTag(currentSessionDay) + " | " + nowLocal.ToString("HH:mm:ss", Inv) + " | " + dayState);
            sb.AppendLine(Rule);

            long dropSession = dropSnap - Interlocked.Read(ref liveDroppedBase);
            long failSession = failSnap - Interlocked.Read(ref liveFailedBase);
            long reconRaw = dv - storeVol;
            long reconLots = dv <= 0 ? 0 : reconRaw / ls;
            string reconState = ReconState(dv, storeVol, recvSnap, procSnap, dropSession, failSession, barSnap, lastRecvSnap, stalledSec);

            if (dv < 0) sb.AppendLine(L("DAY_VOL") + "n/a raw - n/a Lots" + atTick);
            else sb.AppendLine(L("DAY_VOL") + dv + " raw - " + (dv / ls) + " Lots" + atTick);
            sb.AppendLine(L("QUEUE") + "received " + recvSnap + " | processed " + procSnap + " | dropped " + dropSession + " | failed " + failSession);
            sb.AppendLine(L("FLOOR") + Interlocked.Read(ref liveFloorDropped) + " ticks at history boundary");
            sb.AppendLine(Rule);

            sb.AppendLine(L("BUY") + LotsOf(vBuy).ToString(Inv).PadRight(5) + " - " + buyPct + "%" + (vBuy > vSell ? " ✓" : ""));
            sb.AppendLine(L("SELL") + LotsOf(vSell).ToString(Inv).PadRight(5) + " - " + sellPct + "%" + (vSell > vBuy ? " ✓" : ""));
            sb.AppendLine(L("NEUTRAL") + LotsOf(vNeut).ToString(Inv).PadRight(5) + " - " + neutPct + "%");
            sb.AppendLine(L("UNKNOWN") + LotsOf(vUnk).ToString(Inv).PadRight(5) + " - " + unkPct + "%");
            sb.AppendLine(L("CLASS") + LotsOf(classVol).ToString(Inv));
            sb.AppendLine();

            if (dv <= 0) sb.AppendLine(L("RECON") + "n/a - " + LotsOf(storeVol) + " = n/a | " + reconState + atTick);
            else sb.AppendLine(L("RECON") + (dv / ls) + " - " + LotsOf(storeVol) + " = " + reconLots + " | " + reconState + " | raw " + reconRaw + atTick);
            sb.AppendLine(Rule);

            long snapLitedbWin, snapVecTotal, snapVecToday, snapLate, snapLive;
            lock (totalsLock) { snapLitedbWin = litedbWindowRaw; snapVecTotal = vecTotalRaw; snapVecToday = vecTodayRaw; snapLate = lateLoginRaw; snapLive = liveRaw; }
            sb.AppendLine(L("MISSED") + LotsOf(snapLate).ToString(Inv) + " LOTS");
            sb.AppendLine(L("LIVE") + LotsOf(snapLive).ToString(Inv) + " LOTS");
            sb.AppendLine();
            sb.AppendLine(L("LITEDB") + LotsOf(snapLitedbWin) + " | " + LotsOf(vNin));
            sb.AppendLine(L("VECTOR") + LotsOf(snapVecTotal) + " | " + LotsOf(snapVecToday));
            sb.AppendLine(Rule);

            sb.AppendLine(L("STRENGTH") + strength);

            long dropped = Interlocked.Read(ref liveDroppedCount);
            if (dropped > 0) { sb.AppendLine(Rule); sb.AppendLine(L("DROPPED") + dropped); }
            return sb.ToString();
        }

        // Per-item try/catch inside the loop: one throwing Dispose still lets the remaining SharpDX resources be released, in order.
        private static void DisposeQuiet(params IDisposable[] items)
        {
            foreach (IDisposable d in items) if (d != null) try { d.Dispose(); } catch { }
        }

        private void RenderDuplicateBanner()
        {
            SharpDX.Direct2D1.SolidColorBrush bgBrush = null, borderBrush = null, textBrush = null;
            SharpDX.DirectWrite.TextFormat textFormat = null;
            SharpDX.DirectWrite.TextLayout layout = null;
            try
            {
                if (ChartPanel == null) return;
                float panelW = (float)ChartPanel.W, panelH = (float)ChartPanel.H, bannerH = 80f, bannerY = panelH / 2f - bannerH / 2f;
                bgBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, new SharpDX.Color4(1f, 0f, 0f, 0.85f));
                RenderTarget.FillRectangle(new SharpDX.RectangleF(0f, bannerY, panelW, bannerH), bgBrush);
                borderBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, new SharpDX.Color4(1f, 0f, 0f, 1f));
                RenderTarget.DrawRectangle(new SharpDX.RectangleF(0f, bannerY, panelW, bannerH), borderBrush, 3f);
                textFormat = new SharpDX.DirectWrite.TextFormat(NinjaTrader.Core.Globals.DirectWriteFactory, "Consolas",
                    SharpDX.DirectWrite.FontWeight.Bold, SharpDX.DirectWrite.FontStyle.Normal, 22f);
                textFormat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center;
                textFormat.ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Center;
                textFormat.WordWrapping = SharpDX.DirectWrite.WordWrapping.NoWrap;
                layout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, duplicateWarning, textFormat, panelW, bannerH);
                textBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, new SharpDX.Color4(1f, 1f, 1f, 1f));
                RenderTarget.DrawTextLayout(new SharpDX.Vector2(0f, bannerY), layout, textBrush);
            }
            catch { }
            finally
            {
                DisposeQuiet(textFormat, layout, textBrush, bgBrush, borderBrush);
            }
        }

        private void RenderAutoStopBanner()
        {
            SharpDX.Direct2D1.SolidColorBrush bg = null, border = null, txt = null;
            SharpDX.DirectWrite.TextFormat fmt = null;
            SharpDX.DirectWrite.TextLayout layout = null;
            try
            {
                if (ChartPanel == null) return;
                float w = (float)ChartPanel.W;
                float h = (float)ChartPanel.H;
                float bh = 90f;
                float by = h / 2f - bh / 2f;

                bg = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, new SharpDX.Color4(1f, 0.45f, 0f, 0.9f));
                RenderTarget.FillRectangle(new SharpDX.RectangleF(0f, by, w, bh), bg);
                border = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, new SharpDX.Color4(1f, 0.2f, 0f, 1f));
                RenderTarget.DrawRectangle(new SharpDX.RectangleF(0f, by, w, bh), border, 3f);

                string msg = "ROUTER AUTO-STOPPED : " + (autoStopReason ?? "unknown")
                           + " | " + (instanceId ?? "?")
                           + " | restart the indicator to recover";

                fmt = new SharpDX.DirectWrite.TextFormat(
                    NinjaTrader.Core.Globals.DirectWriteFactory,
                    "Consolas", SharpDX.DirectWrite.FontWeight.Bold,
                    SharpDX.DirectWrite.FontStyle.Normal, 18f);
                fmt.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center;
                fmt.ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Center;
                fmt.WordWrapping = SharpDX.DirectWrite.WordWrapping.NoWrap;

                layout = new SharpDX.DirectWrite.TextLayout(
                    NinjaTrader.Core.Globals.DirectWriteFactory, msg, fmt, w, bh);

                txt = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, new SharpDX.Color4(1f, 1f, 1f, 1f));
                RenderTarget.DrawTextLayout(new SharpDX.Vector2(0f, by), layout, txt);
            }
            catch { }
            finally
            {
                if (fmt != null)    try { fmt.Dispose();    } catch { }
                if (layout != null) try { layout.Dispose(); } catch { }
                if (txt != null)    try { txt.Dispose();    } catch { }
                if (bg != null)     try { bg.Dispose();     } catch { }
                if (border != null) try { border.Dispose(); } catch { }
            }
        }

        protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
        {
            base.OnRender(chartControl, chartScale);
            if (autoStopped) { RenderAutoStopBanner(); return; }
            if (duplicateWarning != null) { RenderDuplicateBanner(); return; }
            if (ChartPanel == null) return;
            string panel = BuildDashboardText();
            SharpDX.DirectWrite.TextFormat textFormat = null;
            SharpDX.DirectWrite.TextLayout layout = null;
            SharpDX.Direct2D1.SolidColorBrush textBrush = null;
            try
            {
                textFormat = new SharpDX.DirectWrite.TextFormat(NinjaTrader.Core.Globals.DirectWriteFactory, "Consolas", 15f);
                textFormat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Leading;
                textFormat.WordWrapping = SharpDX.DirectWrite.WordWrapping.NoWrap;
                layout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, panel, textFormat, 640, 560);
                textBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, new SharpDX.Color4(0.95f, 0.95f, 0.95f, 1f));
                RenderTarget.DrawTextLayout(new SharpDX.Vector2(8, 33), layout, textBrush);
            }
            catch { }
            finally
            {
                DisposeQuiet(textFormat, layout, textBrush);
            }
        }
    }   // end class ROUTER
}       // end namespace NinjaTrader.NinjaScript.Indicators.PRASAN
// ==== end of file ====

