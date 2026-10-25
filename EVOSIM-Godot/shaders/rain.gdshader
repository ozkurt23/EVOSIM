shader_type spatial;
render_mode unshaded,cull_disabled,shadows_disabled;
global uniform float evosim_rain;
varying float variation;
void vertex(){
 variation=INSTANCE_CUSTOM.g;
 VERTEX.y+=mod(INSTANCE_CUSTOM.r*9.-TIME*(7.+variation*3.),9.);
 VERTEX.x+=VERTEX.y*.12;
}
void fragment(){ALBEDO=vec3(.67,.79,.82);ALPHA=evosim_rain*(.16+variation*.16);}
