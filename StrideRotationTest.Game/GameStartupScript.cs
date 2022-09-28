using Stride.Engine;
using Stride.Rendering.UI;
using StrideRotationTest.UI;
using System.Diagnostics;
using System.Linq;

namespace StrideRotationTest
{
    public class GameStartupScript : SyncScript
    {
        private IUINavigationManager _uiNavProcessor;

        public override void Start()
        {
            if (Entity.EntityManager.GetProcessor<UINavigationProcessor>() == null)
            {
                var uiNavProcessor = new UINavigationProcessor();
                Entity.EntityManager.Processors.Add(uiNavProcessor);
                Services.AddService<IUINavigationManager>(uiNavProcessor);
                _uiNavProcessor = uiNavProcessor;
            }
            else
            {
                Debug.Fail("UINavigationProcessor has already been added.");
            }
        }

        public override void Update()
        {
            // Note we can't use Start because the RenderFeature isn't initialized in time.
            var uiRenderFeature = SceneSystem.GraphicsCompositor.RenderFeatures.FirstOrDefault(x => x is UIRenderFeature) as UIRenderFeature;
            Debug.Assert(uiRenderFeature != null, "GraphicsCompositor is missing UIRenderFeature");
            if (!uiRenderFeature.Initialized)
            {
                return;
            }
            var rendererFactory = new GameUIRendererFactory(Services);
            rendererFactory.RegisterToUIRenderFeature(uiRenderFeature);

            var uiComp = Entity.Get<UIComponent>();
            if (uiComp != null)
            {
                // Note: it's important the UIComponent is DISABLED at first, otherwise
                // the UI is loaded before registering the GameUIRendererFactory, and it then ignores our custom renderers.
                uiComp.Enabled = true;
                _uiNavProcessor.OnControlStatesUpdated();
            }

            Entity.Remove(this);    // Startup has finished, we can remove this script to prevent further action
        }
    }
}
