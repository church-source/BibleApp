using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    class DynListGeneratorFactory
    {
        public static AMenuDynamicOptionSet getDynamicListGenerator(
            string list_generator,
            string target_page)
        {

            //then use reflection to create an instance of it.
            // the string name must be fully qualified for GetType to work
            string objName = list_generator;
            AMenuDynamicOptionSet obj = 
                (AMenuDynamicOptionSet)Activator.CreateInstance(Type.GetType(objName),new String[]{target_page});
            return obj;
        }
    }
}
