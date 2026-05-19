using Application.DTOs.Payments;

namespace Application.Interfaces.Services
{
    public interface IPaymentService
    {
        Task<PaymentResponseDto> CompletePaymentAsync(
        int orderId);

        Task<PaymentResponseDto> FailPaymentAsync(
            int orderId);
    }
}
