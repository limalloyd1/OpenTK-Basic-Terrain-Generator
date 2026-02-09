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
	// Ambient Lighting
	float ambientStrength = 0.6;
	vec3 ambient = ambientStrength * lightColor;

	// Diffuse Lighting
	vec3 norm = normalize(Normal);
	vec3 lightDir = normalize(lightPos - FragPos);
	float diff = max(dot(norm, lightDir), 0.0);
	vec3 diffuse = diff * lightColor;

	float specularStrength = 0.4;
	vec3 viewDir = normalize(viewPos - FragPos);
	vec3 reflectDir = reflect(-lightDir, norm);
	float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
	vec3 specular = specularStrength * spec * lightColor;

	vec3 result = (ambient + diffuse + specular) * color.rgb;
	FragColor = vec4(result, color.a);

	// White
	// FragColor = vec4(0.0f,0.0f,0.0f, 1.0f);

	// Beige
	// FragColor = vec4(0.65f,0.58f,0.48f, 1.0f);

	// Brown
	// FragColor = vec4(0.25f,0.2f,0.23f, 1.0f);
}

