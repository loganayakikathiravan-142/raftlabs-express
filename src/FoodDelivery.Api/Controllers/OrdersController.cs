using FluentValidation;
using FoodDelivery.Application.Common.Models;
using FoodDelivery.Application.DTOs;
using FoodDelivery.Application.Interfaces;
using FoodDelivery.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FoodDelivery.Api.Controllers;

[ApiController]
[Route("api/v1/orders")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IValidator<CreateOrderRequest> _createOrderValidator;
    private readonly IValidator<UpdateOrderStatusRequest> _updateStatusValidator;

    public OrdersController(
        IOrderService orderService,
        IValidator<CreateOrderRequest> createOrderValidator,
        IValidator<UpdateOrderStatusRequest> updateStatusValidator)
    {
        _orderService = orderService;
        _createOrderValidator = createOrderValidator;
        _updateStatusValidator = updateStatusValidator;
    }

    /// <summary>
    /// Creates and places a new food delivery order.
    /// </summary>
    /// <param name="request">Order placement payload with customer details and requested items.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created order with authoritative totals and tracking number.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<OrderDto>> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _createOrderValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var order = await _orderService.CreateOrderAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
    }

    /// <summary>
    /// Retrieves order details by primary internal GUID identifier.
    /// </summary>
    /// <param name="id">Order identifier GUID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The order details.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetOrderById(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetByIdAsync(id, cancellationToken);
        if (order == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Order Not Found",
                Detail = $"Order with identifier '{id}' was not found."
            });
        }

        return Ok(order);
    }

    /// <summary>
    /// Retrieves order tracking information by human-readable unique order number.
    /// </summary>
    /// <param name="orderNumber">Unique tracking number (e.g. ORD-20261013-1234).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The order details.</returns>
    [HttpGet("by-number/{orderNumber}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetOrderByNumber(string orderNumber, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetByOrderNumberAsync(orderNumber, cancellationToken);
        if (order == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Order Not Found",
                Detail = $"Order with tracking number '{orderNumber}' was not found."
            });
        }

        return Ok(order);
    }

    /// <summary>
    /// Retrieves a paginated list of orders with optional status filtering.
    /// </summary>
    /// <param name="page">Page number (default 1).</param>
    /// <param name="pageSize">Page size (default 10, max 100).</param>
    /// <param name="status">Optional status filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated order collection.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<OrderDto>>> GetOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] OrderStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _orderService.GetOrdersAsync(page, pageSize, status, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates the lifecycle status of an order (e.g., Preparing, OutForDelivery, Delivered, Cancelled).
    /// </summary>
    /// <param name="id">Order identifier.</param>
    /// <param name="request">Requested target status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated order details.</returns>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<OrderDto>> UpdateStatus(
        Guid id,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _updateStatusValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var updatedOrder = await _orderService.UpdateStatusAsync(id, request.Status, cancellationToken);
        return Ok(updatedOrder);
    }

    /// <summary>
    /// Updates customer delivery address and contact information if the order is still in OrderReceived state.
    /// </summary>
    /// <param name="id">Order identifier.</param>
    /// <param name="request">Updated contact details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated order.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<OrderDto>> UpdateDetails(
        Guid id,
        [FromBody] UpdateOrderDetailsRequest request,
        CancellationToken cancellationToken)
    {
        var updatedOrder = await _orderService.UpdateDetailsAsync(id, request, cancellationToken);
        return Ok(updatedOrder);
    }

    /// <summary>
    /// Cancels or deletes an order if it is in an eligible non-terminal initial state.
    /// </summary>
    /// <param name="id">Order identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No Content on successful deletion.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DeleteOrder(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _orderService.DeleteOrderAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Order Not Found",
                Detail = $"Order with identifier '{id}' was not found."
            });
        }

        return NoContent();
    }
}
