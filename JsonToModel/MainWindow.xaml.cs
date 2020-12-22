using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Helpers;
using System.Windows;
using System.Windows.Navigation;

namespace JsonToModel {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private StringBuilder stringBuilder = new StringBuilder();
        private List<(string, dynamic)> innerObjects = new List<(string, dynamic)>();
        private int index = 1;
        private List<string> tempNodes = new List<string>();
        private string result = null;
        public MainWindow() {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            string jsonText = JsonTextBox.Text;
            Converter(jsonText);
            JsonTextBox.Text = result;
            stringBuilder = new StringBuilder();
            innerObjects = new List<(string, dynamic)>();
            index = 1;
            tempNodes = new List<string>();
        }

        private void Converter(string text = null) {
            try {
                dynamic data;
                if (!string.IsNullOrWhiteSpace(text)) {
                    data = Json.Decode(text);
                    stringBuilder.Append("public class Node { \n");

                }
                else {
                    data = innerObjects.First().Item2;
                    stringBuilder.Append(string.Concat("public class ", innerObjects.First().Item1, " { \n"));
                }

                if (data is DynamicJsonArray) {
                    data = data[0];
                }

                foreach (var prop in data) {

                    var pair = new RouteValueDictionary(prop);
                    var key = ((object[])pair.Values)[0];
                    var value = ((object[])pair.Values)[1];
                    if (value is string) {
                        stringBuilder.Append(string.Concat("  public string ", key, " { get; set; } \n"));
                    }
                    else if (value is int) {
                        stringBuilder.Append(string.Concat("  public int ", key, " { get; set; } \n"));
                    }
                    else if (value is decimal) {
                        stringBuilder.Append(string.Concat("  public decimal ", key, " { get; set; } \n"));
                    }
                    else if (value is DynamicJsonArray) {
                        stringBuilder.Append(string.Concat("  public Node", index, "[] ", key, " { get; set; } \n"));
                        innerObjects.Add((string.Concat("Node", index), (dynamic)value));
                        tempNodes.Add(string.Concat("Node", index));
                        index++;
                    }
                    else {
                        stringBuilder.Append(string.Concat("  public Node", index, " ", key, " { get; set; } \n"));
                        innerObjects.Add((string.Concat("Node", index), (dynamic)value));
                        tempNodes.Add(string.Concat("Node", index));
                        index++;
                    }
                }
                if (string.IsNullOrWhiteSpace(text))
                    innerObjects.RemoveAt(0);

                stringBuilder.Append("} \n");

                if (innerObjects.Count(x => x.Item2 != null) > 0)
                    Converter();

                result = stringBuilder.ToString();

                foreach (var tempNode in tempNodes) {
                    if (!result.Contains(string.Concat("public class ", tempNode)))
                        result = result.Replace(tempNode, "string");
                }
            } catch(Exception ex) {
                result = string.Concat($"Unexpected error: {ex.Message}");
            }
            
        }
    }
}
