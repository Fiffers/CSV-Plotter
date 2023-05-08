using CSV_Plotter.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using CSV_Plotter.Commands;
using System.Windows;
using Python.Runtime;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.VisualBasic;

namespace CSV_Plotter.ViewModels
{
    class MainWindowViewModel : BaseViewModel, INotifyPropertyChanged
    {
        #region Fields
        private const string registryKey = @"HKEY_CURRENT_USER\Software\CSVPlotter";
        private const string registryValueName = "PythonDllPath";

        private dynamic[] xAxisCastedValues     = Array.Empty<dynamic>();
        private dynamic[] yAxisCastedValues     = Array.Empty<dynamic>();
        private dynamic[] colorAxisCastedValues = Array.Empty<dynamic>();
        #endregion

        #region Properties
        public enum SortByOptions
        {
            None,
            XAxis,
            YAxis,
        }

        public enum SortDirectionOptions
        {
            Ascending,
            Descending
        }

        public bool SelectPythonDLLButton = true;

        private string _selectedFile = string.Empty;
        public string SelectedFile
        {
            get => this._selectedFile;
            set
            {
                this._selectedFile = value;
                OnPropertyChanged();
            }
        }

        private bool _canParse = false;
        public bool CanParse
        {
            get { return this._canParse; }
            set
            {
                this._canParse = value;
                OnPropertyChanged();
            }
        }

