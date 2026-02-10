#version 410 core
out vec4 FragColor;
in vec3 FragPos;
in vec3 Normal;
uniform vec4 color;
uniform vec3 lightPos;
uniform vec3 lightColor;
uniform vec3 viewPos;

float random(vec2 st){
	return fract(sin(dot(st.xy, vec2(12.9898,78.233))) * 43758.5453123);
}

// Bayer matrix for ordered dithering (PS2 style)
float dither4x4(vec2 position, float brightness) {
    int x = int(mod(position.x, 4.0));
    int y = int(mod(position.y, 4.0));
    
    // 4x4 Bayer matrix
    int index = y * 4 + x;
    float limit = 0.0;
    
    if (index == 0) limit = 0.0;
    if (index == 1) limit = 8.0;
    if (index == 2) limit = 2.0;
    if (index == 3) limit = 10.0;
    if (index == 4) limit = 12.0;
    if (index == 5) limit = 4.0;
    if (index == 6) limit = 14.0;
    if (index == 7) limit = 6.0;
    if (index == 8) limit = 3.0;
    if (index == 9) limit = 11.0;
    if (index == 10) limit = 1.0;
    if (index == 11) limit = 9.0;
    if (index == 12) limit = 15.0;
    if (index == 13) limit = 7.0;
    if (index == 14) limit = 13.0;
    if (index == 15) limit = 5.0;
    
    return brightness < limit / 16.0 ? 0.0 : 1.0;
}

void main()
{
	// Pixelate the position
	float pixelSize = 0.02; // Adjust for larger/smaller pixels
	vec2 pixelatedPos = floor(FragPos.xz / pixelSize) * pixelSize;
	
	// Generate chunky noise
	float noise = random(pixelatedPos * 0.1);
	
	// Create distinct sand color variations (PS2 palette style)
	float colorStep = floor(noise * 3.0) / 3.0; // 3 distinct shades
	vec3 sandBase = color.rgb;
	vec3 sandDark = sandBase * 0.7;
	vec3 sandLight = sandBase * 1.2;
	
	vec3 sandColor = mix(sandDark, sandLight, colorStep);
	
	// Lighting calculations
	float ambientStrength = 0.4;
	vec3 ambient = ambientStrength * lightColor;
	
	vec3 norm = normalize(Normal);
	vec3 lightDir = normalize(lightPos - FragPos);
	float diff = max(dot(norm, lightDir), 0.0);
	vec3 diffuse = diff * lightColor;
	
	float specularStrength = 0.05;
	vec3 viewDir = normalize(viewPos - FragPos);
	vec3 reflectDir = reflect(-lightDir, norm);
	float spec = pow(max(dot(viewDir, reflectDir), 0.0), 4.0);
	vec3 specular = specularStrength * spec * lightColor;
	
	vec3 result = (ambient + diffuse + specular) * sandColor;
	
	// Apply dithering based on brightness
	float brightness = (result.r + result.g + result.b) / 3.0;
	float dither = dither4x4(gl_FragCoord.xy, brightness);
	result *= mix(0.75, 1.0, dither); // Dither between 85% and 100% brightness
	
	FragColor = vec4(result, color.a);
}
