using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace PolleriaLovely.Hubs
{
    [Authorize(Roles = "Administrador,Cajero,Mozo,Cocinero")]
    public class PedidosHub : Hub
    {
    }
}
