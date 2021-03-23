using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Microsoft.Win32;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;

[ValueConversion( typeof( string ), typeof( Visibility ) )]
public class StringToVisibilityConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return string.IsNullOrEmpty( value as string ) ? Visibility.Collapsed : Visibility.Visible;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}

namespace CUBE2LUT2
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			ParseCommandLineArguments();
		}

		private void LogError(string message, bool keep_previous = false)
		{
			Console.Error.WriteLine( message );

			if ( keep_previous && messagesTextBlock.Text.Length > 0)
			{
				messagesTextBlock.Text += ". ";
				messagesTextBlock.Text += message;
			}
			else
			{
				messagesTextBlock.Text = message;
			}
		}

		private void PrintHelp()
		{
			Console.WriteLine( "-swapgb --- Swaps blue and green channels when converting" );
			Console.WriteLine( "-out OUTPUT_PATH  --- Results will be saved to the following file/directory" );
		}

		private void ParseCommandLineArguments()
		{
			string[] args = Environment.GetCommandLineArgs();

			List<string> filesToConvert = new List<string>();
			string output_directory = null;

			// args[0] is executable's filepath
			for ( int i = 1; i < args.Length; i++ )
			{
				string argument_without_prefix = null;
				if ( args[i].StartsWith("/") || args[i].StartsWith("-") )
				{
					argument_without_prefix = args[i].Substring( 1 );
				}

				if ( argument_without_prefix != null)
				{
					switch ( argument_without_prefix.ToLower() )
					{
						case "help":
						case "?":
							PrintHelp();
							Close();
							return;
						case "swapgb":
							swapGreenAndBlueChannels.IsChecked = true;
							break;
						case "out":
							if ( i + 1 < args.Length )
							{
								i++;
								output_directory = args[i];
							}
							else
							{
								LogError( "-out parameter used without specifying output path", true );
							}
							break;
						default:
							LogError( $"Unknown command '{argument_without_prefix}'", true );
							break;
					}
				}
				else
				{
					filesToConvert.Add( args[i] );
				}
			}

			if ( filesToConvert.Count > 0 )
			{
				closeAfterProcessFinishes.IsChecked = true;

				ConvertFiles( filesToConvert, output_directory );
			}
		}
		private void UploadButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog
			{
				CheckFileExists = true,
				CheckPathExists = true,
				ValidateNames = true,
				DefaultExt = ".CUBE",
				Filter = "LUT (*.CUBE)|*.CUBE|All files (*.*)|*.*",
				Multiselect = true,
				Title = "Select 3d lookup table file(s) to convert"
			};

			if ( dialog.ShowDialog() == true)
			{
				string output_path = GetSaveLocation( dialog.FileNames );
				if ( output_path != null )
				{
					ConvertFiles( dialog.FileNames, output_path );
				}
			}
		}

		private void UploadButton_Drop(object sender, DragEventArgs e)
		{
			if ( e.Data.GetDataPresent( DataFormats.FileDrop ) )
			{
				string[] files = e.Data.GetData( DataFormats.FileDrop ) as string[];
				string output_path = GetSaveLocation( files );
				if ( output_path != null )
				{
					ConvertFiles( files, output_path );
				}
			}
		}

		// Returns null if user cancels
		private string GetSaveLocation(IList<string> filenames)
		{
			if ( askForOutputDirectory.IsChecked == false )
			{
				if ( filenames.Count > 1 )
				{
					return System.IO.Path.GetDirectoryName( filenames.First() );
				}
				else
				{
					return System.IO.Path.ChangeExtension( filenames.First(), ".png" );
				}
			}


			if ( filenames.Count > 1 )
			{
				// Get directory where to save files
				FolderBrowserDialog dialog = new FolderBrowserDialog
				{
					SelectedPath = System.IO.Path.GetDirectoryName( filenames.First() ),
					Description = "Save Color Lookup Table textures",
				};

				if ( dialog.ShowDialog( this.AsWinformWindow() ) == System.Windows.Forms.DialogResult.OK)
				{
					return dialog.SelectedPath;
				}
			}
			else
			{
				SaveFileDialog dialog = new SaveFileDialog
				{
					AddExtension = true,
					DefaultExt = ".png",
					Filter = "*.png|*.png|All files (*.*)|*.*",
					Title = "Save Color Lookup Table texture",
					FileName = System.IO.Path.GetFileNameWithoutExtension( filenames.First() ),
					InitialDirectory = System.IO.Path.GetDirectoryName( filenames.First() )
				};

				if ( dialog.ShowDialog() == true )
				{
					return dialog.FileName;
				}
			}

			return null;
		}

		private void ConvertFiles(IList<string> filenames, string output)
		{
			bool swapGB = swapGreenAndBlueChannels.IsChecked == true;

			if ( filenames.Count > 1 )
			{
				foreach ( string file in filenames )
				{
					string output_file = System.IO.Path.ChangeExtension( file, ".png" );
					LutConverter converter = new LutConverter
					{
						inputCubeFilepath = file,
						outputPngFilepath = output_file,
						swapGB = swapGB
					};
					converter.Convert();
				}
			}
			else
			{
				LutConverter converter = new LutConverter
				{
					inputCubeFilepath = filenames.First(),
					outputPngFilepath = output,
					swapGB = swapGB
				};
				converter.Convert();
			}

			if ( closeAfterProcessFinishes.IsChecked == true )
			{
				Close();
			}
		}

	}
}
