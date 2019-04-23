using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseMigration.Core
{
    public class ViewHelper
    {
        public static List<View> ResortViews(List<View> views)
        {
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
            return views;
        }
    }
}
