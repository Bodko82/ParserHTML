using ClosedXML.Excel;
using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ParserHTML
{
    public partial class MainWindow : Window
    {
        private ProductParser _parser;
        public ObservableCollection<Product> Products { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            _parser = new ProductParser();
            Products = new ObservableCollection<Product>();
            DataContext = this;
        }

        private async void ParseButton_Click(object sender, RoutedEventArgs e)
        {
            var parsedProducts = await _parser.ParseProducts();
            Products = new ObservableCollection<Product>(parsedProducts);

            if (parsedProducts.Count > 0)
            {
                productListView.ItemsSource = Products;
                exportButton.IsEnabled = true;
                exportDirectButton.IsEnabled = true;
            }
        }
        private async void ExportDirectButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel Files|*.xlsx";
            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;

                var parsedProducts = await _parser.ParseProducts();
                ExportToExcel(filePath, parsedProducts);
            }
        }

        private void ScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = (ScrollViewer)sender;
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel Files|*.xlsx";
            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                ExportToExcel(filePath, Products.ToList());
            }
        }
        private void ExportToExcel(string filePath, IEnumerable<Product> productList)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Products");
                var gridView = productListView.View as GridView;
                if (gridView != null)
                {
                    for (int i = 0; i < gridView.Columns.Count; i++)
                    {
                        GridViewColumn column = gridView.Columns[i];
                        worksheet.Cell(1, i + 1).Value = column.Header.ToString();
                    }
                    var rowIndex = 2;
                    foreach (var product in productList)
                    {
                        for (int j = 0; j < gridView.Columns.Count; j++)
                        {
                            GridViewColumn column = gridView.Columns[j];
                            string property = ((Binding)column.DisplayMemberBinding).Path.Path;
                            object value = product.GetType().GetProperty(property).GetValue(product);
                            worksheet.Cell(rowIndex, j + 1).Value = value?.ToString();
                        }
                        rowIndex++;
                    }
                }

                workbook.SaveAs(filePath);
            }
        }

    }
}
