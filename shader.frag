#version 410 core
out vec4 FragColor;

in vec3 FragPos;
in vec3 Normal;
in vec2 TexCoords;

uniform vec4 color;
uniform vec3 lightPos;
uniform vec3 lightColor;
uniform vec3 viewPos;
uniform vec3 skyColor;
uniform bool showGrid;


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
    else if (index == 1) limit = 8.0;
    else if (index == 2) limit = 2.0;
    else if (index == 3) limit = 10.0;
    else if (index == 4) limit = 12.0;
    else if (index == 5) limit = 4.0;
    else if (index == 6) limit = 14.0;
    else if (index == 7) limit = 6.0;
    else if (index == 8) limit = 3.0;
    else if (index == 9) limit = 11.0;
    else if (index == 10) limit = 1.0;
    else if (index == 11) limit = 9.0;
    else if (index == 12) limit = 15.0;
    else if (index == 13) limit = 7.0;
    else if (index == 14) limit = 13.0;
    else if (index == 15) limit = 5.0;
    
    return brightness < limit / 16.0 ? 0.0 : 1.0;
}



void main()
{
    // Pixelate the position
    float pixelSize = 0.02;
    vec2 pixelatedPos = floor(FragPos.xz / pixelSize) * pixelSize;
    
    // Generate chunky noise
    float noise = random(pixelatedPos * 0.1);
    
    // Create distinct color variations (PS2 palette style)
    float colorStep = floor(noise * 10.0) / 10.0;
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
    result *= mix(0.75, 1.0, dither);
    
    // Set base color
    FragColor = vec4(result, color.a);
    
    // Apply fog
    float distance = length(viewPos - FragPos);
    float fogFactor = exp(-distance * 0.01);
    FragColor = mix(vec4(skyColor, 1.0), FragColor, fogFactor);
    
    // Grid overlay
	if (showGrid){
		vec2 grid = abs(fract(TexCoords - 0.5) - 0.5) / fwidth(TexCoords);
    	float line = min(grid.x, grid.y);
    	float gridColor = 1.0 - min(line, 1.0);
		// Blend the grid
    	FragColor = mix(FragColor, vec4(0.1,0.1,0.1,1.0), gridColor * 0.1);
	}    

}