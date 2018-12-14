using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPI311.GameEngine {
    public class Skybox {
        public static Effect shader;
        public Model skyboxModel;
        public Texture2D skyboxTexture;

        public void draw(Vector3 position, Camera c, GraphicsDevice g) {

            /*SamplerState ss = new SamplerState();
            ss.AddressU = TextureAddressMode.Clamp;
            ss.AddressV = TextureAddressMode.Clamp;
            g.SamplerStates[0] = ss;

            DepthStencilState dss = new DepthStencilState();
            dss.DepthBufferEnable = false;
            g.DepthStencilState = dss;*/

            g.RasterizerState = new RasterizerState() {
                FillMode = FillMode.Solid,
                CullMode = CullMode.None
            };

            Matrix world = Matrix.CreateScale(Vector3.One) * Matrix.CreateFromQuaternion(Quaternion.Identity) * Matrix.CreateTranslation(position + Vector3.Backward * 1.25f + Vector3.Down * 0.05f);

            Matrix view = c.View;
            Matrix projection = c.Projection;

            shader.CurrentTechnique = shader.Techniques[0];
            shader.Parameters["World"].SetValue(world);
            shader.Parameters["View"].SetValue(view);
            shader.Parameters["Projection"].SetValue(projection);
            shader.Parameters["Texture"].SetValue(skyboxTexture);

            foreach (EffectPass pass in shader.CurrentTechnique.Passes) {
                pass.Apply();
                foreach (ModelMesh mesh in skyboxModel.Meshes)
                    foreach (ModelMeshPart part in mesh.MeshParts) {
                        g.SetVertexBuffer(part.VertexBuffer);
                        g.Indices = part.IndexBuffer;
                        g.DrawIndexedPrimitives(
                            PrimitiveType.TriangleList, part.VertexOffset, 0,
                            part.NumVertices, part.StartIndex, part.PrimitiveCount);
                    }
            }

            /*dss = new DepthStencilState();
            dss.DepthBufferEnable = true;
            g.DepthStencilState = dss;*/
        }
    }
}
