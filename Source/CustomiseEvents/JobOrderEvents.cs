using Surya.India.Data.Infrastructure;


namespace CustomiseEvents
{
    public class JobOrderEvents
    {

        public void Event_OnHeaderSave (IUnitOfWork _unitOfWork, int HeaderId, string Mode) 
        {
        }
        public void Event_OnHeaderDelete(IUnitOfWork _unitOfWork, int HeaderId)
        {
        }
        public void Event_OnHeaderSubmit(IUnitOfWork _unitOfWork, int HeaderId)
        {
        }
        public void Event_OnHeaderApprove(IUnitOfWork _unitOfWork, int HeaderId)
        {
        }
        public void Event_OnHeaderPrint(IUnitOfWork _unitOfWork, int HeaderId)
        {
        }
        public void Event_OnLineSave(IUnitOfWork _unitOfWork, int HeaderId, int LineId, string Mode)
        {
        }
        public void Event_OnLineDelete(IUnitOfWork _unitOfWork, int HeaderId, int LineId)
        {
        }
        public void Event_AfterHeaderSave(IUnitOfWork _unitOfWork, int HeaderId, string Mode)
        {
        }
        public void Event_AfterHeaderDelete(IUnitOfWork _unitOfWork, int HeaderId)
        {
        }
        public void Event_AfterHeaderSubmit(IUnitOfWork _unitOfWork, int HeaderId)
        {
        }
        public void Event_AfterHeaderApprove(IUnitOfWork _unitOfWork, int HeaderId)
        {
        }
        public void Event_AfterLineSave(IUnitOfWork _unitOfWork, int HeaderId, int LineId, string Mode)
        {
        }
        public void Event_AfterLineDelete(IUnitOfWork _unitOfWork, int HeaderId, int LineId)
        {
        }
    }
}
