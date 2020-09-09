using Newtonsoft.Json;
using RetroPOS.App.Models;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;

namespace RetroPOS.App
{
    class ProductWindow : Program
    {
        const int margin = 3;

        static Label lblProductName = null;
        static TextField txtProductName = null;
        static Label lblProductQuantity = null;
        static TextField txtProductQuantity = null;
        static Label lblProductDescription = null;
        static TextField txtProductDescription = null;
        static Label lblWarehouseID = null;
        static TextField txtWarehouseID = null;
        static Label lblProductIdNote = null;
        static Label lblSendingData = null;
        static ProgressBar progressBar = null;

        public static void Draw(Product warehouseProduct = null)
        {
            Application.Init();

            var top = Application.Top;

            var win = new Window("Add Product to Warehouse")
            {
                X = 0,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            top.Add(win);

            var menu = new MenuBar(new MenuBarItem[] {
                new MenuBarItem ("_File", new MenuItem [] {
                    new MenuItem ("_Save & Close", "", async () => {

                        var warehouse = txtWarehouseID.Text.ToString();
                        var productID = Utilities.GetProductID(txtProductName.Text.ToString());
                        var productName = txtProductName.Text.ToString();
                        var productQuantity = txtProductQuantity.Text.ToString();
                        var productDescription = txtProductDescription.Text.ToString();

                        await SaveProduct(warehouse, productID, productName, productQuantity, productDescription);

                    }),
                    new MenuItem ("_Delete", "", async() => {

                        var warehouse = txtWarehouseID.Text.ToString();
                        var productID = Utilities.GetProductID(txtProductName.Text.ToString());

                        await DeleteProduct(warehouse, productID);

                    }, () => (warehouseProduct == null) ? false : true),
                    null,
                    new MenuItem ("_Close", "", () => {
                        Action action = () => SearchProductsWindow.Draw();
                        running = action;
                        Application.RequestStop();
                    }),
                }),
            });
            top.Add(menu);

            lblWarehouseID = new Label("Warehouse ID: ")
            {
                X = 1,
                Y = 1
            };

            txtWarehouseID = new TextField("")
            {
                X = 1,
                Y = Pos.Bottom(lblWarehouseID),
                Width = Dim.Percent(10)
            };

            lblProductName = new Label("Product name: ")
            {
                X = 1,
                Y = Pos.Bottom(txtWarehouseID) + 1
            };

            txtProductName = new TextField("")
            {
                X = 1,
                Y = Pos.Bottom(lblProductName),
                Width = Dim.Percent(20)
            };

            lblProductQuantity = new Label("Quantity: ")
            {
                X = 1,
                Y = Pos.Bottom(txtProductName) + 1
            };

            txtProductQuantity = new TextField("")
            {
                X = 1,
                Y = Pos.Bottom(lblProductQuantity),
                Width = Dim.Percent(10)
            };

            lblProductDescription = new Label("Description: ")
            {
                X = 1,
                Y = Pos.Bottom(txtProductQuantity) + 1
            };

            txtProductDescription = new TextField("")
            {
                X = 1,
                Y = Pos.Bottom(lblProductDescription),
                Width = Dim.Percent(20)
            };

            lblProductIdNote = new Label("NOTE: The product identifier is generated automatically by using a hash of the product name, if you change the name of the product and then save it a new product will be created.")
            {
                X = 1,
                Y = Pos.Bottom(txtProductDescription) + 1
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

            win.Add(lblProductName, txtProductName, lblProductQuantity, txtProductQuantity,
                lblProductDescription, txtProductDescription, lblWarehouseID, txtWarehouseID, lblProductIdNote, lblSendingData, progressBar);

            if (warehouseProduct == null)
            {
                txtWarehouseID.FocusFirst();
            }
            else
            {  
                txtProductName.Text = warehouseProduct.ProductName;
                txtProductQuantity.Text = warehouseProduct.ProductQuantity;
                txtProductDescription.Text = warehouseProduct.ProductDescription;
                txtWarehouseID.Text = warehouseProduct.WarehouseID;
            }

            Application.Run();
        }

        async static Task SaveProduct(string warehouseID, string productID, string productName, string productQuantity, string productDescription)
        {
            try
            {
                lblSendingData.Visible = true;
                progressBar.Visible = true;

                var request = new ProductUpdateRegistrationRequest()
                {
                    WarehouseID = warehouseID,
                    ProductID = productID,
                    ProductName = productName,
                    ProductQuantity = productQuantity,
                    ProductDescription = productDescription
                };

                var jsonPayload = JsonConvert.SerializeObject(request);
                var stringContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                HttpClient httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(15);
                var response = await httpClient.PostAsync($"{Settings.WAREHOUSE_API_BASEURL}/warehouse/productupdateregistration", stringContent);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.ErrorQuery(50, 7, "Alert", "Product has been successfully saved", "Accept");

                    Action action = () => SearchProductsWindow.Draw();
                    running = action;
                    Application.RequestStop();
                }
                else
                {
                    MessageBox.ErrorQuery(50, 7, "Alert", "There was an error saving the product", "Accept");
                }
            }
            catch
            {
                MessageBox.ErrorQuery(50, 7, "Alert", "There was an error saving the product", "Accept");
            }
            finally
            {
                progressBar.Fraction = 0;
                lblSendingData.Visible = false;
                progressBar.Visible = false;
            }
        }

        async static Task DeleteProduct(string warehouseID, string productID)
        {
            try
            {
                lblSendingData.Visible = true;
                progressBar.Visible = true;

                var request = new ProductDeletionRequest()
                {
                    WarehouseID = warehouseID,
                    ProductID = productID
                };

                var jsonPayload = JsonConvert.SerializeObject(request);
                var stringContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                HttpClient httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(15);
                var response = await httpClient.PostAsync($"{Settings.WAREHOUSE_API_BASEURL}/warehouse/productdeletion", stringContent);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.ErrorQuery(50, 7, "Alert", "Product has been successfully deleted", "Accept");

                    Action action = () => SearchProductsWindow.Draw();
                    running = action;
                    Application.RequestStop();
                }
                else
                {
                    MessageBox.ErrorQuery(50, 7, "Alert", "There was an error deleting the product", "Accept");
                }
            }
            catch
            {
                MessageBox.ErrorQuery(50, 7, "Alert", "There was an error deleting the product", "Accept");
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
