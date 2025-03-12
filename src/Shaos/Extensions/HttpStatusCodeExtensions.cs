/*
* MIT License
*
* Copyright (c) 2025 Derek Goslin https://github.com/DerekGn
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

using System.Net;

namespace Shaos.Extensions
{
    internal static class HttpStatusCodeExtension
    {
        internal static bool IsSuccessStatusCode(this HttpStatusCode statusCode) => ((int)statusCode >= 200) && ((int)statusCode <= 299);
        internal static bool IsSuccessStatusCode(this int statusCode) => (statusCode >= 200) && (statusCode <= 299);

        internal static string MapToType(this HttpStatusCode statusCode) => statusCode switch
        {
            HttpStatusCode.Continue => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.2.1",
            HttpStatusCode.SwitchingProtocols => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.2.2",

            HttpStatusCode.OK => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.3.1",
            HttpStatusCode.Created => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.3.2",
            HttpStatusCode.Accepted => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.3.3",
            HttpStatusCode.NonAuthoritativeInformation => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.3.4",
            HttpStatusCode.NoContent => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.3.5",
            HttpStatusCode.ResetContent => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.3.6",
            HttpStatusCode.PartialContent => "https://datatracker.ietf.org/doc/html/rfc7233#section-4.1",

            HttpStatusCode.MultipleChoices => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.4.1",
            HttpStatusCode.MovedPermanently => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.4.2",
            HttpStatusCode.Found => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.4.3",
            HttpStatusCode.SeeOther => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.4.4",
            HttpStatusCode.NotModified => "https://datatracker.ietf.org/doc/html/rfc7232#section-4.1",
            HttpStatusCode.UseProxy => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.4.5",
            HttpStatusCode.TemporaryRedirect => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.4.7",

            HttpStatusCode.BadRequest => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
            HttpStatusCode.Unauthorized => "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1",
            HttpStatusCode.PaymentRequired => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.2",
            HttpStatusCode.Forbidden => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3",
            HttpStatusCode.NotFound => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
            HttpStatusCode.MethodNotAllowed => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.5",
            HttpStatusCode.NotAcceptable => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.6",
            HttpStatusCode.ProxyAuthenticationRequired => "https://datatracker.ietf.org/doc/html/rfc7235#section-3.2",
            HttpStatusCode.RequestTimeout => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.7",
            HttpStatusCode.Conflict => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8",
            HttpStatusCode.Gone => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.9",
            HttpStatusCode.LengthRequired => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.10",
            HttpStatusCode.PreconditionFailed => "https://datatracker.ietf.org/doc/html/rfc7232#section-4.2",
            HttpStatusCode.RequestEntityTooLarge => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.11",
            HttpStatusCode.RequestUriTooLong => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.12",
            HttpStatusCode.UnsupportedMediaType => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.13",
            HttpStatusCode.RequestedRangeNotSatisfiable => "https://datatracker.ietf.org/doc/html/rfc7233#section-4.4",
            HttpStatusCode.ExpectationFailed => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.14",
            HttpStatusCode.UpgradeRequired => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.15",
            HttpStatusCode.InternalServerError => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
            HttpStatusCode.NotImplemented => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.2",
            HttpStatusCode.BadGateway => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.3",
            HttpStatusCode.ServiceUnavailable => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.4",
            HttpStatusCode.GatewayTimeout => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.5",
            HttpStatusCode.HttpVersionNotSupported => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.6",
            _ => string.Empty
        };
    }
}
