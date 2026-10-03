using EvoSim.Core;
using EvoSim.Observer;

internal sealed partial class ObserverApp
{
    bool populationInsightsOpen, populationHealthTab;
    Rect populationGenesTabRect, populationHealthTabRect;
    PopulationHealth? populationHealth;
    World? populationHealthWorld;
    readonly PopulationInsightsCache populationInsightsCache = new();
    Rect populationInsightsRect, populationInsightsClose;

    void ShowPopulationInsights()
    {
        if (mainMenu || !hasActiveWorld) return;
        PrepareObservationModal();
        populationInsightsOpen = true;
        paused = true;
        accumulator = 0;
        populationInsightsCache.Invalidate();
        populationHealthTab = false;
        populationHealth = null; populationHealthWorld = null;
    }

    void ClosePopulationInsights()
    {
        populationInsightsOpen = false;
        populationInsightsRect = populationInsightsClose = default;
        accumulator = 0;
    }

    bool HandlePopulationInsightsEvent(Event ev)
    {
        if (!populationInsightsOpen) return false;
        if (ev.Type == 0x300)
        {
            if (ev.B == 27 || ev.B == 120) ClosePopulationInsights();
            return true;
        }
        if (ev.Type == 0x400 || ev.Type == 0x401 || ev.Type == 0x402)
        {
            mouseX = ev.B; mouseY = ev.C;
            if (ev.Type == 0x401 && ev.Button == 1 && populationInsightsClose.Contains(mouseX, mouseY))
                ClosePopulationInsights();
            else if (ev.Type == 0x401 && ev.Button == 1 && populationGenesTabRect.Contains(mouseX, mouseY)) populationHealthTab = false;
            else if (ev.Type == 0x401 && ev.Button == 1 && populationHealthTabRect.Contains(mouseX, mouseY)) populationHealthTab = true;
            return true;
        }
        // Window/quit events remain available to the host; interaction underneath
        // this modal cannot step, edit, select, pan or zoom the displayed world.
        return ev.Type >= 0x300 && ev.Type <= 0x304 || ev.Type == 0x403;
    }

