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
            var tokens = ParseTokens(definition);

            bool ignore = false;

            foreach(var token in tokens)
            {
                string text = token.Text;

                switch(token.Type)
                {                    
                    case TSQLTokenType.SystemIdentifier:
                        //reverse argument and change data type
                        if (text.ToUpper() == "CONVERT")
                        {
                            if (targetDbInterpreter is MySqlInterpreter)
                            {
                                int startIndex = token.BeginPosition;
                                int endIndex = definition.Substring(startIndex).ToUpper().IndexOf("AS") + startIndex;

                                string functionBody = definition.Substring(startIndex, endIndex - startIndex);

                                int leftBracketCount = functionBody.Length - functionBody.Replace("(", "").Length;
                                int rightBracketCount = functionBody.Length - functionBody.Replace(")", "").Length;

                                if (leftBracketCount < rightBracketCount)
                                {
                                    int count = 0;
                                    for (int k = 0; k < functionBody.Length; k++)
                                    {
                                        if (functionBody[k] == ')')
                                        {
                                            count++;
                                            if (count == leftBracketCount)
                                            {
                                                functionBody = functionBody.Substring(0, k + 1);
                                                break;
                                            }
                                        }
                                    }
                                }

                                string mainBody = functionBody.ToUpper().Replace("CONVERT", "");
                                mainBody = mainBody.Substring(0, mainBody.Length - 1).Substring(1);

                                string[] args = mainBody.Split(',');

                                if (sourceDbInterpreter is SqlServerInterpreter)
                                {
                                    string dataType = args[0];

                                    DataTypeMapping dataTypeMapping = dataTypeMappings.FirstOrDefault(item => item.Source.Type?.ToLower() == dataType?.ToLower());
                                    if (dataTypeMapping != null)
                                    {
                                        DataTypeMappingTarget target = dataTypeMapping.Tareget;
                                        string newDataType = target.Type;
                                        if (!string.IsNullOrEmpty(target.Precision) && !string.IsNullOrEmpty(target.Scale))
                                        {
                                            newDataType += $"({target.Precision},{target.Scale})";
                                        }

                                        string newFunctionBody = $"{text}({args[1]},{newDataType})";
                                        definition = definition.Replace(functionBody, newFunctionBody);                                       
                                    }
                                }
                            }
                        }                     
                        break;
                }
            }

            tokens = ParseTokens(definition);
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

        private static List<TSQL.Tokens.TSQLToken> ParseTokens(string sql)
        {
            return TSQLStatementReader.ParseStatements(sql, true, true).FirstOrDefault().Tokens;
        }
    }    
}
