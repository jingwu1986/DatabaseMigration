using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using TSQL;
using TSQL.Tokens;

namespace DatabaseMigration.Core
{
    public class ViewTranslator
    {
        public static List<View> Translate(List<View> views, DbInterpreter sourceDbInterpreter, DbInterpreter targetDbInterpreter, string targetOwnerName = null)
        {
            if (sourceDbInterpreter.DatabaseType == targetDbInterpreter.DatabaseType)
            {
                return views;
            }

            string configRootFolder = Path.Combine(PathHelper.GetAssemblyFolder(), "Configs");
            string functionMappingFilePath = Path.Combine(configRootFolder, "FunctionMapping.xml");

            #region DataType Mapping
            string dataTypeMappingFilePath = Path.Combine(configRootFolder, $"DataTypeMapping/{sourceDbInterpreter.DatabaseType.ToString()}2{targetDbInterpreter.DatabaseType.ToString()}.xml");
            XDocument dataTypeMappingDoc = XDocument.Load(dataTypeMappingFilePath);

            List<DataTypeMapping> dataTypeMappings = dataTypeMappingDoc.Root.Elements("mapping").Select(item =>
             new DataTypeMapping()
             {
                 Source = new DataTypeMappingSource(item),
                 Tareget = new DataTypeMappingTarget(item)
             })
             .ToList();
            #endregion

            #region Function Mapping
            XDocument functionMappingDoc = XDocument.Load(functionMappingFilePath);
            List<IEnumerable<FunctionMapping>> functionMappings = functionMappingDoc.Root.Elements("mapping").Select(item =>
            item.Elements().Select(t => new FunctionMapping() { DbType = t.Name.ToString(), Function = t.Value }))
            .ToList();
            #endregion  

            string sourceOwnerName = DbInterpreterHelper.GetOwnerName(sourceDbInterpreter);

            if (string.IsNullOrEmpty(targetOwnerName))
            {
                if (targetDbInterpreter is SqlServerInterpreter)
                {
                    targetOwnerName = "dbo";
                }
                else
                {
                    targetOwnerName = DbInterpreterHelper.GetOwnerName(targetDbInterpreter);
                }
            }

            #region Sort Views           
            for (int i = 0; i < views.Count - 1; i++)
            {
                for (int j = i + 1; j < views.Count - 1; j++)
                {
                    if (views[i].Definition.Contains(views[j].Name))
                    {
                        var temp = views[j];
                        views[j] = views[i];
                        views[i] = temp;
                    }
                }
            }
            #endregion

            foreach (View view in views)
            {
                string ownerNameWithQuotation = $"{targetDbInterpreter.QuotationLeftChar}{targetOwnerName}{targetDbInterpreter.QuotationRightChar}";
                string viewNameWithQuotation = $"{targetDbInterpreter.QuotationLeftChar}{view.Name}{targetDbInterpreter.QuotationRightChar}";

                string definition = view.Definition;

                definition = definition
                           .Replace(sourceDbInterpreter.QuotationLeftChar, '"')
                           .Replace(sourceDbInterpreter.QuotationRightChar, '"');               

                definition = ParseDefinition(definition, sourceDbInterpreter, targetDbInterpreter, sourceOwnerName, dataTypeMappings, functionMappings);

                string createAsClause = $"create view {targetOwnerName}.{viewNameWithQuotation} as ";

                if (!definition.Trim().ToLower().StartsWith("create"))
                {
                    definition = createAsClause + Environment.NewLine + definition;
                }
                else
                {
                    int asIndex = definition.ToLower().IndexOf("as");
                    definition = createAsClause + definition.Substring(asIndex + 2);
                }

                view.Definition = definition;
            }

            return views;
        }

