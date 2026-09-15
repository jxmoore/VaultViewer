// Resolve WPF vs WinForms type ambiguity introduced by <UseWindowsForms>true</UseWindowsForms>.
// All WPF-native files see the WPF versions by default; code that needs Drawing types uses fully-qualified names.
global using Application = System.Windows.Application;
global using Brush = System.Windows.Media.Brush;
global using Brushes = System.Windows.Media.Brushes;
global using Button = System.Windows.Controls.Button;
global using FontFamily = System.Windows.Media.FontFamily;
global using Clipboard = System.Windows.Clipboard;
global using Color = System.Windows.Media.Color;
