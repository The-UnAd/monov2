using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace UnAd.Auth.Web;

public static class CognitoJwtBearerExtensions {
    public static AuthenticationBuilder AddCognitoJwtBearer(this AuthenticationBuilder builder, string authority, bool requireHttpsMetadata = true) {
        builder.AddJwtBearer(options => {
            options.Authority = authority;
            options.SaveToken = true;
            options.RequireHttpsMetadata = requireHttpsMetadata;
            options.TokenValidationParameters =
                new TokenValidationParameters {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateActor = false,
                    ValidateLifetime = false
                };

            options.Events = new CustomJwtBearerEvents();
        });
        return builder;
    }
}


