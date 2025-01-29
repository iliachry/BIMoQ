using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BIMoQ
{
    [Transaction(TransactionMode.Manual)]
    public class AnalyzeCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Step 1: Collect all wall types
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> wallTypes = collector.OfClass(typeof(WallType)).ToElements();
            List<string> wallTypeNames = wallTypes.Select(wt => wt.Name).ToList();

            // Step 2: Show WPF window to select a wall type
            WallTypeSelectionWindow selectionWindow = new WallTypeSelectionWindow(wallTypeNames);
            if (selectionWindow.ShowDialog() != true)
                return Result.Cancelled;

            string selectedWallType = selectionWindow.SelectedWallType;

            // Step 3: Analyze walls of the selected type
            ICollection<Element> walls = new FilteredElementCollector(doc).OfClass(typeof(Wall)).ToElements();
            List<string> wallData = new List<string>();
            double totalVolume = 0;
            int count = 0;

            foreach (Element wall in walls)
            {
                ElementId typeId = wall.GetTypeId();
                Element typeElement = doc.GetElement(typeId);
                string typeName = typeElement?.Name;

                if (typeName == selectedWallType)
                {
                    count++;
                    Parameter volumeParam = wall.LookupParameter("Volume");
                    double volume = 0;
                    if (volumeParam != null && volumeParam.StorageType == StorageType.Double)
                    {
                        volume = UnitUtils.ConvertFromInternalUnits(volumeParam.AsDouble(), UnitTypeId.CubicMeters);
                        totalVolume += volume;
                    }
                    wallData.Add($"Wall ID: {wall.Id.IntegerValue}, Volume: {volume} m³");
                }
            }

            // Step 4: Show results in a TaskDialog
            string report = $"Selected Wall Type: {selectedWallType}\n" +
                            $"Count: {count}\n" +
                            $"Total Volume: {totalVolume} m³\n\n" +
                            "Individual Walls:\n" +
                            string.Join("\n", wallData);

            TaskDialog.Show("Wall Properties Report", report);

            return Result.Succeeded;
        }
    }
}
