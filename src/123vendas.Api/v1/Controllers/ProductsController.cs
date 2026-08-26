using _123vendas.Application.DTOs.Common;
using _123vendas.Application.DTOs.Products;
using _123vendas.Application.Mappers.Products;
using _123vendas.Domain.Base;
using _123vendas.Domain.Interfaces.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace _123vendas_server.v1.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class ProductsController(IProductService productService) : ControllerBase
{

    /// <summary>
    /// Retrieves a paged list of products based on the provided filters.
    /// </summary>
    /// <param name="request">The request containing filters for retrieving products.</param>
    /// <returns>A paged response containing a list of products.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseDTO<ProductGetResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<PagedResponseDTO<ProductGetResponseDTO>>> GetAsync([FromQuery] ProductGetRequestDTO request)
    {
        var pagedResult = await productService.GetAllAsync(
            request.Id,
            request.IsActive,
            request.Title,
            request.Category,
            request.MinPrice,
            request.MaxPrice,
            request.StartDate,
            request.EndDate,
            request.Page,
            request.Size,
            request.OrderByClause);

        if (pagedResult?.Items is not null && pagedResult.Items.Any())
            return Ok(new PagedResponseDTO<ProductGetResponseDTO>(pagedResult.Items.ToDTO(), pagedResult.Total, request.Page, request.Size));

        return NoContent();
    }

    /// <summary>
    /// Retrieves all available product categories.
    /// </summary>
    /// <returns>A list of product categories.</returns>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public ActionResult<IEnumerable<string>> GetAllCategories()
    {
        var categories = productService.GetAllCategories();

        if (categories is not null && categories.Any())
            return Ok(categories);

        return NoContent();
    }

    /// <summary>
    /// Retrieves detailed information about a product by its ID.
    /// </summary>
    /// <param name="id">The ID of the product.</param>
    /// <returns>The detailed information of the product.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductGetDetailResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductGetDetailResponseDTO>> GetAsync([FromRoute] int id)
    {
        var product = await productService.GetByIdAsync(id);

        if (product is null)
            return NotFound();

        var response = product.ToDetailDTO();

        return Ok(response);
    }

    /// <summary>
    /// Retrieves a paged list of products filtered by category.
    /// </summary>
    /// <param name="category">The category of the products.</param>
    /// <param name="request">The request containing pagination and sorting details.</param>
    /// <returns>A paged response containing a list of products in the specified category.</returns>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(PagedResponseDTO<ProductGetResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<PagedResponseDTO<ProductGetResponseDTO>>> GetByCategoryAsync(string category, [FromQuery] PagedRequestDTO request)
    {
        var pagedResult = await productService.GetAllAsync(
            category: category,
            page: request.Page,
            maxResults: request.Size,
            orderByClause: request.OrderByClause);

        if (pagedResult?.Items is not null && pagedResult.Items.Any())
            return Ok(new PagedResponseDTO<ProductGetResponseDTO>(pagedResult.Items.ToDTO(), pagedResult.Total, request.Page, request.Size));

        return NoContent();
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="request">The product data to create.</param>
    /// <returns>The response containing the created product details.</returns>
    [Authorize(Policy = "ManagerOnly")]
    [HttpPost]
    [ProducesResponseType(typeof(ProductPostResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductPostResponseDTO>> PostAsync([FromBody] ProductPostRequestDTO request)
    {
        var createdProduct = await productService.CreateAsync(request.ToEntity());

        var response = createdProduct.ToPostResponseDTO();

        return Created(string.Empty, response);
    }

    /// <summary>
    /// Updates an existing product by its ID.
    /// </summary>
    /// <param name="id">The ID of the product to update.</param>
    /// <param name="request">The product data to update.</param>
    /// <returns>The response containing the updated product details.</returns>
    [Authorize(Policy = "ManagerOnly")]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProductPutResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductPutResponseDTO>> PutAsync([FromRoute] int id, [FromBody] ProductPutRequestDTO request)
    {
        var product = await productService.UpdateAsync(id, request.ToEntity());

        return Ok(product.ToPutResponseDTO());
    }

    /// <summary>
    /// Deletes a product by its ID.
    /// </summary>
    /// <param name="id">The ID of the product to delete.</param>
    /// <returns>No content response.</returns>
    [Authorize(Policy = "ManagerOnly")]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        await productService.DeleteAsync(id);

        return NoContent();
    }
}