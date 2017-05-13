using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Globalization;

namespace LUTtoUE4
{
	public struct Vector3
	{
		public float r, g, b;

		public static Vector3 FromString(string text)
		{
			Vector3 v = new Vector3();
			string[] tab = text.Split(' ');
			v.r = float.Parse(tab[0], CultureInfo.InvariantCulture.NumberFormat);
			v.g = float.Parse(tab[1], CultureInfo.InvariantCulture.NumberFormat);
			v.b = float.Parse(tab[2], CultureInfo.InvariantCulture.NumberFormat);
			return v;
		}

		public Vector3(float r, float g, float b)
		{
			this.r = r;this.g = g;this.b = b;
		}



		public Color ToPixel(Vector3 domainMin, Vector3 domainMax)
		{
			return Color.FromArgb(
				ToByte(r, domainMin.r, domainMax.r),
				ToByte(g, domainMin.g, domainMax.g),
				ToByte(b, domainMin.b, domainMax.b));
		}
		public Color ToPixelSwizzle(Vector3 domainMin, Vector3 domainMax)
		{
			return Color.FromArgb(
				ToByte(r, domainMin.r, domainMax.r),
				ToByte(b, domainMin.b, domainMax.b),
				ToByte(g, domainMin.g, domainMax.g));
		}

		private static int ToByte(float f, float in0, float in1)
		{
			return (int)(byte.MaxValue * ((f - in0) / (in1 - in0)));
		}
	}
}
