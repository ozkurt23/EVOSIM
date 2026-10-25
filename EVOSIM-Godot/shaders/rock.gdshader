shader_type spatial;
global uniform float evosim_dry;
global uniform vec2 evosim_region_offset;
global uniform float evosim_rain;
render_mode diffuse_burley;
varying vec3 world_pos;
void vertex(){world_pos=(MODEL_MATRIX*vec4(VERTEX,1.)).xyz;world_pos.xz+=evosim_region_offset;}
void fragment(){
 float grain=fract(sin(dot(floor(world_pos.xz*18.),vec2(17.12,61.8)))*43758.5453);
 ALBEDO=vec3(.075,.087,.083)*(.88+grain*.20);
 ALBEDO*=1.-evosim_rain*.18;ROUGHNESS=mix(.97,.65,evosim_rain);SPECULAR=.1;
}
