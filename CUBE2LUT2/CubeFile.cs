using System;
using System.Drawing;

namespace CUBE2LUT2
{
	struct ColorF
	{
		public float red;
		public float green;
		public float blue;

		public ColorF(float r, float g, float b)
		{
			red = r;
			green = g;
			blue = b;
		}
		public ColorF(string stringToParse)
		{
			stringToParse = stringToParse.Trim();
			string[] elements = stringToParse.Split( ' ' );

			var numberFormat = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

			red = float.Parse( elements[0], numberFormat );
			green = float.Parse( elements[1], numberFormat );
			blue = float.Parse( elements[2], numberFormat );
		}

		public static ColorF Interpolate(ColorF col1, ColorF col2, ColorF alpha)
		{
			return new ColorF(
				Lerp( col1.red, col2.red, alpha.red ),
				Lerp( col1.green, col2.green, alpha.green ),
				Lerp( col1.blue, col2.blue, alpha.blue )
				);
		}

		private static float Lerp(float val1, float val2, float alpha)
		{
			return ( val1 * ( 1.0f - alpha ) ) + ( val2 * alpha );
		}
	}

	class CubeFile
	{
		public readonly ColorF[] data;

		public readonly ColorF domainMin = new ColorF( 0, 0, 0 );
		public readonly ColorF domainMax = new ColorF( 1, 1, 1 );

		public readonly int dimensions = 3;
		public readonly int size = 256;

		public CubeFile(string filepath)
		{
			bool data_block_began = false;
			int data_idx = 0;

			string[] text = System.IO.File.ReadAllLines( filepath );

			foreach ( string line in text )
			{
				if ( string.IsNullOrWhiteSpace( line ) || line.StartsWith( "#" ) || line.StartsWith( "LUT_1D_INPUT_RANGE" ) || line.StartsWith( "LUT_3D_INPUT_RANGE" ) )
				{
					// Empty line or a comment. Ignore
					continue;
				}

				if ( !data_block_began )
				{
					if ( CheckForParameter( line, "TITLE", out _ ) )
					{
						// Not used by this tool
						continue;
					}
					else if ( CheckForParameter( line, "LUT_1D_SIZE", out string sLut1dSize ) )
					{
						dimensions = 1;
						size = int.Parse( sLut1dSize );
						continue;
					}
					else if ( CheckForParameter( line, "LUT_3D_SIZE", out string sLut3dSize ) )
					{
						dimensions = 3;
						size = int.Parse( sLut3dSize );
						continue;
					}
					else if ( CheckForParameter( line, "DOMAIN_MIN", out string sDomainMin ) )
					{
						domainMin = new ColorF( sDomainMin );
						continue;
					}
					else if ( CheckForParameter( line, "DOMAIN_MAX", out string sDomainMax ) )
					{
						domainMax = new ColorF( sDomainMax );
						continue;
					}
				}

				// We checked all keywords, so at this point there should be only data
				if ( !data_block_began )
				{
					data_block_began = true;
					int totalSize = (int)Math.Pow((double)size, (double)dimensions);
					data = new ColorF[totalSize];
				}
				data[data_idx] = new ColorF( line );
				data_idx++;
			}
		}

		/// If line starts with paramName, returns true and copies rest of the line into paramValue
		private bool CheckForParameter(string line, string paramName, out string paramValue)
		{
			if ( line.StartsWith( paramName ) )
			{
				paramValue = line.Substring( paramName.Length + 1 );
				return true;
			}

			paramValue = null;
			return false;
		}

		/// r, g and b are in [0 --- size-1] range
		public ColorF GetPixel(int r, int g, int b)
		{
			int offset = r;

			if( dimensions == 1 )
			{
				// Read out the 1D LUT data.
				ColorF color;
				color.red = data[r].red;
				color.green = data[g].green;
				color.blue = data[b].blue;

				return color;
			}
			else
			{
				// It's a 3D LUT.
				offset += size * g;
				offset += size * size * b;
			}

			return data[offset];
		}

		/// nr, ng and nb should in normalized space, that is [0.0f --- 1.0f)
		public ColorF GetColor(float nr, float ng, float nb)
		{
			ColorF floor = GetPixel( (int)Math.Floor( nr * size ), (int)Math.Floor( ng * size ), (int)Math.Floor( nb * size ) );
			ColorF ceiling = GetPixel( (int)Math.Ceiling( nr * size ), (int)Math.Ceiling( ng * size ), (int)Math.Ceiling( nb * size ) );
			ColorF alpha = new ColorF( nr - (float)Math.Floor( nr ), ng - (float)Math.Floor( ng ), nb - (float)Math.Floor( nb ) );

			return ColorF.Interpolate( floor, ceiling, alpha );
		}

		public Color ToBitmapColor(ColorF color)
		{
			int Convert(float value, float rangeMin, float rangeMax)
			{
				float rangeSize = rangeMax - rangeMin;
				value -= rangeMin;
				value /= rangeSize;
				value *= byte.MaxValue;
				return (int)value;
			}

			int red = Convert( color.red, domainMin.red, domainMax.red );
			int green = Convert( color.green, domainMin.green, domainMax.green );
			int blue = Convert( color.blue, domainMin.blue, domainMax.blue );

			return Color.FromArgb( red, green, blue );
		}
	}
}
