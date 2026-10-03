using EvoSim.Observer;
internal sealed partial class ObserverApp
{
    bool lifeTraceOpen;BackgroundLifeTrace? lifeTrace;Rect lifeTraceRect,lifeTraceClose,lifeTraceChart;LifeTracePoint? lifeTraceHovered;
    void CloseLifeTrace(){lifeTraceOpen=false;lifeTrace?.Cancel();lifeTrace=null;lifeTraceChart=default;lifeTraceHovered=null;accumulator=0;}
    void ShowLifeTrace(int id)
    {
        if(mainMenu||!hasActiveWorld||world.Find(id)==null)return;
        long until=world.Tick;var source=EnsureTimeline();if(source.IsSeeking)return;
        PrepareObservationModal();lifeTraceOpen=true;
        try{lifeTrace=new BackgroundLifeTrace(source,id,until);}
        catch(Exception ex){CloseLifeTrace();Say("Yaşam izi açılamadı: "+ex.Message);}
    }
    void PumpLifeTrace(){if(lifeTraceOpen)lifeTrace?.Pump();}
    LifeTracePoint? TracePointAt(int px,int py)
    {
        if(lifeTrace is not {Complete:true} job||!lifeTraceChart.Contains(px,py)||job.Points.Count==0)return null;
        double tick=job.FromTick+(px-lifeTraceChart.X)/(double)Math.Max(1,lifeTraceChart.W-1)*(job.ToTick-job.FromTick);
        return job.Points.MinBy(a=>Math.Abs(a.Tick-tick));
    }
    bool HandleLifeTraceEvent(Event ev)
    {
        if(!lifeTraceOpen)return false;
        if(ev.Type==0x300){if(ev.B==27)CloseLifeTrace();return true;}
        if(ev.Type is 0x400 or 0x401 or 0x402){mouseX=ev.B;mouseY=ev.C;if(ev.Type==0x401&&ev.Button==1){if(lifeTraceClose.Contains(mouseX,mouseY))CloseLifeTrace();else if(TracePointAt(mouseX,mouseY) is LifeTracePoint point){long target=point.Tick;ShowTimeline();RequestTimelineSeek(target);}}return true;}
        return ev.Type>=0x300&&ev.Type<=0x304||ev.Type==0x403;
    }
    void DrawLifeTracePanel()
    {
        if(!lifeTraceOpen||lifeTrace==null)return;buttons.Clear();lifeTraceChart=default;lifeTraceHovered=null;var job=lifeTrace;
        int w=Math.Min(1040,p.W-64),h=680,x=(p.W-w)/2,y=(p.H-h)/2;lifeTraceRect=new Rect(x,y,w,h);
        p.Box(0,0,p.W,p.H,new Col(29,49,40,150));p.RoundBox(x,y,w,h,18,bg);
        p.Text($"#{job.CreatureId} · Yaşam izi",x+28,y+25,25,text);
        p.Text($"{Clock(job.FromTick*.05)} – {Clock(job.ToTick*.05)} · görüntülenen ana kadar",x+29,y+66,13,muted);
        lifeTraceClose=new Rect(x+w-132,y+23,104,32);Button(job.Complete?"Kapat":"Vazgeç",lifeTraceClose,CloseLifeTrace,false,true);
        if(job.Error!=null){Wrap("İz oluşturulamadı: "+job.Error,x+29,y+135,15,red,90,5);return;}
        if(!job.Complete)
        {
            p.Text(job.Preparing?"Kayıtlı geçmiş hazırlanıyor…":$"Yaşam inceleniyor · %{job.Fraction*100:0}",x+29,y+142,18,green);
            p.RoundBox(x+29,y+188,w-58,6,3,line);p.RoundBox(x+29,y+188,Math.Max(3,(w-58)*(float)job.Fraction),6,3,green);
            p.Text("Bu işlem ayrı bir dünya kopyasında çalışıyor. Esc ile iptal edebilirsin.",x+29,y+226,13,muted);return;
        }
        var samples=job.Points.Where(a=>a.Exists).ToList();
        if(samples.Count==0){p.Text("Bu aralıkta birey henüz mevcut değil.",x+29,y+145,17,muted);return;}
        var death=samples.FirstOrDefault(a=>!a.Alive);if(death!=null)samples=samples.Where(a=>a.Tick<=death.Tick).ToList();
        p.Text("Enerji / kapasite",x+29,y+112,13,green);p.Text("Su",x+180,y+112,13,blue);p.Text("Sağlık",x+231,y+112,13,red);
        float gx=x+62,gy=y+160,gw=w-420,gh=300;
        lifeTraceChart=new Rect((int)gx,(int)gy,(int)gw,(int)gh);lifeTraceHovered=TracePointAt(mouseX,mouseY);
        for(int i=0;i<=4;i++){float yy=gy+gh-i*gh/4;p.Line(gx,yy,gx+gw,yy,line);p.Text((i*25).ToString(),gx-32,yy-7,11,muted);}
        float PX(long tick)=>gx+(float)((tick-job.FromTick)/(double)Math.Max(1,job.ToTick-job.FromTick))*gw;
        float PY(float value)=>gy+gh-gh*Math.Clamp(value/100,0,1);
        for(int metric=0;metric<3;metric++)
        {
            Col color=metric==0?green:metric==1?blue:red;LifeTracePoint? prior=null;
            foreach(var a in samples)
            {
                float Value(LifeTracePoint point)=>metric==0?100*(point.Energy??0)/Math.Max(.001f,point.MaxEnergy??1):metric==1?point.Water??0:point.Health??0;
                if(prior!=null){p.Line(PX(prior.Tick),PY(Value(prior)),PX(a.Tick),PY(Value(a)),color);p.Line(PX(prior.Tick),PY(Value(prior))+1,PX(a.Tick),PY(Value(a))+1,color);}
                else p.Disk(PX(a.Tick),PY(Value(a)),3,color);
                prior=a;
            }
        }
        if(lifeTraceHovered is LifeTracePoint hover){
            float xx=PX(hover.Tick);p.Line(xx,gy,xx,gy+gh,muted.Alpha(150));
            float labelX=Math.Clamp(xx-90,gx,gx+gw-196);p.RoundBox(labelX,gy-28,196,23,5,panel);p.Text(WorldCalendar.Duration(hover.Tick*.05,true),labelX+9,gy-24,11,text);
            if(hover.Exists){p.Disk(xx,PY(100*(hover.Energy??0)/Math.Max(.001f,hover.MaxEnergy??1)),3,green);p.Disk(xx,PY(hover.Water??0),3,blue);p.Disk(xx,PY(hover.Health??0),3,red);}
        }
        p.Text(Clock(job.FromTick*.05),gx,gy+gh+14,11,muted);p.Text(Clock(job.ToTick*.05),gx+gw-132,gy+gh+14,11,muted);
        float rx=x+w-304,ry=y+160,rw=260,rh=300;
        p.Text("Örneklenen konumlar",rx,y+112,14,text);p.RoundBox(rx,ry,rw,rh,10,panel);
        float minX=samples.Min(a=>a.X!.Value),maxX=samples.Max(a=>a.X!.Value),minY=samples.Min(a=>a.Y!.Value),maxY=samples.Max(a=>a.Y!.Value);
        float scale=Math.Min((rw-32)/Math.Max(1,maxX-minX),(rh-32)/Math.Max(1,maxY-minY));
        Pt Position(LifeTracePoint a)=>new(rx+rw/2+(a.X!.Value-(minX+maxX)/2)*scale,ry+rh/2+(a.Y!.Value-(minY+maxY)/2)*scale);
        for(int i=1;i<samples.Count;i++){var a=Position(samples[i-1]);var b=Position(samples[i]);p.Line(a.X,a.Y,b.X,b.Y,green);}
        var first=Position(samples[0]);var last=Position(samples[^1]);p.Disk(first.X,first.Y,4,blue);p.Disk(last.X,last.Y,4,red);
        if(lifeTraceHovered is {Exists:true} routePoint){var at=Position(routePoint);p.Ring(at.X,at.Y,7,blue);}
        p.Text("Mavi: ilk · kırmızı: son konum",rx,ry+rh+14,11,muted);
        p.Line(x+28,y+520,x+w-28,y+520,line);
        string status=death!=null?$"Ölüm: {Clock(death.DeathTime??0)} · {death.DeathCause}":"Görüntülenen son anda yaşıyor.";
        if(lifeTraceHovered is LifeTracePoint detail){
            status=WorldCalendar.Duration(detail.Tick*.05,true)+" · "+(!detail.Exists?"Henüz mevcut değil":$"{(detail.Alive?"Yaşıyor":"Ölü · son beden durumu")} · Enerji %{100*(detail.Energy??0)/Math.Max(.001f,detail.MaxEnergy??1):0.0} · Su {detail.Water:0.0} · Sağlık {detail.Health:0.0}");
        }
        p.Text(status,x+29,y+541,14,text);
        p.Text($"{job.Points.Count} örnek · yaklaşık 1 gün arayla; doğum ve ölüm geçişleri ayrıca kaydedilir.",x+29,y+573,12,muted);
        p.Text("En fazla son 3 yıl 105 gün. Çizgiler örnek noktaları birleştirir; tam güzergâh veya niyet değildir.",x+29,y+600,12,muted);
        p.Text("Grafikte gez: değerleri incele · Grafiğe tıkla: kayıtlı ana git · Esc: kapat",x+29,y+627,12,muted);
    }
}
