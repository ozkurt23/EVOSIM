shader_type spatial;
global uniform float evosim_dry;
global uniform float evosim_rain;
render_mode diffuse_burley;
uniform sampler2D shore_mask:filter_linear,repeat_disable;
uniform vec2 region_offset=vec2(0.);
uniform float extent=48.;
varying vec3 world_pos;
float hash(vec2 p){return fract(sin(dot(p,vec2(127.1,311.7)))*43758.5453);}
float noise(vec2 p){vec2 i=floor(p),f=fract(p);f=f*f*(3.-2.*f);return mix(mix(hash(i),hash(i+vec2(1,0)),f.x),mix(hash(i+vec2(0,1)),hash(i+vec2(1,1)),f.x),f.y);}
void vertex(){world_pos=(MODEL_MATRIX*vec4(VERTEX,1)).xyz;VERTEX.y+=sin(world_pos.x*.8+world_pos.z*.6+TIME*.7)*.008;}
void fragment(){
 float edge=texture(shore_mask,world_pos.xz/extent).r;
 vec2 flow=world_pos.xz+region_offset+vec2(TIME*.055,TIME*.13);
 float n=noise(flow*.55)*.6+noise(flow*1.8)*.28+noise(flow*4.5)*.12;
 float rip=sin(flow.x*3.7+flow.y*1.8+noise(flow*.7)*5.);
 float broken=smoothstep(.62,.85,noise(flow*vec2(.8,5.)));
 float glint=smoothstep(.94,1.,rip)*broken*.05;
 vec3 deep=vec3(.015,.063,.11),shallow=vec3(.072,.17,.15);
 ALBEDO=mix(deep,shallow,edge*.75)*( .80+n*.50)+vec3(glint*.7,glint*.9,glint);
 float foam=smoothstep(.25,.58,edge)*(1.-smoothstep(.75,.98,edge))*smoothstep(.45,.72,noise(flow*3.));
 ALBEDO+=vec3(.055,.064,.060)*foam;
 vec2 cell=fract(world_pos.xz*.8)-.5;float ring=fract(TIME*1.4+hash(floor(world_pos.xz*.8)));
 float ripple=(1.-smoothstep(.015,.05,abs(length(cell)-ring*.48)))*(1.-ring)*evosim_rain;
 ALBEDO+=vec3(.012,.020,.025)*ripple*step(.6,hash(floor(world_pos.xz*.8)+vec2(15.,22.)));
 ALBEDO=mix(ALBEDO,ALBEDO*vec3(1.05,.83,.66),evosim_dry*.35);
 NORMAL=normalize(NORMAL+vec3(rip*.045,0.,sin(flow.y*3.4+n*8.)*.035));
 ROUGHNESS=.34;SPECULAR=.35;
}
