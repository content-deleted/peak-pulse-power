using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace CPI311.GameEngine {
    public abstract class Material {
        public abstract void Render(Camera c, Transform t, Model m, GraphicsDevice g);
    }

    public class DefaultMaterial : Material {
        public override void Render(Camera c, Transform t, Model m, GraphicsDevice g) {
            m.Draw(t.World, c.View, c.Projection);
        }
    }

    public class StandardLightingMaterial : Material {
        public static Effect effect;

        public Vector3 lightPosition = Vector3.Zero;
        public float shininess = 20f;
        public Vector3 ambientColor = new Vector3(0.1f, 0.5f, 0.1f);
        public Vector3 diffuseColor = new Vector3(0.6f, 0.3f, 0.5f);
        public Vector3 specularColor = new Vector3(0, 0, 0.3f);

        public Texture2D texture;

        public bool useTexture = true;

        public override void Render(Camera c, Transform t, Model m, GraphicsDevice g) {
            Matrix view = c.View;
            Matrix projection = c.Projection;

            effect.CurrentTechnique = effect.Techniques[0];
            effect.Parameters["World"].SetValue(t.World);
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);
            effect.Parameters["LightPosition"].SetValue(c.Transform.Position + Vector3.Up * 10);
            effect.Parameters["CameraPosition"].SetValue(c.Transform.Position);
            effect.Parameters["Shininess"].SetValue(shininess);
            effect.Parameters["AmbientColor"].SetValue(ambientColor);
            effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
            effect.Parameters["SpecularColor"].SetValue(specularColor);
            effect.Parameters["DiffuseTexture"].SetValue(texture);
            effect.Parameters["UseTexture"].SetValue(useTexture);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
                pass.Apply();
                foreach (ModelMesh mesh in m.Meshes)
                    foreach (ModelMeshPart part in mesh.MeshParts) {
                        g.SetVertexBuffer(part.VertexBuffer);
                        g.Indices = part.IndexBuffer;
                        g.DrawIndexedPrimitives(
                            PrimitiveType.TriangleList, part.VertexOffset, 0,
                            part.NumVertices, part.StartIndex, part.PrimitiveCount);
                    }
            }
        }
    }

    public class TerrainRenderer : Material {
        public static Effect effect;

        public GameObject3d ourObject; // LITERALLY A HACK
        private Texture2D HeightMap;
        public Texture2D NormalMap;

        public Vector2 size;
        private VertexPositionTexture[] Vertices { get; set; }

        private int[] Indices { get; set; }
        private float[] heights { get; set; }
        public TerrainRenderer(Texture2D texture, Vector2 size, Vector2 res) {

            HeightMap = texture;
            this.size = size;

            CreateHeights(); //  Heights data is crated 

            // We should also save the value of size somewhere
            int rows = (int)res.Y + 1;
            int cols = (int)res.X + 1;

            Vector3 offset = new Vector3(-size.X / 2, 0, -size.Y / 2);
            float stepX = size.X / res.X;
            float stepZ = size.Y / res.Y;
            Vertices = new VertexPositionTexture[rows * cols];
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    Vertices[r * cols + c] = new VertexPositionTexture(
                        offset + new Vector3(c * stepX, GetHeight(new Vector2(c / res.X, r / res.Y)), r * stepZ),
                        new Vector2(c / res.X, r / res.Y));

            Indices = new int[(rows - 1) * (cols - 1) * 6];
            int index = 0;
            for (int r = 0; r < rows - 1; r++)
                for (int c = 0; c < cols - 1; c++) {
                    Indices[index++] = r * cols + c;
                    Indices[index++] = r * cols + c + 1;
                    Indices[index++] = (r + 1) * cols + c;

                    Indices[index++] = (r + 1) * cols + c;
                    Indices[index++] = r * cols + c + 1;
                    Indices[index++] = (r + 1) * cols + c + 1;
                }
        }
        public float GetHeight(Vector2 tex) {
            // First, scale it to dimensions of the image
            tex = Vector2.Clamp(tex, Vector2.Zero, Vector2.One) * new Vector2(HeightMap.Width - 1, HeightMap.Height - 1);
            int x = (int)tex.X; float u = tex.X - x;
            int y = (int)tex.Y; float v = tex.Y - y;
            return heights[y * HeightMap.Width + x] * (1 - u) * (1 - v) +
                 heights[y * HeightMap.Width + Math.Min(x + 1, HeightMap.Width - 1)] * u * (1 - v) +
                 heights[Math.Min(y + 1, HeightMap.Height - 1) * HeightMap.Width + x] * (1 - u) * v +
                 heights[Math.Min(y + 1, HeightMap.Height - 1) * HeightMap.Width + Math.Min(x + 1, HeightMap.Width - 1)] * u * v;
        }
        private void CreateHeights() {
            Color[] data = new Color[HeightMap.Width * HeightMap.Height];
            HeightMap.GetData<Color>(data);
            heights = new float[HeightMap.Width * HeightMap.Height];
            for (int i = 0; i < heights.Length; i++)
                heights[i] = data[i].G / 255f;
        }

        public float GetAltitude(Vector3 position) {
            position = Vector3.Transform(position, Matrix.Invert(ourObject.transform.World));
            if (position.X > -size.X / 2 && position.X < size.X / 2 &&
                              position.Z > -size.Y / 2 && position.Z < size.Y / 2)
                return GetHeight(new Vector2((position.X + size.X / 2) / size.X,
                    (position.Z + size.Y / 2) / size.Y)) * ourObject.transform.LocalScale.Y;
            return -1;
        }

        public Vector3 lightPosition;
        float shininess = 0.3f;

        public Vector3 ambientColor = new Vector3(0.3f, 0.2f, 0.2f);
        public Vector3 diffuseColor = new Vector3(0.1f, 0.1f, 0.1f);
        public Vector3 specularColor = new Vector3(0.2f, 0.2f, 0.2f);

        public override void Render(Camera c, Transform t, Model m, GraphicsDevice g) {
            Matrix view = c.View;
            Matrix projection = c.Projection;

            // Setup custom shader etc.
            effect.CurrentTechnique = effect.Techniques[0];
            effect.Parameters["World"].SetValue(ourObject.transform.World);
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);
            effect.Parameters["LightPosition"].SetValue(lightPosition);
            effect.Parameters["CameraPosition"].SetValue(c.Transform.Position);
            effect.Parameters["Shininess"].SetValue(shininess);
            effect.Parameters["AmbientColor"].SetValue(ambientColor);
            effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
            effect.Parameters["SpecularColor"].SetValue(specularColor);
            effect.Parameters["NormalMap"].SetValue(NormalMap);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
                pass.Apply();
                g.DrawUserIndexedPrimitives<VertexPositionTexture>
                (PrimitiveType.TriangleList, Vertices, 0, Vertices.Length, Indices, 0, Indices.Length / 3);
            }
        }

    }

    public class CustomTerrainRenderer : Material {
        public static Effect effect;


        private VertexPositionNormalTexture[] Vertices { get; set; }

        private int[] Indices { get; set; }
        private float[] heights { get; set; }

        private static int rows, cols;

        private static Perlin noise = new Perlin();

        public GameObject3d obj;
        public int lastRowDepth;

        public void updateDepth (int depth) {
            for (int c = 0; c < cols; c++)
                Vertices[(depth % cols) * cols + c] = new VertexPositionNormalTexture(
                    new Vector3(c, GetHeight(c, depth+cols), depth + cols),
                     Vector3.Down,
                    new Vector2(c, depth + cols));
            lastRowDepth++;
        }
        public void updateNormals (int depth, Vector3 normal) {
            for (int c = 0; c < cols; c++) Vertices[( depth % cols) * cols + c].Normal = normal;
        }


        public CustomTerrainRenderer(Vector2 res) {

            rows = (int)res.Y + 1;
            cols = (int)res.X + 1;

            heights = new float[rows * cols];
            //CreateHeights(); 

            Vertices = new VertexPositionNormalTexture[rows * cols];

            for (int r = 0; r < rows; r++) {
                if (r > flatBuffer) heightAlter = 1; //THIS IS TO BUFFER WITH SOME FLAT GROUND AT THE START
                for (int c = 0; c < cols; c++)
                    Vertices[r * cols + c] = new VertexPositionNormalTexture(
                        new Vector3(c, GetHeight(c, r), r),
                         Vector3.Up,
                        new Vector2(c, r));
            }
            //c / res.X, r / res.Y <-uv covers whole mesh
            // 
            Indices = new int[(rows) * (cols - 1) * 6];
            int index = 0;
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols - 1; c++) {
                    Indices[index++] = r * cols + c;
                    Indices[index++] = r * cols + c + 1;
                    Indices[index++] = ((r + 1) % rows) * cols + c;

                    //Vector3 n = generateNormal(Vertices[Indices[index - 1]].Position, Vertices[Indices[index - 2]].Position, Vertices[Indices[index - 3]].Position); 

                    Indices[index++] = ((r + 1) % rows) * cols + c;
                    Indices[index++] = r * cols + c + 1;
                    Indices[index++] = ((r + 1) % rows) * cols + c + 1;

                    //Vector3 n2 = generateNormal(Vertices[Indices[index - 1]].Position, Vertices[Indices[index - 2]].Position, Vertices[Indices[index - 3]].Position);

                    //Vertices[r * cols + c].Normal = Vector3.Normalize(n + n2);
                }
        }



        // We take 3 points of a triangle and find its normal
        public Vector3 generateNormal(Vector3 a, Vector3 b, Vector3 c) {
            Vector3 ab = Vector3.Normalize(a - b);
            Vector3 bc = Vector3.Normalize( b - c);
            Vector3 ca = Vector3.Normalize(c - a);

            return Vector3.Normalize(Vector3.Cross(ab, bc) + Vector3.Cross(bc, ca) + Vector3.Cross(ca, ab));
        }

        const int flatBuffer = 120; // Represents the y distance before the start of the song
        const int minimumTerrainHeight = 22;
        const float canyonEdgePercent = 0.66f; // What percentage of the field is not the rising edge of the canyon
        const float canyonIncline = 27; // how steep
        public static float GetHeight(double x, double y) {
            double halfRows = rows / 2.0;
            double canyonEdge = (Math.Max(Math.Abs(halfRows - x) / halfRows, canyonEdgePercent) - canyonEdgePercent) * canyonIncline;

            // start with a canyon to obscure the players view of the terrain edges
            float canyonShape = (float)(canyonEdge * canyonEdge);
            // generate a base terrain level for smooth valleys
            float baseHeight = (float)(100 * noise.OctavePerlin(x / (float)rows, y / (float)cols, 0.5f, 2, 7)) - 225;
            // base scaler
            float baseScaler = (float) Math.Min((y - flatBuffer) / 10, baseHeight);
            if (y < flatBuffer) {
                return minimumTerrainHeight;
            } else {
                // bruh i have no idea
                return baseScaler + (float)noise.OctavePerlin(10f * x / (float)rows, 10f * y / (float)cols, 0.5f, 2, 6) * 10;
            }
        }

        public float heightAlter = 0;

        public float GetAltitude(Vector3 position) {
           position = Vector3.Transform(position, Matrix.Invert(obj.transform.World));
            //if (position.X > 0 && position.X < rows  && position.Z > 0 && position.Z < cols )
                return GetHeight(position.X, position.Z);
            //return 0;
        }

        public Vector3 lightPosition;
        float shininess = 0.3f;

        public static Texture2D wire;

        public Vector3 ambientColor = new Vector3(0.3f, 0.2f, 0.2f);
        public Vector3 diffuseColor = new Vector3(0.1f, 0.1f, 0.1f);
        public Vector3 specularColor = new Vector3(0.2f, 0.2f, 0.2f);

        public static Texture2D song;
        public float songPos;
        public float avgE;

        public override void Render(Camera c, Transform t, Model m, GraphicsDevice g) {
            
            Matrix view = c.View;
            Matrix projection = c.Projection;

            // Setup custom shader etc.
            effect.CurrentTechnique = effect.Techniques[0];
            effect.Parameters["World"].SetValue(t.World);
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);
            effect.Parameters["CameraPosition"].SetValue(c.Transform.Position);
            
            //wierd shit
            effect.Parameters["songData"].SetValue(song);
            effect.Parameters["songPos"].SetValue(songPos);
            effect.Parameters["avgE"].SetValue(avgE);

            GameScreenManager.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GameScreenManager.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            g.RasterizerState = new RasterizerState() {
                FillMode = FillMode.Solid,
                CullMode = CullMode.None//CullMode.CullCounterClockwiseFace
            };

            effect.Parameters["Offset"].SetValue(30f);
            effect.Parameters["Color"].SetValue(new Vector3(.1f, 0, 0.5f));
            effect.Parameters["AlphaMax"].SetValue(0.9f);
            effect.Parameters["HeightOffset"].SetValue(0f);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
                pass.Apply();
                g.DrawUserIndexedPrimitives<VertexPositionNormalTexture>
                (PrimitiveType.TriangleList, Vertices, 0, Vertices.Length, Indices, 0, Indices.Length / 3);
            }

            // Specific Params
            effect.Parameters["Offset"].SetValue(40f);
            effect.Parameters["Color"].SetValue(new Vector3(0.8f,0,0.2f));
            effect.Parameters["AlphaMax"].SetValue(1f);
            effect.Parameters["HeightOffset"].SetValue(0.5f);
            
            g.RasterizerState = new RasterizerState() {
                FillMode = FillMode.WireFrame,
                CullMode = CullMode.None
            };

            foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
                pass.Apply();
                g.DrawUserIndexedPrimitives<VertexPositionNormalTexture>
                (PrimitiveType.TriangleList, Vertices, 0, Vertices.Length, Indices, 0, Indices.Length / 3);
            }

            g.RasterizerState = new RasterizerState() {
                FillMode = FillMode.Solid,
                CullMode = CullMode.None
            };

            // Ground bellow

        }

    }

    public class Hoop : Material {
        public static Effect effect;
        
        private VertexPositionNormalTexture[] Vertices { get; set; }

        private int[] Indices { get; set; }

        public GameObject3d obj;

        public Hoop(float innerRadius, float outerRadius, float width, int triangles) {

            Vertices = new VertexPositionNormalTexture[triangles * 5];

            float currentInnerA = 0;
            float currentOuterA = -(float)Math.PI / (triangles * 2);

            int index = 0;
            for (int i = 0; i < triangles; i++) {

                // inner point
                Vector3 inner = pointOnCircle(innerRadius, currentInnerA, 0);
                Vertices[index++] = new VertexPositionNormalTexture(
                    inner,
                    inner,
                    new Vector2(0.5f, 1f));

                //outer 1
                Vector3 temp = pointOnCircle(outerRadius, currentOuterA, width);
                Vertices[index++] = new VertexPositionNormalTexture(
                    temp,
                    inner,
                    new Vector2(0f, 0f));

                //backside
                temp = pointOnCircle(outerRadius, currentOuterA, -width);
                Vertices[index++] = new VertexPositionNormalTexture(
                    temp,
                    inner,
                    new Vector2(0f, 1f));

                currentOuterA += (float)Math.PI / (triangles);

                //outer 2
                temp = pointOnCircle(outerRadius, currentOuterA, width);
                Vertices[index++] = new VertexPositionNormalTexture(
                    temp,
                    inner,
                    new Vector2(1f, 0f));

                //backside
                temp = pointOnCircle(outerRadius, currentOuterA, -width);
                Vertices[index++] = new VertexPositionNormalTexture(
                    temp,
                    inner,
                    new Vector2(1f, 0f));

                currentOuterA += (float)Math.PI / (triangles);
                currentInnerA += 2*(float)Math.PI / triangles;
            }
            //c / res.X, r / res.Y <-uv covers whole mesh
            // 
            Indices = new int[triangles * 18];
            index = 0;
            for (int i = 0; i < triangles; i++) {
                int offset = (i * 5);
                Indices[index++] = offset + 1;
                Indices[index++] = offset + 3;
                Indices[index++] = offset + 0;

                Indices[index++] = offset + 3;
                Indices[index++] = offset + 4;
                Indices[index++] = offset + 0;

                Indices[index++] = offset + 4;
                Indices[index++] = offset + 2;
                Indices[index++] = offset + 0;

                Indices[index++] = offset + 2;
                Indices[index++] = offset + 1;
                Indices[index++] = offset + 0;

                Indices[index++] = offset + 2;
                Indices[index++] = offset + 4;
                Indices[index++] = offset + 3;

                Indices[index++] = offset + 3;
                Indices[index++] = offset + 1;
                Indices[index++] = offset + 2;
            }
        }

        Vector3 pointOnCircle(float radius, float angle, float z) => new Vector3((float)(radius * Math.Cos(angle)), (float)(radius * Math.Sin(angle)), z);

        public float rotation;

        public override void Render(Camera c, Transform t, Model m, GraphicsDevice g) {

            Matrix view = c.View;
            Matrix projection = c.Projection;

            // Setup custom shader etc.
            effect.CurrentTechnique = effect.Techniques[0];
            effect.Parameters["World"].SetValue(t.World);
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);
            effect.Parameters["CameraPosition"].SetValue(c.Transform.Position);

            effect.Parameters["rotation"].SetValue(rotation);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
                pass.Apply();
                g.DrawUserIndexedPrimitives<VertexPositionNormalTexture>
                (PrimitiveType.TriangleList, Vertices, 0, Vertices.Length, Indices, 0, Indices.Length / 3);
            }
        }
    }


    public class SpeedAndCollideEffect : Material {
    public static Effect effect;

    public float shininess = 20f;
    public Vector3 ambientColor = new Vector3(0.5f, 0.5f, 0.5f);
    public Vector3 diffuseColor = new Vector3(0, 0, 0);
    public Vector3 specularColor = new Vector3(0, 0, 0.5f);

    //public Texture2D texture;
    public static Texture2D disperseSample;
    public float timeSinceCol = 0;

    public override void Render(Camera c, Transform t, Model m, GraphicsDevice g) {
        Matrix view = c.View;
        Matrix projection = c.Projection;
        /*
        effect.CurrentTechnique = effect.Techniques[0];
        effect.Parameters["World"].SetValue(t.World);
        effect.Parameters["View"].SetValue(view);
        effect.Parameters["Projection"].SetValue(projection);
        effect.Parameters["LightPosition"].SetValue(Vector3.Up * 5);
        effect.Parameters["CameraPosition"].SetValue(c.Transform.Position);
        effect.Parameters["Shininess"].SetValue(shininess);
        effect.Parameters["AmbientColor"].SetValue(ambientColor);
        effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
        effect.Parameters["SpecularColor"].SetValue(specularColor);
        // effect.Parameters["DiffuseTexture"].SetValue(texture);
        effect.Parameters["timeSinceCol"].SetValue(timeSinceCol);
        effect.Parameters["DisperseTexture"].SetValue(disperseSample);
        */
        effect.CurrentTechnique = effect.Techniques[0];
        effect.Parameters["World"].SetValue(t.World);
        effect.Parameters["View"].SetValue(view);
        effect.Parameters["Projection"].SetValue(projection);
        //effect.Parameters["LightPosition"].SetValue(lightPosition);
        effect.Parameters["CameraPosition"].SetValue(c.Transform.Position);

        foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
            pass.Apply();
            foreach (ModelMesh mesh in m.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts) {
                    g.SetVertexBuffer(part.VertexBuffer);
                    g.Indices = part.IndexBuffer;
                    g.DrawIndexedPrimitives(
                        PrimitiveType.TriangleList, part.VertexOffset, 0,
                        part.NumVertices, part.StartIndex, part.PrimitiveCount);
                }
            }
        }
    }
}
