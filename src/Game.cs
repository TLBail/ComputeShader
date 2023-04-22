using System.Numerics;
using ImGuiNET;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace ComputeShader;

public class Game
{

    private Camera camera;

    private OpenGl openGl;
    
    public Game() {
        this.openGl = OpenGl.Instance();
        openGl.DrawEvent += Draw;
        openGl.UpdateEvent += Update;
        openGl.LoadEvent += Load; 
        openGl.OnCloseEvent += Stop;
        
        openGl.Run();
    }

   

    private  void Load(GL gl) {
        camera = new Camera(openGl.window);
        camera.Position = new Vector3(0, 0, 8);
    }
    
    private  void Update(double deltatime) {
    }

    private void Draw(GL gl) {
        SetUniforms(gl);

        ImGui.ShowDemoWindow();
    }

    private unsafe void SetUniforms(GL gl) {
        
        gl.BindBuffer(BufferTargetARB.UniformBuffer, openGl.uboWorld);
        Matrix4x4 projectionMatrix = camera.GetProjectionMatrix();
        gl.BufferSubData(BufferTargetARB.UniformBuffer, 0, (uint)sizeof(Matrix4x4), projectionMatrix);
        gl.BindBuffer(BufferTargetARB.UniformBuffer, 0);


        gl.BindBuffer(BufferTargetARB.UniformBuffer, openGl.uboWorld);
        Matrix4x4 viewMatrix = camera.GetViewMatrix();
        gl.BufferSubData(BufferTargetARB.UniformBuffer, sizeof(Matrix4x4), (uint)sizeof(Matrix4x4), viewMatrix);
        gl.BindBuffer(BufferTargetARB.UniformBuffer, 0);

    }

    private void Stop() {
        
    }
    
}