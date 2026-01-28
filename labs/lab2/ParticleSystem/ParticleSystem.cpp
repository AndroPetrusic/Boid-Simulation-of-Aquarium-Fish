#include <GL/glut.h>
#include <cstdlib>
#include <cmath>
#include <ctime>
#include <cstdio>

#define _USE_MATH_DEFINES
#include <math.h>

#define MAX_PARTICLES 500

struct Particle {
    float x, y, z;
    float vx, vy, vz;
    float age, lifespan;
    float r, g, b, a;
};

Particle particles[MAX_PARTICLES];
float emitterX = 0.0f, emitterY = 0.0f, emitterZ = 0.0f;
float gravity = -9.81f;
clock_t lastTime = 0;

float fountainPower = 0.6f;
float spread = 1.0f;
float fadeSpeed = 3.0f;
float particleSize = 5.0f;

float radius = 13.0f;
float angle = float(M_PI) / 2;


float centerX = 0;
float centerZ = -5;
float eyeX = centerX + radius * cos(angle);
float eyeZ = centerZ + radius * sin(angle);
float eyeY = 5.0f;


float newEmitterX = 0;
float newEmitterZ = -5;


void initParticles() {
    srand(static_cast<unsigned int>(time(nullptr)));

    glEnable(GL_BLEND);
    glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
    glEnable(GL_DEPTH_TEST);
    glDisable(GL_TEXTURE_2D);

    for (int i = 0; i < MAX_PARTICLES; i++) {
        particles[i].x = emitterX;
        particles[i].y = emitterY;
        particles[i].z = emitterZ;

        particles[i].vx = ((rand() % 200) - 100) / 100.0f * spread * fountainPower;
        particles[i].vy = (10.0f + (rand() % 5)) * fountainPower;
        particles[i].vz = ((rand() % 200) - 100) / 100.0f * spread * fountainPower;

        particles[i].age = 0;
        particles[i].lifespan = 4.0f + (rand() % 20) / 10.0f;

        particles[i].r = 0.5f + (rand() % 30) / 100.0f;
        particles[i].g = 0.8f;
        particles[i].b = 1.0f;
        particles[i].a = 1.0f;
    }
}

void updateParticles() {
    clock_t currentTime = clock();

    if (lastTime == 0) {
        lastTime = currentTime;
        return;
    }
    float dt = static_cast<float>(currentTime - lastTime) / CLOCKS_PER_SEC;

    if (dt > 0.1f)  { dt = 0.01f; }

    lastTime = currentTime;

    for (int i = 0; i < MAX_PARTICLES; i++) {
        particles[i].age += dt;

        particles[i].vy += gravity * dt;
        particles[i].x += particles[i].vx * dt;
        particles[i].y += particles[i].vy * dt;
        particles[i].z += particles[i].vz * dt;

        // Fade out
        particles[i].a = 1.0f - fadeSpeed * pow(particles[i].age / particles[i].lifespan, 2);

        if (particles[i].age > particles[i].lifespan || particles[i].y < 0) {
            particles[i].x = emitterX;
            particles[i].y = emitterY + 0.3f;
            particles[i].z = emitterZ;

            particles[i].vx = ((rand() % 200) - 100) / 100.0f * spread * fountainPower;
            particles[i].vy = (10.0f + (rand() % 7)) * fountainPower;
            particles[i].vz = ((rand() % 200) - 100) / 100.0f * spread * fountainPower;

            particles[i].age = 0;
            particles[i].lifespan = 4.0f + (rand() % 20) / 10.0f;
        }
    }
}

void renderParticles() {
    glPointSize(particleSize);
    glBegin(GL_POINTS);
    for (int i = 0; i < MAX_PARTICLES; i++) {
        if (particles[i].age > particles[i].lifespan || particles[i].a <= 0) continue;
        if (rand() % 2 == 0) {
            glColor4f(particles[i].r, particles[i].g, particles[i].b, particles[i].a);
            glVertex3f(particles[i].x, particles[i].y, particles[i].z);
        }
        else {
            glColor4f(1.0f, particles[i].g, particles[i].b, particles[i].a);
            glVertex3f(particles[i].x + newEmitterX, particles[i].y, particles[i].z + newEmitterZ);
        }
        
    }
    glEnd();
}

void display() {
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
    glLoadIdentity();

    // Camera
    eyeX = centerX + radius * cos(angle);
    eyeZ = centerZ + radius * sin(angle);
    gluLookAt(eyeX, eyeY, eyeZ, emitterX, 0, emitterZ, 0, 1, 0);

    // Ground
    glColor3f(0.1f, 0.4f, 0.05f);
    glBegin(GL_QUADS);
    glVertex3f(-20, 0, -20);
    glVertex3f(20, 0, -20);
    glVertex3f(20, 0, 20);
    glVertex3f(-20, 0, 20);
    glEnd();

    // Emitter
    glColor3f(0.0f, 0.0f, 1.0f);
    glPushMatrix();
    glTranslatef(emitterX, emitterY, emitterZ);
    glutSolidSphere(0.2f, 10, 10);
    glPopMatrix();

    updateParticles();
    renderParticles();

    glutSwapBuffers();
}

void reshape(int w, int h) {
    glViewport(0, 0, w, h);
    glMatrixMode(GL_PROJECTION);
    glLoadIdentity();
    gluPerspective(60, static_cast<float>(w) / h, 0.1f, 100.0f);
    glMatrixMode(GL_MODELVIEW);
}

void timer(int value) {
    glutPostRedisplay();
    glutTimerFunc(10, timer, 0);
}

void keyboard(unsigned char key, int x, int y) {
    switch (key) {
    case 'a': emitterX -= 0.5f; break;
    case 'd': emitterX += 0.5f; break;
    case 'w': emitterZ -= 0.5f; break; 
    case 's': emitterZ += 0.5f; break;
    case 'u': fountainPower += 0.01f; break;
    case 'j': fountainPower -= 0.01f; if (fountainPower < 0.1f) fountainPower = 0.1f; break;
    case 'i': spread += 0.05f; break;
    case 'k': spread -= 0.05f; if (spread < 0.1f) spread = 0.1f; break;
    case '+': radius += 0.1f; break;
    case '-': radius -= 0.1f; break;
    case 27: case 'q': exit(0); break;
    }
}

void specialKeys(int key, int x, int y) {
    switch (key) {
    case GLUT_KEY_UP: eyeY += 0.05f; break;
    case GLUT_KEY_DOWN: eyeY -= 0.05f; break;
    case GLUT_KEY_RIGHT: angle -= 0.01f; break;
    case GLUT_KEY_LEFT: angle += 0.01f; break;
    }
    if (angle < 0) angle += 2 * M_PI;
    if (angle >= 2 * M_PI) angle -= M_PI;
}

int main(int argc, char** argv) {
    glutInit(&argc, argv);
    glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGB | GLUT_DEPTH);
    glutInitWindowSize(800, 600);
    glutCreateWindow("Fountain - WASD move, U/J power, I/K spread, ESC quit");

    printf("Controls: WASD = move fountain\n");
    printf("U = increase power | J = decrease power | I = increase spread | K = decrease spread\n");
    //printf("Current power: %.1f\n", fountainPower);

    initParticles();

    glutDisplayFunc(display);
    glutReshapeFunc(reshape);
    glutKeyboardFunc(keyboard);
    glutSpecialFunc(specialKeys);
    glutTimerFunc(0, timer, 0);

    glClearColor(0.2f, 0.2f, 0.5f, 1.0f);

    glutMainLoop();
    return 0;
}
