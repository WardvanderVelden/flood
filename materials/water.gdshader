shader_type spatial;
render_mode depth_prepass_alpha, cull_back;

uniform sampler2D depth_texture : hint_depth_texture;

void fragment() {
    float scene_depth = texture(depth_texture, SCREEN_UV).x;
	vec3 ndc = vec3(SCREEN_UV * 2.0 - 1.0, scene_depth);
	vec4 view = INV_PROJECTION_MATRIX * vec4(ndc, 1.0);
	view.xyz /= view.w;
	float linear_scene_depth = -view.z; // Distance from camera
	
	float linear_frag_depth = -VERTEX.z;
	
	//if (linear_frag_depth < linear_scene_depth + 0.1) {
        //discard;
    //}
	
	//if (linear_frag_depth > 10.0) {
		//discard;
	//
	
	//if (linear_frag_depth + 0.1 < linear_scene_depth) {
		//discard;
	//}

    //ALBEDO = vec3(fract(linear_scene_depth));
	ALBEDO = vec3(0.0, 0.0, 0.8);
	//ALBEDO = vec3(fract(scene_depth));
    ALPHA = 0.5;
}