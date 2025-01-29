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
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            SelectionWindow window = new SelectionWindow(uidoc);
            window.ShowDialog();
            return Result.Succeeded;
        }
    }
}
