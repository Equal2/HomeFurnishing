using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Web;
using System.Web.ModelBinding;
using System.Web.Mvc;

using IMvcModelBinder = System.Web.Mvc.IModelBinder;

using MvcModelBindingContext = System.Web.Mvc.ModelBindingContext;


namespace Reports.Presentation.ModelBinder
{
    public class DbGeographyModelBinder : IMvcModelBinder
    {
        public object BindModel(ControllerContext controllerContext, MvcModelBindingContext bindingContext)
        {
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            return BindModelImpl(valueProviderResult != null ? valueProviderResult.AttemptedValue : null);
        }

     

        private DbGeography BindModelImpl(string value)
        {
            if (value == null)
            {
                return (DbGeography)null;
            }
            string[] latLongStr = value.Split(',');
            // TODO: More error handling here.            
            string point = string.Format("POINT ({0} {1})", latLongStr[1], latLongStr[0]);
            //4326 format puts LONGITUDE first then LATITUDE. (IMP)
            DbGeography result = DbGeography.FromText(point, 4326);
            return result;
        }
    }

    public class EFModelBinderProviderMvc : System.Web.Mvc.IModelBinderProvider
    {
        public IMvcModelBinder GetBinder(Type modelType)
        {
            if (modelType == typeof(DbGeography))
                return new DbGeographyModelBinder();
            return null;
        }
    }

  
}