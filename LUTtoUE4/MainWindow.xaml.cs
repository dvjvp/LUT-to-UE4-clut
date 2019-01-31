using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;

namespace LUTtoUE4
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void button_Click(object sender, RoutedEventArgs e)
		{
			string[] fileContent = FileOpener.OpenLUT();
			if (fileContent == null) return;

			int LUTsize;
			Vector3 domainMin, domainMax;
			GetLUTMetadata(fileContent, out LUTsize, out domainMin, out domainMax);

			string savePath = FileOpener.GetImageSaveLocation();
			if (savePath == null) return;

			Image3D content = new Image3D(GetLUTData(fileContent), LUTsize);

			const int ue4colorLookUpSize = 16;
			using (Bitmap b = content.ToColorLookUpImage(ue4colorLookUpSize, domainMin, domainMax, (bool)swizzle.IsChecked))
			{
				if (b == null)
				{
					b.Dispose();
					MessageBox.Show("Error while trying to convert image!");
					return;
				}

				b.Save(savePath, System.Drawing.Imaging.ImageFormat.Png);
			}

			MessageBox.Show("Conversion successful!");
		}

		private string[] GetLUTData(string[] fileContent)
		{
			int startIndex = fileContent.TakeWhile(s=> !s.StartsWith("#LUT data points")).Count();
			int arrayLength = fileContent.Length - startIndex - 1;

			string[] subArray = new string[arrayLength];
			Array.Copy(fileContent, startIndex+1, subArray, 0, arrayLength);
			return subArray;
		}

		private void GetLUTMetadata(string[] fileContent, out int LUTsize, out Vector3 domainMin, out Vector3 domainMax)
		{
			string temp;

			temp = fileContent.First(s => s.StartsWith("LUT_3D_SIZE"));
			LUTsize = Convert.ToInt32(temp.Substring("LUT_3D_SIZE ".Length));

			// In case values are not specified, use default values:
			//http://wwwimages.adobe.com/www.adobe.com/content/dam/acom/en/products/speedgrade/cc/pdfs/cube-lut-specification-1.0.pdf

			try
			{
				temp = fileContent.First( s => s.StartsWith( "DOMAIN_MIN" ) );
				domainMin = Vector3.FromString( temp.Substring( "DOMAIN_MIN ".Length ) );
			}
			catch (Exception)
			{
				domainMin = new Vector3( 0, 0, 0 );
			}

			try
			{
				temp = fileContent.First( s => s.StartsWith( "DOMAIN_MAX" ) );
				domainMax = Vector3.FromString( temp.Substring( "DOMAIN_MAX ".Length ) );
			}
			catch (Exception)
			{
				domainMax = new Vector3( 1, 1, 1 );
			}

		}

	}
}
