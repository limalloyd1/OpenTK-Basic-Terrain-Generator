#version 410 core
out vec4 FragColor;

in vec3 FragPos;
in vec3 Normal;

uniform vec4 color;
uniform vec3 lightPos;
uniform vec3 lightColor;
uniform vec3 viewPos;

void main()
{
    // Ambient lighting with a tint
    float ambientStrength = 0.5;
    vec3 shadowTint = vec3(0.7, 0.8, 1.0);  // Cool blue tint for shadows
    vec3 ambient = ambientStrength * lightColor * shadowTint;
    
    // Diffuse lighting
    float diffuseStrength = 0.6f;
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(lightPos - FragPos);
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diffuseStrength * diff * lightColor;
    
    // Specular lighting
    float specularStrength = 0.2;
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
    vec3 specular = specularStrength * spec * lightColor;
    
    // Combine with shadow tint affecting darker areas more
    vec3 result = (ambient + diffuse + specular) * color.rgb;
    FragColor = vec4(result, color.a);
}
