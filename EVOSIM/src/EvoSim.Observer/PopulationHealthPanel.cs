using EvoSim.Core;
using EvoSim.Observer;

internal sealed partial class ObserverApp
{
    void DrawPopulationHealth(int x, int y, int width, int height)
    {
        if (populationHealth == null || !ReferenceEquals(populationHealthWorld, world) || populationHealth.Tick != world.Tick)
        { populationHealth = PopulationHealth.Read(world); populationHealthWorld = world; }
        var data = populationHealth; var breeding = data.Breeding; var gate = breeding.ExclusiveFirstBlocker;
        p.Text($"{data.Births} doğum   ·   {data.Deaths} ölüm   ·   {data.Mutations} mutasyon", x + 29, y + 116, 17, green);
        p.Line(x + 28, y + 156, x + width - 28, y + 156, line);
        int left = x + 29, right = x + width / 2 + 20, colWidth = width / 2 - 60;
        p.Text("Kaydedilen ölüm nedenleri", left, y + 178, 17, text);
        p.Text("Üreme · bu andaki ilk engel", right, y + 178, 17, text);
        p.Text("Ölü bireylerin son neden kaydı", left, y + 207, 12, muted);
        p.Text("Her canlı yalnız bir satırda sayılır", right, y + 207, 12, muted);
        int recorded = data.DeathCauses.Values.Sum();
        var causes = new (string Key, string Label)[] { ("susuzluk", "Susuzluk"), ("açlık", "Açlık"), ("avlanma / çatışma", "Avlanma / çatışma"), ("yaşlılık", "Yaşlılık") };
        for (int i = 0; i < causes.Length; i++)
            HealthRow(causes[i].Label, data.DeathCauses.GetValueOrDefault(causes[i].Key), recorded, left, y + 244 + i * 51, colWidth, red);
        int other = data.DeathCauses.Where(pair => !causes.Any(c => c.Key == pair.Key)).Sum(pair => pair.Value);
        HealthRow("Diğer / bilinmiyor", other, recorded, left, y + 448, colWidth, muted);
        p.Text($"Neden kaydı: {recorded} / toplam {data.Deaths} ölüm", left, y + 509, 12, muted);
        p.Text("Bu sayılar birikimlidir; ölüm anında", left, y + 539, 12, muted);
        p.Text("birlikte etkili tüm etkenleri göstermez.", left, y + 558, 12, muted);

        var gates = new (string Label, int Count)[] {
            ("Üreme kapalı", gate.Disabled), ("Henüz yetişkin değil", gate.Immature),
            ("Doğum sonrası bekleme", gate.Cooldown), ("Enerji yetersiz", gate.LowEnergy),
            ("Su yetersiz", gate.LowWater), ("Sağlık yetersiz", gate.LowHealth),
            ("Diğer uygunluk koşulları", gate.OtherIneligible),
            ("Olumlu üreme sinyali yok", gate.NoPositiveMateSignal), ("Hazır ve sinyal veriyor", gate.ReadyWithSignal) };
        for (int i = 0; i < gates.Length; i++)
            HealthRow(gates[i].Label, gates[i].Count, breeding.Living, right, y + 241 + i * 39, colWidth, i == 8 ? green : blue);
        int foot = y + height - 99;
        p.Line(x + 28, foot - 12, x + width - 28, foot - 12, line);
        p.Text($"Temas mesafesinde hazır eşeyli çift: {breeding.NearbyCompatibleReadyPairs}   ·   Hazır eşeysiz birey: {breeding.AsexualReadyWithSignal}", left, foot, 13, green);
        p.Text("Hazır olmak doğum garantisi değildir. İlk engel sırası yukarıdan aşağıya; diğer engeller de birlikte bulunabilir.", left, foot + 27, 12, muted);
        p.Text("Geçmişte yalnız görüntülenen ana kadar olan kayıtlar gösterilir.", left, foot + 54, 12, muted);
        p.Text("Esc · kapat", x + width - 117, foot + 54, 11, muted);
    }

    void HealthRow(string label, int count, int total, int x, int y, int width, Col color)
    {
        p.Text(label, x, y, 13, text);
        p.Text(count.ToString(), x + width - 34, y, 13, color);
        p.RoundBox(x, y + 24, width, 4, 2, line.Alpha(100));
        if (count > 0 && total > 0) p.RoundBox(x, y + 24, Math.Max(2, width * count / (float)total), 4, 2, color.Alpha(180));
    }
}
