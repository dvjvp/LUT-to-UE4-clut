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
			float domainMin, domainMax;
			GetLUTMetadata(out LUTsize, out domainMin, out domainMax);

			string savePath = FileOpener.GetImageSaveLocation();
			if (savePath == null) return;

			using (Bitmap b = FileOpener.ToImage(GetLUTData(fileContent), LUTsize, domainMin, domainMax))
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
			throw new NotImplementedException();
		}

		private void GetLUTMetadata(out int LUTsize, out float domainMin, out float domainMax)
		{
			throw new NotImplementedException();
		}
	}
}
