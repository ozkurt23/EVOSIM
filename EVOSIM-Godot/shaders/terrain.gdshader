shader_type spatial;
global uniform float evosim_dry;
global uniform float evosim_rain;
render_mode diffuse_burley;
uniform vec2 region_offset=vec2(0.);
varying vec3 world_pos;
float hash(vec2 p){return fract(sin(dot(p,vec2(127.1,311.7)))*43758.5453);}
float noise(vec2 p){vec2 i=floor(p),f=fract(p);f=f*f*(3.0-2.0*f);return mix(mix(hash(i),hash(i+vec2(1,0)),f.x),mix(hash(i+vec2(0,1)),hash(i+vec2(1,1)),f.x),f.y);}
void vertex(){world_pos=(MODEL_MATRIX*vec4(VERTEX,1.0)).xyz;world_pos.xz+=region_offset;}
void fragment(){
 float broad=noise(world_pos.xz*.13);float patch=noise(world_pos.xz*.9);float grain=noise(world_pos.xz*14.);
 vec3 green=mix(vec3(.043,.095,.016),vec3(.085,.16,.043),smoothstep(.15,.85,broad));
 vec3 earth=vec3(.10,.092,.042);
 green=mix(green,earth,smoothstep(.68,.9,patch)*.3);
 vec3 soil=mix(vec3(.044,.027,.019),vec3(.085,.060,.037),noise(vec2(world_pos.x+world_pos.z,world_pos.y*9.)*4.));
 green=mix(soil,green,smoothstep(.22,.47,world_pos.y));
 green=mix(green,vec3(.17,.115,.049),evosim_dry*.68);
 green*=1.-evosim_rain*.12;
 ALBEDO=green*(.90+grain*.18+patch*.08);ROUGHNESS=mix(.96,.68,evosim_rain);SPECULAR=.08;
}
