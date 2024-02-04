﻿using AutoMapper;
using Domus.Common.Helpers;
using Domus.DAL.Interfaces;
using Domus.Domain.Entities;
using Domus.Service.Exceptions;
using Domus.Service.Interfaces;
using Domus.Service.Models;
using Domus.Service.Models.Requests.Base;
using Domus.Service.Models.Requests.Packages;

namespace Domus.Service.Implementations;

public class PackageService : IPackageService
{
    private readonly IPackageRepository _packageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductDetailService _productDetailService;
    private readonly IServiceService _serviceService;
    private readonly IMapper _mapper;

    public PackageService(IPackageRepository packageRepository, IUnitOfWork unitOfWork,
        IProductDetailService productDetailService, IServiceService serviceService, IMapper mapper)
    {
        _packageRepository = packageRepository;
        _unitOfWork = unitOfWork;
        _productDetailService = productDetailService;
        _serviceService = serviceService;
        _mapper = mapper;
    }
    
    public async Task<ServiceActionResult> GetAllPackages()
    {
        var packageList = await _packageRepository.FindAsync(x=> x.IsDeleted == false);
        return new ServiceActionResult()
        {
            IsSuccess = true,
            Data = packageList,
        };
    }

    public async Task<ServiceActionResult> GetPaginatedPackages(BasePaginatedRequest request)
    {
        var packageList = await _packageRepository.FindAsync(x => x.IsDeleted == false);
        var paginatedList = PaginationHelper.BuildPaginatedResult(packageList, request.PageSize, request.PageIndex);
        return new ServiceActionResult()
        {
            IsSuccess = true,
            Data = paginatedList
        };

    }

    public async Task<ServiceActionResult> GetPackage(Guid packageId)
    {
        return new ServiceActionResult()
        {
            IsSuccess = true,
            Data = await _packageRepository.FindAsync(x => x.Id == packageId && x.IsDeleted == false)
        };
    }

    public async Task<ServiceActionResult> CreatePackage(CreatePackageRequest request)
    {
        var serviceList = await _serviceService.GetServices(request.ServiceIds);
        var productDetailList = await _productDetailService.GetProductDetails(request.ProductDetailIds);
        var package = _mapper.Map<Package>(request);
        foreach (var productDetail in productDetailList)
        { 
            package.ProductDetails.Add(productDetail);
        }
        foreach (var service in serviceList)
        {
            package.Services.Add(service);
        }

        await _packageRepository.AddAsync(package);
        await _unitOfWork.CommitAsync();
        return new ServiceActionResult()
        {
            IsSuccess = true,
            Data = package
        };
    }

    public Task<ServiceActionResult> UpdatePackage(CreatePackageRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceActionResult> DeletePackage(Guid packageId)
    {
        throw new NotImplementedException();
    }
}