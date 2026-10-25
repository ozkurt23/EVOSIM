extends SceneTree
const Sampler=preload("res://scripts/decoration_sampler.gd")
func _initialize():
	var checks={}
	for kind in [[17.,1],[1.18,2],[1.89,3]]:
		var first=Sampler.sample(Vector2(42,20),48,kind[0],kind[1])
		var second=Sampler.sample(Vector2(48,24),48,kind[0],kind[1])
		var common_a={};var common_b={}
		for p in first:
			if p.x>49 and p.y>25 and p.x<89 and p.y<67: common_a[p]=true
		for p in second:
			if p.x>49 and p.y>25 and p.x<89 and p.y<67: common_b[p]=true
		checks[str(kind[1])+"_overlap"]=common_a.size()>100 and common_a==common_b
		checks[str(kind[1])+"_repeat"]=first==Sampler.sample(Vector2(42,20),48,kind[0],kind[1])
	var ok=checks.values().all(func(v): return v)
	FileAccess.open("res://validation/decoration-checks-28.json",FileAccess.WRITE).store_string(JSON.stringify({"passed":ok,"checks":checks},"  "))
	print(checks);quit(0 if ok else 1)