    void DrawPopulationInsightsPanel()
    {
        if (!populationInsightsOpen) return;
        var data = populationInsightsCache.Get(world);
        buttons.Clear();
        p.Box(0, 0, p.W, p.H, new Col(29, 49, 40, 150));
        int width = Math.Min(1040, p.W - 64), height = Math.Min(760, p.H - 64);
        int x = (p.W - width) / 2, y = (p.H - height) / 2;
        populationInsightsRect = new Rect(x, y, width, height);
        p.RoundBox(x + 3, y + 7, width, height, 18, new Col(22, 43, 31, 40));
        p.RoundBox(x, y, width, height, 18, Col.Hex(0xF5F2E9));
        p.Text("Nüfus gözlemi", x + 28, y + 25, 25, text);
        bool historical = timeline?.IsPreviewingHistory == true;
        p.Text(Clock(data.Tick * World.Dt) + (historical ? " · geçmiş" : " · şu an"), x + width - 238, y + 37, 12, muted);
        p.Text($"Görüntülenen anda {data.Living} canlı · gözlem duraklatıldı", x + 29, y + 67, 13, muted);
        populationInsightsClose = new Rect(x + width - 49, y + 19, 31, 31);
        Button("×", populationInsightsClose, ClosePopulationInsights, false, true);

        populationGenesTabRect = new Rect(x + width - 233, y + 62, 89, 30);
        populationHealthTabRect = new Rect(x + width - 135, y + 62, 106, 30);
        Button("Genler", populationGenesTabRect, () => populationHealthTab = false, !populationHealthTab, true);
        Button("Yaşam", populationHealthTabRect, () => populationHealthTab = true, populationHealthTab, true);
        if (populationHealthTab) { DrawPopulationHealth(x, y, width, height); return; }

        DrawPopulationDietSummary(data.Herbivorous, "Otçul ağırlıklı", x + 29, y + 105, blue);
        DrawPopulationDietSummary(data.Carnivorous, "Etçil ağırlıklı", x + width / 2 + 12, y + 105, red);
        p.Line(x + 28, y + 158, x + width - 28, y + 158, line);

        float chartX = x + 196, chartWidth = width - 526, statsX = x + width - 281;
        p.Text("Kalıtsal özellik", x + 29, y + 176, 11, muted);
        p.Text("Grup içindeki pay · her çubukta %0–100", chartX, y + 176, 11, muted);
        p.Text("Ortanca · en az–en çok", statsX, y + 176, 11, muted);
        int rowHeight = 71;
        for (var gene = 0; gene < Genetics.Count; gene++)
        {
            float rowY = y + 204 + gene * rowHeight;
            p.Text(Genetics.Names[gene], x + 29, rowY + 15, 14, text);
            var herb = data.Herbivorous.Genes[gene];
            var carn = data.Carnivorous.Genes[gene];
            float bottom = rowY + 49, step = chartWidth / PopulationInsights.BinCount;
            p.Line(chartX, bottom, chartX + chartWidth, bottom, line);
            for (var bin = 0; bin < PopulationInsights.BinCount; bin++)
            {
                float bx = chartX + bin * step + 3, barWidth = (step - 8) * .5f;
                float herbHeight = herb.Count == 0 ? 0 : 42f * herb.Bins[bin] / herb.Count;
                float carnHeight = carn.Count == 0 ? 0 : 42f * carn.Bins[bin] / carn.Count;
                if (herbHeight > 0) p.RoundBox(bx, bottom - herbHeight, barWidth, herbHeight, 2, blue.Alpha(220));
                if (carnHeight > 0) p.RoundBox(bx + barWidth + 2, bottom - carnHeight, barWidth, carnHeight, 2, red.Alpha(210));
            }
            p.Disk(statsX + 3, rowY + 15, 2, blue);
            p.Text(DistributionText(herb), statsX + 13, rowY + 7, 12, herb.Count == 0 ? muted : text);
            p.Disk(statsX + 3, rowY + 38, 2, red);
            p.Text(DistributionText(carn), statsX + 13, rowY + 30, 12, carn.Count == 0 ? muted : text);
            if (gene < Genetics.Count - 1) p.Line(x + 28, rowY + 63, x + width - 28, rowY + 63, line.Alpha(105));
        }
        float axisY = y + 204 + 5 * rowHeight + 56;
        p.Text("0", chartX, axisY, 10, muted);
        p.Text("0,5", chartX + chartWidth / 2 - 8, axisY, 10, muted);
        p.Text("1", chartX + chartWidth - 5, axisY, 10, muted);
        p.Text("Gen değeri", x + 29, axisY, 10, muted);

        int foot = y + height - 109;
        p.Line(x + 28, foot - 13, x + width - 28, foot - 13, line);
        p.Text("Ayrım güncel beslenme genine göre: 0,5 altı otçul, 0,5 ve üzeri etçil ağırlıklı.", x + 29, foot + 1, 12, muted);
        p.Text("Yüzücü: su uyumu ≥ 0,85. Yaşlanma sayısı dünyanın etkin yaşlanma kuralını kullanır.", x + 29, foot + 26, 12, muted);
        p.Text(data.Living == 0 ? "Bu anda yaşayan birey yok; dağılımlar boş." : "Bu dağılım tek başına öğrenme veya neden–sonuç göstermez.",
            x + 29, foot + 59, 12, data.Living == 0 ? red : muted);
        p.Text("Esc · kapat", x + width - 117, foot + 59, 11, muted);
    }

    void DrawPopulationDietSummary(PopulationDietInsights group, string name, float x, float y, Col color)
    {
        p.Disk(x + 4, y + 11, 4, color);
        p.Text($"{name} · {group.Count}", x + 16, y, 16, color);
        p.Text($"{group.Swimmers} yüzücü   ·   {group.Mature} yetişkin   ·   {group.Aging} yaşlanıyor", x + 16, y + 27, 12, muted);
    }

    static string DistributionText(GeneDistribution distribution) => distribution.Count == 0 ? "—" :
        $"{distribution.Median:0.00}  ·  {distribution.Minimum:0.00}–{distribution.Maximum:0.00}";
}
