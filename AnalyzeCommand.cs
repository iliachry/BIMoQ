using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BIMoQ
{
    [Transaction(TransactionMode.Manual)]
    public class AnalyzeCommand : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            // Step 3: Add your custom logic here

            // 3.1. Access Revit document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> walls = collector.OfClass(typeof(Wall)).ToElements();

            // Step 3: Target specific wall type
            string targetType = "SW01_CIP Wall-300mm_Exterior";
            List<string> wallData = new List<string>();
            double totalVolume = 0;
            int count = 0;

            foreach (Element wall in walls)
            {
                // Get the wall type
                ElementId typeId = wall.GetTypeId();
                Element typeElement = doc.GetElement(typeId);
                string typeName = typeElement?.Name;

                if (typeName == targetType)
                {
                    count++;

                    // Get volume parameter (volume is stored in cubic feet in Revit)
                    Parameter volumeParam = wall.LookupParameter("Volume");
                    double volume = 0;
                    if (volumeParam != null && volumeParam.StorageType == StorageType.Double)
                    {
                        volume = UnitUtils.ConvertFromInternalUnits(volumeParam.AsDouble(), UnitTypeId.CubicMeters);

                        totalVolume += volume;
                    }

                    // Add wall data to the list
                    wallData.Add($"Wall ID: {wall.Id.IntegerValue}, Volume: {volume} m³");
                }
            }

            // Step 4: Show a report
            string report = $"Target Type: {targetType}\n" +
                            $"Count: {count}\n" +
                            $"Total Volume: {totalVolume} m³\n\n" +
                            "Individual Walls:\n" +
                            string.Join("\n", wallData);

            TaskDialog.Show("Wall Properties Report", report);

            return Result.Succeeded;
        }
    }
}
