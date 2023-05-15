using MFilesAPI;
using MfilesDSS.Models;
using static System.Net.Mime.MediaTypeNames;
using System.Net;

namespace MfilesDSS.Services
{
    public class MfilesClass : IMfiles
    {
        public void mfiles(Vault vault,Root root)
        {
            string path = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(),"Download");
            string filepath = System.IO.Path.Combine(path,"Output.pdf");
            try
            {
                WebClient myWebClient = new WebClient();
                myWebClient.DownloadFile(root.filedonwloadLink, filepath);
                

                string useremail = "";
                int fileid = 0;
                // Create our search conditions.
                var searchConditions = new SearchConditions();
                // Add a class filter.
           
                // Add a "not deleted" filter.
                {
                    // Create the condition.
                    var condition = new SearchCondition();

                    // Set the expression.
                    condition.Expression.SetStatusValueExpression(MFStatusType.MFStatusTypeDeleted);

                    // Set the condition.
                    condition.ConditionType = MFConditionType.MFConditionTypeEqual;

                    // Set the value.
                    condition.TypedValue.SetValue(MFilesAPI.MFDataType.MFDatatypeBoolean, false);

                    // Add the condition to the collection.
                    searchConditions.Add(-1, condition);
                }
                // Add a condition that the external id is equal to id
                {
                    // Create the condition.
                    var condition = new SearchCondition();

                    // Set the expression.
                    condition.Expression.DataStatusValueType = MFStatusType.MFStatusTypeExtID;

                    // Set the condition type.
                    condition.ConditionType = MFConditionType.MFConditionTypeEqual;

                    // Set the value.
                    // In this case "MyExternalObjectId" is the ID of the object in the remote system.
                    condition.TypedValue.SetValue(MFDataType.MFDatatypeText, root.objectID.ToString());
                    searchConditions.Add(-1, condition);
                }
                var searchResults = vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(searchConditions,
        MFSearchFlags.MFSearchFlagNone, SortResults: false);

                foreach (ObjectVersion objectVersion in searchResults)
                {
                    vault.ObjectOperations.ForceUndoCheckout(objectVersion.ObjVer);

                    var objID = new MFilesAPI.ObjID();
                    objID.SetIDs(
                        ObjType: (int)MFBuiltInObjectType.MFBuiltInObjectTypeDocument,
                        ID: objectVersion.ObjVer.ID);

                    // Check out the object.
                    var checkedOutObjectVersion = vault.ObjectOperations.CheckOut(objID);
                    //foreach file
                    foreach(ObjectFile objectFile in checkedOutObjectVersion.Files)
                    {
                        vault.ObjectFileOperations.UploadFile(objectFile.ID,objectFile.Version,filepath);
                    }
                    // Check the object back in.
                    vault.ObjectOperations.CheckIn(checkedOutObjectVersion.ObjVer);
                    System.IO.File.Delete(filepath);
                }

            }
            catch (Exception ex)
            {

            }
        }
    }
}
