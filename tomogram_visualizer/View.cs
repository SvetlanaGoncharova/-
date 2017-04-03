using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;

namespace Shumihin_tomogram_visualizer
{
    class View
    {
        Bitmap textureImage;
        int VBOtexture;

        int transfMin = 0;
        int transfWidth = 2000;

        public void setTransfMin(int value)
        {
            transfMin = value;
        }

        public void setTranfWidth(int value)
        {
            transfWidth = value;
        }
        
        public int clamp(int value, int min, int max) // value set in the borders
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        public void setupView(int width, int height)
        {
            GL.ShadeModel(ShadingModel.Smooth);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Bin.X, 0, Bin.Y, -1, 1);
            GL.Viewport(0, 0, width, height);
        }

        Color TransferFunction(short value)
        {
            int min = transfMin;
            //int max = 2000;
            int max = transfMin + transfWidth;
            int kost=max-min;
            if (kost == 0) kost += 1; 
            int newVal = clamp((value - min) * 255 / (kost), 0, 255);
            return Color.FromArgb(255, newVal, newVal, newVal);
        }

        public void drawQuads(int layerNuber)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Begin(BeginMode.Quads);
            for (int x_coord = 0; x_coord < Bin.X - 1; x_coord++)
                for (int y_coord = 0; y_coord < Bin.Y - 1; y_coord++)
                {
                    short value;

                    // 1
                    value = Bin.array[x_coord + y_coord * Bin.X + layerNuber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value));
                    GL.Vertex2(x_coord, y_coord);

                    //2
                    value = Bin.array[x_coord + (y_coord + 1) * Bin.X + layerNuber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value));
                    GL.Vertex2(x_coord, y_coord + 1);


                    //3
                    value = Bin.array[x_coord + 1 + (y_coord + 1) * Bin.X + layerNuber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value));
                    GL.Vertex2(x_coord + 1, y_coord + 1);

                    //4
                    value = Bin.array[x_coord + 1 + (y_coord) * Bin.X + layerNuber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value));
                    GL.Vertex2(x_coord + 1, y_coord);
                }
            GL.End();
        }

        public void load2DTexture()
        {
            GL.BindTexture(TextureTarget.Texture2D, VBOtexture);
            BitmapData data = textureImage.LockBits(
                new System.Drawing.Rectangle(0, 0, textureImage.Width, textureImage.Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                PixelType.UnsignedByte, data.Scan0);

            textureImage.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Linear);

            ErrorCode er = GL.GetError();
            string str = er.ToString();
        }

        public void generateTextureImage(int layerNumber)
        {
            textureImage = new Bitmap(Bin.X, Bin.Y);
            for (int i = 0; i < Bin.X; ++i)
                for (int j=0; j < Bin.Y; ++j)
                {
                    int pixelNumber = i + j * Bin.X + layerNumber * Bin.X * Bin.Y;
                    textureImage.SetPixel(i, j, TransferFunction(Bin.array[pixelNumber]));
                }
        }

        public void drawTexture()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, VBOtexture);

            GL.Begin(BeginMode.Quads);
            GL.Color3(Color.White);
            GL.TexCoord2(0f, 0f);
            GL.Vertex2(0, 0);

            GL.TexCoord2(0f, 1f);
            GL.Vertex2(0, Bin.Y);

            GL.TexCoord2(1f, 1f);
            GL.Vertex2(Bin.X, Bin.Y);

            GL.TexCoord2(1f, 0f);
            GL.Vertex2(Bin.X, 0);

            GL.End();
            GL.Disable(EnableCap.Texture2D);

        }
    }
}
