extends SceneTree
func _initialize():
	FileAccess.open("res://validation/Godot-LICENSE.txt",FileAccess.WRITE).store_string(Engine.get_license_text())
	var text="Godot third-party licenses\n\n"
	for name in Engine.get_license_info():text+=name+"\n"+Engine.get_license_info()[name]+"\n\n"
	text+="\nCopyright notices\n"+JSON.stringify(Engine.get_copyright_info(),"  ")
	FileAccess.open("res://validation/Godot-THIRD-PARTY.txt",FileAccess.WRITE).store_string(text);quit()
