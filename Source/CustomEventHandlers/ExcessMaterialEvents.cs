using CustomEventArgs;
using Data.Models;
using System;
using ExcessMaterialDocumentEvents;
using System.Linq;
using Core.Common;
using Model.Models;

namespace Jobs.Controllers
{


    public class ExcessMaterialEvents : ExcessMaterialDocEvents
    {

        //For Subscribing Events
        public ExcessMaterialEvents()
        {
            Initialized = true;
            //_beforeHeaderSave += ExcessMaterialEvents__beforeHeaderSave;
            //_onHeaderSave += ExcessMaterialEvents__onHeaderSave;
            //_afterHeaderSave += ExcessMaterialEvents__afterHeaderSave;
            //_beforeHeaderDelete += ExcessMaterialEvents__beforeHeaderDelete;
            //_onLineSave += ExcessMaterialEvents__onLineSave;
            //_onLineDelete += ExcessMaterialEvents__onLineDelete;
            //_beforeLineDelete += ExcessMaterialEvents__beforeLineDelete;
            //_afterHeaderDelete += ExcessMaterialEvents__afterHeaderDelete;
            //_onHeaderDelete += ExcessMaterialEvents__onHeaderDelete;
            //_onLineSaveBulk += ExcessMaterialEvents__onLineSaveBulk;
        }

     
    }
}
