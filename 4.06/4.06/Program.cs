using System;
using System.Xml;
using System.Collections.Generic;

namespace OrderApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string fileName = "Orders.xml";

            // 1. Создание XML-файла с помощью XmlTextWriter
            CreateOrdersXml(fileName);

            // 2. Чтение и вывод с помощью XmlDocument
            Console.WriteLine("=== Чтение через XmlDocument ===");
            ReadWithXmlDocument(fileName);

            // 3. Чтение и вывод с помощью XmlTextReader
            Console.WriteLine("\n=== Чтение через XmlTextReader ===");
            ReadWithXmlTextReader(fileName);

            Console.ReadKey();
        }

        // Метод для создания XML-файла с заказами
        static void CreateOrdersXml(string fileName)
        {
            XmlTextWriter writer = null;
            try
            {
                writer = new XmlTextWriter(fileName, System.Text.Encoding.UTF8);
                writer.Formatting = Formatting.Indented; // для читаемости
                writer.WriteStartDocument();

                // Корневой элемент <Orders>
                writer.WriteStartElement("Orders");

                // Заказ №1
                writer.WriteStartElement("Order");
                writer.WriteAttributeString("Id", "1001");
                writer.WriteElementString("Date", "2025-03-15");
                writer.WriteElementString("Customer", "Иванов Иван");

                // Товары первого заказа
                writer.WriteStartElement("Products");
                writer.WriteStartElement("Product");
                writer.WriteElementString("Name", "Ноутбук");
                writer.WriteElementString("Quantity", "1");
                writer.WriteElementString("Price", "75000");
                writer.WriteEndElement();

                writer.WriteStartElement("Product");
                writer.WriteElementString("Name", "Мышь");
                writer.WriteElementString("Quantity", "2");
                writer.WriteElementString("Price", "1500");
                writer.WriteEndElement();
                writer.WriteEndElement(); // Products
                writer.WriteEndElement(); // Order

                // Заказ №2
                writer.WriteStartElement("Order");
                writer.WriteAttributeString("Id", "1002");
                writer.WriteElementString("Date", "2025-03-16");
                writer.WriteElementString("Customer", "Петрова Мария");

                writer.WriteStartElement("Products");
                writer.WriteStartElement("Product");
                writer.WriteElementString("Name", "Книга 'C# в действии'");
                writer.WriteElementString("Quantity", "1");
                writer.WriteElementString("Price", "1200");
                writer.WriteEndElement();

                writer.WriteStartElement("Product");
                writer.WriteElementString("Name", "Ручка");
                writer.WriteElementString("Quantity", "5");
                writer.WriteElementString("Price", "50");
                writer.WriteEndElement();
                writer.WriteEndElement(); // Products
                writer.WriteEndElement(); // Order

                writer.WriteEndElement(); // Orders
                writer.WriteEndDocument();

                Console.WriteLine($"XML-файл '{fileName}' успешно создан.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при создании XML: " + ex.Message);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

        // Чтение с помощью XmlDocument (DOM)
        static void ReadWithXmlDocument(string fileName)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(fileName);

                // Получаем корневой элемент Orders
                XmlNode root = doc.DocumentElement;

                foreach (XmlNode orderNode in root.ChildNodes)
                {
                    // Проверяем, что узел - элемент Order
                    if (orderNode.NodeType == XmlNodeType.Element && orderNode.Name == "Order")
                    {
                        string orderId = orderNode.Attributes["Id"].Value;
                        string date = orderNode["Date"].InnerText;
                        string customer = orderNode["Customer"].InnerText;

                        Console.WriteLine($"Заказ №{orderId}, Дата: {date}, Клиент: {customer}");

                        // Получаем список товаров
                        XmlNode productsNode = orderNode["Products"];
                        if (productsNode != null)
                        {
                            Console.WriteLine("  Товары:");
                            foreach (XmlNode productNode in productsNode.ChildNodes)
                            {
                                if (productNode.Name == "Product")
                                {
                                    string name = productNode["Name"].InnerText;
                                    string quantity = productNode["Quantity"].InnerText;
                                    string price = productNode["Price"].InnerText;
                                    Console.WriteLine($"    - {name}, кол-во: {quantity}, цена: {price} руб.");
                                }
                            }
                        }
                        Console.WriteLine();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при чтении XmlDocument: " + ex.Message);
            }
        }

        // Чтение с помощью XmlTextReader (потоковый)
        static void ReadWithXmlTextReader(string fileName)
        {
            XmlTextReader reader = null;
            try
            {
                reader = new XmlTextReader(fileName);
                reader.WhitespaceHandling = WhitespaceHandling.None; // игнорируем пустые пробелы

                string currentOrderId = "";
                string currentDate = "";
                string currentCustomer = "";
                bool insideProducts = false;

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == "Order")
                        {
                            currentOrderId = reader.GetAttribute("Id");
                        }
                        else if (reader.Name == "Date")
                        {
                            currentDate = reader.ReadElementString();
                        }
                        else if (reader.Name == "Customer")
                        {
                            currentCustomer = reader.ReadElementString();
                        }
                        else if (reader.Name == "Products")
                        {
                            insideProducts = true;
                            // Выводим информацию о заказе, когда начинаем читать товары
                            if (!string.IsNullOrEmpty(currentOrderId))
                            {
                                Console.WriteLine($"Заказ №{currentOrderId}, Дата: {currentDate}, Клиент: {currentCustomer}");
                                Console.WriteLine("  Товары:");
                            }
                        }
                        else if (reader.Name == "Product" && insideProducts)
                        {
                            // Читаем вложенные элементы товара
                            string productName = "";
                            string quantity = "";
                            string price = "";

                            while (reader.Read())
                            {
                                if (reader.NodeType == XmlNodeType.Element)
                                {
                                    if (reader.Name == "Name")
                                        productName = reader.ReadElementString();
                                    else if (reader.Name == "Quantity")
                                        quantity = reader.ReadElementString();
                                    else if (reader.Name == "Price")
                                        price = reader.ReadElementString();
                                }
                                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Product")
                                {
                                    break; // выходим из внутреннего цикла, товар считан
                                }
                            }
                            Console.WriteLine($"    - {productName}, кол-во: {quantity}, цена: {price} руб.");
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Products")
                    {
                        insideProducts = false;
                        Console.WriteLine(); // пустая строка после заказа
                        // Сбрасываем данные заказа для следующего
                        currentOrderId = "";
                        currentDate = "";
                        currentCustomer = "";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при чтении XmlTextReader: " + ex.Message);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }
    }
}