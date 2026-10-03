using EvoSim.Observer;
internal sealed partial class ObserverApp
{
    Rect layerLegend;
    static string LayerName(MapLayer layer) => layer switch { MapLayer.Population => "Canlı yoğunluğu", MapLayer.PlantFood => "Bitkisel besin", MapLayer.Moisture => "Kara nemi", _ => "Kapalı" };
    void CycleMapLayer() { view.Layer = (MapLayer)(((int)view.Layer + 1) % 4); }
    void DrawMapLayerLegend()
    {
        layerLegend = default;
        if (view.Layer == MapLayer.None || mainMenu || setupOpen || libraryOpen || naming || view.LayerData == null) return;
        var data = view.LayerData;
        layerLegend = new Rect(20, 78, 292, 132);
        p.RoundBox(20, 78, 292, 132, 12, new Col(245, 243, 233, 242));
        p.Text(LayerName(view.Layer), 34, 92, 16, text);
        Button("×", new Rect(275, 86, 28, 26), () => view.Layer = MapLayer.None, false, true);
        p.Text(view.Layer == MapLayer.Population ? "8 × 8 bölgede yaşayan birey" : view.Layer == MapLayer.PlantFood ? "8 × 8 bölgede ot + meyve ham enerjisi" : "8 × 8 bölgede kara hücrelerinin ortalaması", 34, 121, 11, muted);
        for (int i = 0; i < 100; i++) p.Box(34 + i * 2.6f, 149, 2.7f, 8, WorldView.LayerColor(view.Layer, i / 99f));
        p.Text("0", 34, 162, 11, muted);
        p.Text(view.Layer == MapLayer.Moisture ? "1 · sabit ölçek" : $"{data.Maximum:0.##} · bu haritadaki en çok", 128, 162, 11, muted);
        p.Text("V · sonraki katman", 34, 186, 11, muted);
    }
}
