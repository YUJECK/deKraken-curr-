using Framp.InputSystem;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Framp.Windows;

public class RenderManager : ITickable
{
    private readonly RenderWindow RenderWindow;
    private readonly List<IToDraw> _toDraws = new();

    private Camera CurrentCamera;
    private Camera ToCamera;
    
    public Vector2u WindowSize => RenderWindow.Size;

    public RenderManager(WindowWrapper windowWrapper)
    {
        RenderWindow = new RenderWindow(VideoMode.DesktopMode, "Framp", Styles.Default);
        
        windowWrapper.SetRenderWindow(RenderWindow);
        EntityMaster.OnAdded += OnEntityAdded;
        
        RenderWindow.Resized += OnResize;
    }

    public void Tick()
    {
        RenderWindow.DispatchEvents();
        
        if(CurrentCamera != null)
            CurrentCamera.ApplyViewToRenderWindow(RenderWindow);
        
        foreach (var toDraw in _toDraws)
        {
            Draw(toDraw.ToDraw);
        }
    }

    private void OnResize(object? sender, SizeEventArgs e)
    {
        CurrentCamera.SetViewSize(e.Width, e.Height);
    }

    private void Draw(Drawable drawable)
    {
        RenderWindow.Draw(drawable);    
    }

    public async void SetCamera(Camera camera, float transition = 0f)
    {
        if (transition > 0 && CurrentCamera != null)
        {
            Vector2f startCenter = CurrentCamera.Center;
            Vector2f endCenter = camera.Center;
     
            CurrentCamera = camera;
            camera.SetCameraPosition(startCenter);
            
            while (CurrentCamera.Center != endCenter)
            {
                CurrentCamera.SetCameraPosition(Vector2Utilities.MoveTo(CurrentCamera.Center, endCenter, transition));
                
                await Task.Delay(10);
            }
        }
        
        CurrentCamera = camera;
    }

    private void OnEntityAdded(Entity entity)
    {
        var toDraw = entity.Components.FindByBaseClass<IToDraw>();
        
        if(toDraw != null)
            _toDraws.Add(toDraw);
    }
}