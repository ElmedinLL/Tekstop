# API versioning

All JSON endpoints use a **URL segment** after `/api/`: **`/api/v1/...`**, **`/api/v2/...`**, etc.

Examples:

- `GET /api/v1/products`
- `POST /api/v1/auth/login`
- `POST /api/v1/payments/webhook` (update Stripe dashboard if you use webhooks)

Swagger (Development) lists one OpenAPI document per version (e.g. **V1**). The `version` route parameter is filled automatically when you try requests from the UI.

---

## Adding **v2** for a breaking change

1. **Register the new API version** in `Program.cs` (optional if you only use attributes):

   ```csharp
   // Example: advertise v2 alongside v1 (both are discovered from controllers)
   builder.Services.AddApiVersioning(options =>
   {
       options.DefaultApiVersion = new ApiVersion(1, 0);
       options.AssumeDefaultVersionWhenUnspecified = false;
       options.ReportApiVersions = true;
       options.ApiVersionReader = new UrlSegmentApiVersionReader();
   })
   ```

   New versions are picked up when at least one endpoint declares `[ApiVersion(2.0)]`. No change is required here unless you use **conventions** instead of attributes.

2. **Add a v2 surface** (pick one strategy):

   - **Parallel controller** (clearest for large breaks): copy or split into `ProductsV2Controller` with `[ApiVersion(2.0)]` and `[Route("api/v{version:apiVersion}/products")]`, change only the v2 contract.
   - **Same controller, two versions**: apply multiple attributes, e.g. `[ApiVersion(1.0)] [ApiVersion(2.0)]`, and use **version-neutral** actions or `MapToApiVersion` on actions that differ.

   ```csharp
   [ApiController]
   [ApiVersion(2.0)]
   [Route("api/v{version:apiVersion}/products")]
   public sealed class ProductsV2Controller : ControllerBase
   {
       // New shapes, renamed fields, etc.
   }
   ```

3. **Keep v1 stable** for existing clients: leave `[ApiVersion(1.0)]` controllers in place until you deprecate them.

4. **Sunset v1** when ready: mark v1 types or actions with `[ApiVersion(1.0, Deprecated = true)]`. Swagger will show **deprecated** operations via `SwaggerDefaultValues`.

5. **Regenerate / publish Swagger**: a **V2** entry appears in the Swagger UI dropdown automatically from `IApiVersionDescriptionProvider`.

6. **External URLs**: update frontends, mobile apps, and integrations (e.g. Stripe webhook URL to `/api/v2/...` if you move the route).

---

## Packages

- `Asp.Versioning.Mvc`
- `Asp.Versioning.Mvc.ApiExplorer` (Swagger / OpenAPI grouping)
