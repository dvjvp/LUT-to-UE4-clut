using System;
using System.Drawing;

namespace LUTtoUE4
{
	class Image3D
	{
		public Vector3[,,] pixels;
		private int dimensionSize;

		public Image3D(string[] data, int dimensionSize)
		{
			this.dimensionSize = dimensionSize;
			pixels = new Vector3[dimensionSize, dimensionSize, dimensionSize];

			int arrayIndex = 0;
			for (int w = 0; w < dimensionSize; w++)
			{
				for (int y = 0; y < dimensionSize; y++)
				{
					for (int x = 0; x < dimensionSize; x++, arrayIndex++)
					{
						pixels[x, y, w] = Vector3.FromString(data[arrayIndex]); ;
					}
				}
			}
		}

		public Color GetColorAt(float x, float y, float w, Vector3 domainMin, Vector3 domainMax, bool swizzle)
		{
			Vector3 vc = pixels[
				IndexFrom0to1Range(x),
				IndexFrom0to1Range(y),
				IndexFrom0to1Range(w)];
			
			return swizzle ? vc.ToPixelSwizzle(domainMin, domainMax) : vc.ToPixel(domainMin, domainMax);
		}

		private int IndexFrom0to1Range(float value)
		{
			return (int)(Math.Round(value * dimensionSize));
		}

		public Bitmap ToColorLookUpImage(int dimensionSize, Vector3 domainMin, Vector3 domainMax, bool swizzle)
		{
			Bitmap b = new Bitmap(dimensionSize * dimensionSize, dimensionSize);
			float fDimensionSize = dimensionSize;

			for (int w = 0; w < dimensionSize; w++)
			{
				for (int y = 0; y < dimensionSize; y++)
				{
					for (int x = 0; x < dimensionSize; x++)
					{
						b.SetPixel(x + w * dimensionSize, y, 
							GetColorAt(
							x / fDimensionSize, y / fDimensionSize, w / fDimensionSize,
							domainMin, domainMax, swizzle)
							);
					}
				}
			}

			return b;
		}

	}
}
