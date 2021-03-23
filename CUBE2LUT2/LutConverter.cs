using System.Drawing;

namespace CUBE2LUT2
{
	class LutConverter
	{
		public string inputCubeFilepath;
		public string outputPngFilepath;
		public bool swapGB;

		const int UE4_LUT_DIMENSION_SIZE = 16;


		public void Convert()
		{
			CubeFile cube = new CubeFile( inputCubeFilepath );
			float dimension_mult = 1.0f / cube.size;

			Bitmap bitmap = new Bitmap( UE4_LUT_DIMENSION_SIZE * UE4_LUT_DIMENSION_SIZE, UE4_LUT_DIMENSION_SIZE );

			for ( int b = 0; b < UE4_LUT_DIMENSION_SIZE; b++ )
			{
				for ( int g = 0; g < UE4_LUT_DIMENSION_SIZE; g++ )
				{
					for ( int r = 0; r < UE4_LUT_DIMENSION_SIZE; r++ )
					{
						ColorF cubePixel = cube.GetColor( r * dimension_mult, g * dimension_mult, b * dimension_mult );

						if ( swapGB )
						{
							float temp = cubePixel.blue;
							cubePixel.blue = cubePixel.green;
							cubePixel.green = temp;
						}

						Color bitmapPixel = cube.ToBitmapColor( cubePixel );

						bitmap.SetPixel( r + ( b * UE4_LUT_DIMENSION_SIZE ), g, bitmapPixel );
					}
				}
			}

			bitmap.Save( outputPngFilepath, System.Drawing.Imaging.ImageFormat.Png );

			bitmap.Dispose();


		}
	}
}
