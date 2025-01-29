using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace BIMoQ
{
    public partial class SelectionWindow : Window
    {
        private UIDocument uidoc;
        private Document doc;
        private Dictionary<BuiltInCategory, List<string>> categoryData;

        public SelectionWindow(UIDocument uidoc)
        {
            InitializeComponent();
            this.uidoc = uidoc;
            this.doc = uidoc.Document;
            LoadCategories();
        }

        private void LoadCategories()
        {
            categoryData = new Dictionary<BuiltInCategory, List<string>>();
            BuiltInCategory[] categories = new BuiltInCategory[]
            {
            BuiltInCategory.OST_Walls,
            BuiltInCategory.OST_Floors,
            BuiltInCategory.OST_Roofs,
            BuiltInCategory.OST_Columns,
            BuiltInCategory.OST_StructuralFraming,
            BuiltInCategory.OST_Doors,
            BuiltInCategory.OST_Windows
            };

            foreach (BuiltInCategory category in categories)
            {
                CategoryComboBox.Items.Add(category.ToString().Replace("OST_", ""));
            }
        }

        private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryComboBox.SelectedItem != null)
            {
                ElementTypeComboBox.Items.Clear();
                string selectedCategory = CategoryComboBox.SelectedItem.ToString();
                BuiltInCategory builtInCategory = (BuiltInCategory)Enum.Parse(
                    typeof(BuiltInCategory),
                    "OST_" + selectedCategory
                );

                FilteredElementCollector collector = new FilteredElementCollector(doc)
                    .OfCategory(builtInCategory)
                    .WhereElementIsElementType();

                foreach (Element type in collector)
                {
                    ElementTypeComboBox.Items.Add(type.Name);
                }
            }
        }

        private void SelectElements_Click(object sender, RoutedEventArgs e)
        {
            if (ElementTypeComboBox.SelectedItem == null) return;

            string selectedType = ElementTypeComboBox.SelectedItem.ToString();
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> elements = collector
                .OfCategory((BuiltInCategory)Enum.Parse(
                    typeof(BuiltInCategory),
                    "OST_" + CategoryComboBox.SelectedItem.ToString()))
                .WhereElementIsNotElementType()
                .ToElements();

            ElementListView.Items.Clear();
            double totalVolume = 0;
            int count = 0;

            foreach (Element element in elements)
            {
                ElementId typeId = element.GetTypeId();
                Element typeElement = doc.GetElement(typeId);
                string typeName = typeElement?.Name;

                if (typeName == selectedType)
                {
                    count++;
                    Parameter volumeParam = element.LookupParameter("Volume");
                    double volume = 0;
                    if (volumeParam != null && volumeParam.StorageType == StorageType.Double)
                    {
                        volume = UnitUtils.ConvertFromInternalUnits(
                            volumeParam.AsDouble(),
                            UnitTypeId.CubicMeters
                        );
                        totalVolume += volume;
                    }

                    ElementListView.Items.Add(new
                    {
                        ElementId = element.Id.IntegerValue,
                        Volume = volume.ToString("F2")
                    });
                }
            }

            SummaryBlock.Text = $"Selected Type: {selectedType}\n" +
                                $"Count: {count}\n" +
                                $"Total Volume: {totalVolume:F2} m³";
        }

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            CategoryComboBox.SelectedIndex = -1;
            ElementTypeComboBox.Items.Clear();
            ElementListView.Items.Clear();
            SummaryBlock.Text = string.Empty;
        }


        private void OK_Click(object sender, RoutedEventArgs e)
        {
            // Clear selections and reset for next selection
            CategoryComboBox.SelectedIndex = -1;
            ElementTypeComboBox.Items.Clear();
        }
    }
}
