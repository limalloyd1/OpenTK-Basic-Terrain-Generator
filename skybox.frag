#version 410 core
out vec4 FragColor;
in vec3 FragPos;

uniform float time;
uniform bool showRain;
uniform vec3 skyColor;

float random(vec2 p)
{
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453123);
}

float rain(vec2 uv, float t) {
    float rainAmount = 0.0;

    uv *= 12.0;

    for (int layer = 0; layer < 5; layer++) 
    {
        float speed = 3.0 + float(layer) * 1.2;
        float density = 1.0 + float(layer) * 0.35;

        vec2 rainUV = uv * density;
        rainUV.y += t * speed;

        vec2 cell = floor(rainUV);
        vec2 f = fract(rainUV);

        float dropChance = random(cell);

        if (dropChance > 0.50) {
            // thin vertical streak around x=0.5
            float xdist = abs(f.x - 0.5);
            float width = 1.0 - smoothstep(0.0, 0.015, xdist);

            float len = mix(0.12, 0.35, random(cell + 13.37));
            float yoff = random(cell + 7.77);
            float y = fract(f.y + yoff);

            float streak = smoothstep(0.0, 0.03, y) * smoothstep(len, 0.0, y);

            rainAmount += width * streak * 0.9;
        }
    }

    return clamp(rainAmount, 0.0, 1.0);
}

void main()
{
    // base color (use your uniform if you want)
    vec3 color = skyColor;

    // scanlines
    float scanline = fract(FragPos.y * 150.0);
    if (scanline < 0.1) {
        color *= 0.6;
    }

    // rain overlay
    if (showRain) {
        vec3 dir = normalize(FragPos);

        vec2 uv = vec2(
            atan(dir.z, dir.x) / (2.0 * 3.14159265) + 0.5,
            asin(dir.y) / 3.14159265 + 0.5
        );

        float rainStreaks = rain(uv, 10.0);

        vec3 rainColor = vec3(0.7, 0.7, 0.85);
        color = mix(color, rainColor, rainStreaks);
    }


    FragColor = vec4(skyColor, 1.0);
}

