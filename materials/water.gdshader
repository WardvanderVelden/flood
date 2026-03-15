shader_type spatial;
render_mode blend_mix, depth_prepass_alpha, cull_back;

uniform float water_alpha : hint_range(0.0,1.0) = 0.35;
uniform vec4 water_color : source_color = vec4(0.1, 0.4, 0.8, 1.0);
uniform sampler2D depth_texture : hint_depth_texture, filter_linear_mipmap;

void fragment() {
	// Compute the difference in depth 
    float depth = texture(depth_texture, SCREEN_UV).x * 2.0 - 1.0;
	#if CURRENT_RENDERER == RENDERER_COMPATIBILITY
	vec3 ndc = vec3(SCREEN_UV, depth) * 2.0 - 1.0;
	#else
	vec3 ndc = vec3(SCREEN_UV * 2.0 - 1.0, depth);
	#endif
	
	vec4 view = INV_PROJECTION_MATRIX * vec4(ndc, 1.0);
	view.xyz /= view.w;
	float scene_depth = -view.z;
	
    vec4 world = INV_VIEW_MATRIX * INV_PROJECTION_MATRIX * vec4(ndc, 1.0);
  	vec3 world_position = world.xyz / world.w;
	
	float depth_diff = world_position.z - scene_depth;

  	// Visualize linear depth
  	//ALBEDO.rgb = vec3(fract(scene_depth));
  	//ALBEDO.rgb = vec3(fract(scene_depth));
	//ALBEDO.rgb = fract(world_position).xyz;
	ALBEDO = water_color.rgb;
	ALPHA = 0.6;
}