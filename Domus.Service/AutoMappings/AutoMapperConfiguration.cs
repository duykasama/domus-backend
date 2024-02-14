using AutoMapper;
using Domus.Domain.Dtos;
using Domus.Domain.Dtos.Articles;
using Domus.Domain.Dtos.Products;
using Domus.Domain.Dtos.Quotations;
using Domus.Domain.Entities;
using Domus.Service.Models.Requests.Articles;
using Domus.Service.Models.Requests.Authentication;
using Domus.Service.Models.Requests.ProductDetails;
using Domus.Service.Models.Requests.Products;
using Domus.Service.Models.Requests.Quotations;
using Domus.Service.Models.Requests.Services;
using Domus.Service.Models.Requests.Users;

namespace Domus.Service.AutoMappings;

public static class AutoMapperConfiguration
{
	public static void RegisterMaps(IMapperConfigurationExpression mapper)
	{
		CreateUserMaps(mapper);

		CreateArticleMaps(mapper);
		
		CreateProductMaps(mapper);

		CreateServiceMaps(mapper);

		CreateQuotationMaps(mapper);
	}

	private static void CreateUserMaps(IMapperConfigurationExpression mapper)
	{
		mapper.CreateMap<RegisterRequest, DomusUser>()
			.ForMember(dest => dest.UserName, opt => opt.MapFrom((src) => src.Email));
		mapper.CreateMap<LoginRequest, DomusUser>();
		mapper.CreateMap<CreateUserRequest, DomusUser>();
		mapper.CreateMap<DomusUser, DtoDomusUser>()
			.ForMember(dest => dest.Name, opt => opt.MapFrom((src) => src.UserName));
	}

	private static void CreateArticleMaps(IMapperConfigurationExpression mapper)
	{
		mapper.CreateMap<CreateArticleRequest, Article>();
		mapper.CreateMap<Article, DtoArticle>();
		mapper.CreateMap<Article, DtoArticleWithoutCategory>();

		mapper.CreateMap<ArticleCategory, DtoArticleCategory>();
		mapper.CreateMap<ArticleImage, DtoArticleImage>();
	}
	
	private static void CreateProductMaps(IMapperConfigurationExpression mapper)
	{
		mapper.CreateMap<Product, DtoProduct>();
		mapper.CreateMap<Product, DtoProductWithoutCategory>();
		mapper.CreateMap<Product, DtoProductWithoutCategoryAndDetails>();
		mapper.CreateMap<CreateProductRequest, Product>();
		mapper.CreateMap<ProductCategory, DtoProductCategory>();
		mapper.CreateMap<ProductDetailQuotation, DtoProductDetailQuotation>()
			.ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductDetail.Product.ProductName));
		mapper.CreateMap<ProductDetail, DtoProductDetail>()
			.ForMember(dest => dest.DisplayPrice, opt => opt.MapFrom(src => Math.Round(src.DisplayPrice, 2)))
			.ForMember(dest => dest.ProductAttributeValues, opt => opt.MapFrom((src) => src.ProductAttributeValues.Select(pav => new DtoProductAttributeValue { Name = pav.ProductAttribute.AttributeName, Value = pav.Value, ValueType = pav.ValueType })));
		mapper.CreateMap<CreateProductDetailRequest, ProductDetail>();
		mapper.CreateMap<ProductImage, DtoProductImage>();
	}

	private static void CreateServiceMaps(IMapperConfigurationExpression mapper)
	{
		mapper.CreateMap<Domain.Entities.Service, DtoService>().ReverseMap();
		mapper.CreateMap<CreateServiceRequest,Domain.Entities.Service>();
		mapper.CreateMap<UpdateServiceRequest, Domain.Entities.Service>();
	}

	private static void CreateQuotationMaps(IMapperConfigurationExpression mapper)
	{
		mapper.CreateMap<CreateQuotationRequest, Quotation>()
			.ForMember(dest => dest.Services, opt => opt.Ignore());
		mapper.CreateMap<Quotation, DtoQuotation>()
			.ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.UserName))
			.ForMember(dest => dest.StaffName, opt => opt.MapFrom(src => src.Staff.UserName));
		mapper.CreateMap<Quotation, DtoQuotationFullDetails>();
		mapper.CreateMap<QuotationNegotiationLog, DtoQuotationNegotiationLog>();
		mapper.CreateMap<QuotationNegotiationLog, DtoQuotationNegotiationLogWithoutMessages>();
		mapper.CreateMap<CreateNegotiationMessageRequest, NegotiationMessage>()
			.ForMember(dest => dest.SentAt, opt => opt.MapFrom(src => DateTime.Now));
		mapper.CreateMap<NegotiationMessage, DtoNegotiationMessage>();
	}
}
