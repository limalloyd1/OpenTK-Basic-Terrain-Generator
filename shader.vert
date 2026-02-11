#version 410 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;

out vec3 FragPos;
out vec3 Normal;
out vec2 TexCoords;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;


void main(void)
{
	FragPos = vec3(model * vec4(aPosition, 1.0));
	Normal = mat3(transpose(inverse(model))) * aNormal;

	TexCoords = (model * vec4(aPosition, 1.0)).xz * 0.1;
	gl_Position = projection * view * model * vec4(aPosition, 1.0);
	// gl_Position = projection * view * model * vec4(aPosition, 1.0);
}
