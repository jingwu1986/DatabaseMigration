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
                           .Replace(sourceDbInterpreter.QuotationRightChar, '"')
                           .Replace("<>", "!=")
                           .Replace(">", " > ")
                           .Replace("<", " < ")
                           .Replace("!=", "<>");


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
            bool hasChanged = false;
            foreach (var token in tokens)
            {
                string text = token.Text;

                int startIndex = -1;
                int endIndex = -1;
                int leftBracketCount = 0;
                int rightBracketCount = 0;
                string dataType = "";
                string newDataType = "";

                switch (token.Type)
                {
                    case TSQLTokenType.SystemIdentifier:                      

                        switch (text.ToUpper())
                        {
                            case "CONVERT":
                                startIndex = token.BeginPosition;
                                endIndex = definition.Substring(startIndex).ToUpper().IndexOf("AS") + startIndex;

                                string functionBody = definition.Substring(startIndex, endIndex - startIndex);

                                leftBracketCount = functionBody.Length - functionBody.Replace("(", "").Length;
                                rightBracketCount = functionBody.Length - functionBody.Replace(")", "").Length;

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

                                string mainBody = Regex.Replace(functionBody, "CONVERT", "", RegexOptions.IgnoreCase);
                                mainBody = mainBody.Substring(0, mainBody.Length - 1).Substring(1);

                                string[] args = mainBody.Split(',');
                                string expression = "";                                
                                dataType = "";
                                
                                if(sourceDbInterpreter is SqlServerInterpreter)
                                {
                                    dataType = args[0];
                                    expression = args[1];
                                }
                                else if(sourceDbInterpreter is MySqlInterpreter)
                                {
                                    dataType = args[1];
                                    expression = args[0];
                                }
                                
                                newDataType = GetNewDataType(dataTypeMappings, dataType);
                                
                                string newFunctionBody = "";                                

                                if (targetDbInterpreter is OracleInterpreter)
                                {
                                    newFunctionBody = $"CAST({expression} AS {newDataType})";
                                }
                                else if
                                (
                                    (sourceDbInterpreter is SqlServerInterpreter || sourceDbInterpreter is MySqlInterpreter)
                                    &&
                                    (targetDbInterpreter is SqlServerInterpreter || targetDbInterpreter is MySqlInterpreter)
                                )
                                {
                                    newFunctionBody = ExchangeFunctionArgs(text,  newDataType, expression);
                                }

                                definition = definition.Replace(functionBody, newFunctionBody);

                                hasChanged = true;
                                break;                           
                        }

                        break;

                    case TSQLTokenType.Identifier:
                        switch(text.ToUpper())
                        {
                            case "CAST":
                                startIndex = token.BeginPosition;
                                int asIndex = startIndex + definition.Substring(startIndex).ToUpper().IndexOf(" AS ");
                              
                                string arg = definition.Substring(token.EndPosition + 1, asIndex - startIndex -3);

                                int functionEndIndex = -1;
                                for (int i = asIndex + 3; i < definition.Length; i++)
                                {
                                    if (definition[i] == '(')
                                    {
                                        leftBracketCount++;
                                    }
                                    else if (definition[i] == ')')
                                    {
                                        rightBracketCount++;
                                    }

                                    if (rightBracketCount - leftBracketCount == 1)
                                    {
                                        dataType = definition.Substring(asIndex + 4, i - asIndex-4);
                                        functionEndIndex = i;
                                        break;
                                    }
                                }

                                newDataType = GetNewDataType(dataTypeMappings, dataType);

                                definition = definition.Replace(dataType, newDataType);

                                hasChanged = true;

                                break;
                        }

                        break;
                }
            }

            if (hasChanged)
            {
                tokens = ParseTokens(definition);
            }

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
                    case TSQLTokenType.Keyword:
                        switch (text.ToUpper())
                        {
                            case "AS":
                                if (targetDbInterpreter is OracleInterpreter)
                                {
                                    var previousKeyword = (from t in tokens where t.Type == TSQLTokenType.Keyword && t.EndPosition < token.BeginPosition select t).LastOrDefault();
                                    if (previousKeyword != null && previousKeyword.Text.ToUpper() == "FROM")
                                    {
                                        continue;
                                    }
                                }
                                break;
                        }
                        sb.Append(token.Text);
                        break;
                    default:
                        sb.Append(token.Text);
                        break;
                }
            }

            definition = sb.ToString();
            return definition;
        }

        private static string ExchangeFunctionArgs(string functionName, string args1, string args2)
        {
            string newFunctionBody = $"{functionName}({args2},{args1})";

            return newFunctionBody;
        }

        private static DataTypeMapping GetDataTypeMapping(List<DataTypeMapping> mappings, string dataType)
        {
            return mappings.FirstOrDefault(item => item.Source.Type?.ToLower() == dataType?.ToLower());
        }

        private static string GetNewDataType(List<DataTypeMapping> mappings, string dataType)
        {
            string cleanDataType = dataType.Split('(')[0];
            string newDataType = cleanDataType;
            bool hasPrecisionScale = false;

            if(cleanDataType != dataType)
            {
                hasPrecisionScale = true;
            }

            DataTypeMapping mapping = GetDataTypeMapping(mappings, cleanDataType);
            if(mapping!=null)
            {
                DataTypeMappingTarget targetDataType = mapping.Tareget;
                newDataType = targetDataType.Type;
                if (!hasPrecisionScale && !string.IsNullOrEmpty(targetDataType.Precision) && !string.IsNullOrEmpty(targetDataType.Scale))
                {
                    newDataType += $"({targetDataType.Precision},{targetDataType.Scale})";
                }
                else if(hasPrecisionScale)
                {
                    newDataType += "(" + dataType.Split('(')[1];
                }
            }

            return newDataType;
        }

        private static List<TSQL.Tokens.TSQLToken> ParseTokens(string sql)
        {
            return TSQLStatementReader.ParseStatements(sql, true, true).FirstOrDefault().Tokens;
        }
    }
}
