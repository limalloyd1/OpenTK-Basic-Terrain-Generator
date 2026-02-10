#version 410 core
out vec4 FragColor;
in vec3 FragPos;

void main()
{
    // Blue screen background
    vec3 blueScreen = vec3(0.0, 0.0, 0.67);
    
    // Add horizontal scanlines
    float scanline = mod(FragPos.y * 150.0, 1.0);
    if (scanline < 0.1) {
        blueScreen *= 0.9;
    }
    
    FragColor = vec4(blueScreen, 1.0);
}
