using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace TSOClient.Code.Utils
{
    public class RenderUtils
    {
        public static RenderTarget2D CreateRenderTarget(GraphicsDevice device, int numberLevels, SurfaceFormat surface, int width, int height)
        {
            return new RenderTarget2D(device, width, height, false, surface, DepthFormat.Depth24);
        }
    }
}
