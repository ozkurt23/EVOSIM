using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using EvoSim.Core;
using EvoSim.Observer;
using Microsoft.VisualBasic.FileIO;

internal sealed partial class ObserverApp
{
    void ExportSmoke()
    {
        void Check(bool condition, string why) { if (!condition) throw new Exception("Export smoke: " + why); }
        byte[] Snapshot(World w) { using var stream = new MemoryStream(); w.Save(stream); return stream.ToArray(); }
        string dir = Path.Combine(Path.GetFullPath(Arg("--out") ?? "artifacts/ui-smoke"), "export-checks");
        var sample = new World(42); var session = TimelineSession.Create(sample);
        for (int i = 0; i < 1800; i++) session.AdvancePlaybackOneTick();
        Check(sample.Deaths > 0 && sample.Births > 0, "real lifecycle available");
        byte[] before = Snapshot(sample); var previousCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            string target = ObservationExporter.Write(sample, dir, session.HeadTick, session.EarliestTick, "fixture-id", "Çınar, gözlem");
            Check(before.SequenceEqual(Snapshot(sample)), "full checkpoint and RNG unchanged");
            using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(target, "report.json")));
            var report = document.RootElement;
            Check(report.GetProperty("schema").GetString() == "evosim.observation.v2", "explicit new schema");
            Check(report.GetProperty("sourceWorldId").GetString() == "fixture-id" && report.GetProperty("worldName").GetString() == "Çınar, gözlem", "world identity metadata");
            Check(report.GetProperty("creatures").GetArrayLength() == sample.Creatures.Count, "living and dead included");
            foreach (var node in report.GetProperty("creatures").EnumerateArray())
            {
                var c = sample.Find(node.GetProperty("Id").GetInt32())!;
                Check(node.GetProperty("ParentA").GetInt32() == c.ParentA && node.GetProperty("ParentB").GetInt32() == c.ParentB, "actual genealogy");
                Check(c.Alive ? node.GetProperty("deathTime").ValueKind == JsonValueKind.Null : node.GetProperty("deathCause").GetString() == c.DeathCause, "death nulls and real reasons");
            }
            using (var hashes = JsonDocument.Parse(File.ReadAllText(Path.Combine(target, "integrity.json"))))
                foreach (var entry in hashes.RootElement.EnumerateObject())
                    Check(entry.Value.GetString() == Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(Path.Combine(target, entry.Name)))), "payload hash");
            string again = ObservationExporter.Write(sample, dir, session.HeadTick, session.EarliestTick);
            Check(target != again && Directory.Exists(target), "same tick exports never overwrite");

            var dead = sample.Creatures.First(c => !c.Alive); string originalCause = dead.DeathCause;
            dead.DeathCause = "ölüm, \"örnek\"\nyeni satır";
            try
            {
                string quoted = ObservationExporter.Write(sample, dir, session.HeadTick, session.EarliestTick);
                using var csv = new TextFieldParser(Path.Combine(quoted, "creatures.csv")); csv.SetDelimiters(","); csv.HasFieldsEnclosedInQuotes = true;
                var header = csv.ReadFields()!; int rows = 0; bool found = false;
                while (!csv.EndOfData)
                {
                    var row = csv.ReadFields()!; rows++; Check(row.Length == header.Length, "CSV column count including embedded newline");
                    if (int.Parse(row[1], CultureInfo.InvariantCulture) == dead.Id) { found = true; Check(row[16] == dead.DeathCause, "UTF8 quotes comma newline roundtrip"); }
                    double.Parse(row[8], CultureInfo.InvariantCulture);
                    if (row[7] == "true") Check(row[15] == "" && row[16] == "", "living CSV death cells empty");
                }
                Check(found && rows == sample.Creatures.Count, "all creature CSV rows");
            }
            finally { dead.DeathCause = originalCause; }

            int completeBefore = Directory.GetDirectories(dir).Length;
            float gene = sample.Creatures[0].Genes[0]; sample.Creatures[0].Genes[0] = float.NaN;
            bool rejected = false;
            try { ObservationExporter.Write(sample, dir, session.HeadTick, session.EarliestTick); }
            catch (ArgumentException) { rejected = true; }
            finally { sample.Creatures[0].Genes[0] = gene; }
            Check(rejected && Directory.GetDirectories(dir).Length == completeBefore && !Directory.GetDirectories(dir, "*.partial").Any(), "failed serialization publishes nothing and cleans staging");
            Check(before.SequenceEqual(Snapshot(sample)), "test fixtures restored");

            session.BeginSeek(0); while (session.IsSeeking) session.PumpSeek(20, 100);
            var past = session.DisplayWorld;
            string historical = ObservationExporter.Write(past, dir, session.HeadTick, session.EarliestTick);
            using var pastDocument = JsonDocument.Parse(File.ReadAllText(Path.Combine(historical, "report.json")));
            var p = pastDocument.RootElement;
            Check(p.GetProperty("viewTick").GetInt64() == 0 && p.GetProperty("headTick").GetInt64() == 1800 && p.GetProperty("historyPreview").GetBoolean(), "past provenance");
            Check(p.GetProperty("deaths").GetInt32() == 0 && p.GetProperty("births").GetInt32() == 0 && p.GetProperty("creatures").GetArrayLength() == 165, "no future creature states");
            Check(before.SequenceEqual(Snapshot(session.HeadWorld)), "export and seek retain head state");
        }
        finally { CultureInfo.CurrentCulture = previousCulture; }
    }
}
