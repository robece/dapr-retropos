using Newtonsoft.Json;
using RetroPOS.App.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;

namespace RetroPOS.App
{
    class SearchProductsWindow : Program
    {
        const int margin = 3;

        static Label lblWarehouseID = null;
        static TextField txtWarehouseID = null;
        static Label lblResult = null;
        static ListView lstResult = null;
        static Label lblSendingData = null;
        static ProgressBar progressBar = null;

        static List<WarehouseProduct> warehouseProducts = null;
        static int selectedResultsIndex = -1;


        public static void Draw()
        {
            Application.Init();

            var top = Application.Top;

            var win = new Window("Products")
            {
                X = 0,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            top.Add(win);

            var menu = new MenuBar(new MenuBarItem[] {
            new MenuBarItem ("_File", new MenuItem [] {
                    new MenuItem ("_Add Product", "", () => {
                        Action action = () => ProductWindow.Draw();
                        running = action;
                        Application.RequestStop();
                    }),
                    null,
                    new MenuItem ("_Close", "", () => {
                        running = MainApp; 
                        Application.RequestStop();
                    }),
                }),
            });
            top.Add(menu);

            lblWarehouseID = new Label(1, 1, "Warehouse ID: ");

            txtWarehouseID = new TextField("")
            {
                X = Pos.Right(lblWarehouseID),
                Y = 1,
                Width = Dim.Percent(40),
                KeyPress = async (e) =>
                {
                    if (e.KeyEvent.Key == Key.Enter)
                    {
                        await WarehouseProducts(txtWarehouseID.Text.ToString());
                    }
                }
            };

            lblResult = new Label("Result: ")
            {
                X = 1,
                Y = Pos.Bottom(txtWarehouseID) + 1
            };

            lstResult = new ListView()
            {
                X = 1,
                Y = Pos.Bottom(lblResult) + 1,
                Width = Dim.Fill() - 1,
                Height = Dim.Fill() - 1
            };

            lstResult.SelectedItemChanged += (ListViewItemEventArgs item) => {
                selectedResultsIndex = item.Item;
            };

            lstResult.OpenSelectedItem += (ListViewItemEventArgs item) => {

                var warehouseProduct = warehouseProducts[selectedResultsIndex];

                Action action = () => ProductWindow.Draw(warehouseProduct);
                running = action;
                Application.RequestStop();
            };

            lblSendingData = new Label("SENDING DATA TO SERVICE...")
            {
                Visible = false
            };

            progressBar = new ProgressBar()
            {
                Width = 10,
                Visible = false
            };

            bool timer(MainLoop caller)
            {
                progressBar.Pulse();
                return true;
            }
            Application.MainLoop.AddTimeout(TimeSpan.FromMilliseconds(300), timer);

            top.LayoutComplete += (e) =>
            {
                lblSendingData.X = win.X;
                lblSendingData.Y = Pos.Bottom(win) - Pos.Top(win) - margin;
                progressBar.X = Pos.Right(lblSendingData);
                progressBar.Y = Pos.Bottom(win) - Pos.Top(win) - margin;
            };

            win.Add(lblWarehouseID, txtWarehouseID, lblResult, lstResult, lblSendingData, progressBar);

            txtWarehouseID.FocusFirst();

            Application.Run();
        }

        async static Task WarehouseProducts(string warehouseID)
        {
            try
            {
                lblSendingData.Visible = true;
                progressBar.Visible = true;

                var request = new WarehouseProductsRequest()
                {
                    WarehouseID = warehouseID
                };

                var jsonPayload = JsonConvert.SerializeObject(request);
                var stringContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                HttpClient httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(15);
                var response = await httpClient.PostAsync($"{Settings.WAREHOUSE_API_BASEURL}/warehouse/warehouseproducts", stringContent);

                if (response.IsSuccessStatusCode)
                {
                    var payload = await response.Content.ReadAsStringAsync();
                    warehouseProducts = JsonConvert.DeserializeObject<List<WarehouseProduct>>(payload);
                    var list = new List<string>();
                    foreach (var p in warehouseProducts)
                    {
                        var productID = (p.ProductID.Length > 20) ? p.ProductID.Substring(0, 20) : p.ProductID;
                        var productName = (p.ProductName.Length > 20) ? p.ProductName.Substring(0, 20) : p.ProductName;
                        var productQuantity = (p.ProductQuantity.Length > 20) ? p.ProductQuantity.Substring(0, 20) : p.ProductQuantity;
                        var productDescription = (p.ProductDescription.Length > 20) ? p.ProductDescription.Substring(0, 20) : p.ProductDescription;

                        list.Add(string.Format("| ProductID: {0,20} | Name: {1,20} | Quantity: {2,20} | Description: {3,20} |", productID, productName, productQuantity, productDescription));
                    }

                    lstResult.SetSource(list);
                }
                else
                {
                    MessageBox.ErrorQuery(50, 7, "Alert", "There was an error retrieving the products", "Accept");
                }
            }
            catch
            {
                MessageBox.ErrorQuery(50, 7, "Alert", "There was an error retrieving the products", "Accept");
            }
            finally
            {
                progressBar.Fraction = 0;
                lblSendingData.Visible = false;
                progressBar.Visible = false;
            }
        }
    }
}