        private static string ParseDefinition(string definition, DbInterpreter sourceDbInterpreter, DbInterpreter targetDbInterpreter, string sourceOwnerName, List<DataTypeMapping> dataTypeMappings, List<IEnumerable<FunctionMapping>> functionMappings)
        {
            StringBuilder sb = new StringBuilder();
            var tokens = TSQLStatementReader.ParseStatements(definition, true, true).FirstOrDefault().Tokens;

            bool ignore = false;

            for (int i = 0; i < tokens.Count; i++)
            {
                if (ignore)
                {
                    ignore = false;
                    continue;
                }

                var token = tokens[i];

                var tokenType = token.Type;
                string text = token.Text;

                switch (tokenType)
                {
                    case TSQLTokenType.Identifier:

                        var nextToken = i + 1 < tokens.Count ? tokens[i + 1] : null;

                        //Remove owner name
                        if (nextToken != null && nextToken.Text.Trim() != "(" &&
                            text.Trim('"') == sourceOwnerName && i + 1 < tokens.Count && tokens[i + 1].Text == "."
                            )
                        {
                            ignore = true;
                            continue;
                        }
                        else if (nextToken != null && nextToken.Text.Trim() == "(") //function handle
                        {
                            IEnumerable<FunctionMapping> funcMappings = functionMappings.FirstOrDefault(item => item.Any(t => t.DbType == sourceDbInterpreter.DatabaseType.ToString() && t.Function.Split(',').Any(m => m.ToLower() == text.ToLower())));
                            if (funcMappings != null)
                            {
                                string targetFunction = funcMappings.FirstOrDefault(item => item.DbType == targetDbInterpreter.DatabaseType.ToString())?.Function.Split(',')?.FirstOrDefault();

                                if (!string.IsNullOrEmpty(targetFunction))
                                {
                                    sb.Append(targetFunction);
                                }
                            }
                            else
                            {
                                sb.Append(text);
                            }
                        }
                        else
                        {
                            sb.Append($"{targetDbInterpreter.QuotationLeftChar}{text.Trim('"')}{targetDbInterpreter.QuotationRightChar}");
                        }

                        break;
                    case TSQLTokenType.SystemIdentifier:
                        //reverse parameter and change data type
                        if (text.ToUpper() == "CONVERT")
                        {
                            if (targetDbInterpreter is MySqlInterpreter)
                            {
                                int startIndex = token.BeginPosition;
                                int endIndex = definition.Substring(startIndex).ToUpper().IndexOf("AS") + startIndex;

                                string functionBody = definition.Substring(startIndex, endIndex - startIndex);

                                int leftCount = functionBody.Length - functionBody.Replace("(", "").Length;
                                int rightCount = functionBody.Length - functionBody.Replace(")", "").Length;

                                if(leftCount< rightCount)
                                {
                                    int count = 0;
                                    for(int k= 0;k<functionBody.Length;k++)
                                    {                                       
                                        if(functionBody[k]==')')
                                        {
                                            count++;
                                            if(count==leftCount)
                                            {
                                                functionBody = functionBody.Substring(0, k+1);
                                                break;
                                            }
                                        }
                                    }                                    
                                }

                                string mainBody = functionBody.ToUpper().Replace("CONVERT", "");                                
                                mainBody = mainBody.Substring(0, mainBody.Length-1).Substring(1);

                                string[] parameters = mainBody.Split(',');

                                if (sourceDbInterpreter is SqlServerInterpreter)
                                {
                                    string dataType = parameters[0];

                                    DataTypeMapping dataTypeMapping = dataTypeMappings.FirstOrDefault(item => item.Source.Type?.ToLower() == dataType?.ToLower());
                                    if (dataTypeMapping != null)
                                    {
                                        DataTypeMappingTarget target = dataTypeMapping.Tareget;
                                        string newDataType = target.Type;
                                        if (!string.IsNullOrEmpty(target.Precision) && !string.IsNullOrEmpty(target.Scale))
                                        {
                                            newDataType += $"({target.Precision},{target.Scale})";
                                        }

                                        string newFunctionBody = $"{text}({parameters[1]},{newDataType})";
                                        definition = definition.Replace(functionBody, newFunctionBody);

                                        if(functionBody.ToLower()!=newFunctionBody.ToLower())
                                        {
                                            return ParseDefinition(definition, sourceDbInterpreter, targetDbInterpreter, sourceOwnerName, dataTypeMappings, functionMappings);
                                        }                                        
                                    }
                                }
                            }
                        }

                        sb.Append(text);
                        break;
                    case TSQLTokenType.SingleLineComment:
                    case TSQLTokenType.MultilineComment:
                        continue;
                    default:
                        sb.Append(token.Text);
                        break;
                }
            }

            definition = sb.ToString();
            return definition;
        }
    }    
}
