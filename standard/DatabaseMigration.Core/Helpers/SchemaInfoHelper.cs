using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

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
            var dic = schemaInfo.TableIndices.GroupBy(_ => new { _.Owner, _.TableName, _.IndexName })
                .ToDictionary(_ => _.Key, __ => __.ToList());

            List<string> indexNames = new List<string>();
            foreach (var pair in dic)            {
               
                var indexName = pair.Key.IndexName;
                if (indexNames.Contains(pair.Key.IndexName))
                {
                    string columnNames = string.Join("_", schemaInfo.TableIndices.Where(t => t.Owner == pair.Key.Owner && t.TableName == pair.Key.TableName).Select(t => t.ColumnName));

                    indexName = $"IX_{pair.Key.TableName}_{columnNames}";

                    foreach (var item in schemaInfo.TableIndices)
                    {
                        if (item.TableName == pair.Key.TableName
                            && item.Owner == pair.Key.Owner
                            && item.IndexName == pair.Key.IndexName)
                        {
                            item.IndexName = indexName;
                        }
                    }
                }

                indexNames.Add(indexName);
            }
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
