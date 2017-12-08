using Infrastructure.IO;
using Models.BasicSetup.ViewModels;
using Service;
using Services.BasicSetup;
using System;
using System.Collections.Generic;

namespace Services.Customize
{
    public interface ICustomizeComboHelpListService : IDisposable
    {
        ComboBoxPagedResult GetJobWorkers(string searchTerm, int pageSize, int pageNum);
        ComboBoxPagedResult GetJobWorkersWithProcess(string searchTerm, int pageSize, int pageNum, int ProcessId);
        ComboBoxResult GetJobWorkerById(int Id);
        ComboBoxResult GetPersonById(int Id);
        List<ComboBoxResult> GetMultipleJobWorkers(string Id);

        ComboBoxPagedResult GetEmployees(string searchTerm, int pageSize, int pageNum);
        ComboBoxPagedResult GetEmployeesWithProcess(string searchTerm, int pageSize, int pageNum, int ProcessId);
        ComboBoxResult GetEmployeeById(int Id);
        List<ComboBoxResult> GetMultipleEmployees(string Id);
        ComboBoxPagedResult GetGodowns(string searchTerm, int pageSize, int pageNum);
        ComboBoxResult GetGodownById(int Id);
        List<ComboBoxResult> GetMultipleGodowns(string Id);
        ComboBoxPagedResult GetMachines(string searchTerm, int pageSize, int pageNum);
        ComboBoxResult GetMachineById(int Id);
        List<ComboBoxResult> GetMultipleMachines(string Id);
        ComboBoxPagedResult GetDimension1(string searchTerm, int pageSize, int pageNum);
        ComboBoxResult GetDimension1ById(int Id);
        List<ComboBoxResult> GetMultipleDimension1(string Id);
        ComboBoxPagedResult GetDimension2(string searchTerm, int pageSize, int pageNum);
        ComboBoxResult GetDimension2ById(int Id);
        List<ComboBoxResult> GetMultipleDimension2(string Id);
        ComboBoxPagedResult GetProcess(string searchTerm, int pageSize, int pageNum);
        ComboBoxResult GetProcessById(int Id);
        List<ComboBoxResult> GetMultipleProcess(string Id);
        ComboBoxPagedResult GetProduct(string searchTerm, int pageSize, int pageNum);
        ComboBoxResult GetProductById(int Id);
        List<ComboBoxResult> GetMultipleProduct(string Id);
        ComboBoxPagedResult GetPersonWithProcess(string searchTerm, int pageSize, int pageNum, int ProcessId);



    }

    public class CustomizeComboHelpListService : ICustomizeComboHelpListService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomizeComboHelpListService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ComboBoxPagedResult GetJobWorkers(string searchTerm, int pageSize, int pageNum)
        {
            return new JobWorkerService(_unitOfWork).GetList(searchTerm, pageSize, pageNum);
        }

        public ComboBoxPagedResult GetJobWorkersWithProcess(string searchTerm, int pageSize, int pageNum, int ProcessId)
        {
            return new JobWorkerService(_unitOfWork).GetListWithProcess(searchTerm, pageSize, pageNum, ProcessId);
        }

        public ComboBoxResult GetJobWorkerById(int Id)
        {
            return new JobWorkerService(_unitOfWork).GetValue(Id);
        }

        public List<ComboBoxResult> GetMultipleJobWorkers(string Id)
        {
            return new JobWorkerService(_unitOfWork).GetListCsv(Id);
        }

        public ComboBoxPagedResult GetEmployees(string searchTerm, int pageSize, int pageNum)
        {
            return new ComboHelpListService(_unitOfWork).GetEmployees(searchTerm, pageSize, pageNum);
        }

        public ComboBoxPagedResult GetEmployeesWithProcess(string searchTerm, int pageSize, int pageNum, int ProcessId)
        {
            return new ComboHelpListService(_unitOfWork).GetEmployeesWithProcess(searchTerm, pageSize, pageNum, ProcessId);
        }

