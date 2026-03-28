using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Asp.Versioning.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using TechVault.API.Payments;
using TechVault.API.Services;

namespace TechVault.API.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/payments")]
public sealed class PaymentController(
    IPaymentService paymentService,
    IOptions<StripeSettings> stripeOptions) : ControllerBase
{
    [Authorize]
    [HttpPost("create-intent")]
    [ProducesResponseType(typeof(PaymentIntentCreateResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PaymentIntentCreateResult>> CreateIntent(
        [FromBody] CreatePaymentIntentRequestDto dto,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var result = await paymentService.CreatePaymentIntentAsync(
                dto.Amount,
                dto.OrderId,
                userId,
                cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpPost("confirm")]
    [ProducesResponseType(typeof(PaymentIntent), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PaymentIntent>> Confirm(
        [FromBody] ConfirmPaymentRequestDto dto,
        CancellationToken cancellationToken)
    {
        if (ResolveUserId() is null)
        {
            return Unauthorized();
        }

        try
        {
            var intent = await paymentService.ConfirmPaymentAsync(
                dto.PaymentIntentId,
                dto.PaymentMethodId,
                cancellationToken);
            return Ok(intent);
        }
        catch (StripeException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [AllowAnonymous]
    [HttpPost("webhook")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Webhook(CancellationToken cancellationToken)
    {
        var webhookSecret = stripeOptions.Value.WebhookSecret;
        if (string.IsNullOrWhiteSpace(webhookSecret))
        {
            return BadRequest();
        }

        Request.EnableBuffering();
        string json;
        using (var reader = new StreamReader(Request.Body, leaveOpen: true))
        {
            json = await reader.ReadToEndAsync(cancellationToken);
        }

        Request.Body.Position = 0;
        var signature = Request.Headers["Stripe-Signature"].ToString();
        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(json, signature, webhookSecret);
        }
        catch (StripeException)
        {
            return BadRequest();
        }

        await paymentService.HandleWebhookAsync(stripeEvent, cancellationToken);
        return Ok();
    }

    private string? ResolveUserId() =>
        User.FindFirstValue("UserId")
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
}
