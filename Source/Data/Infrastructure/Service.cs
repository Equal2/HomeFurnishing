namespace Data.Infrastructure
{
    public abstract class Service
    {
        public IUnitOfWork UnitOfWork;

        protected Service(IUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork;
        }
    }
}