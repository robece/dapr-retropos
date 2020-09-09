using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace RetroPOS.App
{
    public class Utilities
    {
        public static string GetProductID(string productName)
        {
            var productID = string.Empty;
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.ASCII.GetBytes(productName.ToUpper()));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                productID = builder.ToString();
            }
            return productID;
        }
    }
}
