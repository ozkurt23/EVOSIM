shader_type spatial;
global uniform float evosim_dry;
global uniform vec2 evosim_region_offset;
global uniform float evosim_rain;
render_mode diffuse_burley,cull_disabled;
uniform vec4 base_color:source_color=vec4(.3,.46,.16,1);
uniform float strength=.065;
uniform bool use_growth=false;
uniform vec4 reveal[16];
uniform int reveal_count=0;
varying vec3 world_pos;
varying float warmth;
varying vec2 tree_condition;
void vertex(){tree_condition=INSTANCE_CUSTOM.ba;warmth=INSTANCE_CUSTOM.g;if(use_growth){VERTEX*=INSTANCE_CUSTOM.r;}world_pos=(MODEL_MATRIX*vec4(VERTEX,1)).xyz;world_pos.xz+=evosim_region_offset;float anchored=smoothstep(.03,1.8,VERTEX.y);VERTEX.x+=sin(TIME*.8+world_pos.x*.7+world_pos.z*.4)*strength*anchored;VERTEX.z+=cos(TIME*.63+world_pos.z*.5)*strength*.5*anchored;}
void fragment(){
 if(tree_condition.y>.5)discard;
 float light_variation=.9+.12*sin(world_pos.x*1.6+world_pos.z*.9+world_pos.y*2.);
 vec3 leaf_color=base_color.rgb*COLOR.rgb;
 vec3 autumn=mix(leaf_color*vec3(1.9,1.13,.70),vec3(.14,.034,.028),smoothstep(.45,.85,warmth));
 ALBEDO=mix(leaf_color,autumn,smoothstep(.05,.7,warmth))*light_variation;ALBEDO=mix(ALBEDO,ALBEDO*vec3(1.35,.86,.53),evosim_dry*.65);ALBEDO*=1.-evosim_rain*.10;ALBEDO=mix(ALBEDO,vec3(.30,.24,.11),tree_condition.x*.8);ROUGHNESS=.96;SPECULAR=.12;
 float keep=1.;
 for(int i=0;i<reveal_count;i++){
  if(VERTEX.z>reveal[i].z+.3){
   float dist=length((SCREEN_UV-reveal[i].xy)*vec2(VIEWPORT_SIZE.x/VIEWPORT_SIZE.y,1.));
   keep=min(keep,mix(.05,1.,smoothstep(reveal[i].w*.65,reveal[i].w,dist)));
  }
 }
 if(keep<.99){float d=fract(sin(dot(FRAGCOORD.xy,vec2(12.9898,78.233)))*43758.5453);if(d>keep)discard;}
}
