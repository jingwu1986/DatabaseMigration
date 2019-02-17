using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseMigration.Core
{
    public class SchemaInfoHelper
    {
        public static SchemaInfo Clone(SchemaInfo schemaInfo)
        {
            SchemaInfo cloneSchemaInfo =(SchemaInfo) JsonConvert.DeserializeObject(JsonConvert.SerializeObject(schemaInfo), typeof(SchemaInfo));           
            return cloneSchemaInfo;
        }
        public static void TransformOwner(SchemaInfo schemaInfo, string owner)
        {
            schemaInfo.UserDefinedTypes.ForEach(item =>
            {
                item.Owner = owner;
            });
            schemaInfo.Tables.ForEach(item =>
            {
                item.Owner = owner;
            });
            schemaInfo.Columns.ForEach(item =>
            {
                item.Owner = owner;
            });
            schemaInfo.TablePrimaryKeys.ForEach(item =>
            {
                item.Owner = owner;
            });
            schemaInfo.TableForeignKeys.ForEach(item =>
            {
                item.Owner = owner;
            });
            schemaInfo.TableIndices.ForEach(item =>
            {
                item.Owner = owner;
            });
        }

        public static void EnsurePrimaryKeyNameUnique(SchemaInfo schemaInfo)
        {
            List<string> keyNames = new List<string>();
            schemaInfo.TablePrimaryKeys.ForEach(item =>
            {
                if(keyNames.Contains(item.KeyName))
                {
                    item.KeyName = "PK_" + item.TableName;
                }

                keyNames.Add(item.KeyName);
            });
        }

        public static void EnsureIndexNameUnique(SchemaInfo schemaInfo)
        {
            List<string> indexNames = new List<string>();
            schemaInfo.TableIndices.ForEach(item =>
            {
                if (indexNames.Contains(item.IndexName))
                {
                    string columnNames = string.Join("_", schemaInfo.TableIndices.Where(t => t.Owner == item.Owner && t.TableName == item.TableName).Select(t => t.ColumnName));

                    item.IndexName = $"IX_{item.TableName}_{columnNames}";
                }

                indexNames.Add(item.IndexName);
            });
        }

        public static void RistrictNameLength(SchemaInfo schemaInfo, int maxLength)
        {
            schemaInfo.TablePrimaryKeys.ForEach(item =>
            {
                if(item.KeyName.Length>maxLength)
                {
                    item.KeyName = item.KeyName.Substring(0, maxLength);
                }               
            });

            schemaInfo.TableForeignKeys.ForEach(item =>
            {
                if (item.KeyName.Length > maxLength)
                {
                    item.KeyName = item.KeyName.Substring(0, maxLength);
                }
            });

            schemaInfo.TableIndices.ForEach(item =>
            {
                if (item.IndexName.Length > maxLength)
                {
                    item.IndexName = item.IndexName.Substring(0, maxLength);
                }
            });
        }
    }
}