        private bool _hasParsed = false;
        public bool HasParsed
        {
            get { return this._hasParsed; }
            set
            {
                this._hasParsed = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<HeaderColumn> _headers = new();
        public ObservableCollection<HeaderColumn> Headers
        {
            get => this._headers;
            set
            {
                this._headers = value;
                OnPropertyChanged();
            }
        }

        private HeaderColumn _xAxisSelectedItem = new();
        public HeaderColumn XAxisSelectedItem
        {
            get => _xAxisSelectedItem;
            set
            {
                _xAxisSelectedItem = value;
                OnPropertyChanged();
                if (XAxisSelectedItem != null)
                {
                    XAxisPlotLabel = XAxisSelectedItem.Header;
                    XAxisSelectedItem.Column = getColumnValues(SelectedFile, XAxisSelectedItem.Header);
                    List<Type> castableTypes = getCastableTypes(XAxisSelectedItem.Column[0]);

                    if (castableTypes == null)
                    {
                        return;
                    }

                    XAxisCastableTypes = new();

                    foreach (Type type in castableTypes)
                    {
                        XAxisCastableTypes.Add(type);
                    }
                }                
            }
        }

        private HeaderColumn _yAxisSelectedItem = new();
        public HeaderColumn YAxisSelectedItem
        {
            get => _yAxisSelectedItem;
            set
            {
                _yAxisSelectedItem = value;
                OnPropertyChanged();
                if (YAxisSelectedItem != null)
                {
                    YAxisPlotLabel = YAxisSelectedItem.Header;
                    YAxisSelectedItem.Column = getColumnValues(SelectedFile, YAxisSelectedItem.Header);

                    List<Type> castableTypes = getCastableTypes(YAxisSelectedItem.Column[0]);

                    if (castableTypes == null)
                    {
                        return;
                    }

                    YAxisCastableTypes = new();

                    foreach (Type type in castableTypes)
                    {
                        YAxisCastableTypes.Add(type);
                    }
                }                
            }
        }

        private HeaderColumn _colorAxisSelectedItem = new();
        public HeaderColumn ColorAxisSelectedItem
        {
            get => _colorAxisSelectedItem;
            set
            {
                _colorAxisSelectedItem = value;
                OnPropertyChanged();
                if (ColorAxisSelectedItem != null)
                {
                    ColorAxisPlotLabel = ColorAxisSelectedItem.Header;
                    ColorAxisSelectedItem.Column = getColumnValues(SelectedFile, ColorAxisSelectedItem.Header);

                    List<Type> castableTypes = getCastableTypes(ColorAxisSelectedItem.Column[0]);

                    if (castableTypes == null)
                    {
                        return;
                    }

                    ColorAxisCastableTypes = new();
                    
                    foreach (Type type in castableTypes)
                    {
                        ColorAxisCastableTypes.Add(type);
                    }
                }                
            }
        }

        private string _xAxisPlotLabel = string.Empty;
        public string XAxisPlotLabel
        {
            get => _xAxisPlotLabel;
            set
            {
                _xAxisPlotLabel = value;
                OnPropertyChanged();
            }
        }

        private string _yAxisPlotLabel = string.Empty;
        public string YAxisPlotLabel
        {
            get => _yAxisPlotLabel;
            set
            {
                _yAxisPlotLabel = value;
                OnPropertyChanged();
            }
        }

        private string _colorAxisPlotLabel = string.Empty;
        public string ColorAxisPlotLabel
        {
            get => _colorAxisPlotLabel;
            set
            {
                _colorAxisPlotLabel = value;
                OnPropertyChanged();
            }
        }

        private string[] _xAxisCastToOptions = Array.Empty<string>();
        public string[] XAxisCastToOptions
        {
            get => _xAxisCastToOptions;
            set
            {
                _xAxisCastToOptions = value;
                OnPropertyChanged();
            }
        }

        private string[] _yAxisCastToOptions = Array.Empty<string>();
        public string[] YAxisCastToOptions
        {
            get => _yAxisCastToOptions;
            set
            {
                _yAxisCastToOptions = value;
                OnPropertyChanged();
            }
        }

        private string[] _colorAxisCastToOptions = Array.Empty<string>();
        public string[] ColorAxisCastToOptions
        {
            get => _colorAxisCastToOptions;
            set
            {
                _colorAxisCastToOptions = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Type> _xAxisCastableTypes = new();
        public ObservableCollection<Type> XAxisCastableTypes
        {
            get => _xAxisCastableTypes;
            set
            {
                _xAxisCastableTypes = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Type> _yAxisCastableTypes = new();
        public ObservableCollection<Type> YAxisCastableTypes
        {
            get => _yAxisCastableTypes;
            set
            {
                _yAxisCastableTypes = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Type> _colorAxisCastableTypes = new();
        public ObservableCollection<Type> ColorAxisCastableTypes
        {
            get => _colorAxisCastableTypes;
            set
            {
                _colorAxisCastableTypes = value;
                OnPropertyChanged();
            }
        }

        private Type _xAxisSelectedCastType = typeof(string);
        public Type XAxisSelectedCastType
        {
            get => _xAxisSelectedCastType;
            set
            {
                _xAxisSelectedCastType = value;
                OnPropertyChanged();
            }
        }

        private Type _yAxisSelectedCastType = typeof(string);
        public Type YAxisSelectedCastType
        {
            get => _yAxisSelectedCastType;
            set
            {
                _yAxisSelectedCastType = value;
                OnPropertyChanged();
            }
        }

        private Type _colorAxisSelectedCastType = typeof(string);
        public Type ColorAxisSelectedCastType
        {
            get => _colorAxisSelectedCastType;
            set
            {
                _colorAxisSelectedCastType = value;
                OnPropertyChanged();
            }
        }

        private byte[] _encodedImage = Array.Empty<byte>();
        public byte[] EncodedImage
        {
            get => _encodedImage;
            set
            {
                _encodedImage = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<SortByOptions> _sortBy = new();
        public ObservableCollection<SortByOptions> SortBy
        {
            get
            {
                return new ObservableCollection<SortByOptions>()
                {
                    SortByOptions.None,
                    SortByOptions.XAxis,
                    SortByOptions.YAxis                    
                };
            }
        }
        
        public ObservableCollection<SortDirectionOptions> SortDirections
        {
            get
            {
                return new ObservableCollection<SortDirectionOptions>()
                {
                    SortDirectionOptions.Ascending,
                    SortDirectionOptions.Descending
                };
            }
        }

        private SortByOptions _sortBySelection;
        public SortByOptions SortBySelection
        {
            get => _sortBySelection;
            set
            {
                _sortBySelection = value;
                OnPropertyChanged();
            }
        }

        private SortDirectionOptions _sortDirectionSelection;
        public SortDirectionOptions SortDirectionSelection
        {
            get => _sortDirectionSelection;
            set
            {
                _sortDirectionSelection = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<string> _colorMapOptions = new();
        public ObservableCollection<string> ColorMapOptions
        {
            get => _colorMapOptions;
            set
            {
                _colorMapOptions = value;
                OnPropertyChanged();
            }
        }

        private string _colorMapSelection = string.Empty;
        public string ColorMapSelection
        {
            get => _colorMapSelection;
            set 
            {
                _colorMapSelection = value;
                OnPropertyChanged();
            }
        }

        private bool _invertColorMap = false;
        public bool ReverseColorMap
        {
            get => _invertColorMap;
            set
            {
                _invertColorMap = value;
                OnPropertyChanged();
            }
        }

        private string _plotTitle = string.Empty;
        public string PlotTitle
        {
            get => _plotTitle;
            set
            {
                _plotTitle = value;
                OnPropertyChanged();
            }
        }

        private bool _canSavePlotImage = false;
        public bool CanSavePlotImage
        {
            get => _canSavePlotImage;
            set
            {
                _canSavePlotImage = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands
        public ICommand ExitApplicationCommand { get; }
        public ICommand UpdatePythonDLLCommand { get; }
        public ICommand LoadCsvHeadersCommand { get; }
        public ICommand SelectFileCommand { get; }
        public ICommand CreatePlotCommand { get; }
        public ICommand SavePlotImageCommand { get; }
        #endregion

        #region Constructor
        public MainWindowViewModel()
        {
            string? appFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (appFolder == null)
            {
                this.exitApplication(new object());
            }

            getPythonDLLPath(false);
            PythonEngine.Initialize();
            getColorMaps();

            ExitApplicationCommand = new RelayCommand(exitApplication);
            UpdatePythonDLLCommand = new RelayCommand(updatePythonDLLPath);
            LoadCsvHeadersCommand  = new RelayCommand(loadCsvHeaders);
            SelectFileCommand      = new RelayCommand(selectFile);
            CreatePlotCommand      = new RelayCommand(createPlot);
            SavePlotImageCommand   = new RelayCommand(savePlotImage);
        }
        #endregion       

        #region Private Methods
        private void getColorMaps()
        {
            using (Py.GIL())
            {
                dynamic plt = Py.Import("matplotlib.pyplot");

                string[] colormaps = plt.colormaps();
                colormaps = colormaps.Where(s => !s.EndsWith("_r")).ToArray();
                ColorMapOptions = new ObservableCollection<string>(colormaps.OrderBy(x => x));
            }
        }

        private static string[] getColumnValues(string filePath, string headerName)
        {
            string[] lines = File.ReadAllLines(filePath);
            string? headerRow = lines.FirstOrDefault();

            if (headerRow == null)
            {
                return Array.Empty<string>();
            }

            var matchingColumnIndex = headerRow.Split(',').ToList().FindIndex(c => c.Trim() == headerName.Trim());

            if (matchingColumnIndex == -1)
            {
                throw new ArgumentException($"Column with matching string '{headerName}' not found.");
            }

            var result = new List<string>();
            for (int i = 1; i < lines.Length; i += 2)
            {
                var cells = lines[i].Split(',');
                if (cells.Length > matchingColumnIndex)
                {
                    result.Add(cells[matchingColumnIndex]);
                }
            }            

            return result.ToArray();
        }

        private static void getPythonDLLPath(bool updatePath)
        {        
            string? path = (string?)Registry.GetValue(registryKey, registryValueName, null);

            if (string.IsNullOrEmpty(path) || File.Exists(path) == false)
            {
                const string message = "You do not have a Python DLL file selected.\n" +
                                       "You will need to specify its location to continue.";
                const string caption = "Python DLL Missing!";
                MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            if (string.IsNullOrEmpty(path) || File.Exists(path) == false || updatePath == true)
            {
                string appDataLocalFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                OpenFileDialog openFileDialog = new()
                {
                    InitialDirectory = appDataLocalFolderPath,
                    Filter = "Python DLL files (*.dll)|*.dll|All files (*.*)|*.*",
                    Title = "Select Python DLL file"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    path = openFileDialog.FileName;
                    Registry.SetValue(registryKey, registryValueName, path);
                }
            }

            Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", path);
        }

        private void exitApplication(object parameter)
        {
            PythonEngine.Shutdown();
            Application.Current.Shutdown();
        }

        private void updatePythonDLLPath(object parameter)
        {
            getPythonDLLPath(true);
        }

        private void selectFile(object parameter)
        {
            OpenFileDialog openFileDialog = new();
            if (openFileDialog.ShowDialog() == true)
            {
                if (File.Exists(openFileDialog.FileName))
                {
                    SelectedFile = openFileDialog.FileName;
                    CanParse = true;
                    sanitizeCSV(SelectedFile);
                }
            }
        }

        private void castToSelection()
        {            
                dynamic[] resultsX = new dynamic[XAxisSelectedItem.Column.Length];
                for (int i = 0; i < XAxisSelectedItem.Column.Length; i++)
                {
                    resultsX[i] = (Convert.ChangeType(XAxisSelectedItem.Column[i], XAxisSelectedCastType));
                }

                xAxisCastedValues = resultsX;

                dynamic[] resultsY = new dynamic[YAxisSelectedItem.Column.Length];
                for (int i = 0; i < YAxisSelectedItem.Column.Length; i++)
                {
                    resultsY[i] = (Convert.ChangeType(YAxisSelectedItem.Column[i], YAxisSelectedCastType));
                }

                yAxisCastedValues = resultsY;

                dynamic[] resultsC = new dynamic[ColorAxisSelectedItem.Column.Length];
                for (int i = 0; i < ColorAxisSelectedItem.Column.Length; i++)
                {
                    resultsC[i] = (Convert.ChangeType(ColorAxisSelectedItem.Column[i], ColorAxisSelectedCastType));
                }

                colorAxisCastedValues = resultsC;           
        }

        private void sortColumns()
        {
            if (SortBySelection != SortByOptions.None)
            {
                if (SortBySelection == SortByOptions.XAxis)
                {
                    int[] indices = Enumerable.Range(0, xAxisCastedValues.Length).ToArray();
                    Array.Sort(xAxisCastedValues, indices);
                    Array.Sort(indices, yAxisCastedValues);

                    if (SortDirectionSelection == SortDirectionOptions.Descending)
                    {   
                        Array.Reverse(xAxisCastedValues);
                        Array.Reverse(yAxisCastedValues);
                    }
                }

                if (SortBySelection == SortByOptions.YAxis)
                {
                    int[] indices = Enumerable.Range(0, yAxisCastedValues.Length).ToArray();
                    Array.Sort(yAxisCastedValues, indices);
                    Array.Sort(indices, xAxisCastedValues);

                    if (SortDirectionSelection == SortDirectionOptions.Descending)
                    {
                        Array.Reverse(xAxisCastedValues);
                        Array.Reverse(yAxisCastedValues);
                    }
                }
            }         
        }

        private void createPlot(object parameter)
        {
            try
            {
                this.castToSelection();
                this.sortColumns();

                using (Py.GIL())
                {
                    dynamic np = Py.Import("numpy");
                    dynamic plt = Py.Import("matplotlib.pyplot");
                    dynamic io = Py.Import("io");
                    dynamic base64 = Py.Import("base64");

                    if (colorAxisCastedValues.Length > 0)
                    {
                        dynamic cmap = plt.cm.get_cmap(ColorMapSelection);
                        if (ReverseColorMap == true)
                        {
                            cmap = cmap.reversed();
                        }
                        plt.scatter(xAxisCastedValues, yAxisCastedValues, c: colorAxisCastedValues, cmap: cmap, vmin: colorAxisCastedValues.Min(), vmax: colorAxisCastedValues.Max());
                        dynamic cbar = plt.colorbar();
                        cbar.set_label(ColorAxisPlotLabel);
                    }
                    else
                    {
                        plt.scatter(xAxisCastedValues, yAxisCastedValues);
                    }

                    plt.xlabel(XAxisPlotLabel);
                    plt.ylabel(YAxisPlotLabel);
                    plt.title(PlotTitle);
                    // Save the plot to a buffer as PNG image
                    dynamic buffer = io.BytesIO();
                    plt.savefig(buffer, dpi: 300);

                    // Convert the buffer to a base64 encoded string
                    dynamic img_str = base64.b64encode(buffer.getvalue());

                    // Close the plot and buffer to release resources
                    plt.clf();
                    buffer.close();

                    // Return the base64 encoded image string
                    string? result = img_str.ToString();
                    result = result.Remove(0, 2);
                    result = result.Replace("'", "");
                    EncodedImage = Convert.FromBase64String(result);

                    CanSavePlotImage = true;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.StackTrace, e.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }                       
        }

        private void loadCsvHeaders(object parameter)
        {            
            string? filePath = parameter.ToString();

            if (filePath == null)
            {
                return;
            }

            Headers.Clear();
            using StreamReader reader = new(File.OpenRead(filePath));
            if (reader.EndOfStream == false)
            {
                var headerLine = reader.ReadLine();

                if (headerLine == null) { return; }

                var headers = headerLine.Split(',');

                foreach (var header in headers)
                {
                    HeaderColumn headerColumn = new()
                    {
                        Header = header
                    };
                    Headers.Add(headerColumn);
                }
                HasParsed = true;
            }
        }

        private static void sanitizeCSV(string filePath)
        {
            List<string> sanitizedLines = new();

            using (StreamReader sr = new(filePath))
            {
                string? line;
                bool insideQuotes = false;

                while ((line = sr.ReadLine()) != null)
                {
                    var sanitizedLine = new StringBuilder(line.Length);

                    for (int i = 0; i < line.Length; i++)
                    {
                        char currentChar = line[i];

                        if (currentChar == '"')
                        {
                            insideQuotes = !insideQuotes;
                        }

                        if (currentChar != ' ' || insideQuotes)
                        {
                            sanitizedLine.Append(currentChar);
                        }
                    }

                    sanitizedLines.Add(sanitizedLine.ToString());
                }
            }

            using StreamWriter sw = new(filePath, false, Encoding.UTF8);
            foreach (string sanitizedLine in sanitizedLines)
            {
                sw.WriteLine(sanitizedLine);
            }
        }

        private static List<Type> getCastableTypes(string input)
        {
            List<Type> castableTypes = new();
            if (input.GetType() == typeof(string))
            {
                castableTypes.Add(typeof(string));
            }

            if (bool.TryParse(input, out _))
            {
                castableTypes.Add(typeof(bool));
            }

            if (sbyte.TryParse(input, out _))
            {
                castableTypes.Add(typeof(sbyte));
            }

            byte byteResult;
            if (byte.TryParse(input, out _))
            {
                castableTypes.Add(typeof(byte));
            }

            short shortResult;
            if (short.TryParse(input, out _))
            {
                castableTypes.Add(typeof(short));
            }

            ushort ushortResult;
            if (ushort.TryParse(input, out _))
            {
                castableTypes.Add(typeof(ushort));
            }

            int intResult;
            if (int.TryParse(input, out _))
            {
                castableTypes.Add(typeof(int));
            }

            uint uintResult;
            if (uint.TryParse(input, out _))
            {
                castableTypes.Add(typeof(uint));
            }

            long longResult;
            if (long.TryParse(input, out _))
            {
                castableTypes.Add(typeof(long));
            }

            ulong ulongResult;
            if (ulong.TryParse(input, out _))
            {
                castableTypes.Add(typeof(ulong));
            }

            float floatResult;
            if (float.TryParse(input, out _))
            {
                castableTypes.Add(typeof(float));
            }

            double doubleResult;
            if (double.TryParse(input, out _))
            {
                castableTypes.Add(typeof(double));
            }

            char charResult;
            if (char.TryParse(input, out _))
            {
                castableTypes.Add(typeof(char));
            }

            DateTime dateTimeResult;
            if (DateTime.TryParse(input, out _))
            {
                castableTypes.Add(typeof(DateTime));
            }

            return castableTypes;
        }

        private void savePlotImage(object parameter)
        {
            if (parameter is not BitmapImage bitmapImage)
            {
                return;
            }

            // Create a file save dialog
            var dialog = new SaveFileDialog();
            dialog.Filter = "Image files (*.png;*.jpeg;*.jpg;*.bmp)|*.png;*.jpeg;*.jpg;*.bmp";
            dialog.DefaultExt = ".png";
            dialog.Title = "Save Image As";

            // Show the dialog and get the result
            bool? result = dialog.ShowDialog();

            // If the user clicked OK, save the image
            if (result == true)
            {
                // Open the file stream and create a new encoder for the image format
                using (FileStream stream = new FileStream(dialog.FileName, FileMode.Create))
                {
                    BitmapEncoder encoder = null;

                    if (dialog.FileName.EndsWith(".png"))
                    {
                        encoder = new PngBitmapEncoder();
                    }
                    else if (dialog.FileName.EndsWith(".jpeg") || dialog.FileName.EndsWith(".jpg"))
                    {
                        encoder = new JpegBitmapEncoder();
                    }
                    else if (dialog.FileName.EndsWith(".bmp"))
                    {
                        encoder = new BmpBitmapEncoder();
                    }

                    // If an encoder was found, encode the bitmap and save it to the stream
                    if (encoder != null)
                    {
                        encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                        encoder.Save(stream);
                    }
                }
            }

        }

        #endregion
    }
}