        public ComboBoxResult GetEmployeeById(int Id)
        {
            return new ComboHelpListService(_unitOfWork).GetEmployee(Id);
        }

        public List<ComboBoxResult> GetMultipleEmployees(string Id)
        {
            return new ComboHelpListService(_unitOfWork).GetMultipleEmployees(Id);
        }

        public ComboBoxPagedResult GetGodowns(string searchTerm, int pageSize, int pageNum)
        {
            return new GodownService(_unitOfWork).GetList(searchTerm, pageSize, pageNum);
        }

        public ComboBoxResult GetGodownById(int Id)
        {
            return new GodownService(_unitOfWork).GetValue(Id);
        }

        public List<ComboBoxResult> GetMultipleGodowns(string Id)
        {
            return new GodownService(_unitOfWork).GetListCsv(Id);
        }

        public ComboBoxPagedResult GetMachines(string searchTerm, int pageSize, int pageNum)
        {
            return new ProductService(_unitOfWork).GetMachineList(searchTerm, pageSize, pageNum);
        }

        public ComboBoxResult GetMachineById(int Id)
        {
            return new ProductService(_unitOfWork).GetValue(Id);
        }

        public List<ComboBoxResult> GetMultipleMachines(string Id)
        {
            return new ProductService(_unitOfWork).GetListCsv(Id);
        }

        public ComboBoxPagedResult GetDimension1(string searchTerm, int pageSize, int pageNum)
        {
            return new Dimension1Service(_unitOfWork).GetList(searchTerm, pageSize, pageNum);
        }

        public ComboBoxResult GetDimension1ById(int Id)
        {
            return new Dimension1Service(_unitOfWork).GetValue(Id);
        }

        public List<ComboBoxResult> GetMultipleDimension1(string Id)
        {
            return new Dimension1Service(_unitOfWork).GetListCsv(Id);
        }

        public ComboBoxPagedResult GetDimension2(string searchTerm, int pageSize, int pageNum)
        {
            return new Dimension2Service(_unitOfWork).GetList(searchTerm, pageSize, pageNum);
        }

        public ComboBoxResult GetDimension2ById(int Id)
        {
            return new Dimension2Service(_unitOfWork).GetValue(Id);
        }

        public List<ComboBoxResult> GetMultipleDimension2(string Id)
        {
            return new Dimension2Service(_unitOfWork).GetListCsv(Id);
        }

        public ComboBoxPagedResult GetProcess(string searchTerm, int pageSize, int pageNum)
        {
            return new ProcessService(_unitOfWork).GetList(searchTerm, pageSize, pageNum);
        }

        public ComboBoxResult GetProcessById(int Id)
        {
            return new ProcessService(_unitOfWork).GetValue(Id);
        }

        public List<ComboBoxResult> GetMultipleProcess(string Id)
        {
            return new ProcessService(_unitOfWork).GetListCsv(Id);
        }

        public ComboBoxPagedResult GetProduct(string searchTerm, int pageSize, int pageNum)
        {
            return new ProductService(_unitOfWork).GetList(searchTerm, pageSize, pageNum);
        }

        public ComboBoxResult GetProductById(int Id)
        {
            return new ProductService(_unitOfWork).GetValue(Id);
        }

        public List<ComboBoxResult> GetMultipleProduct(string Id)
        {
            return new ProductService(_unitOfWork).GetListCsv(Id);
        }
        public ComboBoxPagedResult GetPersonWithProcess(string searchTerm, int pageSize, int pageNum, int ProcessId)
        {
            return new PersonService(_unitOfWork).GetListWithProcess(searchTerm, pageSize, pageNum, ProcessId);
        }
        public ComboBoxResult GetPersonById(int Id)
        {
            return new PersonService(_unitOfWork).GetValue(Id);
        }
        public void Dispose()
        {
            _unitOfWork.Dispose();
        }
    }
}
