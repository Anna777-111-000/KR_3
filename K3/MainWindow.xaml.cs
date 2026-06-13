using System;
using System.Collections.Generic;
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
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;

namespace K3
{
    public partial class MainWindow : Window
    {
        string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=3;Integrated Security=True";
        ObservableCollection<ProductCard> products = new ObservableCollection<ProductCard>();

        public MainWindow()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadProducts();
        }

        private void LoadProducts()
        {
            string query = @"
                SELECT 
                    p.ProductsID,
                    p.Artikle,
                    p.Name,
                    pt.ProductType,
                    p.Price,
                    p.Count
                FROM Products p
                JOIN ProductTypes pt ON p.ProductTypeID = pt.ProductTypeID";

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                conn.Open();
                adapter.Fill(dt);
            }

            products.Clear();
            foreach (DataRow row in dt.Rows)
            {
                string productType = row["ProductType"].ToString();
                decimal price = Convert.ToDecimal(row["Price"]);

                // Расчет скидки по типу товара
                int discount = CalculateDiscount(productType);
                decimal priceWithDiscount = price * (1 - (decimal)discount / 100);

                products.Add(new ProductCard
                {
                    ProductID = Convert.ToInt32(row["ProductsID"]),
                    Artikle = row["Artikle"].ToString(),
                    Name = row["Name"].ToString(),
                    ProductType = productType,
                    Price = price,
                    Count = Convert.ToInt32(row["Count"]),
                    DiscountPercent = discount,
                    PriceWithDiscount = priceWithDiscount
                });
            }
            productsControl.ItemsSource = products;
        }

        // Алгоритм расчета скидки
        private int CalculateDiscount(string productType)
        {
            switch (productType.ToLower())
            {
                case "карточная":
                    return 0;
                case "семейная":
                    return 3;
                case "стратегическая":
                    return 6;
                case "детективная":
                    return 9;
                default:
                    return 5;
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Добавление продукции");
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Редактирование продукции");
        }
    }

    public class ProductCard
    {
        public int ProductID { get; set; }
        public string Artikle { get; set; }
        public string Name { get; set; }
        public string ProductType { get; set; }
        public decimal Price { get; set; }
        public int Count { get; set; }
        public int DiscountPercent { get; set; }
        public decimal PriceWithDiscount { get; set; }
    }
}